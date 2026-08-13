Imports System.Data
Imports System.Data.SqlClient
Imports System.Net.Http
Imports System.Text
Imports Newtonsoft.Json
Imports System.Threading
Imports System.Net.NetworkInformation
Imports System.Threading.Tasks

Public Class StateAndStopReasonUpload
    Private Const InterfaceName As String = "StateAndStopReasonUpload"
    'URL 由 MesApiAddress 配置项拼接 (R2-14)
    Private ReadOnly url As String = GetMesUrl("StateAndStopReasonUpload")
    Private Shared Function GetMesUrl(suffix As String) As String
        Dim baseAddr As String = If(String.IsNullOrEmpty(Initsetting.MesApiAddress), "http://172.16.110.10:9015", Initsetting.MesApiAddress)
        Return baseAddr.TrimEnd("/"c) & "/api/" & suffix
    End Function
    Private ReadOnly client As HttpClient
    Private ReadOnly TimerInterval As Integer = 10000 ' 10 seconds
    Private textListBox As ListBox
    Private connectionString As String = pubConnectionString
    Private lastStatus As Integer = -1 ' 初始状态为-1，表示未设置
    Private timer As Timer
    Private _isRunning As Integer = 0


    Public Sub StartTimer()
        'StartTimer 幂等保护,避免 btnStart 双击创建多个 Timer 并发跑 (R2-10)
        If timer IsNot Nothing Then
            Return
        End If
        timer = New Timer(Async Sub(state As Object) Await TimerCallback(), Nothing, 0, TimerInterval)
    End Sub

    Public Sub StopTimer()
        If timer IsNot Nothing Then
            timer.Dispose()
            timer = Nothing
        End If
    End Sub
    Public Sub New(textListBox As ListBox)
        '复用 Module1 共享 HttpClient 池 (R2-12)
        client = MesHttpClient
        Me.textListBox = textListBox
    End Sub


    Private Async Function TimerCallback() As Task
        If Threading.Interlocked.Exchange(_isRunning, 1) = 1 Then
            Return
        End If

        Try
            If IsNetworkAvailable() Then
                Await UploadPendingData()
                Await CheckAndUploadStatusAsync()
            Else
                AddToOutput("MES network is unavailable.")
            End If
        Finally
            Threading.Interlocked.Exchange(_isRunning, 0)
        End Try
    End Function

    Private Function IsNetworkAvailable() As Boolean
        Try
            Using ping As New Ping()
                Dim reply As PingReply = ping.Send("172.16.110.10") ' Replace with your MES server IP or hostname
                Return reply.Status = IPStatus.Success
            End Using
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Async Function CheckAndUploadStatusAsync() As Task
        ' 模拟获取设备状态的方法，需要根据实际情况进行修改
        Dim currentStatus As Integer = GetCurrentDeviceStatus()
        If currentStatus <> lastStatus Then
            lastStatus = currentStatus
            Await UploadDataAsync(currentStatus)
        End If
    End Function

    Private Function GetCurrentDeviceStatus() As Integer
        ' 真实设备状态码:
        ' 1 = 运行(测试中,isRuning=True)
        ' 2 = 待机(空闲,isAskOcvNum=True 已扫码待开始)
        ' 3 = 停机(异常/超温/未连接)
        Try
            If alarmStatus Then
                Return 3 '停机 - 有报警
            ElseIf isRuning Then
                Return 1 '运行
            ElseIf isAskOcvNum Then
                Return 2 '待机
            Else
                Return 3 '停机
            End If
        Catch
            Return 3 '读取失败视为停机
        End Try
    End Function

    Public Async Function UploadDataAsync(status As Integer) As Task
        Dim equip_Code As String = "ZJ01-P-COCV-A-C-O-000" & currentOCVNum.ToString().Trim()
        Dim data As New Dictionary(Of String, Object) From {
            {"EquipPC_ID", If(String.IsNullOrEmpty(Initsetting.EquipPC_ID), "HZWL", Initsetting.EquipPC_ID)},
            {"EquipPC_Password", If(String.IsNullOrEmpty(Initsetting.EquipPC_Password), "123456", Initsetting.EquipPC_Password)},
            {"Equip_Code", equip_Code},
            {"Upload_Time", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},
            {"Status", status},
            {"Start_Time", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},
            {"Reason_Code", If(status = 3, GetReasonCode(), Nothing)}
        }

        Dim json As String = JsonConvert.SerializeObject(data)
        Dim content As New StringContent(json, Encoding.UTF8, "application/json")

        Try
            Dim response As HttpResponseMessage = Await client.PostAsync(url, content)
            Dim responseString As String = Await response.Content.ReadAsStringAsync()
            If Not response.IsSuccessStatusCode OrElse Not IsSuccessfulResponse(responseString) Then
                SaveDataLocally(json)
                Return
            End If
        Catch ex As Exception
            AddToOutput("Error: " & ex.Message)
            SaveDataLocally(json)
        End Try
    End Function

    Private Function GetReasonCode() As Integer
        ' 停机原因码:
        ' 0 = 短停机(正常切换工序/正常空闲)
        ' 1 = 设备故障(超温 / 中位机失联 / 仪表故障)
        ' 2 = 操作员停机(急停按钮 / 人工停止)
        Try
            '超温、报警、MES失联都归为"设备故障"
            If alarmStatus Then
                Return 1
            End If
            '没有具体急停状态标志,默认按短停机上报
            Return 0
        Catch
            Return 0
        End Try
    End Function

    Private Function IsSuccessfulResponse(responseString As String) As Boolean
        Try
            Dim response As Dictionary(Of String, Object) = JsonConvert.DeserializeObject(Of Dictionary(Of String, Object))(responseString)
            If response Is Nothing OrElse Not response.ContainsKey("Return_Code") Then
                AddToOutput("MES response missing Return_Code: " & responseString)
                Return False
            End If

            Dim returnCode As String = response("Return_Code").ToString()
            Dim msg As String = If(response.ContainsKey("Msg"), response("Msg").ToString(), "")
            If returnCode = "S" Then
                AddToOutput("Success:设备状态+停机原因上传- " & msg)
                Return True
            End If

            AddToOutput("Error: " & returnCode & " 设备状态+停机原因上传- " & msg)
            Return False
        Catch ex As Exception
            AddToOutput("Error parsing MES response: " & ex.Message)
            Return False
        End Try
    End Function
    Private Sub AddToOutput(message As String)
        If textListBox.InvokeRequired Then
            textListBox.Invoke(New Action(Of String)(AddressOf AddToOutput), message)
        Else
            textListBox.Items.Add(message)
        End If
    End Sub

    Private Sub SaveDataLocally(data As String)
        Try
            Using service As New OcvDataService()
                service.EnqueueUpload(BuildOutboxIdempotencyKey(), InterfaceName, url, data)
            End Using
        Catch ex As Exception
            AddToOutput("Error while saving state data to UploadOutbox: " & ex.Message)
        End Try
    End Sub

    Private Async Function UploadPendingData() As Task
        Try
            Using service As New OcvDataService()
                Dim table As DataTable = service.GetPendingUploads(InterfaceName, 100, "StateAndStopReasonUpload.StateAndStopReasonUpload", 120)
                For Each row As DataRow In table.Rows
                    Dim payload As String = Convert.ToString(row("PayloadJson"))
                    Dim outboxId As Long = Convert.ToInt64(row("UploadOutboxId"))
                    Try
                        Dim content As New StringContent(payload, Encoding.UTF8, "application/json")
                        Dim response As HttpResponseMessage = Await client.PostAsync(url, content)
                        Dim responseString As String = Await response.Content.ReadAsStringAsync()
                        If response.IsSuccessStatusCode AndAlso IsSuccessfulResponse(responseString) Then
                            service.MarkUploadSent(outboxId)
                        Else
                            service.MarkUploadFailed(outboxId, "HTTP failed: " & responseString, 60)
                            AddToOutput("Failed to send pending data.")
                        End If
                    Catch ex As Exception
                        service.MarkUploadFailed(outboxId, ex.Message, 60)
                        AddToOutput("Error while sending pending data: " & ex.Message)
                    End Try
                Next
            End Using
        Catch ex As Exception
            AddToOutput("Error loading pending state uploads from UploadOutbox: " & ex.Message)
        End Try
    End Function

    Private Shared Function BuildOutboxIdempotencyKey() As String
        Return "StateAndStopReasonUpload:" & Guid.NewGuid().ToString("N")
    End Function
End Class

'' Form code to initialize and start the timer
'Public Class MainForm
'    Private stateAndStopReasonUpload As StateAndStopReasonUpload

'    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
'        stateAndStopReasonUpload = New StateAndStopReasonUpload(TextListBox)
'        stateAndStopReasonUpload.StartTimer()
'    End Sub
'End Class

