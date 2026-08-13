Imports System.Data
Imports System.Net.Http
Imports System.Text
Imports Newtonsoft.Json
Imports System.Threading
Imports System.Net.NetworkInformation
Imports System.Threading.Tasks

Public Class WIPInStationCheck
    Private Const InterfaceName As String = "WIPInStationCheck"
    'URL 由 MesApiAddress 配置项拼接 (R2-14)
    Private ReadOnly url As String = GetMesUrl("WIPInStationCheck")
    Private Shared Function GetMesUrl(suffix As String) As String
        Dim baseAddr As String = If(String.IsNullOrEmpty(Initsetting.MesApiAddress), "http://172.16.110.10:9015", Initsetting.MesApiAddress)
        Return baseAddr.TrimEnd("/"c) & "/api/" & suffix
    End Function
    Private ReadOnly client As HttpClient
    Private ReadOnly TimerInterval As Integer = 10000 ' 60 seconds
    Private textListBox As ListBox
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
                Await UploadDataAsync()
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
                Dim reply As PingReply = ping.Send("172.16.110.10")
                Return reply.Status = IPStatus.Success
            End Using
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Async Function UploadDataAsync() As Task
        Dim equip_Code As String = "ZJ01-P-COCV-A-C-O-000" & currentOCVNum.ToString().Trim()
        Dim data As New Dictionary(Of String, Object) From {
            {"EquipPC_ID", If(String.IsNullOrEmpty(Initsetting.EquipPC_ID), "HZWL", Initsetting.EquipPC_ID)},
            {"EquipPC_Password", If(String.IsNullOrEmpty(Initsetting.EquipPC_Password), "123456", Initsetting.EquipPC_Password)},
            {"Equip_Code", equip_Code},
            {"Check_Time", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},
            {"Wip_List", GetWipList()},
            {"Operator", "Operator1"}
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

    Private Function GetWipList() As List(Of Dictionary(Of String, String))
        Dim wipList As New List(Of Dictionary(Of String, String))()
        Dim wip As String = If(processNumSet, "").Trim()
        If wip.Length > 0 Then
            wipList.Add(New Dictionary(Of String, String) From {{"WIP_NO", wip}})
        End If
        Return wipList
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
                AddToOutput("Success:在制品进站校验 " & msg)
                Return True
            End If

            AddToOutput("Error: " & returnCode & " 在制品进站校验 " & msg)
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
            AddToOutput("Error while saving WIP data to UploadOutbox: " & ex.Message)
        End Try
    End Sub

    Private Async Function UploadPendingData() As Task
        Try
            Using service As New OcvDataService()
                Dim table As DataTable = service.GetPendingUploads(InterfaceName, 100, "WIPInStationCheck.WIPInStationCheck", 120)
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
            AddToOutput("Error loading pending WIP uploads from UploadOutbox: " & ex.Message)
        End Try
    End Function

    Private Shared Function BuildOutboxIdempotencyKey() As String
        Return "WIPInStationCheck:" & Guid.NewGuid().ToString("N")
    End Function

    Private Function CompressJson(json As String) As String
        Return json.Replace(" ", "").Replace(Environment.NewLine, "")
    End Function
End Class
