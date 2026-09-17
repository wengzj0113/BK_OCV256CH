Imports System.Net
Imports System.Net.Sockets
Imports MpFByteArrDLL
Imports CmdMemberDLL
Imports System.Threading.Thread
Imports System.Threading

Public Class frmDebug
    Public mSock As SocketCtrolCmd
    Public FrmLink As New List(Of frmDebug)
    Dim mCmdCls As ICmdFormat
    Dim mType As New Dictionary(Of String, ICmdFormat)
    Public mSocket As Socket

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


End Class
