Imports System.Net.Sockets
Imports System.Net
Imports System.Threading
Imports System.IO.MemoryMappedFiles
Imports MpFByteArrDLL
Imports CmdMemberDLL

Public Delegate Sub ProcessBuffer(ByVal port As Integer, ByVal buffer As FByteArrBuild, ByRef PointPos As Integer)

Public MustInherit Class SockClass
    Public IsStopRev As Boolean = False
    Public MustOverride Function Send(ByVal b As Byte()) As String
    Public MustOverride Function Receive(ByRef msg As String) As Byte()
    Protected Function SendArr(ByVal mSocket As Socket, ByVal b As Byte()) As String
        Dim m As Integer = 0, r As Integer
        Dim msg As String = ""
        While m < b.Length
            r = 0
            Try
                r = mSocket.Send(b, m, b.Length - m, SocketFlags.None)
            Catch ex As Exception
                msg = ex.Message
                Exit While
            End Try
            m = m + r
        End While
        Return msg
    End Function
End Class
Public Class DataCh
    Public Vol As Single '电压
    Public Resistance As Single '内阻
    Public Bar As String
    Public TestTime As Date '测试时间
    Public IsTest As Boolean = False '测试状态
    Public DisCap As Single

    Public FstChCap As Single
    Public BuCap As Single
    Public V7 As Single
    Public VStart As Single '开始电压 2018/08/09 新增变量
    Public VEnd As Single '结束电压 2018/08/09 新增变量

    Public IsOK As Boolean = False '是否合格
    Public Gear As Integer = -1 '分容挡位 2017/10/16 李添就 新增变量 ；2017年11月份 此项数据改为“槽位编号”

    Public Remark As String '备注信息 用于注明不合格原因 2017/12/9 李添就 新增变量

    Public Leakage As Boolean = False '漏损量
    Public PreLeakage As Boolean = False '前期漏损量
    Public IsEmpty As Boolean = False
End Class
Public Class SocketCtrolCmd
    Inherits SockClass
    Public mSocket As Socket
    Dim buff As New FByteArrBuild
    Dim ptr As Integer = 0
    Dim bytes(5000) As Byte
    Dim PortType As Integer

    Dim endP As EndPoint
    Dim mProcessSub As ProcessBuffer
    Public Event SockClose(ByVal PortType As Integer, ByVal mSocket As Socket, ByVal exce As Exception)
    Public Sub Close()
        Try
            mSocket.Close()
        Catch ex As Exception

        End Try

    End Sub

    Sub New(ByVal mSocket As Socket, ByVal mSub As ProcessBuffer, ByVal mtype As Integer, ByVal endP As EndPoint)
        Me.mSocket = mSocket
        Me.PortType = mtype
        Me.mProcessSub = mSub
        Me.endP = endP

    End Sub
    Public Sub Start()

    End Sub
    Public Overrides Function Send(ByVal b As Byte()) As String
        Dim msg As String = ""
        'If (frmTest.ReloadSocket()) Then
        msg = SendArr(mSocket, b)
        'End If

        Return msg
    End Function
    Public Overloads Function Receive(ByRef msg As String, ByVal replay As CmdReplyStandard) As Byte()
        Dim pos = 0
        Dim b() As Byte = {}
        Dim c As Integer = 0

        mSocket.ReceiveTimeout = 10

        Do
            Try
                Dim bytesRec As Integer = mSocket.Receive(bytes)
                If bytesRec > 0 Then
                    ReDim Preserve b(pos + bytesRec - 1)
                    Array.Copy(bytes, 0, b, pos, bytesRec)
                    pos = pos + bytesRec

                End If
            Catch ex As Exception
                If b.Length > 0 Then
                    If replay.ParseUnitByte(b, 0, 1) Then
                        msg = ""
                        replay.RetTime = Date.Now
                        Return b
                    End If
                End If
            End Try
            c = c + 1
            If c > 50 Then
                Exit Do
            End If
        Loop


        msg = b.Length

        Return b
    End Function
    Public Overrides Function Receive(ByRef msg As String) As Byte()


        Dim pos = 0
        Dim b() As Byte = {}

        mSocket.ReceiveTimeout = 10
        Do
            Try
                Dim bytesRec As Integer = mSocket.Receive(bytes)
                If bytesRec > 0 Then
                    ReDim Preserve b(pos + bytesRec - 1)
                    Array.Copy(bytes, 0, b, pos, bytesRec)
                    pos = pos + bytesRec
                End If
            Catch ex As Exception
                If b.Length = 0 Then
                    msg = ex.Message
                End If
                Exit Do
            End Try
        Loop
        Return b
    End Function
    '''' <summary>
    '''' 发送指令数据给中位机
    '''' </summary>
    '''' <param name="Cmd"></param>
    '''' <returns></returns>
    'Private Function SendCmdMPBase(Cmd As Cmds) As String

    '    Dim msg As String = ""
    '    Dim tmp As MPCalibrationTCP.Data.Cls_TimeBug = Nothing

    '    Dim Arr As Byte() = Cmd.ClsSendData.MakeByte()

    '    Try
    '        AddHandler Port.DataReceived, AddressOf Command.ResponseData.DataReceived
    '        Dim Retry As Integer = 0

    '        While Retry < Cmd.ReCount + 1
    '            Retry += 1
    '            Port.WaitResponse = Cmd.WaitResponse
    '        End While
    '    Catch ex As Exception
    '        ' Handle the exception here
    '    End Try

    '    ' Add any necessary code here

    '    Return msg
    'End Function



    ''' <summary>
    ''' 读取版本信息 01h
    ''' </summary>
    ''' <returns></returns>
    Public Function GetSignalReadVersion() As Byte()
        Dim sendByte As Byte() = New Byte(7) {}
        sendByte(0) = &H0
        sendByte(1) = &HBB
        sendByte(2) = &HCC
        sendByte(3) = &HEE
        sendByte(4) = &H1
        sendByte(5) = &H0
        sendByte(6) = &H0
        sendByte(7) = &H0
        Return sendByte
    End Function
    ''' <summary>
    ''' 下压夹具命令 02h
    ''' </summary>
    ''' <returns></returns>
    Public Function GetSignalFixtureDown() As Byte()
        Dim lenthOfData As Integer = MAX_CHANNEL - 1 + 8
        Dim sendByte As Byte() = New Byte(lenthOfData) {}
        sendByte(0) = &H0
        sendByte(1) = &HBB
        sendByte(2) = &HCC
        sendByte(3) = &HEE
        sendByte(4) = &H2
        sendByte(5) = &H0
        sendByte(6) = &H1
        sendByte(7) = &H0

        For i As Integer = 8 To lenthOfData
            sendByte(i) = &H0
        Next

        Return sendByte
    End Function
    ''' <summary>
    ''' 松开夹具命令 23h
    ''' </summary>
    ''' <returns></returns>
    Public Function GetSignalFixtureUp() As Byte()
        Dim lenthOfData As Integer = MAX_CHANNEL - 1 + 8
        Dim sendByte As Byte() = New Byte(lenthOfData) {}
        sendByte(0) = &H0
        sendByte(1) = &HBB
        sendByte(2) = &HCC
        sendByte(3) = &HEE
        sendByte(4) = &H23
        sendByte(5) = &H0
        sendByte(6) = &H1
        sendByte(7) = &H0
        For i As Integer = 8 To lenthOfData
            sendByte(i) = &H0
        Next

        Return sendByte
    End Function
    ''' <summary>
    ''' 整柜复位(全部不选通,气缸上升) 2ah
    ''' </summary>
    ''' <returns></returns>

    Public Function GetSignalReset() As Byte()
        Dim sendByte As Byte() = New Byte(7) {}
        sendByte(0) = &H0
        sendByte(1) = &HBB
        sendByte(2) = &HCC
        sendByte(3) = &HEE
        sendByte(4) = &H2A
        sendByte(5) = &H0
        sendByte(6) = &H0
        sendByte(7) = &H0

        Return sendByte
    End Function

    ''' <summary>
    ''' 发送选通通道信息 19h
    ''' </summary>
    ''' <param name="channel"></param>
    ''' <returns></returns>
    Public Function GetSignalSelectChannel(ByVal channel As Integer) As Byte()
        Dim lenthOfData As Integer = MAX_CHANNEL - 1 + 8
        Dim sendByte As Byte() = New Byte(lenthOfData) {}
        sendByte(0) = &H0
        sendByte(1) = &HBB
        sendByte(2) = &HCC
        sendByte(3) = &HEE
        sendByte(4) = &H19
        sendByte(5) = &H0
        sendByte(6) = &H1
        sendByte(7) = &H0

        For i As Integer = 8 To lenthOfData
            If (i - 7) = channel Then
                sendByte(i) = &H1
            Else
                sendByte(i) = &H0
            End If
        Next
        Return sendByte
    End Function
    ''' <summary>
    ''' 多通道选通
    ''' </summary>
    ''' <param name="FirstTestNum"></param>
    ''' <param name="LastTestNum"></param>
    ''' <returns></returns>
    Public Function GetSignalSelectMutiChannel(ByVal FirstTestNum As Integer, ByVal LastTestNum As Integer) As Byte()
        Dim lenthOfData As Integer = MAX_CHANNEL - 1 + 8
        Dim sendByte As Byte() = New Byte(lenthOfData) {}
        sendByte(0) = &H0
        sendByte(1) = &HBB
        sendByte(2) = &HCC
        sendByte(3) = &HEE
        '命令号H13
        sendByte(4) = &H13
        '命令内容长度 为1
        sendByte(5) = &H0
        sendByte(6) = &H1
        sendByte(7) = &H0
        '命令内容：256个字节,需要选通的为1，不选通的为0
        For i As Integer = 8 To lenthOfData
            If (i >= (FirstTestNum + 7)) And (i <= (LastTestNum + 7)) Then
                sendByte(i) = &H1
            Else
                sendByte(i) = &H0
            End If
        Next
        Return sendByte
    End Function

    Public Function GetSignalSelectMutiChannelNG(ByVal retestArr As Int16()) As Byte()
        Dim lenthOfData As Integer = MAX_CHANNEL - 1 + 8
        Dim sendByte As Byte() = New Byte(lenthOfData) {}
        sendByte(0) = &H0
        sendByte(1) = &HBB
        sendByte(2) = &HCC
        sendByte(3) = &HEE
        '命令号H13
        sendByte(4) = &H13
        '命令内容长度 为1
        sendByte(5) = &H0
        sendByte(6) = &H1
        sendByte(7) = &H0
        '命令内容：256个字节,需要选通的为1，不选通的为0
        For i As Integer = 8 To lenthOfData
            sendByte(i) = &H0
        Next
        '把需要复测的点位置1
        For j As Integer = 8 To retestArr.Count - 1 + 8
            sendByte(j) = retestArr(j)
        Next
        Return sendByte
    End Function
    ''' <summary>
    ''' 10查询多通道测试完成命令 15h
    ''' </summary>
    ''' <returns></returns>
    Public Function GetSignalMutiChannelTestFinish() As Byte()
        Dim sendByte As Byte() = New Byte(8) {}
        sendByte(0) = &H0
        sendByte(1) = &HBB
        sendByte(2) = &HCC
        sendByte(3) = &HEE
        '命令号H15
        sendByte(4) = &H15
        '命令内容
        sendByte(5) = &H0
        sendByte(6) = &H0
        sendByte(7) = &H0
        Return sendByte
    End Function
    ''' <summary>
    ''' 查询气缸压合状态命令 14h
    ''' </summary>
    ''' <returns></returns>
    Public Function GetSignalTrayStatus() As Byte()
        Dim sendByte As Byte() = New Byte(8) {}
        sendByte(0) = &H0
        sendByte(1) = &HBB
        sendByte(2) = &HCC
        sendByte(3) = &HEE
        '命令号H14
        sendByte(4) = &H14
        '命令内容长度 为0
        sendByte(5) = &H0
        sendByte(6) = &H0
        sendByte(7) = &H0
        Return sendByte
    End Function
    ''' <summary>
    ''' 查询夹具板温度和IO状态信息 16h
    ''' </summary>
    ''' <returns></returns>
    Public Function GetSignalTrayTempertureAndIOStatus() As Byte()
        Dim sendByte As Byte() = New Byte(8) {}
        sendByte(0) = &H0
        sendByte(1) = &HBB
        sendByte(2) = &HCC
        sendByte(3) = &HEE
        '命令号H16
        sendByte(4) = &H16
        '命令内容长度 为0
        sendByte(5) = &H0
        sendByte(6) = &H0
        sendByte(7) = &H0
        Return sendByte
    End Function

    ''' <summary>
    ''' 发送上位机与MES系统连接状态,下发超温保护值 17h
    ''' </summary>
    ''' <returns></returns>
    Public Function SetOverTempertureAndMesStatus(ByVal ConnectStatus As Byte, ByVal setTemperateVal As Int16) As Byte()
        Dim sendByte As Byte() = New Byte(11) {}
        sendByte(0) = &H0
        sendByte(1) = &HBB
        sendByte(2) = &HCC
        sendByte(3) = &HEE
        '命令号H17
        sendByte(4) = &H17
        '命令内容长度 为1
        sendByte(5) = &H4
        sendByte(6) = &H0
        sendByte(7) = &H0
        '数据内容 4 字节
        sendByte(8) = ConnectStatus  '=1 连接  =0 断开
        '温度数据
        Dim byteArray() As Byte = BitConverter.GetBytes(setTemperateVal)
        sendByte(9) = byteArray(0)
        sendByte(10) = byteArray(1)
        sendByte(11) = &H0
        Return sendByte
    End Function

    ''' <summary>
    ''' 发送选择仪表 18h
    ''' </summary>
    ''' <returns></returns>
    Public Function SendSignalSellectInstrumentType(ByVal InstrumentType As Byte) As Byte()
        Dim sendByte As Byte() = New Byte(9) {}
        sendByte(0) = &H0
        sendByte(1) = &HBB
        sendByte(2) = &HCC
        sendByte(3) = &HEE
        '命令号H18
        sendByte(4) = &H18
        '命令内容长度 为1
        sendByte(5) = &H2
        sendByte(6) = &H0
        sendByte(7) = &H0
        '数据内容 2 字节
        sendByte(8) = InstrumentType  '=1 选择测量OCV(安捷伦 34461)  =0 选择测量ACIR(日置 3562）
        sendByte(9) = &H0
        Return sendByte
    End Function
End Class
