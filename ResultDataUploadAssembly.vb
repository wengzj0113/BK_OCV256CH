Imports System.Data
Imports System.Data.SqlClient
Imports System.Net.Http
Imports System.Text
Imports Newtonsoft.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Linq
Imports DevExpress.Data.Linq.Helpers
Imports System.Net.NetworkInformation

Public Class ResultDataUploadAssembly
    Private Const InterfaceName As String = "ResultDataUploadAssembly"
    'URL 由 MesApiAddress 配置项拼接 (R2-14)
    Private ReadOnly url As String = GetMesUrl("ResultDataUploadAssembly")
    Private Shared Function GetMesUrl(suffix As String) As String
        Dim baseAddr As String = If(String.IsNullOrEmpty(Initsetting.MesApiAddress), "http://172.16.110.10:9015", Initsetting.MesApiAddress)
        Return baseAddr.TrimEnd("/"c) & "/api/" & suffix
    End Function
    Private ReadOnly client As HttpClient
    Private ReadOnly outputListBox As ListBox
    Private ReadOnly TimerInterval As Integer
    Public Property ChannelNumbers As List(Of Integer)
    Private connectionString As String = pubConnectionString
    Private timer As Timer
    Private _isRunning As Integer = 0
    Public Sub StartTimer()
        'StartTimer 幂等保护,避免重复启动 (R2-10)
        If timer IsNot Nothing Then
            Return
        End If
        timer = New Timer(Async Sub(state As Object) Await TimerCallback(), Nothing, 0, Timeout.Infinite)
    End Sub

    Public Sub StopTimer()
        If timer IsNot Nothing Then
            timer.Dispose()
            timer = Nothing
        End If
    End Sub
    Public Sub New(outputListBox As ListBox)
        '复用 Module1 共享 HttpClient 池 (R2-12)
        client = MesHttpClient
        'TimerInterval = Integer.Parse(FrmMesTask.txtInterval.Text)
        TimerInterval = 10000
        Me.outputListBox = outputListBox
    End Sub
    Private Async Function TimerCallback() As Task
        If Threading.Interlocked.Exchange(_isRunning, 1) = 1 Then
            Return
        End If

        Try
            If IsNetworkAvailable() Then
                Await UploadOnceAsync()
            Else
                AddToOutput("MES network is unavailable.")
                Await UploadDataAsync()
            End If
        Finally
            Threading.Interlocked.Exchange(_isRunning, 0)
            StopTimer()
        End Try
    End Function

    Public Async Function UploadOnceAsync() As Task
        Await UploadPendingData()
        Await UploadDataAsync()
    End Function

    Public Async Function UploadDataAsync() As Task
        Dim equip_Code As String = "ZJ01-P-COCV-A-C-O-000" & currentOCVNum.ToString().Trim()
        Dim data As New Dictionary(Of String, Object) From {
            {"EquipPC_ID", If(String.IsNullOrEmpty(Initsetting.EquipPC_ID), "HZWL", Initsetting.EquipPC_ID)},
            {"EquipPC_Password", If(String.IsNullOrEmpty(Initsetting.EquipPC_Password), "123456", Initsetting.EquipPC_Password)},
            {"Equip_Code", equip_Code},
            {"Upload_Time", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},
            {"Start_Time", RecieveData(0).TestTime.ToString("yyyy/MM/dd HH:mm:ss")},
            {"End_Time", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},
            {"Manufacture_Code", processNumSet},
            {"Remark", ""},
            {"Operator", operatorName},
            {"Out_List", GetOutList()}
        }

        Dim json As String = JsonConvert.SerializeObject(data)
        Dim content As New StringContent(json, Encoding.UTF8, "application/json")

        If Not IsNetworkAvailable() Then
            AddToOutput("Network unavailable. Saving data locally.")
            SaveDataLocally(json)
            Return
        End If

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

    Private Function GetOutList() As List(Of Dictionary(Of String, Object))
        Dim outList As New List(Of Dictionary(Of String, Object))

        Dim channels As List(Of Integer)
        If ChannelNumbers IsNot Nothing AndAlso ChannelNumbers.Count > 0 Then
            channels = ChannelNumbers.Distinct().OrderBy(Function(channel) channel).ToList()
        Else
            channels = Enumerable.Range(1, Math.Max(0, Math.Min(avalibleCount, MAX_CHANNEL))).ToList()
        End If

        For Each channel As Integer In channels
            Dim index0 As Integer = channel - 1
            If index0 < 0 OrElse index0 >= RecieveData.Length Then
                Continue For
            End If
            Dim outItem As New Dictionary(Of String, Object) From
            {
                {"Out_Lot_No", RecieveData(index0).BatCode},
                {"Out_Tray_No", trayCodeSet},
                {"Batch_Code", ""},
                {"In_Lot_No", RecieveData(index0).BatCode},
                {"In_Tray_No", trayCodeSet},
                {"Is_NG", 0},
                {"NG_Code", New List(Of String)()},
                {"Param_List", GetParamList(RecieveData(index0), channel)}
            }
            outList.Add(outItem)
        Next

        Return outList
    End Function

    Private Function GetParamList(ByVal RecData As Bat_info, ByVal tdh As Integer) As List(Of Dictionary(Of String, String))
        Return New List(Of Dictionary(Of String, String)) From {
            New Dictionary(Of String, String) From {
                {"Param_Code", "CSSJ"},
                {"Param_Value", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}
            },
            New Dictionary(Of String, String) From {
                {"Param_Code", "DY"},
                {"Param_Value", RecData.Voltage.ToString("0.00000")}
            },
            New Dictionary(Of String, String) From {
                {"Param_Code", "NZ"},
                {"Param_Value", RecData.Resistance.ToString("0.000")}
            },
            New Dictionary(Of String, String) From {
                {"Param_Code", "TDH"},
                {"Param_Value", tdh}
            },
            New Dictionary(Of String, String) From {
                {"Param_Code", "HJWD"},
                {"Param_Value", frmTest.hjwd}
            },
            New Dictionary(Of String, String) From {
                {"Param_Code", "TZSYSM"},
                {"Param_Value", (Initsetting.Probelifespan - Initsetting.ProbeUsedCount).ToString()}
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
                AddToOutput("Success: 产品结果数据上传-电芯段-" & msg)
                Return True
            End If

            AddToOutput("Error: " & returnCode & " 产品结果数据上传-电芯段 - " & msg)
            Return False
        Catch ex As Exception
            AddToOutput("Error parsing MES response: " & ex.Message)
            Return False
        End Try
    End Function

    Private Sub AddToOutput(message As String)
        If outputListBox.InvokeRequired Then
            outputListBox.Invoke(New Action(Of String)(AddressOf AddToOutput), message)
        Else
            outputListBox.Items.Add(message)
        End If
    End Sub

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
    Private Sub SaveDataLocally(data As String)
        Try
            Using service As New OcvDataService()
                service.EnqueueUpload(BuildOutboxIdempotencyKey(), InterfaceName, url, data)
            End Using
        Catch ex As Exception
            AddToOutput("Error while saving result data to UploadOutbox: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' 上传缓存在本地的数据
    ''' </summary>
    ''' <returns></returns>
    Private Async Function UploadPendingData() As Task
        Try
            Using service As New OcvDataService()
                Dim table As DataTable = service.GetPendingUploads(InterfaceName, 100, "ResultDataUploadAssembly.ResultDataUploadAssembly", 120)
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
            AddToOutput("Error loading pending result uploads from UploadOutbox: " & ex.Message)
        End Try
    End Function

    Private Shared Function BuildOutboxIdempotencyKey() As String
        Return "ResultDataUploadAssembly:" & Guid.NewGuid().ToString("N")
    End Function
End Class
'sql代码， 建立表格。缓存数据， 网络联通后再上传
'CREATE TABLE UnsentData (
'    Id INT PRIMARY KEY IDENTITY(1,1),
'    Data NVARCHAR(MAX),
'    Upload_Time DATETIME,
'    Is_Sent BIT Default 0
');
