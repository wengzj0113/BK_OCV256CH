Imports System.Net
Imports System.Net.Sockets
Imports MpFByteArrDLL
Imports CmdMemberDLL
Imports System.Threading.Thread
Imports System.Threading
Imports System.Text
Imports System.Linq

Public Class frmDebug
    Public mSock As SocketCtrolCmd
    Public FrmLink As New List(Of frmDebug)
    Dim mCmdCls As ICmdFormat
    Dim mType As New Dictionary(Of String, ICmdFormat)
    Public mSocket As Socket
    Private modernDashboard As DebugDashboardControl

    Private Function MakeSock()
        Dim mClientTCP = New TcpClient()
        Return mClientTCP.Client     ' New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    End Function
    Private Function GetRomteEndPoint() As IPEndPoint
        Dim myIP As IPAddress = IPAddress.Parse(Initsetting.SCMIpAdd)
        Dim ipe As IPEndPoint = New IPEndPoint(myIP, Initsetting.SCM_port)
        Return ipe
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
    Private Sub sockTest()
        Dim temp As Byte()

        Try
            If mSock IsNot Nothing Then
                mSock.Close()
            End If
        Catch ex As Exception

        End Try

        Dim sock = MakeSock()
        Dim ipe As IPEndPoint = GetRomteEndPoint()

        Dim msg = ConnectTCP(sock, ipe)
        If msg = "" Then

            mSock = New SocketCtrolCmd(sock, AddressOf ParseBuffer, Initsetting.SCM_port, ipe)
            MsgBox("单片机通信成功！", vbOKOnly)
        Else
            'MsgBox("单片机通信失败！", vbOKOnly)
            MessageBox.Show(msg)
        End If

        temp = mSock.GetSignalTrayTempertureAndIOStatus()
        Dim msg1 As String = ""

        txtSend.Clear()
        Dim MyHex
        For intLoop = 0 To UBound(temp)
            MyHex = Hex(temp(intLoop))
            txtSend.Text = txtSend.Text & MyHex.ToString & Space(1)
        Next

        msg1 = mSock.Send(temp)
        Sleep(900)
        If msg1 <> "" Then
            MessageBox.Show(msg1)
            'Bnt_Send.Enabled = False
            Exit Sub
        End If

        SetSock(mSock)

        Dim bytes(5000) As Byte
        '获取接收的回复信息，放在bytes
        mSocket = sock
        Threading.Monitor.Enter(mSocket)
        Dim bytesRec As Integer = mSocket.Receive(bytes)
        Threading.Monitor.Exit(mSocket)

        txtRecieve_SCM.Clear()
        Dim MyHex1
        For intLoop = 0 To bytesRec - 1
            MyHex1 = Hex(bytes(intLoop))
            txtRecieve_SCM.Text = txtRecieve_SCM.Text & MyHex1.ToString & Space(1)
        Next
    End Sub
    Private Sub btnDTest_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDTest.Click
        Dim temp As Byte()

        Try
            If mSock IsNot Nothing Then
                mSock.Close()
            End If
        Catch ex As Exception

        End Try

        Dim sock = MakeSock()
        Dim ipe As IPEndPoint = GetRomteEndPoint()

        Dim msg = ConnectTCP(sock, ipe)
        If msg = "" Then

            mSock = New SocketCtrolCmd(sock, AddressOf ParseBuffer, Initsetting.SCM_port, ipe)
            MsgBox("单片机通信成功！", vbOKOnly)
        Else
            'MsgBox("单片机通信失败！", vbOKOnly)
            MessageBox.Show(msg)
        End If

        temp = mSock.GetSignalReset()
        Dim msg1 As String = ""

        txtSend.Clear()
        Dim MyHex
        For intLoop = 0 To UBound(temp)
            MyHex = Hex(temp(intLoop))
            txtSend.Text = txtSend.Text & MyHex.ToString & Space(1)
        Next

        msg1 = mSock.Send(temp)
        Sleep(900)
        If msg1 <> "" Then
            MessageBox.Show(msg1)
            'Bnt_Send.Enabled = False
            Exit Sub
        End If

        SetSock(mSock)

        Dim bytes(5000) As Byte
        '获取接收的回复信息，放在bytes
        mSocket = sock
        Threading.Monitor.Enter(mSocket)
        Dim bytesRec As Integer = mSocket.Receive(bytes)
        Threading.Monitor.Exit(mSocket)

        txtRecieve_SCM.Clear()
        Dim MyHex1
        For intLoop = 0 To bytesRec - 1
            MyHex1 = Hex(bytes(intLoop))
            txtRecieve_SCM.Text = txtRecieve_SCM.Text & MyHex1.ToString & Space(1)
        Next

    End Sub
    Private Sub DisplayText(ByRef Sock As Byte())

    End Sub
    Private Sub btnSend_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSend.Click
        Dim ch = cmbChannel.SelectedIndex + 1
        Dim temp As Byte()

        Try
            If mSock IsNot Nothing Then
                mSock.Close()
            End If
        Catch ex As Exception

        End Try

        Dim sock = MakeSock()
        Dim ipe As IPEndPoint = GetRomteEndPoint()
        Dim msg As String

        Try
            msg = ConnectTCP(sock, ipe)
            If msg = "" Then
                mSock = New SocketCtrolCmd(sock, AddressOf ParseBuffer, Initsetting.SCM_port, ipe)
                ' MsgBox("单片机通信成功！", vbOKOnly)
            Else
                'MsgBox("单片机通信失败！", vbOKOnly)
                'MessageBox.Show(msg)
                Return
            End If
        Catch ex As Exception

        End Try

        temp = mSock.GetSignalSelectChannel(ch)
        Dim msg1 As String = ""

        txtSend.Clear()
        Dim MyHex
        For intLoop = 0 To UBound(temp)
            MyHex = Hex(temp(intLoop))
            txtSend.Text = txtSend.Text & MyHex.ToString & Space(1)
        Next

        msg1 = mSock.Send(temp)

        SetSock(mSock)
        Me.Cursor = Cursors.Default
        Dim bytes(5000) As Byte

        mSocket = sock
        Threading.Monitor.Enter(mSocket)
        Dim bytesRec As Integer
        Sleep(900)
        Try
            bytesRec = mSocket.Receive(bytes)
        Catch ex As Exception

        End Try
        Threading.Monitor.Exit(mSocket)

        txtRecieve_SCM.Clear()
        Dim MyHex1
        For intLoop = 0 To bytesRec - 1
            MyHex1 = Hex(bytes(intLoop))
            txtRecieve_SCM.Text = txtRecieve_SCM.Text & MyHex1.ToString & Space(1)
        Next
    End Sub

    Private Sub frmDebug_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        If Not (Me.DesignMode OrElse System.ComponentModel.LicenseManager.UsageMode = System.ComponentModel.LicenseUsageMode.Designtime) AndAlso Not AuthorizationService.RequireAdministrator(Me) Then
            Close()
            Return
        End If

        InitializeModernDashboard()
        Return

        Dim intLoop As Integer
        For intLoop = 0 To MAX_CHANNEL - 1
            cmbChannel.Items.Add(intLoop + 1)
        Next
        Control.CheckForIllegalCrossThreadCalls = False  '防止多线程情况下显示冲突
        cmbChannel.SelectedIndex = 0
    End Sub

    Private Sub btnRead_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRead.Click
        Dim ReFlag_R As Short
        Dim voltage As Single
        Dim resistance As Single

        Try
            Me.Cursor = Cursors.WaitCursor
            ReFlag_R = Trans_ReadData_3561(voltage, resistance)
            If ReFlag_R <> 0 Then
                MsgBox("日置内阻仪通讯失败！", vbOKOnly)
            End If
            txtRecieve_SCM.Clear()
            txtRecieve.Text = "V(v)：" & voltage.ToString & vbCrLf
            txtRecieve.Text = txtRecieve.Text & "R(Ω)：" & resistance.ToString & vbCrLf
        Catch
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub
    Private Sub btnTestR_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTestR.Click
        Dim ReFlag_R As Short
        Me.Cursor = Cursors.WaitCursor
        ReFlag_R = ConnectFor3561()
        If ReFlag_R <> 0 Then
            MsgBox("日置内阻仪通讯失败！", vbOKOnly)
        Else
            MsgBox("日置内阻仪通信成功！", vbOKOnly)
        End If
        Me.Cursor = Cursors.Default
    End Sub


    Private Sub cmbChannel_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbChannel.SelectedIndexChanged
        Dim intPosition As Int16

        intPosition = Val(cmbChannel.Text)
        lblposition.Text = intPosition \ intRow + 1 & "-" & intPosition Mod intRow
    End Sub
    Private Sub Pic_left_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Pic_left.MouseDown
        Pic_left.BorderStyle = BorderStyle.Fixed3D
    End Sub

    Private Sub Pic_left_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Pic_left.MouseUp
        Dim NowIndex As Int16

        NowIndex = cmbChannel.SelectedIndex
        NowIndex = NowIndex - 1
        If NowIndex < 0 Then
            cmbChannel.SelectedIndex = 0
        Else
            cmbChannel.SelectedIndex = NowIndex
        End If
        Pic_left.BorderStyle = BorderStyle.None
    End Sub

    Private Sub Pic_right_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Pic_right.MouseDown
        Pic_right.BorderStyle = BorderStyle.Fixed3D

    End Sub

    Private Sub Pic_right_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Pic_right.MouseUp
        Dim NowIndex As Int16

        NowIndex = cmbChannel.SelectedIndex
        NowIndex = NowIndex + 1
        If NowIndex >= cmbChannel.Items.Count Then
            cmbChannel.SelectedIndex = 0
        Else
            cmbChannel.SelectedIndex = NowIndex
        End If
        Pic_right.BorderStyle = BorderStyle.None

    End Sub

    Private Sub Pic_left_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Pic_left.Click

    End Sub

    Private Sub btnClear_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClear.Click
        txtRecieve.Text = ""
        txtRecieve_SCM.Text = ""
        txtSend.Text = ""
    End Sub

    Private Sub btncancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Me.Dispose()
    End Sub

    Private Sub btnTrayState_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTrayState.Click
        Dim Buffer(5) As Byte
        Dim ReturnValue As Int16
        Me.Cursor = Cursors.WaitCursor

        If ReturnValue = 1 Then
            txtInfo.Text = "托盘已闭合！"
        ElseIf ReturnValue = 2 Then
            txtInfo.Text = "托盘已开启！"
        Else
            txtInfo.Text = "无法获取托盘状态！"
        End If
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub btnOpen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOpen.Click

        Dim temp As Byte()

        Try
            If mSock IsNot Nothing Then
                mSock.Close()
            End If
        Catch ex As Exception

        End Try

        Dim sock = MakeSock()
        Dim ipe As IPEndPoint = GetRomteEndPoint()
        Dim msg = ConnectTCP(sock, ipe)
        If msg = "" Then

            mSock = New SocketCtrolCmd(sock, AddressOf ParseBuffer, Initsetting.SCM_port, ipe)
            'MsgBox("单片机通信成功！", vbOKOnly)
        Else
            'MsgBox("单片机通信失败！", vbOKOnly)
            MessageBox.Show(msg)
            Return
        End If

        temp = mSock.GetSignalFixtureUp '松开夹具（打开托盘）
        Dim msg1 As String = ""
        txtSend.Clear()
        Dim MyHex
        For intLoop = 0 To UBound(temp)
            MyHex = Hex(temp(intLoop))
            txtSend.Text = txtSend.Text & MyHex.ToString & Space(1)
        Next
        msg1 = mSock.Send(temp)
        Sleep(900)
        SetSock(mSock)
        Me.Cursor = Cursors.Default
        Dim bytes(500) As Byte

        mSocket = sock
        Threading.Monitor.Enter(mSock)
        Dim bytesRec As Integer = mSocket.Receive(bytes)
        Threading.Monitor.Exit(mSock)
        txtRecieve_SCM.Clear()
        Dim MyHex1
        For intLoop = 0 To bytesRec - 1
            MyHex1 = Hex(bytes(intLoop))
            txtRecieve_SCM.Text = txtRecieve_SCM.Text & MyHex1.ToString & Space(1)
        Next

        'If ReturnValue = 0 Then
        '    txtInfo.Text = "托盘已闭合"
        'Else
        '    txtInfo.Text = "操作托盘失败！"
        'End If
    End Sub

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Dim temp As Byte()

        Try
            If mSock IsNot Nothing Then
                mSock.Close()
            End If
        Catch ex As Exception

        End Try

        Dim sock = MakeSock()
        Dim ipe As IPEndPoint = GetRomteEndPoint()

        Dim msg = ConnectTCP(sock, ipe)
        If msg = "" Then

            mSock = New SocketCtrolCmd(sock, AddressOf ParseBuffer, Initsetting.SCM_port, ipe)
            'MsgBox("单片机通信成功！", vbOKOnly)
        Else
            'MsgBox("单片机通信失败！", vbOKOnly)
            MessageBox.Show(msg)
            Return
        End If

        temp = mSock.GetSignalFixtureDown '松开夹具（打开托盘）
        Dim msg1 As String = ""
        txtSend.Clear()
        Dim MyHex
        For intLoop = 0 To UBound(temp)
            MyHex = Hex(temp(intLoop))
            txtSend.Text = txtSend.Text & MyHex.ToString & Space(1)
        Next
        msg1 = mSock.Send(temp)
        Sleep(900)
        SetSock(mSock)
        Me.Cursor = Cursors.Default
        Dim bytes(500) As Byte

        mSocket = sock
        Threading.Monitor.Enter(mSocket)
        Dim bytesRec As Integer = mSocket.Receive(bytes)
        Threading.Monitor.Exit(mSocket)

        'If ReturnValue = 0 Then
        '    txtInfo.Text = "托盘已闭合"
        'Else
        '    txtInfo.Text = "操作托盘失败！"
        'End If
        Me.Cursor = Cursors.Default
    End Sub

    Private Sub btnReset_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnReset.Click

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim temp As Byte()
        Try
            If mSock IsNot Nothing Then
                mSock.Close()
            End If
        Catch ex As Exception

        End Try

        Dim sock = MakeSock()
        Dim ipe As IPEndPoint = GetRomteEndPoint()

        Dim msg = ConnectTCP(sock, ipe)
        If msg = "" Then

            mSock = New SocketCtrolCmd(sock, AddressOf ParseBuffer, Initsetting.SCM_port, ipe)
            'Bnt_Send.Enabled = True
            'Bnt_LinkCmd.Enabled = True
        Else
            'Bnt_Send.Enabled = False
            'Bnt_LinkCmd.Enabled = False
            MessageBox.Show(msg)
        End If

        If Val(TextBox1.Text) = 0 Then
            temp = mSock.GetSignalReset()

        ElseIf Val(TextBox1.Text) = 1 Then
            temp = mSock.GetSignalSelectChannel(1)
        ElseIf Val(TextBox1.Text) = 2 Then
            temp = mSock.GetSignalSelectChannel(2)
        ElseIf Val(TextBox1.Text) = 3 Then
            temp = mSock.GetSignalTrayTempertureAndIOStatus()
        Else
            temp = mSock.GetSignalReadVersion()
        End If

        Dim msg1 As String = ""
        msg1 = mSock.Send(temp)

        If msg1 <> "" Then
            MessageBox.Show(msg1)
            'Bnt_Send.Enabled = False
            Exit Sub
        End If
        Dim MyHex
        For intLoop = 0 To UBound(temp)
            MyHex = Hex(temp(intLoop))
            txtSend.Text = txtSend.Text & MyHex.ToString & Space(1)
        Next
        ''Dim mCmdCls As ICmdFormat
        'Dim m = mType("发送工步设定")
        'mCmdCls = m
        Dim bytes(5000) As Byte

        mSocket = sock


        Threading.Monitor.Enter(mSock)
        Dim bytesRec As Integer = mSocket.Receive(bytes)
        Threading.Monitor.Exit(mSock)
        If msg = "" Then
            'Dim pos As Integer = 0
            'Try
            '    mCmdCls.Rely.ParseUnitByte(rec, pos, 1)
            'Catch ex As Exception
            '    MessageBox.Show("解析返回数据有误:数据长度=" & rec.Length)
            'End Try


        Else
            'MessageBox.Show("接收有误:数据长度=" & rec.Length)
        End If
        Dim MyHex1
        For intLoop = 0 To bytesRec - 1
            MyHex1 = Hex(bytes(intLoop))
            txtRecieve_SCM.Text = txtRecieve_SCM.Text & MyHex1.ToString & Space(1)
        Next
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles btnSockTest.Click
        Dim temp As Byte()
        Try
            If mSock IsNot Nothing Then
                mSock.Close()
            End If
        Catch ex As Exception

        End Try

        Dim sock = MakeSock()
        Dim ipe As IPEndPoint = GetRomteEndPoint()

        Dim msg = ConnectTCP(sock, ipe)
        If msg = "" Then

            mSock = New SocketCtrolCmd(sock, AddressOf ParseBuffer, Initsetting.SCM_port, ipe)
            'Bnt_Send.Enabled = True
            'Bnt_LinkCmd.Enabled = True
        Else
            'Bnt_Send.Enabled = False
            'Bnt_LinkCmd.Enabled = False
            MessageBox.Show(msg)
        End If

        If Val(TextBox1.Text) = 0 Then
            temp = mSock.GetSignalReset()

        ElseIf Val(TextBox1.Text) = 1 Then
            temp = mSock.GetSignalSelectChannel(1)
        ElseIf Val(TextBox1.Text) = 2 Then
            temp = mSock.GetSignalSelectChannel(2)
        ElseIf Val(TextBox1.Text) = 3 Then
            temp = mSock.GetSignalTrayTempertureAndIOStatus()
        Else
            temp = mSock.GetSignalReadVersion()
        End If

        Dim msg1 As String = ""
        msg1 = mSock.Send(temp)

        If msg1 <> "" Then
            MessageBox.Show(msg1)
            'Bnt_Send.Enabled = False
            Exit Sub
        End If
        Dim MyHex
        For intLoop = 0 To UBound(temp)
            MyHex = Hex(temp(intLoop))
            txtSend.Text = txtSend.Text & MyHex.ToString & Space(1)
        Next

        Dim bytes(5000) As Byte

        mSocket = sock


        Threading.Monitor.Enter(mSock)
        Dim bytesRec As Integer = mSocket.Receive(bytes)
        Threading.Monitor.Exit(mSock)
        If msg = "" Then
            'Dim pos As Integer = 0
            'Try
            '    mCmdCls.Rely.ParseUnitByte(rec, pos, 1)
            'Catch ex As Exception
            '    MessageBox.Show("解析返回数据有误:数据长度=" & rec.Length)
            'End Try
        Else
            'MessageBox.Show("接收有误:数据长度=" & rec.Length)
        End If

        Dim MyHex1
        For intLoop = 0 To bytesRec - 1
            MyHex1 = Hex(bytes(intLoop))
            txtRecieve_SCM.Text = txtRecieve_SCM.Text & MyHex1.ToString & Space(1)
        Next



        ' JIG第一个字节
        For bitIndex As Integer = 0 To 7
            ' 获取第 bitIndex 位的值
            Dim bitValue As Integer = (bytes(5) >> bitIndex) And 1
            '检查急停位的值，如果位1，报警，颜色为红色，闪烁
            If bitIndex = 2 Then
                If bitValue = 1 Then

                Else

                End If
            End If
            ' 打印结果，包括字节索引和位索引
            TextJIG1.Text = TextJIG1.Text & bitValue.ToString & Space(1)
            'Console.WriteLine("字节 " & bytes(5) & " 的第 " & bitIndex & " 位的值是：" & bitValue)
        Next

        ' JIG第二个字节
        For bitIndex As Integer = 0 To 7
            ' 获取第 bitIndex 位的值
            Dim bitValue As Integer = (bytes(6) >> bitIndex) And 1

            ' 打印结果，包括字节索引和位索引
            TextJIG2.Text = TextJIG2.Text & bitValue.ToString & Space(1)

        Next

        ' 创建一个整数类型的数组ARR2int，长度为4
        Dim TemperateList(3) As Integer

        ' 将ARR1的最后8个字节复制到ARR2int中
        ' 使用Array.Copy方法来复制字节数组的一部分
        ' 参数分别是源数组，源数组的起始索引，目标数组，目标数组的起始索引，以及要复制的元素数量
        Array.Copy(bytes, bytesRec - 8, TemperateList, 0, 4)

    End Sub

    Private Sub Button2_Click_1(sender As Object, e As EventArgs) Handles Button2.Click
        FrmSocketTest.Show()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Frm34461Ctrl.Show()

    End Sub

    Private Sub InitializeModernDashboard()
        If modernDashboard IsNot Nothing Then Return

        SuspendLayout()
        For Each legacyControl As Control In Controls
            legacyControl.Visible = False
        Next

        Text = "设备调试中心"
        ClientSize = New Size(1180, 760)
        MinimumSize = New Size(980, 680)
        MaximizeBox = True
        MinimizeBox = True
        BackColor = UiTheme.PageBackground

        modernDashboard = New DebugDashboardControl()
        AddHandler modernDashboard.DeviceTestRequested, AddressOf ModernDashboard_DeviceTestRequested
        AddHandler modernDashboard.AllDevicesTestRequested, AddressOf ModernDashboard_AllDevicesTestRequested
        AddHandler modernDashboard.CommunicationSendRequested, AddressOf ModernDashboard_CommunicationSendRequested
        AddHandler modernDashboard.ManualActionRequested, AddressOf ModernDashboard_ManualActionRequested
        AddHandler modernDashboard.CloseRequested, AddressOf ModernDashboard_CloseRequested
        Controls.Add(modernDashboard)
        modernDashboard.BringToFront()
        modernDashboard.AppendLog("INFO", "调试中心已打开；未执行自动设备操作")
        modernDashboard.SetRunningState(isRuning)
        ResumeLayout(True)
    End Sub

    Private Async Sub ModernDashboard_DeviceTestRequested(sender As Object, e As DebugDeviceEventArgs)
        If modernDashboard Is Nothing Then Return
        modernDashboard.SetDeviceTesting(e.DeviceId, True)
        Try
            Dim result As DebugDeviceResult = Await System.Threading.Tasks.Task.Run(Function() DebugProbeService.Probe(e.DeviceId))
            If modernDashboard IsNot Nothing Then modernDashboard.SetDeviceResult(result)
        Catch ex As Exception
            If modernDashboard IsNot Nothing Then
                modernDashboard.SetDeviceResult(New DebugDeviceResult With {
                    .DeviceId = e.DeviceId,
                    .State = DebugDeviceState.Failed,
                    .Message = ex.Message,
                    .InterfaceDescription = DebugProbeService.GetInterfaceDescription(e.DeviceId),
                    .CheckedAt = DateTime.Now
                })
            End If
        Finally
            If modernDashboard IsNot Nothing Then modernDashboard.SetDeviceTesting(e.DeviceId, False)
        End Try
    End Sub

    Private Async Sub ModernDashboard_AllDevicesTestRequested(sender As Object, e As EventArgs)
        If modernDashboard Is Nothing OrElse isRuning Then Return
        modernDashboard.AppendLog("INFO", "开始按顺序检查全部设备")
        Dim deviceIds() As DebugDeviceId = {
            DebugDeviceId.ControlBoard,
            DebugDeviceId.MiddleMachine,
            DebugDeviceId.Hioki3562,
            DebugDeviceId.Keysight34461,
            DebugDeviceId.PlcTray,
            DebugDeviceId.IoCard,
            DebugDeviceId.Scanner,
            DebugDeviceId.Database,
            DebugDeviceId.Mes
        }
        For Each deviceId As DebugDeviceId In deviceIds
            If modernDashboard Is Nothing OrElse modernDashboard.IsDisposed Then Return
            Dim currentDeviceId As DebugDeviceId = deviceId
            modernDashboard.SetDeviceTesting(currentDeviceId, True)
            Try
                Dim result As DebugDeviceResult = Await System.Threading.Tasks.Task.Run(Function() DebugProbeService.Probe(currentDeviceId))
                modernDashboard.SetDeviceResult(result)
            Catch ex As Exception
                modernDashboard.AppendLog("ERROR", DebugProbeService.GetDeviceName(currentDeviceId) & "：" & ex.Message)
            Finally
                modernDashboard.SetDeviceTesting(currentDeviceId, False)
            End Try
        Next
        modernDashboard.AppendLog("INFO", "全部设备检查完成")
    End Sub

    Private Sub ModernDashboard_CommunicationSendRequested(sender As Object, e As DebugCommunicationRequestEventArgs)
        If modernDashboard Is Nothing Then Return
        If isRuning Then
            modernDashboard.AppendCommunicationExchange(False, Nothing, "正式测试运行中，已阻止发送")
            Return
        End If

        Try
            Dim port As System.IO.Ports.SerialPort = Nothing
            If e.InterfaceName.StartsWith("日置", StringComparison.OrdinalIgnoreCase) Then
                port = Com_3561
            Else
                port = Com_SCM
            End If
            If port Is Nothing OrElse Not port.IsOpen Then Throw New InvalidOperationException("接口串口未打开，请先在设备总览中检查连接")

            SyncLock port
                port.DiscardInBuffer()
                port.Write(e.Payload, 0, e.Payload.Length)
                Sleep(80)
                Dim responseBytes As Byte() = ReadAvailableSerialBytes(port)
                modernDashboard.AppendCommunicationExchange(True, responseBytes, "发送完成，接收长度：" & responseBytes.Length.ToString() & " 字节")
            End SyncLock
        Catch ex As Exception
            modernDashboard.AppendCommunicationExchange(False, Nothing, ex.Message)
        End Try
    End Sub

    Private Shared Function ReadAvailableSerialBytes(port As System.IO.Ports.SerialPort) As Byte()
        Dim available As Integer = port.BytesToRead
        If available <= 0 Then Return New Byte() {}
        Dim buffer(available - 1) As Byte
        Dim offset As Integer = 0
        While offset < buffer.Length
            Dim count As Integer = port.Read(buffer, offset, buffer.Length - offset)
            If count <= 0 Then Exit While
            offset += count
        End While
        If offset = buffer.Length Then Return buffer
        Array.Resize(buffer, offset)
        Return buffer
    End Function

    Private Sub ModernDashboard_ManualActionRequested(sender As Object, e As DebugManualActionEventArgs)
        If modernDashboard Is Nothing Then Return
        If isRuning Then
            modernDashboard.AppendLog("WARN", "手动控制：正式测试运行中，已阻止操作")
            Return
        End If

        Try
            Select Case e.ActionName
                Case "ReadMiddleMachineStatus"
                    Dim middleMachine As New SerialMiddleMachineClient(GetScmPort(), AddressOf DebugTrace)
                    middleMachine.EnsureOpen()
                    Dim status() As UShort = middleMachine.ReadBasicStatusRegisters()
                    modernDashboard.AppendLog("INFO", "中位机基础状态读取成功：" & String.Join(" ", status.Select(Function(value) value.ToString())))
                Case "ResetMiddleMachine"
                    Dim middleMachine As New SerialMiddleMachineClient(GetScmPort(), AddressOf DebugTrace)
                    middleMachine.EnsureOpen()
                    middleMachine.ResetToSafeState()
                    modernDashboard.AppendLog("INFO", "中位机已恢复安全状态")
                Case "StopMiddleMachine"
                    Dim middleMachine As New SerialMiddleMachineClient(GetScmPort(), AddressOf DebugTrace)
                    middleMachine.EnsureOpen()
                    middleMachine.StopAndClearLists()
                    modernDashboard.AppendLog("INFO", "中位机已停止并清除通道计划")
                Case "ConfigureChannel"
                    Dim middleMachine As New SerialMiddleMachineClient(GetScmPort(), AddressOf DebugTrace)
                    middleMachine.EnsureOpen()
                    middleMachine.ConfigureChannelList(New Integer() {e.Value}.ToList())
                    modernDashboard.AppendLog("INFO", "已下发通道 CH" & e.Value.ToString() & " 选择，不会启动测量")
                Case "ReadTrayStatus"
                    EnsurePlcConnected()
                    modernDashboard.AppendLog("INFO", If(plcController.ReadTrayStatus(), "PLC 托盘状态：闭合", "PLC 托盘状态：开启"))
                Case "OpenTray"
                    EnsurePlcConnected()
                    modernDashboard.AppendLog("INFO", If(plcController.OpenTray(), "PLC 托盘已开启", "PLC 托盘开启失败"))
                Case "CloseTray"
                    EnsurePlcConnected()
                    modernDashboard.AppendLog("INFO", If(plcController.CloseTray(), "PLC 托盘已闭合", "PLC 托盘闭合失败"))
                Case "ResetPlc"
                    EnsurePlcConnected()
                    plcController.ResetTray()
                    modernDashboard.AppendLog("INFO", "PLC 托盘复位命令已发送")
            End Select
        Catch ex As Exception
            modernDashboard.AppendLog("ERROR", "手动控制失败：" & ex.Message)
        End Try
    End Sub

    Private Function GetScmPort() As System.IO.Ports.SerialPort
        If frmMDI Is Nothing OrElse frmMDI.Com_SCM Is Nothing Then Throw New InvalidOperationException("SCM 串口对象未初始化")
        Return frmMDI.Com_SCM
    End Function

    Private Sub EnsurePlcConnected()
        If plcAutomaticModeActive Then Throw New InvalidOperationException("PLC正在由自动测试流程独占，请先停止自动流程。")
        If plcController Is Nothing Then plcController = PlcControllerFactory.Create(Initsetting)
        If Not plcConnected AndAlso Not plcController.Connect() Then Throw New InvalidOperationException("PLC 连接失败")
        plcConnected = True
    End Sub

    Private Sub DebugTrace(message As String)
        If modernDashboard IsNot Nothing Then modernDashboard.AppendLog("TRACE", message)
    End Sub

    Private Sub ModernDashboard_CloseRequested(sender As Object, e As EventArgs)
        Close()
    End Sub

    Private Sub frmDebug_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        If modernDashboard Is Nothing Then Return
        RemoveHandler modernDashboard.DeviceTestRequested, AddressOf ModernDashboard_DeviceTestRequested
        RemoveHandler modernDashboard.AllDevicesTestRequested, AddressOf ModernDashboard_AllDevicesTestRequested
        RemoveHandler modernDashboard.CommunicationSendRequested, AddressOf ModernDashboard_CommunicationSendRequested
        RemoveHandler modernDashboard.ManualActionRequested, AddressOf ModernDashboard_ManualActionRequested
        RemoveHandler modernDashboard.CloseRequested, AddressOf ModernDashboard_CloseRequested
        modernDashboard = Nothing
    End Sub


End Class
