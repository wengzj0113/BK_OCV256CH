Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Net.Sockets
Imports System.Net
Imports System.Threading.Thread
Imports MpFByteArrDLL
Imports CmdMemberDLL
Imports System.Threading
Imports System.Reflection.Emit

Public Class MDIForm
    Private ngRetestForm As FrmNgRetest
    Private applyingMdiClientLayout As Boolean
    Private mdiLayoutOrderApplied As Boolean
    Private mdiChildCreationInProgress As Boolean
    Private ReadOnly resizeDebounceTimer As New System.Windows.Forms.Timer()
    Private databaseHealthCheckStarted As Integer
    Private startupDeviceFailurePromptShown As Integer
    'Dim adoCnn As SqlConnection
    Public mSock As SocketCtrolCmd
    Public FrmLink As New List(Of MDIForm)
    Public mSocket As Socket
    Public fixtureClose As Boolean
    Public operTypeNum As Integer = 0
    Private environmentResetStarted As Boolean
    Private Const ShutdownResetTimeoutMs As Integer = 3000
    Private Const ResizeDebounceMs As Integer = 75

    Private Sub OpenFile(ByVal sender As Object, ByVal e As EventArgs)
        '已删除的死代码占位 (R2-15)
    End Sub
    Private Sub SaveAsToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        '已删除的死代码占位 (R2-15)
    End Sub
    Private Sub CutToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        '已删除的死代码占位 (R2-15)
    End Sub

    Private Sub CopyToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        '已删除的死代码占位 (R2-15)
    End Sub

    Private Sub PasteToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        '已删除的死代码占位 (R2-15)
    End Sub

    Private Sub ToolBarToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        '旧工具栏菜单项已从 Designer 移除，保留事件入口以兼容历史调用。
    End Sub

    Private Sub CascadeToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        Me.LayoutMdi(MdiLayout.Cascade)
    End Sub

    Private Sub TileVerticalToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        Me.LayoutMdi(MdiLayout.TileVertical)
    End Sub

    Private Sub TileHorizontalToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        Me.LayoutMdi(MdiLayout.TileHorizontal)
    End Sub

    Private Sub ArrangeIconsToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        Me.LayoutMdi(MdiLayout.ArrangeIcons)
    End Sub

    Private Sub CloseAllToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs)
        ' 鍏抽棴姝ょ埗绐椾綋鐨勬墍鏈夊瓙绐椾綋銆?
        For Each ChildForm As Form In Me.MdiChildren
            ChildForm.Close()
        Next
    End Sub
    Private m_ChildFormNumber As Integer
    Private Sub init_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles init.Click
        If Not AuthorizationService.RequireAdministrator(Me) Then Return
        Me.Cursor = Cursors.WaitCursor
        Try
            Using initForm As New frmInit()
                initForm.ShowDialog(Me)
            End Using
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub
    Private Sub OnLineTest_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OnLineTest.Click
        Me.Cursor = Cursors.WaitCursor
        Dim obj_frmTest As Form
        Dim I, J As Int16
        I = -1
        J = -1
        Try
            '首次创建 MDI 子窗体前先固定菜单栏/底部状态栏边界，
            '避免子窗体在 MdiClient 尚未完成布局时缓存过大的最大化客户区。
            ApplyMdiClientLayout()
            mdiChildCreationInProgress = True
            Try
                For I = 0 To UBound(Me.MdiChildren)
                    If Me.MdiChildren(I).Text.ToString = "测试" Then
                        J = I
                        Exit For
                    End If
                Next
                If J = -1 Then
                    obj_frmTest = New frmTest
                    obj_frmTest.MdiParent = Me
                Else
                    obj_frmTest = Me.MdiChildren(J)
                End If
                obj_frmTest.Show()
                '保持首次 Show 的 WinForms 初始化路径；显示后在已经稳定的 MDI
                '客户区内最大化，避免未显示子窗体时触发递归布局。
                obj_frmTest.WindowState = FormWindowState.Maximized
                obj_frmTest.Activate()
            Finally
                mdiChildCreationInProgress = False
            End Try
            ApplyMdiClientLayout()
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
        Me.Cursor = Cursors.Default
    End Sub
    Private Sub ExitTool_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitTool.Click
        Me.Dispose()
    End Sub
    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim obj_frmAbout As Form
        obj_frmAbout = New frmAbout
        obj_frmAbout.ShowDialog()
    End Sub

    Private Sub btnTest_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTest.Click
        OnLineTest_Click(sender, e)
    End Sub

    Private Sub ManualTestToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ManualTestToolStripMenuItem.Click
        If Not AuthorizationService.RequireAuthenticatedUser(Me) Then Return
        Using manualForm As New FrmManualTest()
            manualForm.ShowDialog(Me)
        End Using
    End Sub

    Private Sub NgRetestToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NgRetestToolStripMenuItem.Click
        If Not AuthorizationService.RequireAuthenticatedUser(Me) Then Return
        If ngRetestForm Is Nothing OrElse ngRetestForm.IsDisposed Then
            ngRetestForm = New FrmNgRetest()
        End If
        ngRetestForm.Show(Me)
        ngRetestForm.Activate()
    End Sub

    Private Sub LoginToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles loginMenu.Click
        Using login As New FrmLoginNew()
            If login.ShowDialog(Me) <> DialogResult.OK Then Return
        End Using
        ApplyOperatorPermissions()
        UpdateBottomStatus()
    End Sub

    Private Sub logoutMenu_Click(sender As Object, e As EventArgs) Handles logoutMenu.Click
        AuthorizationService.ResetSession()
        ApplyOperatorPermissions()
        UpdateBottomStatus()
    End Sub

    Private Sub ApplyOperatorPermissions()
        Dim businessFunctionsEnabled As Boolean = AuthorizationService.CanAccess(AppPermission.UseBusinessFunctions)
        btnInit.Enabled = AuthorizationService.CanAccess(AppPermission.InitialSettings)
        ToolDebug.Enabled = AuthorizationService.CanAccess(AppPermission.Debug)
        init.Enabled = AuthorizationService.CanAccess(AppPermission.InitialSettings)
        btnDebug.Enabled = AuthorizationService.CanAccess(AppPermission.Debug)
        userMenage.Enabled = AuthorizationService.CanAccess(AppPermission.UserManagement)
        SetTestEntryEnabled(businessFunctionsEnabled)
        logoutMenu.Enabled = AuthorizationService.IsAuthenticated()
        loginMenu.Text = If(AuthorizationService.IsAuthenticated(), "切换用户", "登录")
        operTypeNum = CInt(AuthorizationService.CurrentRole)
    End Sub

    Private Sub btnExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.Close()
    End Sub

    Private Sub MDIForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If environmentResetStarted Then Return
        environmentResetStarted = True
        ApplicationShuttingDown = True

        '先停止 frmTest 的计时器和通道刷新，避免关闭 MDI 时 256 个方块反复布局。
        For Each child As Form In MdiChildren
            Dim testForm As frmTest = TryCast(child, frmTest)
            If testForm IsNot Nothing Then testForm.PrepareForShutdown()
        Next
        resizeDebounceTimer.Stop()

        Try
            Dim middleMachine As SerialMiddleMachineClient = Nothing
            If Com_SCM IsNot Nothing AndAlso Com_SCM.IsOpen Then
                middleMachine = New SerialMiddleMachineClient(Com_SCM)
            End If
            TestEnvironmentResetService.ResetAll(middleMachine, True, ShutdownResetTimeoutMs)
        Catch ex As Exception
            SerialMiddleMachineClient.LogMessage("Application exit reset exception: " & ex.Message)
        End Try
    End Sub

    Private Sub MDIForm_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        On Error Resume Next
        resizeDebounceTimer.Stop()
        resizeDebounceTimer.Dispose()
        If ngRetestForm IsNot Nothing AndAlso Not ngRetestForm.IsDisposed Then
            ngRetestForm.Close()
        End If
        '鍏抽棴绔彛鍐呴樆浠拰鍗曠墖鏈轰笌PC鐨勯€氳绔彛
        If Com_3561.IsOpen Then
            Com_3561.Close()
        End If
        If Com_SCM.IsOpen Then
            Com_SCM.Close()
        End If
        ''鍒犻櫎鐢熸垚鐨凚arCode鏂囦欢
        'File.Delete(Application.StartupPath & "\bar.set")

    End Sub
    Public Sub UpdateTextBoxValue(newValue As String)
        txtTesting.Text = newValue
    End Sub
    Private Sub MDIForm_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        StartupTiming.Mark("mdi-load")
        resizeDebounceTimer.Interval = ResizeDebounceMs
        AddHandler resizeDebounceTimer.Tick, AddressOf ResizeDebounceTimer_Tick
        ApplyReferenceChrome()
        EnsureBottomStatusControl()
        ApplyMdiClientLayout()

        'R2-17:数据库检查改成异步 (在 Shown 事件里 Task.Run),Load 阶段只显示占位状态
        blnOnLine = False

        ApplyOperatorPermissions()

        'R2-15: myClsDB 已随 clsDB 一起淘汰,Dispose 死代码删除 (数据库生命周期由 OcvDataService 每次 Using 管理)
        mSock = frmDebug.mSock
        mSocket = frmDebug.mSocket
    End Sub

    Private Sub MDIForm_Layout(ByVal sender As Object, ByVal e As LayoutEventArgs) Handles Me.Layout
        If ApplicationShuttingDown Then Return
        'Layout 事件可能在一次 Resize 中连续触发；统一交给防抖计时器，
        '避免每个中间尺寸都同步重算 MDI 客户区。
        QueueMdiLayoutSync()
    End Sub

    Private Sub MDIForm_Resize(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Resize
        If ApplicationShuttingDown Then Return
        QueueMdiLayoutSync()
    End Sub

    Private Sub QueueMdiLayoutSync()
        If ApplicationShuttingDown OrElse IsDisposed OrElse Not IsHandleCreated Then Return
        resizeDebounceTimer.Stop()
        resizeDebounceTimer.Start()
    End Sub

    Private Sub ResizeDebounceTimer_Tick(sender As Object, e As EventArgs)
        resizeDebounceTimer.Stop()
        If ApplicationShuttingDown OrElse IsDisposed Then Return

        ApplyMdiClientLayout()
    End Sub

    Private Sub ApplyMdiClientLayout()
        If ApplicationShuttingDown OrElse bottomStatusControl Is Nothing OrElse applyingMdiClientLayout OrElse mdiChildCreationInProgress Then Return

        Dim mdiClient As MdiClient = Nothing
        For Each child As Control In Controls
            If TypeOf child Is MdiClient Then
                mdiClient = DirectCast(child, MdiClient)
                Exit For
            End If
        Next
        If mdiClient Is Nothing Then Return

        '让 WinForms 按 Dock 顺序自然计算 MDI 客户区，避免缓存过期的 Bounds。
        applyingMdiClientLayout = True
        Try
            If MenuStrip.Dock <> DockStyle.Top Then MenuStrip.Dock = DockStyle.Top
            If bottomStatusControl.Dock <> DockStyle.Bottom Then bottomStatusControl.Dock = DockStyle.Bottom
            If mdiClient.Dock <> DockStyle.Fill Then mdiClient.Dock = DockStyle.Fill

            If Not mdiLayoutOrderApplied Then
                '多个 Dock = Top/Bottom 控件的上下位置取决于 Controls 集合顺序。
                'BringToFront 会把 MenuStrip 移到错误的索引，导致运行时 ToolStrip 出现在菜单上方。
                '按设计器中的顺序显式设置索引：菜单在工具栏上方，底部状态控件位于最下方。
                Controls.SetChildIndex(ToolStrip, 0)
                Controls.SetChildIndex(MenuStrip, 1)
                Controls.SetChildIndex(bottomStatusControl, 2)
                Controls.SetChildIndex(mdiClient, Controls.Count - 1)
                mdiLayoutOrderApplied = True
            End If
        Finally
            applyingMdiClientLayout = False
        End Try
    End Sub

    Private Sub ApplyReferenceChrome()
        MenuStrip.Renderer = New HeaderMenuRenderer()
    End Sub

    Private Sub EnsureBottomStatusControl()
        UpdateBottomStatus()
    End Sub

    Private Sub UpdateBottomStatus()
        If bottomStatusControl Is Nothing Then Return
        bottomStatusControl.UpdateStatus(
            "OCV1",
            If(controlBoardConnected, "已连接", "未连接"),
            If(plcConnected, "已连接", "未连接"),
            If(dmm34461Connected, "已连接", "未连接"),
            If(resistance3561Connected AndAlso Com_3561 IsNot Nothing AndAlso Com_3561.IsOpen, "已连接", "未连接"),
            If(databaseConnected, "已连接", "未连接"),
            If(mesConnected, "已连接", "未连接"),
            If(String.IsNullOrWhiteSpace(AuthorizationService.CurrentUserName), "未登录", AuthorizationService.CurrentUserName),
            DateTime.Now)
    End Sub

    Private dmm34461Connected As Boolean
    Private resistance3561Connected As Boolean
    Private mesConnected As Boolean
    Private startupExternalChecksStarted As Integer

    '供启动期后台设备检查完成后刷新底部状态控件。
    Public Sub UpdateDeviceCheckStatus(ByVal dmmStatus As Short,
                                       ByVal resistanceStatus As Short,
                                       ByVal mesStatus As Short)
        '现有设备检查函数的成功码不同：34461=0，3561=1，MES=1。
        dmm34461Connected = (dmmStatus = 0)
        resistance3561Connected = (resistanceStatus = 1)
        mesConnected = (mesStatus = 1)
        UpdateBottomStatus()
    End Sub

    ''' <summary>
    ''' Displays one startup summary for failed external-device checks.
    ''' </summary>
    Public Sub ShowStartupDeviceFailurePrompt(ByVal result As StartupDeviceCheckResult)
        If result Is Nothing OrElse Not result.HasFailures Then Return
        If Threading.Interlocked.Exchange(startupDeviceFailurePromptShown, 1) = 1 Then Return

        Dim message As New System.Text.StringBuilder()
        message.AppendLine("启动设备检查发现以下设备未连接：")
        If result.Scm <> 1 Then
            AppendStartupFailure(message, result, "SCM", "控制模块（COM" & (Initsetting.SCMComPort + 1).ToString() & "）")
        End If
        If result.Dmm34461 <> 0 Then
            AppendStartupFailure(message, result, "34461", "数字万用表（" & DisplayOrDefault(Initsetting.IpAddOf34461, "地址未配置") & "）")
        End If
        If result.Resistance3561 <> 1 Then
            AppendStartupFailure(message, result, "3561", "内阻仪（COM" & (Initsetting.Recom + 1).ToString() & "）")
        End If
        If result.Plc <> 1 Then
            AppendStartupFailure(message, result, "PLC", FormatPlcDisplayName())
        End If
        If result.Mes <> 1 Then
            AppendStartupFailure(message, result, "MES", "MES（" & DisplayOrDefault(Initsetting.MesApiAddress, "地址未配置") & "）")
        End If

        message.AppendLine()
        message.Append("请检查设备供电、通信线缆、串口号及通信参数后再进行测试。")
        MessageBox.Show(Me,
                        message.ToString(),
                        "设备连接检查",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning)
    End Sub

    Private Function FormatPlcDisplayName() As String
        If Initsetting.PLCMode = 1 Then
            Dim port As Integer = If(Initsetting.PLCTcpPort > 0, Initsetting.PLCTcpPort, 502)
            Return "PLC（TCP " & DisplayOrDefault(Initsetting.PLCIP, "地址未配置") & ":" & port.ToString() & "）"
        End If
        Return "PLC（COM" & (Initsetting.PLCComPort + 1).ToString() & "）"
    End Function

    Private Shared Function DisplayOrDefault(ByVal value As String, ByVal defaultValue As String) As String
        If String.IsNullOrWhiteSpace(value) Then Return defaultValue
        Return value.Trim()
    End Function

    Private Shared Sub AppendStartupFailure(ByVal message As System.Text.StringBuilder,
                                             ByVal result As StartupDeviceCheckResult,
                                             ByVal key As String,
                                             ByVal displayName As String)
        Dim reason As String = Nothing
        If Not result.FailureReasons.TryGetValue(key, reason) OrElse String.IsNullOrWhiteSpace(reason) Then
            reason = "检查返回失败"
        End If
        message.AppendLine("- " & displayName & "：" & reason)
    End Sub

    ''' <summary>
    ''' Starts the external-device and database checks once after the first MDI page is shown.
    ''' </summary>
    Public Sub StartExternalDeviceChecksAsync()
        If ApplicationShuttingDown OrElse IsDisposed Then Return
        If Threading.Interlocked.Exchange(startupExternalChecksStarted, 1) = 1 Then Return

        MainwindowsShowAsync(Me)
        StartDatabaseHealthCheckAsync()
    End Sub

    'Shown 只负责调度测试页；数据库检查由首屏后的后台阶段启动。
    Private Sub MDIForm_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        StartupTiming.Mark("mdi-shown")

        'frmTest 显示放到消息队列，不阻塞 Shown。
        Try
            Me.BeginInvoke(New Action(Sub()
                                          OnLineTest_Click(sender, e)
                                          '窗体已经显示并完成首个 MDI 子窗体创建后，可靠启动设备/数据库检查。
                                          '不能只依赖 frmTest.Paint：MDI 子窗体首次显示时该事件可能不会再次触发。
                                          StartExternalDeviceChecksAsync()
                                          'frmTest 在此回调中才真正创建并最大化;再排队一次,确保它读取到
                                          'Shown 完成后的最终 MdiClient 客户区,而不是创建瞬间的旧尺寸。
                                          Me.BeginInvoke(New MethodInvoker(AddressOf NormalizeInitialMdiChildLayout))
                                      End Sub))
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("OnLineTest BeginInvoke 失败: " & ex.Message)
        End Try
    End Sub

    Private Sub NormalizeInitialMdiChildLayout()
        If ApplicationShuttingDown OrElse IsDisposed Then Return

        ApplyMdiClientLayout()
    End Sub

    ''' <summary>
    ''' Starts database health checking after the first page has been painted.
    ''' </summary>
    Public Sub StartDatabaseHealthCheckAsync()
        If ApplicationShuttingDown OrElse IsDisposed Then Return
        If Threading.Interlocked.Exchange(databaseHealthCheckStarted, 1) = 1 Then Return

        Threading.ThreadPool.QueueUserWorkItem(Sub()
                                                   Dim ready As Boolean = False
                                                   Try
                                                       StartupTiming.Mark("database-check-start")
                                                       ready = CheckOcvDataHealth()
                                                   Catch ex As Exception
                                                       System.Diagnostics.Debug.WriteLine("CheckOcvDataHealth 异常: " & ex.Message)
                                                   End Try

                                                   StartupTiming.Mark("database-check-end", If(ready, "connected", "disconnected"))
                                                   Try
                                                       If Me.IsDisposed Then Return
                                                       Me.BeginInvoke(New Action(Sub()
                                                                                     If Me.IsDisposed Then Return
                                                                                     databaseConnected = ready
                                                                                     UpdateBottomStatus()
                                                                                 End Sub))
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("数据库状态回调失败: " & ex.Message)
            End Try
        End Sub)
    End Sub

    Private Function MakeSock()
        Dim mClientTCP = New TcpClient()
        Return mClientTCP.Client     ' New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    End Function

    Private Function CheckOcvDataHealth() As Boolean
        '此函数只读取数据库健康状态；UI 更新由 StartDatabaseHealthCheckAsync 统一调度。
        Try
            Dim service As New OcvDataService()
            Dim health As OcvDatabaseHealthResult = service.CheckDatabaseHealth()
            If health.IsSuccess Then
                databaseConnected = True
                Return True
            End If

            databaseConnected = False
            blnOnLine = False
            System.Diagnostics.Debug.WriteLine("OCV_DATA 健康检查未通过: " & health.Message)
            Return False
        Catch ex As Exception
            databaseConnected = False
            blnOnLine = False
            System.Diagnostics.Debug.WriteLine("OCV_DATA 健康检查异常: " & ex.Message)
            Return False
        End Try
    End Function

    Private Sub SetTestEntryEnabledSafe(ByVal enabled As Boolean)
        If Me.IsDisposed Then Return
        If Me.InvokeRequired Then
            Me.BeginInvoke(New Action(Of Boolean)(AddressOf SetTestEntryEnabledSafe), enabled)
            Return
        End If
        OnLineTest.Enabled = enabled
    End Sub

    Private Sub SetTestEntryEnabled(ByVal enabled As Boolean)
        OnLineTest.Enabled = enabled
        btnTest.Enabled = enabled
        ManualTestToolStripMenuItem.Enabled = enabled
    End Sub

    Private Class HeaderMenuRenderer
        Inherits ToolStripProfessionalRenderer

        Protected Overrides Sub OnRenderItemText(ByVal e As ToolStripItemTextRenderEventArgs)
            If TypeOf e.Item.Owner Is MenuStrip Then
                e.TextColor = Color.White
            End If
            MyBase.OnRenderItemText(e)
        End Sub

        Protected Overrides Sub OnRenderMenuItemBackground(ByVal e As ToolStripItemRenderEventArgs)
            If TypeOf e.Item.Owner Is MenuStrip Then
                Dim background As Color = If(e.Item.Selected, UiTheme.HeaderHighlight, UiTheme.HeaderBackground)
                Using brush As New SolidBrush(background)
                    e.Graphics.FillRectangle(brush, e.Item.ContentRectangle)
                End Using
                Return
            End If
            MyBase.OnRenderMenuItemBackground(e)
        End Sub
    End Class

    Private Function GetRomteEndPoint() As IPEndPoint
        Dim myIP As IPAddress = IPAddress.Parse(Initsetting.SCMIpAdd)
        Dim ipe As IPEndPoint = New IPEndPoint(myIP, Initsetting.SCM_port)
        Return ipe
    End Function
    Private Sub btnDebug_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDebug.Click
        If Not AuthorizationService.RequireAdministrator(Me) Then Return
        Me.Cursor = Cursors.WaitCursor
        If Not My.Forms.frmDebug.IsDisposed Then
            My.Forms.frmDebug.Dispose()
        End If
        My.Forms.frmDebug.Show()
        My.Forms.frmDebug.Activate()
        Me.Cursor = Cursors.Default
    End Sub
    Private Sub ToolStripButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton1.Click
        frmMDI.Dispose()
    End Sub

    Private Sub ToolDebug_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolDebug.Click
        If Not AuthorizationService.RequireAdministrator(Me) Then Return
        frmDebug.Show()
    End Sub
    Private Sub ToolInit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        init_Click(sender, e)
    End Sub

    Private Sub btnInit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnInit.Click
        If Not AuthorizationService.RequireAdministrator(Me) Then Return
        init_Click(sender, e)
    End Sub
    Private Sub TStripbtnInQuery_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TStripbtnInQuery.Click
        Me.Cursor = Cursors.WaitCursor
        My.Forms.frmInquery.ShowDialog()
        Me.Cursor = Cursors.Default
    End Sub
    Private Sub TStripMenuCode_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TStripMenuCode.Click
        Me.Cursor = Cursors.WaitCursor
        If Not My.Forms.frmInput.IsDisposed Then
            My.Forms.frmInput.Dispose()
        End If
        My.Forms.frmInput.Show()
        My.Forms.frmInput.Activate()
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub TlStripMenuG_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TlStripMenuG.Click
        Me.Cursor = Cursors.WaitCursor
        If Not My.Forms.frmInquery.IsDisposed Then
            My.Forms.frmInquery.Dispose()
        End If
        My.Forms.frmInquery.Show()
        My.Forms.frmInquery.Activate()
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub TlStripMenuLog_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TlStripMenuLog.Click
        Using logQuery As New FrmLogQuery()
            logQuery.ShowDialog(Me)
        End Using
    End Sub

    Private Sub TStripbtn_Barcode_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TStripbtn_Barcode.Click
        Me.Cursor = Cursors.WaitCursor
        If Not My.Forms.frmInput.IsDisposed Then
            My.Forms.frmInput.Dispose()
        End If
        My.Forms.frmInput.ShowDialog()
        My.Forms.frmInput.Activate()
        Me.Cursor = Cursors.Default

        '鎵樼洏缂栫爜鑷姩鏇存柊鍒拌仈鏈虹晫闈?
        My.Forms.frmTest.txtTray.Text = My.Forms.frmInput.txtTray.Text
        '鏄剧ず棣栧熬鏉″舰鐮?
        My.Forms.frmTest.txtFirstNum.Text = My.Forms.frmInput.txtFirst.Text
        My.Forms.frmTest.txtLastNum.Text = My.Forms.frmInput.txtLast.Text
        ''鏄剧ず鐢垫睜鎬绘暟
        'If My.Forms.frmTest.intNumber > 0 Then
        '    lblBarNum.Text = intNumber
        'Else
        '    lblBarNum.Text = "*"
        'End If
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        ReadInitFile()
    End Sub

    Private Sub SetSock(ByVal Sock As SocketCtrolCmd)
        mSock = Sock
        For i As Integer = 0 To FrmLink.Count - 1
            FrmLink(i).SetSock(mSock)
        Next
    End Sub
    Private Function ConnectTCP(ByVal mSocket As Socket, ByVal ipe As IPEndPoint) As String
        Try
            Dim connResult As IAsyncResult = mSocket.BeginConnect(ipe, Nothing, Nothing)
            connResult.AsyncWaitHandle.WaitOne(800, True)
            If (Not connResult.IsCompleted) Then
                mSocket.Close()
                Throw New Exception(ipe.ToString() & "鏃犳硶杩炴帴")
            End If
            Return ""

        Catch ex As Exception
            Return ex.Message
        End Try
    End Function
    Protected Sub ParseBuffer(ByVal port As Integer, ByVal buffer As FByteArrBuild, ByRef PointPos As Integer)

    End Sub

    Private Sub userMenage_Click(sender As Object, e As EventArgs) Handles userMenage.Click
        If Not AuthorizationService.RequireAdministrator(Me) Then Return
        frmUserManage.ShowDialog()
        frmUserManage.Activate()

    End Sub

    Private Sub EMS涓婁紶ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EMS上传ToolStripMenuItem.Click
        FrmMesTask.ShowDialog()
        FrmMesTask.Activate()
    End Sub
    Private Sub CalibrationToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles calibrationMenu.Click
        Dim calibrationForm As FrmChannelCalibration = Nothing
        For Each child As Form In Me.MdiChildren
            If TypeOf child Is FrmChannelCalibration Then
                calibrationForm = CType(child, FrmChannelCalibration)
                Exit For
            End If
        Next

        If calibrationForm Is Nothing OrElse calibrationForm.IsDisposed Then
            calibrationForm = New FrmChannelCalibration()
            calibrationForm.MdiParent = Me
        End If

        calibrationForm.WindowState = FormWindowState.Maximized
        calibrationForm.Show()
        calibrationForm.Activate()
    End Sub
End Class
