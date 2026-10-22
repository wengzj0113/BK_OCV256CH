Imports System.Data
Imports System.Data.SqlClient
Imports System.Net.Http
Imports System.Text
Imports Newtonsoft.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Linq
Imports DevExpress.Data.Linq.Helpers
Imports System.Globalization

Public Class ResultDataUploadAssembly
    Private Const InterfaceName As String = "ResultDataUploadAssembly"
    'URL 由 MesApiAddress 配置项拼接 (R2-14)
    Private ReadOnly url As String
    Private Shared Function GetMesUrl(suffix As String) As String
        Dim baseAddr As String = If(String.IsNullOrEmpty(Initsetting.MesApiAddress), "http://172.16.110.10:9015", Initsetting.MesApiAddress)
        Return baseAddr.TrimEnd("/"c) & "/api/" & suffix
    End Function
    Private ReadOnly client As HttpClient
    Private ReadOnly outputListBox As ListBox
    Private ReadOnly TimerInterval As Integer
    Public Property ChannelNumbers As List(Of Integer)
    Private resultSnapshot As Bat_info()
    Private channelSnapshot As List(Of Integer)
    Private traySnapshot As String
    Private ocvSnapshot As Integer
    Private temperatureSnapshot As String
    Private probeRemainingSnapshot As String
    Private processSnapshot As String
    Private operatorSnapshot As String
    Private userSnapshot As String
    Private passwordSnapshot As String
    Private startedSnapshot As DateTime
    Private endedSnapshot As DateTime
    Private ngSnapshot As New Dictionary(Of Integer, Integer)()
    Private ReadOnly explicitRound As ManualTestRoundSnapshot
    Private timer As Timer
    Private _isRunning As Integer = 0
    Public Sub StartTimer()
        'StartTimer 幂等保护,避免重复启动 (R2-10)
        If timer IsNot Nothing Then
            Return
        End If
        CaptureSnapshot()
        timer = New Timer(Async Sub(state As Object) Await TimerCallback(), Nothing, 0, Timeout.Infinite)
    End Sub

    Public Sub CaptureSnapshot()
        If explicitRound IsNot Nothing Then Return
        resultSnapshot = CType(RecieveData.Clone(), Bat_info())
        channelSnapshot = If(ChannelNumbers Is Nothing, Nothing, ChannelNumbers.ToList())
        traySnapshot = trayCodeSet
        ocvSnapshot = currentOCVNum
        temperatureSnapshot = frmTest.hjwd
        probeRemainingSnapshot = (Initsetting.Probelifespan - Initsetting.ProbeUsedCount).ToString()
        processSnapshot = processNumSet
        operatorSnapshot = operatorName
        userSnapshot = Initsetting.EquipPC_ID
        passwordSnapshot = Initsetting.EquipPC_Password
        Dim selected = If(channelSnapshot Is Nothing OrElse channelSnapshot.Count = 0,
                          Enumerable.Range(1, Math.Max(0, Math.Min(avalibleCount, MAX_CHANNEL))).ToList(), channelSnapshot)
        ngSnapshot = selected.ToDictionary(Function(c) c, Function(c) CInt(IsNG(resultSnapshot(c - 1).Voltage, resultSnapshot(c - 1).Resistance)))
        startedSnapshot = If(selected.Count > 0, selected.Select(Function(c) resultSnapshot(c - 1).TestTime).Min(), DateTime.Now)
        endedSnapshot = DateTime.Now
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
        url = GetMesUrl("ResultDataUploadAssembly")
    End Sub

    Public Sub New(outputListBox As ListBox, round As ManualTestRoundSnapshot)
        If round Is Nothing Then Throw New ArgumentNullException(NameOf(round))
        If round.Context.Plan.IsSimulated Then Throw New InvalidOperationException("模拟条码结果禁止上传 MES。")
        Dim context = round.Context
        Dim endpoint As Uri = Nothing
        If Not Uri.TryCreate(context.MesAddress, UriKind.Absolute, endpoint) OrElse
           (endpoint.Scheme <> Uri.UriSchemeHttp AndAlso endpoint.Scheme <> Uri.UriSchemeHttps) Then
            Throw New ArgumentException("请配置有效的 MES HTTP 接口地址。")
        End If
        client = MesHttpClient
        Me.outputListBox = outputListBox
        explicitRound = round
        url = context.MesAddress.TrimEnd("/"c) & "/api/" & InterfaceName
        resultSnapshot = round.CopyMeasurements()
        channelSnapshot = context.Channels.ToList()
        traySnapshot = context.Plan.TrayCode
        ocvSnapshot = context.OcvNumber
        temperatureSnapshot = context.Temperature
        probeRemainingSnapshot = context.ProbeRemaining
        processSnapshot = context.ProcessNumber
        operatorSnapshot = context.OperatorName
        userSnapshot = context.MesUser
        passwordSnapshot = context.MesPassword
        startedSnapshot = round.StartedAt
        endedSnapshot = round.CapturedAt
        ngSnapshot = context.Channels.ToDictionary(Function(c) c, Function(c) round.NgCode(c))
    End Sub
    Private Async Function TimerCallback() As Task
        If Threading.Interlocked.Exchange(_isRunning, 1) = 1 Then
            Return
        End If

        Try
            Await UploadOnceAsync()
        Catch ex As Exception
            '后台 Timer 使用 Async Sub 回调，异常必须在此终止传播。
            '手动页直接等待 UploadSnapshotAsync，仍能收到上传队列写入失败。
            AppLog.Write("MES", "后台结果上传或上传队列写入失败：" & ex.Message)
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
        Await UploadSnapshotAsync()
    End Function

    Public Function BuildPayloadJson() As String
        If resultSnapshot Is Nothing Then CaptureSnapshot()
        Dim equip_Code As String = "ZJ01-P-COCV-A-C-O-000" & ocvSnapshot.ToString().Trim()
        Dim data As New Dictionary(Of String, Object) From {
            {"EquipPC_ID", If(String.IsNullOrEmpty(userSnapshot), "HZWL", userSnapshot)},
            {"EquipPC_Password", If(String.IsNullOrEmpty(passwordSnapshot), "123456", passwordSnapshot)},
            {"Equip_Code", equip_Code},
            {"Upload_Time", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},
            {"Start_Time", startedSnapshot.ToString("yyyy/MM/dd HH:mm:ss")},
            {"End_Time", endedSnapshot.ToString("yyyy/MM/dd HH:mm:ss")},
            {"Manufacture_Code", processSnapshot},
            {"Remark", ""},
            {"Operator", operatorSnapshot},
            {"Out_List", GetOutList()}
        }

        Return JsonConvert.SerializeObject(data)
    End Function

    Public Async Function UploadSnapshotAsync() As Task(Of String)
        Dim json As String = BuildPayloadJson()
        Dim failure As String = Nothing
        Try
            Using content As New StringContent(json, Encoding.UTF8, "application/json")
                Using response As HttpResponseMessage = Await client.PostAsync(url, content)
                    Dim responseString As String = Await response.Content.ReadAsStringAsync()
                    If response.IsSuccessStatusCode AndAlso IsSuccessfulResponse(responseString) Then Return "MES 上传成功"
                    failure = "MES 接口返回失败（HTTP " & CInt(response.StatusCode).ToString() & "）"
                End Using
            End Using
        Catch ex As Exception
            failure = ex.Message
        End Try
        AddToOutput("MES 本次上传未完成：" & failure)
        SaveDataLocally(json)
        Return "MES 上传待重试，数据已写入本地上传队列"
    End Function

    Private Function GetOutList() As List(Of Dictionary(Of String, Object))
        Dim outList As New List(Of Dictionary(Of String, Object))

        Dim channels As List(Of Integer)
        If channelSnapshot IsNot Nothing AndAlso channelSnapshot.Count > 0 Then
            channels = channelSnapshot.Distinct().OrderBy(Function(channel) channel).ToList()
        Else
            channels = Enumerable.Range(1, Math.Max(0, Math.Min(avalibleCount, MAX_CHANNEL))).ToList()
        End If

        For Each channel As Integer In channels
            Dim index0 As Integer = channel - 1
            If index0 < 0 OrElse index0 >= resultSnapshot.Length Then
                Continue For
            End If
            Dim outItem As New Dictionary(Of String, Object) From
            {
                {"Out_Lot_No", resultSnapshot(index0).BatCode},
                {"Out_Tray_No", traySnapshot},
                {"Batch_Code", ""},
                {"In_Lot_No", resultSnapshot(index0).BatCode},
                {"In_Tray_No", traySnapshot},
                {"Is_NG", If(ngSnapshot(channel) = 0, 0, 1)},
                {"NG_Code", New List(Of String)()},
                {"Param_List", GetParamList(resultSnapshot(index0), channel)}
            }
            outList.Add(outItem)
        Next

        Return outList
    End Function

    Private Function GetParamList(ByVal RecData As Bat_info, ByVal tdh As Integer) As List(Of Dictionary(Of String, String))
        Return New List(Of Dictionary(Of String, String)) From {
            New Dictionary(Of String, String) From {
                {"Param_Code", "CSSJ"},
                {"Param_Value", endedSnapshot.ToString("yyyy/MM/dd HH:mm:ss")}
            },
            New Dictionary(Of String, String) From {
                {"Param_Code", "DY"},
                {"Param_Value", RecData.Voltage.ToString("0.00000", CultureInfo.InvariantCulture)}
            },
            New Dictionary(Of String, String) From {
                {"Param_Code", "NZ"},
                {"Param_Value", RecData.Resistance.ToString("0.000", CultureInfo.InvariantCulture)}
            },
            New Dictionary(Of String, String) From {
                {"Param_Code", "TDH"},
                {"Param_Value", tdh}
            },
            New Dictionary(Of String, String) From {
                {"Param_Code", "HJWD"},
                {"Param_Value", temperatureSnapshot}
            },
            New Dictionary(Of String, String) From {
                {"Param_Code", "TZSYSM"},
                {"Param_Value", probeRemainingSnapshot}
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
        If outputListBox Is Nothing OrElse outputListBox.IsDisposed Then
            AppLog.Write("MES", message)
            Return
        End If
        If outputListBox.InvokeRequired Then
            outputListBox.Invoke(New Action(Of String)(AddressOf AddToOutput), message)
        Else
            outputListBox.Items.Add(message)
        End If
    End Sub

    Private Sub SaveDataLocally(data As String)
        Using service As New OcvDataService()
            Dim key As String = If(explicitRound Is Nothing, BuildOutboxIdempotencyKey(),
                                   "ManualResult:" & explicitRound.Context.RunId & ":" & explicitRound.RoundNumber.ToString(CultureInfo.InvariantCulture))
            service.EnqueueUpload(key, InterfaceName, url, data)
        End Using
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
                        Dim queuedUrl As String = Convert.ToString(row("BusinessKey"))
                        If String.IsNullOrWhiteSpace(queuedUrl) Then queuedUrl = url
                        Dim response As HttpResponseMessage = Await client.PostAsync(queuedUrl, content)
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
