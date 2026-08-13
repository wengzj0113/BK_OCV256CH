Imports DevExpress.Utils.Filtering.Internal
Imports Newtonsoft.Json
Imports System.Net.Http
Imports System.Text
Imports System.Threading.Tasks
Imports System.Threading
Imports System.Data
Imports System.Security.Cryptography


Public Class FrmMesTask

    ' 创建HttpClient实例
    Private client As New HttpClient()
    Private ReadOnly ocvDataService As New OcvDataService()
    Private WithEvents chkHeartbeat As CheckBox
    Private WithEvents chkStateAndStopReason As CheckBox
    Private WithEvents chkAlarmUpload As CheckBox
    Private WithEvents chkEnergyUpload As CheckBox
    Private WithEvents chkResultDataUploadAssembly As CheckBox
    Private WithEvents chkWIPInStationCheck As CheckBox
    Private timer As Timer
    Private interval As Integer = 1000
    '单独做测试数据上传接口，因为这个数据上传会放在测试完成后直接进行
    Private resultDataUploader As ResultDataUploadAssembly
    Private wipInStationCheck As WIPInStationCheck

    '单例标记:避免与 FrmMesTask 同时运行造成重复上报 (R2-16)
    Private Shared _instanceCount As Integer = 0

    Public Sub New()
        ' 此调用是设计器所必需的。
        InitializeComponent()
        ' 初始化多线程定时器
        timer = New Timer(AddressOf TimerCallback, Nothing, Timeout.Infinite, Timeout.Infinite)
        System.Threading.Interlocked.Increment(_instanceCount)

    End Sub
    ' 更新ListBox的方法
    Private Sub UpdateOutput(message As String)
        If listBoxOutput.InvokeRequired Then
            listBoxOutput.Invoke(New Action(Of String)(AddressOf UpdateOutput), message)
        Else
            listBoxOutput.Items.Add(message)
            listBoxOutput.TopIndex = listBoxOutput.Items.Count - 1 ' 自动滚动到最后一行
        End If
    End Sub
    ' 检查网络状态
    Private Function IsNetworkAvailable() As Boolean
        Try
            Using client As New HttpClient()
                client.Timeout = TimeSpan.FromSeconds(2)
                Dim response = client.GetAsync("http://172.16.110.10:9015/").Result
                Return response.IsSuccessStatusCode
            End Using
        Catch
            Return False
        End Try
    End Function
    ' 上传数据函数
    Private Async Function UploadDataAsync(url As String, data As Object, interfaceName As String) As Task
        Dim json As String = JsonConvert.SerializeObject(data)
        Try
            If Not IsNetworkAvailable() Then
                EnqueueUpload(json, interfaceName, url)
                Return
            End If

            Using content As New StringContent(json, Encoding.UTF8, "application/json")
                Dim response As HttpResponseMessage = Await client.PostAsync(url, content)
                Dim responseString As String = Await response.Content.ReadAsStringAsync()

                If response.IsSuccessStatusCode AndAlso IsMesSuccess(responseString) Then
                    UpdateOutput($"{interfaceName} 数据上传成功")
                Else
                    UpdateOutput($"{interfaceName} 数据上传失败，HTTP状态码: {response.StatusCode}")
                    EnqueueUpload(json, interfaceName, url)
                End If
            End Using
        Catch ex As Exception
            UpdateOutput($"{interfaceName} 上传异常，已进入重试队列: {ex.Message}")
            EnqueueUpload(json, interfaceName, url)
        End Try
    End Function

    Private Sub EnqueueUpload(json As String, interfaceName As String, targetUrl As String)
        Try
            Dim idempotencyKey As String
            Using sha As SHA256 = SHA256.Create()
                Dim source As Byte() = Encoding.UTF8.GetBytes(interfaceName & "|" & targetUrl & "|" & json)
                idempotencyKey = "MES-" & interfaceName & "-" &
                                 BitConverter.ToString(sha.ComputeHash(source)).Replace("-", String.Empty).Substring(0, 32)
            End Using
            ocvDataService.EnqueueUpload(idempotencyKey, interfaceName, targetUrl, json)
        Catch ex As Exception
            UpdateOutput($"{interfaceName} 失败数据写入上传队列失败: {ex.Message}")
        End Try
    End Sub

    ' Heartbeat数据上传
    Private Async Function SendHeartbeatDataAsync() As Task
        Dim url As String = GetMesUrl("heartbeat")
        Dim data As New Dictionary(Of String, String) From {
            {"EquipPC_ID", If(String.IsNullOrEmpty(Initsetting.EquipPC_ID), "HZWL", Initsetting.EquipPC_ID)},
            {"EquipPC_Password", If(String.IsNullOrEmpty(Initsetting.EquipPC_Password), "123456", Initsetting.EquipPC_Password)},
            {"Equip_Code", "ZJ01-P-COCV-A-C-0-0001"},
            {"Upload_Time", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},
            {"Is_Online", "true"}
        }
        Await UploadDataAsync(url, data, "Heartbeat")
    End Function

    Private Async Function SendStateAndStopReasonDataAsync() As Task
        Dim url As String = GetMesUrl("stateAndStopReason")
        Dim data As New Dictionary(Of String, String) From {
            {"EquipPC_ID", If(String.IsNullOrEmpty(Initsetting.EquipPC_ID), "HZWL", Initsetting.EquipPC_ID)},
            {"EquipPC_Password", If(String.IsNullOrEmpty(Initsetting.EquipPC_Password), "123456", Initsetting.EquipPC_Password)},
            {"Equip_Code", "ZJ01-P-COCV-A-C-0-0001"},
            {"Upload_Time", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},
            {"Status", "2"},
            {"Start_Time", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},
            {"Reason_Code", "2"}
        }
        Await UploadDataAsync(url, data, "stateAndStopReason")
    End Function

    Private Async Function SendAlarmUploadDataAsync() As Task
        Dim url As String = GetMesUrl("alarmUpload")
        Dim data As New Dictionary(Of String, Object) From {
            {"EquipPC_ID", If(String.IsNullOrEmpty(Initsetting.EquipPC_ID), "HZWL", Initsetting.EquipPC_ID)},
            {"EquipPC_Password", If(String.IsNullOrEmpty(Initsetting.EquipPC_Password), "123456", Initsetting.EquipPC_Password)},
            {"Equip_Code", "ZJ01-P-COCV-A-C-0-0001"},
            {"Upload_Time", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},
            {"Alarm_List", New List(Of Dictionary(Of String, String)) From {
                New Dictionary(Of String, String) From {
                    {"Alarm_Value", "1"},
                    {"Alarm_Code", "111"},
                    {"Start_Time", "2023/03/20 11:30:21"}
                },
                New Dictionary(Of String, String) From {
                    {"Alarm_Value", "0"},
                    {"Alarm_Code", "111"},
                    {"Start_Time", "2023/03/20 11:31:21"}
                }
            }}
        }
        Await UploadDataAsync(url, data, "alarmUpload")
    End Function

    Private Async Function SendEnergyUploadDataAsync() As Task
        Dim url As String = GetMesUrl("energyUpload")
        Dim data As New Dictionary(Of String, Object) From {
            {"EquipPC_ID", If(String.IsNullOrEmpty(Initsetting.EquipPC_ID), "HZWL", Initsetting.EquipPC_ID)},
            {"EquipPC_Password", If(String.IsNullOrEmpty(Initsetting.EquipPC_Password), "123456", Initsetting.EquipPC_Password)},
            {"Equip_Code", "ZJ01-P-COCV-A-C-0-0001"},
            {"Upload_Time", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},
            {"Param_List", New List(Of Dictionary(Of String, String)) From {
                New Dictionary(Of String, String) From {
                    {"Param_Code", "Param1"},
                    {"Param_Value", "123"}
                },
                New Dictionary(Of String, String) From {
                    {"Param_Code", "Param2"},
                    {"Param_Value", "456"}
                }
            }}
        }
        Await UploadDataAsync(url, data, "energyUpload")
    End Function

    Private Async Function SendResultDataUploadAssemblyAsync() As Task
        Dim url As String = GetMesUrl("resultDataUploadAssembly")
        Dim data As New Dictionary(Of String, Object) From {
            {"EquipPC_ID", If(String.IsNullOrEmpty(Initsetting.EquipPC_ID), "HZWL", Initsetting.EquipPC_ID)},
            {"EquipPC_Password", If(String.IsNullOrEmpty(Initsetting.EquipPC_Password), "123456", Initsetting.EquipPC_Password)},
            {"Equip_Code", "ZJ01-P-COCV-A-C-0-0001"},
            {"Check_Time", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},
            {"Operator", "张三"},
            {"Wip_List", New List(Of Dictionary(Of String, String)) From {
                New Dictionary(Of String, String) From {
                    {"WIP_NO", "LDX-20230413-022"}
                },
                New Dictionary(Of String, String) From {
                    {"WIP_NO", "LDX-20230413-052"}
                }
            }}
        }
        Await UploadDataAsync(url, data, "resultDataUploadAssembly")
    End Function

    Private Async Function SendWIPInStationCheckDataAsync() As Task
        Dim url As String = GetMesUrl("wipInStationCheck")
        Dim data As New Dictionary(Of String, Object) From {
            {"EquipPC_ID", If(String.IsNullOrEmpty(Initsetting.EquipPC_ID), "HZWL", Initsetting.EquipPC_ID)},
            {"EquipPC_Password", If(String.IsNullOrEmpty(Initsetting.EquipPC_Password), "123456", Initsetting.EquipPC_Password)},
            {"Equip_Code", "ZJ01-P-COCV-A-C-0-0001"},
            {"Check_Time", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},
            {"Operator", "张三"},
            {"Wip_List", New List(Of Dictionary(Of String, String)) From {
                New Dictionary(Of String, String) From {
                    {"WIP_NO", "LDX-20230413-022"}
                },
                New Dictionary(Of String, String) From {
                    {"WIP_NO", "LDX-20230413-052"}
                }
            }}
        }
        Await UploadDataAsync(url, data, "wipInStationCheck")
    End Function

    ' 上传本地存储的待上传数据
    Private Async Function UploadPendingDataAsync() As Task
        If IsNetworkAvailable() Then
            Dim interfaceNames As String() = {
                "Heartbeat",
                "stateAndStopReason",
                "alarmUpload",
                "energyUpload",
                "resultDataUploadAssembly",
                "wipInStationCheck"
            }

            For Each interfaceName As String In interfaceNames
                Dim pendingUploads As DataTable = ocvDataService.GetPendingUploads(
                    interfaceName, 100, "FrmMesTask.Compat", 120)

                For Each row As DataRow In pendingUploads.Rows
                    Dim uploadId As Long = Convert.ToInt64(row("UploadOutboxId"))
                    Dim jsonData As String = Convert.ToString(row("PayloadJson"))
                    Dim content As New StringContent(jsonData, Encoding.UTF8, "application/json")
                    Dim url As String = GetMesUrl(interfaceName)

                    Try
                        Dim response As HttpResponseMessage = Await client.PostAsync(url, content)
                        Dim responseString As String = Await response.Content.ReadAsStringAsync()

                        If response.IsSuccessStatusCode AndAlso IsMesSuccess(responseString) Then
                            Console.WriteLine($"{interfaceName} 数据续传成功")
                            ocvDataService.MarkUploadSent(uploadId)
                        Else
                            Console.WriteLine($"{interfaceName} 数据续传失败")
                            Console.WriteLine("响应状态码: " & response.StatusCode)
                            Console.WriteLine("响应内容: " & responseString)
                            ocvDataService.MarkUploadFailed(uploadId, responseString)
                        End If
                    Catch ex As Exception
                        ocvDataService.MarkUploadFailed(uploadId, ex.Message)
                    End Try
                Next
            Next
        End If
    End Function

    Private Shared Function IsMesSuccess(responseJson As String) As Boolean
        Try
            Dim responseData = JsonConvert.DeserializeObject(Of Dictionary(Of String, String))(responseJson)
            Dim returnCode As String = Nothing
            If responseData IsNot Nothing AndAlso responseData.TryGetValue("Return_Code", returnCode) Then
                Return String.Equals(returnCode, "S", StringComparison.OrdinalIgnoreCase)
            End If
        Catch
        End Try
        Return False
    End Function

    ' 启动按钮事件
    Private Sub btnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        Dim interval As Integer
        If Integer.TryParse(txtInterval.Text, interval) AndAlso interval >= 1 AndAlso interval <= 999 Then
            StartTimer()
        Else
            MessageBox.Show("请输入1到999之间的有效数字。")
        End If
    End Sub

    ' 停止按钮事件
    Private Sub btnStop_Click(sender As Object, e As EventArgs) Handles btnStop.Click
        StopTimer()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Hide()

    End Sub
    ' 定时器回调函数
    Private Sub TimerCallback(state As Object)
        If chkHeartbeat.Checked Then
            Task.Run(AddressOf SendHeartbeatDataAsync)
        End If
        If chkStateAndStopReason.Checked Then
            Task.Run(AddressOf SendStateAndStopReasonDataAsync)
        End If
        If chkAlarmUpload.Checked Then
            Task.Run(AddressOf SendAlarmUploadDataAsync)
        End If
        If chkEnergyUpload.Checked Then
            Task.Run(AddressOf SendEnergyUploadDataAsync)
        End If
        If chkResultDataUploadAssembly.Checked Then
            Task.Run(AddressOf SendResultDataUploadAssemblyAsync)
        End If
        If chkWIPInStationCheck.Checked Then
            Task.Run(AddressOf SendWIPInStationCheckDataAsync)
        End If
    End Sub

    ' 开始定时器
    Private Sub StartTimer()
        timer.Change(0, Interval)
    End Sub

    ' 停止定时器
    Private Sub StopTimer()
        Timer.Change(Timeout.Infinite, Timeout.Infinite)
    End Sub

    ' 设置定时器间隔
    Private Sub SetTimerInterval(newInterval As Integer)
        interval = newInterval
        ' Timer.Change(0, interval)
    End Sub

    Private Sub txtInterval_Leave(sender As Object, e As EventArgs) Handles txtInterval.Leave
        Dim inputValue As String = txtInterval.Text
        Dim result As Integer

        ' 尝试将文本框的值转换为整型
        If Integer.TryParse(inputValue, result) Then
            interval = result * 1000
            'MessageBox.Show("最终值已转换并赋值给interval: " & interval)
            UpdateOutput($"最终值已转换并赋值给interval:{interval}")
        Else
            MessageBox.Show("输入的值不是有效的整数，请重新输入")
        End If
    End Sub

    Private Sub FrmEMSTask_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        '若 FrmMesTask 已在运行则提示并关闭,避免双重上报 (R2-16)
        If _instanceCount > 1 Then
            MsgBox("FrmMesTask 已经在运行,请勿重复启动 FrmEMSTask,以免重复上报 MES。", vbOKOnly, "重复实例")
            Me.Close()
            Return
        End If
        resultDataUploader = New ResultDataUploadAssembly(listBoxOutput)
        wipInStationCheck = New WIPInStationCheck(listBoxOutput)
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        System.Threading.Interlocked.Decrement(_instanceCount)
        MyBase.OnFormClosed(e)
    End Sub

    'URL 由 MesApiAddress 配置项拼接 (R2-14)
    Private Shared Function GetMesUrl(suffix As String) As String
        Dim baseAddr As String = If(String.IsNullOrEmpty(Initsetting.MesApiAddress), "http://172.16.110.10:9015", Initsetting.MesApiAddress)
        Return baseAddr.TrimEnd("/"c) & "/api/" & suffix
    End Function
End Class


