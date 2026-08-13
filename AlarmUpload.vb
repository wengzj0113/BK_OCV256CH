Imports System.Data
Imports System.Net.NetworkInformation
Imports System.Threading.Tasks
Imports Newtonsoft.Json
Imports System.Net.Http
Imports System.Text
Imports System.Threading

Public Class AlarmUpload
    Private Const InterfaceName As String = "AlarmUpload"
    'URL 由 MesApiAddress 配置项拼接 (R2-14)
    Private ReadOnly url As String = GetMesUrl("AlarmUpload")
    Private Shared Function GetMesUrl(suffix As String) As String
        Dim baseAddr As String = If(String.IsNullOrEmpty(Initsetting.MesApiAddress), "http://172.16.110.10:9015", Initsetting.MesApiAddress)
        Return baseAddr.TrimEnd("/"c) & "/api/" & suffix
    End Function
    Private ReadOnly client As HttpClient
    Private ReadOnly textListBox As ListBox
    Private lastAlarmStatus As Integer? = Nothing
    Private lastAlarmCode As String = Nothing
    Private timer As Timer
    Private ReadOnly TimerInterval As Integer = 10000
    Private _isRunning As Integer = 0
    Private ReadOnly alarmStatusProvider As Func(Of Integer)
    Private ReadOnly alarmCodeProvider As Func(Of String)
    Private ReadOnly alarmStartTimeProvider As Func(Of DateTime)

    Public Sub New(textListBox As ListBox,
                   Optional alarmStatusProvider As Func(Of Integer) = Nothing,
                   Optional alarmCodeProvider As Func(Of String) = Nothing,
                   Optional alarmStartTimeProvider As Func(Of DateTime) = Nothing)
        '复用 Module1 共享 HttpClient 池 (R2-12)
        client = MesHttpClient
        Me.textListBox = textListBox
        Me.alarmStatusProvider = alarmStatusProvider
        Me.alarmCodeProvider = alarmCodeProvider
        Me.alarmStartTimeProvider = alarmStartTimeProvider
    End Sub

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

    Public Async Sub TriggerUpload(alarmStatus As Integer, alarmCode As String, startTime As DateTime)
        If IsSameAlarm(alarmStatus, alarmCode) Then
            AddToOutput("报警状态未改变，未触发上传。")
            Return
        End If

        Dim handled As Boolean = Await UploadOrCacheAlarmAsync(alarmStatus, alarmCode, startTime)
        If handled Then
            RememberAlarm(alarmStatus, alarmCode)
        End If
    End Sub

    Private Async Function TimerCallback() As Task
        If Threading.Interlocked.Exchange(_isRunning, 1) = 1 Then
            Return
        End If

        Try
            Dim alarmStatus As Integer = GetAlarmStatus()
            Dim alarmCode As String = GetAlarmCode()
            Dim startTime As DateTime = GetAlarmStartTime()

            If IsSameAlarm(alarmStatus, alarmCode) Then
                AddToOutput("报警状态未改变，未触发上传。")
                Return
            End If

            Dim handled As Boolean = Await UploadOrCacheAlarmAsync(alarmStatus, alarmCode, startTime)
            If handled Then
                RememberAlarm(alarmStatus, alarmCode)
            End If
        Finally
            Threading.Interlocked.Exchange(_isRunning, 0)
        End Try
    End Function

    Private Async Function UploadOrCacheAlarmAsync(alarmStatus As Integer, alarmCode As String, startTime As DateTime) As Task(Of Boolean)
        If Await IsNetworkAvailableAsync() Then
            Await UploadPendingDataAsync()
            Return Await UploadDataAsync(alarmStatus, alarmCode, startTime)
        End If

        AddToOutput("MES网络不可用，报警数据保存到本地。")
        Return Await SaveAlarmDataLocallyAsync(alarmStatus, alarmCode, startTime)
    End Function

    Private Async Function IsNetworkAvailableAsync() As Task(Of Boolean)
        Try
            Using ping As New Ping()
                Dim reply As PingReply = Await ping.SendPingAsync("172.16.110.10")
                Return reply.Status = IPStatus.Success
            End Using
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Async Function UploadDataAsync(alarmStatus As Integer, alarmCode As String, startTime As DateTime) As Task(Of Boolean)
        Dim json As String = BuildAlarmJson(alarmStatus, alarmCode, startTime)
        Dim content As New StringContent(json, Encoding.UTF8, "application/json")

        Try
            Dim response As HttpResponseMessage = Await client.PostAsync(url, content)
            Dim responseString As String = Await response.Content.ReadAsStringAsync()
            If Not response.IsSuccessStatusCode OrElse Not IsSuccessfulResponse(responseString) Then
                Return SaveDataLocally(json)
            End If

            Return True
        Catch ex As Exception
            AddToOutput("Error: " & ex.Message)
            Return SaveDataLocally(json)
        End Try
    End Function

    Private Function BuildAlarmJson(alarmStatus As Integer, alarmCode As String, startTime As DateTime) As String
        Dim equip_Code As String = "ZJ01-P-COCV-A-C-O-000" & currentOCVNum.ToString().Trim()
        Dim data As New Dictionary(Of String, Object) From {
            {"EquipPC_ID", If(String.IsNullOrEmpty(Initsetting.EquipPC_ID), "HZWL", Initsetting.EquipPC_ID)},
            {"EquipPC_Password", If(String.IsNullOrEmpty(Initsetting.EquipPC_Password), "123456", Initsetting.EquipPC_Password)},
            {"Equip_Code", equip_Code},
            {"Upload_Time", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},
            {"Alarm_List", GetAlarmList(alarmStatus, alarmCode, startTime)}
        }
        Return JsonConvert.SerializeObject(data)
    End Function

    Private Async Function SaveAlarmDataLocallyAsync(alarmStatus As Integer, alarmCode As String, startTime As DateTime) As Task(Of Boolean)
        Return Await Task.Run(Function() SaveDataLocally(BuildAlarmJson(alarmStatus, alarmCode, startTime)))
    End Function

    Private Function GetAlarmList(alarmStatus As Integer, alarmCode As String, startTime As DateTime) As List(Of Dictionary(Of String, Object))
        Return New List(Of Dictionary(Of String, Object)) From {
            New Dictionary(Of String, Object) From {
                {"Alarm_Value", alarmStatus},
                {"Alarm_Code", alarmCode},
                {"Start_Time", startTime.ToString("yyyy/MM/dd HH:mm:ss")}
            }
        }
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
                AddToOutput("Success: 设备报警信息上传-" & msg)
                Return True
            End If

            AddToOutput("Error: " & returnCode & " 设备报警信息上传 - " & msg)
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

    Private Function SaveDataLocally(data As String) As Boolean
        Try
            '直接写 OCV_DATA.UploadOutbox (R2-15):每次失败都新增一条,
            '与旧 SqlServerHelper.InsertUnsentData 行为一致(失败重试会堆积),
            '依靠 usp_MarkUploadSent 在成功后标 Sent,失败后由 RetryCount/NextRetryAt 决定下次重试。
            Using service As New OcvDataService()
                service.EnqueueUpload(BuildOutboxIdempotencyKey(), InterfaceName, url, data)
            End Using
            Return True
        Catch ex As Exception
            AddToOutput("Error while saving alarm data to UploadOutbox: " & ex.Message)
            Return False
        End Try
    End Function

    Private Async Function UploadPendingDataAsync() As Task
        Try
            Using service As New OcvDataService()
                Dim table As DataTable = service.GetPendingUploads(InterfaceName, 100, "AlarmUpload.AlarmUpload", 120)
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
            AddToOutput("Error loading pending alarm uploads from UploadOutbox: " & ex.Message)
        End Try
    End Function

    ''' <summary>
    ''' 生成 UploadOutbox 的 IdempotencyKey。
    ''' 每次失败都生成新 GUID,确保多次失败都会产生独立的 outbox 行 (与旧 InsertUnsentData 行为一致)。
    ''' </summary>
    Private Shared Function BuildOutboxIdempotencyKey() As String
        Return "AlarmUpload:" & Guid.NewGuid().ToString("N")
    End Function

    Private Function GetAlarmStatus() As Integer
        If alarmStatusProvider Is Nothing Then
            Return 0
        End If

        Return alarmStatusProvider.Invoke()
    End Function

    Private Function GetAlarmCode() As String
        If alarmCodeProvider Is Nothing Then
            Return "NORMAL"
        End If

        Return alarmCodeProvider.Invoke()
    End Function

    Private Function GetAlarmStartTime() As DateTime
        If alarmStartTimeProvider Is Nothing Then
            Return DateTime.Now
        End If

        Return alarmStartTimeProvider.Invoke()
    End Function

    Private Function IsSameAlarm(alarmStatus As Integer, alarmCode As String) As Boolean
        Return lastAlarmStatus.HasValue AndAlso
               lastAlarmStatus.Value = alarmStatus AndAlso
               String.Equals(lastAlarmCode, alarmCode, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Sub RememberAlarm(alarmStatus As Integer, alarmCode As String)
        lastAlarmStatus = alarmStatus
        lastAlarmCode = alarmCode
    End Sub
End Class
