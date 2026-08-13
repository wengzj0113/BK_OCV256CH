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
    Private bottomStatusControl As BottomStatusControl
    'Dim adoCnn As SqlConnection
    Public mSock As SocketCtrolCmd
    Public FrmLink As New List(Of MDIForm)
    Public mSocket As Socket
    Public fixtureClose As Boolean
    Public operTypeNum As Integer

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

    Private Sub StatusBarToolStripMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs) 
        '旧状态栏菜单项已从 Designer 移除，保留事件入口以兼容历史调用。
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
        Me.Cursor = Cursors.WaitCursor
        If Not My.Forms.frmInit.IsDisposed Then
            My.Forms.frmInit.Dispose()
        End If
        My.Forms.frmInit.Show()
        My.Forms.frmInit.Activate()
        Me.Cursor = Cursors.Default
    End Sub
    Private Sub OnLineTest_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OnLineTest.Click
        Me.Cursor = Cursors.WaitCursor
        Dim obj_frmTest As Form
        Dim I, J As Int16
        I = -1
        J = -1
        Try
            For I = 0 To UBound(Me.MdiChildren)
                If Me.MdiChildren(I).Text.ToString = "娴嬭瘯" Then
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
            obj_frmTest.WindowState = FormWindowState.Maximized
            obj_frmTest.Show()
            obj_frmTest.Activate()
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
        Using manualForm As New FrmManualTest()
            manualForm.ShowDialog(Me)
        End Using
    End Sub

    Private Sub btnExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Me.Close()
    End Sub

    Private Sub MDIForm_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        On Error Resume Next
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
        EnsureCalibrationMenu()
        EnsureBottomStatusControl()

        'R2-17:数据库检查改成异步 (在 Shown 事件里 Task.Run),Load 阶段只显示占位状态
        Me.status_DataBase.Text = "数据库：检查中"
        blnOnLine = False

        If operTypeNum <> 1 Then
            btnInit.Enabled = False
            ToolDebug.Enabled = False
            init.Enabled = False
            btnDebug.Enabled = False
            userMenage.Enabled = False
        End If
        txtTesting.Location = New Point(Me.Size.Width - 200, txtTesting.Location.Y)

        'R2-15: myClsDB 已随 clsDB 一起淘汰,Dispose 死代码删除 (数据库生命周期由 OcvDataService 每次 Using 管理)
        mSock = frmDebug.mSock
        mSocket = frmDebug.mSocket
    End Sub

    Private Sub EnsureBottomStatusControl()
        If bottomStatusControl Is Nothing Then
            bottomStatusControl = New BottomStatusControl()
            Controls.Add(bottomStatusControl)
            bottomStatusControl.BringToFront()
        End If
        UpdateBottomStatus()
    End Sub

    Private Sub UpdateBottomStatus()
        If bottomStatusControl Is Nothing Then Return
        bottomStatusControl.UpdateStatus(
            "OCV1",
            If(mSock Is Nothing, "未连接", "已连接"),
            If(status_34461 Is Nothing OrElse status_34461.Text = "——", "未连接", "已连接"),
            If(Com_3561 Is Nothing OrElse Not Com_3561.IsOpen, "未连接", "已连接"),
            If(blnOnLine, "已连接", "未连接"),
            If(StatusMes Is Nothing OrElse StatusMes.Text = "——", "未连接", "已连接"),
            If(String.IsNullOrWhiteSpace(operatorName), "Admin", operatorName),
            DateTime.Now)
    End Sub

    'R2-17 Shown 后无条件显示 frmTest (用户要求 frmTest 正常显示,不受数据库状态影响),
    '        数据库健康检查改在后台异步执行,只更新状态栏
    '        R2-18:OnLineTest_Click 用 BeginInvoke 异步调度,避免 frmTest_Load 的 socketSR1
    '        阻塞 Shown 导致数据库检查线程无法启动
    Private Sub MDIForm_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        System.Threading.ThreadPool.QueueUserWorkItem(Sub()
            Dim ready As Boolean = False
            Dim failMsg As String = ""
            Try
                ready = CheckOcvDataHealth()
            Catch ex As Exception
                failMsg = ex.Message
                System.Diagnostics.Debug.WriteLine("CheckOcvDataHealth 异常: " & ex.Message)
            End Try

            Try
                Me.BeginInvoke(New Action(Sub()
                    If String.IsNullOrEmpty(failMsg) Then
                        Me.status_DataBase.Text = If(ready, "数据库：已连接", "数据库：未连接")
                        UpdateBottomStatus()
                    Else
                        Me.status_DataBase.Text = "数据库：未连接（" & failMsg & "）"
                        UpdateBottomStatus()
                    End If
                    blnOnLine = ready
                End Sub))
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("MDIForm_Shown BeginInvoke 失败: " & ex.Message)
            End Try
        End Sub)

        'frmTest 显示放到消息队列,不阻塞 Shown
        Try
            Me.BeginInvoke(New Action(Sub()
                OnLineTest_Click(sender, e)
            End Sub))
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("OnLineTest BeginInvoke 失败: " & ex.Message)
        End Try
    End Sub
    Private Function MakeSock()
        Dim mClientTCP = New TcpClient()
        Return mClientTCP.Client     ' New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    End Function

    Private Function CheckOcvDataHealth() As Boolean
        'R2-17:此函数可能在后台线程调用 (MDIForm_Shown → ThreadPool),
        '       所有 UI 写入统一通过 BeginInvoke,MsgBox 改成返回消息字符串让调用方决定弹窗时机
        Try
            Dim service As New OcvDataService()
            Dim health As OcvDatabaseHealthResult = service.CheckDatabaseHealth()
            If health.IsSuccess Then
                UpdateDbStatusSafe("OCV_DATA connected")
                Return True
            End If

            blnOnLine = False
            UpdateDbStatusSafe("OCV_DATA disconnected")
            SetTestEntryEnabledSafe(False)
            System.Diagnostics.Debug.WriteLine("OCV_DATA 健康检查未通过: " & health.Message)
            Return False
        Catch ex As Exception
            blnOnLine = False
            UpdateDbStatusSafe("OCV_DATA disconnected")
            SetTestEntryEnabledSafe(False)
            System.Diagnostics.Debug.WriteLine("OCV_DATA 健康检查异常: " & ex.Message)
            Return False
        End Try
    End Function

    'R2-17 跨线程安全的 UI 写入包装
    Private Sub UpdateDbStatusSafe(ByVal text As String)
        If Me.IsDisposed Then Return
        If Me.InvokeRequired Then
            Me.BeginInvoke(New Action(Of String)(AddressOf UpdateDbStatusSafe), text)
            Return
        End If
        Me.status_DataBase.Text = text
    End Sub

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

    Private Function GetRomteEndPoint() As IPEndPoint
        Dim myIP As IPAddress = IPAddress.Parse(Initsetting.SCMIpAdd)
        Dim ipe As IPEndPoint = New IPEndPoint(myIP, Initsetting.SCM_port)
        Return ipe
    End Function
    Private Sub btnDebug_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDebug.Click
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
        frmDebug.Show()
    End Sub
    Private Sub ToolInit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        init_Click(sender, e)
    End Sub

    Private Sub btnInit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnInit.Click
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

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Return
        ' 浠呭湪 mSock 涓嶅彲鐢ㄦ椂閲嶅缓杩炴帴锛涙甯告儏鍐典笅杞鎵樼洏鐘舵€?+ 澶氶€氶亾瀹屾垚浣?
        Dim temp As Byte()
        Dim msg1 As String = ""

        ' 1. 鑻?mSock 涓虹┖鎴栧凡鍏抽棴,閲嶅缓杩炴帴(閬垮厤姣忕閮?Close+Connect 鐨勯鏆?
        If mSock Is Nothing Then
            Dim mClientTCP As New TcpClient()
            Dim sock As Socket = mClientTCP.Client
            Dim myIP As IPAddress = IPAddress.Parse(Initsetting.SCMIpAdd)
            Dim ipe As New IPEndPoint(myIP, Initsetting.SCM_port)
            Dim msg = ConnectTCP(sock, ipe)
            If msg <> "" Then
                ' 杩炴帴澶辫触涓嶅脊妗?閬垮厤瀹氭椂鍣ㄥ埛灞?,浠呰褰曞埌鐘舵€佹爮
                status_com.Text = "鍗曠墖鏈洪噸杩炰腑..."
                Return
            End If
            mSock = New SocketCtrolCmd(sock, AddressOf ParseBuffer, Initsetting.SCM_port, ipe)
            mSocket = sock
            status_com.Text = "鍗曠墖鏈虹鍙ｆ甯?  "
            Return
        End If

        ' 2. 鏌ヨ鎵樼洏鐘舵€?
        Try
            temp = mSock.GetSignalTrayStatus
            msg1 = mSock.Send(temp)
        Catch ex As Exception
            ' 涓綅鏈烘柇绾?- 閲婃斁 mSock 璁╀笅涓€杞噸杩?
            Try : mSock.Close() : Catch : End Try
            mSock = Nothing
            Return
        End Try

        SetSock(mSock)
        Me.Cursor = Cursors.Default
        Dim bytes(500) As Byte
        Dim bytesRec As Integer = 0

        Try
            mSocket.ReceiveTimeout = 2000
            bytesRec = mSocket.Receive(bytes)
        Catch
            bytesRec = 0
        End Try

        If bytesRec > 4 AndAlso bytes(4) = 1 Then
            fixtureClose = True
            frmTest.txtInfo.Text = ChrW(&H6258) & ChrW(&H76D8) & ChrW(&H5DF2) & ChrW(&H5173) & ChrW(&H95ED)
        Else
            frmTest.txtInfo.Text = ChrW(&H6258) & ChrW(&H76D8) & ChrW(&H5DF2) & ChrW(&H6253) & ChrW(&H5F00)
        End If

        ' 3. 鏌ヨ澶氶€氶亾瀹屾垚浣?
        Try
            temp = mSock.GetSignalMutiChannelTestFinish()
            msg1 = mSock.Send(temp)
            mSocket.ReceiveTimeout = 2000
            bytesRec = mSocket.Receive(bytes)
        Catch
            bytesRec = 0
        End Try
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
        frmUserManage.ShowDialog()
        frmUserManage.Activate()

    End Sub

    Private Sub EMS涓婁紶ToolStripMenuItem_Click(sender As Object, e As EventArgs)
        FrmMesTask.ShowDialog()
        FrmMesTask.Activate()
    End Sub
    Private Sub EnsureCalibrationMenu()
        For Each item As ToolStripItem In MenuStrip.Items
            If item.Name = "CalibrationToolStripMenuItem" Then
                Return
            End If
        Next

        Dim calibrationMenu As New ToolStripMenuItem(ChrW(&H6821) & ChrW(&H51C6))
        calibrationMenu.Name = "CalibrationToolStripMenuItem"
        AddHandler calibrationMenu.Click, AddressOf CalibrationToolStripMenuItem_Click

        Dim insertIndex As Integer = -1
        For i As Integer = 0 To MenuStrip.Items.Count - 1
            If MenuStrip.Items(i).Text.StartsWith("MES") Then
                insertIndex = i
                RemoveHandler MenuStrip.Items(i).Click, AddressOf EMS涓婁紶ToolStripMenuItem_Click
                AddHandler MenuStrip.Items(i).Click, AddressOf EMS涓婁紶ToolStripMenuItem_Click
                Exit For
            End If
        Next

        If insertIndex < 0 Then
            MenuStrip.Items.Add(calibrationMenu)
        Else
            MenuStrip.Items.Insert(insertIndex, calibrationMenu)
        End If
    End Sub

    Private Sub CalibrationToolStripMenuItem_Click(sender As Object, e As EventArgs)
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
