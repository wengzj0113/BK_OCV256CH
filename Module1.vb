Imports System.IO.Ports
Imports System.Threading.Thread
Imports System.Net.Sockets
Imports System.Net
Imports System.Threading
Imports System.Text
Imports MpFByteArrDLL
Imports Ivi.Visa.Interop
Imports Modbus.Device
Imports Telerik.SvgIcons

Module Module1
    '===========全局变量==================
    Public Initsetting As Initset
    Public frmMDI As MDIForm
    Public Com_SCM As SerialPort
    Public Tcp_Scm As SocketCtrolCmd
    Public Com_3561 As SerialPort
    Public Com_scaner As SerialPort
    Public Com_IOCard As SerialPort
    Public plcController As IPlcController
    Public plcConnected As Boolean = False
    Public Const MAX_CHANNEL = 256
    Public CommunicationCounts As Integer
    Public FirstCom As Boolean
    Public blnOnLine As Boolean   '设备是否联机正常
    Public controlBoardConnected As Boolean = False   '控制板串口已完成协议握手
    Public databaseConnected As Boolean = False   'OCV_DATA 数据库连接状态
    Public ApplicationShuttingDown As Boolean = False   '应用程序正在退出，禁止后台 UI 更新
    Public RecieveData(MAX_CHANNEL - 1) As Bat_info
    Public Const intRow As Int16 = 16
    Public Const intCol As Int16 = 16
    Public g_objLv As Dictionary(Of String, String)
    Public g_objCN As Dictionary(Of String, String)
    Public Data As DataCh() '每一个电池得信息
    Public IsRec As Boolean
    Public ReFlag_34461 As Short = -1
    Public IsSending As Boolean = False
    Public recDataof34461 As Double
    Public ReturnValue34461 As String '接收仪表返回的读数值
    Dim ResourcePort As New Ivi.Visa.Interop.ResourceManager
    Dim visaSession As Ivi.Visa.Interop.FormattedIO488 = Nothing
    Private Const VisaInstrumentIoTimeoutMs As Integer = 3000
    Private Const VisaShutdownTimeoutMs As Integer = 1000
    Public bytes_IODI As Byte() = {&H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0}
    Public bytes_IODO As Byte() = {&H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0}
    Public CalibrateR As Single = 0 '初始化内阻测试机校准值为0
    Public dataOfTemPeraturesAndIO As Byte()
    '定时发送命令16H，发送数据为send111，接收反馈上行数据为rec111
    Public send111 As String
    Public rec111 As String
    Public isRuning As Boolean
    Public isAskOcvNum As Boolean
    Public alarmStatus As Boolean = False
    Public retestArray As Int16() = New Int16(255) {}  '复测点位数组
    Public testArray As Int16() = New Int16(255) {}    '测试点位数组
    '保存校准值的数组和文件如下
    Public calibrateValueArray(MAX_CHANNEL) As Double
    Public currentOCVNum As Integer
    '公共 sql 语句
    Public pubConnectionString As String
    '有效电池个数
    Public avalibleCount As Integer
    Public trayCodeSet As String
    Public processNumSet As String
    Public operatorName As String

    'Public Probelifespan As Integer '探针的可使用次数 初始值30000
    'Public ProbeUsedCount As Integer '探针已经使用的次数 初始值 0
    'Public PsdOfAdmin As String '系统管理员密码
    'Public PsdOfOper As String '操作员密码
    'Public PsdOfGuest As String '游客密码

    '共享 MES HttpClient 池:7 个 Upload 类复用,避免句柄累积 (R2-12)
    Public MesHttpClient As New System.Net.Http.HttpClient()



    '-------------------数据结构----------------------
    Public Structure Initset
        Dim ScanerPort As Integer
        Dim ScanerRate As Integer
        '中位机IP地址  weng 20220802 添加
        Dim SCMIpAdd As String
        Dim SCM_port As Integer  '中位机网络端口

        'IO控制卡参数  weng 20230514修改
        Dim IOPort As Integer
        Dim IORate As Integer

        '数字万用表IP地址参数  weng 20230518修改
        Dim IpAddOf34461 As String

        '日置内阻仪
        Dim ComRateMode As String
        Dim Recom As Integer
        Dim ReRate As Integer
        '内阻范围
        Dim upResistance As Single
        Dim downResistance As Single
        '电压范围
        Dim upVoltage As Single
        Dim downVoltage As Single
        '数据库管理
        Dim chkDatabase As Boolean
        Dim strDatabase As String
        '数据库保存地址
        Dim sPath As String
        '自动上传
        Dim autoUpdate As Boolean
        '电压校准值
        Dim sngV_B As Single
        '内阻校准值
        Dim sngR_B As Single
        '条码长度设定
        Dim txtCodeLenth As Integer

        Dim strUserID As String '数据库用户名
        Dim strpwd As String '数据库用户密码

        Dim delayTime As String '通道切换时间

        Dim Probelifespan As Integer '探针的可使用次数 初始值30000
        Dim ProbeUsedCount As Integer '探针已经使用的次数 初始值 0
        '用户管理
        Dim PsdOfAdmin As String '系统管理员密码
        Dim PsdOfOper As String '操作员密码
        Dim PsdOfGuest As String '游客密码
        'MES管理
        Dim MesApiAddress As String 'MES API 地址
        Dim EquipPC_ID As String 'MES 设备电脑 ID (R2-09)
        Dim EquipPC_Password As String 'MES 设备电脑密码 (R2-09,不再源码硬编码)
        '当前操作员
        Dim curOperator As String '当前操作员
        '校准模块阻值
        Dim cailibrateModelValue As Single
        '超温报警值
        Dim overTemperateValue As Integer
        '工作站选择
        Dim stationSelect As Integer
        '中位机串口参数
        Dim SCMComPort As Integer
        Dim SCMComRate As Integer
        'PLC 独立通讯参数：0=Modbus RTU，1=Modbus TCP
        Dim PLCMode As Integer
        Dim PLCIP As String
        Dim PLCTcpPort As Integer
        Dim PLCComPort As Integer
        Dim PLCComRate As Integer
        'NG复测3562串口参数
        Dim Retest3562Port As Integer
        Dim Retest3562Rate As Integer
        'SCM控制板时间参数（毫秒）
        Dim ScmRelaySettleMs As Integer
        Dim ScmChannelSettleMs As Integer
        Dim ScmAutoStepMs As Integer
        Dim ScmTriggerPulseMs As Integer
        Dim ScmPhaseGuardMs As Integer
        '主页面通道状态颜色（ARGB，追加字段，兼容旧配置）
        Dim ChannelPassColorArgb As Integer
        Dim ChannelTestingColorArgb As Integer
        Dim ChannelIdleColorArgb As Integer
        Dim ChannelResistanceNgColorArgb As Integer
        Dim ChannelVoltageNgColorArgb As Integer
        Dim ChannelSelectedColorArgb As Integer

    End Structure
    Public Structure Bat_struct
        Dim Left As Single
        Dim Top As Single
        Dim width As Single
        Dim Hight As Single
    End Structure
    Public Structure Bat_info
        Dim BatCode As String '电池编码
        Dim Voltage As Single
        Dim Resistance As Single
        Dim TestTime As Date
        Dim Result As String '测试结果
    End Structure
    ' 定义测试数据的结构
    Public Structure TestDataInfo
        Dim SmCode As String
        Dim Result As String
        Dim ChNum As Integer
        Dim IR1 As Single
        Dim OCV1 As Single
        Dim Time1 As Date
        Dim IR2 As Single
        Dim OCV2 As Single
        Dim Time2 As Date
        Dim TA1 As String
        Dim TA2 As String
        Dim TA3 As String
        Dim TA4 As String
    End Structure
    Public Function IsNG(ByVal Voltage As Single, ByVal Resistance As Single) As Short
        IsNG = 0
        If Voltage * 1000 > Initsetting.upVoltage Or Voltage * 1000 < Initsetting.downVoltage Then
            IsNG = 1
            'frmTest.VolNgTxt = "电压超出合格范围。"
        End If
        If Resistance > Initsetting.upResistance Or Resistance < Initsetting.downResistance Then
            IsNG = 2
            'frmTest.ResNgTxt = "内阻超出合格范围。"
        End If
    End Function
    ' IO 板由控制板内部控制。保留历史函数实现仅用于源码兼容参考，编译时不启用，
    ' 防止上位机继续打开、读写与控制板共用的 COM12。
#If False Then
    ''' <summary>
    ''' 控制气缸 参数 OPType=1,压紧亮灯 OPType=2,松开灭灯,切换边电压的继电器松开Y5=0
    ''' </summary>
    ''' <param name="OPType"></param>
    ''' <returns></returns>
    Public Function IOCardSetOperation(ByVal OPType As Integer) As String
        Dim reflag_Io As Short
        Dim RecvStr As String
        Dim bytes1 As Byte() = {&H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0}
        Dim bytes As Byte() = {&H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0}
        Dim bytesClose As Byte() = {&H48, &H3A, &H1, &H57, &H0, &H1, &H0, &H1, &H0, &H0, &H0, &H0, &HDC, &H45, &H44}
        Dim bytesOpen As Byte() = {&H48, &H3A, &H1, &H57, &H1, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &HDB, &H45, &H44}

        If OPType = 1 Then
            bytes = bytesClose
        Else
            bytes = bytesOpen
        End If

        RecvStr = ""
        '再次检查端口是否正常
        reflag_Io = CheckCom_IOCard()
        If reflag_Io = -1 Then
            MsgBox("IO卡通信失败！！"， vbOKOnly)
            IOCardSetOperation = ""
            Exit Function
        End If
        Dim len As Integer

        len = bytes.Length()
        Try
            Com_IOCard.Write(bytes, 0, len)
        Catch ex As Exception
            IOCardSetOperation = ""
            Exit Function
        End Try

        Dim receivedData As New List(Of Byte)()
        Sleep(300)
        While Com_IOCard.BytesToRead > 0
            Dim data As Integer = Com_IOCard.ReadByte()
            receivedData.Add(CByte(data))
        End While
        Dim k As Integer
        k = 0
        For Each Obj In receivedData
            RecvStr += Obj.ToString & ","
            bytes1(k) = Obj
            k += 1
        Next Obj
        'If bytes1(4) <> channel Or bytes1(5) <> WorkStatus Then
        '    IOCardSetOperation = ""
        '    MsgBox("IO卡操作不正常，停止测试"， vbOKOnly)
        'End If

        IOCardSetOperation = RecvStr

    End Function
    ''' <summary>
    ''' 设定4个继电器的闭合或打开，
    ''' </summary>
    ''' <param name="OPType"></param>
    ''' <returns></returns>
    Public Function IOCardSetAcirOrOcv(ByVal OPType As String) As String
        Dim reflag_Io As Short
        Dim RecvStr As String
        Dim bytes1 As Byte() = {&H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0}
        Dim bytes As Byte() = {&H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0}
        Dim bytesACIR As Byte() = {&H48, &H3A, &H1, &H57, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &HDA, &H45, &H44}
        Dim bytesOCV As Byte() = {&H48, &H3A, &H1, &H57, &H1, &H1, &H1, &H1, &H0, &H0, &H0, &H0, &HDE, &H45, &H44}

        If OPType = "ACIR" Then
            bytes = bytesACIR
        ElseIf OPType = "OCV" Then
            bytes = bytesOCV
        End If

        RecvStr = ""
        '再次检查端口是否正常
        reflag_Io = CheckCom_IOCard()
        If reflag_Io = -1 Then
            MsgBox("IO卡通信失败！！"， vbOKOnly)
            IOCardSetAcirOrOcv = ""
            Exit Function
        End If

        Dim len As Integer
        len = bytes.Length()
        Try
            frmMDI.Com_IOCard.Write(bytes, 0, len)
        Catch ex As Exception
            IOCardSetAcirOrOcv = ""
            Exit Function
        End Try

        Dim receivedData As New List(Of Byte)()
        Sleep(300)
        While frmMDI.Com_IOCard.BytesToRead > 0
            Dim data As Integer = frmMDI.Com_IOCard.ReadByte()
            receivedData.Add(CByte(data))
        End While
        Dim k As Integer
        k = 0
        For Each Obj In receivedData
            RecvStr += Obj.ToString & ","
            bytes1(k) = Obj
            k += 1
        Next Obj
        IOCardSetAcirOrOcv = RecvStr
    End Function
    ''' <summary>
    ''' 闭合或总控某个继电器，channel决定位置
    ''' </summary>
    ''' <param name="channel"></param>
    ''' <returns></returns>
    Public Function IOCardSetCh(ByVal channel As Byte, ByVal WorkStatus As Byte) As String
        Dim reflag_Io As Short
        Dim RecvStr As String
        Dim bytes1 As Byte() = {&H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0}
        Dim bytes As Byte() = {&H48, &H3A, &H1, &H70, &H0, &H0, &H0, &H0, &H45, &H44}
        bytes(4) = channel
        bytes(5) = WorkStatus

        RecvStr = ""
        '再次检查端口是否正常
        reflag_Io = CheckCom_IOCard()
        If reflag_Io = -1 Then
            MsgBox("IO卡通信失败！！"， vbOKOnly)
            IOCardSetCh = ""
            Exit Function
        End If
        Dim len As Integer

        len = bytes.Length()
        Try
            Com_IOCard.Write(bytes, 0, len)
        Catch ex As Exception
            IOCardSetCh = ""
            Exit Function
        End Try

        Dim receivedData As New List(Of Byte)()
        Sleep(300)
        While Com_IOCard.BytesToRead > 0
            Dim data As Integer = Com_IOCard.ReadByte()
            receivedData.Add(CByte(data))
        End While
        Dim k As Integer
        k = 0
        For Each Obj In receivedData
            RecvStr += Obj.ToString & ","
            bytes1(k) = Obj
            k += 1
        Next Obj
        If bytes1(4) <> channel Or bytes1(5) <> WorkStatus Then
            IOCardSetCh = ""
            MsgBox("IO卡操作不正常，停止测试"， vbOKOnly)
        End If

        IOCardSetCh = RecvStr

    End Function

    ''' <summary>
    ''' 读取IO卡的开关量输出状态（DO）和输入量（DI），参数为2（DO）,参数为1（DI）
    ''' </summary>
    ''' <param name="dataType"></param>
    ''' <returns></returns>
    Public Function IOCard_ReadDoDi（dataType As String） As String

        Dim reflag_Io As Short
        Dim RecvStr As String
        Dim bytes As Byte() = {&H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0}
        Dim bytes_Rec As Byte() = {&H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0}
        If (dataType = "2") Then
            bytes = {&H48, &H3A, &H1, &H53, &HAA, &HAA, &HAA, &HAA, &HAA, &HAA, &HAA, &HAA, &H26, &H45, &H44}
        End If
        If (dataType = "1") Then
            bytes = {&H48, &H3A, &H1, &H52, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &HD5, &H45, &H44}
        End If

        RecvStr = ""
        '再次检查端口是否正常
        reflag_Io = CheckCom_IOCard()
        If reflag_Io = -1 Then
            MsgBox("IO卡通信失败！！"， vbOKOnly)
            IOCard_ReadDoDi = ""
            Exit Function
        End If
        Dim len As Integer

        len = bytes.Length()
        Try
            Com_IOCard.Write(bytes, 0, len)
        Catch ex As Exception
            IOCard_ReadDoDi = ""
            Exit Function
        End Try

        Dim receivedData As New List(Of Byte)()
        Sleep(300)
        While Com_IOCard.BytesToRead > 0
            Dim data As Integer = Com_IOCard.ReadByte()
            receivedData.Add(CByte(data))
        End While
        ' bytes_Rec =receivedData
        Dim k = 0
        For Each Obj In receivedData
            RecvStr += Obj.ToString & ","
            bytes_Rec(k) = Obj
            k += 1
        Next Obj
        If dataType = "1" Then
            bytes_IODI = bytes_Rec
        Else
            bytes_IODO = bytes_Rec
        End If

        IOCard_ReadDoDi = RecvStr
        'Com_IOCard.Close()
    End Function
    Public Function IOCard_ReadDi() As String
        'Dim RecvStr As String
        Dim bytes As Byte()

        bytes = {&H48, &H3A, &H1, &H52, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &HD5, &H45, &H44}

        Dim len As Integer
        len = bytes.Length()
        If Com_IOCard.IsOpen Then
            MDIForm.Com_IOCard.Close()
        End If

        Com_IOCard.Open()
        Try
            MDIForm.Com_IOCard.Write(bytes, 0, len)
        Catch ex As Exception
            IOCard_ReadDi = ""
            Exit Function
        End Try

        Dim receivedData As New List(Of Byte)()
        Sleep(300)
        While MDIForm.Com_IOCard.BytesToRead > 0
            Dim data As Integer = MDIForm.Com_IOCard.ReadByte()
            receivedData.Add(CByte(data))
        End While

        If receivedData.Item(6) = 0 Then
            IOCardSetOperation(2)
        End If

        IOCard_ReadDi = ""
        MDIForm.Com_IOCard.Close()
    End Function

    '***************************************************************
#End If

    ''' <summary>
    ''' IO 板动作由控制板执行，上位机不再直接发送 IO 协议。
    ''' 保留历史入口，避免旧调用方改变，但不访问任何串口。
    ''' </summary>
    Public Function IOCardSetOperation(ByVal OPType As Integer) As String
        System.Diagnostics.Debug.WriteLine("IO板动作已旁路，由控制板执行。Operation=" & OPType.ToString())
        Return "BYPASSED"
    End Function

    Public Function IOCardSetAcirOrOcv(ByVal OPType As String) As String
        System.Diagnostics.Debug.WriteLine("IO板模式切换已旁路，由控制板执行。Mode=" & OPType)
        Return "BYPASSED"
    End Function

    Public Function IOCardSetCh(ByVal channel As Byte, ByVal WorkStatus As Byte) As String
        System.Diagnostics.Debug.WriteLine("IO板通道动作已旁路，由控制板执行。Channel=" & channel.ToString() & ", Status=" & WorkStatus.ToString())
        Return "BYPASSED"
    End Function

    Public Function IOCard_ReadDoDi(dataType As String) As String
        System.Diagnostics.Debug.WriteLine("IO板DI/DO读取已旁路，由控制板执行。Type=" & dataType)
        Return "BYPASSED"
    End Function

    Public Function IOCard_ReadDi() As String
        System.Diagnostics.Debug.WriteLine("IO板DI读取已旁路，由控制板执行。")
        Return "BYPASSED"
    End Function

    '***************************************************************
    Public Function Communication(ByVal Buffer() As Byte, ByVal intCount As Int16) As Short
        '通讯失败返回-1;托盘未放好-2 ；接收数据错误-3;托盘闭合1；托盘开启2communicatio
        Dim sendBuf() As Byte
        Dim ReBuf() As Byte = New Byte() {}
        Dim REPEATECOUNT As Short
        Dim Response As Boolean
        Dim DataCount As Short
        Dim Repeat As Short
        Dim ReOrder As String = ""
        Dim temp_long As Integer
        Dim I As Int16
        sendBuf = Buffer
        Response = False
        REPEATECOUNT = 20
        Repeat = 0
Loop_Begin:
        'send data
        Try
            frmMDI.Com_SCM.Write(Buffer, 0, intCount)
        Catch ex As Exception
            Communication = -1
            Exit Function
        End Try
        Sleep(1000)

        'the length of return data 
        Do
            DataCount = frmMDI.Com_SCM.BytesToRead
            If DataCount >= 6 Then
                ReDim ReBuf(DataCount - 1)
                frmMDI.Com_SCM.Read(ReBuf, 0, DataCount)
                For I = 0 To 5
                    My.Forms.frmDebug.txtRecieve_SCM.Text = My.Forms.frmDebug.txtRecieve_SCM.Text & ReBuf(I) & "  "
                Next
                ReOrder = ReBuf(2)
                Response = True
                Exit Do
            End If
            temp_long = temp_long + 1
            If temp_long > CommunicationCounts Then Exit Do
        Loop

        'if then length of return data<6,repeat  REPEATECOUNT
        If Response = False Then
            If Repeat < REPEATECOUNT Then '---------如果连续通讯REPEATECOUNT次都失败，则认为通讯失败
                Repeat = Repeat + 1
                ReDim ReBuf(frmMDI.Com_SCM.BytesToRead - 1)
                frmMDI.Com_SCM.Read(ReBuf, 0, frmMDI.Com_SCM.BytesToRead)
                GoTo Loop_Begin
            End If
            Communication = -1
            Exit Function
        End If
        If Response = True Then
            If ReOrder = 2 Then
                If ReBuf(4) = 0 Then
                    'The Tray isn't ready
                    Communication = -2
                ElseIf ReBuf(4) = Buffer(4) Then
                    Communication = 0
                Else
                    Communication = -3
                End If
            ElseIf ReOrder = 253 Or ReOrder = 254 Then
                'Error
                Repeat = Repeat + 1
                If Repeat > 5 Then
                    Communication = -3
                    Exit Function
                End If
                Sleep(50)
                GoTo Loop_Begin
            ElseIf ReOrder = 3 Then
                'control Tray close
                Communication = 0
            ElseIf ReOrder = 4 Then
                'control Tray open
                Communication = 0
            ElseIf ReOrder = 5 Then
                'return the state of tray
                If ReBuf(4) = 0 Then
                    Communication = 1
                ElseIf ReBuf(4) = 1 Then
                    Communication = 2
                Else
                    Communication = -3
                End If
            Else
                Communication = -1
            End If
        Else
            Communication = -1
        End If
    End Function
    Public Function ConnectFor3561(Optional ByVal suppressDialogs As Boolean = False) As Integer
        '---------返回值：-1    未联机；0   正常----------------
        Dim RecvStr As String
        Dim intCounter As Integer
        Dim Start As Single
        Dim Timer As Single
        intCounter = 3
        RecvStr = ""
        Do
            CheckCom_3561(suppressDialogs)
            Try
                Com_3561.DiscardInBuffer()
                Com_3561.DiscardOutBuffer()
                Com_3561.Write("*RST" & vbLf)  '仪器复位初始化 20220823 weng
                Com_3561.Write("*IDN?" & vbLf)
            Catch ex As Exception
                ConnectFor3561 = -1
                Exit Function
            End Try
            Start = Timer
            Do While Timer <= Start + 0.1 '这是什么意思
                Timer = Now.TimeOfDay.Hours * 3600 + Now.TimeOfDay.Minutes * 60 + Now.TimeOfDay.Seconds
                RecvStr = RecvStr + Com_3561.ReadExisting
                If RecvStr Like "HIOKI,*" Then
                    intCounter = 1
                End If
                If Not suppressDialogs Then System.Windows.Forms.Application.DoEvents()
            Loop
            intCounter = intCounter - 1
        Loop Until (intCounter = 0)
        If RecvStr Like "HIOKI,*" Then
            ' MsgBox("内阻仪通信成功！", vbOKOnly)
            '初始化内阻仪
            InitRes(Not suppressDialogs)
            ConnectFor3561 = 0
        Else
            If Not suppressDialogs Then MsgBox("内阻仪通信失败！", vbOKOnly)
            ConnectFor3561 = -1
        End If
    End Function
    Public Sub InitRes(Optional ByVal pumpUi As Boolean = True)

        'Dim intCounter As Integer
        Dim RecvStr As String

        Dim Timer As Single
        'Static TempUpperR As Single
        'Static TempDownerR As Single
        'Static TempUpperV As Single
        'Static TempDownerV As Single
        Static blnInitRun As Boolean
        Dim Start
        If pumpUi Then System.Windows.Forms.Application.DoEvents()
        If Not blnInitRun Then
            RecvStr = ""
            With Com_3561
                .DiscardInBuffer()
                .DiscardOutBuffer()
                Select Case Initsetting.ComRateMode
                    Case "低速"
                        .Write("SAMPle:RATE SLOW" & vbCrLf)
                    Case "中速"
                        .Write("SAMPle:RATE MED" & vbCrLf)
                    Case "高速"
                        .Write("SAMPle:RATE FAST" & vbCrLf)
                    Case "超快"
                        .Write("SAMPle:RATE EXF" & vbCrLf)
                End Select

            End With


            Com_3561.Write(":RES:RANG 300E-3" & vbLf)
            Threading.Thread.Sleep(50)

            RecvStr = ""
            Com_3561.DiscardInBuffer()
            Com_3561.DiscardOutBuffer()
            Com_3561.Write(":MEMory:State On" & vbLf)
            Start = Timer
            Do While Timer < Start + 0.1
                Timer = Now.TimeOfDay.Hours * 3600 + Now.TimeOfDay.Minutes * 60 + Now.TimeOfDay.Seconds
                RecvStr = RecvStr + Com_3561.ReadExisting
                If pumpUi Then System.Windows.Forms.Application.DoEvents()
            Loop
            blnInitRun = True
        End If
    End Sub

    Public Sub main()
        '启动直接进入主窗体;登录作为一级菜单操作,默认使用 Admin 身份。
        operatorName = "Admin"
        '只读取本地配置，不在消息循环建立前启动任何设备连接或轮询。
        Call ReadInitFile()
        Dim mainWindow As New MDIForm()
        mainWindow.operTypeNum = 1
        frmMDI = mainWindow
        Application.Run(mainWindow)
    End Sub

    Public Function MainwindowsShow(ByVal frmMDI1 As MDIForm) As Short
        '兼容层:立即显示主窗并返回 True。设备检查由首屏后的后台阶段启动。
        frmMDI = frmMDI1
        blnOnLine = True
        Call ReadInitFile()
        If frmMDI1 IsNot Nothing AndAlso Not frmMDI1.IsDisposed Then
            frmMDI1.Show()
        End If
        Return blnOnLine
    End Function

    '首屏完成后的异步设备检查入口。此方法只能由首屏后的阶段或手动重试调用。
    Public Sub MainwindowsShowAsync(ByVal frmMDI1 As MDIForm)
        If frmMDI1 Is Nothing Then Return
        frmMDI = frmMDI1

        'R2-17:必须先读 Initset.set 填充 Initsetting,否则 OcvDataService 连接串为空导致数据库连不上
        Call ReadInitFile()

        StartupDeviceService.StartAsync(frmMDI1,
                                        Sub(result As StartupDeviceCheckResult)
                                            StartupTiming.Mark("device-check-ui-callback")
                                            ApplyCheckResultsToUi(frmMDI1,
                                                                 result.Scm,
                                                                 result.Dmm34461,
                                                                 result.Resistance3561,
                                                                 result.Plc,
                                                                 result.Mes,
                                                                 result)
                                        End Sub)
    End Sub

    '后台回调:在主线程更新连接标志、底部状态控件，并显示一次启动失败汇总。
    Private Sub ApplyCheckResultsToUi(ByVal frmMDI1 As MDIForm,
                                     ByVal reflag_SCM As Short,
                                      ByVal reflag_34461 As Short,
                                      ByVal reflag_3561 As Short,
                                      ByVal reflag_PLC As Short,
                                      ByVal reflag_MES As Short,
                                      ByVal result As StartupDeviceCheckResult)
        If frmMDI1 Is Nothing OrElse frmMDI1.IsDisposed Then Return

        '1、中位机
        controlBoardConnected = (reflag_SCM = 1)
        If reflag_SCM = -1 Then
            blnOnLine = False
        End If

        '3、万用表
        If reflag_34461 = -1 Then
            blnOnLine = False
        End If

        '5、内阻仪
        If reflag_3561 = -1 Then
            blnOnLine = False
        End If

        'PLC
        plcConnected = (reflag_PLC = 1)
        If Not plcConnected Then
            blnOnLine = False
        End If

        '6、MES
        If reflag_MES = -1 Then
            blnOnLine = False
        End If

        '底部状态控件使用统一的最终检查结果。
        frmMDI1.UpdateDeviceCheckStatus(reflag_34461, reflag_3561, reflag_MES)
        frmMDI1.ShowStartupDeviceFailurePrompt(result)
    End Sub

    Public Function CheckPlc() As Short
        plcConnected = False
        Try
            If plcController IsNot Nothing Then plcController.Disconnect()
            plcController = PlcControllerFactory.Create(Initsetting)
            If plcController Is Nothing OrElse Not plcController.Connect() Then Return -1
            plcConnected = True
            Return 1
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("CheckPlc failed: " & ex.Message)
            Return -1
        End Try
    End Function

    Private Function ConnectTCP1(ByVal mSocket As Socket, ByVal ipe As IPEndPoint) As String
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
        'Dim clientSocket As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        'Dim myIP As IPAddress = IPAddress.Parse(Initsetting.SCMIpAdd)
        'Dim myIPPort As IPAddress = IPAddress.Parse(Initsetting.SCM_port)
        'clientSocket.Connect(myIP, myIPPort)

        '' Check if the socket is connected
        'If clientSocket.Connected Then
        '    Console.WriteLine("Connected to the server")
        '    ' Do further operations if the connection is normal
        'Else
        '    Console.WriteLine("Unable to connect to the server")
        'End If

    End Function
    Public Sub ParseBuffer1(ByVal port As Integer, ByVal buffer As FByteArrBuild, ByRef PointPos As Integer)

    End Sub
    ''' <summary>
    ''' 检查 MES API 是否可达
    ''' </summary>
    ''' <returns>1 = 可达，-1 = 不可达</returns>
    Public Function CheckTcp_MES() As Short
        Dim mesUrl As String = If(String.IsNullOrWhiteSpace(Initsetting.MesApiAddress),
                                  "http://127.0.0.1/api/save",
                                  Initsetting.MesApiAddress.Trim())
        Try
            Dim request As HttpWebRequest = CType(WebRequest.Create(mesUrl), HttpWebRequest)
            request.Method = "GET"
            request.Timeout = 3000
            request.ReadWriteTimeout = 3000
            Using response As HttpWebResponse = CType(request.GetResponse(), HttpWebResponse)
                CheckTcp_MES = If(CInt(response.StatusCode) < 400, CType(1, Short), CType(-1, Short))
            End Using
        Catch ex As WebException
            'HTTP 4xx/5xx 说明服务可达,但接口不可用,不能显示为 MES 已连接。
            CheckTcp_MES = -1
        Catch ex As Exception
            CheckTcp_MES = -1
        End Try
        Return CheckTcp_MES
    End Function
    Public Function CheckTcp_SCM() As Short
        '**************************
        '检查单片机TcpIp是否可用
        Dim result As Short

        Tcp_Scm = frmDebug.mSock '单片机
        With frmDebug
            Try
                If .mSock IsNot Nothing Then
                    .mSock.Close()
                End If
            Catch ex As Exception

            End Try


            Dim mClientTCP = New TcpClient()
            Dim sock = mClientTCP.Client
            ''Dim frmInitial As New Form
            ''frmInitial = frmInit
            'Dim scmip As String = Initsetting.SCMIpAdd
            Dim myIP As IPAddress = IPAddress.Parse(Initsetting.SCMIpAdd)
            Dim ipe As IPEndPoint = New IPEndPoint(myIP, Initsetting.SCM_port)
            'Initsetting.SCMIpAdd
            Dim msg As String

            Try
                msg = ConnectTCP1(sock, ipe)
                If msg = "" Then
                    .mSock = New SocketCtrolCmd(sock, AddressOf ParseBuffer1, Initsetting.SCM_port, ipe)
                    ' MsgBox("单片机通信成功！", vbOKOnly)
                    CheckTcp_SCM = 1
                    result = 1
                Else
                    MsgBox("单片机通信失败！", vbOKOnly)
                    'MessageBox.Show(msg)
                    CheckTcp_SCM = -1
                    result = -1
                End If
            Catch ex As Exception
                result = -1
                CheckTcp_SCM = -1
                Return result
            End Try
            '成功路径：返回 CheckTcp_SCM = 1，已经在 If 分支内设置
            Return CheckTcp_SCM
        End With
    End Function
    Public Function CheckCom_SCM(Optional ByVal rethrowOnFailure As Boolean = False) As Short
        controlBoardConnected = False
        Try
            Com_SCM = frmMDI.Com_SCM
            With frmMDI.Com_SCM
                If .IsOpen Then
                    .Close()
                End If
                .PortName = "COM" & (Initsetting.SCMComPort + 1).ToString()
                .BaudRate = If(Initsetting.SCMComRate > 0, Initsetting.SCMComRate, 115200)
                .Parity = Parity.None
                .DataBits = 8
                .StopBits = StopBits.One
                .ReadTimeout = 1000
                .WriteTimeout = 1000
                .Open()
            End With

            '仅打开 COM 口不能证明控制板存在；发送只读状态寄存器请求并校验协议帧/CRC。
            Dim client As New SerialMiddleMachineClient(frmMDI.Com_SCM)
            If Not client.CheckConnection(3000) Then
                Throw New System.IO.IOException("控制板未返回有效协议帧（3秒内无响应或协议校验失败）")
            End If
            controlBoardConnected = True
            Return 1
        Catch ex As Exception
            Dim configuredPort As String = "COM" & (Initsetting.SCMComPort + 1).ToString()
            Dim detail As String = configuredPort & "，" &
                                   If(Initsetting.SCMComRate > 0, Initsetting.SCMComRate, 115200).ToString() &
                                   "，8N1：" & ex.Message
            Try
                If frmMDI IsNot Nothing AndAlso frmMDI.Com_SCM IsNot Nothing AndAlso frmMDI.Com_SCM.IsOpen Then
                    frmMDI.Com_SCM.Close()
                End If
            Catch closeEx As Exception
                System.Diagnostics.Debug.WriteLine("CheckCom_SCM close failed: " & closeEx.Message)
            End Try
            AppLog.Write("Startup", "[DEVICE] SCM failed: " & detail)
            System.Diagnostics.Debug.WriteLine("CheckCom_SCM failed: " & detail)
            If rethrowOnFailure Then Throw New System.IO.IOException(detail, ex)
            Return -1
        End Try
    End Function
    'Public Function CheckCom_SCM() As Short
    '    '**************************
    '    '检查单片机端口是否可用
    '    Com_SCM = frmMDI.Com_SCM '单片机
    '    With frmMDI.Com_SCM
    '        If .IsOpen Then
    '            .Close()
    '        End If
    '        If Initsetting.ComRateMode > 0 Then
    '            .BaudRate = Initsetting.ComRateMode
    '        Else
    '            .BaudRate = 9600
    '        End If
    '        .PortName = "Com" & (Initsetting.SCM_com + 1).ToString
    '        .DataBits = 8
    '        .StopBits = 1
    '        .Parity = Parity.None
    '        .ReadBufferSize = 100
    '        Try
    '            If Not .IsOpen Then
    '                .Open()
    '            End If
    '        Catch e As Exception
    '            MsgBox(e.Message, vbOK)
    '            CheckCom_SCM = -1
    '            Exit Function
    '        End Try
    '    End With
    '    CheckCom_SCM = 1
    '    Return CheckCom_SCM
    'End Function
    Dim socketsent As Socket
    Public Function CheckCom_scaner() As Short
        '**************************
        '检查内阻仪端口是否可用
        Com_scaner = frmMDI.Com_scaner  '日置内阻仪
        With frmMDI.Com_scaner
            If .IsOpen Then
                .Close()
            End If
            .PortName = "Com" & (Initsetting.ScanerPort + 1).ToString
            If Initsetting.ScanerRate > 0 Then
                .BaudRate = Initsetting.ScanerRate
            Else
                .BaudRate = 9600
            End If
            .DataBits = 8
            .StopBits = 1
            .Parity = Parity.None
            'MEMory:DATA? 返回每条记录的序号、内阻、电压；256点时返回数据明显超过100字节。
            '缓冲区过小会在连续返回时丢字节，最终表现为“cached data read failed”。
            .ReadBufferSize = 8192
            .NewLine = vbCrLf
            SerialMiddleMachineClient.LogMessage("3562 serial configured: " & .PortName & ", " & .BaudRate.ToString() & ", 8N1, buffer=" & .ReadBufferSize.ToString())
            Try
                If Not .IsOpen Then
                    .Open()
                End If
            Catch e As Exception
                MsgBox(e.Message, vbOKOnly)
                CheckCom_scaner = -1
                Exit Function
            End Try
        End With
        CheckCom_scaner = 1
    End Function

    '链接com3561
    Public Function CheckCom_3561(Optional ByVal suppressDialogs As Boolean = False) As Short
        '**************************
        '检查内阻仪端口是否可用
        Com_3561 = frmMDI.Com_3561  '日置内阻仪
        Dim count As Integer

        With frmMDI.Com_3561
            If .IsOpen Then
                .Close()
            End If
            .PortName = "Com" & (Initsetting.Recom + 1).ToString
            If Initsetting.ReRate > 0 Then
                .BaudRate = Initsetting.ReRate
            Else
                .BaudRate = 9600
            End If
            .DataBits = 8
            .StopBits = 1
            .Parity = Parity.None
            .ReadBufferSize = 100
            .NewLine = vbCrLf
            count = 0
            Do While count < 3
                Sleep(100)
                Try
                    If Not .IsOpen Then
                        .Open()
                    End If
                    CheckCom_3561 = 1
                    Exit Function
                Catch e As Exception
                    CheckCom_3561 = -1
                    If count = 2 Then
                        If Not suppressDialogs Then MsgBox(e.Message, vbOKOnly)
                        Exit Function
                    End If
                End Try
                count = count + 1
            Loop

        End With
        CheckCom_3561 = 1
    End Function
    Public Sub SaveInitFile()
        '保存初始化文件------------------------
        Dim configPath As String = Application.StartupPath & "\Initset.set"
        Dim tempConfigPath As String = configPath & ".tmp"
        Dim FS As New System.IO.FileStream(tempConfigPath, IO.FileMode.Create, IO.FileAccess.Write, IO.FileShare.None)
        Dim BW As New System.IO.BinaryWriter(FS)
        '通讯参数
        BW.Write(Initsetting.ScanerPort)
        BW.Write(Initsetting.ScanerRate)

        BW.Write(If(Initsetting.SCMIpAdd, String.Empty))
        BW.Write(Initsetting.SCM_port)

        BW.Write(Initsetting.IOPort)
        BW.Write(Initsetting.IORate)

        BW.Write(If(Initsetting.ComRateMode, String.Empty))
        BW.Write(If(Initsetting.IpAddOf34461, String.Empty))

        BW.Write(Initsetting.Recom)
        BW.Write(Initsetting.ReRate)

        'BW.Write(Initsetting.IpAddOf34461)

        '电压内阻上下限
        BW.Write(Initsetting.upResistance)
        BW.Write(Initsetting.downResistance)
        BW.Write(Initsetting.upVoltage)
        BW.Write(Initsetting.downVoltage)
        '数据库
        BW.Write(Initsetting.chkDatabase)
        BW.Write(If(Initsetting.strDatabase, String.Empty))
        '文件路径
        BW.Write(If(Initsetting.sPath, String.Empty))
        '电压B值；内阻B值
        BW.Write(Initsetting.sngV_B)
        BW.Write(Initsetting.sngR_B)
        BW.Write(Initsetting.txtCodeLenth)
        'DataBase UserID and Password
        BW.Write(If(Initsetting.strUserID, String.Empty))
        BW.Write(If(Initsetting.strpwd, String.Empty))
        BW.Write(If(Initsetting.delayTime, String.Empty))
        '探针数据
        BW.Write(Initsetting.Probelifespan)
        BW.Write(Initsetting.ProbeUsedCount)
        '用户管理
        BW.Write(If(Initsetting.PsdOfAdmin, String.Empty))
        BW.Write(If(Initsetting.PsdOfOper, String.Empty))
        BW.Write(If(Initsetting.PsdOfGuest, String.Empty))
        'MES api 地址
        BW.Write(If(Initsetting.MesApiAddress, String.Empty))
        '校准模块阻值
        BW.Write(Initsetting.cailibrateModelValue)
        '超温报警
        BW.Write(Initsetting.overTemperateValue)
        '保留旧智能电表字段占位，确保旧 Initset.set 的二进制布局兼容；运行时不再使用。
        BW.Write(0)
        BW.Write(0)
        '工作站选择
        BW.Write(Initsetting.stationSelect)
        '中位机串口参数
        BW.Write(Initsetting.SCMComPort)
        BW.Write(Initsetting.SCMComRate)
        'PLC 独立通讯参数
        BW.Write(Initsetting.PLCMode)
        BW.Write(If(Initsetting.PLCIP, String.Empty))
        BW.Write(Initsetting.PLCTcpPort)
        BW.Write(Initsetting.PLCComPort)
        BW.Write(Initsetting.PLCComRate)
        'NG复测3562串口参数（追加字段，兼容旧配置文件）
        BW.Write(Initsetting.Retest3562Port)
        BW.Write(Initsetting.Retest3562Rate)
        'SCM控制板时间参数（追加字段，兼容旧配置文件）
        BW.Write(Initsetting.ScmRelaySettleMs)
        BW.Write(Initsetting.ScmChannelSettleMs)
        BW.Write(Initsetting.ScmAutoStepMs)
        BW.Write(Initsetting.ScmTriggerPulseMs)
        BW.Write(Initsetting.ScmPhaseGuardMs)
        '通道状态颜色（追加字段，兼容旧配置）
        BW.Write(Initsetting.ChannelPassColorArgb)
        BW.Write(Initsetting.ChannelTestingColorArgb)
        BW.Write(Initsetting.ChannelIdleColorArgb)
        BW.Write(Initsetting.ChannelResistanceNgColorArgb)
        BW.Write(Initsetting.ChannelVoltageNgColorArgb)
        BW.Write(Initsetting.ChannelSelectedColorArgb)

        'close
        BW.Close()
        FS.Close()
        Try
            If System.IO.File.Exists(configPath) Then
                System.IO.File.Replace(tempConfigPath, configPath, Nothing)
            Else
                System.IO.File.Move(tempConfigPath, configPath)
            End If
        Catch
            Try
                If System.IO.File.Exists(tempConfigPath) Then
                    System.IO.File.Delete(tempConfigPath)
                End If
            Catch
            End Try
            Throw
        End Try
    End Sub
    Public Sub ReadInitFile()
        '读初始化文件---------------
        'If Not System.IO.File.Exists(Application.StartupPath & "\Initset.set") Then
        '    System.IO.File.Create(Application.StartupPath & "\Initset.set")
        'End If
        Dim FS As New System.IO.FileStream(Application.StartupPath & "\Initset.set", IO.FileMode.OpenOrCreate)
        Dim BR As New System.IO.BinaryReader(FS)
        If FS.Length <= 0 Then
            GoTo exit_sub
        End If
        '通讯参数
        Initsetting.ScanerPort = BR.ReadInt32()
        Initsetting.ScanerRate = BR.ReadInt32()

        Initsetting.SCMIpAdd = BR.ReadString
        Initsetting.SCM_port = BR.ReadInt32()

        Initsetting.IOPort = BR.ReadInt32()
        Initsetting.IORate = BR.ReadInt32()

        Initsetting.ComRateMode = BR.ReadString
        Initsetting.IpAddOf34461 = BR.ReadString

        Initsetting.Recom = BR.ReadInt32()
        Initsetting.ReRate = BR.ReadInt32()

        '电压内阻合格范围
        Initsetting.upResistance = BR.ReadSingle
        Initsetting.downResistance = BR.ReadSingle
        Initsetting.upVoltage = BR.ReadSingle
        Initsetting.downVoltage = BR.ReadSingle
        '数据库
        Initsetting.chkDatabase = BR.ReadBoolean
        Initsetting.strDatabase = BR.ReadString
        '文件路径
        Initsetting.sPath = BR.ReadString
        '电压内阻B值
        Initsetting.sngV_B = BR.ReadSingle
        Initsetting.sngR_B = BR.ReadSingle
        '条码长度
        Initsetting.txtCodeLenth = BR.ReadInt32
        'DataBase userID and Password
        Initsetting.strUserID = BR.ReadString
        Initsetting.strpwd = BR.ReadString
        Initsetting.delayTime = BR.ReadString
        '探针使用数据
        Initsetting.Probelifespan = BR.ReadInt32()
        Initsetting.ProbeUsedCount = BR.ReadInt32()
        If Initsetting.ProbeUsedCount < 0 Or Initsetting.ProbeUsedCount > Initsetting.Probelifespan Then
            Initsetting.ProbeUsedCount = 0
        End If
        '用户管理
        Initsetting.PsdOfAdmin = BR.ReadString()
        Initsetting.PsdOfOper = BR.ReadString()
        Initsetting.PsdOfGuest = BR.ReadString()
        'MES api 地址
        Initsetting.MesApiAddress = BR.ReadString()
        '校准模块阻值
        Initsetting.cailibrateModelValue = BR.ReadSingle
        '超温报警
        Initsetting.overTemperateValue = BR.ReadInt32()
        '跳过旧智能电表字段占位；智能电表已从运行代码中移除。
        BR.ReadInt32()
        BR.ReadInt32()
        '工作站选择
        Initsetting.stationSelect = BR.ReadInt32()
        '中位机串口参数，兼容旧 Initset.set
        If FS.Position < FS.Length Then
            Initsetting.SCMComPort = BR.ReadInt32()
        Else
            Initsetting.SCMComPort = 0
        End If
        If FS.Position < FS.Length Then
            Initsetting.SCMComRate = BR.ReadInt32()
        Else
            Initsetting.SCMComRate = 115200
        End If
        If FS.Position < FS.Length Then
            Initsetting.PLCMode = BR.ReadInt32()
        Else
            Initsetting.PLCMode = 0
        End If
        If FS.Position < FS.Length Then
            Initsetting.PLCIP = BR.ReadString()
        Else
            Initsetting.PLCIP = String.Empty
        End If
        If FS.Position < FS.Length Then
            Initsetting.PLCTcpPort = BR.ReadInt32()
        Else
            Initsetting.PLCTcpPort = 502
        End If
        If FS.Position < FS.Length Then
            Initsetting.PLCComPort = BR.ReadInt32()
        Else
            Initsetting.PLCComPort = 0
        End If
        If FS.Position < FS.Length Then
            Initsetting.PLCComRate = BR.ReadInt32()
        Else
            Initsetting.PLCComRate = 115200
        End If
        If FS.Position < FS.Length Then
            Initsetting.Retest3562Port = BR.ReadInt32()
        Else
            Initsetting.Retest3562Port = 1
        End If
        If FS.Position < FS.Length Then
            Initsetting.Retest3562Rate = BR.ReadInt32()
        Else
            Initsetting.Retest3562Rate = 9600
        End If
        If FS.Position < FS.Length Then
            Initsetting.ScmRelaySettleMs = BR.ReadInt32()
        Else
            Initsetting.ScmRelaySettleMs = ScmTimingSettings.DefaultRelaySettleMs
        End If
        If FS.Position < FS.Length Then
            Initsetting.ScmChannelSettleMs = BR.ReadInt32()
        Else
            Initsetting.ScmChannelSettleMs = ScmTimingSettings.DefaultChannelSettleMs
        End If
        If FS.Position < FS.Length Then
            Initsetting.ScmAutoStepMs = BR.ReadInt32()
        Else
            Initsetting.ScmAutoStepMs = ScmTimingSettings.DefaultAutoStepMs
        End If
        If FS.Position < FS.Length Then
            Initsetting.ScmTriggerPulseMs = BR.ReadInt32()
        Else
            Initsetting.ScmTriggerPulseMs = ScmTimingSettings.DefaultTriggerPulseMs
        End If
        If FS.Position < FS.Length Then
            Initsetting.ScmPhaseGuardMs = BR.ReadInt32()
        Else
            Initsetting.ScmPhaseGuardMs = ScmTimingSettings.DefaultPhaseGuardMs
        End If
        If FS.Position < FS.Length Then
            Initsetting.ChannelPassColorArgb = BR.ReadInt32()
        Else
            Initsetting.ChannelPassColorArgb = ChannelColorSettings.DefaultPass.ToArgb()
        End If
        If FS.Position < FS.Length Then
            Initsetting.ChannelTestingColorArgb = BR.ReadInt32()
        Else
            Initsetting.ChannelTestingColorArgb = ChannelColorSettings.DefaultTesting.ToArgb()
        End If
        If FS.Position < FS.Length Then
            Initsetting.ChannelIdleColorArgb = BR.ReadInt32()
        Else
            Initsetting.ChannelIdleColorArgb = ChannelColorSettings.DefaultIdle.ToArgb()
        End If
        If FS.Position < FS.Length Then
            Initsetting.ChannelResistanceNgColorArgb = BR.ReadInt32()
        Else
            Initsetting.ChannelResistanceNgColorArgb = ChannelColorSettings.DefaultResistanceNg.ToArgb()
        End If
        If FS.Position < FS.Length Then
            Initsetting.ChannelVoltageNgColorArgb = BR.ReadInt32()
        Else
            Initsetting.ChannelVoltageNgColorArgb = ChannelColorSettings.DefaultVoltageNg.ToArgb()
        End If
        If FS.Position < FS.Length Then
            Initsetting.ChannelSelectedColorArgb = BR.ReadInt32()
        Else
            Initsetting.ChannelSelectedColorArgb = ChannelColorSettings.DefaultSelected.ToArgb()
        End If
exit_sub:
        BR.Close()
        FS.Close()
    End Sub

    Public Function ConnetTcp_SCM() As Short
        Try
            socketsent = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            Dim ip As IPAddress = IPAddress.Parse(Initsetting.SCMIpAdd)
            Dim piont As IPEndPoint = New IPEndPoint(ip, Convert.ToInt32(Initsetting.SCM_port))
            socketsent.Connect(piont)
            ConnetTcp_SCM = 1
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try

        Try
            Dim th As Thread = New Thread(AddressOf ReceivefromScaner)
            th.IsBackground = True
            th.Start()

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
        ConnetTcp_SCM = 1
    End Function

    'Sub readEmm()
    '    ' 配置串口参数
    '    Dim serialPort As New SerialPort("COM1")
    '    serialPort.BaudRate = 9600
    '    serialPort.Parity = Parity.None
    '    serialPort.DataBits = 8
    '    serialPort.StopBits = StopBits.One
    '    serialPort.Open()

    '    ' 创建Modbus RTU主站
    '    Dim master As IModbusSerialMaster = ModbusSerialMaster.CreateRtu(serialPort)

    '    ' 读取寄存器地址3204的值
    '    Dim startAddress As UShort = &HC83 ' 地址转换为十六进制表示
    '    Dim numRegisters As UShort = 2 ' 64位整数需要读取2个寄存器
    '    Dim slaveId As Byte = 1 ' 设定从站地址为1

    '    Try
    '        Dim registers As UShort() = master.ReadHoldingRegisters(slaveId, startAddress, numRegisters)
    '        ' 合并两个16位寄存器为一个64位整数
    '        Dim totalEnergy As Long = (CLng(registers(0)) << 16) + registers(1)

    '        ' 输出读取的值
    '        Console.WriteLine("Total Energy: " & totalEnergy.ToString() & " Wh")
    '    Catch ex As Exception
    '        Console.WriteLine("读取寄存器时出错: " & ex.Message)
    '    Finally
    '        serialPort.Close()
    '    End Try
    'End Sub
    Private Sub ReceivefromScaner()
        Dim availibleCodecount As Integer = 0

        Try

            While True
                Dim buffer As Byte() = New Byte(5119) {}
                Dim r As Integer = socketsent.Receive(buffer)

                If r = 0 Then
                    Exit While
                End If

                Dim s As String = Encoding.UTF8.GetString(buffer, 0, r)
                'Dim result As Boolean = CodeAnalysis(s)
            End While

        Catch ex As Exception
            ' 跨线程访问 UI:转 UI 线程,避免 InvalidOperationException 与界面无响应 (M25)
            If frmMDI IsNot Nothing AndAlso Not frmMDI.IsDisposed Then
                If frmMDI.InvokeRequired Then
                    frmMDI.Invoke(New Action(Of String)(Sub(msg) MessageBox.Show(msg)), ex.Message)
                Else
                    MessageBox.Show(ex.Message)
                End If
            End If
        End Try
    End Sub
    'Public Sub TestSpeed()
    '    'Screen.MousePointer = 11
    '    'fMainForm.sbStatusBar.Panels(1).Text = ResString(2) '"正在检测微机速度..."
    '    CommunicationCounts = 0
    '    Dim LastSecond, NowSecond As Integer
    '    LastSecond = Second(Now)
    '    Do
    '        NowSecond = Second(Now)
    '        If NowSecond <> LastSecond Then Exit Do
    '    Loop
    '    LastSecond = Second(Now)
    '    Do
    '        CommunicationCounts = CommunicationCounts + 1
    '        NowSecond = Second(Now)
    '        If NowSecond <> LastSecond Then Exit Do
    '    Loop
    '    CommunicationCounts = CLng(CommunicationCounts) / 50
    '    If CommunicationCounts < 5000 Then CommunicationCounts = 5000
    '    'Screen.MousePointer = 1
    'End Sub
    Public Function ConnectForSCM() As Short
        Dim SendBuf(5) As Byte  '发送缓存
        Dim Response As Boolean '响应
        Dim Repeate As Integer
        Dim ReBuffer() As Byte = New Byte() {}  '接收缓存
        Dim Temp_long As Integer
        Dim DataCount As Short  '数据长度
        Dim I As Int16
        Const RepeateCount = 20  '重复次数
        Response = False
        Temp_long = 0
        SendBuf(0) = 170 ' "AA"
        SendBuf(1) = 85  ' "55"
        SendBuf(2) = 1
        SendBuf(3) = 0
        SendBuf(4) = 0
        SendBuf(5) = SendBuf(2) Or SendBuf(3) Or SendBuf(4)
        For I = 0 To 5
            My.Forms.frmDebug.txtSend.Text = My.Forms.frmDebug.txtSend.Text & SendBuf(I) & "   "
        Next
Loop_begin:
        Try
            frmMDI.Com_SCM.Write(SendBuf, 0, 6)
        Catch ex As Exception
            ConnectForSCM = -1
            Exit Function
        End Try
        Sleep(50)
        Do
            DataCount = frmMDI.Com_SCM.BytesToRead
            If DataCount >= 32 Then
                Try
                    ReDim ReBuffer(DataCount - 1)
                    frmMDI.Com_SCM.Read(ReBuffer, 0, DataCount)
                    For I = 0 To 32
                        My.Forms.frmDebug.txtRecieve_SCM.Text = My.Forms.frmDebug.txtRecieve_SCM.Text & ReBuffer(I) & "　"
                    Next
                    Response = True
                    Exit Do
                Catch e As Exception
                    GoTo Loop_No
                End Try
            End If
Loop_No:
            Temp_long = Temp_long + 1
            If Temp_long > 200 Then Exit Do
        Loop
        If Response = True Then
            If ReBuffer(1) = 85 Then
                ConnectForSCM = 0
                Exit Function
            Else
                ConnectForSCM = -1
            End If
        Else
            If Repeate < RepeateCount Then
                Repeate = Repeate + 1
                GoTo Loop_begin
            Else
                ConnectForSCM = -1
            End If
        End If
    End Function

    ''' <summary>
    ''' 读取3561、3562的测试数据。内部触发FETCH
    ''' </summary>
    ''' <param name="Voltage"></param>
    ''' <param name="Resistance"></param>
    ''' <returns></returns>
    Public Function Trans_ReadData_3562(ByRef Voltage, ByRef Resistance) As Integer

        'Dim intCounter As Integer
        Dim RecvStr As String
        Dim strDataArr() As String
        Dim Timer As Single

        Sleep(100)

        RecvStr = ""
        Com_3561.DiscardInBuffer()
        Com_3561.DiscardOutBuffer()
        Com_3561.Write(":FETC?" & vbLf)

        Do While Right(RecvStr, 1) <> Chr(10)
            Timer = Now.TimeOfDay.Hours * 3600 + Now.TimeOfDay.Minutes * 60 + Now.TimeOfDay.Seconds
            RecvStr = RecvStr + Com_3561.ReadExisting
            System.Windows.Forms.Application.DoEvents()
            'System.Windows.Forms.Application.DoEvents()
        Loop

        If RecvStr <> "" Then
            strDataArr = Split(RecvStr, ",")
            Try
                Resistance = CSng(CDbl(strDataArr(0))) * 1000
                Voltage = CSng(CDbl(strDataArr(1)))
            Catch er As Exception
            End Try
            FirstCom = True
            Trans_ReadData_3562 = 0
        Else
            Trans_ReadData_3562 = -1
        End If
    End Function

    Public Function Trans_ReadCachedData_3562(expectedCount As Integer,
                                             ByRef voltages As List(Of Single),
                                             ByRef resistances As List(Of Single)) As Integer
        Dim recvStr As String = ""
        voltages = New List(Of Single)()
        resistances = New List(Of Single)()

        If expectedCount <= 0 Then
            SerialMiddleMachineClient.LogMessage("3562 cache read skipped: expectedCount <= 0")
            Return -1
        End If

        Try
            SerialMiddleMachineClient.LogMessage("3562 cache read start, expectedCount=" & expectedCount.ToString())
            Com_3561.DiscardInBuffer()
            Com_3561.DiscardOutBuffer()
            Com_3561.WriteTimeout = 3000
            Com_3561.Write("MEMory:DATA?" & vbCrLf)

            Dim startTime As DateTime = DateTime.Now
            Do
                Dim chunk As String = Com_3561.ReadExisting()
                If chunk <> "" Then
                    recvStr &= chunk
                    startTime = DateTime.Now
                End If
                System.Windows.Forms.Application.DoEvents()
                'MEMory:DATA? 返回的是三字段记录（序号、内阻、电压）。不能按“2个数字/点”提前退出，
                '否则收到前两字段后就会在一条记录中间停止，导致解析器误判缓存不足。
            Loop While Not recvStr.ToUpperInvariant().Contains("END") AndAlso
                       DateTime.Now.Subtract(startTime).TotalSeconds < 10

            Dim rawPreview As String = recvStr.Replace(vbCr, "<CR>").Replace(vbLf, "<LF>")
            If rawPreview.Length > 600 Then rawPreview = rawPreview.Substring(0, 600)
            SerialMiddleMachineClient.LogMessage("3562 cache raw preview=" & rawPreview)

            Dim parseResult As InstrumentCacheParseResult = InstrumentCacheParser.Parse3562MemoryData(recvStr, expectedCount)
            voltages = parseResult.Voltages
            resistances = parseResult.Resistances
            SerialMiddleMachineClient.LogMessage("3562 cache parse summary: expected=" & expectedCount.ToString() &
                                                 ", valid=" & parseResult.ValidRecordCount.ToString() &
                                                 ", invalid=" & parseResult.InvalidRecordCount.ToString())
            If Not parseResult.Success Then
                If parseResult.InvalidRecordCount > 0 Then
                    SerialMiddleMachineClient.LogMessage("3562 cache read failed: invalid record format detected.")
                ElseIf parseResult.ValidRecordCount = 0 Then
                    SerialMiddleMachineClient.LogMessage("3562 cache read failed: cache is empty or contains no valid records.")
                Else
                    SerialMiddleMachineClient.LogMessage("3562 cache read failed: record count mismatch.")
                End If
                Return -1
            End If

            SerialMiddleMachineClient.LogMessage("3562 cache read rawLength=" & recvStr.Length.ToString() & ", recordCount=" & resistances.Count.ToString() & ", format=index,resistanceOhm,voltageVolt")
            SerialMiddleMachineClient.LogMessage("3562 cache read success, resultCount=" & resistances.Count.ToString())
            Return 0
        Catch ex As Exception
            SerialMiddleMachineClient.LogMessage("3562 cache read exception: " & ex.Message)
            Return -1
        End Try
    End Function

    Public Function PrepareCachedData_3562(expectedCount As Integer) As Integer
        If expectedCount <= 0 Then
            SerialMiddleMachineClient.LogMessage("3562 prepare skipped: expectedCount <= 0")
            Return -1
        End If

        Try
            SerialMiddleMachineClient.LogMessage("3562 prepare cache, expectedCount=" & expectedCount.ToString())
            Com_3561.DiscardInBuffer()
            Com_3561.DiscardOutBuffer()
            Com_3561.WriteTimeout = 3000
            '每次正式测试都重新锁定到 300 mΩ，避免仪表面板或上一次测试留下自动量程/Ω量程。
            '改变量程会清空仪表内存，因此必须先设置量程，再执行 MEMory:CLEA。
            Com_3561.Write(":AUT OFF" & vbLf)
            Com_3561.Write(":RES:RANG 300E-3" & vbLf)
            SerialMiddleMachineClient.LogMessage("3562 prepare range: 300 mOhm (auto range off)")
            Com_3561.Write("TRIG:SOUR EXT" & vbCrLf)
            Com_3561.Write("INIT:CONT ON" & vbLf)
            Com_3561.Write("MEMory:CLEA" & vbLf)
            Com_3561.Write(":MEMory:STAT ON" & vbLf)
            Return 0
        Catch ex As Exception
            SerialMiddleMachineClient.LogMessage("3562 prepare exception: " & ex.Message)
            Return -1
        End Try
    End Function

    Public Sub ClearCachedData_3562()
        Try
            Com_3561.DiscardInBuffer()
            Com_3561.DiscardOutBuffer()
            Com_3561.Write("MEMory:CLEA" & vbLf)
        Catch ex As Exception
            SerialMiddleMachineClient.LogMessage("3562 clear cache exception: " & ex.Message)
        End Try
    End Sub

    Public Sub ResetCachedData_3562(Optional timeoutMs As Integer = 3000)
        Try
            If Com_3561 Is Nothing OrElse Not Com_3561.IsOpen Then
                SerialMiddleMachineClient.LogMessage("3562 reset skipped: port is not open")
                Return
            End If
            SerialMiddleMachineClient.LogMessage("3562 reset start")
            Com_3561.DiscardInBuffer()
            Com_3561.DiscardOutBuffer()
            Com_3561.WriteTimeout = Math.Max(100, Math.Min(timeoutMs, 3000))
            Com_3561.Write("ABOR" & vbLf)
            Com_3561.Write("INIT:CONT OFF" & vbLf)
            Com_3561.Write("MEMory:CLEA" & vbLf)
            Com_3561.Write(":SYST:LOC" & vbLf)
            Com_3561.DiscardInBuffer()
            Com_3561.DiscardOutBuffer()
            SerialMiddleMachineClient.LogMessage("3562 reset completed")
        Catch ex As Exception
            SerialMiddleMachineClient.LogMessage("3562 reset exception: " & ex.Message)
        End Try
    End Sub

    Private Function ExtractNumericValues(text As String) As List(Of Double)
        Dim values As New List(Of Double)()
        If text Is Nothing Then
            Return values
        End If

        Dim normalized As String = text.Replace(vbCr, ",").Replace(vbLf, ",").Replace(";", ",").Replace("END", ",").Replace("end", ",")
        Dim tokens As String() = normalized.Split(","c)
        For Each token As String In tokens
            Dim value As Double
            If Double.TryParse(token.Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, value) Then
                values.Add(value)
            End If
        Next

        Return values
    End Function

    Public Function PrepareCachedData_34461(expectedCount As Integer) As Integer
        If expectedCount <= 0 Then
            SerialMiddleMachineClient.LogMessage("34461 prepare skipped: expectedCount <= 0")
            Return -1
        End If

        For attempt As Integer = 1 To 2
            Try
                'ReFlag 只能表示上一次状态；实际 I/O 失败时必须关闭旧会话并重新连接。
                If ReFlag_34461 <> 0 OrElse visaSession Is Nothing Then
                    ReFlag_34461 = ConnectFor34461()
                End If
                If ReFlag_34461 <> 0 OrElse visaSession Is Nothing Then
                    Throw New InvalidOperationException("34461 VISA session is not connected.")
                End If

                SerialMiddleMachineClient.LogMessage("34461 prepare cache, expectedCount=" & expectedCount.ToString() & ", attempt=" & attempt.ToString())
                SerialMiddleMachineClient.LogMessage("34461 SCPI prepare: IO.Clear; ABOR; *CLS; CONF:VOLT:DC; SAMP:COUN 1; TRIG:COUN " & expectedCount.ToString() & "; TRIG:SOUR EXT; INIT")
                visaSession.IO.Clear()
                '采用 INIT + FETC? 的分离式采集，避免 READ? 在触发未完成时长时间阻塞。
                visaSession.IO.Timeout = 10000
                visaSession.WriteString("ABOR")
                visaSession.WriteString("*CLS")
                visaSession.WriteString("CONF:VOLT:DC")
                '1 PLC降低单次积分时间，使34461更快准备好接受下一次外部触发。
                visaSession.WriteString("VOLT:DC:NPLC 1")
                '每个中位机外部触发对应一个电压采样，全部读数写入 34461 reading memory。
                visaSession.WriteString("SAMP:COUN 1")
                visaSession.WriteString("TRIG:COUN " & expectedCount.ToString())
                visaSession.WriteString("TRIG:SOUR EXT")
                '不强制设置 TRIG:SLOP：34461A及原有硬件链路使用默认低有效外部触发。
                visaSession.WriteString("INIT")

                'INIT 后立即读取错误队列，避免把仪表拒绝的设置误判为已进入采集状态。
                visaSession.IO.Timeout = 1000
                visaSession.WriteString("SYST:ERR?")
                Dim instrumentError As String = visaSession.ReadString()
                SerialMiddleMachineClient.LogMessage("34461 prepare SYST:ERR?=" & instrumentError)
                If Not instrumentError.TrimStart().StartsWith("0") AndAlso Not instrumentError.TrimStart().StartsWith("+0") Then
                    Throw New InvalidOperationException("34461 SCPI error: " & instrumentError.Trim())
                End If
                visaSession.IO.Timeout = 10000
                SerialMiddleMachineClient.LogMessage("34461 acquisition initialized; waiting for external triggers.")
                Return 0
            Catch ex As Exception
                SerialMiddleMachineClient.LogMessage("34461 prepare exception (attempt " & attempt.ToString() & "): " & ex.Message)
                ReFlag_34461 = -1
                Close34461Session()
                If attempt = 2 Then Return -1
            End Try
        Next
        Return -1
    End Function

    Public Function Trans_ReadCachedData_34461(expectedCount As Integer,
                                               ByRef voltages As List(Of Single)) As Integer
        voltages = New List(Of Single)()
        If expectedCount <= 0 Then
            SerialMiddleMachineClient.LogMessage("34461 cache read skipped: expectedCount <= 0")
            Return -1
        End If

        Try
            If ReFlag_34461 <> 0 OrElse visaSession Is Nothing Then
                SerialMiddleMachineClient.LogMessage("34461 cache read failed: not connected.")
                Return -1
            End If

            SerialMiddleMachineClient.LogMessage("34461 cache fetch start, expectedCount=" & expectedCount.ToString())
            '中位机完成事件表示所有外部触发已经发出；FETC? 会等待仪表完成并一次性转移 reading memory。
            visaSession.IO.Timeout = 3000
            visaSession.WriteString("DATA:POINTS?")
            Dim pointsText As String = visaSession.ReadString()
            Dim actualPointCount As Integer
            If Not Integer.TryParse(pointsText.Trim(), actualPointCount) Then
                SerialMiddleMachineClient.LogMessage("34461 cache point query invalid: " & pointsText.Trim())
                Return -1
            End If
            SerialMiddleMachineClient.LogMessage("34461 cache points actual=" & actualPointCount.ToString() & ", expected=" & expectedCount.ToString())
            If actualPointCount <> expectedCount Then
                SerialMiddleMachineClient.LogMessage("34461 cache incomplete; FETC? skipped to avoid blocking read.")
                Return -1
            End If
            visaSession.IO.Timeout = Math.Max(10000, expectedCount * 1000)
            visaSession.WriteString("FETC?")
            'ReadList 对不同 VISA 终止符格式的兼容性较差；先读取原始 ASCII 响应，再统一解析数值。
            Dim rawResponse As String = visaSession.ReadString()
            Dim values As List(Of Double) = ExtractNumericValues(rawResponse)
            SerialMiddleMachineClient.LogMessage("34461 cache read rawLength=" & If(rawResponse, String.Empty).Length.ToString() & ", numericCount=" & values.Count.ToString())
            If values.Count < expectedCount Then
                SerialMiddleMachineClient.LogMessage("34461 cache read failed: actualCount=" & values.Count.ToString())
                Return -1
            End If

            For i As Integer = 0 To expectedCount - 1
                voltages.Add(CSng(values(i)))
            Next

            SerialMiddleMachineClient.LogMessage("34461 cache read success, resultCount=" & voltages.Count.ToString())
            Return 0
        Catch ex As Exception
            Dim instrumentError As String = "未能读取仪表错误队列"
            Try
                visaSession.IO.Timeout = 1000
                visaSession.WriteString("SYST:ERR?")
                instrumentError = visaSession.ReadString()
            Catch errorQueueException As Exception
                instrumentError = "错误队列读取失败：" & errorQueueException.Message
            End Try
            SerialMiddleMachineClient.LogMessage("34461 cache read exception: " & ex.Message & "; SYST:ERR?=" & instrumentError)
            Return -1
        End Try
    End Function

    Public Sub ClearCachedData_34461()
        Try
            If visaSession IsNot Nothing Then
                visaSession.WriteString("ABOR")
                visaSession.IO.Clear()
            End If
        Catch ex As Exception
            SerialMiddleMachineClient.LogMessage("34461 clear cache exception: " & ex.Message)
        End Try
    End Sub

    Public Sub ResetCachedData_34461(Optional timeoutMs As Integer = VisaInstrumentIoTimeoutMs)
        Try
            If visaSession Is Nothing Then
                SerialMiddleMachineClient.LogMessage("34461 reset skipped: session is not open")
                Return
            End If
            SerialMiddleMachineClient.LogMessage("34461 reset start")
            '退出清理不能继承 FETC? 的长超时；否则仪表无响应时会阻塞整个 FormClosing。
            visaSession.IO.Timeout = VisaShutdownTimeoutMs
            If timeoutMs > 0 AndAlso timeoutMs < VisaShutdownTimeoutMs Then
                visaSession.IO.Timeout = Math.Max(100, timeoutMs)
            End If
            Try
                '先清除 VISA 挂起的读写/触发状态，再发送 SCPI 中止命令。
                '仪表处于外部触发等待时，直接 WriteString("ABOR") 可能被 VISA 阻塞。
                visaSession.IO.Clear()
            Catch ex As Exception
                SerialMiddleMachineClient.LogMessage("34461 pre-clear failed: " & ex.Message)
            End Try
            Try
                visaSession.WriteString("ABOR", True)
            Catch ex As Exception
                SerialMiddleMachineClient.LogMessage("34461 abort failed: " & ex.Message)
            End Try
            Try
                visaSession.WriteString("*CLS", True)
            Catch ex As Exception
                SerialMiddleMachineClient.LogMessage("34461 clear status failed: " & ex.Message)
            End Try
            SerialMiddleMachineClient.LogMessage("34461 reset completed")
        Catch ex As Exception
            SerialMiddleMachineClient.LogMessage("34461 reset exception: " & ex.Message)
        End Try
    End Sub

    Public Function Reset34461ForNewTest() As Integer
        Try
            If ReFlag_34461 <> 0 OrElse visaSession Is Nothing Then
                ReFlag_34461 = ConnectFor34461()
            End If
            If ReFlag_34461 <> 0 OrElse visaSession Is Nothing Then
                SerialMiddleMachineClient.LogMessage("34461 pre-test reset failed: not connected")
                Return -1
            End If
            SerialMiddleMachineClient.LogMessage("34461 pre-test reset start: IO.Clear; *RST; *CLS")
            visaSession.IO.Clear()
            visaSession.IO.Timeout = 10000
            visaSession.WriteString("*RST")
            visaSession.WriteString("*CLS")
            SerialMiddleMachineClient.LogMessage("34461 pre-test reset completed")
            Return 0
        Catch ex As Exception
            SerialMiddleMachineClient.LogMessage("34461 pre-test reset exception: " & ex.Message)
            ReFlag_34461 = -1
            Close34461Session()
            Return -1
        End Try
    End Function

    Public Sub Close34461Session(Optional sendAbort As Boolean = True)
        Try
            If visaSession IsNot Nothing Then
                Try
                    visaSession.IO.Timeout = VisaShutdownTimeoutMs
                Catch
                End Try
                If sendAbort Then
                    Try
                        visaSession.WriteString("ABOR")
                    Catch
                    End Try
                End If
                Try
                    visaSession.IO.Clear()
                Catch
                End Try
                Try
                    visaSession.IO.Close()
                Catch
                End Try
                Try
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(visaSession)
                Catch
                End Try
                visaSession = Nothing
            End If
        Catch ex As Exception
            SerialMiddleMachineClient.LogMessage("34461 close exception: " & ex.Message)
        Finally
            ReFlag_34461 = -1
        End Try
    End Sub

    Public Function Trans_ReadData_3561(ByRef Voltage, ByRef Resistance) As Integer
        Dim intCounter As Integer
        Dim RecvStr As String
        Dim strDataArr() As String
        Dim Timer As Single
        Static TempUpperR As Single
        Static TempDownerR As Single
        Static TempUpperV As Single
        Static TempDownerV As Single
        Static blnInitRun As Boolean
        Dim Start
        System.Windows.Forms.Application.DoEvents()
        If Not blnInitRun Then
            RecvStr = ""
            Com_3561.DiscardInBuffer()
            Com_3561.DiscardOutBuffer()
            Com_3561.Write(":CALC:LIM:STAT ON" & vbLf)
            Start = Timer
            Do While Timer < Start + 0.1
                Timer = Now.TimeOfDay.Hours * 3600 + Now.TimeOfDay.Minutes * 60 + Now.Second
                RecvStr = RecvStr + Com_3561.ReadExisting
                System.Windows.Forms.Application.DoEvents()
            Loop
            intCounter = 2
            Do
                RecvStr = ""
                Com_3561.DiscardInBuffer()
                Com_3561.DiscardOutBuffer()
                '            MyComm.Output = "COMP:VHIGH?" & vbLf
                Com_3561.Write(":CALC:LIM:VOLT:UPP?" & vbLf)
                Start = Timer
                Do While Timer < Start + 0.01
                    Timer = Now.TimeOfDay.Hours * 3600 + Now.TimeOfDay.Minutes * 60 + Now.Second
                    RecvStr = RecvStr + Com_3561.ReadExisting
                    System.Windows.Forms.Application.DoEvents()
                Loop
                intCounter = intCounter - 1
            Loop Until (intCounter = 0)
            If IsNumeric(RecvStr) Then
                TempUpperV = CSng(CDbl(RecvStr))
            Else
                TempUpperV = 0
            End If
            intCounter = 3
            Do
                RecvStr = ""
                Com_3561.DiscardInBuffer()
                Com_3561.DiscardOutBuffer()
                '            MyComm.Output = "COMP:VLOW?" & vbLf
                Com_3561.Write(":CALC:LIM:VOLT:LOW?" & vbLf)
                Start = Timer
                Do While Timer < Start + 0.1
                    Timer = Now.TimeOfDay.Hours * 3600 + Now.TimeOfDay.Minutes * 60 + Now.Second
                    RecvStr = RecvStr + Com_3561.ReadExisting
                    System.Windows.Forms.Application.DoEvents()
                Loop

                intCounter = intCounter - 1
            Loop Until (intCounter = 0)
            If IsNumeric(RecvStr) Then
                TempDownerV = CSng(CDbl(RecvStr))
            Else
                TempDownerV = 0
            End If
            intCounter = 3
            Do
                RecvStr = ""
                Com_3561.DiscardInBuffer()
                Com_3561.DiscardOutBuffer()
                '            MyComm.Output = "COMP:RHIGH?" & vbLf
                Com_3561.Write(":CALC:LIM:RES:UPP?" & vbLf)
                Start = Timer
                Do While Timer < Start + 0.1
                    Timer = Now.TimeOfDay.Hours * 3600 + Now.TimeOfDay.Minutes * 60 + Now.Second
                    RecvStr = RecvStr + Com_3561.ReadExisting
                    System.Windows.Forms.Application.DoEvents()
                Loop
                intCounter = intCounter - 1
            Loop Until (intCounter = 0)
            If IsNumeric(RecvStr) Then
                TempUpperR = CSng(CDbl(RecvStr))
            Else
                TempUpperR = 0
            End If
            intCounter = 3
            Do
                RecvStr = ""
                Com_3561.DiscardInBuffer()
                Com_3561.DiscardOutBuffer()
                '            MyComm.Output = "COMP:RLOW?" & vbLf
                Com_3561.Write(":CALC:LIM:RES:LOW?" & vbLf)
                Start = Timer
                Do While Timer < Start + 0.1
                    Timer = Now.TimeOfDay.Hours * 3600 + Now.TimeOfDay.Minutes * 60 + Now.Second
                    RecvStr = RecvStr + Com_3561.ReadExisting
                    System.Windows.Forms.Application.DoEvents()
                Loop
                intCounter = intCounter - 1
            Loop Until (intCounter = 0)
            If IsNumeric(RecvStr) Then
                TempDownerR = CSng(CDbl(RecvStr))
            Else
                TempDownerR = 0
            End If
            intCounter = 2
            Do
                RecvStr = ""
                Com_3561.DiscardInBuffer()
                Com_3561.DiscardOutBuffer()
                Com_3561.Write(":CALC:LIM:STAT OFF" & vbLf)
                Start = Timer
                Do While Timer < Start + 0.1
                    Timer = Now.TimeOfDay.Hours * 3600 + Now.TimeOfDay.Minutes * 60 + Now.Second
                    RecvStr = RecvStr + Com_3561.ReadExisting
                    System.Windows.Forms.Application.DoEvents()
                Loop
                intCounter = intCounter - 1
            Loop Until (intCounter = 0)
            intCounter = 2
            Do
                RecvStr = ""
                Com_3561.DiscardInBuffer()
                Com_3561.DiscardOutBuffer()
                Com_3561.Write(":SYST:LOC" & vbLf)
                Start = Timer
                Do While Timer < Start + 0.1
                    Timer = Now.TimeOfDay.Hours * 3600 + Now.TimeOfDay.Minutes * 60 + Now.TimeOfDay.Seconds
                    RecvStr = RecvStr + Com_3561.ReadExisting
                    System.Windows.Forms.Application.DoEvents()
                Loop

                intCounter = intCounter - 1
            Loop Until (intCounter = 0)
            RecvStr = ""
            Com_3561.DiscardInBuffer()
            Com_3561.DiscardOutBuffer()
            Com_3561.Write(":AUT ON" & vbLf)  '运行这个命令出错
            Start = Timer
            Do While Timer < Start + 0.1
                Timer = Now.TimeOfDay.Hours * 3600 + Now.TimeOfDay.Minutes * 60 + Now.TimeOfDay.Seconds
                RecvStr = RecvStr + Com_3561.ReadExisting
                System.Windows.Forms.Application.DoEvents()
            Loop
            blnInitRun = True
        End If
        intCounter = 2

        Do
            RecvStr = ""
            '清空串口缓存
            Com_3561.DiscardInBuffer()
            Com_3561.DiscardOutBuffer()
            Com_3561.WriteTimeout = 3000
            '发送命令，获取内阻仪存储区的数据
            Com_3561.Write("MEMory:DATA?" & vbCrLf)
            '读取内阻仪串口缓存区的数据
            RecvStr = RecvStr + Com_3561.ReadExisting()
            System.Windows.Forms.Application.DoEvents()



            Timer = Now.TimeOfDay.Hours * 3600 + Now.TimeOfDay.Minutes * 60 + Now.Second
            Start = Timer

            Dim TmpRec As String = ""
            Do While Left(Right(Trim(RecvStr), 5), 3) <> "END" And Timer < Start + 10
                Timer = Now.TimeOfDay.Hours * 3600 + Now.TimeOfDay.Minutes * 60 + Now.Second
                TmpRec = Com_3561.ReadExisting()
                If TmpRec <> "" Then
                    RecvStr = RecvStr + TmpRec
                    Start = Timer
                Else

                End If
                If Len(RecvStr) > 100 Then
                    'mStr.Add(RecvStr)
                    RecvStr = ""
                End If
            Loop

            Com_3561.Write("TRIG:SOUR IMM" & vbLf)
            Com_3561.Write(":FETC?" & vbLf)
            Start = Timer
            'Do While Timer < Start + 0.5
            Do While Right(RecvStr, 1) <> Chr(10)
                Timer = Now.TimeOfDay.Hours * 3600 + Now.TimeOfDay.Minutes * 60 + Now.TimeOfDay.Seconds
                RecvStr = RecvStr + Com_3561.ReadExisting
                System.Windows.Forms.Application.DoEvents()
            Loop
            intCounter = intCounter - 1
        Loop Until (intCounter = 0)
        If RecvStr <> "" Then
            strDataArr = Split(RecvStr, ",")
            Try
                Resistance = CSng(CDbl(strDataArr(0))) * 1000
                Voltage = CSng(CDbl(strDataArr(1)))
            Catch er As Exception
            End Try
            If FirstCom = False Then
                If TempUpperV <> Initsetting.upVoltage Or
                   TempDownerV <> Initsetting.downVoltage Or
                   TempUpperR <> Initsetting.upResistance Or
                   TempDownerR <> Initsetting.downResistance Then
                    Initsetting.upVoltage = TempUpperV
                    Initsetting.downVoltage = TempDownerV
                    Initsetting.upResistance = TempUpperR
                    Initsetting.downVoltage = TempDownerR
                    'intRtn = MsgBox("仪表和电脑设置的上下报警不一样！是否将电脑的上下限报警值设为与仪表一样？", vbExclamation + vbYesNo, "")
                    'Select Case intRtn
                    '    Case vbYes
                    '        Initsetting.upVoltage = TempUpperV
                    '        Initsetting.downVoltage = TempDownerV
                    '        Initsetting.upResistance = TempUpperR
                    '        Initsetting.downVoltage = TempDownerR
                    '    Case vbNo
                    '        MsgBox("数据文件将按电脑设置的上下限报警保存测试数据！", vbExclamation)
                    'End Select
                End If
            End If
            FirstCom = True
            Trans_ReadData_3561 = 0
        Else
            Trans_ReadData_3561 = -1
        End If
    End Function
    Public Sub paintBat(ByVal Pic_bat As PictureBox)
        Dim G As Graphics
        Dim obj_bat As Bat_struct
        Dim intRow As Int16 = 16
        Dim intCol As Int16 = 16
        Dim I, J As Int16
        Dim P1 As Point
        Dim P2 As Point
        Dim obj_pen As System.Drawing.Pen
        Dim Box As Rectangle
        G = Pic_bat.CreateGraphics
        G.Clear(Color.LightBlue)
        obj_bat.Left = Pic_bat.Left
        obj_bat.Top = Pic_bat.Top
        obj_bat.Hight = Pic_bat.Size.Height / intRow
        obj_bat.width = Pic_bat.Size.Width / intCol
        For I = 0 To intRow - 1
            For J = 0 To intCol - 1
                '电池的头部
                obj_pen = New Pen(Brushes.Gray, obj_bat.width / 4)
                P1 = New Point(obj_bat.width * J + obj_bat.width / 2.2, obj_bat.Hight * I + 4)
                P2 = New Point(obj_bat.width * J + obj_bat.width / 2.2, obj_bat.Hight * I + obj_bat.Hight / 10)
                G.DrawLine(obj_pen, P1, P2)
                '电池
                P1 = New Point(obj_bat.width * J, obj_bat.Hight * I + obj_bat.Hight / 10)
                P2 = New Point(obj_bat.width * J, obj_bat.Hight * I + obj_bat.Hight)
                obj_pen = New Pen(Brushes.Gray, obj_bat.width * 1.7)
                G.DrawLine(obj_pen, P1, P2)
                Box = New Rectangle(P1, New Size(obj_bat.width, obj_bat.Hight))
                G.DrawRectangle(Pens.White, Box)
            Next J
        Next I
    End Sub
    ''' <summary>
    ''' 保存条码到文件\bar.set
    ''' </summary>
    Public Sub SaveBarcode()
        Dim I As Short
        'barcode 存到临时文件---------------------------------------------------------------------------
        Dim FS As New System.IO.FileStream(Application.StartupPath & "\bar.set", IO.FileMode.Create)
        Dim BW As New System.IO.BinaryWriter(FS)
        For I = 0 To UBound(RecieveData)
            If Not IsNothing(RecieveData(I).BatCode) Then
                BW.Write(RecieveData(I).BatCode)
            End If
        Next
        BW.Close()
        FS.Close()
    End Sub
    Public Function GetBarCodeNum(ByRef tempCode As String, ByRef strFix As String) As Double
        Dim temp_string As String
        Dim tap_0 As Short
        Dim tap_1 As Short
        Dim temp_str As String
        Dim start_num As Long
        Dim strlen As Short
        Dim FixStr As String

        temp_string = tempCode
        tap_0 = -1
        tap_1 = -1
        For i = Len(temp_string) To 1 Step -1
            temp_str = Mid(temp_string, i, 1)
            If Asc(temp_str) >= Asc("0") And Asc(temp_str) <= Asc("9") Then
                If tap_0 = -1 Then
                    tap_0 = i
                Else
                    tap_1 = i
                End If
            Else
                Exit For
            End If
        Next i
        If tap_0 = -1 Then
            start_num = 1
            FixStr = temp_string
            strlen = 1
        Else
            If tap_1 = -1 Then tap_1 = tap_0
            temp_str = Mid(temp_string, tap_1, tap_0 - tap_1 + 1)
            start_num = CLng(temp_str)
            FixStr = Mid(temp_string, 1, tap_1 - 1)
            strlen = tap_0 - tap_1 + 1
        End If
        strFix = FixStr
        GetBarCodeNum = start_num
    End Function
    ''' <summary>
    ''' 读取条码文件
    ''' </summary>
    Public Sub GetBarCodeFile()
        Dim I As Short
        Dim FS As New System.IO.FileStream(Application.StartupPath & "\bar.set", IO.FileMode.OpenOrCreate)
        Dim BR As New System.IO.BinaryReader(FS)
        If BR.BaseStream.Length <= 0 Then Exit Sub
        For I = 0 To UBound(RecieveData)
            RecieveData(I).BatCode = BR.ReadString
            If BR.BaseStream.Position >= BR.BaseStream.Length Then Exit For
        Next
        BR.Close()
        FS.Close()
    End Sub
    Public Sub GetExpression(ByVal strLvName As String)
        Dim Temp_str As String = ""
        Dim TempValue As String = ""

        '等级名称保存为g_objLv的key,等级的表达式保存为相应的value
        g_objLv = New Dictionary(Of String, String)
        If strLvName = "" Then
            GoTo exit_sub
        End If

        g_objLv.Add("LvName", strLvName)
        Try
            Using service As New OcvDataService()
                Dim table As DataTable = service.GetGradeRules(strLvName)
                For Each row As DataRow In table.Rows
                    Temp_str = Convert.ToString(row("GradeCode"))
                    Dim fieldName As String = Convert.ToString(row("FieldName"))
                    Dim suffix As String = Convert.ToString(row("LegacyColumnSuffix"))
                    If String.IsNullOrEmpty(suffix) Then suffix = "A"
                    Dim expressionFieldName As String = fieldName
                    Dim exprPart As String = ""
                    If row("LowerLimit") IsNot DBNull.Value Then
                        exprPart = expressionFieldName & ">=" & Convert.ToString(row("LowerLimit"))
                    End If
                    If row("UpperLimit") IsNot DBNull.Value Then
                        If exprPart <> "" Then exprPart = exprPart & " and "
                        exprPart = exprPart & expressionFieldName & "<" & Convert.ToString(row("UpperLimit"))
                    End If
                    If exprPart = "" Then Continue For
                    If g_objLv.TryGetValue(Temp_str, TempValue) = True Then
                        TempValue = TempValue & " and " & exprPart
                        g_objLv.Item(Temp_str) = TempValue
                    Else
                        g_objLv.Add(Temp_str, exprPart)
                    End If
                Next
            End Using
        Catch ex As Exception
        End Try
exit_sub:

    End Sub

    Public Sub GetCN()
        Dim emptyResult As New Dictionary(Of String, String)()
        Try
            '连接和查询各自有超时设置；这里再给整个操作一个统一的 3 秒预算，
            '避免两段超时叠加后拖住后台初始化线程。
            Dim loadTask As System.Threading.Tasks.Task(Of Dictionary(Of String, String)) =
                System.Threading.Tasks.Task.Run(Function()
                                                    Dim result As New Dictionary(Of String, String)()
                                                    Using service As New OcvDataService()
                                                        Dim table As DataTable = service.GetGradeSelectableFields()
                                                        For Each row As DataRow In table.Rows
                                                            result(Convert.ToString(row("FieldName"))) = Convert.ToString(row("DisplayName"))
                                                        Next
                                                    End Using
                                                    Return result
                                                End Function)
            If loadTask.Wait(3000) AndAlso loadTask.Status = System.Threading.Tasks.TaskStatus.RanToCompletion Then
                g_objCN = loadTask.Result
            Else
                g_objCN = emptyResult
            End If
        Catch ex As Exception
            'OCV_DATA 不可用时,g_objCN 为空字典
            g_objCN = emptyResult
        End Try
    End Sub
    Function ScanBarCode() As String

        'Dim i As Integer
        Dim sendData(2) As Byte
        Dim recvData(0) As Byte
        Dim ReDataFlag As Integer
        Dim str As String

        sendData(0) = &H1 'ASCII<SOH>
        sendData(1) = &H54
        sendData(2) = &H4 'ASCII<EOT>

        ReDataFlag = CommunicationScan(sendData, recvData)   'CommunicationScan(strData, Temp_Buf, 3, 1, 0)

        If ReDataFlag = 1 Then
            str = tranStrByBuff(recvData)
            If str = "NG" Then
                str = ""
            End If
            ScanBarCode = str
        Else
            ScanBarCode = ""
        End If

    End Function

    Private Function CommunicationScan(ByVal sendData As Byte(), ByRef recvData As Byte()) As Integer

        Dim Response As Boolean
        Dim LastTimer As Date
        Dim TempCount As Integer
        Dim rec() As Byte
        Dim len As Integer
        Dim emptyCnt As Integer


        If Com_scaner.IsOpen = False Then
            Return -1
        End If

        Com_scaner.DiscardInBuffer()

        TempCount = 0
        Response = False
        Com_scaner.Write(sendData, 0, sendData.Length) ' Last_Buf  '+ SumChk(strData)   '发送数据
        LastTimer = Now

        len = 0
        emptyCnt = 0
        Do
            Threading.Thread.Sleep(10)
            If Com_scaner.BytesToRead > 0 Then

                ReDim Preserve rec(Com_scaner.BytesToRead - 1 + len)

                Com_scaner.Read(rec, len, Com_scaner.BytesToRead)
                len = rec.Length

                'If rec(rec.Length - 2) = &HD And rec(rec.Length - 1) = &HA Then
                Response = True
                recvData = rec
                Exit Do
                'Else
                'Response = False
                'End If


            Else
                If len <> 0 Then
                    emptyCnt += 1
                    If emptyCnt > 5 Then
                        Response = False
                        Exit Do
                    End If
                End If

            End If

            If LastTimer.AddSeconds(5) < Now Then
                Exit Do
            End If

        Loop

        If Response = True Then
            CommunicationScan = 1
        Else
            CommunicationScan = 0
        End If
    End Function
    Function tranStrByBuff(ByVal sData() As Byte) As String
        Dim s As String
        'sData = "33 39 37 30 31 32 35 2E 2E 41 31 30 30 30 36 37 30 0D "
        Dim r As String
        Dim i As Integer
        Dim U As Integer
        On Error GoTo Err
        U = UBound(sData)
        r = ""

        For i = 0 To UBound(sData)
            If Val(sData(i)) = 13 Then
                Exit For
            End If
            s = Chr(Val(sData(i)))

            If i = 0 Then
                r = s
            Else
                r = r & s
            End If
        Next
        tranStrByBuff = r
        Exit Function
Err:
        tranStrByBuff = ""
    End Function
    Public Sub ClearData()
        Com_3561.Write(":MEMory:Clear" & vbLf)
    End Sub
    ''' <summary>
    ''' 传送---一批从3561读取的数据 20220825 weng 添加注释
    ''' </summary>
    ''' <returns></returns>
    Friend Function Trans_BatchReadData_3561(ByVal firstCellNum As Integer, ByVal testNum As Integer, ByVal testType As Integer) As String()
        Dim RecvStr As String '接收字符串变量
        Dim strDataArr() As String '字符串数据数组
        Dim Timer As Single '时间值
        Dim msg As String = ""
        '如果串口没有打开，退出
        If Com_3561.IsOpen = False Then
            Return Nothing
        End If
        Dim Start
        '获取当前的日期
        'InsObj.TestData.CurData.TestDate = Date.Now
        '定义字符串集合（链表）mStr
        Dim mStr As New List(Of String)
        Dim retestArray1(63) As Int16  '复测点位数组
        Array.Copy(retestArray, retestArray1, Math.Min(retestArray.Length, retestArray1.Length))
        RecvStr = ""
        '清空串口缓存
        Com_3561.DiscardInBuffer()
        Com_3561.DiscardOutBuffer()
        Com_3561.WriteTimeout = 3000
        '一次性获取内阻仪存储区的数据
        Com_3561.Write("MEMory:DATA?" & vbLf)
        'Port.Port.Write(":FETC?" & vbLf)
        '计算当前的时间，用秒来计算
        Timer = Now.TimeOfDay.Hours * 3600 + Now.TimeOfDay.Minutes * 60 + Now.Second
        Start = Timer
        '定义临时变量接收数据
        Dim TmpRec As String = ""
        '启动循环，如果获取数据不是END并且在5秒内
        Do While Left(Right(Trim(RecvStr), 5), 3) <> "END" And Timer < Start + 5
            Timer = Now.TimeOfDay.Hours * 3600 + Now.TimeOfDay.Minutes * 60 + Now.Second
            '通过串口读取缓存数据
            TmpRec = Com_3561.ReadExisting()
            If TmpRec <> "" Then
                RecvStr = RecvStr + TmpRec
                Start = Timer
            Else
            End If
            '如果长度大于100,字符串集合添加一个元素
            If Len(RecvStr) > 100 Then
                mStr.Add(RecvStr)
                RecvStr = ""
            End If
        Loop
        '跳出循环后，如果RecvStr还有值，继续添加进mStr集合
        If RecvStr <> "" Then
            mStr.Add(RecvStr)
            RecvStr = ""
        End If
        '如果mStr有值
        If mStr.Count > 0 Then
            RecvStr = ""
            '把字符串集合拼成字符串，放在RecvStr
            For mL As Integer = 0 To mStr.Count - 1
                RecvStr = RecvStr & mStr(mL)
            Next
            '拆分字符串，用vbCrLf关键字，放在strDataArr数组
            strDataArr = Split(RecvStr, vbCrLf)
            '直接返回字符串数组strDataArr？
            'Return strDataArr
            '如果字符串数组为空，返回失败提示
            If strDataArr Is Nothing Then
                MsgBox("内阻仪取数失败！")
                ClearData()
                Return Nothing
                '不为空，如果字符串数组的长度==总通道数+2
            ElseIf strDataArr.Length = (testNum + 2) Then
                '启动循环，遍历字符串数组0->length-1
                For i = 0 To strDataArr.Length - 1
                    Dim tmp As String = strDataArr(i)
                    '如果取到的值为END，结束循环
                    If Left(tmp, 3) = "END" Then
                        Exit For
                    Else
                        '继续拆分数组元素，关键字为“，”，拆分三项：arr(0)通道号，arr(1)成电阻值，arr(2)电压值
                        Dim arr() = tmp.Split(",")
                        '定义ch ==元素素组数组（0）-1
                        Dim ch As Integer
                        If (testType = 1) Then
                            ch = Val(arr(0) - 1) + （firstCellNum - 1）
                        End If
                        If (testType = 0) Then
                            ch = 0
                            Dim j As Integer
                            For j = 0 To retestArray1.Length - 1
                                If retestArray1(j) = 1 Then
                                    ch = j
                                    retestArray1(j) = 0
                                    Exit For
                                End If
                            Next
                        End If
                        '如果通道号小于总的通道数，存储通道对应的值
                        If ch < (testNum + firstCellNum) Then
                            '如果元素数组长度>=3,才是正确数据
                            If arr.Length >= 3 Then
                                RecieveData(ch).Resistance = Val(arr(1)) * 1000 + calibrateValueArray(ch) '+ Val(Initsetting.sngR_B)
                                If RecieveData(ch).Resistance > 10000 Then
                                    RecieveData(ch).Resistance = 10000
                                End If
                                RecieveData(ch).Voltage = Val(arr(2)) '+ Val(Initsetting.sngV_B)
                                If RecieveData(ch).Voltage > 10000 Then
                                    RecieveData(ch).Voltage = 10000
                                End If
                            Else
                                msg = "解析传回的电压、内阻值出错:" & ch & ":" & tmp
                            End If
                            '继续完善结构数据的其他元素TestTime，IsTest
                            RecieveData(ch).TestTime = Now
                            Dim CHSTRING As String
                            If ch < 10 Then
                                CHSTRING = "0" + ch.ToString()
                            Else
                                CHSTRING = ch.ToString()
                            End If
                            'RecieveData(ch).BatCode = CHSTRING
                        Else
                            msg = "传回的内阻索引值超出范围"
                        End If
                    End If
                Next
                Return Nothing
            Else
                msg = "内阻仪取数失败！个数不对： " & strDataArr.Length - (testNum + 2)
                Return Nothing
            End If
        Else
            msg = "内阻仪取数失败！没有接收到任何数据"
            Return Nothing
        End If
    End Function
    '向34461发送指令
    Public Function WriteLine(ByVal p As String) As String
        Dim msg As String = ""
        IsSending = True
        IsRec = False

        visaSession.WriteString(p & vbLf, True)
        'visaSession.WriteString(p & vbLf, True)
        Dim tmp As DateTime = DateTime.Now
        Dim isFin As Boolean = False
        Dim c As Integer = 0

        While (tmp.AddSeconds(10) > DateTime.Now OrElse c < 3) AndAlso isFin = False AndAlso IsRec = False
            c += 1
            ProcessData()
            System.Threading.Thread.Sleep(10)
        End While

        If IsRec = False Then
            msg = "接收34461数据失败"
        End If

        Return msg
    End Function

    ''' <summary>
    ''' 读取34461的电压值，并保存在
    ''' </summary>
    Public Sub ProcessData()
        ReturnValue34461 = ""
        ReturnValue34461 = visaSession.ReadString()
        'ReturnValue34461 = visaSession.IO.ReadSTB(Of Single)
        'recDataof34461 = visaSession.ReadNumber()
        '把读取34461仪表值赋值给公共变量recDataof34461
        recDataof34461 = Convert.ToDouble(ReturnValue34461)

        'For i As Integer = 0 To mList.Count - 1
        '    mList(i)(ReturnValue)
        'Next
        If ReturnValue34461 <> "" Then
            IsRec = True
        End If
    End Sub
    Public Sub Read34461A(ByVal firstCellNum As Integer, ByVal testNum As Integer)
        Dim rm As Ivi.Visa.Interop.ResourceManager = New Ivi.Visa.Interop.ResourceManager()   '打开一个新的资源管理器
        Dim myDmm As Ivi.Visa.Interop.FormattedIO488 = New Ivi.Visa.Interop.FormattedIO488() '打开新的格式化IO 488会话
        Dim delayTim As Integer = testNum * 300
        Try
            Dim DutAddr As String = "TCPIP0::192.168.66.81"   'LAN的示例字符串
            DutAddr = Initsetting.IpAddOf34461
            myDmm.IO = rm.Open(DutAddr, AccessMode.NO_LOCK, 2000, "") '打开DMM的手柄，超时2秒
            ' myDmm.WriteString("READ?") '将DMM设置为等待触发状态，并在下一个触发信号后获取读数。
            Dim DCVResult As Object() = myDmm.ReadList(IEEEASCIIType.ASCIIType_Any)  '获取数组中的读数

            '继续拆分数组元素，关键字为“，”，拆分三项：arr(0)通道号，arr(1)成电阻值，arr(2)电压值
            'Dim arr() = DCVResult.Split(",")
            '定义ch ==元素素组数组（0）-1
            Dim ch As Integer = firstCellNum

            '把读出的电压数据放在RecieveData(ch).Voltage中
            For ch = firstCellNum To firstCellNum + testNum
                RecieveData(ch).Voltage = DCVResult(ch - firstCellNum)
                If RecieveData(ch).Voltage > 10000 Then
                    RecieveData(ch).Voltage = 10000
                End If
            Next
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
    Public Sub extTrig34461(ByVal firstCellNum As Integer, ByVal testNum As Integer) ' As String()
        Dim rm As Ivi.Visa.Interop.ResourceManager = New Ivi.Visa.Interop.ResourceManager()   '打开一个新的资源管理器
        Dim myDmm As Ivi.Visa.Interop.FormattedIO488 = New Ivi.Visa.Interop.FormattedIO488() '打开新的格式化IO 488会话
        Dim delayTim As Integer = testNum * 300
        Dim commString As String
        Try

            Dim DutAddr As String = "TCPIP0::192.168.66.81"   'LAN的示例字符串
            DutAddr = Initsetting.IpAddOf34461
            myDmm.IO = rm.Open(DutAddr, AccessMode.NO_LOCK, 2000, "") '打开DMM的手柄，超时2秒
            myDmm.IO.Timeout = 100000  '您也可以通过执行此命令设置超时，设置为10秒.要计算，不通点位数不同

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


            Dim DCVResult As Object() = myDmm.ReadList(IEEEASCIIType.ASCIIType_Any)  '获取数组中的读数

            '继续拆分数组元素，关键字为“，”，拆分三项：arr(0)通道号，arr(1)成电阻值，arr(2)电压值
            'Dim arr() = DCVResult.Split(",")
            '定义ch ==元素素组数组（0）-1
            Dim ch As Integer = firstCellNum

            '把读出的电压数据放在RecieveData(ch).Voltage中

            For ch = firstCellNum To firstCellNum + testNum - 1
                RecieveData(ch).Voltage = DCVResult(ch - firstCellNum)
                If RecieveData(ch).Voltage > 10000 Then
                    RecieveData(ch).Voltage = 10000
                End If
                'TextBox1.AppendText((RecieveData(ch).Voltage).ToString() & vbCrLf)
            Next
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
    Sub CheckDMMError(myDmm As FormattedIO488)
        myDmm.WriteString("SYST:ERR?")
        Dim errStr As String = myDmm.ReadString()

        If (errStr.Contains("No error")) Then 'If no error, then return
            Return
            'If there is an error, read out all of the errors and return them in an exception
        Else
            Dim errStr2 As String = ""
            Do
                myDmm.WriteString("SYST:ERR?")
                errStr2 = myDmm.ReadString()
                If (Not errStr2.Contains("No error")) Then
                    errStr = errStr + "\n" + errStr2
                End If
            Loop While (Not errStr2.Contains("No error"))
            Throw New Exception("Exception: Encountered system error(s)\n" + errStr)

        End If

    End Sub
    Public Function Trans_ReadData_34461() As Integer


        Dim msg As String = ""
        If ReFlag_34461 = -1 Then
            ReFlag_34461 = ConnectFor34461()
        End If

        If ReFlag_34461 <> 0 Then
            MsgBox("数字万用表连接失败！", vbOKOnly)
            Trans_ReadData_34461 = -1
            Exit Function
        Else
            'MsgBox("数字万用表连接成功！", vbOKOnly)
        End If
        msg = WriteLine（"MEAS:VOLT:DC?"）
        'txtSend.Text = "MEAS:VOLT:DC?"
        'txtRecieve_34461.Text = recDataof34461.ToString()

        If msg <> "" Then
            MsgBox("数字万用表接收电压数据失败！", vbOKOnly)
            Trans_ReadData_34461 = -1
            Exit Function
        Else
            'MsgBox("数字万用表接收电压数据成功！", vbOKOnly)
        End If
        Trans_ReadData_34461 = 0
    End Function

    ''' <summary>
    ''' 传送---一批从34465读取的数据 20230928 weng 添加注释
    ''' </summary>
    ''' <returns></returns>
    Friend Function Trans_BatchReadData_34465(ByVal firstCellNum As Integer, ByVal testNum As Integer) As String()
        Dim RecvStr As String '接收字符串变量
        Dim strDataArr() As String '字符串数据数组
        Dim Timer As Single '时间值
        Dim msg As String = ""
        '如果串口没有打开，退出
        If Com_3561.IsOpen = False Then
            Return Nothing
        End If
        Dim Start
        '获取当前的日期
        'InsObj.TestData.CurData.TestDate = Date.Now
        '定义字符串集合（链表）mStr
        Dim mStr As New List(Of String)

        RecvStr = ""
        '清空串口缓存
        Com_3561.DiscardInBuffer()
        Com_3561.DiscardOutBuffer()
        Com_3561.WriteTimeout = 3000
        '一次性获取内阻仪存储区的数据
        Com_3561.Write("MEMory:DATA?" & vbLf)
        'Port.Port.Write(":FETC?" & vbLf)
        '计算当前的时间，用秒来计算
        Timer = Now.TimeOfDay.Hours * 3600 + Now.TimeOfDay.Minutes * 60 + Now.Second
        Start = Timer
        '定义临时变量接收数据
        Dim TmpRec As String = ""
        '启动循环，如果获取数据不是END并且在5秒内
        Do While Left(Right(Trim(RecvStr), 5), 3) <> "END" And Timer < Start + 5
            Timer = Now.TimeOfDay.Hours * 3600 + Now.TimeOfDay.Minutes * 60 + Now.Second
            '通过串口读取缓存数据
            TmpRec = Com_3561.ReadExisting()
            If TmpRec <> "" Then
                RecvStr = RecvStr + TmpRec
                Start = Timer
            Else
            End If
            '如果长度大于100,字符串集合添加一个元素
            If Len(RecvStr) > 100 Then
                mStr.Add(RecvStr)
                RecvStr = ""
            End If
        Loop
        '跳出循环后，如果RecvStr还有值，继续添加进mStr集合
        If RecvStr <> "" Then
            mStr.Add(RecvStr)
            RecvStr = ""
        End If
        '如果mStr有值
        If mStr.Count > 0 Then
            RecvStr = ""
            '把字符串集合拼成字符串，放在RecvStr
            For mL As Integer = 0 To mStr.Count - 1
                RecvStr = RecvStr & mStr(mL)
            Next
            '拆分字符串，用vbCrLf关键字，放在strDataArr数组
            strDataArr = Split(RecvStr, vbCrLf)
            '直接返回字符串数组strDataArr？
            'Return strDataArr
            '如果字符串数组为空，返回失败提示
            If strDataArr Is Nothing Then
                MsgBox("内阻仪取数失败！")
                ClearData()
                Return Nothing
                '不为空，如果字符串数组的长度==总通道数+2
            ElseIf strDataArr.Length = (testNum + 2) Then
                '启动循环，遍历字符串数组0->length-1
                For i = 0 To strDataArr.Length - 1
                    Dim tmp As String = strDataArr(i)
                    '如果取到的值为END，结束循环
                    If Left(tmp, 3) = "END" Then
                        Exit For
                    Else
                        '继续拆分数组元素，关键字为“，”，拆分三项：arr(0)通道号，arr(1)成电阻值，arr(2)电压值
                        Dim arr() = tmp.Split(",")
                        '定义ch ==元素素组数组（0）-1
                        Dim ch As Integer = Val(arr(0) - 1) + （firstCellNum - 1）
                        '如果通道号小于总的通道数，存储通道对应的值
                        If ch < (testNum + firstCellNum) Then
                            '如果元素数组长度>=3,才是正确数据
                            If arr.Length >= 3 Then
                                RecieveData(ch).Resistance = Val(arr(1)) * 1000 + Val(Initsetting.sngR_B)
                                If RecieveData(ch).Resistance > 10000 Then
                                    RecieveData(ch).Resistance = 10000
                                End If
                                RecieveData(ch).Voltage = Val(arr(2)) + Val(Initsetting.sngV_B)
                                If RecieveData(ch).Voltage > 10000 Then
                                    RecieveData(ch).Voltage = 10000
                                End If
                            Else
                                msg = "解析传回的电压、内阻值出错:" & ch & ":" & tmp
                            End If
                            '继续完善结构数据的其他元素TestTime，IsTest
                            RecieveData(ch).TestTime = Now
                            ' RecieveData(ch).BatCode = frmInput.txtTray.Text
                        Else
                            msg = "传回的内阻索引值超出范围"
                        End If
                    End If
                Next
                Return Nothing
            Else
                msg = "内阻仪取数失败！个数不对： " & strDataArr.Length - (testNum + 2)
                Return Nothing
            End If
        Else
            msg = "内阻仪取数失败！没有接收到任何数据"
            Return Nothing
        End If
    End Function
    Public Function CheckCom_IOCard() As Short
        'IO 板由控制板控制，上位机不再打开或检测 IO 串口。
        CheckCom_IOCard = 1
    End Function

    ''' <summary>
    ''' 检查34461数字万用表的链接与通信
    ''' </summary>
    ''' <returns></returns>
    Public Function CheckCom_34461() As Short
        Dim rm As Ivi.Visa.Interop.ResourceManager = New Ivi.Visa.Interop.ResourceManager()   '打开一个新的资源管理器
        Dim myDmm As Ivi.Visa.Interop.FormattedIO488 = New Ivi.Visa.Interop.FormattedIO488() '打开新的格式化IO 488会话
        Try

            Dim DutAddr As String = "TCPIP0::192.168.66.81"   'LAN的示例字符串
            DutAddr = Initsetting.IpAddOf34461
            myDmm.IO = rm.Open(DutAddr, AccessMode.NO_LOCK, 2000, "") '打开DMM的手柄，超时2秒
            myDmm.IO.Timeout = VisaInstrumentIoTimeoutMs

            '启动检查只确认设备身份，不复位仪表，避免启动阶段等待仪表初始化。
            myDmm.WriteString("*IDN?")  'Get the IDN string                
            Dim IDN As String = myDmm.ReadString()
            If IDN.Contains("Keysight Technologies") Then
                CheckCom_34461 = 0
            Else
                CheckCom_34461 = -1
            End If
            Console.WriteLine(IDN)  'report the DMM's identity

        Catch
            CheckCom_34461 = -1
            Exit Function
        Finally
            '启动检查只使用临时会话；必须释放它，避免随后正式采集再开会话时与34461争用控制权。
            Try
                If myDmm IsNot Nothing AndAlso myDmm.IO IsNot Nothing Then
                    myDmm.IO.Close()
                End If
            Catch
            End Try
            Try
                If myDmm IsNot Nothing Then
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(myDmm)
                End If
            Catch
            End Try
            Try
                If rm IsNot Nothing Then
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(rm)
                End If
            Catch
            End Try
        End Try
    End Function
    ''' <summary>
    ''' 连接安捷伦 34461仪表
    ''' </summary>
    ''' <returns></returns>
    Public Function ConnectFor34461() As Integer

        'Dim instr As String = "USB0::0x2A8D::0x1201::MY57501015::0::INSTR"
        Dim instr As String = "TCPIP0::192.168.66.81::inst0::INSTR"
        instr = Initsetting.IpAddOf34461
        '重连前关闭旧会话，避免启动自检或上一次测试遗留的会话继续占用仪表。
        Try
            If visaSession IsNot Nothing AndAlso visaSession.IO IsNot Nothing Then
                visaSession.IO.Close()
            End If
        Catch
        End Try
        Try
            If visaSession IsNot Nothing Then
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(visaSession)
            End If
        Catch
        End Try
        visaSession = New Ivi.Visa.Interop.FormattedIO488
        Try
            visaSession.IO = ResourcePort.Open(instr, Ivi.Visa.Interop.AccessMode.NO_LOCK, 5000, "")
            visaSession.IO.Timeout = VisaInstrumentIoTimeoutMs
        Catch
            visaSession = Nothing
            ConnectFor34461 = -1
            Exit Function
        End Try
        ConnectFor34461 = 0
    End Function
    ''' <summary>
    ''' 添加日志
    ''' </summary>
    ''' <param name="logConstant"></param>
    ''' <returns></returns>
    Public Function AddAlarmLog(ByVal logConstant As String) As Integer
        ' 记录错误到系统事件日志
        Dim eventLogName As String = "MyEventLog" ' 事件日志的名称，替换为你想要使用的日志名称
        Dim sourceName As String = "MyApplication" ' 日志来源的名称，替换为你的应用程序名称
        AddAlarmLog = 1

        If Not EventLog.SourceExists(sourceName) Then
            EventLog.CreateEventSource(sourceName, eventLogName)
        End If

        Using eventLog As New EventLog(eventLogName)
            eventLog.Source = sourceName
            eventLog.WriteEntry("报警信息：" & logConstant & "  " & Now.ToString("s"))
        End Using

    End Function
End Module
