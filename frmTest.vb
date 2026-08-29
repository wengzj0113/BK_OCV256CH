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
    '窗体是否已加载完成（用于屏蔽 Resize 期间重绘，避免初始化阶段空引用）
    Private _isLoaded As Boolean = False
    Private shutdownPrepared As Boolean = False
    Private responsiveLayout As ResponsiveTestLayout
    Private grpTestMode As GroupBox
    Private chkAutoTest As CheckBox
    Private chkManualTest As CheckBox
    Private modeChangeInProgress As Boolean
    Private deferredStartupScheduled As Boolean
    Private deferredStartupCompleted As Boolean
    Private firstPaintRecorded As Boolean
    Private lastTestSelection As TestChannelSelection
    Private currentTestChannels As List(Of Integer)
    Private ReadOnly testingChannelNumbers As New HashSet(Of Integer)()
    Public autoStartFlag As Boolean '自动测试标志
    Public haveNgPoint As Boolean '存在NG的点
    Public hjwd As String
    Public trayCode As String
    '存放校准值的文件：安装目录可能位于 WindowsApps，只读；改用当前用户可写目录。
    Private ReadOnly filePath As String = GetCalibrationFilePath()
    Dim alarmStatus As Boolean = False
    Dim logDirectory As String = AppLog.LogDirectory
    Dim logFilePath As String = Path.Combine(logDirectory, "Run.log")

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
        StartupTiming.Mark("frmTest-selection-controls")
        'R2-18:提前标记已加载完成,确保 frmTest_Resize 的布局调整始终执行
        '       (若 myConnection 未打开,后面 Exit Sub 会提前返回,原 _isLoaded=True 在末尾无法执行)
        _isLoaded = True
        '首屏只建立必要容器和尺寸，完整样式延后到第一次 Paint 之后。
        ApplyFrmTestEssentialLayout()
        StartupTiming.Mark("frmTest-essential-layout")
        '*******************************
        '显示相关信息
        '数据服务器字段已从主测试页移除。
        lblBarNum.Text = "*"
        '合格范围
        txtMaxV.Text = Initsetting.upVoltage
        txtMinV.Text = Initsetting.downVoltage
        txtMaxR.Text = Initsetting.upResistance
        txtMinR.Text = Initsetting.downResistance
        '**************************************
        ReadInitFile()
        StartupTiming.Mark("frmTest-read-init")
        ConfigureStationControls()
        StartupTiming.Mark("frmTest-station-controls")
        '探针相关
        txtTotle.Text = Initsetting.Probelifespan.ToString
        txtUsed.Text = Initsetting.ProbeUsedCount.ToString
        txtRemain.Text = (Initsetting.Probelifespan - Initsetting.ProbeUsedCount).ToString
        '设置ocv次数确认标志初始值为false，表示一次都没有跟客户确认过。
        isAskOcvNum = False

        StartupTiming.Mark("frmTest-combo-begin")
        For index = 1 To MAX_CHANNEL
            cmb_ChSelect.Items.Add(index)
        Next
        cmb_ChSelect.SelectedIndex = 0
        StartupTiming.Mark("frmTest-channel-combo")
        '读取校准值数组
        StartupTiming.Mark("frmTest-calibration-begin")
        Call CalibrateOperate()
        StartupTiming.Mark("frmTest-calibration")
        '初始化校准值数组
        ReDim calibrateValueArray(255)
        For i As Integer = 0 To MAX_CHANNEL - 1
            calibrateValueArray(i) = 0
        Next
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
        '初始化托盘状态为打开
        fixtureClose = False
        '获取分选条件项目到g_objCN
        btnStop.Enabled = False
        CmdStart.Enabled = True
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

        '先启动后台工作，所有连接、查询和轮询都不能阻塞首屏。
        If frmMDI IsNot Nothing AndAlso Not frmMDI.IsDisposed Then
            MainwindowsShowAsync(frmMDI)
            frmMDI.StartDatabaseHealthCheckAsync()
        End If

        System.Threading.ThreadPool.QueueUserWorkItem(Sub()
                                                          If ApplicationShuttingDown Then Return
                                                          Try
                                                              GetCN()
                                                          Catch ex As Exception
                                                              System.Diagnostics.Debug.WriteLine("后台加载分档字段失败: " & ex.Message)
                                                          End Try
                                                      End Sub)

        timer3.Interval = 1000
        timer3.Start()
        Timer4.Start()

        '完整样式不属于首屏必要路径，放在首次 Paint 之后执行。
        ApplyFrmTestDeferredVisualStyle()
        StartupTiming.Mark("frmTest-deferred-style")
        StartupTiming.Mark("frmTest-deferred-complete")
    End Sub

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
            button.BackColor = If(button Is activeButton, UiTheme.Primary, SystemColors.Control)
            button.ForeColor = If(button Is activeButton, Color.White, SystemColors.ControlText)
        Next
    End Sub

    Private Sub HandleSelectedChannelsChanged(ByVal sender As Object, ByVal e As EventArgs)
        If channelGrid Is Nothing Then Return
        SelectedChannelNumbers = channelGrid.SelectedChannelNumbers
        lblBarNum.Text = channelGrid.SelectedChannelCount.ToString()
    End Sub

    Private Sub SetSelectionControlsEnabled(ByVal enabled As Boolean)
        Dim buttons() As Button = {btnChannelSelectAll, btnChannelClear, btnChannelRectangle, btnChannelSelectSingle}
        For Each button As Button In buttons
            button.Enabled = enabled
        Next
        channelGrid.Enabled = enabled
    End Sub

    Public Function GetSignalTrayStatus() As Boolean
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
        '复位NG标志
        haveNgPoint = False
        '初始化复测数组
        ' ReDim retestArray(63)
        'frmTest_Resize1(）
        'Pic_Bat.Refresh()
        Sleep(1000)
        'frmInput.FreshBat2（2）
        If (alarmStatus = True) Then
            MessageBox.Show("目前处于报警状态，请先复位报警！", "提示", MessageBoxButtons.OK)
            Exit Sub
        End If
        If SelectedChannelNumbers Is Nothing OrElse SelectedChannelNumbers.Count = 0 Then
            MessageBox.Show("请先选择至少一个待测试通道！", "提示", MessageBoxButtons.OK)
            Exit Sub
        End If
        '启动测试
        Call StarTest(1)
    End Sub
    ''' <summary>
    ''' 启动测试：1：正常测试；0：NG补测
    ''' </summary>
    ''' <param name="testType"></param>
    ''' <returns></returns>
    Public Function StarTest(ByVal testType As Integer)
        StarTest = 0
        If SelectedChannelNumbers Is Nothing OrElse SelectedChannelNumbers.Count = 0 Then
            MessageBox.Show("请先选择至少一个待测试通道！", "提示", MessageBoxButtons.OK)
            Return StarTest
        End If
        'Dim intLoop As Int16
        Dim reFlag As Short
        Dim I As Int16
        Dim intOCV As Integer
        Dim strTrayNo As String
        Dim Buffer_SCM(5) As Byte
        Dim Buffer_Re(5) As Byte
        Dim isOpenOk As Boolean
        Dim mesUP As New ResultDataUploadAssembly(FrmMesTask.listBoxOutput)
        '清空报警信息
        txtAlarm.Text = ""
        txtAlarm.Refresh()
        'frmTest_Resize1(）
        Pic_Bat.Refresh()
        Sleep(100)

        frmMDI.Refresh()
        frmMDI.status_test.Text = ""
        frmMDI.txtTesting.Text = ""
        frmMDI.Refresh()
        '设定为测试状态
        isRuning = True
        SetSelectionControlsEnabled(False)
        '改变鼠标形状
        Me.Cursor = Cursors.WaitCursor
        '***********************************
        '初始化变量
        firstCellNumToTest = Val(frmInput.txtFirst.Text)
        lastCellNumToTest = Val(frmInput.txtLast.Text)
        totalCellNumToTest = Val(frmInput.txtLast.Text) - Val(frmInput.txtFirst.Text) + 1
        strTrayNo = Trim(txtTray.Text)
        Ftest = New Collection
        Stest = New Collection
        Ttest = New Collection
        intF = 0 : intS = 0 : intT = 0
        blstop = False
        '初始话保存测试结果的数组
        If (testType = 1) Then
            For I = 0 To MAX_CHANNEL - 1
                RecieveData(I).Voltage = 0
                RecieveData(I).Resistance = 0
                RecieveData(I).TestTime = Now
            Next
        End If

        If strTrayNo = "" Then
            MsgBox("托盘号为空，请完成扫码！", vbOKOnly, "提示")
            GoTo exit_sub

        Else
            Dim result2 As DialogResult
            If strTrayNo = lastTrayNo Then

                result2 = MessageBox.Show("托盘号与上次相同，是否继续执行程序？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question)

                If result2 = DialogResult.Cancel Then
                    ' 用户选择取消，退出程序
                    ' Application.Exit()
                    GoTo exit_sub
                ElseIf result2 = DialogResult.OK Then

                End If
            Else
                lastTrayNo = strTrayNo
            End If

        End If

        ' 记录日志
        LogRun(logFilePath, strTrayNo, "开始测试")
        '***********************************
        '第n次测试
        If Rbtn1.Checked = True Then
            intOCV = 1
        End If
        If Rbtn2.Checked = True Then
            intOCV = 2
        End If
        If Rbtn3.Checked = True Then
            intOCV = 3
        End If
        If intOCV <= 0 Then
            MsgBox("请选择OCV！", vbOKOnly, "提示")
            GoTo exit_sub
        End If
        ' Show a message box to confirm action
        Dim result As DialogResult
        If isAskOcvNum = False Then
            result = MessageBox.Show("你确定本次是测试 OCV" + intOCV.ToString() + "吗？", "请确认", MessageBoxButtons.OKCancel)
            If result = DialogResult.OK Then
                ' User clicked OK, proceed with the method execution
                isAskOcvNum = True
            Else
                ' User clicked Cancel or closed the message box, do nothing or show a message
                isAskOcvNum = False
                Exit Function
                ' You can add more code here if you want to handle the cancellation differently
            End If
        End If
        '***********************************
        '检测中位机串口通讯
        reFlag = CheckCom_SCM()
        If reFlag <> 1 Then
            MsgBox("单片机通讯失败！", vbOKOnly)
            GoTo exit_sub
        End If
        '启动测试前的准备工作：托盘由 PLC 控制
        If Not plcConnected OrElse plcController Is Nothing Then
            MsgBox("PLC通讯失败，禁止启动测试！", vbOKOnly, "提示")
            GoTo exit_sub
        End If
        GetSignalTrayStatus()
        If Not fixtureClose Then
            Try
                If Not plcController.CloseTray() Then GoTo exit_sub
                Sleep(9000)
            Catch
                MsgBox("PLC关闭托盘失败，测试停止！", vbOKOnly, "提示")
                GoTo exit_sub
            End Try
            GetSignalTrayStatus()
        End If
        If fixtureClose = True Then
        Else
            MsgBox("托盘未闭合！测试停止！", vbOKOnly, "提示")
            GoTo exit_sub
        End If
        'Exit Function
        'End If
        '处理探针的相关数据
        Dim fixtureDownForRun As Boolean = True
        Initsetting.ProbeUsedCount = Initsetting.ProbeUsedCount + 1
        txtUsed.Text = Initsetting.ProbeUsedCount.ToString
        txtRemain.Text = (Initsetting.Probelifespan - Initsetting.ProbeUsedCount).ToString
        If Initsetting.Probelifespan - Initsetting.ProbeUsedCount = 0 Then
            MsgBox("探针的使用次数以达到上限，请联系供应商更换探针！", vbOKOnly, "提示")
        End If

        frmMDI.txtTesting.Text = "测试中......"
        Sleep(100)
        frmMDI.Refresh()
        '暂时不允许控制托盘
        btnOpen.Enabled = False
        btnTrayState.Enabled = False
        btnClose.Enabled = False
        '本次测试必须从当前计划重新生成通道选择，不能沿用上一次 Runner 的结果。
        lastTestSelection = Nothing
        currentTestChannels = GetSelectedPhysicalChannels(testType)
        MarkSelectedChannelsTesting()
        Dim ocvTestTaskId As Long? = Nothing
        Dim testTaskStatusFinalized As Boolean = False
        Try
            ocvTestTaskId = CreateOcvDataTestTask(testType)
        Catch ex As Exception
            txtAlarm.AppendText($"OCV_DATA 创建测试任务异常: {ex.Message}{Environment.NewLine}")
            MsgBox("OCV_DATA 创建测试任务失败: " & ex.Message, vbOKOnly, "错误")
            GoTo exit_sub
        End Try
        '************** 正式测试：中位机只负责通道计划和 START，仪表缓存完成后统一回填 *********************
        Try
            Dim formalRunner As New FormalTestRunner()
            If testType = 0 Then
                lastTestSelection = formalRunner.RunRetestMeasurement(retestArray)
            ElseIf SelectedChannelNumbers IsNot Nothing AndAlso SelectedChannelNumbers.Count > 0 Then
                lastTestSelection = formalRunner.RunSparseMeasurement(SelectedChannelNumbers)
            Else
                lastTestSelection = formalRunner.RunRangeMeasurement(firstCellNumToTest, lastCellNumToTest)
            End If
        Catch ex As Exception
            CompleteOcvDataTestTaskSafely(ocvTestTaskId, "Failed", ex.Message)
            txtAlarm.AppendText($"正式测试异常: {ex.Message}{Environment.NewLine}")
            MsgBox("正式测试出现异常: " & ex.Message, vbOKOnly, "错误")
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
            '***********测试完成，松开托盘****************
            '松开托盘
            isOpenOk = TrayOpen()
            Sleep(2000)
            ' 记录日志
            LogRun(logFilePath, strTrayNo, "测试结束")
            '设置电池的测试结果：OK或者NG'设定结果数组中的各个点位的测试结果， 分三种情况
            '1）全测，遍历所有的点位，逐个判断进行设定；(1,2合并，都作为选定的部分）
            '2）部分测：只对选定的部分进行判断；
            '3）复测NG点，只对复测的点进行判断。
            If testType = 1 Then
                For Each channel As Integer In GetSelectedPhysicalChannels(testType)
                    Dim channelIndex0 As Integer = channel - 1
                    Dim recVol As Single = RecieveData(channelIndex0).Voltage
                    Dim recRes As Single = RecieveData(channelIndex0).Resistance
                    If IsNG(recVol, recRes) <> 0 Then
                        RecieveData(channelIndex0).Result = "NG"
                    Else
                        RecieveData(channelIndex0).Result = "OK"
                    End If
                Next
            Else '如果是NG复测的话，复测数组为1的通道才赋值。
                For j As Integer = 0 To retestArray.Length - 1
                    If retestArray(j) = 1 Then
                        Dim recVol As Single = RecieveData(j).Voltage
                        Dim recRes As Single = RecieveData(j).Resistance
                        If IsNG(recVol, recRes) <> 0 Then
                            RecieveData(j).Result = "NG"
                        Else
                            RecieveData(j).Result = "OK"
                        End If
                    End If
                Next
            End If
            testingChannelNumbers.Clear()
            RefreshChannelOverview()
            SaveOcvDataTestResults(ocvTestTaskId, testType)
            CompleteOcvDataTestTaskSafely(ocvTestTaskId, "Completed", Nothing)
            testTaskStatusFinalized = True
            '把数据保存在本都文件夹中（按天建立文件夹），每个托盘保存一个文件
            SaveTestDataLocalFile(RecieveData)
            '主程序调用数据上传接口
            If FrmMesTask.chkResultDataUploadAssembly.Checked Then
                '数据出站，上传到Mes
                mesUP.ChannelNumbers = GetSelectedPhysicalChannels(testType)
                mesUP.StopTimer()
                mesUP.StartTimer()
            End If
            '旧库 CW_tblData_Info 并行写入已删除 (R2-15):
            '测试结果已由 SaveOcvDataTestResults 写入 OCV_DATA.TestResult,
            '托盘已由 CreateOcvDataTestTask -> usp_RegisterTray 注册到 OCV_DATA.Tray。
        Catch ex As Exception
            txtAlarm.AppendText($"OCV/上传/数据库写入异常: {ex.Message}{Environment.NewLine}")
            MsgBox("OCV 测量或数据上传出现异常: " & ex.Message, vbOKOnly, "错误")
        End Try
        If ocvTestTaskId.HasValue AndAlso Not testTaskStatusFinalized Then
            CompleteOcvDataTestTaskSafely(ocvTestTaskId, "Failed", "Test result processing failed before task completion.")
            testTaskStatusFinalized = True
        End If
        frmMDI.status_test.Text = ""
        '如果存在NG点，询问是否重测坏点
        If haveNgPoint Then
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
        testingChannelNumbers.Clear()
        RefreshChannelOverview()
        If fixtureDownForRun Then
            Try
                TrayOpen()
            Catch ex As Exception
                txtAlarm.AppendText("Tray open cleanup failed: " & ex.Message & Environment.NewLine)
            Finally
                fixtureDownForRun = False
            End Try
        End If
        '恢复控制托盘
        btnOpen.Enabled = True
        btnTrayState.Enabled = True
        btnClose.Enabled = True
        SetSelectionControlsEnabled(True)
        Me.Cursor = Cursors.Default
        isRuning = False
        Return 0

    End Function

    Private Function CreateOcvDataTestTask(ByVal testType As Integer) As Long?
        Dim service As New OcvDataService()
        Dim channels As List(Of Integer) = GetSelectedPhysicalChannels(testType)
        Dim request As New OcvCreateTestTaskRequest With {
            .TaskCode = BuildOcvDataTaskCode(testType),
            .IdempotencyKey = BuildOcvDataIdempotencyKey(testType),
            .TrayCode = Trim(txtTray.Text),
            .OcvStageCode = GetOcvStageCode(),
            .TestType = If(testType = 0, "Retest", "Formal"),
            .SelectedChannelsJson = BuildChannelJson(channels),
            .FirstChannelNo = If(channels.Count > 0, CType(channels.Min(), Integer?), Nothing),
            .LastChannelNo = If(channels.Count > 0, CType(channels.Max(), Integer?), Nothing),
            .ChannelCount = channels.Count,
            .EquipmentCode = "XL_OCV_256CH",
            .WorkstationName = Environment.MachineName,
            .OperatorName = Initsetting.curOperator,
            .IsRetest = (testType = 0)
        }

        Dim table As DataTable = service.CreateTestTask(request)
        If table.Rows.Count = 0 OrElse IsDBNull(table.Rows(0)("TestTaskId")) Then
            Throw New InvalidOperationException("usp_CreateTestTask did not return TestTaskId.")
        End If

        Return Convert.ToInt64(table.Rows(0)("TestTaskId"))
    End Function

    Private Sub SaveOcvDataTestResults(ByVal testTaskId As Long?, ByVal testType As Integer)
        If Not testTaskId.HasValue Then
            Return
        End If

        Dim results As New List(Of OcvTestResultInput)()
        Dim channels As List(Of Integer) = GetSelectedPhysicalChannels(testType)
        For orderIndex As Integer = 0 To channels.Count - 1
            Dim channel As Integer = channels(orderIndex)
            Dim index0 As Integer = channel - 1
            If index0 < 0 OrElse index0 > UBound(RecieveData) Then
                Continue For
            End If

            Dim finalResistance As Decimal = Convert.ToDecimal(RecieveData(index0).Resistance)
            Dim calibrationValue As Decimal = Convert.ToDecimal(calibrateValueArray(index0))
            Dim rawResistance As Decimal = finalResistance - calibrationValue
            Dim ngCode As Integer = IsNG(RecieveData(index0).Voltage, RecieveData(index0).Resistance)

            results.Add(New OcvTestResultInput With {
                .ChannelNo = channel,
                .BatteryCode = RecieveData(index0).BatCode,
                .ExecutionOrder = orderIndex + 1,
                .VoltageVolt = Convert.ToDecimal(RecieveData(index0).Voltage),
                .RawResistanceMilliohm = rawResistance,
                .CalibrationValueMilliohm = calibrationValue,
                .FinalResistanceMilliohm = finalResistance,
                .IsNg = (ngCode <> 0),
                .NgReason = BuildNgReason(ngCode),
                .ResultSource = If(testType = 0, "Retest", "FormalTest")
            })
        Next

        Dim service As New OcvDataService()
        service.SaveTestResults(testTaskId.Value, results)
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
            txtAlarm.AppendText($"OCV_DATA 更新测试任务状态异常: {ex.Message}{Environment.NewLine}")
        End Try
    End Sub

    Private Function GetSelectedPhysicalChannels(ByVal testType As Integer) As List(Of Integer)
        If currentTestChannels IsNot Nothing AndAlso currentTestChannels.Count > 0 Then
            Return currentTestChannels.ToList()
        End If

        If lastTestSelection IsNot Nothing AndAlso
           lastTestSelection.SelectedPhysicalChannels IsNot Nothing AndAlso
           lastTestSelection.SelectedPhysicalChannels.Count > 0 Then
            Return lastTestSelection.SelectedPhysicalChannels.Distinct().OrderBy(Function(channel) channel).ToList()
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

    Private Function BuildOcvDataTaskCode(ByVal testType As Integer) As String
        Return String.Format("{0}-{1}-{2}-{3:yyyyMMddHHmmssfff}",
                             Trim(txtTray.Text),
                             GetOcvStageCode(),
                             If(testType = 0, "RETEST", "FORMAL"),
                             DateTime.Now)
    End Function

    Private Function BuildOcvDataIdempotencyKey(ByVal testType As Integer) As String
        Return String.Format("{0}|{1}|{2}|{3:yyyyMMddHHmmssfff}",
                             Trim(txtTray.Text),
                             GetOcvStageCode(),
                             If(testType = 0, "RETEST", "FORMAL"),
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
    Private Sub SaveTestDataLocalFile(recieveData() As Bat_info)
        ' 示例托盘号
        Dim trayNum As String = txtTray.Text

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

        ' 初始化测试数据数组，按 256 个物理通道保存。
        Dim testData(MAX_CHANNEL - 1) As TestDataInfo
        If currentOCVNum = 1 Then
            For i As Integer = 0 To MAX_CHANNEL - 1
                testData(i) = New TestDataInfo With {
                    .SmCode = recieveData(i).BatCode,
                    .Result = recieveData(i).Result,
                    .ChNum = i + 1,
                    .IR1 = recieveData(i).Resistance,
                    .OCV1 = recieveData(i).Voltage * 1000,
                    .Time1 = recieveData(i).TestTime,
                    .TA1 = t1.Text,
                    .TA2 = t2.Text,
                    .TA3 = t3.Text,
                    .TA4 = t4.Text
                }
            Next
        End If
        If currentOCVNum = 2 Then
            For i As Integer = 0 To MAX_CHANNEL - 1
                testData(i) = New TestDataInfo With {
                    .SmCode = recieveData(i).BatCode,
                    .Result = recieveData(i).Result,
                    .ChNum = i + 1,
                    .IR2 = recieveData(i).Resistance,
                    .OCV2 = recieveData(i).Voltage * 1000,
                    .Time2 = recieveData(i).TestTime,
                    .TA1 = t1.Text,
                    .TA2 = t2.Text,
                    .TA3 = t3.Text,
                    .TA4 = t4.Text
                }
            Next
        End If
        ' 填充测试数据


        ' 创建文件并写入数据
        Using writer As New StreamWriter(filePath)
            ' 写入文件头
            writer.WriteLine("电池条码,测试结果,通道号,内阻(mΩ),电压(mV),测试时间,TA1,TA2,TA3,TA4")

            ' 写入每一行数据
            If currentOCVNum = 1 Then
                For Each data As TestDataInfo In testData
                    writer.WriteLine($"{data.SmCode},{data.Result},{data.ChNum},{data.IR1},{data.OCV1},{data.Time1},{data.TA1},{data.TA2},{data.TA3},{data.TA4}")
                Next
            End If
            If currentOCVNum = 2 Then
                For Each data As TestDataInfo In testData
                    writer.WriteLine($"{data.SmCode},{data.Result},{data.ChNum},{data.IR2},{data.OCV2},{data.Time2},{data.TA1},{data.TA2},{data.TA3},{data.TA4}")
                Next
            End If
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

    Private Sub AppendAlarmText(line As String)
        txtAlarm.AppendText(line & Environment.NewLine)
    End Sub

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
                isPoint = True
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
                        txtAlarm.Invoke(New Action(Of String)(AddressOf AppendAlarmText), alarmLog)
                    Else
                        txtAlarm.AppendText(alarmLog & Environment.NewLine)
                    End If
                    Try
                        AppLog.Write("Alarm", alarmLog)
                    Catch ex As Exception
                        System.Diagnostics.Debug.WriteLine("AlarmLog write failed: " & ex.Message)
                    End Try
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
        blstop = True
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
        If shutdownPrepared OrElse ApplicationShuttingDown Then Return
        ' 这里写定时器触发后需要执行的代码
        '启动一个异步定时检测socket对象，检测温度和IO信息
        Dim cl As New SocketClient
        Dim bytesToSend As Byte() = cl.GetSignalTrayTempertureAndIOStatus()
        socketSR1(bytesToSend)
        '启动监测温度和JIG信号
        Call AnalyzeData()
        ' 注意：因为这是在独立线程上运行的，所以如果你需要更新UI，必须使用跨线程调用（例如：Invoke或BeginInvoke方法）  
    End Sub
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
                        alarmStatus = True
                        '关闭定时器
                        timer3.Close()
                        ' 显示一个警告消息框
                        MessageBox.Show("急停报警，请立刻停机检查", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        temp1 = mSock.GetSignalFixtureUp '松开夹具（打开托盘）
                        bytes1 = socketSR(temp1)
                        Dim timestamp As String = DateTime.Now.ToString()
                        ' 添加时间戳
                        Dim alarmLog As String = $"{timestamp}:急停报警 "   ' 添加时间戳和报警信息
                        txtAlarm.AppendText(alarmLog & Environment.NewLine)
                        Try
                            Dim filePath As String = "AlarmLog.txt"
                            ' 使用 StreamWriter 写入文件
                            Using writer As New System.IO.StreamWriter(filePath, True)
                                writer.WriteLine(DateTime.Now.ToString() & ": " & alarmLog)
                            End Using
                        Catch ex As Exception

                        End Try
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
                        txtAlarm.AppendText(alarmLog & Environment.NewLine)
                        '设置报警状态
                        alarmStatus = True
                        Try
                            Dim filePath As String = "AlarmLog.txt"
                            ' 使用 StreamWriter 写入文件
                            Using writer As New System.IO.StreamWriter(filePath, True)
                                writer.WriteLine(DateTime.Now.ToString() & ": " & alarmLog)
                            End Using
                        Catch ex As Exception

                        End Try
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
                        txtAlarm.AppendText(alarmLog & Environment.NewLine)
                        '设置报警状态
                        alarmStatus = True
                        Try
                            Dim filePath As String = "AlarmLog.txt"
                            ' 使用 StreamWriter 写入文件
                            Using writer As New System.IO.StreamWriter(filePath, True)
                                writer.WriteLine(DateTime.Now.ToString() & ": " & alarmLog)
                            End Using
                        Catch ex As Exception

                        End Try
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
        ' 创建一个整数类型的数组ARR2int，长度为4
        Dim TemperateList(3) As Integer
        ' 使用Array.Copy方法来复制字节数组的一部分,后8个字节为温度数据
        ' 参数分别是源数组，源数组的起始索引，目标数组，目标数组的起始索引，以及要复制的元素数量
        ' Array.Copy(bytes, bytes.Length - 8, TemperateList, 0, 4)
        TemperateList(0) = bytes(bytes.Length - 7) * 256 + bytes(bytes.Length - 8)
        TemperateList(1) = bytes(bytes.Length - 5) * 256 + bytes(bytes.Length - 6)
        TemperateList(2) = bytes(bytes.Length - 3) * 256 + bytes(bytes.Length - 4)
        TemperateList(3) = bytes(bytes.Length - 1) * 256 + bytes(bytes.Length - 2)
        Dim f As Single() = New Single(3) {}
        f(0) = TemperateList(0) * 0.1
        f(1) = TemperateList(1) * 0.1
        f(2) = TemperateList(2) * 0.1
        f(3) = TemperateList(3) * 0.1
        hjwd = f(0).ToString("00.00")
        trayCode = txtTray.Text
        Updatet1Text(f(0))
        Updatet2Text(f(1))
        Updatet3Text(f(2))
        Updatet4Text(f(3))
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
        PrepareForShutdown()
        '复位报警状态
        alarmStatus = False
        'clientSocket.Close()
        ' 测试结束，把校准值保存到文件。
        SaveCalibrateArrayToFile()
        '保存设置值文件
        Call SaveInitFile()
        '复位ocv次数确认标志。
        isAskOcvNum = False
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

    Public Sub PrepareForShutdown()
        If shutdownPrepared Then Return
        shutdownPrepared = True
        _isLoaded = False

        If timer3 IsNot Nothing Then timer3.Stop()
        If Timer1 IsNot Nothing Then Timer1.Stop()
        If Timer2 IsNot Nothing Then Timer2.Stop()
        If Timer4 IsNot Nothing Then Timer4.Stop()
        If channelGrid IsNot Nothing Then
            channelGrid.Visible = False
            channelGrid.SuspendLayout()
        End If
    End Sub

    Private Shared Function GetCalibrationFilePath() As String
        Dim dataDirectory As String = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "XL_OCV_256CH")
        Directory.CreateDirectory(dataDirectory)
        Return Path.Combine(dataDirectory, "CalibrateArr.csv")
    End Function

    Private Sub Timer4_Tick(sender As Object, e As EventArgs) Handles Timer4.Tick
        If chkAutoTest Is Nothing OrElse Not chkAutoTest.Checked Then Return
        If autoStartFlag = True Then
            If alarmStatus = True Then
                MessageBox.Show("目前处于报警状态，请先复位报警！", "提示", MessageBoxButtons.OK)
                Exit Sub
            End If

            If isRuning = False Then
                '复位NG标志
                haveNgPoint = False
                Call StarTest(1)
                isRuning = False
            End If
        End If
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

        txtJiajuR.Text = "3.75"
        txtCaliR.Text = (Resistance - Single.Parse(txtJiajuR.Text)).ToString()

        txtTestR.Text = Resistance.ToString()
        CalibrateR = Resistance - 3.75
        ''把数据保存到initset文件中
        Initsetting.sngR_B = CalibrateR
        '把校准数据保存在校准值数组calibrateValueArray(i)中
        calibrateValueArray(ch) = Single.Parse(txtJiajuR.Text)

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
    '通过检测温度变化，判读是否有超温报警。
    Private Sub t1_TextChanged(sender As Object, e As EventArgs) Handles t1.TextChanged
        If Single.Parse(t1.Text) > Initsetting.overTemperateValue Then
            Dim temp As Byte()
            '关闭定时器
            timer3.Close()
            '设置报警状态
            alarmStatus = True
            ' 显示一个警告消息框
            MessageBox.Show("超温报警，请立刻停机检查", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Dim bytes(500) As Byte
            mSock = frmDebug.mSock
            temp = mSock.GetSignalFixtureUp '松开夹具（打开托盘）
            bytes = socketSR(temp)
            Dim timestamp As String = DateTime.Now.ToString()    ' 添加时间戳
            Dim alarmLog As String = $"{timestamp}:超温报警 " & "温度探头 1 位置"  ' 添加时间戳和报警信息
            txtAlarm.AppendText(alarmLog & Environment.NewLine)
            Try
                Dim filePath As String = "AlarmLog.txt"
                ' 使用 StreamWriter 写入文件
                Using writer As New System.IO.StreamWriter(filePath, True)
                    writer.WriteLine(DateTime.Now.ToString() & ": " & alarmLog)
                End Using
            Catch ex As Exception

            End Try

            '' 调用 Application.Exit() 方法退出整个应用程序
            'Application.Exit()
        End If
        '启动监测温度和JIG信号
        Call checkJIGSignal()
    End Sub
    Private Sub t2_TextChanged(sender As Object, e As EventArgs) Handles t2.TextChanged
        If Single.Parse(t2.Text) > Initsetting.overTemperateValue Then
            Dim temp As Byte()
            '关闭定时器
            timer3.Close()
            '设置报警状态
            alarmStatus = True
            ' 显示一个警告消息框
            MessageBox.Show("超温报警，请立刻停机检查", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning)

            Dim bytes(500) As Byte
            temp = mSock.GetSignalFixtureUp '松开夹具（打开托盘）
            bytes = socketSR(temp)
            Dim timestamp As String = DateTime.Now.ToString()    ' 添加时间戳
            Dim alarmLog As String = $"{timestamp}:超温报警 " & "温度探头 2 位置"  ' 添加时间戳和报警信息
            txtAlarm.AppendText(alarmLog & Environment.NewLine)
            Try
                Dim filePath As String = "AlarmLog.txt"
                ' 使用 StreamWriter 写入文件
                Using writer As New System.IO.StreamWriter(filePath, True)
                    writer.WriteLine(DateTime.Now.ToString() & ": " & alarmLog)
                End Using
            Catch ex As Exception

            End Try
            '' 调用 Application.Exit() 方法退出整个应用程序
            'Application.Exit()
        End If
    End Sub

    Private Sub t3_TextChanged(sender As Object, e As EventArgs) Handles t3.TextChanged
        If Single.Parse(t3.Text) > Initsetting.overTemperateValue Then
            Dim temp As Byte()
            '关闭定时器
            timer3.Close()
            '设置报警状态
            alarmStatus = True
            ' 显示一个警告消息框
            MessageBox.Show("超温报警，请立刻停机检查", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Dim bytes(500) As Byte
            temp = mSock.GetSignalFixtureUp '松开夹具（打开托盘）
            bytes = socketSR(temp)
            Dim timestamp As String = DateTime.Now.ToString()    ' 添加时间戳
            Dim alarmLog As String = $"{timestamp}:超温报警 " & "温度探头 3 位置"  ' 添加时间戳和报警信息
            txtAlarm.AppendText(alarmLog & Environment.NewLine)
            Try
                Dim filePath As String = "AlarmLog.txt"
                ' 使用 StreamWriter 写入文件
                Using writer As New System.IO.StreamWriter(filePath, True)
                    writer.WriteLine(DateTime.Now.ToString() & ": " & alarmLog)
                End Using
            Catch ex As Exception

            End Try

            '' 调用 Application.Exit() 方法退出整个应用程序
            'Application.Exit()
        End If
    End Sub

    Private Sub t4_TextChanged(sender As Object, e As EventArgs) Handles t4.TextChanged
        If Single.Parse(t4.Text) > Initsetting.overTemperateValue Then
            Dim temp As Byte()
            '关闭定时器
            timer3.Close()
            '设置报警状态
            alarmStatus = True
            ' 显示一个警告消息框
            MessageBox.Show("超温报警，请立刻停机检查", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Dim bytes(500) As Byte
            temp = mSock.GetSignalFixtureUp '松开夹具（打开托盘）
            bytes = socketSR(temp)
            Dim timestamp As String = DateTime.Now.ToString()    ' 添加时间戳
            Dim alarmLog As String = $"{timestamp}:超温报警 " & "温度探头 4 位置"  ' 添加时间戳和报警信息
            txtAlarm.AppendText(alarmLog & Environment.NewLine)
            Try
                Dim filePath As String = "AlarmLog.txt"
                ' 使用 StreamWriter 写入文件
                Using writer As New System.IO.StreamWriter(filePath, True)
                    writer.WriteLine(DateTime.Now.ToString() & ": " & alarmLog)
                End Using
            Catch ex As Exception

            End Try
            '要不要退出整个程序
            '' 调用 Application.Exit() 方法退出整个应用程序
            'Application.Exit()
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        alarmStatus = False
        Dim timestamp As String = DateTime.Now.ToString()    ' 添加时间戳
        Dim alarmLog As String = $"{timestamp}:报警复位。"  ' 添加时间戳和报警信息
        txtAlarm.AppendText(alarmLog & Environment.NewLine)
        Try
            Dim filePath As String = "AlarmLog.txt"
            ' 使用 StreamWriter 写入文件
            Using writer As New System.IO.StreamWriter(filePath, True)
                writer.WriteLine(DateTime.Now.ToString() & ": " & alarmLog)
            End Using
        Catch ex As Exception

        End Try
    End Sub

    Private Sub txtLastNum_TextChanged(sender As Object, e As EventArgs) Handles txtLastNum.TextChanged
        avalibleCount = Val(txtLastNum.Text)
    End Sub

    Private Sub btnNGTest_Click(sender As Object, e As EventArgs) Handles btnNGTest.Click
        '复位NG标志
        If (alarmStatus = True) Then
            MessageBox.Show("目前处于报警状态，请先复位报警！", "提示", MessageBoxButtons.OK)
            Exit Sub
        End If
        haveNgPoint = False
        '启动NG测试
        Call StarTest(0)
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
        Dim row As Integer = (index Mod intRow) + 1
        Dim column As Integer = (index \ intRow) + 1
        Dim resultText As String = If(RecieveData(index).Result, String.Empty).Trim()
        Dim statusText As String
        If testingChannelNumbers.Contains(channelNo) Then
            statusText = "TESTING"
        ElseIf String.Equals(resultText, "NG", StringComparison.OrdinalIgnoreCase) Then
            statusText = "NG"
        ElseIf String.Equals(resultText, "OK", StringComparison.OrdinalIgnoreCase) OrElse String.Equals(resultText, "PASS", StringComparison.OrdinalIgnoreCase) Then
            statusText = "PASS"
        ElseIf String.IsNullOrWhiteSpace(RecieveData(index).BatCode) Then
            statusText = "IDLE"
        Else
            statusText = "WAITING"
        End If

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



    ''' <summary>
    ''' Applies the visual layer to existing controls without changing their
    ''' event bindings or business behavior.
    ''' </summary>
    Private Sub ApplyFrmTestVisualStyle()
        ApplyFrmTestEssentialLayout()
        ApplyFrmTestDeferredVisualStyle()
    End Sub

    Private Sub ApplyFrmTestEssentialLayout()
        MinimumSize = New Size(0, 0)
        If Panel1 Is Nothing Then Return

        SuspendLayout()
        Try
            '首屏不切换窗体字体/AutoScaleMode，避免设计器控件整体重新缩放。
            StartupTiming.Mark("frmTest-essential-before-controls")
            EnsureTestModeControls()
            ConfigureCompactLeftControls()
            StartupTiming.Mark("frmTest-essential-before-responsive")
            If responsiveLayout Is Nothing Then responsiveLayout = New ResponsiveTestLayout()
            responsiveLayout.Apply(Me, Panel1, statisticsControl, channelGrid, Pic_Bat, False)
            StartupTiming.Mark("frmTest-essential-after-responsive")
        Finally
            ResumeLayout(False)
        End Try
    End Sub

    Private Sub ApplyFrmTestDeferredVisualStyle()
        MinimumSize = New Size(0, 0)
        If Panel1 Is Nothing Then Return

        SuspendLayout()
        Try
            '结构化左侧布局不加入首屏后的同步工作，避免首次显示后再次长时间占用 UI 线程。
            '首屏先沿用设计器控件布局；后续 Resize/布局刷新时再由 ResponsiveTestLayout 处理。
            Me.BackColor = UiTheme.PageBackground
            Me.DoubleBuffered = True
            Panel1.BackColor = UiTheme.PageBackground
            Pic_Bat.BackColor = UiTheme.CardBackground

            Dim cards As GroupBox() = {
                grpTestMode, GroupBox1, GroupBox2, GroupBox3,
                GroupBox5, GroupBox6, GroupBox7, GroupBox8
            }
            For Each card As GroupBox In cards
                UiTheme.ApplyCardStyle(card)
            Next

            Dim inputs As TextBox() = {
                txtTray, txtFirstNum, txtLastNum,
                txtMinV, txtMaxV, txtMinR, txtMaxR,
                txtTestR, txtCaliR, txtJiajuR, txtAlarm,
                t1, t2, t3, t4, txtTotle, txtUsed, txtRemain
            }
            For Each input As TextBox In inputs
                UiTheme.ApplyTextBoxStyle(input)
            Next

            Dim fieldLabels As Label() = {
                Label2, Label5, Label7, Label12,
                Label1, Label14, Label15, Label16,
                Label19, Label17, Label28, Label26, Label29
            }
            For Each fieldLabel As Label In fieldLabels
                UiTheme.ApplyFieldLabelStyle(fieldLabel)
            Next

            UiTheme.ApplyCheckBoxStyle(chkAutoTest)
            UiTheme.ApplyCheckBoxStyle(chkManualTest)
            UiTheme.ApplyRadioButtonStyle(Rbtn1)
            UiTheme.ApplyRadioButtonStyle(Rbtn2)
            UiTheme.ApplyRadioButtonStyle(Rbtn3)

            UiTheme.ApplyButtonStyle(CmdStart, UiTheme.ButtonRole.Primary)
            UiTheme.ApplyButtonStyle(btnStop, UiTheme.ButtonRole.Danger)
            UiTheme.ApplyButtonStyle(btnNGTest, UiTheme.ButtonRole.Warning)
            UiTheme.ApplyButtonStyle(btnTrayState, UiTheme.ButtonRole.Neutral)
            UiTheme.ApplyButtonStyle(btnOpen, UiTheme.ButtonRole.Neutral)
            UiTheme.ApplyButtonStyle(btnClose, UiTheme.ButtonRole.Neutral)
            'UiTheme.ApplyButtonStyle(btn_ZeroAdjustment, UiTheme.ButtonRole.Neutral)
            UiTheme.ApplyButtonStyle(btn_ChCalibrate, UiTheme.ButtonRole.Primary)
            'UiTheme.ApplyButtonStyle(btnScanTray, UiTheme.ButtonRole.Neutral)
            UiTheme.ApplyButtonStyle(btnChannelSelectAll, UiTheme.ButtonRole.Primary)
            UiTheme.ApplyButtonStyle(btnChannelClear, UiTheme.ButtonRole.Neutral)
            UiTheme.ApplyButtonStyle(btnChannelRectangle, UiTheme.ButtonRole.Neutral)
            UiTheme.ApplyButtonStyle(btnChannelSelectSingle, UiTheme.ButtonRole.Neutral)
            UiTheme.ApplyButtonStyle(Button2, UiTheme.ButtonRole.Danger)
            If responsiveLayout IsNot Nothing Then responsiveLayout.RefreshLeftContent(Panel1)
        Finally
            ResumeLayout(False)
        End Try
    End Sub

    Private Sub EnsureTestModeControls()
        If grpTestMode IsNot Nothing Then Return

        grpTestMode = New GroupBox With {
            .Name = "grpTestMode",
            .Text = "测试控制",
            .BackColor = UiTheme.CardBackground,
            .Size = New Size(306, 42),
            .TabStop = False
        }
        chkAutoTest = New CheckBox With {
            .Name = "chkAutoTest",
            .Text = "自动",
            .AutoSize = True,
            .Checked = True,
            .Location = New Point(55, 15),
            .TabIndex = 0
        }
        chkManualTest = New CheckBox With {
            .Name = "chkManualTest",
            .Text = "手动",
            .AutoSize = True,
            .Checked = False,
            .Location = New Point(190, 15),
            .TabIndex = 1
        }
        AddHandler chkAutoTest.CheckedChanged, AddressOf TestModeCheckedChanged
        AddHandler chkManualTest.CheckedChanged, AddressOf TestModeCheckedChanged
        grpTestMode.Controls.Add(chkAutoTest)
        grpTestMode.Controls.Add(chkManualTest)
        Panel1.Controls.Add(grpTestMode)
        SetTestMode(True)
    End Sub

    Private Sub TestModeCheckedChanged(ByVal sender As Object, ByVal e As EventArgs)
        If modeChangeInProgress Then Return

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
        Finally
            modeChangeInProgress = False
            UpdateTestModeVisuals()
        End Try
    End Sub

    Private Sub SetTestMode(ByVal autoMode As Boolean)
        If chkAutoTest Is Nothing OrElse chkManualTest Is Nothing Then Return

        modeChangeInProgress = True
        Try
            chkAutoTest.Checked = autoMode
            chkManualTest.Checked = Not autoMode
        Finally
            modeChangeInProgress = False
            UpdateTestModeVisuals()
        End Try
    End Sub

    Private Sub UpdateTestModeVisuals()
        If chkAutoTest Is Nothing OrElse chkManualTest Is Nothing Then Return
        UiTheme.ApplyToggleStateStyle(chkAutoTest, chkAutoTest.Checked)
        UiTheme.ApplyToggleStateStyle(chkManualTest, chkManualTest.Checked)
    End Sub

    Private Sub ConfigureStationControls()
        Rbtn1.Text = "OCV0"
        Rbtn2.Text = "OCV1"
        Rbtn3.Text = "OCV2"
        Rbtn1.Visible = True
        Rbtn2.Visible = True
        Rbtn3.Visible = True
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
            Case 2
                Rbtn3.Checked = True
            Case Else
                Rbtn1.Checked = True
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

    Private Sub ConfigureCompactLeftControls()
        '控件的父子关系和尺寸由 ResponsiveTestLayout 的结构化布局统一管理。
        '这里仅保留报警复位按钮的视觉尺寸，避免恢复旧的固定坐标布局。
        If Button2 IsNot Nothing Then Button2.Size = New Size(133, 34)
    End Sub

    Private Sub ResetLeftPanelScroll()
        If Panel1 Is Nothing OrElse Panel1.IsDisposed Then Return
        Panel1.AutoScrollPosition = New Point(0, 0)
    End Sub

    Private Sub SetTestStatus(ByVal message As String)
        If frmMDI Is Nothing OrElse frmMDI.IsDisposed Then Return
        If frmMDI.txtTesting IsNot Nothing Then
            frmMDI.txtTesting.Text = If(message, String.Empty)
        End If
    End Sub
End Class
