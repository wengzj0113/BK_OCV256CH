Imports System.Data
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading.Thread
Imports System.Timers
Imports Ivi.Visa.Interop
Imports MpFByteArrDLL
Imports File = System.IO.File
Imports Timer = System.Timers.Timer
Public Class frmTest
    Implements IAutomaticTestHost, IManualTestHost, IPreparedTrayChannelsProvider, IPlcSmokeMonitorHost
    Dim PaintFlag As Boolean
    Dim objOLECommamd As OleDb.OleDbCommand
    Dim intNumber As Short
    Dim obj_bat As Bat_struct
    Public Ftest As Collection
    Public Stest As Collection
    Public Ttest As Collection
    Public totalCellNumToTest As Integer
    Public firstCellNumToTest As Integer
    Public lastCellNumToTest As Integer
    Public Property SelectedChannelNumbers As List(Of Integer)
    Dim intF, intS, intT As Short
    Dim blstop As Boolean
    Public mSock As SocketCtrolCmd
    Public FrmLink As New List(Of frmTest)
    Public mSocket As Socket
    Public fixtureClose As Boolean
    Private WithEvents timer3 As New Timer()
    Private WithEvents temperatureTimer As New Timer()
    Private temperatureModuleClient As NtcTemperatureModuleClient
    Private timer3PollBusy As Integer
    Private temperaturePollBusy As Integer
    Private lastTemperatureModuleError As String = String.Empty
    Private lastNtcReadings As NtcTemperatureReading()
    Private lastNtcReadingsAtUtc As DateTime = DateTime.MinValue
    Private temperatureAlarmLatched As Boolean
    Private temperatureAlarmRequiresUnavailableSensors As Boolean
    Private ioAlarmLatched As Boolean
    Private smokeAlarmLatched As Boolean
    Private smokeSignalReadFailed As Boolean
    Private lastSmokeInputObservedAtUtc As DateTime = DateTime.MinValue
    Private Const SmokeAlarmIsActiveLow As Boolean = False
    Private alarmCylinderReleaseState As Integer
    Private resetFailureLatched As Boolean
    Private resetInProgress As Boolean
    Private ReadOnly pendingResetChannelsToClear As New List(Of Integer)()
    '窗体是否已加载完成（用于屏蔽 Resize 期间重绘，避免初始化阶段空引用）
    Private _isLoaded As Boolean = False
    Private shutdownPrepared As Boolean = False
    Private modeChangeInProgress As Boolean
    Private initializationApplyInProgress As Boolean
    Private deferredStartupScheduled As Boolean
    Private deferredStartupCompleted As Boolean
    Private firstPaintRecorded As Boolean
    Private lastTestSelection As TestChannelSelection
    Private currentTestChannels As List(Of Integer)
    Private ReadOnly testingChannelNumbers As New HashSet(Of Integer)()
    Private automaticEventLogFilePath As String = String.Empty
    Private channelStatusColors As ChannelStatusColors = ChannelStatusColors.CreateDefault()
    Public autoStartFlag As Boolean '自动测试标志
    Public haveNgPoint As Boolean '存在NG的点
    Public hjwd As String
    Public trayCode As String
    '存放校准值的文件：安装目录可能位于 WindowsApps，只读；改用当前用户可写目录。
    Private ReadOnly filePath As String = GetCalibrationFilePath()
    Dim alarmStatus As Boolean = False
    Dim logDirectory As String = AppLog.LogDirectory
    Dim logFilePath As String = Path.Combine(logDirectory, "Run.log")
    Private ReadOnly testProgressState As New TestProgressState()
    Private ReadOnly automaticEventLogWriter As New AutomaticEventLogWriter()
    Private plcRuntimeStatusGeneration As Integer
    Private testProgressDialog As frmTestProgress
    Private plcAutomationClient As IPlcAutomationClient
    Private trayScannerClient As ITrayScannerClient
    Private plcAutomaticCoordinator As PlcAutomaticTestCoordinator
    Private automaticTestPassInProgress As Boolean
    Private manualNgRetestCancellation As System.Threading.CancellationTokenSource
    Private Const ManualNgFixturePressTimeoutMs As Integer = 30000
    Private activeTrayPlan As TrayTestPlan
    Private activeTrayDebugMode As Boolean
    Private pendingTraySkipNotice As String = String.Empty
    Private lastPublishedAutomaticTrayCode As String = String.Empty
    Private calibrationLoadError As String = String.Empty

    Private Function IsStopRequested() As Boolean
        Return System.Threading.Volatile.Read(blstop)
    End Function

    Private Sub SetStopRequested(value As Boolean)
        System.Threading.Volatile.Write(blstop, value)
    End Sub

    Private Sub CancelPendingManualNgRetest()
        Dim cancellation As System.Threading.CancellationTokenSource = manualNgRetestCancellation
        If cancellation Is Nothing Then Return
        Try
            cancellation.Cancel()
        Catch ex As ObjectDisposedException
        End Try
    End Sub

    Public ReadOnly Property CurrentAlarmValue As Integer
        Get
            Return If(alarmStatus, 1, 0)
        End Get
    End Property

    Public ReadOnly Property CurrentAlarmCodeValue As String
        Get
            Return If(alarmStatus, "ALARM", "")
        End Get
    End Property

    Public ReadOnly Property CurrentAlarmStartTimeValue As DateTime
        Get
            Return DateTime.Now
        End Get
    End Property

    Private Sub frmTest_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
        StartupTiming.Mark("frmTest-load-begin")
        InitializeChannelSelectionControls()
        InitializeLeftPanelButtonStyles()
        StartupTiming.Mark("frmTest-selection-controls")
        'R2-18:提前标记已加载完成,确保 frmTest_Resize 的布局调整始终执行
        '       (若 myConnection 未打开,后面 Exit Sub 会提前返回,原 _isLoaded=True 在末尾无法执行)
        _isLoaded = True
        '*******************************
        '显示相关信息
        '数据服务器字段已从主测试页移除。
        'lblBarNum.Text = "*"
        ReadInitFile()
        StartupTiming.Mark("frmTest-read-init")
        channelStatusColors = ChannelColorSettings.FromInitset(Initsetting)
        channelGrid.ApplyStatusColors(channelStatusColors)
        statisticsControl.ApplyStatusColors(channelStatusColors)
        ConfigureStationControls()
        ocvStationReady = True
        InitializeAutomaticTestControls()
        StartupTiming.Mark("frmTest-station-controls")
        '合格范围
        txtMaxV.Text = Initsetting.upVoltage
        txtMinV.Text = Initsetting.downVoltage
        txtMaxR.Text = Initsetting.upResistance
        txtMinR.Text = Initsetting.downResistance
        '**************************************
        '探针相关
        txtTotle.Text = Initsetting.Probelifespan.ToString
        txtUsed.Text = Initsetting.ProbeUsedCount.ToString
        txtRemain.Text = (Initsetting.Probelifespan - Initsetting.ProbeUsedCount).ToString
        StartupTiming.Mark("frmTest-combo-begin")
        For index = 1 To MAX_CHANNEL
            cmb_ChSelect.Items.Add(index)
        Next
        cmb_ChSelect.SelectedIndex = 0
        StartupTiming.Mark("frmTest-channel-combo")
        '读取校准值数组
        StartupTiming.Mark("frmTest-calibration-begin")
        Try
            Dim snapshot As CalibrationSnapshot = CalibrationService.SharedInstance.LoadSnapshot()
            calibrationLoadError = String.Empty
            SetTestStatus("校准补偿已载入：" & snapshot.SourceDescription &
                          If(snapshot.UncalibratedChannelCount > 0,
                             "；未校准通道 " & snapshot.UncalibratedChannelCount.ToString() & " 个",
                             String.Empty) &
                          If(String.IsNullOrWhiteSpace(CalibrationService.SharedInstance.LastCacheWriteWarning),
                             String.Empty,
                             "；" & CalibrationService.SharedInstance.LastCacheWriteWarning))
        Catch ex As Exception
            calibrationLoadError = ex.Message
            SetTestStatus("校准补偿载入失败，已禁止测试：" & ex.Message)
        End Try
        StartupTiming.Mark("frmTest-calibration")
        '**********************************
        '****************************
        '刷新界面
        If Pic_Bat IsNot Nothing AndAlso Pic_Bat.Visible Then
            FreshBat(0, 0, intNumber, RecieveData, 1)
        End If
        StartupTiming.Mark("frmTest-overview-begin")
        RefreshChannelOverview()
        StartupTiming.Mark("frmTest-channel-overview")
        channelGrid.SelectAll()
        StartupTiming.Mark("frmTest-selection-all")
        '****************************
        LabelAlarm.Visible = False
        LabelRun.Visible = True
        ResetTestProgressDisplay()
        '初始化托盘状态为打开
        fixtureClose = False
        '获取分选条件项目到g_objCN
        btnStop.Enabled = False
        CmdStart.Enabled = CalibrationService.HasCurrentSnapshot
        UpdateTestModeVisuals()
        smokeAlarm1.BackColor = Color.Green
        smokeAlarm2.BackColor = Color.Green
        StartupTiming.Mark("frmTest-initial-ui")
        ''启动mes
        'FrmMesTask.ShowDialog()
        ''FrmMesTask.Activate()
        'FrmMesTask.Hide()
        'Rbtn2.Checked = True
        'Timer1.Start()
        '加载完成后才允许 Resize 触发的重绘，避免初始化阶段空引用。
        _isLoaded = True
        StartupTiming.Mark("frmTest-load-end")
        '布局稳定后强制左侧滚动区回到顶部,保证首屏从工位选择/托盘信息开始
        Me.BeginInvoke(New MethodInvoker(AddressOf ResetLeftPanelScroll))
        StartTemperaturePolling()
    End Sub

    Private Sub frmTest_Paint(ByVal sender As Object, ByVal e As PaintEventArgs) Handles MyBase.Paint
        If firstPaintRecorded Then Return
        firstPaintRecorded = True
        StartupTiming.Mark("frmTest-first-paint")

        If deferredStartupScheduled OrElse Not IsHandleCreated Then Return
        deferredStartupScheduled = True
        BeginInvoke(New MethodInvoker(AddressOf StartDeferredStartupWork))
    End Sub

    Private Sub StartDeferredStartupWork()
        If deferredStartupCompleted OrElse ApplicationShuttingDown OrElse IsDisposed Then Return
        deferredStartupCompleted = True
        StartupTiming.Mark("frmTest-deferred-start")

        '先确立 PLC 串口所有者，再启动后台设备检查，避免旧 Modbus 检查抢占 ASCII 串口。
        StartPlcAutomaticTest()
        If frmMDI IsNot Nothing AndAlso Not frmMDI.IsDisposed Then
            frmMDI.StartExternalDeviceChecksAsync()
        End If

        System.Threading.ThreadPool.QueueUserWorkItem(Sub()
                                                          If ApplicationShuttingDown Then Return
                                                          Try
                                                              GetCN()
                                                          Catch ex As Exception
                                                              System.Diagnostics.Debug.WriteLine("后台加载分档字段失败: " & ex.Message)
                                                          End Try
                                                      End Sub)

        '新系统不再轮询控制板的 JIG/IO 状态。
        Timer4.Start()

        StartupTiming.Mark("frmTest-deferred-complete")
    End Sub

    Private Sub StartTemperaturePolling()
        If shutdownPrepared OrElse ApplicationShuttingDown OrElse IsDisposed Then Return

        temperatureTimer.Interval = 5000
        temperatureTimer.Start()
        AppLog.Write("Temperature", "NTC polling timer started: COM" & (Initsetting.TemperatureComPort + 1).ToString() & ", interval=5000 ms.")
    End Sub

    Private Sub InitializeLeftPanelButtonStyles()
        UiTheme.ApplyButtonStyle(btnGetBarcode, UiTheme.ButtonRole.Primary)
        UiTheme.ApplyButtonStyle(CmdStart, UiTheme.ButtonRole.Primary)
        UiTheme.ApplyButtonStyle(btnStop, UiTheme.ButtonRole.Danger)
        UiTheme.ApplyButtonStyle(btnNGTest, UiTheme.ButtonRole.Warning)
        UiTheme.ApplyButtonStyle(btnReset, UiTheme.ButtonRole.Danger)
    End Sub

    Private Sub InitializeAutomaticTestControls()
        Dim retestCountControl As ComboBox = FindAutomaticControl(Of ComboBox)("cmbRetestNum")
        Dim needRetestControl As CheckBox = FindAutomaticControl(Of CheckBox)("btnNeedNGRetest")
        If retestCountControl Is Nothing OrElse needRetestControl Is Nothing Then Return

        If retestCountControl.Items.Count = 0 Then
            For roundNumber As Integer = 1 To 3
                retestCountControl.Items.Add(roundNumber.ToString())
            Next
        End If
        If retestCountControl.SelectedIndex < 0 Then retestCountControl.SelectedIndex = 0
        needRetestControl.Checked = False
    End Sub

    Private Function FindAutomaticControl(Of T As Control)(ByVal controlName As String) As T
        If String.IsNullOrWhiteSpace(controlName) Then Return Nothing
        Return TryCast(Controls.Find(controlName, True).FirstOrDefault(), T)
    End Function

    Private Sub InitializeChannelSelectionControls()
        SetSelectionModeButton(btnChannelSelectSingle)
        AddHandler channelGrid.SelectedChannelsChanged, AddressOf HandleSelectedChannelsChanged
        HandleSelectedChannelsChanged(channelGrid, EventArgs.Empty)
    End Sub

    Private Sub HandleChannelSelectAll(ByVal sender As Object, ByVal e As EventArgs) Handles btnChannelSelectAll.Click
        channelGrid.SelectAll()
    End Sub

    Private Sub HandleChannelClear(ByVal sender As Object, ByVal e As EventArgs) Handles btnChannelClear.Click
        channelGrid.ClearSelection()
    End Sub

    Private Sub HandleChannelRectangle(ByVal sender As Object, ByVal e As EventArgs) Handles btnChannelRectangle.Click
        channelGrid.SelectionMode = ChannelSelectionMode.Rectangle
        SetSelectionModeButton(btnChannelRectangle)
    End Sub

    Private Sub HandleChannelSingle(ByVal sender As Object, ByVal e As EventArgs) Handles btnChannelSelectSingle.Click
        channelGrid.SelectionMode = ChannelSelectionMode.SingleChannel
        SetSelectionModeButton(btnChannelSelectSingle)
    End Sub

    Private Sub SetSelectionModeButton(ByVal activeButton As Button)
        Dim buttons() As Button = {btnChannelSelectAll, btnChannelClear, btnChannelRectangle, btnChannelSelectSingle}
        For Each button As Button In buttons
            Dim role As UiTheme.ButtonRole = If(button Is activeButton, UiTheme.ButtonRole.Primary, UiTheme.ButtonRole.Neutral)
            UiTheme.ApplyButtonStyle(button, role)
        Next
    End Sub

    Private Sub HandleSelectedChannelsChanged(ByVal sender As Object, ByVal e As EventArgs)
        If channelGrid Is Nothing Then Return
        SelectedChannelNumbers = channelGrid.SelectedChannelNumbers
        'lblBarNum.Text = channelGrid.SelectedChannelCount.ToString()
    End Sub

    Private Sub SetSelectionControlsEnabled(ByVal enabled As Boolean)
        Dim buttons() As Button = {btnChannelSelectAll, btnChannelClear, btnChannelRectangle, btnChannelSelectSingle}
        For Each button As Button In buttons
            button.Enabled = enabled
        Next
        channelGrid.Enabled = enabled
        If enabled Then channelGrid.RefreshHoverDetailsAtCursor()
    End Sub

    Public Function GetSignalTrayStatus() As Boolean
        If Initsetting.PLCMode = 0 Then
            Try
                If plcAutomaticModeActive OrElse plcAutomationClient Is Nothing OrElse Not plcAutomationClient.IsOpen Then
                    SetTestStatus("手动 PLC 串口未就绪")
                    Return False
                End If
                fixtureClose = plcAutomationClient.ReadR151()
                SetTestStatus(If(fixtureClose, "托盘已压合", "托盘未压合"))
                Return fixtureClose
            Catch ex As Exception
                fixtureClose = False
                SetTestStatus("托盘压合状态读取失败：" & ex.Message)
                Return False
            End Try
        End If
        If Not plcConnected OrElse plcController Is Nothing Then
            fixtureClose = False
            SetTestStatus("PLC未连接，无法读取托盘状态")
            Return False
        End If
        Try
            fixtureClose = plcController.ReadTrayStatus()
        Catch ex As Exception
            fixtureClose = False
            SetTestStatus("托盘状态读取失败")
            Return False
        End Try
        If fixtureClose Then
            fixtureClose = True
            GetSignalTrayStatus = True
            SetTestStatus("托盘已关闭")
        Else
            GetSignalTrayStatus = False
            SetTestStatus("托盘已打开")
        End If
        '----------------------------------------------------------
    End Function
    Private Function ConnectTCP(ByVal mSocket As Socket, ByVal ipe As IPEndPoint) As String
        Try
            Dim connResult As IAsyncResult = mSocket.BeginConnect(ipe, Nothing, Nothing)
            connResult.AsyncWaitHandle.WaitOne(800, True)
            If (Not connResult.IsCompleted) Then
                mSocket.Close()
                Throw New Exception(ipe.ToString() & "无法连接")
            End If
            Return ""

        Catch ex As Exception
            Return ex.Message
        End Try
    End Function
    Protected Sub ParseBuffer(ByVal port As Integer, ByVal buffer As FByteArrBuild, ByRef PointPos As Integer)

    End Sub
    Private Sub SetSock(ByVal Sock As SocketCtrolCmd)
        mSock = Sock
        For i As Integer = 0 To FrmLink.Count - 1
            FrmLink(i).SetSock(mSock)
        Next
    End Sub
    Public Function TrayOpen() As Boolean
        Me.Cursor = Cursors.Default
        If Not plcConnected OrElse plcController Is Nothing Then
            SetTestStatus("PLC未连接，无法打开托盘")
            Return False
        End If
        Try
            If plcController.OpenTray() AndAlso Not plcController.ReadTrayStatus() Then
                fixtureClose = False
                SetTestStatus("托盘已开启")
                Return True
            Else
                SetTestStatus("操作托盘开启失败！")
                Return False
            End If
        Catch ex As Exception
            SetTestStatus("操作托盘开启失败！")
            Return False
        End Try
    End Function


    Public Function channelFinishSwitch() As Boolean
        Dim msg1 As String
        Dim temp As Byte()
        Dim temp2 As Byte()
        mSock = frmDebug.mSock
        mSocket = frmDebug.mSocket
        Try
            If mSock IsNot Nothing Then
                mSock.Close()
            End If
        Catch ex As Exception

        End Try
        Dim mClientTCP = New TcpClient()
        Dim sock = mClientTCP.Client
        Dim myIP As IPAddress = IPAddress.Parse(Initsetting.SCMIpAdd)
        Dim ipe As IPEndPoint = New IPEndPoint(myIP, Initsetting.SCM_port)
        Dim msg = ConnectTCP(sock, ipe)
        If msg = "" Then
            mSock = New SocketCtrolCmd(sock, AddressOf ParseBuffer, Initsetting.SCM_port, ipe)
            channelFinishSwitch = True
            'MsgBox("单片机通信成功！", vbOKOnly)
        Else
            MsgBox("单片机通信失败！", vbOKOnly)
            MessageBox.Show(msg)
            Return False
        End If
        temp = mSock.GetSignalSelectMutiChannel(firstCellNumToTest, lastCellNumToTest)
        msg1 = mSock.Send(temp)
        ' 加超时 + 异常捕获,避免中位机无响应时永久阻塞 UI (M24)
        Const MAX_FINISH_RETRY As Integer = 20
        Dim finishRetry As Integer = 0
        Dim bytes(20) As Byte
        Dim bytesRec As Integer = 0
        channelFinishSwitch = False
        Do While finishRetry < MAX_FINISH_RETRY
            temp2 = mSock.GetSignalMutiChannelTestFinish '获得多信道切换是否完成
            msg1 = mSock.Send(temp2)
            Sleep(900)
            Me.Cursor = Cursors.Default
            Try
                sock.ReceiveTimeout = 2000
                bytesRec = sock.Receive(bytes)
            Catch
                bytesRec = 0
            End Try
            '判断是否完成通道切换，获取测试结果
            If bytesRec > 4 AndAlso bytes(4) = 1 Then
                channelFinishSwitch = True
                Exit Do
            End If
            finishRetry += 1
        Loop
        ' 超时则保持 channelFinishSwitch = False,由调用方决定是否继续
        sock.ReceiveTimeout = 0
    End Function

    Dim mSocket1 As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    '保护 mSocket1 跨调用方并发访问 (R2-17)
    Private ReadOnly mSocket1Lock As New Object()
    Public Function socketSR(ByVal temp As Byte()) As Byte()
        ' 创建Socket对象mSocket
        Try
            SyncLock mSocket1Lock
                ' 设置IP地址和端口号
                Dim ipAddress As IPAddress = IPAddress.Parse("192.168.69.101")
                Dim port As Integer = 1030
                ' 连接到指定的IP地址和端口号 (R2-17:用 BeginConnect + WaitOne 加 2 秒超时)
                If mSocket1.Connected = False Then
                    Dim connectResult As IAsyncResult = mSocket1.BeginConnect(New IPEndPoint(ipAddress, port), Nothing, Nothing)
                    If Not connectResult.AsyncWaitHandle.WaitOne(2000, False) Then
                        Try
                            mSocket1.Close()
                        Catch
                        End Try
                        mSocket1 = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                        Return Nothing
                    End If
                    mSocket1.EndConnect(connectResult)
                End If
                ' 发送数据send[10]
                mSocket1.Send(temp)
                ' 接收数据到receive[20]中(2 秒超时,避免中位机离线时启动卡 75-130 秒)
                mSocket1.ReceiveTimeout = 2000
                Dim receiveData As Byte() = New Byte(1024) {}
                Dim receivedBytes As Integer = mSocket1.Receive(receiveData)
                Dim newArray(receivedBytes - 1) As Byte
                Array.Copy(receiveData, newArray, receivedBytes)
                'dataOfTemPeraturesAndIO = newArray
                Return newArray
            End SyncLock
        Catch ex As Exception
            ' 处理网络连接和断开时可能出现的异常
            Console.WriteLine("发生错误： " & ex.Message)
        Finally
            ' 关闭Socket连接
            'mSocket1.Close()
        End Try
        Return Nothing
    End Function

    Public Function socketSR1(ByVal temp As Byte()) As Byte()
        ' 创建Socket对象mSocket
        Try
            SyncLock mSocket1Lock
                ' 设置IP地址和端口号
                Dim ipAddress As IPAddress = IPAddress.Parse("192.168.69.101")
                Dim port As Integer = 1030

                ' 连接到指定的IP地址和端口号 (R2-17:用 BeginConnect + WaitOne 加 2 秒超时,避免中位机不通时阻塞 75-130 秒)
                If mSocket1.Connected = False Then
                    Dim connectResult As IAsyncResult = mSocket1.BeginConnect(New IPEndPoint(ipAddress, port), Nothing, Nothing)
                    If Not connectResult.AsyncWaitHandle.WaitOne(2000, False) Then
                        ' 超时则放弃连接,释放 socket,本调用返回 Nothing
                        Try
                            mSocket1.Close()
                        Catch
                        End Try
                        mSocket1 = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                        Return Nothing
                    End If
                    mSocket1.EndConnect(connectResult)
                End If
                ' 发送数据send[10]
                mSocket1.Send(temp)
                ' 接收数据到receive[20]中(2 秒超时,避免中位机离线时启动卡 75-130 秒)
                mSocket1.ReceiveTimeout = 2000
                Dim receiveData As Byte() = New Byte(1024) {}
                Dim receivedBytes As Integer = mSocket1.Receive(receiveData)
                Dim newArray(receivedBytes - 1) As Byte
                Array.Copy(receiveData, newArray, receivedBytes)
                If newArray.Length = 22 Then
                    dataOfTemPeraturesAndIO = newArray
                End If
                Return newArray
            End SyncLock
        Catch ex As Exception
            ' 处理网络连接和断开时可能出现的异常
            Console.WriteLine("发生错误： " & ex.Message)
        Finally
            ' 关闭Socket连接
            'mSocket1.Close()
        End Try
        Return Nothing
    End Function
    Private Sub CmdStart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdStart.Click
        If Not AuthorizationService.RequireAuthenticatedUser(Me) Then Return
        If resetInProgress Then
            MessageBox.Show("设备正在复位，请复位完成后再启动测试。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        If alarmStatus Then
            MessageBox.Show("目前处于报警锁定状态，请先确认报警源正常并完成复位。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        If chkAutoTest.Checked OrElse plcAutomaticModeActive Then
            MessageBox.Show("自动模式由 PLC 启动，请先切换到手动模式。", "提示", MessageBoxButtons.OK)
            Return
        End If
        If isRuning Then Return
        If plcAutomaticCoordinator Is Nothing Then
            MessageBox.Show("手动 PLC 测试周期未就绪，请检查 PLC 串口模式和连接。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Try
            Dim selectedChannelSnapshot As Integer() = If(SelectedChannelNumbers, New List(Of Integer)()).ToArray()
            If Not plcAutomaticCoordinator.RequestManualCycle(txtTray.Text, selectedChannelSnapshot) Then
                MessageBox.Show("当前已有测试周期，不能重复启动。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As OperationCanceledException
            SetTestStatus("已取消本次手动测试启动。")
        Catch ex As NoTestableTrayChannelsException
            ReportAutomaticEvent("手动测试未启动，未发送R160：" & ex.Message)
            SetTestStatus("手动托盘没有可测电芯，测试未启动")
        Catch ex As Exception
            MessageBox.Show("测试前准备失败：" & ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            UpdateTestModeVisuals()
        End Try
    End Sub
    ''' <summary>
    ''' 启动测试：1：正常测试；0：NG补测
    ''' </summary>
    ''' <param name="testType"></param>
    ''' <returns></returns>
    Public Function StarTest(ByVal testType As Integer,
                             Optional ByVal automaticMode As Boolean = False,
                             Optional ByVal automaticCompleted As Action(Of TestPassResult) = Nothing,
                             Optional ByVal automaticFailed As Action(Of Exception) = Nothing,
                             Optional ByVal automaticChannels As IEnumerable(Of Integer) = Nothing,
                             Optional ByVal manualCyclePass As Boolean = False,
                             Optional ByVal trayCodeSnapshot As String = Nothing)
        StarTest = 0
        Dim automaticFailure As Exception = Nothing
        Dim automaticResult As TestPassResult = Nothing
        Dim runCalibrationSnapshot As CalibrationSnapshot = Nothing
        If resetInProgress Then
            automaticFailure = New InvalidOperationException("设备正在复位，当前测试启动已取消。")
            GoTo exit_sub
        End If
        If Not AuthorizationService.IsAuthenticated() Then
            If automaticMode Then
                automaticFailure = New InvalidOperationException("自动测试要求先完成用户登录。")
                GoTo exit_sub
            End If
            AuthorizationService.RequireAuthenticatedUser(Me)
            Return StarTest
        End If
        If CalibrationService.IsCalibrationAcquisitionInProgress OrElse CalibrationService.IsBarcodeRetestInProgress Then
            automaticFailure = New InvalidOperationException("通道校准或扫码复测正在采集，当前不能启动测试。")
            If Not automaticMode Then
                MessageBox.Show(automaticFailure.Message, "校准采集忙", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return StarTest
            End If
            GoTo exit_sub
        End If
        Try
            If CalibrationService.HasCurrentSnapshot Then
                runCalibrationSnapshot = CalibrationService.SharedInstance.CurrentSnapshot
            Else
                runCalibrationSnapshot = CalibrationService.SharedInstance.LoadSnapshot()
            End If
            calibrationLoadError = String.Empty
        Catch ex As Exception
            calibrationLoadError = ex.Message
            automaticFailure = New InvalidOperationException("校准补偿值不可用，已阻止测试：" & ex.Message, ex)
            If Not automaticMode Then
                MessageBox.Show(automaticFailure.Message, "校准值不可用", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return StarTest
            End If
            GoTo exit_sub
        End Try
        Dim runChannels As New List(Of Integer)()
        If testType = 0 Then
            If retestArray IsNot Nothing Then
                For channelNo As Integer = 1 To Math.Min(MAX_CHANNEL, retestArray.Length)
                    If retestArray(channelNo - 1) = 1 Then runChannels.Add(channelNo)
                Next
            End If
        ElseIf automaticMode AndAlso automaticChannels IsNot Nothing Then
            runChannels = automaticChannels.
                Where(Function(channelNo) channelNo >= 1 AndAlso channelNo <= MAX_CHANNEL).
                Distinct().
                OrderBy(Function(channelNo) channelNo).
                ToList()
        ElseIf SelectedChannelNumbers IsNot Nothing Then
            runChannels = SelectedChannelNumbers.
                Where(Function(channelNo) channelNo >= 1 AndAlso channelNo <= MAX_CHANNEL).
                Distinct().
                OrderBy(Function(channelNo) channelNo).
                ToList()
        End If

        If runChannels.Count = 0 Then
            If automaticMode Then
                automaticFailure = New InvalidOperationException("自动测试没有可执行的通道。")
                GoTo exit_sub
            End If
            MessageBox.Show("请先选择至少一个待测试通道！", "提示", MessageBoxButtons.OK)
            Return StarTest
        End If
        Dim deviceChannels As List(Of Integer) = runChannels.
            Select(Function(channelNo) TrayChannelMapping.ToDeviceChannel(channelNo)).
            ToList()
        'Dim intLoop As Int16
        Dim reFlag As Short
        Dim I As Int16
        Dim strTrayNo As String
        Dim Buffer_SCM(5) As Byte
        Dim Buffer_Re(5) As Byte
        Dim mesUP As New ResultDataUploadAssembly(FrmMesTask.listBoxOutput)
        If pendingTraySkipNotice.Length > 0 Then
            Dim skippedEventLine As String = DateTime.Now.ToString("HH:mm:ss") & " " & pendingTraySkipNotice
            PrependEventLogLine(skippedEventLine)
        End If
        'frmTest_Resize1(）
        Pic_Bat.Refresh()
        Sleep(100)

        frmMDI.Refresh()
        frmMDI.txtTesting.Text = ""
        frmMDI.Refresh()
        '设定为测试状态
        SetStopRequested(False)
        isRuning = True
        UpdateTestModeVisuals()
        SetSelectionControlsEnabled(False)
        '改变鼠标形状
        Me.Cursor = Cursors.WaitCursor
        '***********************************
        '初始化变量
        firstCellNumToTest = runChannels.Min()
        lastCellNumToTest = runChannels.Max()
        totalCellNumToTest = runChannels.Count
        strTrayNo = If(manualCyclePass, If(trayCodeSnapshot, String.Empty).Trim(), Trim(txtTray.Text))
        Ftest = New Collection
        Stest = New Collection
        Ttest = New Collection
        intF = 0 : intS = 0 : intT = 0
        '初始话保存测试结果的数组
        If (testType = 1) Then
            For I = 0 To MAX_CHANNEL - 1
                RecieveData(I).Voltage = 0
                RecieveData(I).Resistance = 0
                RecieveData(I).RawResistanceMilliohm = 0
                RecieveData(I).CalibrationValueMilliohm = 0
                RecieveData(I).FinalResistanceMilliohm = 0
                RecieveData(I).TestTime = Now
            Next
        End If

        If strTrayNo = "" Then
            If automaticMode Then
                automaticFailure = New InvalidOperationException("托盘号为空，自动测试不能启动。")
            Else
                MsgBox("托盘号为空，请完成扫码！", vbOKOnly, "提示")
            End If
            GoTo exit_sub

        Else
            Dim result2 As DialogResult
            If testType = 1 AndAlso automaticMode AndAlso Not manualCyclePass AndAlso strTrayNo = lastPublishedAutomaticTrayCode Then
                automaticFailure = New InvalidOperationException("自动流程检测到重复托盘码：" & strTrayNo)
                GoTo exit_sub
            End If
            If testType = 1 AndAlso strTrayNo = lastTrayNo AndAlso Not automaticMode Then

                result2 = MessageBox.Show("托盘号与上次相同，是否继续执行程序？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question)

                If result2 = DialogResult.Cancel Then
                    ' 用户选择取消，退出程序
                    ' Application.Exit()
                    GoTo exit_sub
                ElseIf result2 = DialogResult.OK Then

                End If
            ElseIf testType = 1 AndAlso Not automaticMode Then
                lastTrayNo = strTrayNo
            End If

        End If

        ' 记录日志
        LogRun(logFilePath, strTrayNo, "开始测试")
        '***********************************
        'OCV阶段由设备配置固定，启动时只验证配置是否有效。
        If currentOCVNum < 1 OrElse currentOCVNum > 3 Then
            If automaticMode Then
                automaticFailure = New InvalidOperationException("未选择 OCV 测试阶段。")
            Else
                MsgBox("请选择OCV！", vbOKOnly, "提示")
            End If
            GoTo exit_sub
        End If
        BeginTestProgress(runChannels.Count, If(testType = 0, "准备 NG 复测", "准备正式测试"))
        PublishTestProgress(TestProgressStage.Preparing, "正在检查测试设备，请稍候...", 5)
        '***********************************
        '检测中位机串口通讯
        reFlag = CheckCom_SCM()
        If reFlag <> 1 Then
            automaticFailure = New InvalidOperationException("单片机通讯失败。")
            If Not automaticMode Then MsgBox("单片机通讯失败！", vbOKOnly)
            GoTo exit_sub
        End If
        '处理探针的相关数据
        Initsetting.ProbeUsedCount = Initsetting.ProbeUsedCount + 1
        txtUsed.Text = Initsetting.ProbeUsedCount.ToString
        txtRemain.Text = (Initsetting.Probelifespan - Initsetting.ProbeUsedCount).ToString
        If Initsetting.Probelifespan - Initsetting.ProbeUsedCount = 0 Then
            If automaticMode Then
                SetTestStatus("探针已达到使用上限，请联系供应商更换探针")
            Else
                MsgBox("探针的使用次数以达到上限，请联系供应商更换探针！", vbOKOnly, "提示")
            End If
        End If

        frmMDI.txtTesting.Text = "测试中......"
        Sleep(100)
        frmMDI.Refresh()

        '本次测试必须从当前计划重新生成通道选择，不能沿用上一次 Runner 的结果。
        lastTestSelection = Nothing
        currentTestChannels = runChannels.ToList()
        MarkSelectedChannelsTesting()
        Dim ocvTestTaskId As Long? = Nothing
        Dim stageTestTime As DateTime? = Nothing
        Dim testTaskStatusFinalized As Boolean = False
        Try
            If Not automaticMode Then
                PublishTestProgress(TestProgressStage.CreatingTask, "正在创建 OCV_DATA 测试任务...", 10)
                ocvTestTaskId = CreateOcvDataTestTask(testType)
            Else
                PublishTestProgress(TestProgressStage.CreatingTask, "自动测试准备完成，最终结果统一写入 OCV_DATA...", 10)
            End If
            PublishTestProgress(TestProgressStage.Preparing, "正在准备仪表和控制板...", 20)
        Catch ex As Exception
            FailTestProgress("创建测试任务失败：" & ex.Message)
            automaticFailure = ex
            If Not automaticMode Then MsgBox("OCV_DATA 创建测试任务失败: " & ex.Message, vbOKOnly, "错误")
            GoTo exit_sub
        End Try
        '************** 正式测试：中位机只负责通道计划和 START，仪表缓存完成后统一回填 *********************
        Try
            PublishTestProgress(TestProgressStage.Testing, "正在执行通道测试，请勿操作...", 25)
            Dim formalRunner As New FormalTestRunner(
                New SerialMiddleMachineClient(Com_SCM),
                New TestChannelPlanService(),
                New TwoPassResultMapper(),
                Sub(applyAction)
                    If Me.InvokeRequired Then
                        Me.Invoke(applyAction)
                    Else
                        applyAction()
                    End If
                End Sub)
            Dim measurementTask As System.Threading.Tasks.Task(Of TestChannelSelection) =
                System.Threading.Tasks.Task.Run(Function() formalRunner.RunSparseMeasurement(deviceChannels,
                                                                                               cancellationRequested:=Function() IsStopRequested(),
                                                                                               calibrationSnapshot:=runCalibrationSnapshot))
            While Not measurementTask.IsCompleted
                Application.DoEvents()
                System.Threading.Thread.Sleep(25)
            End While
            lastTestSelection = measurementTask.GetAwaiter().GetResult()
            stageTestTime = DateTime.Now
            If IsStopRequested() Then Throw New OperationCanceledException("测试已由操作员停止。")
            PublishTestProgress(TestProgressStage.CollectingResults, "测量完成，正在读取和整理结果...", 70)
        Catch ex As Exception
            If TypeOf ex Is OperationCanceledException OrElse IsStopRequested() Then
                CancelTestProgress("测试已停止")
                CompleteOcvDataTestTaskSafely(ocvTestTaskId, "Canceled", "操作员停止了当前测试。")
                ClearIncompleteTestResults(runChannels)
                haveNgPoint = False
                SetTestStatus("测试已停止，等待托盘进入")
                ReportAutomaticEvent("测试已停止，本轮未完成结果未保存")
                If automaticMode Then automaticFailure = New OperationCanceledException("测试已由操作员停止。")
                GoTo exit_sub
            End If
            FailTestProgress("正式测试失败：" & ex.Message)
            CompleteOcvDataTestTaskSafely(ocvTestTaskId, "Failed", ex.Message)
            automaticFailure = ex
            If Not automaticMode Then MsgBox("正式测试出现异常: " & ex.Message, vbOKOnly, "错误")
            GoTo exit_sub
        End Try
        ''******************** 测试完成后的刷新、保存、上传和数据库处理 ***********************
        Try
            ''刷新界面
            If testType = 0 Then
                'FreshBat(1, 1, RecieveData, 0)
                FreshBat(1, firstCellNumToTest, totalCellNumToTest, RecieveData, testType)
            Else
                'FreshBat(1, 1, RecieveData, 1)
                FreshBat(1, firstCellNumToTest, totalCellNumToTest, RecieveData, testType)
            End If

            frmMDI.txtTesting.Text = "测试结束"
            Sleep(100)
            frmMDI.Refresh()
            ' 记录日志
            LogRun(logFilePath, strTrayNo, "测试结束")
            testProgressState.Advance(runChannels.Count, Nothing, "测量结果已完成，正在保存...")
            PublishTestProgress(TestProgressStage.Saving, "正在保存测试结果到 OCV_DATA...", 82)
            '设置电池的测试结果：OK或者NG'设定结果数组中的各个点位的测试结果， 分三种情况
            '1）全测，遍历所有的点位，逐个判断进行设定；(1,2合并，都作为选定的部分）
            '2）部分测：只对选定的部分进行判断；
            '3）复测NG点，只对复测的点进行判断。
            If testType = 1 Then
                For Each channel As Integer In GetSelectedStandardChannels(testType)
                    Dim channelIndex0 As Integer = channel - 1
                    Dim recVol As Single = RecieveData(channelIndex0).Voltage
                    Dim recRes As Single = RecieveData(channelIndex0).Resistance
                    If IsNG(recVol, recRes) <> 0 Then
                        RecieveData(channelIndex0).Result = "NG"
                    Else
                        RecieveData(channelIndex0).Result = "OK"
                    End If
                Next
            Else 'NG复测必须按本轮实际执行的通道赋值，不能依赖上一轮残留的 retestArray。
                For Each channel As Integer In GetSelectedStandardChannels(testType)
                    Dim channelIndex0 As Integer = channel - 1
                    Dim recVol As Single = RecieveData(channelIndex0).Voltage
                    Dim recRes As Single = RecieveData(channelIndex0).Resistance
                    If IsNG(recVol, recRes) <> 0 Then
                        RecieveData(channelIndex0).Result = "NG"
                    Else
                        RecieveData(channelIndex0).Result = "OK"
                    End If
                Next
            End If
            testingChannelNumbers.Clear()
            RefreshChannelOverview()
            If automaticMode Then automaticResult = BuildAutomaticPassResult(runChannels)
            If Not automaticMode Then
                SaveOcvDataTestResults(ocvTestTaskId, testType, stageTestTime.Value)
                SaveTestDataLocalFile(RecieveData)
                If Not ocvTestTaskId.HasValue Then Throw New InvalidOperationException("OCV_DATA 测试任务号不存在。")
                CompleteOcvDataTestTaskStrict(ocvTestTaskId.Value, "Completed", Nothing)
                testTaskStatusFinalized = True
                BusinessLogService.Write("操作", "成功", "测试结果保存完成", strTrayNo, "测试结果已保存到 OCV_DATA 和本地文件。", String.Format("测试类型={0}；OCV_DATA任务号={1}；通道数={2}", If(testType = 0, "NG复测", "正式测试"), ocvTestTaskId.Value, GetSelectedStandardChannels(testType).Count), "测试结果保存")
            End If
            If Not automaticMode Then
                '自动流程统一延迟到最终复测结束；手动流程完成本地保存后再提交 MES。
                PublishTestProgress(TestProgressStage.Saving,
                                    If(activeTrayDebugMode, "调试结果已保存到 OCV_DATA 和本地文件，不上传生产 MES...", "数据库和本地结果已保存，正在提交后台上传..."),
                                    92)
                If Not activeTrayDebugMode AndAlso FrmMesTask.chkResultDataUploadAssembly.Checked Then
                    Try
                        mesUP.ChannelNumbers = GetSelectedStandardChannels(testType)
                        mesUP.StopTimer()
                        mesUP.StartTimer()
                    Catch uploadError As Exception
                        AppLog.Write("MES", "后台上传启动失败：" & uploadError.ToString())
                    End Try
                End If
            Else
                PublishTestProgress(TestProgressStage.Saving, "本轮结果已保存，等待最终复测判定...", 92)
            End If
            If Not automaticMode Then
                PublishTestProgress(TestProgressStage.Uploading,
                                    If(activeTrayDebugMode, "调试结果不上传生产 MES。", "测试完成，MES 上传已进入后台队列。"),
                                    96)
                CompleteTestProgress("测试完成")
            End If
            '旧库 CW_tblData_Info 并行写入已删除 (R2-15):
            '测试结果已由 SaveOcvDataTestResults 写入 OCV_DATA.TestResult,
            '托盘已由 CreateOcvDataTestTask -> usp_RegisterTray 注册到 OCV_DATA.Tray。
        Catch ex As Exception
            FailTestProgress("结果保存失败：" & ex.Message)
            If Not automaticMode Then
                BusinessLogService.Write("操作", "失败", "测试结果保存失败", strTrayNo, "测试结果未能完整保存。", ex.ToString(), "测试结果保存")
            Else
                BusinessLogService.Write("操作", "失败", "自动测试结果处理失败", strTrayNo, "本轮测量结果处理失败。", ex.ToString(), "测试结果保存")
            End If
            automaticFailure = ex
            If Not automaticMode Then MsgBox("OCV 测量或数据上传出现异常: " & ex.Message, vbOKOnly, "错误")
        End Try
        If ocvTestTaskId.HasValue AndAlso Not testTaskStatusFinalized Then
            CompleteOcvDataTestTaskSafely(ocvTestTaskId, "Failed", "Test result processing failed before task completion.")
            testTaskStatusFinalized = True
        End If
        '如果存在NG点，询问是否重测坏点
        If haveNgPoint AndAlso Not automaticMode AndAlso automaticFailure Is Nothing Then
            Dim result1 As DialogResult = MessageBox.Show("发现NG测试点，是否重新执行测试相应点位？", "NG提示", MessageBoxButtons.OKCancel)
            If result1 = DialogResult.OK Then
                ' 用户点击了 OK，执行处理程序的代码
                'MessageBox.Show("请再次点击测试按钮，谢谢", "NG提示", MessageBoxButtons.OK)
                ' 处理错误的代码
                '复位NG标志
                haveNgPoint = False
                '启动NG测试
                Call StarTest(0)
            Else
                '用户点击了 Cancel，不执行处理程序
            End If
        End If
exit_sub:
        StopTestProgressIfNeeded()
        testingChannelNumbers.Clear()
        RefreshChannelOverview()
        currentTestChannels = Nothing
        'PLC负责测试完成后的后续动作，上位机只恢复自身测试控件。
        SetSelectionControlsEnabled(True)
        Me.Cursor = Cursors.Default
        If IsStopRequested() Then SetStopRequested(False)
        isRuning = False
        UpdateTestModeVisuals()
        If automaticMode Then
            automaticTestPassInProgress = False
            If automaticFailure IsNot Nothing Then
                If automaticFailed IsNot Nothing Then automaticFailed(automaticFailure)
            ElseIf automaticResult Is Nothing Then
                If automaticFailed IsNot Nothing Then automaticFailed(New InvalidOperationException("自动测试未生成有效测试结果。"))
            ElseIf automaticCompleted IsNot Nothing Then
                automaticCompleted(automaticResult)
            End If
        End If
        Return 0

    End Function

    Private Function CreateOcvDataTestTask(ByVal testType As Integer,
                                           Optional ByVal trayCodeOverride As String = Nothing) As Long?
        Dim authenticatedUserName As String = AuthorizationService.CurrentUserName
        If String.IsNullOrWhiteSpace(authenticatedUserName) Then
            Throw New InvalidOperationException("当前未登录，无法创建测试任务。")
        End If

        Dim service As New OcvDataService()
        Dim channels As List(Of Integer) = GetSelectedStandardChannels(testType)
        Dim request As New OcvCreateTestTaskRequest With {
            .TaskCode = BuildOcvDataTaskCode(testType, trayCodeOverride),
            .IdempotencyKey = BuildOcvDataIdempotencyKey(testType, trayCodeOverride),
            .TrayCode = If(trayCodeOverride Is Nothing, Trim(txtTray.Text), trayCodeOverride.Trim()),
            .OcvStageCode = GetOcvStageCode(),
            .TestType = If(activeTrayDebugMode, "Debug", If(testType = 0, "Retest", "Formal")),
            .SelectedChannelsJson = BuildChannelJson(channels),
            .FirstChannelNo = If(channels.Count > 0, CType(channels.Min(), Integer?), Nothing),
            .LastChannelNo = If(channels.Count > 0, CType(channels.Max(), Integer?), Nothing),
            .ChannelCount = channels.Count,
            .EquipmentCode = "XL_OCV_256CH",
            .WorkstationName = Environment.MachineName,
            .OperatorName = authenticatedUserName,
            .IsRetest = (testType = 0)
        }

        Dim table As DataTable = service.CreateTestTask(request)
        If table.Rows.Count = 0 OrElse IsDBNull(table.Rows(0)("TestTaskId")) Then
            Throw New InvalidOperationException("usp_CreateTestTask did not return TestTaskId.")
        End If

        Return Convert.ToInt64(table.Rows(0)("TestTaskId"))
    End Function

    Private Sub SaveOcvDataTestResults(ByVal testTaskId As Long?, ByVal testType As Integer, ByVal stageTestTime As DateTime)
        If Not testTaskId.HasValue Then
            Return
        End If

        Dim results As New List(Of OcvTestResultInput)()
        Dim channels As List(Of Integer) = GetSelectedStandardChannels(testType)
        For orderIndex As Integer = 0 To channels.Count - 1
            Dim channel As Integer = channels(orderIndex)
            Dim index0 As Integer = channel - 1
            If index0 < 0 OrElse index0 > UBound(RecieveData) Then
                Continue For
            End If

            Dim measuredResult As Bat_info = RecieveData(index0)
            Dim finalResistance As Decimal = Convert.ToDecimal(measuredResult.FinalResistanceMilliohm)
            Dim calibrationValue As Decimal = Convert.ToDecimal(measuredResult.CalibrationValueMilliohm)
            Dim rawResistance As Decimal = Convert.ToDecimal(measuredResult.RawResistanceMilliohm)
            Dim ngCode As Integer = IsNG(RecieveData(index0).Voltage, RecieveData(index0).Resistance)

            results.Add(New OcvTestResultInput With {
                .ChannelNo = channel,
                .BatteryCode = RecieveData(index0).BatCode,
                .ExecutionOrder = orderIndex + 1,
                .VoltageVolt = Convert.ToDecimal(measuredResult.Voltage),
                .RawResistanceMilliohm = rawResistance,
                .CalibrationValueMilliohm = calibrationValue,
                .FinalResistanceMilliohm = finalResistance,
                .IsNg = (ngCode <> 0),
                .NgReason = BuildNgReason(ngCode),
                .ResultSource = If(activeTrayDebugMode, "Debug", If(testType = 0, "Retest", "FormalTest"))
            })
        Next

        Dim service As New OcvDataService()
        service.SaveTestResults(testTaskId.Value, stageTestTime, results)
    End Sub

    Private Sub CompleteOcvDataTestTaskSafely(ByVal testTaskId As Long?,
                                              ByVal status As String,
                                              ByVal errorMessage As String)
        If Not testTaskId.HasValue Then
            Return
        End If

        Try
            Dim service As New OcvDataService()
            service.CompleteTestTask(testTaskId.Value, status, errorMessage)
        Catch ex As Exception
        End Try
    End Sub

    Private Function GetSelectedStandardChannels(ByVal testType As Integer) As List(Of Integer)
        If currentTestChannels IsNot Nothing AndAlso currentTestChannels.Count > 0 Then
            Return currentTestChannels.ToList()
        End If

        If lastTestSelection IsNot Nothing AndAlso
           lastTestSelection.SelectedPhysicalChannels IsNot Nothing AndAlso
           lastTestSelection.SelectedPhysicalChannels.Count > 0 Then
            Return lastTestSelection.SelectedPhysicalChannels.
                Select(Function(channel) TrayChannelMapping.ToStandardChannel(channel)).
                Distinct().OrderBy(Function(channel) channel).ToList()
        End If

        If testType <> 0 AndAlso SelectedChannelNumbers IsNot Nothing AndAlso SelectedChannelNumbers.Count > 0 Then
            Return SelectedChannelNumbers.Distinct().OrderBy(Function(channel) channel).ToList()
        End If

        Dim channels As New List(Of Integer)()
        If testType = 0 Then
            If retestArray IsNot Nothing Then
                For i As Integer = 0 To retestArray.Length - 1
                    If retestArray(i) = 1 Then
                        channels.Add(i + 1)
                    End If
                Next
            End If
        Else
            For channel As Integer = firstCellNumToTest To lastCellNumToTest
                If channel >= 1 AndAlso channel <= MAX_CHANNEL Then
                    channels.Add(channel)
                End If
            Next
        End If

        Return channels.Distinct().OrderBy(Function(channel) channel).ToList()
    End Function

    Private Function GetOcvStageCode() As String
        Select Case currentOCVNum
            Case 1
                Return "OCV0"
            Case 2
                Return "OCV1"
            Case 3
                Return "OCV2"
            Case Else
                Return "OCV" & Math.Max(0, currentOCVNum - 1).ToString()
        End Select
    End Function

    Private Function BuildOcvDataTaskCode(ByVal testType As Integer,
                                          Optional ByVal trayCodeOverride As String = Nothing) As String
        Dim taskType As String = If(activeTrayDebugMode, "DEBUG", If(testType = 0, "RETEST", "FORMAL"))
        Return String.Format("{0}-{1}-{2}-{3:yyyyMMddHHmmssfff}",
                             If(trayCodeOverride Is Nothing, Trim(txtTray.Text), trayCodeOverride.Trim()),
                             GetOcvStageCode(),
                             taskType,
                             DateTime.Now)
    End Function

    Private Function BuildOcvDataIdempotencyKey(ByVal testType As Integer,
                                                 Optional ByVal trayCodeOverride As String = Nothing) As String
        Dim taskType As String = If(activeTrayDebugMode, "DEBUG", If(testType = 0, "RETEST", "FORMAL"))
        Return String.Format("{0}|{1}|{2}|{3:yyyyMMddHHmmssfff}",
                             If(trayCodeOverride Is Nothing, Trim(txtTray.Text), trayCodeOverride.Trim()),
                             GetOcvStageCode(),
                             taskType,
                             DateTime.Now)
    End Function

    Private Function BuildChannelJson(ByVal channels As IEnumerable(Of Integer)) As String
        Return "[" & String.Join(",", channels.Select(Function(channel) channel.ToString())) & "]"
    End Function

    Private Function BuildNgReason(ByVal ngCode As Integer) As String
        Select Case ngCode
            Case 1
                Return "Voltage out of range"
            Case 2
                Return "Resistance out of range"
            Case Else
                Return Nothing
        End Select
    End Function

    Sub LogRun(logFilePath As String, trayNo As String, status As String)
        Dim logEntry As String = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - 托盘号: {trayNo} - 状态: {status}"
        Try
            AppLog.Write("Run", logEntry)
        Catch ex As Exception
            MsgBox("写入日志文件时出错: " & ex.Message, vbOKOnly, "错误")
        End Try
    End Sub

    ''' <summary>
    ''' 把测试数据保存为本地文件
    ''' </summary>
    ''' <param name="recieveData"></param>
    Friend Shared Function FormatLocalResultRows(ByVal recieveData() As Bat_info,
                                                 ByVal ocvStage As Integer,
                                                 ByVal channels As IEnumerable(Of Integer),
                                                 ByVal temperatures() As String) As IReadOnlyList(Of String)
        If ocvStage < 1 OrElse ocvStage > 3 Then Throw New ArgumentOutOfRangeException(NameOf(ocvStage))
        If recieveData Is Nothing OrElse recieveData.Length < MAX_CHANNEL Then Throw New ArgumentException("测试数据不足256通道。", NameOf(recieveData))
        If channels Is Nothing Then Throw New ArgumentNullException(NameOf(channels))
        If temperatures Is Nothing OrElse temperatures.Length <> 4 Then Throw New ArgumentException("需要四个温度值。", NameOf(temperatures))

        Dim rows As New List(Of String)()
        For Each channelNo As Integer In channels.Distinct().OrderBy(Function(item) item)
            If channelNo < 1 OrElse channelNo > MAX_CHANNEL Then Throw New ArgumentOutOfRangeException(NameOf(channels))
            Dim item As Bat_info = recieveData(channelNo - 1)
            If String.IsNullOrWhiteSpace(item.BatCode) Then Continue For
            rows.Add($"{item.BatCode},{item.Result},{channelNo},{item.Resistance},{item.Voltage * 1000},{item.TestTime},{temperatures(0)},{temperatures(1)},{temperatures(2)},{temperatures(3)},{TrayChannelMapping.ToDeviceChannel(channelNo)}")
        Next
        Return rows
    End Function

    Private Sub SaveTestDataLocalFile(recieveData() As Bat_info,
                                      Optional ByVal trayCodeOverride As String = Nothing)
        Dim resultRows As IReadOnlyList(Of String) = FormatLocalResultRows(
            recieveData, currentOCVNum, currentTestChannels, {t1.Text, t2.Text, String.Empty, String.Empty})
        ' 示例托盘号
        Dim trayNum As String = If(trayCodeOverride Is Nothing, txtTray.Text, trayCodeOverride.Trim())

        ' 获取当前日期并生成文件夹路径
        Dim currentDate As String = DateTime.Now.ToString("yyyy-MM-dd")
        Dim baseDirectory As String = "d:\OcvTestData"
        Dim dateDirectory As String = Path.Combine(baseDirectory, currentDate)

        ' 创建日期文件夹（如果不存在）
        If Not Directory.Exists(dateDirectory) Then
            Directory.CreateDirectory(dateDirectory)
        End If

        '' 生成CSV文件路径
        'Dim filePath As String = Path.Combine(dateDirectory, trayNum & ".csv")
        '' 生成CSV备份文件路径
        Dim filePathBackup As String = Path.Combine(dateDirectory, trayNum & "bk.csv")
        ' 生成初始的CSV文件路径
        Dim filePath As String = Path.Combine(dateDirectory, trayNum & ".csv")
        Dim fileCount As Integer = 0

        ' 检查并生成唯一文件名
        While IO.File.Exists(filePath)
            fileCount += 1
            filePath = Path.Combine(dateDirectory, trayNum & "_" & fileCount.ToString() & ".csv")
        End While
        '' 检查并生成唯一文件名
        'While IO.File.Exists(filePathBackup)
        '    fileCount += 1
        '    filePathBackup = Path.Combine(dateDirectory, trayNum & "_" & fileCount.ToString() & ".csv")
        'End While

        ' 创建文件并写入数据
        Using writer As New StreamWriter(filePath, False, New System.Text.UTF8Encoding(True))
            ' 写入文件头
            writer.WriteLine("电池条码,测试结果,通道号,内阻(mΩ),电压(mV),测试时间,TA1,TA2,TA3,TA4,维修通道")

            For Each row As String In resultRows
                writer.WriteLine(row)
            Next
        End Using
        Console.WriteLine("CSV 文件已生成。")
    End Sub


    Public Sub Trig34461(ByVal mSock1 As SocketCtrolCmd, ByVal firstCellNum As Integer, ByVal testNum1 As Integer, ByVal testType As Integer) ' As String()

        Dim rm As Ivi.Visa.Interop.ResourceManager = New Ivi.Visa.Interop.ResourceManager()   '打开一个新的资源管理器
        Dim myDmm As Ivi.Visa.Interop.FormattedIO488 = New Ivi.Visa.Interop.FormattedIO488() '打开新的格式化IO 488会话
        Dim delayTim As Integer
        Dim commString As String
        Dim temp As Byte()
        Dim retestArray1 As Int16() = New Int16(255) {}  '复测点位数组
        ReDim retestArray1(255)
        Array.Copy(retestArray, retestArray1, retestArray.Length)
        '确定有多少个点需要测试
        Dim testNum As Integer
        If testType = 0 Then '判断是否为NG重测
            temp = mSock.GetSignalSelectMutiChannelNG(retestArray)
            testNum = 0
            For i = 0 To retestArray.Length - 1
                If retestArray(i) = 1 Then
                    testNum = testNum + 1
                End If
            Next
        Else
            temp = mSock.GetSignalSelectMutiChannel(firstCellNumToTest, lastCellNumToTest)
            testNum = testNum1
        End If
        delayTim = testNum * 300
        Try
            'Dim DutAddr As String = "TCPIP0::192.168.66.81"   'LAN的示例字符串TCPIP0::192.168.128.58::hislip0::INSTR
            Dim DutAddr As String = "TCPIP0::192.168.66.81::inst0::INSTR"
            DutAddr = Initsetting.IpAddOf34461
            myDmm.IO = rm.Open(DutAddr, AccessMode.NO_LOCK, 2000, "") '打开DMM的手柄，超时2秒 
            myDmm.IO.Timeout = testNum * 500  '您也可以通过执行此命令设置超时，设置为10秒.要计算，不通点位数不同
            '先复位
            myDmm.IO.Clear()  '首先发送一个设备清除，以停止任何正在进行的测量
            myDmm.WriteString("*RST")  'Reset the device
            myDmm.WriteString("*IDN?")  'Get the IDN string                
            Dim IDN As String = myDmm.ReadString()
            Console.WriteLine(IDN)  'report the DMM's identity
            '配置DCV 100V范围，10mV分辨率这是一个快速读数
            myDmm.WriteString("CONF:VOLT:DC")
            '配置触发器计数
            commString = "TRIG:COUN " + testNum.ToString()
            myDmm.WriteString(commString)  ' Set the trigger count to 64. 
            '配置触发器源
            myDmm.WriteString("TRIG:SOUR EXT")
            '配置Ext触发器以查找正斜率信号。
            myDmm.WriteString("TRIG:SLOP POS")
            '设置电压表完整连接器以输出正信号。测量完成后，此接头总是发出信号。
            '此命令仅决定信号的极性。它不会打开或关闭信号。
            myDmm.WriteString("OUTP:TRIG:SLOP POS")
            myDmm.WriteString("READ?") '将DMM设置为等待触发状态，并在下一个触发信号后获取读数。
            Threading.Thread.Sleep(1000)
            '发送多信道切换信号给中位机，中位机负责新到切换
            socketSR(temp)
            Dim DCVResult As Object() = myDmm.ReadList(IEEEASCIIType.ASCIIType_Any)  '获取数组中的读数
            '继续拆分数组元素，关键字为“，”，拆分三项：arr(0)通道号，arr(1)成电阻值，arr(2)电压值
            'Dim arr() = DCVResult.Split(",")
            '定义ch ==元素素组数组（0）-1
            Dim ch As Integer
            If (testType = 1) Then
                ch = firstCellNum
                '把读出的电压数据放在RecieveData(ch).Voltage中
                For ch = firstCellNum - 1 To firstCellNum + testNum - 1
                    RecieveData(ch).Voltage = DCVResult(ch - （firstCellNum - 1）)
                    If RecieveData(ch).Voltage > 10000 Then
                        RecieveData(ch).Voltage = 10000
                    End If
                    'TextBox1.AppendText((RecieveData(ch).Voltage).ToString() & vbCrLf)
                Next
            End If
            Dim j1 As Integer
            If (testType = 0) Then
                For i = 0 To DCVResult.Length - 1
                    For j1 = 0 To retestArray1.Length - 1
                        If retestArray1(j1) = 1 Then
                            ch = j1
                            retestArray1(j1) = 0
                            RecieveData(ch).Voltage = DCVResult(i)
                            If RecieveData(ch).Voltage > 10000 Then
                                RecieveData(ch).Voltage = 10000
                            End If
                            Exit For
                        End If

                    Next

                Next
            End If
        Catch e As Exception
            Console.WriteLine("Error occured: " + e.Message)
        Finally
            '关闭您的资源
            Try
                myDmm.IO.Close()
            Catch ex As Exception

            End Try
            Try
                System.Runtime.InteropServices.Marshal.ReleaseComObject(myDmm)
            Catch ex As Exception

            End Try
            Try
                System.Runtime.InteropServices.Marshal.ReleaseComObject(rm)
            Catch ex As Exception

            End Try
        End Try
    End Sub
    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
    Dim intPaintFlag As Short
    Dim intPaintNumber As Short
    Private temp2 As Byte()
    Private lastTrayNo As String


    Private Sub FreshBat(ByVal intFlag As Short, ByVal firstNumber As Short, ByVal intNumber As Short, ByRef recieveData As Object, ByVal freshType As Short)
        ' 刷新电池显示的函数
        Dim bmp As Bitmap
        Dim obj_bat As Bat_struct
        Dim I, J As Int16
        Dim P1 As Point
        Dim P2 As Point
        Dim temp_string As String
        Dim obj_Font As Font
        Dim obj_pen As System.Drawing.Pen
        Dim Box As Rectangle
        Dim G As Graphics
        Dim sbrush As SolidBrush
        Dim TempNum As Int16
        Dim selectedStandardChannels As New HashSet(Of Integer)()
        If freshType = 0 AndAlso retestArray IsNot Nothing Then
            For channelNo As Integer = 1 To Math.Min(MAX_CHANNEL, retestArray.Length)
                If retestArray(channelNo - 1) > 0 Then selectedStandardChannels.Add(channelNo)
            Next
        ElseIf currentTestChannels IsNot Nothing Then
            For Each channelNo As Integer In currentTestChannels
                selectedStandardChannels.Add(channelNo)
            Next
        ElseIf SelectedChannelNumbers IsNot Nothing Then
            For Each channelNo As Integer In SelectedChannelNumbers
                selectedStandardChannels.Add(channelNo)
            Next
        End If
        'intNumber = 16
        ' 检查图像框的尺寸是否有效
        If Pic_Bat.Width <= 0 Or Pic_Bat.Height <= 0 Then
            Exit Sub
        End If
        ' 创建一个新的位图，并将其设置为图像框的图像
        bmp = New Bitmap(Pic_Bat.Width, Pic_Bat.Height)
        Pic_Bat.Image = bmp
        G = Graphics.FromImage(bmp)
        ' 清空图像为蓝色背景
        G.Clear(Color.Blue)
        ' 设置电池对象的位置和尺寸
        obj_bat.Left = Pic_Bat.Left
        obj_bat.Top = Pic_Bat.Top
        obj_bat.Hight = Pic_Bat.Size.Height / intRow
        obj_bat.width = Pic_Bat.Size.Width / intCol
        Dim index = 0
        Dim isPoint As Boolean = False '是测试点
        Dim isReTestPoint As Boolean = False '是复测点
        ' 循环处理每个电池格
        For I = 0 To intCol - 1
            For J = 0 To intRow - 1
                isPoint = selectedStandardChannels.Contains(intRow * I + J + 1)
                isReTestPoint = False
                TempNum = intRow * I + J
                If TempNum > (firstNumber + intNumber - 2) Or TempNum < (firstNumber - 1) Then
                    isPoint = False
                End If
                '如果是NG复测而且判断该点是不是复测点
                If (freshType = 0) Then
                    If retestArray(TempNum) > 0 And freshType = 0 Then
                        isReTestPoint = True
                    End If
                    If retestArray(TempNum) = 0 And freshType = 0 Then
                        isPoint = False
                    End If
                End If

                P1 = New Point(obj_bat.width * I, obj_bat.Hight * J)
                '如果NG，格子颜色为红色 weng20231208
                Dim recVol As Single = recieveData(TempNum).Voltage
                Dim recRes As Single = recieveData(TempNum).Resistance
                If IsNG(recVol, recRes) <> 0 And intFlag <> 0 And isPoint = True Then
                    ' NG 设置电池格子的画笔和填充红色
                    If isPoint = True Then
                        obj_pen = New Pen(Brushes.Red, 1)
                        sbrush = New SolidBrush(Color.Red)
                    Else
                        ' 非测试点，设置电池格子的画笔和填充绿色
                        obj_pen = New Pen(Brushes.LightGreen, 1)
                        sbrush = New SolidBrush(Color.LightGreen)
                    End If

                    haveNgPoint = True
                    '报警信息显示和保存入文件。
                    Dim alarmString As String
                    If IsNG(recVol, recRes) = 1 Then
                        alarmString = "电压不合格。"
                    Else
                        alarmString = "内阻不合格。"
                    End If
                    alarmString = ":通道" & (I + 1).ToString() & "-" & (J + 1).ToString() & alarmString

                    Dim timestamp As String = DateTime.Now.ToString()    ' 添加时间戳
                    Dim alarmLog As String = $"{timestamp}:NG报警 " & alarmString  ' 添加时间戳和报警信息
                    If txtAlarm.InvokeRequired Then
                    Else
                    End If
                    Try
                        AppLog.Write("Alarm", alarmLog)
                    Catch ex As Exception
                        System.Diagnostics.Debug.WriteLine("AlarmLog write failed: " & ex.Message)
                    End Try
                    BusinessLogService.WriteAlarm(alarmLog)
                    '把需要复测的编号对应数组值置1
                    retestArray(TempNum) = 1
                    index = index + 1
                Else
                    ' 合格，设置电池格子的画笔和填充绿色
                    obj_pen = New Pen(Brushes.LightGreen, 1)
                    sbrush = New SolidBrush(Color.LightGreen)
                End If
                ' 绘制电池格子的填充矩形
                Box = New Rectangle(P1, New Size(obj_bat.width, obj_bat.Hight / 5))
                G.FillRectangle(sbrush, Box)
                ' 绘制电池格子的边框矩形
                G.DrawRectangle(obj_pen, Box)
                ' 在电池格子上方显示电池编号
                obj_Font = New Font("宋体", 10, FontStyle.Regular)
                G.DrawString((I + 1).ToString & "-" & (J + 1).ToString, obj_Font, Brushes.Black, Box)
                ' 电池-------------------------------------------------------------------------------
                P1 = New Point(obj_bat.width * I + obj_bat.width / 2, obj_bat.Hight * J + obj_bat.Hight / 5)
                P2 = New Point(obj_bat.width * I + obj_bat.width / 2, obj_bat.Hight * J + obj_bat.Hight)
                ' 根据在线状态设置电池线的颜色
                If blnOnLine = False Then
                    obj_pen = New Pen(Brushes.Gray, obj_bat.width)
                Else
                    '测试后刷新，是NG点，是测试点，是复测点
                    If (intFlag = 1 And IsNG(recVol, recRes) <> 0) And isPoint = True Then
                        If freshType = 0 Then
                            If isReTestPoint = True Then
                                obj_pen = New Pen(Brushes.Red, obj_bat.width)
                            End If
                        Else
                            obj_pen = New Pen(Brushes.Red, obj_bat.width)
                        End If
                    Else
                        obj_pen = New Pen(Brushes.White, obj_bat.width)
                    End If

                End If
                ' 绘制电池线
                G.DrawLine(obj_pen, P1, P2)
                '----------------------------------------------------------------------------------------
                P1 = New Point(obj_bat.width * I, obj_bat.Hight * J + obj_bat.Hight / 5)
                Box = New Rectangle(P1, New Size(obj_bat.width, obj_bat.Hight - obj_bat.Hight / 5))
                ' 设置电池格子的边框颜色
                If intNumber = TempNum Then
                    obj_pen = New Pen(Brushes.Black, 1)
                Else
                    obj_pen = New Pen(Brushes.Black, 1)
                End If
                ' 绘制电池格子的边框
                G.DrawRectangle(obj_pen, Box)
                obj_Font = New Font("宋体", 10, FontStyle.Regular)
                If intFlag = 0 Then
                    ' 条形码
                    'temp_string = RecieveData(intNumber).BatCode
                    'G.DrawString(temp_string, obj_Font, Brushes.Black, Box)
                ElseIf intFlag = 1 Then
                    ' 显示电池电压和电阻信息
                    Dim offsetY As Integer = 0 ' 设置垂直偏移量
                    offsetY += obj_Font.Height ' 可能需要根据字体大小调整偏移量
                    temp_string = "V：" & （recieveData(TempNum).Voltage * 1000）.ToString & " mV"
                    If isPoint = False Then
                        temp_string = ""
                    End If
                    G.DrawString(temp_string, obj_Font, Brushes.Black, New PointF(Box.X + 10, Box.Y + offsetY))
                    ' 调整垂直位置
                    offsetY += obj_Font.Height + 5 ' 可能需要根据字体大小调整偏移量
                    temp_string = "R：" & (recieveData(TempNum).Resistance).ToString & " mΩ"
                    If isPoint = False Then
                        temp_string = ""
                    End If
                    G.DrawString(temp_string, obj_Font, Brushes.Black, New PointF(Box.X + 10, Box.Y + offsetY))
                End If
            Next J
        Next I

    End Sub
    Private Sub btnStop_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnStop.Click
        If IsStopRequested() OrElse shutdownPrepared OrElse resetInProgress Then Return
        Dim cycleActive As Boolean = plcAutomaticCoordinator IsNot Nothing AndAlso plcAutomaticCoordinator.IsCycleOccupied
        Dim manualNgRetestPending As Boolean = manualNgRetestCancellation IsNot Nothing
        If Not isRuning AndAlso Not cycleActive AndAlso Not manualNgRetestPending Then Return

        ReportAutomaticEvent("收到停止测试请求，正在停止当前测试周期")
        If manualNgRetestPending Then CancelPendingManualNgRetest()
        If isRuning Then
            SetStopRequested(True)
            UpdateTestModeVisuals()
            SetTestStatus("正在停止测试并复位设备...")
            Return
        End If
        If manualNgRetestPending AndAlso Not cycleActive Then
            SetTestStatus("正在取消等待针床压合...")
            Return
        End If

        '自动或手动周期都可能正在等待 PLC 请求，测量线程未运行时也需要通过协调器复位。
        If plcAutomaticCoordinator IsNot Nothing Then plcAutomaticCoordinator.RequestStopCurrentCycle()
        ClearIncompleteTestResults(Enumerable.Range(1, MAX_CHANNEL))
        haveNgPoint = False
        currentTestChannels = Nothing
        lastTestSelection = Nothing
        ResetTestProgressDisplay()
        SetStopRequested(False)
        UpdateTestModeVisuals()
    End Sub

    Private Async Sub btnReset_Click(sender As Object, e As EventArgs) Handles btnReset.Click
        If resetInProgress OrElse shutdownPrepared Then Return

        Dim cycleActive As Boolean = isRuning OrElse automaticTestPassInProgress OrElse
                                     (plcAutomaticCoordinator IsNot Nothing AndAlso plcAutomaticCoordinator.IsCycleOccupied)
        Dim hasCurrentTrayContext As Boolean = activeTrayPlan IsNot Nothing OrElse
                                               (txtTray IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txtTray.Text)) OrElse
                                               currentTestChannels IsNot Nothing OrElse
                                               pendingResetChannelsToClear.Count > 0 OrElse
                                               haveNgPoint
        If cycleActive OrElse hasCurrentTrayContext Then
            Dim decision As DialogResult = MessageBox.Show(
                "复位会取消当前流程，并清除当前托盘码、界面测量值和未完成结果；已保存结果、测试设置和历史日志会保留。是否继续？",
                "复位确认",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning)
            If decision <> DialogResult.Yes Then Return
        End If

        Dim channelsToClear As List(Of Integer) = CaptureCurrentResetChannels()
        pendingResetChannelsToClear.Clear()
        pendingResetChannelsToClear.AddRange(channelsToClear)
        resetInProgress = True
        If btnReset IsNot Nothing Then btnReset.Enabled = False
        UpdateTestModeVisuals()
        SetTestStatus("正在取消当前流程并等待设备退出...")

        Try
            CancelPendingManualNgRetest()
            If isRuning Then SetStopRequested(True)
            If plcAutomaticCoordinator IsNot Nothing Then plcAutomaticCoordinator.RequestStopCurrentCycle()

            Dim workerDeadline As DateTime = DateTime.UtcNow.AddSeconds(30)
            While isRuning OrElse automaticTestPassInProgress OrElse manualNgRetestCancellation IsNot Nothing
                If DateTime.UtcNow >= workerDeadline Then
                    resetFailureLatched = True
                    RefreshAlarmStatus()
                    Dim timeoutMessage As String = "复位失败：测试线程在30秒内没有安全退出；设备未复位，当前上下文已保留。"
                    SetTestStatus(timeoutMessage)
                    RecordResetEvent(timeoutMessage)
                    MessageBox.Show(timeoutMessage, "复位失败", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return
                End If
                Await System.Threading.Tasks.Task.Delay(100)
            End While

            SetStopRequested(False)
            ResetCurrentTrayContext(channelsToClear)
            SetTestStatus("测试线程已退出，正在复位中位机和仪表...")

            Dim resetResult As TestEnvironmentResetResult = Await System.Threading.Tasks.Task.Run(
                Function()
                    If Com_SCM Is Nothing OrElse Not Com_SCM.IsOpen Then
                        Dim unavailableResult As New TestEnvironmentResetResult()
                        unavailableResult.AddFailure("中位机", "串口未打开")
                        Return unavailableResult
                    End If
                    Return TestEnvironmentResetService.ResetAllWithResult(
                        New SerialMiddleMachineClient(Com_SCM),
                        closeInstrumentSessions:=False,
                        totalTimeoutMs:=30000)
                End Function)

            Dim alarmWasLatched As Boolean = temperatureAlarmLatched OrElse ioAlarmLatched OrElse
                                             smokeAlarmLatched OrElse smokeSignalReadFailed OrElse resetFailureLatched
            Dim temperatureSourceIsNormal As Boolean = True
            If temperatureAlarmLatched Then
                temperatureSourceIsNormal = Await RefreshTemperatureForResetAsync()
                If temperatureSourceIsNormal AndAlso Not temperatureAlarmRequiresUnavailableSensors Then
                    temperatureAlarmLatched = False
                End If
            End If

            Dim smokeSourceIsNormal As Boolean = True
            If smokeAlarmLatched OrElse smokeSignalReadFailed Then
                smokeSourceIsNormal = Await RefreshSmokeInputForResetAsync()
                If resetResult.Succeeded AndAlso smokeSourceIsNormal Then
                    smokeAlarmLatched = False
                    smokeSignalReadFailed = False
                End If
            End If

            If resetResult.Succeeded Then
                resetFailureLatched = False
            Else
                resetFailureLatched = True
            End If

            Dim remainingAlarmReasons As New List(Of String)()
            If ioAlarmLatched Then remainingAlarmReasons.Add("急停/其他IO报警源尚未取得有效的实时正常信号")
            If smokeAlarmLatched Then
                remainingAlarmReasons.Add(If(smokeSourceIsNormal,
                                             "烟感信号已恢复，但设备复位未成功，烟感报警仍保持锁定",
                                             "烟感输入 X1F 仍处于报警电平或无法确认正常"))
            ElseIf smokeSignalReadFailed AndAlso Not smokeSourceIsNormal Then
                remainingAlarmReasons.Add("烟感输入 X1F 通信异常")
            End If
            If temperatureAlarmRequiresUnavailableSensors Then remainingAlarmReasons.Add("温度探头3/4没有当前实时采集数据")
            If temperatureAlarmLatched AndAlso Not temperatureSourceIsNormal Then remainingAlarmReasons.Add("温度信号无效、过期或仍高于阈值")
            If Not resetResult.Succeeded Then remainingAlarmReasons.AddRange(resetResult.Failures)

            RefreshAlarmStatus()
            If Not alarmStatus Then System.Threading.Interlocked.Exchange(alarmCylinderReleaseState, 0)
            Dim completionMessage As String
            If Not resetResult.Succeeded Then
                completionMessage = "复位未完成：" & String.Join("；", resetResult.Failures)
            ElseIf alarmStatus Then
                completionMessage = "测试流程已复位；报警仍保持锁定：" & String.Join("；", remainingAlarmReasons) &
                                    "。PLC触发输入需先回到待机电平，再重新触发。"
            Else
                completionMessage = If(alarmWasLatched,
                                       "设备和报警源已复位，测试已回到待开始状态。",
                                       "设备和当前测试流程已复位，已回到待开始状态。") &
                                    "PLC触发输入需先回到待机电平，再重新触发。"
            End If
            SetTestStatus(completionMessage)
            RecordResetEvent(completionMessage)
            If resetResult.Succeeded AndAlso Not alarmStatus Then
                MessageBox.Show(completionMessage, "复位完成", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                MessageBox.Show(completionMessage, "复位状态", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            resetFailureLatched = True
            RefreshAlarmStatus()
            Dim failureMessage As String = "复位失败：" & ex.Message
            SetTestStatus(failureMessage)
            RecordResetEvent(failureMessage)
            MessageBox.Show(failureMessage, "复位失败", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            resetInProgress = False
            UpdateTestModeVisuals()
        End Try
    End Sub

    Private Async Function RefreshSmokeInputForResetAsync() As System.Threading.Tasks.Task(Of Boolean)
        Dim plcClient As IPlcAutomationClient = plcAutomationClient
        If plcClient Is Nothing OrElse Not plcClient.IsOpen Then Return False

        Try
            Dim smokeSample As Tuple(Of Boolean, DateTime) = Await System.Threading.Tasks.Task.Run(
                Function()
                    Dim inputHigh As Boolean = plcClient.ReadX1F()
                    Return Tuple.Create(inputHigh, DateTime.UtcNow)
                End Function)
            ReportPlcSmokeInput(smokeSample.Item1, smokeSample.Item2)
            Return Not IsSmokeAlarmActive(smokeSample.Item1)
        Catch ex As Exception
            ReportPlcSmokeInputReadFailure("复位校验烟感输入 X1F 失败：" & ex.Message, DateTime.UtcNow)
            Return False
        End Try
    End Function

    Private Function CaptureCurrentResetChannels() As List(Of Integer)
        Dim channels As New HashSet(Of Integer)()
        If currentTestChannels IsNot Nothing Then
            For Each channel As Integer In currentTestChannels
                If channel >= 1 AndAlso channel <= MAX_CHANNEL Then channels.Add(channel)
            Next
        End If
        If activeTrayPlan IsNot Nothing Then
            For Each channel As Integer In activeTrayPlan.OccupiedChannels.Concat(activeTrayPlan.MeasurementChannels)
                If channel >= 1 AndAlso channel <= MAX_CHANNEL Then channels.Add(channel)
            Next
        End If
        For Each channel As Integer In pendingResetChannelsToClear
            If channel >= 1 AndAlso channel <= MAX_CHANNEL Then channels.Add(channel)
        Next
        If retestArray IsNot Nothing Then
            For index As Integer = 0 To Math.Min(MAX_CHANNEL, retestArray.Length) - 1
                If retestArray(index) = 1 Then channels.Add(index + 1)
            Next
        End If
        Return channels.OrderBy(Function(channel) channel).ToList()
    End Function

    Private Sub ResetCurrentTrayContext(channels As IEnumerable(Of Integer))
        Dim channelSnapshot As List(Of Integer) = If(channels, Enumerable.Empty(Of Integer)()).Distinct().ToList()
        ClearIncompleteTestResults(channelSnapshot)
        For Each channel As Integer In channelSnapshot
            If channel >= 1 AndAlso channel <= MAX_CHANNEL Then RecieveData(channel - 1).BatCode = String.Empty
        Next
        If txtTray IsNot Nothing Then txtTray.Clear()
        trayCode = String.Empty
        trayCodeSet = String.Empty
        lastTrayNo = String.Empty
        currentTestChannels = Nothing
        lastTestSelection = Nothing
        pendingResetChannelsToClear.Clear()
        haveNgPoint = False
        autoStartFlag = False
        activeTrayDebugMode = False
        pendingTraySkipNotice = String.Empty
        firstCellNumToTest = 0
        lastCellNumToTest = 0
        totalCellNumToTest = 0
        ResetTestProgressDisplay()
        RefreshChannelOverview()
    End Sub

    Private Sub RecordResetEvent(message As String)
        Dim timestamp As DateTime = DateTime.Now
        Dim logText As String = timestamp.ToString("yyyy-MM-dd HH:mm:ss") & ":" & If(message, String.Empty)
        PrependEventLogLine(timestamp.ToString("HH:mm:ss") & " " & If(message, String.Empty))
        AppLog.Write("Alarm", logText)
        BusinessLogService.WriteAlarm(logText)
    End Sub

    Private Sub btnGetBarcode_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnGetBarcode.Click
        If resetInProgress Then
            MessageBox.Show("设备正在复位，请复位完成后再准备托盘。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        If chkAutoTest.Checked OrElse plcAutomaticModeActive Then
            MessageBox.Show("自动模式会自行读取并核对托盘码。", "提示", MessageBoxButtons.OK)
            Return
        End If
        If isRuning Then
            MessageBox.Show("测试进行中，不能重新获取条码。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim trayCode As String = If(txtTray.Text, String.Empty).Trim()
        If trayCode = String.Empty Then
            MessageBox.Show("托盘码不能为空。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtTray.Focus()
            Return
        End If

        Try
            Dim plan As TrayTestPlan = PrepareTrayForTest(trayCode, chkAutoTest.Checked)
            Dim requestedChannelCount As Integer = If(SelectedChannelNumbers Is Nothing, 0, SelectedChannelNumbers.Distinct().Count())
            If plan.MeasurementChannels.Count = requestedChannelCount Then
                Dim resultLabel As String = If(activeTrayDebugMode, "已生成 ", "已获取 ")
                Dim codeLabel As String = If(activeTrayDebugMode, " 个调试电芯码。", " 个电芯条码。")
                MessageBox.Show(resultLabel & plan.MeasurementChannels.Count.ToString() & codeLabel, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As NoTestableTrayChannelsException
            ReportAutomaticEvent("托盘电芯码查询没有可测通道，测试未启动且未发送R160：" & ex.Message)
            SetTestStatus("托盘没有可测通道，测试未启动")
        Catch ex As Exception
            MessageBox.Show("获取托盘条码失败：" & ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function PrepareTrayForTest(ByVal requestedTrayCode As String,
                                        ByVal automaticMode As Boolean,
                                        Optional ByVal selectedChannels As IEnumerable(Of Integer) = Nothing) As TrayTestPlan
        If resetInProgress Then Throw New InvalidOperationException("设备正在复位，暂不能准备新托盘。")
        Dim normalizedCode As String = If(requestedTrayCode, String.Empty).Trim()
        If Not ValidateTrayCode(normalizedCode) Then Throw New InvalidOperationException("托盘码为空或长度不符合设置。")
        Dim channelsForPreparation As IEnumerable(Of Integer) = selectedChannels
        If channelsForPreparation Is Nothing Then channelsForPreparation = SelectedChannelNumbers
        activeTrayPlan = Nothing
        activeTrayDebugMode = chk_Test.Checked
        pendingTraySkipNotice = String.Empty
        For index As Integer = 0 To MAX_CHANNEL - 1
            RecieveData(index).BatCode = String.Empty
            RecieveData(index).Voltage = 0
            RecieveData(index).Resistance = 0
            RecieveData(index).RawResistanceMilliohm = 0
            RecieveData(index).CalibrationValueMilliohm = 0
            RecieveData(index).FinalResistanceMilliohm = 0
            RecieveData(index).Result = String.Empty
        Next
        RefreshChannelOverview()
        If activeTrayDebugMode Then
            Return CreateDebugTrayPlan(automaticMode, channelsForPreparation)
        End If

        Dim trayCells As DataTable
        Using service As New OcvDataService()
            trayCells = service.GetTrayCellsByTrayCode(normalizedCode)
        End Using
        Dim plan As TrayTestPlan
        Try
            plan = TrayTestPreparation.Prepare(trayCells, channelsForPreparation, automaticMode)
        Catch ex As NoTestableTrayChannelsException
            If Not automaticMode Then StartAutomaticEventLog(DateTime.Now)
            RefreshChannelOverview()
            Throw
        End Try
        Dim requestedChannels As IEnumerable(Of Integer) = If(automaticMode,
                                                               Enumerable.Range(1, MAX_CHANNEL),
                                                               If(channelsForPreparation, Enumerable.Empty(Of Integer)()))
        Dim skippedChannels As List(Of Integer) = requestedChannels.
            Distinct().
            OrderBy(Function(channel) channel).
            Where(Function(channel) Not plan.MeasurementChannels.Contains(channel)).
            ToList()
        If skippedChannels.Count > 0 Then
            pendingTraySkipNotice = String.Format("托盘{0}有{1}个通道缺少电芯码，已跳过：{2}",
                                                  normalizedCode,
                                                  skippedChannels.Count,
                                                  String.Join(",", skippedChannels.Select(Function(channel) "CH" & channel.ToString())))
            If Not automaticMode Then StartAutomaticEventLog(DateTime.Now)
            ReportAutomaticEvent(pendingTraySkipNotice)
        End If
        For Each item As KeyValuePair(Of Integer, String) In plan.BatteryCodes
            RecieveData(item.Key - 1).BatCode = item.Value
        Next
        activeTrayPlan = plan
        RefreshChannelOverview()
        Return plan
    End Function

    Private Function CreateDebugTrayPlan(ByVal automaticMode As Boolean,
                                         Optional ByVal selectedChannels As IEnumerable(Of Integer) = Nothing) As TrayTestPlan
        Dim measurementChannels As List(Of Integer)
        If automaticMode Then
            measurementChannels = Enumerable.Range(1, MAX_CHANNEL).ToList()
        Else
            Dim channelsForPlan As IEnumerable(Of Integer) = selectedChannels
            If channelsForPlan Is Nothing Then channelsForPlan = SelectedChannelNumbers
            measurementChannels = If(channelsForPlan, Enumerable.Empty(Of Integer)()).
                Distinct().
                OrderBy(Function(channel) channel).
                ToList()
        End If
        If measurementChannels.Count = 0 Then Throw New InvalidOperationException("请先选择至少一个待测试通道。")

        Dim codes As IDictionary(Of Integer, String) = DebugBarcodePolicy.CreateCodes(measurementChannels, DateTime.Now)
        For Each item As KeyValuePair(Of Integer, String) In codes
            RecieveData(item.Key - 1).BatCode = item.Value
        Next

        Dim planChannels As List(Of Integer) = codes.Keys.OrderBy(Function(channel) channel).ToList()
        activeTrayPlan = New TrayTestPlan(planChannels, planChannels, codes)
        RefreshChannelOverview()
        SetTestStatus("调试模式：已生成 " & planChannels.Count.ToString() & " 个临时电芯码")
        Return activeTrayPlan
    End Function

    Private Sub DebugTestModeCheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles chk_Test.CheckedChanged
        If chk_Test Is Nothing Then Return
        Dim coordinatorState As PlcAutomaticTestState = If(plcAutomaticCoordinator Is Nothing,
                                                            PlcAutomaticTestState.Idle,
                                                            plcAutomaticCoordinator.State)
        Dim cycleOccupied As Boolean = plcAutomaticCoordinator IsNot Nothing AndAlso plcAutomaticCoordinator.IsCycleOccupied
        If isRuning OrElse automaticTestPassInProgress OrElse
           (cycleOccupied AndAlso coordinatorState <> PlcAutomaticTestState.WaitingTrayPreparationRecovery) OrElse
           (coordinatorState <> PlcAutomaticTestState.Idle AndAlso
            coordinatorState <> PlcAutomaticTestState.Fault AndAlso
            coordinatorState <> PlcAutomaticTestState.WaitingTrayPreparationRecovery) Then
            chk_Test.Checked = activeTrayDebugMode
            SetTestStatus("测试周期进行中，不能切换调试模式")
            Return
        End If

        activeTrayDebugMode = chk_Test.Checked
        activeTrayPlan = Nothing
        For index As Integer = 0 To MAX_CHANNEL - 1
            RecieveData(index).BatCode = String.Empty
        Next
        RefreshChannelOverview()
        SetTestStatus(If(activeTrayDebugMode, "调试模式已开启，启动时生成临时电芯码。", "调试模式已关闭，请重新获取真实电芯码。"))
        If activeTrayDebugMode AndAlso coordinatorState = PlcAutomaticTestState.WaitingTrayPreparationRecovery Then
            plcAutomaticCoordinator.RequestTrayPreparationRetry()
            SetTestStatus("调试模式已开启，正在使用当前托盘码重试准备流程。")
        End If
        UpdateTestModeVisuals()
    End Sub

    Private Sub txtLastNum_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtLastNum.KeyDown
        '        Dim intF, intL As Long
        '        Dim I As Short
        '        Dim intBarnum As Short
        '        Dim strFixF As String = ""
        '        Dim strFixL As String = ""

        '        Me.Cursor = Cursors.WaitCursor
        '        If e.KeyCode = 13 Then
        '            If txtFirstNum.Text = "" Or txtLastNum.Text = "" Then
        '                MsgBox("请输入首尾电池编号！")
        '                GoTo exit_sub
        '            End If
        '            intF = GetBarCodeNum(Trim(txtFirstNum.Text), strFixF)
        '            intL = GetBarCodeNum(Trim(txtLastNum.Text), strFixL)
        '            If Not strFixF.Equals(strFixL) Then '首尾电池条形码固定部分不一致
        '                MsgBox("请检查首尾电池条形码！")
        '                GoTo exit_sub
        '            End If
        '            intBarnum = intL - intF
        '            If intBarnum >= 128 Then
        '                MsgBox("未更新电池条形码，请检查首尾编码！")
        '                GoTo exit_sub
        '            Else
        '                lblBarNum.Text = intBarnum + 1
        '                For I = 0 To intBarnum
        '                    RecieveData(I).BatCode = strFixF & (intF + I).ToString
        '                Next
        '            End If
        '            SaveBarcode()
        '            MsgBox("已经成功更新电池条形码。")
        '        End If
        'exit_sub:
        '        Me.Cursor = Cursors.Default
    End Sub
    Private Sub txtTray_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs)
        If e.KeyCode = 13 Then
            txtFirstNum.Focus()
            txtFirstNum.SelectionStart = 0
            txtFirstNum.SelectionLength = Len(txtFirstNum.Text)
        End If
    End Sub
    Private Sub timer3_Elapsed(sender As Object, e As ElapsedEventArgs) Handles timer3.Elapsed
        '新系统不再轮询控制板的 JIG/IO 状态；线路温度由 temperatureTimer 单独采集。
    End Sub

    Private Sub temperatureTimer_Elapsed(sender As Object, e As ElapsedEventArgs) Handles temperatureTimer.Elapsed
        If System.Threading.Interlocked.Exchange(temperaturePollBusy, 1) = 1 Then Return
        Try
            If shutdownPrepared OrElse ApplicationShuttingDown Then Return
            Try
                If temperatureModuleClient Is Nothing Then
                    temperatureModuleClient = New NtcTemperatureModuleClient(Initsetting.TemperatureComPort)
                End If
                Dim readings As NtcTemperatureReading() = temperatureModuleClient.ReadTemperatures()
                UpdateTemperatureReadings(readings)
                If lastTemperatureModuleError.Length > 0 Then
                    AppLog.Write("Temperature", "NTC temperature module recovered.")
                    lastTemperatureModuleError = String.Empty
                End If
            Catch ex As NtcTemperatureUnitException
                UpdateTemperatureUnitError()
                If Not String.Equals(lastTemperatureModuleError, ex.Message, StringComparison.Ordinal) Then
                    AppLog.Write("Temperature", "NTC temperature module unit configuration error: " & ex.Message)
                    lastTemperatureModuleError = ex.Message
                End If
            Catch ex As Exception
                If temperatureModuleClient IsNot Nothing Then
                    temperatureModuleClient.Dispose()
                    temperatureModuleClient = Nothing
                End If
                UpdateTemperatureCommunicationError()
                If Not String.Equals(lastTemperatureModuleError, ex.Message, StringComparison.Ordinal) Then
                    AppLog.Write("Temperature", "NTC temperature module read failed: " & ex.Message)
                    lastTemperatureModuleError = ex.Message
                End If
            End Try
        Finally
            System.Threading.Interlocked.Exchange(temperaturePollBusy, 0)
        End Try
    End Sub

    Private Sub UpdateTemperatureReadings(readings As NtcTemperatureReading())
        If readings Is Nothing OrElse readings.Length <> 2 Then
            lastNtcReadings = Nothing
            lastNtcReadingsAtUtc = DateTime.MinValue
            UpdateTemperatureCommunicationError()
            Return
        End If
        lastNtcReadings = CType(readings.Clone(), NtcTemperatureReading())
        lastNtcReadingsAtUtc = DateTime.UtcNow
        Updatet1Text(FormatTemperatureReading(readings(0)))
        Updatet2Text(FormatTemperatureReading(readings(1)))
        hjwd = If(readings(0).IsValid,
                  readings(0).TemperatureCelsius.ToString("0.0", System.Globalization.CultureInfo.CurrentCulture),
                  String.Empty)
        trayCode = txtTray.Text
    End Sub

    Private Shared Function FormatTemperatureReading(reading As NtcTemperatureReading) As String
        Return If(reading.IsValid,
                  reading.TemperatureCelsius.ToString("0.0", System.Globalization.CultureInfo.CurrentCulture),
                  "探头故障")
    End Function

    Private Sub UpdateTemperatureCommunicationError()
        lastNtcReadings = Nothing
        lastNtcReadingsAtUtc = DateTime.MinValue
        Updatet1Text("通信故障")
        Updatet2Text("通信故障")
    End Sub

    Private Sub UpdateTemperatureUnitError()
        lastNtcReadings = Nothing
        lastNtcReadingsAtUtc = DateTime.MinValue
        Updatet1Text("请设摄氏度")
        Updatet2Text("请设摄氏度")
    End Sub

    Private Async Function RefreshTemperatureForResetAsync() As System.Threading.Tasks.Task(Of Boolean)
        Dim waitDeadline As DateTime = DateTime.UtcNow.AddSeconds(3)
        While System.Threading.Interlocked.CompareExchange(temperaturePollBusy, 1, 0) <> 0
            If DateTime.UtcNow >= waitDeadline Then Return False
            Await System.Threading.Tasks.Task.Delay(50)
        End While

        Try
            Dim readings As NtcTemperatureReading() = Await System.Threading.Tasks.Task.Run(
                Function()
                    If temperatureModuleClient Is Nothing Then
                        temperatureModuleClient = New NtcTemperatureModuleClient(Initsetting.TemperatureComPort)
                    End If
                    Return temperatureModuleClient.ReadTemperatures()
                End Function)
            UpdateTemperatureReadings(readings)
            Return IsTemperatureSourceNormal()
        Catch ex As Exception
            If temperatureModuleClient IsNot Nothing Then
                temperatureModuleClient.Dispose()
                temperatureModuleClient = Nothing
            End If
            UpdateTemperatureCommunicationError()
            AppLog.Write("Temperature", "Reset verification read failed: " & ex.Message)
            Return False
        Finally
            System.Threading.Interlocked.Exchange(temperaturePollBusy, 0)
        End Try
    End Function

    Private Function IsTemperatureSourceNormal() As Boolean
        If temperatureAlarmRequiresUnavailableSensors OrElse
           lastNtcReadings Is Nothing OrElse
           lastNtcReadings.Length <> 2 OrElse
           DateTime.UtcNow.Subtract(lastNtcReadingsAtUtc) > TimeSpan.FromSeconds(10) Then Return False

        For Each reading As NtcTemperatureReading In lastNtcReadings
            If Not reading.IsValid OrElse reading.TemperatureCelsius > CDec(Initsetting.overTemperateValue) Then Return False
        Next
        Return True
    End Function

    ''' <summary>
    ''' jig信号监控定时器
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        '启动监测温度和JIG信号
        'Call checkJIGSignal()
    End Sub
    Private Sub checkJIGSignal()
        Dim ch As Integer = cmb_ChSelect.SelectedItem
        Dim temp As Byte()
        Dim bytes(500) As Byte
        mSock = frmDebug.mSock

        '获得IO等信息
        Try
            temp = mSock.GetSignalTrayTempertureAndIOStatus()
            bytes = socketSR(temp)
        Catch ex As Exception

        End Try


        Dim temp1 As Byte()
        Dim bytes1(500) As Byte
        Try
            '获取自动启动信号
            If bytes(4) = 0 Then
                autoStartFlag = False
            Else
                autoStartFlag = True
            End If
            ' JIG第一个字节
            For bitIndex As Integer = 0 To 7
                ' 获取第 bitIndex 位的值
                Dim bitValue As Integer = (bytes(5) >> bitIndex) And 1
                '检查急停位的值，如果位1，报警，颜色为红色，闪烁
                If bitIndex = 2 Then
                    If bitValue = 0 Then
                        LabelAlarm.Visible = True
                        LabelRun.Visible = False
                        '设置报警状态
                        ioAlarmLatched = True
                        RefreshAlarmStatus()
                        RequestStopForAlarm("急停报警，请检查急停回路")
                        '关闭定时器
                        timer3.Close()
                        ' 显示一个警告消息框
                        MessageBox.Show("急停报警，请立刻停机检查", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        temp1 = mSock.GetSignalFixtureUp '松开夹具（打开托盘）
                        bytes1 = socketSR(temp1)
                        Dim timestamp As String = DateTime.Now.ToString()
                        ' 添加时间戳
                        Dim alarmLog As String = $"{timestamp}:急停报警 "   ' 添加时间戳和报警信息
                        Try
                            Dim filePath As String = "AlarmLog.txt"
                            ' 使用 StreamWriter 写入文件
                            Using writer As New System.IO.StreamWriter(filePath, True)
                                writer.WriteLine(DateTime.Now.ToString() & ": " & alarmLog)
                            End Using
                        Catch ex As Exception

                        End Try
                        BusinessLogService.WriteAlarm(alarmLog)
                        '' 调用 Application.Exit() 方法退出整个应用程序
                        'Application.Exit()
                    Else
                        LabelAlarm.Visible = False
                        LabelRun.Visible = True
                    End If
                End If
            Next
            ' JIG第二个字节
            For bitIndex As Integer = 0 To 7
                ' 获取第 bitIndex 位的值
                Dim bitValue As Integer = (bytes(6) >> bitIndex) And 1

                If bitIndex = 3 Then
                    '烟感1
                    If bitValue = 0 Then
                        smokeAlarm1.BackColor = Color.Red
                        '关闭定时器
                        timer3.Close()
                        ' 显示一个警告消息框
                        MessageBox.Show("烟感1报警，请立刻停机检查", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        temp1 = mSock.GetSignalFixtureUp '松开夹具（打开托盘）
                        bytes1 = socketSR(temp1)
                        Dim timestamp As String = DateTime.Now.ToString()    ' 添加时间戳
                        Dim alarmLog As String = $"{timestamp}:烟感报警 " & "烟感 1 位置"  ' 添加时间戳和报警信息
                        '设置报警状态
                        ioAlarmLatched = True
                        RefreshAlarmStatus()
                        RequestStopForAlarm("烟感1报警，请检查烟感回路")
                        Try
                            Dim filePath As String = "AlarmLog.txt"
                            ' 使用 StreamWriter 写入文件
                            Using writer As New System.IO.StreamWriter(filePath, True)
                                writer.WriteLine(DateTime.Now.ToString() & ": " & alarmLog)
                            End Using
                        Catch ex As Exception

                        End Try
                        BusinessLogService.WriteAlarm(alarmLog)
                        Timer2.Start()
                        '' 调用 Application.Exit() 方法退出整个应用程序
                        'Application.Exit()
                    Else
                        smokeAlarm1.BackColor = Color.Green
                    End If
                End If
                If bitIndex = 4 Then
                    '烟感2
                    If bitValue = 0 Then
                        smokeAlarm2.BackColor = Color.Red
                        '关闭定时器
                        timer3.Close()
                        ' 显示一个警告消息框
                        MessageBox.Show("烟感2报警，请立刻停机检查", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        temp1 = mSock.GetSignalFixtureUp '松开夹具（打开托盘）
                        bytes1 = socketSR(temp1)
                        Dim timestamp As String = DateTime.Now.ToString()    ' 添加时间戳
                        Dim alarmLog As String = $"{timestamp}:烟感报警 " & "烟感 2位置"  ' 添加时间戳和报警信息
                        '设置报警状态
                        ioAlarmLatched = True
                        RefreshAlarmStatus()
                        RequestStopForAlarm("烟感2报警，请检查烟感回路")
                        Try
                            Dim filePath As String = "AlarmLog.txt"
                            ' 使用 StreamWriter 写入文件
                            Using writer As New System.IO.StreamWriter(filePath, True)
                                writer.WriteLine(DateTime.Now.ToString() & ": " & alarmLog)
                            End Using
                        Catch ex As Exception

                        End Try
                        BusinessLogService.WriteAlarm(alarmLog)
                        Timer2.Start()
                        '' 调用 Application.Exit() 方法退出整个应用程序
                        'Application.Exit()
                    Else
                        smokeAlarm2.BackColor = Color.Green
                    End If
                End If
            Next
            If smokeAlarm2.BackColor = Color.Green And smokeAlarm2.BackColor = Color.Green Then
                Timer2.Stop()
            End If
            ' 创建一个整数类型的数组ARR2int，长度为4
            Dim TemperateList(3) As Integer
            ' 使用Array.Copy方法来复制字节数组的一部分,后8个字节为温度数据
            ' 参数分别是源数组，源数组的起始索引，目标数组，目标数组的起始索引，以及要复制的元素数量
            Array.Copy(bytes, bytes.Length - 8, TemperateList, 0, 4)
        Catch ex As Exception
            ' Handle any unexpected exceptions gracefully
            MessageBox.Show("An unexpected error occurred: " & ex.Message)
        End Try
    End Sub
    Private Sub UpdateLabelOnUIThread(text As String)
        If LabelAlarm.InvokeRequired Then
            LabelAlarm.Invoke(New Action(Of String)(AddressOf UpdateLabelOnUIThread), text)
        Else
            LabelAlarm.Text = text
        End If
    End Sub
    ' 创建一个用于更新 UI 控件的方法
    Private Sub Updatet2Text(ByVal newText As String)
        Try
            ' 检查当前线程是否是创建 LabelAlarm 控件的线程
            If t2.InvokeRequired Then
                ' 如果不是，通过调用 Invoke 方法在创建线程上执行更新操作
                t2.Invoke(Sub() t2.Text = newText)
            Else
                ' 如果是创建线程上的操作，可以直接更新 UI 控件
                t2.Text = newText
            End If
        Catch ex As Exception

        End Try
    End Sub
    Private Sub Updatet3Text(ByVal newText As String)
        Try
            ' 检查当前线程是否是创建 LabelAlarm 控件的线程
            If t3.InvokeRequired Then
                ' 如果不是，通过调用 Invoke 方法在创建线程上执行更新操作
                t3.Invoke(Sub() t3.Text = newText)
            Else
                ' 如果是创建线程上的操作，可以直接更新 UI 控件
                t3.Text = newText
            End If
        Catch ex As Exception

        End Try
    End Sub
    Private Sub Updatet4Text(ByVal newText As String)
        Try
            ' 检查当前线程是否是创建 LabelAlarm 控件的线程
            If t4.InvokeRequired Then
                ' 如果不是，通过调用 Invoke 方法在创建线程上执行更新操作
                t4.Invoke(Sub() t4.Text = newText)
            Else
                ' 如果是创建线程上的操作，可以直接更新 UI 控件
                t4.Text = newText
            End If
        Catch ex As Exception

        End Try
    End Sub
    Private Sub Updatet1Text(ByVal newText As String)
        Try
            ' 检查当前线程是否是创建 LabelAlarm 控件的线程
            If t1.InvokeRequired Then
                ' 如果不是，通过调用 Invoke 方法在创建线程上执行更新操作
                t1.Invoke(Sub() t1.Text = newText)
            Else
                ' 如果是创建线程上的操作，可以直接更新 UI 控件
                t1.Text = newText
            End If
        Catch ex As Exception

        End Try
    End Sub
    Private Sub UpdateControlVisibility(ByVal control As Control, ByVal isVisible As Boolean)
        Try
            If control.InvokeRequired Then
                ' 如果不是在创建线程上，使用 Invoke 方法执行操作
                control.Invoke(Sub() control.Visible = isVisible)
            Else
                ' 如果在创建线程上，可以直接更新属性
                control.Visible = isVisible
            End If
        Catch ex As Exception

        End Try
    End Sub
    Private Sub UpdateControlBackColor(ByVal control As Control, ByVal newBackColor As Color)
        Try
            If control.InvokeRequired Then
                ' 如果不是在创建线程上，使用 Invoke 方法执行操作
                control.Invoke(Sub() control.BackColor = newBackColor)
            Else
                ' 如果在创建线程上，可以直接更新属性
                control.BackColor = newBackColor
            End If
        Catch ex As Exception

        End Try
    End Sub
    Public Sub AnalyzeData()
        Dim bytes As Byte()
        Dim bytes1(500) As Byte
        Dim mSock As SocketCtrolCmd
        mSock = frmDebug.mSock
        bytes = dataOfTemPeraturesAndIO
        If bytes Is Nothing Then
            Exit Sub
        End If
        '获取自动启动信号
        If bytes.Length = 0 Then
            Exit Sub
        End If
        If bytes(4) = 0 Then
            autoStartFlag = False
        Else
            autoStartFlag = True
        End If
        '' JIG第一个字节
        'For bitIndex As Integer = 0 To 7
        '    ' 获取第 bitIndex 位的值
        '    Dim bitValue As Integer = (bytes(5) >> bitIndex) And 1
        '    '检查急停位的值，如果位0，报警，颜色为红色，闪烁
        '    If bitIndex = 2 Then
        '        If bitValue = 0 Then
        '            If LabelAlarm.InvokeRequired Then
        '                LabelAlarm.Invoke(Sub()
        '                                      ' 在控件的创建线程上执行操作
        '                                      LabelAlarm.Visible = True
        '                                  End Sub)
        '            Else
        '                ' 控件已经在正确的线程上，可以直接执行操作
        '                LabelAlarm.Visible = True
        '            End If
        '            If LabelRun.InvokeRequired Then
        '                LabelRun.Invoke(Sub()
        '                                    ' 在控件的创建线程上执行操作
        '                                    LabelRun.Visible = False
        '                                End Sub)
        '            Else
        '                ' 控件已经在正确的线程上，可以直接执行操作
        '                LabelRun.Visible = False
        '            End If
        '            ' 显示一个警告消息框
        '            MessageBox.Show("急停报警，请立刻停机检查", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        '            'temp1 = mSock.GetSignalFixtureUp '松开夹具（打开托盘）
        '            'bytes1 = socketSR(temp1)
        '            Dim timestamp As String = DateTime.Now.ToString()
        '            ' 添加时间戳
        '            Dim alarmLog As String = $"{timestamp}:急停报警 "   ' 添加时间戳和报警信息
        '            If txtAlarm.InvokeRequired Then
        '                txtAlarm.Invoke(Sub()
        '                                    ' 在控件的创建线程上执行操作
        '                                    txtAlarm.AppendText(alarmLog & Environment.NewLine)
        '                                End Sub)
        '            Else
        '                ' 控件已经在正确的线程上，可以直接执行操作
        '                txtAlarm.AppendText(alarmLog & Environment.NewLine)
        '            End If

        '            Try
        '                Dim filePath As String = "AlarmLog.txt"
        '                ' 使用 StreamWriter 写入文件
        '                Using writer As New System.IO.StreamWriter(filePath, True)
        '                    writer.WriteLine(DateTime.Now.ToString() & ": " & alarmLog)
        '                End Using
        '            Catch ex As Exception

        '            End Try
        '            '这里应该关闭定时器3吧
        '            timer3.Close()
        '            '' 调用 Application.Exit() 方法退出整个应用程序
        '            'Application.Exit()
        '        Else
        '            If LabelAlarm.InvokeRequired Then
        '                LabelAlarm.Invoke(Sub()
        '                                      ' 在控件的创建线程上执行操作
        '                                      LabelAlarm.Visible = False
        '                                  End Sub)
        '            Else
        '                ' 控件已经在正确的线程上，可以直接执行操作
        '                LabelAlarm.Visible = False
        '            End If
        '            If LabelRun.InvokeRequired Then
        '                LabelRun.Invoke(Sub()
        '                                    ' 在控件的创建线程上执行操作
        '                                    LabelRun.Visible = True
        '                                End Sub)
        '            Else
        '                ' 控件已经在正确的线程上，可以直接执行操作
        '                LabelRun.Visible = True
        '            End If

        '        End If
        '    End If
        'Next

        '' JIG第二个字节
        'For bitIndex As Integer = 0 To 7
        '    ' 获取第 bitIndex 位的值
        '    Dim bitValue As Integer = (bytes(6) >> bitIndex) And 1

        '    If bitIndex = 3 Then
        '        '烟感1
        '        If bitValue = 0 Then
        '            If smokeAlarm1.InvokeRequired Then
        '                smokeAlarm1.Invoke(Sub()
        '                                       ' 设置控件的背景颜色为红色
        '                                       UpdateControlBackColor(smokeAlarm1, Color.Red)
        '                                   End Sub)
        '            Else
        '                ' 设置控件的背景颜色为红色
        '                UpdateControlBackColor(smokeAlarm1, Color.Red)
        '            End If

        '            ' 设置控件的背景颜色为红色
        '            UpdateControlBackColor(smokeAlarm1, Color.Red)
        '            ' 显示一个警告消息框
        '            MessageBox.Show("烟感1报警，请立刻停机检查", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        '            'temp1 = mSock.GetSignalFixtureUp '松开夹具（打开托盘）
        '            'bytes1 = socketSR(temp1)
        '            Dim timestamp As String = DateTime.Now.ToString()    ' 添加时间戳
        '            Dim alarmLog As String = $"{timestamp}:烟感报警 " & "烟感 1 位置"  ' 添加时间戳和报警信息
        '            If txtAlarm.InvokeRequired Then
        '                txtAlarm.Invoke(Sub()
        '                                    txtAlarm.AppendText(alarmLog & Environment.NewLine)
        '                                End Sub)
        '            Else
        '                txtAlarm.AppendText(alarmLog & Environment.NewLine)
        '            End If

        '            Try
        '                Dim filePath As String = "AlarmLog.txt"
        '                ' 使用 StreamWriter 写入文件
        '                Using writer As New System.IO.StreamWriter(filePath, True)
        '                    writer.WriteLine(DateTime.Now.ToString() & ": " & alarmLog)
        '                End Using
        '            Catch ex As Exception

        '            End Try
        '            Timer2.Start()
        '            timer3.Close()
        '            '' 调用 Application.Exit() 方法退出整个应用程序
        '            'Application.Exit()
        '        Else
        '            If smokeAlarm1.InvokeRequired Then
        '                smokeAlarm1.Invoke(Sub()
        '                                       ' 设置控件的背景颜色为绿色
        '                                       UpdateControlBackColor(smokeAlarm1, Color.Green)
        '                                   End Sub)
        '            Else
        '                ' 设置控件的背景颜色为绿色
        '                UpdateControlBackColor(smokeAlarm1, Color.Green)
        '            End If
        '        End If
        '    End If

        '    If bitIndex = 4 Then
        '        '烟感2
        '        If bitValue = 0 Then
        '            ' 设置控件的背景颜色为红色
        '            If smokeAlarm2.InvokeRequired Then
        '                smokeAlarm2.Invoke(Sub()
        '                                       ' 设置控件的背景颜色为红色
        '                                       UpdateControlBackColor(smokeAlarm2, Color.Red)
        '                                   End Sub)
        '            Else
        '                ' 设置控件的背景颜色为红色
        '                UpdateControlBackColor(smokeAlarm2, Color.Red)
        '            End If
        '            ' 显示一个警告消息框
        '            MessageBox.Show("烟感2报警，请立刻停机检查", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        '            'temp1 = mSock.GetSignalFixtureUp '松开夹具（打开托盘）
        '            'bytes1 = socketSR(temp1)
        '            Dim timestamp As String = DateTime.Now.ToString()    ' 添加时间戳
        '            Dim alarmLog As String = $"{timestamp}:烟感报警 " & "烟感 2位置"  ' 添加时间戳和报警信息
        '            If txtAlarm.InvokeRequired Then
        '                txtAlarm.Invoke(Sub()
        '                                    txtAlarm.AppendText(alarmLog & Environment.NewLine)
        '                                End Sub)
        '            Else
        '                txtAlarm.AppendText(alarmLog & Environment.NewLine)
        '            End If
        '            Try
        '                Dim filePath As String = "AlarmLog.txt"
        '                ' 使用 StreamWriter 写入文件
        '                Using writer As New System.IO.StreamWriter(filePath, True)
        '                    writer.WriteLine(DateTime.Now.ToString() & ": " & alarmLog)
        '                End Using
        '            Catch ex As Exception

        '            End Try
        '            Timer2.Start()
        '            timer3.Close()
        '            '' 调用 Application.Exit() 方法退出整个应用程序
        '            'Application.Exit()
        '        Else
        '            ' 设置控件的背景颜色为红色
        '            UpdateControlBackColor(smokeAlarm2, Color.Green)
        '            If smokeAlarm2.InvokeRequired Then
        '                smokeAlarm2.Invoke(Sub()
        '                                       ' 设置控件的背景颜色为绿色
        '                                       UpdateControlBackColor(smokeAlarm2, Color.Green)
        '                                   End Sub)
        '            Else
        '                ' 设置控件的背景颜色为绿色
        '                UpdateControlBackColor(smokeAlarm2, Color.Green)
        '            End If
        '        End If

        '    End If

        'Next
        'If smokeAlarm2.BackColor = Color.Green And smokeAlarm2.BackColor = Color.Green Then
        '    Timer2.Stop()
        'End If
        '温度由 NtcTemperatureModuleClient 通过独立的 Modbus RTU 串口采集。
    End Sub
    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        ' 交替更改Label的可见性属性
        If smokeAlarm1.Visible And smokeAlarm1.BackColor = Color.Red Then
            smokeAlarm1.Visible = False
        Else
            smokeAlarm1.Visible = True
        End If

        If smokeAlarm2.Visible And smokeAlarm2.BackColor = Color.Red Then
            smokeAlarm2.Visible = False
        Else
            smokeAlarm2.Visible = True
        End If
    End Sub

    Private Sub frmTest_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        If testProgressDialog IsNot Nothing AndAlso Not testProgressDialog.IsDisposed Then
            testProgressDialog.CloseForOwner()
        End If
        PrepareForShutdown()
        '复位报警状态
        alarmStatus = False
        'clientSocket.Close()
        '保存设置值文件
        Call SaveInitFile()
        ocvStationReady = False
        ' 在窗口关闭时释放 Socket 资源
        If mSocket1 IsNot Nothing Then
            If mSocket1.Connected Then
                mSocket1.Shutdown(SocketShutdown.Both)
            End If
            mSocket1.Close()
            mSocket1.Dispose()
        End If
        ' 在窗口关闭时释放 Timer 资源
        If timer3 IsNot Nothing Then
            timer3.Stop() ' 停止计时器
            timer3.Dispose() ' 释放资源
        End If
        Timer2.Enabled = False
        Timer1.Enabled = False
    End Sub

    Private Sub StartPlcAutomaticTest()
        If plcAutomaticCoordinator IsNot Nothing Then Return
        If Initsetting.PLCMode <> 0 Then
            ReportAutomaticStatus("PLC自动ASCII流程要求使用PLC串口模式。")
            ReportAutomaticEvent("自动流程未启动：需要使用PLC串口模式")
            Return
        End If
        If chkAutoTest Is Nothing OrElse Not chkAutoTest.Checked Then Return
        Try
            If TypeOf plcAutomationClient Is IDisposable Then DirectCast(plcAutomationClient, IDisposable).Dispose()
            plcAutomationClient = Nothing
            '自动 ASCII 流程独占 PLC 串口；若旧控制器曾被调试页打开，先释放它。
            If plcController IsNot Nothing Then plcController.Disconnect()
            plcController = Nothing
            plcConnected = False

            plcAutomationClient = New PlcAutomationClient(Initsetting)
            Dim sharedScannerPort As System.IO.Ports.SerialPort = Nothing
            If frmMDI IsNot Nothing AndAlso Not frmMDI.IsDisposed Then sharedScannerPort = frmMDI.Com_scaner
            If sharedScannerPort IsNot Nothing AndAlso sharedScannerPort.IsOpen Then sharedScannerPort.Close()
            trayScannerClient = New TrayScannerClient(Initsetting, sharedScannerPort)
            plcAutomaticCoordinator = New PlcAutomaticTestCoordinator(plcAutomationClient, trayScannerClient, Me, CreatePlcRuntimeStatusReporter())
            plcAutomaticModeActive = True
            plcAutomaticCoordinator.Start()
        Catch ex As Exception
            plcAutomaticModeActive = False
            ReportAutomaticStatus("PLC自动流程启动失败：" & ex.Message)
            ReportAutomaticEvent("自动流程启动失败：" & ex.Message)
            If plcAutomaticCoordinator IsNot Nothing Then
                Try
                    plcAutomaticCoordinator.Stop()
                Catch
                End Try
                plcAutomaticCoordinator = Nothing
            End If
            If TypeOf plcAutomationClient Is IDisposable Then DirectCast(plcAutomationClient, IDisposable).Dispose()
            If TypeOf trayScannerClient Is IDisposable Then DirectCast(trayScannerClient, IDisposable).Dispose()
            plcAutomationClient = Nothing
            trayScannerClient = Nothing
        End Try
    End Sub

    Private Sub StopPlcTestCoordinator()
        Dim statusGeneration As Integer = Threading.Interlocked.Increment(plcRuntimeStatusGeneration)
        plcAutomaticModeActive = False
        If plcAutomaticCoordinator IsNot Nothing Then
            Try
                plcAutomaticCoordinator.Stop()
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("停止 PLC 自动流程失败: " & ex.Message)
            End Try
            plcAutomaticCoordinator = Nothing
        End If
        If TypeOf plcAutomationClient Is IDisposable Then DirectCast(plcAutomationClient, IDisposable).Dispose()
        If TypeOf trayScannerClient Is IDisposable Then DirectCast(trayScannerClient, IDisposable).Dispose()
        plcAutomationClient = Nothing
        trayScannerClient = Nothing
        ReportPlcRuntimeStatus("未连接", statusGeneration)
    End Sub

    Friend ReadOnly Property CanApplyInitialSettings As Boolean
        Get
            If shutdownPrepared OrElse isRuning OrElse automaticTestPassInProgress Then Return False
            If plcAutomaticCoordinator Is Nothing Then Return True
            If plcAutomaticCoordinator.IsCycleOccupied Then Return False
            If plcAutomaticCoordinator.State <> PlcAutomaticTestState.Idle AndAlso
               plcAutomaticCoordinator.State <> PlcAutomaticTestState.Fault Then Return False
            Return Not plcAutomaticModeActive OrElse plcAutomaticCoordinator.IsPausedAtCycleBoundary
        End Get
    End Property

    Friend Sub RequestSettingsApplyAtCycleBoundary(ByVal pausedCallback As Action)
        If plcAutomaticCoordinator Is Nothing OrElse Not plcAutomaticModeActive Then Return
        plcAutomaticCoordinator.RequestPauseAtCycleBoundary(pausedCallback)
    End Sub

    Friend Sub ResumeAutomaticFlowAfterSettingsApply()
        If plcAutomaticCoordinator IsNot Nothing Then plcAutomaticCoordinator.ResumeAfterPause()
    End Sub

    Friend Function ApplyConfiguredInitialSettings(ByVal previousSettings As Initset,
                                                   ByVal currentSettings As Initset,
                                                   ByVal changes As InitSettingsChangeKind) As Boolean
        If InvokeRequired Then Throw New InvalidOperationException("初始化设置必须在测试窗体 UI 线程应用。")
        If InitSettingsApplyPolicy.RequiresSafeBoundary(changes) AndAlso Not CanApplyInitialSettings Then
            Throw New InvalidOperationException("测试流程尚未到达安全的设置应用时机。")
        End If

        Dim wasAutomaticMode As Boolean = (chkAutoTest IsNot Nothing AndAlso chkAutoTest.Checked)
        Dim refreshTemperatureClient As Boolean = InitSettingsApplyPolicy.RequiresTemperatureClientReset(previousSettings, currentSettings)
        Dim restartAutomaticClients As Boolean = wasAutomaticMode AndAlso
                                                 (InitSettingsApplyPolicy.RequiresPlcClientRestart(previousSettings, currentSettings) OrElse
                                                  InitSettingsApplyPolicy.RequiresScannerClientRestart(previousSettings, currentSettings))
        Dim restartManualPlc As Boolean = Not wasAutomaticMode AndAlso
                                         InitSettingsApplyPolicy.RequiresPlcClientRestart(previousSettings, currentSettings)
        Dim shouldTemporarilyPauseAutomaticFlow As Boolean = restartAutomaticClients AndAlso Not initializationApplyInProgress
        If shouldTemporarilyPauseAutomaticFlow Then SetInitializationApplyInProgress(True)

        If refreshTemperatureClient Then
            Dim timerWasEnabled As Boolean = temperatureTimer IsNot Nothing AndAlso temperatureTimer.Enabled
            If temperatureTimer IsNot Nothing Then temperatureTimer.Stop()
            If temperatureModuleClient IsNot Nothing Then
                temperatureModuleClient.Dispose()
                temperatureModuleClient = Nothing
            End If
            lastTemperatureModuleError = String.Empty
            If timerWasEnabled AndAlso Not shutdownPrepared Then temperatureTimer.Start()
        End If

        If (changes And InitSettingsChangeKind.TestRules) <> InitSettingsChangeKind.None Then
            txtMaxV.Text = Initsetting.upVoltage.ToString()
            txtMinV.Text = Initsetting.downVoltage.ToString()
            txtMaxR.Text = Initsetting.upResistance.ToString()
            txtMinR.Text = Initsetting.downResistance.ToString()
        End If

        If (changes And InitSettingsChangeKind.ProbeManagement) <> InitSettingsChangeKind.None Then
            txtTotle.Text = Initsetting.Probelifespan.ToString()
            txtUsed.Text = Initsetting.ProbeUsedCount.ToString()
            txtRemain.Text = (Initsetting.Probelifespan - Initsetting.ProbeUsedCount).ToString()
        End If

        If (changes And InitSettingsChangeKind.Station) <> InitSettingsChangeKind.None Then
            ConfigureStationControls()
        End If

        If (changes And InitSettingsChangeKind.Display) <> InitSettingsChangeKind.None Then
            ApplyConfiguredChannelColors()
        End If

        Dim plcConnectedAfterApply As Boolean = plcConnected
        If restartAutomaticClients Then
            StopPlcTestCoordinator()
            If Initsetting.PLCMode = 1 Then
                plcConnectedAfterApply = (CheckPlc() = 1)
                SetTestStatus("PLC TCP 已重新连接；自动ASCII测试流程需要切换到串口PLC模式。")
            Else
                StartPlcAutomaticTest()
                plcConnectedAfterApply = plcAutomaticModeActive AndAlso plcAutomaticCoordinator IsNot Nothing
            End If
        ElseIf restartManualPlc Then
            StopPlcTestCoordinator()
            If Initsetting.PLCMode = 1 Then
                plcConnectedAfterApply = (CheckPlc() = 1)
            Else
                StartManualPlcConnection()
                plcConnectedAfterApply = plcAutomationClient IsNot Nothing AndAlso plcAutomationClient.IsOpen
            End If
        End If

        If restartAutomaticClients AndAlso Initsetting.PLCMode = 0 AndAlso Not plcConnectedAfterApply Then
            Throw New InvalidOperationException("PLC或扫码器客户端未能按新设置重新启动。")
        End If
        If restartManualPlc AndAlso Not plcConnectedAfterApply Then
            Throw New InvalidOperationException("PLC未能按新设置重新连接。")
        End If

        If shouldTemporarilyPauseAutomaticFlow Then
            SetInitializationApplyInProgress(False)
            ResumeAutomaticFlowAfterSettingsApply()
        End If

        Return plcConnectedAfterApply
    End Function

    Public Sub SetInitializationApplyInProgress(ByVal isApplying As Boolean)
        initializationApplyInProgress = isApplying
        UpdateTestModeVisuals()
    End Sub

    Private Sub StartManualPlcConnection()
        If Initsetting.PLCMode <> 0 Then
            CheckPlc()
            Return
        End If
        If plcController IsNot Nothing Then plcController.Disconnect()
        plcController = Nothing
        plcConnected = False
        If TypeOf plcAutomationClient Is IDisposable Then DirectCast(plcAutomationClient, IDisposable).Dispose()
        plcAutomationClient = New PlcAutomationClient(Initsetting)
        trayScannerClient = Nothing
        plcAutomaticCoordinator = New PlcAutomaticTestCoordinator(plcAutomationClient, Nothing, Me, CreatePlcRuntimeStatusReporter())
        plcAutomaticModeActive = False
        Try
            plcAutomaticCoordinator.StartManual()
        Catch
            StopPlcTestCoordinator()
            Throw
        End Try
        plcConnected = plcAutomationClient.IsOpen
        SetTestStatus("手动 PLC 串口已连接，输入托盘码并选择通道后可启动测试")
    End Sub

    Private Function BuildAutomaticPassResult(ByVal channels As IEnumerable(Of Integer)) As TestPassResult
        Dim passResult As New TestPassResult With {.Completed = True}
        If channels Is Nothing Then Return passResult

        For Each channel As Integer In channels.Distinct().Where(Function(item) item >= 1 AndAlso item <= MAX_CHANNEL).OrderBy(Function(item) item)
            If activeTrayPlan Is Nothing OrElse Not activeTrayPlan.BatteryCodes.ContainsKey(channel) Then Continue For
            Dim data As Bat_info = RecieveData(channel - 1)
            passResult.Results.Add(New ChannelTestResult With {
                .ChannelNumber = channel,
                .Voltage = data.Voltage,
                .Resistance = data.Resistance,
                .IsNg = (IsNG(data.Voltage, data.Resistance) <> 0)
            })
        Next
        Return passResult
    End Function

    Private Sub ExecuteAutomaticOnUi(ByVal action As Action, Optional ByVal waitForCompletion As Boolean = False)
        If action Is Nothing OrElse IsDisposed Then Return
        If InvokeRequired Then
            If waitForCompletion Then
                Invoke(action)
            Else
                BeginInvoke(action)
            End If
        Else
            action()
        End If
    End Sub

    Public Function IsAutomaticModeEnabled() As Boolean Implements IAutomaticTestHost.IsAutomaticModeEnabled
        Dim enabled As Boolean = False
        ExecuteAutomaticOnUi(Sub()
                                 enabled = Not shutdownPrepared AndAlso Not initializationApplyInProgress AndAlso chkAutoTest IsNot Nothing AndAlso
                                           chkAutoTest.Checked AndAlso plcAutomaticModeActive
                             End Sub, True)
        Return enabled
    End Function

    Public Function IsDebugModeEnabled() As Boolean Implements IAutomaticTestHost.IsDebugModeEnabled
        Dim enabled As Boolean = False
        ExecuteAutomaticOnUi(Sub()
                                 enabled = chk_Test IsNot Nothing AndAlso chk_Test.Checked
                             End Sub, True)
        Return enabled
    End Function

    Public Function IsManualModeEnabled() As Boolean Implements IManualTestHost.IsManualModeEnabled
        Dim enabled As Boolean = False
        ExecuteAutomaticOnUi(Sub()
                                 enabled = Not shutdownPrepared AndAlso Not initializationApplyInProgress AndAlso
                                           chkManualTest IsNot Nothing AndAlso chkManualTest.Checked AndAlso
                                           chkAutoTest IsNot Nothing AndAlso Not chkAutoTest.Checked AndAlso
                                           Not plcAutomaticModeActive
                             End Sub, True)
        Return enabled
    End Function

    Public Sub EnsureManualTestReady() Implements IManualTestHost.EnsureManualTestReady
        ExecuteAutomaticOnUi(Sub()
                                 If Not IsManualModeEnabled() Then Throw New InvalidOperationException("主测试页当前不在手动模式。")
                                 If Not AuthorizationService.IsAuthenticated() Then Throw New InvalidOperationException("请先登录后再启动手动测试。")
                                 If alarmStatus Then Throw New InvalidOperationException("目前处于报警状态，请先复位报警。")
                                 If resetInProgress Then Throw New InvalidOperationException("设备正在复位，请稍后再启动测试。")
                                 If isRuning OrElse automaticTestPassInProgress Then Throw New InvalidOperationException("当前已有测量任务正在运行。")
                                 If Initsetting.PLCMode <> 0 Then Throw New InvalidOperationException("当前 PLC TCP 模式没有已定义的 R160 映射；手动周期需要 PLC ASCII 串口模式。")
                                 If plcAutomationClient Is Nothing OrElse Not plcAutomationClient.IsOpen Then Throw New InvalidOperationException("手动 PLC ASCII 串口未连接。")
                                 If CheckCom_SCM() <> 1 Then Throw New InvalidOperationException("中位机通信检查未通过。")
                                 If CheckCom_3561(True) <> 1 Then Throw New InvalidOperationException("3561 仪表通信检查未通过。")
                                 If CheckCom_34461() <> 0 Then Throw New InvalidOperationException("34461 仪表通信检查未通过。")
                             End Sub, True)
    End Sub

    Public Sub PrepareManualTray(ByVal trayCodeValue As String,
                                 ByVal channels As IReadOnlyList(Of Integer)) Implements IManualTestHost.PrepareManualTray
        ExecuteAutomaticOnUi(Sub()
                                 Dim normalizedTrayCode As String = If(trayCodeValue, String.Empty).Trim()
                                 PrepareTrayForTest(normalizedTrayCode, False, channels)
                                 If String.Equals(normalizedTrayCode, lastTrayNo, StringComparison.Ordinal) Then
                                     Dim repeatDecision As DialogResult = MessageBox.Show("托盘号与上次相同，是否继续执行程序？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question)
                                     If repeatDecision = DialogResult.Cancel Then Throw New OperationCanceledException("操作员取消了重复托盘测试。")
                                 End If
                             End Sub, True)
    End Sub

    Public Function CanStartAutomaticTest() As Boolean Implements IAutomaticTestHost.CanStartAutomaticTest
        Dim ready As Boolean = False
        ExecuteAutomaticOnUi(Sub()
                                 ready = IsAutomaticModeEnabled() AndAlso
                                         AuthorizationService.IsAuthenticated() AndAlso
                                         Not alarmStatus AndAlso
                                         Not resetInProgress AndAlso
                                         Not isRuning AndAlso
                                         Not automaticTestPassInProgress AndAlso
                                         CalibrationService.HasCurrentSnapshot AndAlso
                                         activeTrayPlan IsNot Nothing AndAlso
                                         activeTrayPlan.MeasurementChannels.Count > 0 AndAlso
                                         CheckCom_SCM() = 1 AndAlso
                                         CheckCom_3561(True) = 1 AndAlso
                                         CheckCom_34461() = 0
                             End Sub, True)
        Return ready
    End Function

    Public Function ValidateTrayCode(ByVal trayCodeValue As String) As Boolean Implements IAutomaticTestHost.ValidateTrayCode
        Dim normalized As String = If(trayCodeValue, String.Empty).Trim()
        If normalized.Length = 0 Then Return False
        Dim compactNormalized As String = New String(normalized.Where(Function(character) Not Char.IsWhiteSpace(character)).ToArray())
        If String.Equals(compactNormalized, "NoRead", StringComparison.OrdinalIgnoreCase) Then Return False
        If Initsetting.txtCodeLenth > 0 AndAlso normalized.Length <> Initsetting.txtCodeLenth Then Return False
        Return True
    End Function

    Public Function GetNeedNgRetest() As Boolean Implements IAutomaticTestHost.GetNeedNgRetest
        Dim enabled As Boolean = False
        ExecuteAutomaticOnUi(Sub()
                                 Dim control As CheckBox = FindAutomaticControl(Of CheckBox)("btnNeedNGRetest")
                                 enabled = control IsNot Nothing AndAlso control.Checked
                             End Sub, True)
        Return enabled
    End Function

    Public Function GetRetestRoundCount() As Integer Implements IAutomaticTestHost.GetRetestRoundCount
        If Not GetNeedNgRetest() Then Return 0
        Dim rounds As Integer = 0
        ExecuteAutomaticOnUi(Sub()
                                 Dim control As ComboBox = FindAutomaticControl(Of ComboBox)("cmbRetestNum")
                                 If control IsNot Nothing Then Integer.TryParse(Convert.ToString(control.SelectedItem), rounds)
                             End Sub, True)
        Return Math.Max(0, rounds)
    End Function

    Public Function RequiresNgSorting() As Boolean Implements IAutomaticTestHost.RequiresNgSorting
        Dim sorting As Boolean = False
        ExecuteAutomaticOnUi(Sub()
                                 sorting = currentOCVNum = 1
                             End Sub, True)
        Return sorting
    End Function

    Public Sub SetTrayCode(ByVal trayCodeValue As String) Implements IAutomaticTestHost.SetTrayCode
        ExecuteAutomaticOnUi(Sub()
                                 txtTray.Text = If(trayCodeValue, String.Empty).Trim()
                                 trayCode = txtTray.Text
                             End Sub, True)
    End Sub

    Public Sub PrepareAutomaticTray(ByVal trayCodeValue As String) Implements IAutomaticTestHost.PrepareAutomaticTray
        ExecuteAutomaticOnUi(Sub()
                                 PrepareTrayForTest(trayCodeValue, True)
                             End Sub, True)
    End Sub

    Public Function GetPreparedTrayMeasurementChannels() As IReadOnlyList(Of Integer) Implements IPreparedTrayChannelsProvider.GetPreparedTrayMeasurementChannels
        Dim measurementChannels As IReadOnlyList(Of Integer) = Nothing
        ExecuteAutomaticOnUi(Sub()
                                 If activeTrayPlan IsNot Nothing Then measurementChannels = activeTrayPlan.MeasurementChannels
                             End Sub, True)
        Return measurementChannels
    End Function

    Public Sub StartManualInitialPass(ByVal trayCodeValue As String,
                                      ByVal channels As IReadOnlyList(Of Integer),
                                      ByVal canStart As Func(Of Boolean),
                                      ByVal completed As Action(Of TestPassResult),
                                      ByVal failed As Action(Of Exception)) Implements IManualTestHost.StartManualInitialPass
        ExecuteAutomaticOnUi(Sub()
                                 Try
                                     If resetInProgress Then Return
                                     If canStart Is Nothing OrElse Not canStart() Then Return
                                     If activeTrayPlan Is Nothing Then Throw New InvalidOperationException("托盘准备未完成。")
                                     Dim snapshot As List(Of Integer) = channels.ToList()
                                     If Not activeTrayPlan.MeasurementChannels.SequenceEqual(snapshot) Then
                                         Throw New InvalidOperationException("手动测试通道快照与托盘准备结果不一致。")
                                     End If
                                     If Not ValidateTrayCode(trayCodeValue) Then Throw New InvalidOperationException("当前测试周期托盘码无效。")
                                     txtTray.Text = trayCodeValue.Trim()
                                     lastTrayNo = trayCodeValue.Trim()
                                     haveNgPoint = False
                                     automaticTestPassInProgress = True
                                     StarTest(1, True, completed, failed, channels, True, trayCodeValue)
                                 Catch ex As Exception
                                     automaticTestPassInProgress = False
                                     If failed IsNot Nothing Then failed(ex)
                                 End Try
                             End Sub)
    End Sub

    Public Sub RequestManualTrayCode(ByVal accepted As Action(Of String), ByVal cancelled As Action) Implements IAutomaticTestHost.RequestManualTrayCode
        ExecuteAutomaticOnUi(Sub()
                                 Using dialog As New Form()
                                     dialog.Text = "托盘码读取失败"
                                     dialog.FormBorderStyle = FormBorderStyle.FixedDialog
                                     dialog.StartPosition = FormStartPosition.CenterParent
                                     dialog.MinimizeBox = False
                                     dialog.MaximizeBox = False
                                     dialog.ShowInTaskbar = False
                                     dialog.ClientSize = New Size(380, 142)

                                     Dim prompt As New Label With {
                                         .Text = "托盘码读取失败，请手工输入或重新读取。",
                                         .AutoSize = False,
                                         .Location = New Point(14, 14),
                                         .Size = New Size(350, 24)
                                     }
                                     Dim codeInput As New TextBox With {
                                         .Text = txtTray.Text,
                                         .Location = New Point(14, 42),
                                         .Size = New Size(350, 24),
                                         .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
                                     }
                                     Dim confirmButton As New Button With {
                                         .Text = "确定",
                                         .DialogResult = DialogResult.OK,
                                         .Location = New Point(190, 88),
                                         .Size = New Size(80, 30)
                                     }
                                     Dim cancelButton As New Button With {
                                         .Text = "重新读取",
                                         .DialogResult = DialogResult.Cancel,
                                         .Location = New Point(284, 88),
                                         .Size = New Size(80, 30)
                                     }
                                     dialog.Controls.Add(prompt)
                                     dialog.Controls.Add(codeInput)
                                     dialog.Controls.Add(confirmButton)
                                     dialog.Controls.Add(cancelButton)
                                     dialog.AcceptButton = confirmButton
                                     dialog.CancelButton = cancelButton
                                     AddHandler dialog.Shown, Sub(sender, args) codeInput.Focus()

                                     If dialog.ShowDialog(Me) = DialogResult.OK Then
                                         If accepted IsNot Nothing Then accepted(codeInput.Text.Trim())
                                     ElseIf cancelled IsNot Nothing Then
                                         cancelled()
                                     End If
                                 End Using
                             End Sub)
    End Sub

    Public Sub StartInitialPass(ByVal completed As Action(Of TestPassResult), ByVal failed As Action(Of Exception)) Implements IAutomaticTestHost.StartInitialPass
        ExecuteAutomaticOnUi(Sub()
                                 Try
                                     If resetInProgress Then Return
                                     If activeTrayPlan Is Nothing Then Throw New InvalidOperationException("托盘准备未完成。")
                                     haveNgPoint = False
                                     automaticTestPassInProgress = True
                                     StarTest(1, True, completed, failed, activeTrayPlan.MeasurementChannels)
                                 Catch ex As Exception
                                     automaticTestPassInProgress = False
                                     If failed IsNot Nothing Then failed(ex)
                                 End Try
                             End Sub)
    End Sub

    Public Sub StartRetestPass(ByVal channels As IReadOnlyList(Of Integer),
                               ByVal completed As Action(Of TestPassResult),
                               ByVal failed As Action(Of Exception)) Implements IAutomaticTestHost.StartRetestPass
        ExecuteAutomaticOnUi(Sub()
                                 Try
                                     If resetInProgress Then Return
                                     Array.Clear(retestArray, 0, retestArray.Length)
                                     For Each channel As Integer In channels
                                         If channel >= 1 AndAlso channel <= MAX_CHANNEL Then
                                             retestArray(channel - 1) = 1
                                         End If
                                     Next
                                     haveNgPoint = False
                                     automaticTestPassInProgress = True
                                     StarTest(0, True, completed, failed)
                                 Catch ex As Exception
                                     automaticTestPassInProgress = False
                                     If failed IsNot Nothing Then failed(ex)
                                 End Try
                             End Sub)
    End Sub

    Public Function PublishFinalCycle(ByVal cycle As AutomaticTestCycle) As Boolean Implements IAutomaticTestHost.PublishFinalCycle
        If cycle Is Nothing OrElse activeTrayPlan Is Nothing Then Return False

        Dim resultChannels As List(Of Integer) = If(cycle.IsManualCycle,
                                                     cycle.MeasurementChannels.ToList(),
                                                     activeTrayPlan.OccupiedChannels.ToList())
        If resultChannels.Count = 0 OrElse Not resultChannels.SequenceEqual(activeTrayPlan.OccupiedChannels.OrderBy(Function(channel) channel)) Then
            Throw New InvalidOperationException("最终结果通道与已准备的托盘通道不一致。")
        End If
        Dim previousTestChannels As List(Of Integer) = currentTestChannels
        Dim ocvTestTaskId As Long? = Nothing
        Try
            '自动流程的初测和复测只产生内存中的最终结果；此处统一创建并完成唯一的最终 OCV_DATA 任务。
            currentTestChannels = resultChannels
            ocvTestTaskId = CreateOcvDataTestTask(1, cycle.TrayCode)
            SaveOcvDataTestResults(ocvTestTaskId, 1, DateTime.Now)
            SaveTestDataLocalFile(RecieveData, cycle.TrayCode)
            CompleteOcvDataTestTaskStrict(ocvTestTaskId.Value, "Completed", Nothing)
            BusinessLogService.Write("操作", "成功", "测试结果保存完成", cycle.TrayCode, "自动测试最终结果已保存到 OCV_DATA 和本地文件。", String.Format("OCV_DATA任务号={0}；通道数={1}；复测轮数={2}", ocvTestTaskId.Value, resultChannels.Count, cycle.CurrentRetestRound), "测试结果保存")
        Catch ex As Exception
            CompleteOcvDataTestTaskSafely(ocvTestTaskId, "Failed", ex.Message)
            BusinessLogService.Write("操作", "失败", "自动测试结果保存失败", cycle.TrayCode, "自动测试最终结果未能完整保存。", ex.ToString(), "测试结果保存")
            Throw
        Finally
            currentTestChannels = previousTestChannels
        End Try

        If Not cycle.IsManualCycle Then lastPublishedAutomaticTrayCode = cycle.TrayCode
        If Not activeTrayDebugMode AndAlso FrmMesTask.chkResultDataUploadAssembly.Checked Then
            Try
                Dim finalMes As New ResultDataUploadAssembly(FrmMesTask.listBoxOutput)
                finalMes.ChannelNumbers = resultChannels
                finalMes.StartTimer()
            Catch uploadError As Exception
                AppLog.Write("MES", "自动流程后台上传启动失败：" & uploadError.ToString())
            End Try
        End If
        CompleteTestProgress("测试完成")
        Return True
    End Function

    Private Sub CompleteOcvDataTestTaskStrict(ByVal testTaskId As Long,
                                              ByVal status As String,
                                              ByVal errorMessage As String)
        Using service As New OcvDataService()
            service.CompleteTestTask(testTaskId, status, errorMessage)
        End Using
    End Sub

    Private Function CreatePlcRuntimeStatusReporter() As Action(Of String)
        Dim generation As Integer = Threading.Interlocked.Increment(plcRuntimeStatusGeneration)
        Return Sub(status) ReportPlcRuntimeStatus(status, generation)
    End Function

    Private Sub ReportPlcRuntimeStatus(ByVal status As String, ByVal generation As Integer)
        ExecuteAutomaticOnUi(Sub()
                                 If shutdownPrepared OrElse generation <> plcRuntimeStatusGeneration Then Return
                                 plcConnected = plcAutomationClient IsNot Nothing AndAlso plcAutomationClient.IsOpen
                                 If frmMDI IsNot Nothing AndAlso Not frmMDI.IsDisposed Then
                                     frmMDI.UpdatePlcRuntimeStatus(status)
                                 End If
                             End Sub)
    End Sub

    Public Sub ReportAutomaticStatus(ByVal message As String) Implements IAutomaticTestHost.ReportAutomaticStatus
        ExecuteAutomaticOnUi(Sub()
                                 SetTestStatus(message)
                                 UpdateTestModeVisuals()
                             End Sub)
    End Sub

    Public Sub ReportPlcSmokeInput(ByVal inputHigh As Boolean,
                                   ByVal observedAtUtc As DateTime) Implements IPlcSmokeMonitorHost.ReportPlcSmokeInput
        ExecuteAutomaticOnUi(Sub()
                                 If shutdownPrepared Then Return
                                 If observedAtUtc < lastSmokeInputObservedAtUtc Then Return
                                 lastSmokeInputObservedAtUtc = observedAtUtc
                                 Dim communicationRecovered As Boolean = smokeSignalReadFailed
                                 smokeSignalReadFailed = False

                                 Dim smokeAlarmActive As Boolean = IsSmokeAlarmActive(inputHigh)
                                 Dim indicatorColor As Color = If(smokeAlarmActive, Color.Red, Color.LimeGreen)
                                 If smokeAlarm1 IsNot Nothing Then smokeAlarm1.BackColor = indicatorColor
                                 If smokeAlarm2 IsNot Nothing Then smokeAlarm2.BackColor = indicatorColor

                                 If smokeAlarmActive AndAlso Not smokeAlarmLatched Then
                                     smokeAlarmLatched = True
                                     RefreshAlarmStatus()
                                     Dim alarmMessage As String = "烟感报警：PLC输入 X1F 处于报警电平。"
                                     RequestStopForAlarm(alarmMessage, releaseCylinder:=True)
                                     RecordAlarmEvent(alarmMessage)
                                 ElseIf communicationRecovered Then
                                     RefreshAlarmStatus()
                                     ReportAutomaticEvent("PLC烟感输入 X1F 通信已恢复。")
                                 End If
                             End Sub)
    End Sub

    Public Sub ReportPlcSmokeInputReadFailure(ByVal message As String,
                                              ByVal observedAtUtc As DateTime) Implements IPlcSmokeMonitorHost.ReportPlcSmokeInputReadFailure
        ExecuteAutomaticOnUi(Sub()
                                 If shutdownPrepared Then Return
                                 If observedAtUtc < lastSmokeInputObservedAtUtc Then Return
                                 lastSmokeInputObservedAtUtc = observedAtUtc
                                 If smokeAlarm1 IsNot Nothing Then smokeAlarm1.BackColor = Color.DarkGray
                                 If smokeAlarm2 IsNot Nothing Then smokeAlarm2.BackColor = Color.DarkGray
                                 If smokeSignalReadFailed Then Return

                                 smokeSignalReadFailed = True
                                 RefreshAlarmStatus()
                                 Dim failureMessage As String = If(String.IsNullOrWhiteSpace(message),
                                                                   "PLC烟感输入 X1F 读取失败。",
                                                                   message.Trim())
                                 RecordAlarmEvent(failureMessage)
                                 RequestStopForAlarm(failureMessage & " 当前烟感状态无法确认。")
                             End Sub)
    End Sub

    Private Shared Function IsSmokeAlarmActive(ByVal inputHigh As Boolean) As Boolean
        Return If(SmokeAlarmIsActiveLow, Not inputHigh, inputHigh)
    End Function

    Private Sub PrependEventLogLine(ByVal line As String)
        If txtAlarm Is Nothing OrElse txtAlarm.IsDisposed OrElse String.IsNullOrWhiteSpace(line) Then Return

        Dim updatedText As String = line.TrimEnd(ControlChars.Cr, ControlChars.Lf)
        If txtAlarm.TextLength > 0 Then
            updatedText &= Environment.NewLine & txtAlarm.Text
        End If

        Dim maximumLength As Integer = txtAlarm.MaxLength
        If maximumLength > 0 Then
            While updatedText.Length > maximumLength
                Dim lastLineBreak As Integer = updatedText.LastIndexOf(Environment.NewLine, StringComparison.Ordinal)
                If lastLineBreak < 0 Then
                    updatedText = updatedText.Substring(0, maximumLength)
                    Exit While
                End If
                updatedText = updatedText.Substring(0, lastLineBreak)
            End While
        End If

        txtAlarm.Text = updatedText
        txtAlarm.SelectionStart = 0
        txtAlarm.SelectionLength = 0
        txtAlarm.ScrollToCaret()
    End Sub

    Public Sub ReportAutomaticEvent(ByVal message As String) Implements IAutomaticTestHost.ReportAutomaticEvent
        Dim eventText As String = If(message, String.Empty).Trim()
        If eventText.Length = 0 Then Return

        ExecuteAutomaticOnUi(Sub()
                                 If shutdownPrepared OrElse txtAlarm Is Nothing OrElse txtAlarm.IsDisposed Then Return
                                 Dim occurredAt As DateTime = DateTime.Now
                                 Dim eventLine As String = occurredAt.ToString("HH:mm:ss") & " " & eventText
                                 If eventText.IndexOf("检测到R150=1", StringComparison.Ordinal) >= 0 Then
                                     StartAutomaticEventLog(occurredAt)
                                 End If
                                 If Not automaticEventLogWriter.TryEnqueue(automaticEventLogFilePath, eventLine) AndAlso
                                    Not String.IsNullOrWhiteSpace(automaticEventLogFilePath) Then
                                     System.Diagnostics.Debug.WriteLine("Automatic event log queue is full or closing.")
                                 End If
                                 PrependEventLogLine(eventLine)
                                 If eventText = "设备已复位，等待下一托盘" OrElse
                                    eventText = "自动测试已手动停止，本托盘流程已复位，等待新托盘" OrElse
                                    eventText.StartsWith("自动流程异常：", StringComparison.Ordinal) Then
                                     automaticEventLogFilePath = String.Empty
                                 End If
                             End Sub)
    End Sub

    Private Sub StartAutomaticEventLog(ByVal occurredAt As DateTime)
        Try
            Dim logDirectoryPath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs")
            Dim fileName As String = "testLog+" & occurredAt.ToString("yyyyMMdd_HHmmssfff") & ".txt"
            automaticEventLogFilePath = Path.Combine(logDirectoryPath, fileName)
        Catch ex As Exception
            automaticEventLogFilePath = String.Empty
            System.Diagnostics.Debug.WriteLine("创建自动流程事件日志失败: " & ex.Message)
        End Try
    End Sub

    Public Sub PrepareForShutdown()
        If shutdownPrepared Then Return
        shutdownPrepared = True
        CancelPendingManualNgRetest()
        Threading.Interlocked.Increment(plcRuntimeStatusGeneration)
        _isLoaded = False

        If timer3 IsNot Nothing Then timer3.Stop()
        If temperatureTimer IsNot Nothing Then temperatureTimer.Stop()
        If temperatureModuleClient IsNot Nothing Then
            temperatureModuleClient.Dispose()
            temperatureModuleClient = Nothing
        End If
        If Timer1 IsNot Nothing Then Timer1.Stop()
        If Timer2 IsNot Nothing Then Timer2.Stop()
        If Timer4 IsNot Nothing Then Timer4.Stop()
        plcAutomaticModeActive = False
        If plcAutomaticCoordinator IsNot Nothing Then
            Try
                plcAutomaticCoordinator.Stop()
            Catch
            End Try
            plcAutomaticCoordinator = Nothing
        End If
        If TypeOf plcAutomationClient Is IDisposable Then DirectCast(plcAutomationClient, IDisposable).Dispose()
        If TypeOf trayScannerClient Is IDisposable Then DirectCast(trayScannerClient, IDisposable).Dispose()
        plcAutomationClient = Nothing
        trayScannerClient = Nothing
        If channelGrid IsNot Nothing Then
            channelGrid.Visible = False
            channelGrid.SuspendLayout()
        End If
        automaticEventLogWriter.Complete()
    End Sub

    Public Sub PrepareForApplicationExitUi()
        If shutdownPrepared Then Return
        shutdownPrepared = True
        CancelPendingManualNgRetest()
        Threading.Interlocked.Increment(plcRuntimeStatusGeneration)
        _isLoaded = False

        If timer3 IsNot Nothing Then timer3.Stop()
        If temperatureTimer IsNot Nothing Then temperatureTimer.Stop()
        If Timer1 IsNot Nothing Then Timer1.Stop()
        If Timer2 IsNot Nothing Then Timer2.Stop()
        If Timer4 IsNot Nothing Then Timer4.Stop()
        If TimerTestProgress IsNot Nothing Then TimerTestProgress.Stop()

        plcAutomaticModeActive = False
        If channelGrid IsNot Nothing Then
            channelGrid.Visible = False
            channelGrid.SuspendLayout()
        End If
        automaticEventLogWriter.Complete()
    End Sub

    Public Function ShutdownForApplicationExit() As List(Of String)
        Dim failures As New List(Of String)()
        CancelPendingManualNgRetest()

        RunApplicationExitDeviceStep(failures,
                                     "关闭温度模块",
                                     Sub()
                                         Try
                                             If temperatureModuleClient IsNot Nothing Then temperatureModuleClient.Dispose()
                                         Finally
                                             temperatureModuleClient = Nothing
                                         End Try
                                     End Sub)

        RunApplicationExitDeviceStep(failures,
                                     "停止PLC自动流程",
                                     Sub()
                                         If plcAutomaticCoordinator Is Nothing Then Return
                                         Try
                                             plcAutomaticCoordinator.Stop()
                                         Finally
                                             plcAutomaticCoordinator = Nothing
                                         End Try
                                     End Sub)

        RunApplicationExitDeviceStep(failures,
                                     "释放PLC串口客户端",
                                     Sub()
                                         If TypeOf plcAutomationClient Is IDisposable Then DirectCast(plcAutomationClient, IDisposable).Dispose()
                                         plcAutomationClient = Nothing
                                     End Sub)

        RunApplicationExitDeviceStep(failures,
                                     "释放扫码枪客户端",
                                     Sub()
                                         If TypeOf trayScannerClient Is IDisposable Then DirectCast(trayScannerClient, IDisposable).Dispose()
                                         trayScannerClient = Nothing
                                     End Sub)

        Return failures
    End Function

    Private Sub RunApplicationExitDeviceStep(failures As List(Of String), stepName As String, action As Action)
        Try
            action()
        Catch ex As Exception
            failures.Add(stepName & "：" & ex.Message)
            AppLog.Write("Shutdown", stepName & " failed: " & ex.ToString())
        End Try
    End Sub

    Private Shared Function GetCalibrationFilePath() As String
        Dim dataDirectory As String = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "XL_OCV_256CH")
        Directory.CreateDirectory(dataDirectory)
        Return Path.Combine(dataDirectory, "CalibrateArr.csv")
    End Function

    Private Sub Timer4_Tick(sender As Object, e As EventArgs) Handles Timer4.Tick
        '自动测试的正式触发条件由 PlcAutomaticTestCoordinator 统一管理：
        'R150 -> R160 -> R151，不再使用控制板旧 autoStartFlag 链路。
        Return
    End Sub

    'Private Sub btn_ZeroAdjustment_Click(sender As Object, e As EventArgs) Handles btn_ZeroAdjustment.Click
    '    Dim ch As Integer = 65
    '    Dim temp As Byte()
    '    '切换到ocr测量内阻
    '    IOCardSetAcirOrOcv("ACIR")
    '    temp = mSock.GetSignalSelectChannel(ch)
    '    Try
    '        '测试以下用同一个socket是否成功 20240102
    '        Dim receiveData As Byte()
    '        receiveData = socketSR(temp)
    '        MsgBox("内阻仪调零成功！", vbOKOnly)
    '    Catch ex As Exception
    '        MsgBox("内阻仪调零失败！", vbOKOnly)
    '    End Try
    'End Sub

    ''' <summary>
    ''' 读出所有通道校准值
    ''' </summary>
    Sub CalibrateOperate()
        ' Initialize or load the array
        If IO.File.Exists(filePath) Then
            ' If the file exists, read values from the file
            ReadArrayFromFile()
        Else
            ' If the file doesn't exist, initialize the array with some values
            InitializeArray()
        End If
    End Sub

    Sub InitializeArray()
        ' Initialize the array with some values
        For i As Integer = 0 To MAX_CHANNEL - 1
            calibrateValueArray(i) = 0
        Next
    End Sub
    ' 将Double类型数组保存为CSV文件
    Sub SaveCalibrateArrayToFile()
        Try
            Using writer As StreamWriter = New StreamWriter(filePath)
                For Each val As Double In calibrateValueArray
                    writer.WriteLine(val)
                Next
            End Using
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("保存校准数组失败: " & ex.Message)
        End Try
    End Sub

    ' 从CSV文件中读取数据放入Double类型数组中
    Function ReadArrayFromFile() As Double()
        Dim result As New List(Of Double)()
        Using reader As StreamReader = New StreamReader(filePath)
            While Not reader.EndOfStream
                Dim line As String = reader.ReadLine()
                Dim value As Double
                If Double.TryParse(line, value) Then
                    result.Add(value)
                End If
            End While
        End Using
        calibrateValueArray = result.ToArray()
        Return result.ToArray()
    End Function
    Sub SaveCalibrateArrayToFile1()
        ' Save the array to the CSV file
        Using writer As New StreamWriter(filePath)
            For Each value As Integer In calibrateValueArray
                ' writer.WriteLine(value)
                writer.Write(value)
                writer.Write(",")
            Next
        End Using
    End Sub

    Sub ReadArrayFromFile1()
        ' Read values from the CSV file into the array
        Dim values As String() = IO.File.ReadAllText(filePath).Split(","c)

        For i As Integer = 0 To Math.Min(values.Length - 1, MAX_CHANNEL - 1)
            Short.TryParse(values(i), calibrateValueArray(i))
        Next
    End Sub

    ''' <summary>
    ''' 校准按钮，通过测量校准模块，比较测试值和模块阻值的差，把差值存在校准数组中：short [] ArrOfCalibrate.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub btn_ChCalibrate_Click(sender As Object, e As EventArgs)
        Dim ch As Integer = cmb_ChSelect.SelectedItem
        Dim temp As Byte()
        Dim reFlag As Short
        '检测单片机通讯   
        Try
            If frmDebug.mSock IsNot Nothing Then
                reFlag = 0
            End If
        Catch ex As Exception

        End Try
        '*****************1、检查托盘是否闭合******************
        GetSignalTrayStatus()

        'If fixtureClose = True Then
        'Else
        '闭合夹具（关闭托盘）
        temp = mSock.GetSignalFixtureDown
        Dim bytes(500) As Byte
        bytes = socketSR(temp)
        Sleep(9000)
        GetSignalTrayStatus()
        If fixtureClose = True Then
        Else
            MsgBox("托盘未闭合！测试停止！", vbOKOnly, "提示")
            Exit Sub
        End If
        'Exit Function
        'End If

        '把继电器切换到3562仪表 
        IOCardSetAcirOrOcv("ACIR")
        '发送选择仪表信号
        temp = mSock.SendSignalSellectInstrumentType(CByte(0))
        Sleep(500)
        '发送选通单通道命令
        temp = mSock.GetSignalSelectChannel(ch)
        Dim bytes1 = socketSR(temp)
        Sleep(2000)

        '读取3562的内阻值
        Dim Resistance As Single
        Dim Voltage As Single

        'Sleep（2000） 'delayT毫秒，每个通道的切换时间
        reFlag = Trans_ReadData_3562(Voltage, Resistance)
        ' Trans_BatchReadData_3561(ch， 1, 1)

        If Not ApplicationShuttingDown Then
            txtJiajuR.Text = "3.75"
            txtCaliR.Text = (Resistance - Single.Parse(txtJiajuR.Text)).ToString()

            txtTestR.Text = Resistance.ToString()
            CalibrateR = Resistance - 3.75
            ''把数据保存到initset文件中
            Initsetting.sngR_B = CalibrateR
            '把校准数据保存在校准值数组calibrateValueArray(i)中
            calibrateValueArray(ch) = Single.Parse(txtJiajuR.Text)
        End If

        '松开夹具（打开托盘）
        temp = mSock.GetSignalFixtureUp
        Dim bytes2 = socketSR(temp)
        Sleep(100)
        bytes2 = socketSR(temp)
        'msg1 = mSock.Send(temp)
    End Sub
    Private Sub cmb_ChSelect_SelectedIndexChanged(sender As Object, e As EventArgs)
        Dim selectCh As Integer
        selectCh = cmb_ChSelect.SelectedItem
        txtCaliR.Text = calibrateValueArray(selectCh - 1).ToString()
    End Sub

    'Private Sub btnScanTray_Click(sender As Object, e As EventArgs) Handles btnScanTray.Click
    '    '显示条码录入界面
    '    If Not My.Forms.frmInput.IsDisposed Then
    '        My.Forms.frmInput.Dispose()
    '    End If

    '    My.Forms.frmInput.txtTray.Text = txtTray.Text
    '    '显示首尾条形码
    '    My.Forms.frmInput.txtFirst.Text = txtFirstNum.Text
    '    My.Forms.frmInput.txtLast.Text = txtLastNum.Text

    '    My.Forms.frmInput.ShowDialog()
    '    My.Forms.frmInput.Activate()
    '    'My.Forms.frmInput.Close()
    '    '托盘编码自动更新到联机界面
    '    txtTray.Text = My.Forms.frmInput.txtTray.Text
    '    '显示首尾条形码
    '    txtFirstNum.Text = My.Forms.frmInput.txtFirst.Text
    '    txtLastNum.Text = My.Forms.frmInput.txtLast.Text
    '    '选择结果决定待测试通道数，扫码不会覆盖该数量。
    '    HandleSelectedChannelsChanged(channelGrid, EventArgs.Empty)
    'End Sub
    '温度报警保持锁存，只有复位时重新读取到有效且低于阈值的数据才能解除。
    Private Sub t1_TextChanged(sender As Object, e As EventArgs) Handles t1.TextChanged
        HandleTemperatureTextChanged(1, t1.Text)
    End Sub

    Private Sub t2_TextChanged(sender As Object, e As EventArgs) Handles t2.TextChanged
        HandleTemperatureTextChanged(2, t2.Text)
    End Sub

    Private Sub t3_TextChanged(sender As Object, e As EventArgs) Handles t3.TextChanged
        HandleTemperatureTextChanged(3, t3.Text)
    End Sub

    Private Sub t4_TextChanged(sender As Object, e As EventArgs) Handles t4.TextChanged
        HandleTemperatureTextChanged(4, t4.Text)
    End Sub

    Private Sub HandleTemperatureTextChanged(sensorNumber As Integer, text As String)
        Dim temperatureValue As Decimal
        If Not Decimal.TryParse(If(text, String.Empty),
                                System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.CurrentCulture,
                                temperatureValue) Then Return
        If temperatureValue <= CDec(Initsetting.overTemperateValue) Then Return

        If sensorNumber > 2 Then temperatureAlarmRequiresUnavailableSensors = True
        If temperatureAlarmLatched Then Return

        temperatureAlarmLatched = True
        RefreshAlarmStatus()
        Dim alarmText As String = String.Format("超温报警：温度探头 {0}，当前 {1:0.0}℃，阈值 {2}℃。",
                                                sensorNumber,
                                                temperatureValue,
                                                Initsetting.overTemperateValue)
        RequestStopForAlarm(alarmText, releaseCylinder:=True)
        RecordAlarmEvent(alarmText)
    End Sub

    Private Sub RefreshAlarmStatus()
        alarmStatus = temperatureAlarmLatched OrElse ioAlarmLatched OrElse
                      smokeAlarmLatched OrElse smokeSignalReadFailed OrElse resetFailureLatched
        If LabelAlarm IsNot Nothing Then
            LabelAlarm.Visible = alarmStatus
            If smokeAlarmLatched Then
                LabelAlarm.Text = "烟感报警"
            ElseIf smokeSignalReadFailed Then
                LabelAlarm.Text = "烟感通信异常"
            ElseIf ioAlarmLatched Then
                LabelAlarm.Text = "急停报警"
            ElseIf temperatureAlarmLatched Then
                LabelAlarm.Text = "温度报警"
            ElseIf resetFailureLatched Then
                LabelAlarm.Text = "复位异常"
            End If
        End If
        If LabelRun IsNot Nothing Then LabelRun.Visible = Not alarmStatus
        UpdateTestModeVisuals()
    End Sub

    Private Sub RecordAlarmEvent(message As String)
        Dim timestamp As DateTime = DateTime.Now
        Dim alarmText As String = timestamp.ToString("yyyy-MM-dd HH:mm:ss") & ":" & If(message, String.Empty)
        PrependEventLogLine(timestamp.ToString("HH:mm:ss") & " " & If(message, String.Empty))
        AppLog.Write("Alarm", alarmText)
        BusinessLogService.WriteAlarm(alarmText)
    End Sub

    Private Sub RequestStopForAlarm(message As String, Optional ByVal releaseCylinder As Boolean = False)
        If isRuning Then SetStopRequested(True)
        If plcAutomaticCoordinator IsNot Nothing Then plcAutomaticCoordinator.RequestStopCurrentCycle()
        If releaseCylinder Then RequestAlarmCylinderRelease()
        SetTestStatus(message & " 已停止当前测试流程，请检查后复位。")
        UpdateTestModeVisuals()
    End Sub

    Private Sub RequestAlarmCylinderRelease()
        If System.Threading.Interlocked.CompareExchange(alarmCylinderReleaseState, 1, 0) <> 0 Then Return

        Dim plcClient As IPlcAutomationClient = plcAutomationClient
        If plcClient Is Nothing OrElse Not plcClient.IsOpen Then
            System.Threading.Interlocked.Exchange(alarmCylinderReleaseState, 0)
            ReportAutomaticEvent("报警联动失败：PLC串口未打开，未能发送 R161=1 气缸弹开指令。")
            Return
        End If

        Try
            System.Threading.Tasks.Task.Run(
                Sub()
                    Try
                        plcClient.WriteR161()
                        System.Threading.Interlocked.Exchange(alarmCylinderReleaseState, 2)
                        ReportAutomaticEvent("报警联动已确认：PLC R161=1，气缸弹开指令已发送。")
                    Catch ex As Exception
                        System.Threading.Interlocked.Exchange(alarmCylinderReleaseState, 0)
                        ReportAutomaticEvent("报警联动失败：PLC写入 R161=1 未确认，气缸弹开指令可能未执行。" & ex.Message)
                        ExecuteAutomaticOnUi(Sub()
                                                 If shutdownPrepared Then Return
                                                 SetTestStatus("报警停机；PLC未确认 R161=1，请检查PLC及气缸。")
                                             End Sub)
                    End Try
                End Sub)
        Catch ex As Exception
            System.Threading.Interlocked.Exchange(alarmCylinderReleaseState, 0)
            ReportAutomaticEvent("报警联动失败：无法启动PLC R161=1发送任务。" & ex.Message)
        End Try
    End Sub

    Private Sub txtLastNum_TextChanged(sender As Object, e As EventArgs) Handles txtLastNum.TextChanged
        avalibleCount = Val(txtLastNum.Text)
    End Sub

    Private Async Sub btnNGTest_Click(sender As Object, e As EventArgs) Handles btnNGTest.Click
        Dim plcCycleOccupied As Boolean = plcAutomaticCoordinator IsNot Nothing AndAlso plcAutomaticCoordinator.IsCycleOccupied
        If resetInProgress OrElse chkAutoTest.Checked OrElse plcAutomaticModeActive OrElse isRuning OrElse
           automaticTestPassInProgress OrElse plcCycleOccupied Then
            MessageBox.Show("自动流程或正式测试进行中，不能手动启动 NG 复测。", "提示", MessageBoxButtons.OK)
            Return
        End If
        If alarmStatus Then
            MessageBox.Show("目前处于报警状态，请先复位报警！", "提示", MessageBoxButtons.OK)
            Exit Sub
        End If

        If Not AuthorizationService.RequireAuthenticatedUser(Me) Then Return
        If Not ValidateTrayCode(txtTray.Text) Then
            MessageBox.Show("请先确认有效托盘码，再进行 NG 复测。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim ngChannels As Integer() = If(retestArray Is Nothing,
                                         New Integer() {},
                                         Enumerable.Range(1, Math.Min(MAX_CHANNEL, retestArray.Length)).
                                             Where(Function(channelNo) retestArray(channelNo - 1) = 1).
                                             ToArray())
        If ngChannels.Length = 0 Then
            MessageBox.Show("当前没有可复测的 NG 通道。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        If CalibrationService.IsCalibrationAcquisitionInProgress OrElse CalibrationService.IsBarcodeRetestInProgress Then
            MessageBox.Show("通道校准或扫码复测正在采集，当前不能启动 NG 复测。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        If currentOCVNum < 1 OrElse currentOCVNum > 3 Then
            MessageBox.Show("未选择有效的 OCV 测试阶段。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            If CalibrationService.HasCurrentSnapshot Then
                calibrationLoadError = String.Empty
            Else
                CalibrationService.SharedInstance.LoadSnapshot()
                calibrationLoadError = String.Empty
            End If
            EnsureManualTestReady()
        Catch ex As Exception
            MessageBox.Show("NG 复测准备失败，未发送 PLC R160=1：" & ex.Message,
                            "提示",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
            Return
        End Try

        Dim plcClient As IPlcAutomationClient = plcAutomationClient
        If plcClient Is Nothing OrElse Not plcClient.IsOpen Then
            MessageBox.Show("PLC 串口未连接，未发送 R160=1，NG 复测未启动。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim pressCancellation As New System.Threading.CancellationTokenSource()
        manualNgRetestCancellation = pressCancellation
        automaticTestPassInProgress = True
        UpdateTestModeVisuals()
        Try
            plcClient.WriteR160()
            ReportAutomaticEvent("主页面NG点测试已发送PLC R160=1，等待针床压合信号R151=1。")
            SetTestStatus("已发送 PLC R160=1，等待针床压合到位...")

            Dim fixturePressed As Boolean = Await System.Threading.Tasks.Task.Run(
                Function()
                    Dim deadline As DateTime = DateTime.UtcNow.AddMilliseconds(ManualNgFixturePressTimeoutMs)
                    Do
                        pressCancellation.Token.ThrowIfCancellationRequested()
                        If Not plcClient.IsOpen Then Throw New InvalidOperationException("PLC串口已断开。")
                        If plcClient.ReadR151() Then Return True

                        Dim remainingMs As Double = (deadline - DateTime.UtcNow).TotalMilliseconds
                        If remainingMs <= 0 Then Return False
                        pressCancellation.Token.WaitHandle.WaitOne(CInt(Math.Min(250.0R, remainingMs)))
                    Loop
                    Return False
                End Function,
                pressCancellation.Token)

            If Not fixturePressed Then
                Throw New TimeoutException(String.Format("{0}秒内未收到PLC针床压合信号R151=1。", ManualNgFixturePressTimeoutMs \ 1000))
            End If
            If resetInProgress OrElse shutdownPrepared Then Return

            ReportAutomaticEvent("PLC针床压合信号R151=1已确认，开始主页面NG点复测。")
            haveNgPoint = False
            automaticTestPassInProgress = False
            Call StarTest(0)
        Catch ex As OperationCanceledException
            If Not resetInProgress AndAlso Not shutdownPrepared Then
                ReportAutomaticEvent("主页面NG点测试已取消，保留待复测NG点。")
                SetTestStatus("NG复测已取消，待复测NG点保留")
            End If
        Catch ex As Exception
            ReportAutomaticEvent("主页面NG点测试未启动：" & ex.Message)
            MessageBox.Show("针床压合未确认，NG 复测未启动：" & ex.Message,
                            "提示",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
        Finally
            If Object.ReferenceEquals(manualNgRetestCancellation, pressCancellation) Then
                manualNgRetestCancellation = Nothing
            End If
            pressCancellation.Dispose()
            automaticTestPassInProgress = False
            If Not shutdownPrepared AndAlso Not IsDisposed Then UpdateTestModeVisuals()
        End Try
    End Sub

    Private Sub txtTray_TextChanged(sender As Object, e As EventArgs) Handles txtTray.TextChanged
        trayCodeSet = txtTray.Text
    End Sub

    Private Sub txtFirstNum_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs)
        If e.KeyCode = 13 Then
            txtLastNum.Focus()
            txtLastNum.SelectionStart = 0
            txtLastNum.SelectionLength = Len(txtLastNum.Text)
        End If
    End Sub

    Private Sub frmTest_LocationChanged(sender As Object, e As EventArgs) Handles Me.LocationChanged

    End Sub

    Private Function BuildAllChannelDisplayData() As List(Of ChannelDisplayData)
        Dim result As New List(Of ChannelDisplayData)(MAX_CHANNEL)
        For channelNo As Integer = 1 To MAX_CHANNEL
            result.Add(BuildChannelDisplayData(channelNo))
        Next
        Return result
    End Function

    Private Sub RefreshChannelOverview()
        If shutdownPrepared OrElse ApplicationShuttingDown OrElse IsDisposed OrElse Not IsHandleCreated Then Return
        If InvokeRequired Then
            BeginInvoke(New MethodInvoker(AddressOf RefreshChannelOverview))
            Return
        End If

        If channelGrid IsNot Nothing AndAlso Not channelGrid.IsDisposed Then
            channelGrid.UpdateAll(BuildAllChannelDisplayData())
        End If
        If statisticsControl IsNot Nothing AndAlso Not statisticsControl.IsDisposed Then
            statisticsControl.UpdateStatistics(BuildChannelStatistics())
        End If
    End Sub

    Private Function BuildChannelDisplayData(ByVal channelNo As Integer) As ChannelDisplayData
        Dim index As Integer = channelNo - 1
        Dim row As Integer = (index \ intRow) + 1
        Dim column As Integer = (index Mod intRow) + 1
        Dim resultText As String = If(RecieveData(index).Result, String.Empty).Trim()
        Dim statusText As String = GetChannelStatusText(channelNo, resultText)

        Return New ChannelDisplayData With {
            .ChannelNo = channelNo,
            .Row = row,
            .Column = column,
            .Barcode = RecieveData(index).BatCode,
            .Ocv1 = RecieveData(index).Voltage * 1000.0F,
            .Ir1 = RecieveData(index).Resistance,
            .StatusText = statusText,
            .ResultText = resultText
        }
    End Function

    Private Function GetChannelStatusText(ByVal channelNo As Integer, ByVal resultText As String) As String
        Select Case GetChannelStatusKind(channelNo, resultText)
            Case ChannelStatusKind.Pass
                Return "PASS"
            Case ChannelStatusKind.Testing
                Return "TESTING"
            Case ChannelStatusKind.ResistanceNg
                Return "RESISTANCE_NG"
            Case ChannelStatusKind.VoltageNg
                Return "VOLTAGE_NG"
            Case Else
                Return "IDLE"
        End Select
    End Function

    Private Function GetChannelStatusKind(ByVal channelNo As Integer, ByVal resultText As String) As ChannelStatusKind
        If testingChannelNumbers.Contains(channelNo) Then Return ChannelStatusKind.Testing

        If String.Equals(resultText, "OK", StringComparison.OrdinalIgnoreCase) OrElse
           String.Equals(resultText, "PASS", StringComparison.OrdinalIgnoreCase) Then
            Return ChannelStatusKind.Pass
        End If

        If String.Equals(resultText, "NG", StringComparison.OrdinalIgnoreCase) Then
            Dim item As Bat_info = RecieveData(channelNo - 1)
            Dim voltageMillivolts As Single = item.Voltage * 1000.0F
            Dim voltageNg As Boolean = voltageMillivolts > Initsetting.upVoltage OrElse
                                       voltageMillivolts < Initsetting.downVoltage
            Dim resistanceNg As Boolean = item.Resistance > Initsetting.upResistance OrElse
                                          item.Resistance < Initsetting.downResistance
            If resistanceNg Then Return ChannelStatusKind.ResistanceNg
            If voltageNg Then Return ChannelStatusKind.VoltageNg
        End If

        Return ChannelStatusKind.Idle
    End Function

    Public Sub ApplyConfiguredChannelColors()
        ApplyConfiguredChannelColors(Initsetting)
    End Sub

    Friend Sub ApplyConfiguredChannelColors(ByVal settings As Initset)
        If IsDisposed OrElse channelGrid Is Nothing OrElse channelGrid.IsDisposed Then Return
        If InvokeRequired Then
            BeginInvoke(New Action(Of Initset)(AddressOf ApplyConfiguredChannelColors), settings)
            Return
        End If
        channelStatusColors = ChannelColorSettings.FromInitset(settings)
        channelGrid.ApplyStatusColors(channelStatusColors)
        If statisticsControl IsNot Nothing AndAlso Not statisticsControl.IsDisposed Then
            statisticsControl.ApplyStatusColors(channelStatusColors)
        End If
        RefreshChannelOverview()
    End Sub

    Private Sub HandleChannelSelected(ByVal sender As Object, ByVal e As ChannelSelectedEventArgs) Handles channelGrid.ChannelSelected
        If e Is Nothing Then Return
        Dim data As ChannelDisplayData = e.Data
        SetTestStatus(String.Format("通道 {0}-{1}：{2}，OCV1 {3:0.00} mV，IR1 {4:0.000} mΩ", data.Row, data.Column, If(String.IsNullOrWhiteSpace(data.StatusText), "空闲", data.StatusText), If(data.Ocv1.HasValue, data.Ocv1.Value, 0.0F), If(data.Ir1.HasValue, data.Ir1.Value, 0.0F)))
    End Sub

    Private Function BuildChannelStatistics() As ChannelStatistics
        Dim statistics As New ChannelStatistics With {.Total = MAX_CHANNEL}
        For channel As Integer = 0 To MAX_CHANNEL - 1
            Dim resultText As String = If(RecieveData(channel).Result, String.Empty).Trim().ToUpperInvariant()
            If resultText = "NG" Then
                statistics.NgCount += 1
            ElseIf resultText = "OK" OrElse resultText = "PASS" Then
                statistics.PassCount += 1
            ElseIf testingChannelNumbers.Contains(channel + 1) Then
                statistics.TestingCount += 1
            Else
                statistics.IdleCount += 1
            End If
        Next
        Return statistics
    End Function

    Private Sub MarkSelectedChannelsTesting()
        testingChannelNumbers.Clear()
        If currentTestChannels Is Nothing Then Return

        For Each channelNo As Integer In currentTestChannels.Distinct().Where(Function(channel) channel >= 1 AndAlso channel <= MAX_CHANNEL)
            testingChannelNumbers.Add(channelNo)
            RecieveData(channelNo - 1).Result = String.Empty
        Next

        RefreshChannelOverview()
        SetTestStatus(String.Format("正在测试 {0} 个通道...", testingChannelNumbers.Count))
        Application.DoEvents()
    End Sub



    Private Sub TestModeCheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles chkAutoTest.CheckedChanged, chkManualTest.CheckedChanged
        If chkAutoTest Is Nothing OrElse chkManualTest Is Nothing Then Return
        If modeChangeInProgress Then Return

        If _isLoaded AndAlso Not AuthorizationService.IsAuthenticated() Then
            Dim previousAutoMode As Boolean = If(sender Is chkAutoTest, Not chkAutoTest.Checked, chkAutoTest.Checked)
            Dim mainForm As MDIForm = TryCast(Me.MdiParent, MDIForm)
            If mainForm Is Nothing OrElse Not mainForm.EnsureAuthenticatedUser() Then
                SetTestMode(previousAutoMode)
                SetTestStatus("请先登录后再切换测试模式。")
                Return
            End If
        End If

        If isRuning OrElse automaticTestPassInProgress OrElse manualNgRetestCancellation IsNot Nothing OrElse
           (plcAutomaticCoordinator IsNot Nothing AndAlso plcAutomaticCoordinator.IsCycleOccupied) Then
            SetTestMode(plcAutomaticModeActive)
            SetTestStatus("测试周期进行中，不能切换模式")
            Return
        End If

        modeChangeInProgress = True
        Try
            If sender Is chkManualTest AndAlso chkManualTest.Checked Then
                chkAutoTest.Checked = False
            ElseIf sender Is chkAutoTest AndAlso chkAutoTest.Checked Then
                chkManualTest.Checked = False
            End If

            If Not chkAutoTest.Checked AndAlso Not chkManualTest.Checked Then
                chkAutoTest.Checked = True
            End If

            If chkAutoTest.Checked AndAlso channelGrid IsNot Nothing Then
                channelGrid.SelectAll()
            End If
        Finally
            modeChangeInProgress = False
            UpdateTestModeVisuals()
            If _isLoaded AndAlso Not shutdownPrepared Then
                StopPlcTestCoordinator()
                If chkAutoTest.Checked Then
                    StartPlcAutomaticTest()
                Else
                    Try
                        StartManualPlcConnection()
                    Catch ex As Exception
                        SetTestStatus("手动 PLC 连接失败：" & ex.Message)
                    End Try
                End If
            End If
        End Try
    End Sub

    Private Sub SetTestMode(ByVal autoMode As Boolean)
        If chkAutoTest Is Nothing OrElse chkManualTest Is Nothing Then Return

        modeChangeInProgress = True
        Try
            chkAutoTest.Checked = autoMode
            chkManualTest.Checked = Not autoMode
            If autoMode AndAlso channelGrid IsNot Nothing Then
                channelGrid.SelectAll()
            End If
        Finally
            modeChangeInProgress = False
            UpdateTestModeVisuals()
        End Try
    End Sub

    Private Sub UpdateTestModeVisuals()
        If chkAutoTest Is Nothing OrElse chkManualTest Is Nothing Then Return
        UiTheme.ApplyToggleStateStyle(chkAutoTest, chkAutoTest.Checked)
        UiTheme.ApplyToggleStateStyle(chkManualTest, chkManualTest.Checked)
        Dim cycleOccupied As Boolean = plcAutomaticCoordinator IsNot Nothing AndAlso plcAutomaticCoordinator.IsCycleOccupied
        Dim cycleBusy As Boolean = isRuning OrElse automaticTestPassInProgress OrElse
                                   manualNgRetestCancellation IsNot Nothing OrElse cycleOccupied OrElse resetInProgress
        If btnStop IsNot Nothing Then
            btnStop.Enabled = cycleBusy AndAlso Not IsStopRequested() AndAlso Not shutdownPrepared AndAlso Not resetInProgress
        End If
        If btnReset IsNot Nothing Then btnReset.Enabled = Not resetInProgress AndAlso Not shutdownPrepared
        If chk_Test IsNot Nothing Then
            chk_Test.Enabled = Not cycleBusy AndAlso Not initializationApplyInProgress
        End If
        If CmdStart IsNot Nothing Then CmdStart.Enabled = chkManualTest.Checked AndAlso Not cycleBusy AndAlso Not alarmStatus AndAlso Not initializationApplyInProgress AndAlso CalibrationService.HasCurrentSnapshot
        If btnNGTest IsNot Nothing Then btnNGTest.Enabled = Not cycleBusy AndAlso Not alarmStatus AndAlso Not resetInProgress
        If btnGetBarcode IsNot Nothing Then btnGetBarcode.Enabled = chkManualTest.Checked AndAlso Not cycleBusy AndAlso Not initializationApplyInProgress
        If txtTray IsNot Nothing Then txtTray.ReadOnly = chkAutoTest.Checked OrElse cycleBusy
        SetSelectionControlsEnabled(Not cycleBusy)
    End Sub

    Private Sub ConfigureStationControls()
        currentStationButton.Visible = True
        Rbtn1.Text = "OCV0"
        Rbtn2.Text = "OCV1"
        Rbtn3.Text = "OCV2"
        Rbtn1.Visible = False
        Rbtn2.Visible = False
        Rbtn3.Visible = False
        GroupBox2.Visible = False
        GroupBox2.Enabled = False
        cmbShow.Visible = False
        cmbShow.Enabled = False
        CmdBatInput.Visible = False
        CmdBatInput.Enabled = False
        '工位由系统参数决定，主测试页只展示当前工位，避免运行中切换导致测试阶段和数据字段不一致。
        Rbtn1.Enabled = False
        Rbtn2.Enabled = False
        Rbtn3.Enabled = False

        Select Case Initsetting.stationSelect
            Case 1
                Rbtn2.Checked = True
                Rbtn2.Visible = True
                currentStationButton.Text = "OCV1"
            Case 2
                Rbtn3.Checked = True
                Rbtn3.Visible = True
                currentStationButton.Text = "OCV2"
            Case Else
                Rbtn1.Checked = True
                Rbtn1.Visible = True
                currentStationButton.Text = "OCV0"
        End Select
        UpdateCurrentOcvSelection()
        UiTheme.ApplyToggleStateStyle(Rbtn1, Rbtn1.Checked)
        UiTheme.ApplyToggleStateStyle(Rbtn2, Rbtn2.Checked)
        UiTheme.ApplyToggleStateStyle(Rbtn3, Rbtn3.Checked)
    End Sub

    Private Sub UpdateCurrentOcvSelection()
        If Rbtn1.Checked Then
            currentOCVNum = 1
        ElseIf Rbtn2.Checked Then
            currentOCVNum = 2
        ElseIf Rbtn3.Checked Then
            currentOCVNum = 3
        End If
    End Sub



    Private Sub ResetLeftPanelScroll()
        If Panel1 Is Nothing OrElse Panel1.IsDisposed Then Return
        Panel1.AutoScrollPosition = New Point(0, 0)
    End Sub

    Private Sub BeginTestProgress(totalCount As Integer, message As String)
        testProgressState.Begin(totalCount, message)
        TimerTestProgress.Start()
        If testProgressDialog Is Nothing OrElse testProgressDialog.IsDisposed Then
            testProgressDialog = New frmTestProgress()
        End If
        If Not testProgressDialog.Visible Then
            testProgressDialog.Show(Me)
        Else
            testProgressDialog.BringToFront()
        End If
        ApplyTestProgressSnapshot(testProgressState.CreateSnapshot())
        testProgressDialog.BeginProgress(testProgressState.CreateSnapshot())
    End Sub

    Private Sub PublishTestProgress(stage As TestProgressStage,
                                    message As String,
                                    progressPercent As Integer,
                                    Optional currentChannel As Integer? = Nothing)
        testProgressState.SetStage(stage, message, currentChannel, progressPercent)
        ApplyTestProgressSnapshot(testProgressState.CreateSnapshot())
    End Sub

    Private Sub CompleteTestProgress(message As String)
        testProgressState.Complete(message)
        TimerTestProgress.Stop()
        ApplyTestProgressSnapshot(testProgressState.CreateSnapshot())
        If testProgressDialog IsNot Nothing AndAlso Not testProgressDialog.IsDisposed Then
            testProgressDialog.MarkCompleted(message)
            testProgressDialog.Hide()
        End If
    End Sub

    Private Sub FailTestProgress(message As String)
        testProgressState.Fail(message)
        TimerTestProgress.Stop()
        ApplyTestProgressSnapshot(testProgressState.CreateSnapshot())
        If testProgressDialog IsNot Nothing AndAlso Not testProgressDialog.IsDisposed Then
            testProgressDialog.MarkFailed(message)
        End If
    End Sub

    Private Sub StopTestProgressIfNeeded()
        Dim snapshot As TestProgressSnapshot = testProgressState.CreateSnapshot()
        If snapshot.Stage <> TestProgressStage.Idle AndAlso
           snapshot.Stage <> TestProgressStage.Completed AndAlso
           snapshot.Stage <> TestProgressStage.Failed AndAlso
           snapshot.Stage <> TestProgressStage.Canceled Then
            FailTestProgress("测试流程已停止")
        Else
            TimerTestProgress.Stop()
            ApplyTestProgressSnapshot(snapshot)
        End If
    End Sub

    Private Sub CancelTestProgress(message As String)
        testProgressState.Cancel(message)
        TimerTestProgress.Stop()
        ApplyTestProgressSnapshot(testProgressState.CreateSnapshot())
    End Sub

    Private Sub ClearIncompleteTestResults(channels As IEnumerable(Of Integer))
        If channels IsNot Nothing Then
            For Each channel As Integer In channels
                If channel < 1 OrElse channel > MAX_CHANNEL Then Continue For
                RecieveData(channel - 1).Voltage = 0
                RecieveData(channel - 1).Resistance = 0
                RecieveData(channel - 1).RawResistanceMilliohm = 0
                RecieveData(channel - 1).CalibrationValueMilliohm = 0
                RecieveData(channel - 1).FinalResistanceMilliohm = 0
                RecieveData(channel - 1).Result = String.Empty
            Next
        End If
        activeTrayPlan = Nothing
        If txtTray IsNot Nothing Then txtTray.Clear()
        Array.Clear(retestArray, 0, retestArray.Length)
        testingChannelNumbers.Clear()
        RefreshChannelOverview()
    End Sub

    Private Sub ResetTestProgressDisplay()
        TimerTestProgress.Stop()
        testProgressState.Reset("等待开始")
        ApplyTestProgressSnapshot(testProgressState.CreateSnapshot())
    End Sub

    Private Sub TimerTestProgress_Tick(sender As Object, e As EventArgs) Handles TimerTestProgress.Tick
        ApplyTestProgressSnapshot(testProgressState.CreateSnapshot())
    End Sub

    Private Sub ApplyTestProgressSnapshot(snapshot As TestProgressSnapshot)
        If snapshot Is Nothing OrElse IsDisposed Then Return
        If InvokeRequired Then
            BeginInvoke(New Action(Of TestProgressSnapshot)(AddressOf ApplyTestProgressSnapshot), snapshot)
            Return
        End If

        If testProgressDialog IsNot Nothing AndAlso Not testProgressDialog.IsDisposed Then
            testProgressDialog.ApplySnapshot(snapshot)
        End If
        Dim statusText As String
        If snapshot.Stage = TestProgressStage.Idle Then
            statusText = "等待测试"
        ElseIf snapshot.Stage = TestProgressStage.Completed Then
            statusText = "测试完成"
        ElseIf snapshot.Stage = TestProgressStage.Failed Then
            statusText = "测试失败"
        Else
            statusText = "测试中"
        End If
        SetTestStatus(statusText)
    End Sub

    Private Sub SetTestStatus(ByVal message As String)
        If frmMDI Is Nothing OrElse frmMDI.IsDisposed Then Return
        If frmMDI.txtTesting IsNot Nothing Then
            frmMDI.txtTesting.Text = If(message, String.Empty)
        End If
    End Sub
End Class
