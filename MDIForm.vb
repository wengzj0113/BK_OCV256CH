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
Imports System.Threading.Tasks

Public Class MDIForm
    Private plcRuntimeStatus As String = String.Empty
    Private plcRuntimeStatusActive As Boolean
    Private ngRetestForm As FrmNgRetest
    Private applyingMdiClientLayout As Boolean
    Private mdiLayoutOrderApplied As Boolean
    Private mdiChildCreationInProgress As Boolean
    Private ReadOnly resizeDebounceTimer As New System.Windows.Forms.Timer()
    Private ReadOnly pendingSettingsApplyTimer As New System.Windows.Forms.Timer()
    Private pendingInitialSettings As Initset?
    Private pendingInitialSettingsPrevious As Initset
    Private pendingInitialSettingsChanges As InitSettingsChangeKind
    Private databaseHealthCheckStarted As Integer
    Private startupDeviceFailurePromptShown As Integer
    'Dim adoCnn As SqlConnection
    Public mSock As SocketCtrolCmd
    Public FrmLink As New List(Of MDIForm)
    Public mSocket As Socket
    Public fixtureClose As Boolean
    Public operTypeNum As Integer = 0
    Private environmentResetStarted As Boolean
    Private applicationExitReadyToClose As Boolean
    Private applicationExitCurrentStep As String = "准备退出"
    Private Const ShutdownResetTimeoutMs As Integer = 3000
    Private Const ShutdownPortCloseTimeoutMs As Integer = 3000
    Private Const ShutdownWaitPromptIntervalMs As Integer = 5000
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
        If Not AuthorizationService.RequirePermission(AppPermission.ManageProcessParameters, Me) Then Return
        BusinessLogService.Write("操作", "成功", "打开初始设置", "系统设置", "用户打开初始设置窗口。", "", "系统设置")
        Dim exitAfterSettings As Boolean = False
        Me.Cursor = Cursors.WaitCursor
        Try
            Using initForm As New frmInit()
                exitAfterSettings = (initForm.ShowDialog(Me) = DialogResult.Yes)
            End Using
        Finally
            Me.Cursor = Cursors.Default
        End Try
        If exitAfterSettings Then Me.Close()
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
        Me.Close()
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

    Public Function EnsureAuthenticatedUser() As Boolean
        If AuthorizationService.IsAuthenticated() Then Return True

        Using login As New FrmLoginNew()
            If login.ShowDialog(Me) <> DialogResult.OK Then Return False
        End Using

        ApplyOperatorPermissions()
        UpdateBottomStatus()
        Return AuthorizationService.IsAuthenticated()
    End Function

    Private Sub logoutMenu_Click(sender As Object, e As EventArgs) Handles logoutMenu.Click
        Dim loggingOutUser As String = AuthorizationService.CurrentUserName
        AuthorizationService.ResetSession()
        BusinessLogService.Write("操作", "成功", "退出登录", loggingOutUser, "用户已退出登录。", "", "用户认证", loggingOutUser)
        ApplyOperatorPermissions()
        UpdateBottomStatus()
    End Sub

    Private Sub ApplyOperatorPermissions()
        Dim businessFunctionsEnabled As Boolean = AuthorizationService.CanAccess(AppPermission.UseBusinessFunctions)
        Dim engineerFunctionsEnabled As Boolean = AuthorizationService.CanAccess(AppPermission.DeviceDiagnostics)
        Dim processSettingsEnabled As Boolean = AuthorizationService.CanAccess(AppPermission.ManageProcessParameters)
        btnInit.Enabled = processSettingsEnabled
        ToolDebug.Enabled = engineerFunctionsEnabled
        init.Enabled = processSettingsEnabled
        btnDebug.Enabled = engineerFunctionsEnabled
        calibrationMenu.Enabled = AuthorizationService.CanAccess(AppPermission.CalibrateChannels)
        userMenage.Enabled = AuthorizationService.CanAccess(AppPermission.UserManagement)
        SetTestEntryEnabled(businessFunctionsEnabled)
        logoutMenu.Enabled = AuthorizationService.IsAuthenticated()
        loginMenu.Text = If(AuthorizationService.IsAuthenticated(), "切换用户", "登录")
        operTypeNum = CInt(AuthorizationService.CurrentRole)
        CloseRestrictedEngineeringWindows(engineerFunctionsEnabled)
    End Sub

    Private Sub CloseRestrictedEngineeringWindows(engineerFunctionsEnabled As Boolean)
        Dim restrictedForms As New List(Of Form)()
        If Not engineerFunctionsEnabled Then
            For Each openForm As Form In Application.OpenForms
                If TypeOf openForm Is frmDebug OrElse TypeOf openForm Is FrmChannelCalibration Then
                    restrictedForms.Add(openForm)
                End If
            Next
        End If

        For Each restrictedForm As Form In restrictedForms
            restrictedForm.Close()
        Next
    End Sub

    Private Sub btnExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.Close()
    End Sub

    Private Async Sub MDIForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If applicationExitReadyToClose Then Return

        If e.CloseReason = CloseReason.WindowsShutDown OrElse e.CloseReason = CloseReason.TaskManagerClosing Then
            RunLegacyShutdownForSystemClose()
            Return
        End If

        '设备清理期间先取消本次关窗，避免 UI 线程进入同步的串口/VISA Close。
        e.Cancel = True
        If environmentResetStarted Then Return

        environmentResetStarted = True
        ApplicationShuttingDown = True
        applicationExitCurrentStep = "停止界面计时器"
        UseWaitCursor = True
        Enabled = False

        resizeDebounceTimer.Stop()
        pendingSettingsApplyTimer.Stop()

        Dim testForms As New List(Of frmTest)()
        For Each child As Form In MdiChildren
            Dim testForm As frmTest = TryCast(child, frmTest)
            If testForm Is Nothing OrElse testForm.IsDisposed Then Continue For
            testForm.PrepareForApplicationExitUi()
            testForms.Add(testForm)
        Next

        Dim retestFormForShutdown As FrmNgRetest = ngRetestForm
        If retestFormForShutdown IsNot Nothing AndAlso Not retestFormForShutdown.IsDisposed Then
            retestFormForShutdown.PrepareForApplicationExitUi()
        End If

        DetachDevicePortsFromDesignerContainer()
        AppLog.Write("Shutdown", "Application exit requested; UI shutdown preparation completed.")

        Dim cleanupTask As Task(Of List(Of String)) = Task.Run(
            Function() RunApplicationExitCleanup(testForms, retestFormForShutdown))

        Do While Not cleanupTask.IsCompleted
            Dim completedTask As Task = Await Task.WhenAny(cleanupTask, Task.Delay(ShutdownWaitPromptIntervalMs))
            If completedTask Is cleanupTask Then Exit Do

            Dim timeoutMessage As String = String.Format("退出清理仍在执行。当前步骤：{0}{1}选 是 继续等待；选 否 直接退出（设备状态可能尚未确认）。", applicationExitCurrentStep, Environment.NewLine)
            Dim answer As DialogResult = MessageBox.Show(timeoutMessage, "程序正在退出", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)

            If answer = DialogResult.No Then
                AppLog.Write("Shutdown", "User chose to exit before cleanup completed. Current step: " & applicationExitCurrentStep)
                DetachRetestWindowForForcedExit(retestFormForShutdown)
                applicationExitReadyToClose = True
                Close()
                Return
            End If
        Loop

        Dim failures As List(Of String)
        Try
            failures = Await cleanupTask
        Catch ex As Exception
            failures = New List(Of String) From {"退出清理任务异常：" & ex.Message}
            AppLog.Write("Shutdown", "Application exit cleanup task failed: " & ex.ToString())
        End Try

        If failures.Count > 0 Then
            Dim failureMessage As String = String.Format("退出清理过程中有步骤未能正常完成，设备状态需要现场确认：{0}{0}{1}", Environment.NewLine, String.Join(Environment.NewLine, failures))
            MessageBox.Show(failureMessage, "退出清理提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If

        If retestFormForShutdown IsNot Nothing AndAlso Not retestFormForShutdown.IsDisposed Then
            Try
                retestFormForShutdown.Close()
            Catch ex As Exception
                AppLog.Write("Shutdown", "Closing NG retest window failed: " & ex.ToString())
            End Try
        End If

        AppLog.Write("Shutdown", "Application exit cleanup finished; closing main window.")
        applicationExitReadyToClose = True
        Close()
    End Sub

    Private Sub RunLegacyShutdownForSystemClose()
        If environmentResetStarted Then Return
        environmentResetStarted = True
        ApplicationShuttingDown = True

        For Each child As Form In MdiChildren
            Dim testForm As frmTest = TryCast(child, frmTest)
            If testForm IsNot Nothing Then testForm.PrepareForShutdown()
        Next
        resizeDebounceTimer.Stop()
        pendingSettingsApplyTimer.Stop()

        Try
            Dim middleMachine As SerialMiddleMachineClient = Nothing
            If Com_SCM IsNot Nothing AndAlso Com_SCM.IsOpen Then
                middleMachine = New SerialMiddleMachineClient(Com_SCM)
            End If
            TestEnvironmentResetService.ResetAll(middleMachine, True, ShutdownResetTimeoutMs)
        Catch ex As Exception
            SerialMiddleMachineClient.LogMessage("System shutdown reset exception: " & ex.Message)
        End Try
    End Sub

    Private Sub DetachDevicePortsFromDesignerContainer()
        If components Is Nothing Then Return

        Dim devicePorts As System.IO.Ports.SerialPort() = {
            Com_SCM,
            Com_3561,
            Com_scaner,
            Com_IOCard
        }
        For Each port As System.IO.Ports.SerialPort In devicePorts
            If port Is Nothing Then Continue For
            Try
                'MDIForm.Designer.Dispose 会同步释放 Container 中的组件；串口改由退出后台任务释放。
                components.Remove(port)
            Catch ex As Exception
                AppLog.Write("Shutdown", "Could not detach serial port from form container: " & ex.ToString())
            End Try
        Next
    End Sub

    Private Function RunApplicationExitCleanup(testForms As IList(Of frmTest),
                                               retestForm As FrmNgRetest) As List(Of String)
        Dim failures As New List(Of String)()

        For Each testForm As frmTest In testForms
            RunApplicationExitStep(failures,
                                   "关闭测试页设备客户端",
                                   Sub() failures.AddRange(testForm.ShutdownForApplicationExit()))
        Next

        RunApplicationExitStep(failures,
                               "复位中位机和仪表",
                               Sub()
                                   Dim middleMachine As SerialMiddleMachineClient = Nothing
                                   If Com_SCM IsNot Nothing AndAlso Com_SCM.IsOpen Then
                                       middleMachine = New SerialMiddleMachineClient(Com_SCM)
                                   End If
                                   TestEnvironmentResetService.ResetAll(middleMachine, True, ShutdownResetTimeoutMs)
                               End Sub)

        CloseAndDisposeDevicePort(Com_3561, "3561串口", failures)
        CloseAndDisposeDevicePort(Com_SCM, "中位机串口", failures)
        CloseAndDisposeDevicePort(Com_scaner, "扫码枪串口", failures)
        CloseAndDisposeDevicePort(Com_IOCard, "IO板串口", failures)

        If retestForm IsNot Nothing Then
            RunApplicationExitStep(failures,
                                   "关闭复测仪表",
                                   Sub() retestForm.ShutdownForApplicationExit())
        End If

        Return failures
    End Function

    Private Sub CloseAndDisposeDevicePort(port As System.IO.Ports.SerialPort,
                                          portLabel As String,
                                          failures As List(Of String))
        RunApplicationExitStep(failures,
                               "释放" & portLabel,
                               Sub()
                                   If port Is Nothing Then Return

                                   '串口关闭可能被驱动或正在进行的 I/O 卡住；退出线程只等待有限时间。
                                   'SerialPort.Close 在端口打开时已执行 Dispose，无需紧接着重复 Dispose。
                                   Dim closeTask As Task = Task.Run(
                                       Sub()
                                           If port.IsOpen Then
                                               port.Close()
                                           Else
                                               port.Dispose()
                                           End If
                                       End Sub)

                                   Try
                                       If Not closeTask.Wait(ShutdownPortCloseTimeoutMs) Then
                                           closeTask.ContinueWith(
                                               Sub(completedTask)
                                                   Dim ignored As AggregateException = completedTask.Exception
                                               End Sub,
                                               TaskContinuationOptions.OnlyOnFaulted)
                                           Throw New TimeoutException(portLabel & "关闭超过" & ShutdownPortCloseTimeoutMs.ToString() & "毫秒，退出将继续。")
                                       End If
                                   Catch ex As AggregateException
                                       Dim flattened As AggregateException = ex.Flatten()
                                       Dim detail As String = If(flattened.InnerExceptions.Count > 0,
                                                                  flattened.InnerExceptions(0).Message,
                                                                  ex.Message)
                                       Throw New InvalidOperationException(portLabel & "关闭失败：" & detail, ex)
                                   End Try
                               End Sub)
    End Sub

    Private Sub RunApplicationExitStep(failures As List(Of String), stepName As String, action As Action)
        Dim timer As System.Diagnostics.Stopwatch = System.Diagnostics.Stopwatch.StartNew()
        Dim failureCountBefore As Integer = failures.Count
        applicationExitCurrentStep = stepName
        AppLog.Write("Shutdown", "START " & stepName)
        Try
            action()
            Dim result As String = If(failures.Count = failureCountBefore, "returned", "completed-with-errors")
            AppLog.Write("Shutdown", "END " & stepName & " result=" & result & " elapsedMs=" & timer.ElapsedMilliseconds.ToString())
        Catch ex As Exception
            failures.Add(stepName & "：" & ex.Message)
            AppLog.Write("Shutdown", "FAIL " & stepName & " elapsedMs=" & timer.ElapsedMilliseconds.ToString() & " error=" & ex.ToString())
        End Try
    End Sub

    Private Sub DetachRetestWindowForForcedExit(retestForm As FrmNgRetest)
        If retestForm Is Nothing OrElse retestForm.IsDisposed Then Return
        Try
            retestForm.Hide()
            retestForm.Owner = Nothing
        Catch ex As Exception
            AppLog.Write("Shutdown", "Could not detach NG retest window before forced exit: " & ex.ToString())
        End Try
    End Sub

    Private Sub MDIForm_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        On Error Resume Next
        resizeDebounceTimer.Stop()
        resizeDebounceTimer.Dispose()
        pendingSettingsApplyTimer.Stop()
        pendingSettingsApplyTimer.Dispose()
        If ngRetestForm IsNot Nothing AndAlso Not ngRetestForm.IsDisposed Then
            ngRetestForm.Close()
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
        pendingSettingsApplyTimer.Interval = 300
        AddHandler pendingSettingsApplyTimer.Tick, AddressOf PendingSettingsApplyTimer_Tick
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
            GetConfiguredStationName(),
            If(controlBoardConnected, "已连接", "未连接"),
            If(String.IsNullOrWhiteSpace(plcRuntimeStatus), If(plcConnected, "已连接", "未连接"), plcRuntimeStatus),
            If(dmm34461Connected, "已连接", "未连接"),
            If(resistance3561Connected AndAlso Com_3561 IsNot Nothing AndAlso Com_3561.IsOpen, "已连接", "未连接"),
            If(databaseConnected, "已连接", "未连接"),
            If(mesConnected, "已连接", "未连接"),
            If(String.IsNullOrWhiteSpace(AuthorizationService.CurrentUserName), "未登录", AuthorizationService.CurrentUserName),
            DateTime.Now)
    End Sub

    Public Sub UpdatePlcRuntimeStatus(ByVal status As String)
        plcRuntimeStatus = status
        plcRuntimeStatusActive = Not String.IsNullOrWhiteSpace(status) AndAlso status <> "未连接"
        UpdateBottomStatus()
    End Sub

    Private Shared Function GetConfiguredStationName() As String
        Select Case Initsetting.stationSelect
            Case 0
                Return "OCV0"
            Case 1
                Return "OCV1"
            Case 2
                Return "OCV2"
            Case Else
                Return "未配置"
        End Select
    End Function

    Private dmm34461Connected As Boolean
    Private resistance3561Connected As Boolean
    Private mesConnected As Boolean
    Private startupExternalChecksStarted As Integer

    '供启动期后台设备检查完成后刷新底部状态控件。
    Public Sub UpdateDeviceCheckStatus(ByVal dmmStatus As Short?,
                                       ByVal resistanceStatus As Short?,
                                       ByVal mesStatus As Short?)
        '现有设备检查函数的成功码不同：34461=0，3561=1，MES=1。
        If dmmStatus.HasValue Then dmm34461Connected = (dmmStatus.Value = 0)
        If resistanceStatus.HasValue Then resistance3561Connected = (resistanceStatus.Value = 1)
        If mesStatus.HasValue Then mesConnected = (mesStatus.Value = 1)
        UpdateBottomStatus()
    End Sub

    Friend Function CanApplyInitialSettings(ByVal changes As InitSettingsChangeKind,
                                            ByRef reason As String) As Boolean
        Dim testRunning As Boolean = isRuning
        Dim automaticCycleBusy As Boolean = False
        Dim engineeringWindowOpen As Boolean = False
        Dim deviceCheckRunning As Boolean = StartupDeviceService.IsRunning

        For Each child As Form In MdiChildren
            Dim testForm As frmTest = TryCast(child, frmTest)
            If testForm IsNot Nothing AndAlso Not testForm.IsDisposed AndAlso Not testForm.CanApplyInitialSettings Then
                automaticCycleBusy = True
            End If
        Next

        If InitSettingsApplyPolicy.RequiresDeviceRecheck(changes) OrElse
           (changes And InitSettingsChangeKind.ExternalServices) <> InitSettingsChangeKind.None Then
            For Each openForm As Form In Application.OpenForms
                If TypeOf openForm Is frmDebug OrElse
                   TypeOf openForm Is FrmChannelCalibration OrElse
                   TypeOf openForm Is FrmNgRetest OrElse
                   TypeOf openForm Is FrmManualTest Then
                    If openForm.Visible Then engineeringWindowOpen = True
                End If
            Next
        End If

        If Not InitSettingsApplyPolicy.CanApplyNow(testRunning,
                                                  automaticCycleBusy,
                                                  deviceCheckRunning,
                                                  engineeringWindowOpen) Then
            If engineeringWindowOpen Then
                reason = "调试、校准或复测窗口正在使用设备，请关闭相关窗口后再应用设置。"
            ElseIf automaticCycleBusy OrElse testRunning Then
                reason = "测试或PLC自动流程正在运行，请等待本轮结束并停在待机状态后再保存设置。"
            Else
                reason = "设备启动检查正在进行，请稍后再保存设置。"
            End If
            Return False
        End If

        reason = String.Empty
        Return True
    End Function

    Friend Function SaveAndApplyInitialSettings(ByVal currentSettings As Initset,
                                                ByRef resultMessage As String) As Boolean
        If pendingInitialSettings.HasValue Then
            resultMessage = "已有一组设置正在等待安全时机应用，请稍后再修改。"
            Return False
        End If

        Dim previousSettings As Initset = Initsetting
        Dim changes As InitSettingsChangeKind = InitSettingsApplyPolicy.Classify(previousSettings, currentSettings)
        Try
            SaveInitFile(currentSettings)
        Catch ex As Exception
            resultMessage = "保存初始设置失败：" & ex.Message
            BusinessLogService.Write("操作", "失败", "保存初始设置", "系统参数", "初始设置保存失败。", ex.ToString(), "初始设置")
            Return False
        End Try

        If changes = InitSettingsChangeKind.None Then
            resultMessage = "设置没有变化，已确认保存。"
            Return True
        End If

        Dim applyBlockReason As String = String.Empty
        If InitSettingsApplyPolicy.RequiresSafeBoundary(changes) AndAlso
           Not CanApplyInitialSettings(changes, applyBlockReason) Then
            pendingInitialSettingsPrevious = previousSettings
            pendingInitialSettings = currentSettings
            pendingInitialSettingsChanges = changes

            If (changes And InitSettingsChangeKind.Display) <> InitSettingsChangeKind.None Then
                For Each child As Form In MdiChildren
                    Dim testForm As frmTest = TryCast(child, frmTest)
                    If testForm IsNot Nothing AndAlso Not testForm.IsDisposed Then
                        testForm.ApplyConfiguredChannelColors(currentSettings)
                    End If
                Next
            End If

            For Each child As Form In MdiChildren
                Dim testForm As frmTest = TryCast(child, frmTest)
                If testForm Is Nothing OrElse testForm.IsDisposed Then Continue For
                Try
                    testForm.RequestSettingsApplyAtCycleBoundary(
                        Sub()
                            Try
                                If Not IsDisposed Then BeginInvoke(New MethodInvoker(AddressOf TryApplyPendingInitialSettings))
                            Catch ex As Exception
                                System.Diagnostics.Debug.WriteLine("设置应用边界回调失败: " & ex.Message)
                            End Try
                        End Sub)
                Catch ex As Exception
                    System.Diagnostics.Debug.WriteLine("请求PLC周期边界暂停失败: " & ex.Message)
                End Try
            Next

            pendingSettingsApplyTimer.Start()
            resultMessage = "设置已保存；当前测试或设备操作结束后，程序会自动应用新配置。" &
                            If(String.IsNullOrWhiteSpace(applyBlockReason), String.Empty, Environment.NewLine & applyBlockReason)
            SetInitialSettingsApplyStatus("新设置已保存，等待测试/设备流程到达安全边界后自动应用。")
            BusinessLogService.Write("操作", "成功", "保存初始设置", "系统参数", "初始设置已保存并排队等待安全应用。", changes.ToString(), "初始设置")
            Return True
        End If

        Initsetting = currentSettings
        Try
            ApplyInitialSettings(previousSettings, currentSettings, changes)
        Catch ex As Exception
            resultMessage = "配置已保存，但当前程序未能完整应用：" & ex.Message & "。请检查设备连接；若问题仍在，请退出并重新启动程序。"
            BusinessLogService.Write("操作", "失败", "应用初始设置", "系统参数", "配置已保存，但运行时应用未能启动。", ex.ToString(), "初始设置")
            Return True
        End Try

        resultMessage = "初始设置已保存并提交运行时应用。"
        BusinessLogService.Write("操作", "成功", "保存初始设置", "系统参数", "初始设置已保存并提交运行时应用。", changes.ToString(), "初始设置")
        Return True
    End Function

    Private Sub PendingSettingsApplyTimer_Tick(sender As Object, e As EventArgs)
        TryApplyPendingInitialSettings()
    End Sub

    Private Sub TryApplyPendingInitialSettings()
        If Not pendingInitialSettings.HasValue OrElse IsDisposed OrElse ApplicationShuttingDown Then Return
        Dim reason As String = String.Empty
        If Not CanApplyInitialSettings(pendingInitialSettingsChanges, reason) Then Return

        Dim nextSettings As Initset = pendingInitialSettings.Value
        Dim previousSettings As Initset = pendingInitialSettingsPrevious
        Dim changes As InitSettingsChangeKind = pendingInitialSettingsChanges
        pendingInitialSettings = Nothing
        pendingInitialSettingsChanges = InitSettingsChangeKind.None
        pendingSettingsApplyTimer.Stop()

        Initsetting = nextSettings
        Try
            ApplyInitialSettings(previousSettings, nextSettings, changes)
            BusinessLogService.Write("操作", "成功", "应用待处理初始设置", "系统参数", "待应用设置已在安全边界生效。", changes.ToString(), "初始设置")
        Catch ex As Exception
            SetInitialSettingsApplyStatus("设置已保存，但运行时应用失败：" & ex.Message & "；请检查设备或重新启动程序。")
            BusinessLogService.Write("操作", "失败", "应用待处理初始设置", "系统参数", "安全边界应用失败。", ex.ToString(), "初始设置")
            MessageBox.Show(Me,
                            "设置已保存，但当前程序未能完整应用：" & ex.Message & Environment.NewLine &
                            "请检查设备连接；若问题仍在，请退出并重新启动程序。",
                            "设置已保存，应用未完成",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
        End Try
    End Sub

    Friend Sub ApplyInitialSettings(ByVal previousSettings As Initset,
                                    ByVal currentSettings As Initset,
                                    ByVal changes As InitSettingsChangeKind)
        If changes = InitSettingsChangeKind.None Then Return

        Dim testForm As frmTest = Nothing
        For Each child As Form In MdiChildren
            testForm = TryCast(child, frmTest)
            If testForm IsNot Nothing AndAlso Not testForm.IsDisposed Then Exit For
            testForm = Nothing
        Next

        Dim plcNeedsRestart As Boolean = InitSettingsApplyPolicy.RequiresPlcClientRestart(previousSettings, currentSettings)
        Dim options As New StartupDeviceCheckOptions With {
            .CheckScm = InitSettingsApplyPolicy.RequiresScmRecheck(previousSettings, currentSettings),
            .CheckDmm34461 = InitSettingsApplyPolicy.RequiresDmmRecheck(previousSettings, currentSettings),
            .CheckResistance3561 = InitSettingsApplyPolicy.RequiresMain3562Recheck(previousSettings, currentSettings),
            .CheckRetest3562 = InitSettingsApplyPolicy.RequiresRetest3562Recheck(previousSettings, currentSettings),
            .ReportRetest3562Failures = InitSettingsApplyPolicy.RequiresRetest3562Recheck(previousSettings, currentSettings),
            .CheckPlc = testForm Is Nothing AndAlso plcNeedsRestart,
            .CheckMes = InitSettingsApplyPolicy.RequiresMesRecheck(previousSettings, currentSettings)
        }

        Dim hasHardwareChecks As Boolean = options.CheckScm OrElse options.CheckDmm34461 OrElse
                                           options.CheckResistance3561 OrElse options.CheckRetest3562 OrElse options.CheckPlc
        If testForm IsNot Nothing AndAlso hasHardwareChecks Then testForm.SetInitializationApplyInProgress(True)

        Dim testFormApplyError As String = String.Empty
        If testForm IsNot Nothing Then
            Try
                Dim plcWasConnected As Boolean = testForm.ApplyConfiguredInitialSettings(previousSettings, currentSettings, changes)
                If plcNeedsRestart Then plcConnected = plcWasConnected
            Catch ex As Exception
                testFormApplyError = ex.Message
                testForm.SetInitializationApplyInProgress(False)
                testForm.ResumeAutomaticFlowAfterSettingsApply()
                BusinessLogService.Write("操作", "失败", "应用初始设置", "系统参数", "配置已保存，但测试页面运行时对象重建失败。", ex.ToString(), "初始设置")
                SetInitialSettingsApplyStatus("配置已保存，但页面/PLC配置未能完整应用：" & ex.Message)
            End Try
        End If

        If options.CheckScm OrElse options.CheckDmm34461 OrElse options.CheckResistance3561 OrElse
           options.CheckRetest3562 OrElse options.CheckPlc OrElse options.CheckMes Then
            If Not StartupDeviceService.StartAsync(Me,
                                                   Sub(result As StartupDeviceCheckResult)
                                                       ApplyStartupDeviceCheckResult(result, True)
                                                       If testForm IsNot Nothing Then
                                                           testForm.SetInitializationApplyInProgress(False)
                                                           testForm.ResumeAutomaticFlowAfterSettingsApply()
                                                       End If
                                                   End Sub,
                                                   options) Then
                If testForm IsNot Nothing Then
                    testForm.SetInitializationApplyInProgress(False)
                    testForm.ResumeAutomaticFlowAfterSettingsApply()
                End If
                SetInitialSettingsApplyStatus("配置已保存，但设备检查未启动；请稍后重新检查设备。")
            Else
                SetInitialSettingsApplyStatus("配置已保存，正在重新应用受影响的设备设置…")
            End If
        ElseIf testForm IsNot Nothing AndAlso hasHardwareChecks Then
            testForm.SetInitializationApplyInProgress(False)
        End If

        If InitSettingsApplyPolicy.RequiresDatabaseRecheck(previousSettings, currentSettings) Then
            RecheckDatabaseHealthAsync()
        End If
        If testForm IsNot Nothing AndAlso Not hasHardwareChecks Then
            testForm.ResumeAutomaticFlowAfterSettingsApply()
        End If
        If Not String.IsNullOrWhiteSpace(testFormApplyError) AndAlso Not options.CheckPlc Then
            MessageBox.Show(Me,
                            "配置已保存，但当前运行状态未能完整重建：" & testFormApplyError & Environment.NewLine &
                            "请检查设备连接；若问题仍在，请退出并重新启动程序。",
                            "设置已保存，应用未完成",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning)
        End If
    End Sub

    Public Sub ApplyStartupDeviceCheckResult(ByVal result As StartupDeviceCheckResult,
                                             Optional ByVal allowRepeatedFailurePrompt As Boolean = False)
        If result Is Nothing OrElse IsDisposed Then Return

        If result.ScmChecked Then
            controlBoardConnected = (result.Scm = 1)
            If result.Scm <> 1 Then blnOnLine = False
        End If
        If result.Dmm34461Checked AndAlso result.Dmm34461 <> 0 Then blnOnLine = False
        If result.Resistance3561Checked AndAlso result.Resistance3561 <> 1 Then blnOnLine = False
        If result.PlcChecked AndAlso Not plcRuntimeStatusActive Then
            plcRuntimeStatus = String.Empty
            plcConnected = (result.Plc = 1)
            If Not plcConnected Then blnOnLine = False
        End If
        If result.MesChecked AndAlso result.Mes <> 1 Then blnOnLine = False

        UpdateDeviceCheckStatus(
            If(result.Dmm34461Checked, CType(result.Dmm34461, Short?), Nothing),
            If(result.Resistance3561Checked, CType(result.Resistance3561, Short?), Nothing),
            If(result.MesChecked, CType(result.Mes, Short?), Nothing))
        ShowStartupDeviceFailurePrompt(result, allowRepeatedFailurePrompt)
    End Sub

    Private Sub SetInitialSettingsApplyStatus(ByVal message As String)
        If txtTesting IsNot Nothing Then txtTesting.Text = message
    End Sub

    Private Sub RecheckDatabaseHealthAsync()
        If ApplicationShuttingDown OrElse IsDisposed Then Return
        Threading.ThreadPool.QueueUserWorkItem(Sub()
                                                   Dim ready As Boolean = False
                                                   Dim failureDetail As String = String.Empty
                                                   Try
                                                       ready = CheckOcvDataHealth(failureDetail)
                                                   Catch ex As Exception
                                                       System.Diagnostics.Debug.WriteLine("数据库重新检查失败: " & ex.Message)
                                                   End Try
                                                   Try
                                                       If IsDisposed Then Return
                                                       BeginInvoke(New Action(Sub()
                                                                                  If IsDisposed Then Return
                                                                                  databaseConnected = ready
                                                                                  UpdateBottomStatus()
                                                                                  If ready Then
                                                                                      SetInitialSettingsApplyStatus("数据库连接已按新设置确认。")
                                                                                  Else
                                                                                      Dim detailMessage As String = BuildDatabaseFailureMessage(failureDetail)
                                                                                      SetInitialSettingsApplyStatus("配置已保存，但数据库连接检查失败。" & Environment.NewLine & detailMessage)
                                                                                      MessageBox.Show(Me, detailMessage, "数据库连接失败", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                                                                  End If
                                                                              End Sub))
                                                   Catch ex As Exception
                                                       System.Diagnostics.Debug.WriteLine("数据库重检状态更新失败: " & ex.Message)
                                                   End Try
                                               End Sub)
    End Sub

    ''' <summary>
    ''' Displays one startup summary for failed external-device checks.
    ''' </summary>
    Public Sub ShowStartupDeviceFailurePrompt(ByVal result As StartupDeviceCheckResult,
                                              Optional ByVal allowRepeatedPrompt As Boolean = False)
        If result Is Nothing OrElse Not result.HasFailures Then Return
        If Not allowRepeatedPrompt AndAlso Threading.Interlocked.Exchange(startupDeviceFailurePromptShown, 1) = 1 Then Return

        Dim message As New System.Text.StringBuilder()
        message.AppendLine("设备连接检查发现以下设备未能正常连接或应用：")
        If result.ScmChecked AndAlso result.Scm <> 1 Then
            AppendStartupFailure(message, result, "SCM", "控制模块（COM" & (Initsetting.SCMComPort + 1).ToString() & "）")
        End If
        If result.Dmm34461Checked AndAlso result.Dmm34461 <> 0 Then
            AppendStartupFailure(message, result, "34461", "数字万用表（" & DisplayOrDefault(Initsetting.IpAddOf34461, "地址未配置") & "）")
        End If
        If result.Resistance3561Checked AndAlso result.Resistance3561 <> 1 Then
            AppendStartupFailure(message, result, "3561", "内阻仪（COM" & (Initsetting.Recom + 1).ToString() & "）")
        End If
        If result.Retest3562Checked AndAlso result.Retest3562 <> 1 Then
            AppendStartupFailure(message, result, "Retest3562", "NG复测仪（COM" & (Initsetting.Retest3562Port + 1).ToString() & "）")
        End If
        If result.PlcChecked AndAlso result.Plc <> 1 Then
            AppendStartupFailure(message, result, "PLC", FormatPlcDisplayName())
        End If
        If result.MesChecked AndAlso result.Mes <> 1 Then
            AppendStartupFailure(message, result, "MES", "MES（" & DisplayOrDefault(Initsetting.MesApiAddress, "地址未配置") & "）")
        End If

        message.AppendLine()
        message.Append("请检查设备供电、通信线缆、串口号及通信参数后再进行测试。")
        Using dialog As New StartupDeviceFailureDialog(message.ToString())
            dialog.ShowDialog(Me)
        End Using
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
                                                   Dim failureDetail As String = String.Empty
                                                   Try
                                                       StartupTiming.Mark("database-check-start")
                                                       ready = CheckOcvDataHealth(failureDetail)
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
                                                                                     If Not ready Then
                                                                                         MessageBox.Show(Me,
                                                                                                         BuildDatabaseFailureMessage(failureDetail),
                                                                                                         "数据库连接失败",
                                                                                                         MessageBoxButtons.OK,
                                                                                                         MessageBoxIcon.Warning)
                                                                                     End If
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

    Private Function CheckOcvDataHealth(ByRef failureDetail As String) As Boolean
        '此函数只读取数据库健康状态；UI 更新由 StartDatabaseHealthCheckAsync 统一调度。
        Try
            Dim service As New OcvDataService()
            Dim health As OcvDatabaseHealthResult = service.CheckDatabaseHealth()
            If health.IsSuccess Then
                databaseConnected = True
                failureDetail = String.Empty
                Return True
            End If

            databaseConnected = False
            blnOnLine = False
            failureDetail = health.Message
            System.Diagnostics.Debug.WriteLine("OCV_DATA 健康检查未通过: " & health.Message)
            Return False
        Catch ex As Exception
            databaseConnected = False
            blnOnLine = False
            failureDetail = ex.Message
            System.Diagnostics.Debug.WriteLine("OCV_DATA 健康检查异常: " & ex.Message)
            Return False
        End Try
    End Function

    Private Shared Function BuildDatabaseFailureMessage(ByVal failureDetail As String) As String
        Dim message As New System.Text.StringBuilder()
        message.AppendLine("程序连接 OCV_DATA 数据库失败。")
        message.AppendLine("服务器：" & If(String.IsNullOrWhiteSpace(Initsetting.strDatabase), "（未配置）", Initsetting.strDatabase))
        message.AppendLine("账号：" & If(String.IsNullOrWhiteSpace(Initsetting.strUserID), "（未配置）", Initsetting.strUserID))
        message.AppendLine("原因：" & If(String.IsNullOrWhiteSpace(failureDetail), "未知错误", failureDetail))
        message.AppendLine()
        message.AppendLine("数据库与程序在同一台电脑时，请先核对 SQL Server 实例：默认实例可填写 127.0.0.1；命名实例需填写 127.0.0.1\实例名，或填写 127.0.0.1,固定端口。还要确认对应 SQL Server 服务正在运行、TCP/IP 已启用、SQL Server 身份验证已启用，并且该账号已映射到 OCV_DATA。")
        Return message.ToString()
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
        If Not AuthorizationService.RequirePermission(AppPermission.DeviceDiagnostics, Me) Then Return
        Me.Cursor = Cursors.WaitCursor
        If Not My.Forms.frmDebug.IsDisposed Then
            My.Forms.frmDebug.Dispose()
        End If
        My.Forms.frmDebug.Show()
        My.Forms.frmDebug.Activate()
        Me.Cursor = Cursors.Default
    End Sub
    Private Sub ToolStripButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton1.Click
        Me.Close()
    End Sub

    Private Sub ToolDebug_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolDebug.Click
        If Not AuthorizationService.RequirePermission(AppPermission.DeviceDiagnostics, Me) Then Return
        frmDebug.Show()
    End Sub
    Private Sub ToolInit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        init_Click(sender, e)
    End Sub

    Private Sub btnInit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnInit.Click
        If Not AuthorizationService.RequirePermission(AppPermission.ManageProcessParameters, Me) Then Return
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
        If Not AuthorizationService.RequirePermission(AppPermission.CalibrateChannels, Me) Then Return
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
