Imports System.IO
Imports System.Runtime.InteropServices
Public Class frmInit
    Dim TempInitsetting As Initset
    Private Sub CmdOK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdOK.Click
        Dim Rebutton As Microsoft.VisualBasic.MsgBoxResult
        If Not TextToStruct() Then Return
        Rebutton = MsgBox("新的设置将在下次启动时生效！如果想使设置马上生效，请退出本程序并重新进入，确定退出本程序吗？", vbYesNo)
        If Rebutton = vbYes Then
            Initsetting = TempInitsetting

            Call SaveInitFile()
            End

        Else
            Me.Close()
        End If
    End Sub
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Me.Close()
    End Sub
    Private Sub frmInit_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim I As Int16
        InitializeRetest3562Controls()
        CmbRecom.Items.Clear()
        cmb_ScanerPort.Items.Clear()
        CmbIORecom.Items.Clear()
        cmb_SCMPort.Items.Clear()
        ComRate.Items.Clear()
        cmbRerate.Items.Clear()
        cmb_SCMRate.Items.Clear()
        cmbIORerate.Items.Clear()
        cmb_ScanerRate.Items.Clear()

        For I = 0 To 19
            With CmbRecom
                .Items.Add((I + 1).ToString)
                .SelectedIndex = 0
            End With
            With cmb_ScanerPort
                .Items.Add((I + 1).ToString)
                .SelectedIndex = 0
            End With
            With CmbIORecom
                .Items.Add((I + 1).ToString)
                .SelectedIndex = 0
            End With
        Next
        For I = 0 To 63
            cmb_SCMPort.Items.Add((I + 1).ToString)
            cmb_PlcPort.Items.Add((I + 1).ToString)
        Next
        cmb_SCMPort.SelectedIndex = 0
        cmb_PlcPort.SelectedIndex = 0
        With ComRate
            '.Items.Clear()
            .Items.Add("低速")
            .Items.Add("中速")
            .Items.Add("高速")
            .Items.Add("超快")
            .SelectedIndex = 2
        End With
        With cmbRerate
            .Items.Add("9600")
            .Items.Add("19200")
            .Items.Add("57600")
            .SelectedIndex = 1
        End With
        With cmb_SCMRate
            .Items.Add("9600")
            .Items.Add("57600")
            .Items.Add("115200")
            .SelectedIndex = 2
        End With
        With cmb_PlcRate
            .Items.Clear()
            .Items.Add("9600")
            .Items.Add("57600")
            .Items.Add("115200")
            .SelectedIndex = 2
        End With
        With cmbIORerate
            .Items.Add("9600")
            .Items.Add("57600")
            .Items.Add("115200")
            .SelectedIndex = 2
        End With
        With cmb_ScanerRate
            .Items.Add("9600")
            .Items.Add("19200")
            .Items.Add("57600")
            .SelectedIndex = 0
        End With
        With cmbRetestPort
            .Items.Clear()
            For I = 0 To 63
                .Items.Add((I + 1).ToString())
            Next
        End With
        With cmbRetestRate
            .Items.Clear()
            .Items.Add("9600")
            .Items.Add("57600")
            .Items.Add("115200")
        End With
        Call structToText()
        UpdatePlcTransportControls()
    End Sub
    Private Function TextToStruct() As Boolean
        If Not ChbPLCIP.Checked AndAlso Not ChbPlcCom.Checked Then
            ChbPlcCom.Checked = True
        End If

        If ChbPLCIP.Checked Then
            Dim tcpPort As Integer
            If String.IsNullOrWhiteSpace(Txt_PlcIP.Text) OrElse Not Integer.TryParse(txt_PlcPort.Text, tcpPort) OrElse tcpPort < 1 OrElse tcpPort > 65535 Then
                MsgBox("PLC TCP 模式需要填写有效的 IP 地址和端口号。", vbOKOnly, "PLC 参数错误")
                Return False
            End If
            TempInitsetting.PLCMode = 1
            TempInitsetting.PLCIP = Txt_PlcIP.Text.Trim()
            TempInitsetting.PLCTcpPort = tcpPort
        Else
            If cmb_PlcPort.SelectedIndex < 0 OrElse cmb_PlcRate.SelectedIndex < 0 Then
                MsgBox("PLC COM 模式需要选择串口号和波特率。", vbOKOnly, "PLC 参数错误")
                Return False
            End If
            TempInitsetting.PLCMode = 0
            TempInitsetting.PLCComPort = cmb_PlcPort.SelectedIndex
            TempInitsetting.PLCComRate = CInt(cmb_PlcRate.SelectedItem)
        End If

        '通讯参数
        TempInitsetting.ScanerPort = cmb_ScanerPort.SelectedIndex
        TempInitsetting.ScanerRate = cmb_ScanerRate.Items.Item(cmb_ScanerRate.SelectedIndex)

        'Dim SCMIpAdd As String   '中位机IP地址  weng 20220802 添加
        'Dim SCM_port As Integer  '中位机网络端口
        TempInitsetting.SCMIpAdd = Txt_ScmIP.Text
        TempInitsetting.SCM_port = txt_ScmPort.Text
        TempInitsetting.SCMComPort = cmb_SCMPort.SelectedIndex
        TempInitsetting.SCMComRate = CInt(cmb_SCMRate.Items.Item(cmb_SCMRate.SelectedIndex))

        If cmbRetestPort Is Nothing OrElse cmbRetestPort.SelectedIndex < 0 OrElse cmbRetestRate.SelectedIndex < 0 Then
            MsgBox("NG复测3562需要选择串口号和波特率。", vbOKOnly, "复测3562配置错误")
            Return False
        End If
        TempInitsetting.Retest3562Port = cmbRetestPort.SelectedIndex
        TempInitsetting.Retest3562Rate = CInt(cmbRetestRate.SelectedItem)

        TempInitsetting.IpAddOf34461 = txt34461.Text

        TempInitsetting.IOPort = CmbIORecom.SelectedIndex
        TempInitsetting.IORate = cmbIORerate.Items.Item(cmbIORerate.SelectedIndex)


        TempInitsetting.ComRateMode = ComRate.Text
        TempInitsetting.Recom = CmbRecom.SelectedIndex
        TempInitsetting.ReRate = cmbRerate.Items.Item(cmbRerate.SelectedIndex)
        '所有通讯下拉框已经写入 TempInitsetting 后，再校验复测3562串口冲突。
        If TempInitsetting.Retest3562Port = TempInitsetting.Recom OrElse
           TempInitsetting.Retest3562Port = TempInitsetting.ScanerPort OrElse
           TempInitsetting.Retest3562Port = TempInitsetting.SCMComPort OrElse
           TempInitsetting.Retest3562Port = TempInitsetting.IOPort OrElse
           (Not ChbPLCIP.Checked AndAlso TempInitsetting.Retest3562Port = TempInitsetting.PLCComPort) Then
            MsgBox("NG复测3562串口不能与主3562、扫码枪、中位机、IO卡或PLC串口相同。", vbOKOnly, "复测3562配置错误")
            Return False
        End If
        '电压内阻合格范围
        If Val(txtRes1.Text) - Val(txtRes2.Text) >= 0 Then
            TempInitsetting.upResistance = Val(txtRes1.Text)
            TempInitsetting.downResistance = Val(txtRes2.Text)
        Else
            TempInitsetting.upResistance = Val(txtRes2.Text)
            TempInitsetting.downResistance = Val(txtRes1.Text)
        End If
        If Val(txtVol1.Text) - Val(txtVol2.Text) >= 0 Then
            TempInitsetting.upVoltage = Val(txtVol1.Text)
            TempInitsetting.downVoltage = Val(txtVol2.Text)
        Else
            TempInitsetting.upVoltage = Val(txtVol2.Text)
            TempInitsetting.downVoltage = Val(txtVol1.Text)
        End If
        '数据库
        TempInitsetting.chkDatabase = chkAuto.Checked
        TempInitsetting.strDatabase = txtDatabase.Text
        '文件路径
        TempInitsetting.sPath = txtPath.Text
        TempInitsetting.autoUpdate = chkAuto.Checked
        '电压内阻B值(校准值）
        TempInitsetting.sngV_B = CSng(txt_V_B.Text)
        TempInitsetting.sngR_B = CSng(txt_R_B.Text)
        '条码长度
        TempInitsetting.txtCodeLenth = Val(txtCodeLenth.Text)

        'database userID and password
        TempInitsetting.strUserID = txtUser.Text
        TempInitsetting.strpwd = txtpwd.Text
        TempInitsetting.delayTime = Txt_DelayTime.Text
        '探针的使用次数管理
        TempInitsetting.Probelifespan = txtProbelifespan.Text
        TempInitsetting.ProbeUsedCount = txtProbeUsed.Text
        '用户管理
        TempInitsetting.PsdOfAdmin = txtPsdAdmin.Text
        TempInitsetting.PsdOfOper = txtPsdOper.Text
        TempInitsetting.PsdOfGuest = txtPsdGuest.Text
        'mes 管理
        TempInitsetting.MesApiAddress = txtMesApiAddress.Text
        '校准模块阻值
        TempInitsetting.cailibrateModelValue = txtCailiModelValue.Text
        '超温报警
        TempInitsetting.overTemperateValue = txtOverTemperate.Text
        '工作站选择
        If radioOCV0Set.Checked Then
            TempInitsetting.stationSelect = 0
        End If

        If radioOCV1Set.Checked Then
            TempInitsetting.stationSelect = 1
        Else
            TempInitsetting.stationSelect = 2
        End If
        Return True
    End Function

    Private Sub UpdatePlcTransportControls()
        Dim tcpSelected As Boolean = ChbPLCIP.Checked
        Dim comSelected As Boolean = ChbPlcCom.Checked
        If Not tcpSelected AndAlso Not comSelected Then
            ChbPlcCom.Checked = True
            comSelected = True
        End If

        Txt_PlcIP.Visible = tcpSelected
        txt_PlcPort.Visible = tcpSelected
        Label46.Visible = tcpSelected
        Label47.Visible = tcpSelected
        cmb_PlcPort.Visible = comSelected
        cmb_PlcRate.Visible = comSelected
        Label45.Visible = comSelected
        Label44.Visible = comSelected
    End Sub

    Private Sub ChbPLCIP_CheckedChanged(sender As Object, e As EventArgs) Handles ChbPLCIP.CheckedChanged
        If ChbPLCIP.Checked Then
            ChbPlcCom.Checked = False
        ElseIf Not ChbPlcCom.Checked Then
            ChbPlcCom.Checked = True
        End If
        UpdatePlcTransportControls()
    End Sub

    Private Sub ChbPlcCom_CheckedChanged(sender As Object, e As EventArgs) Handles ChbPlcCom.CheckedChanged
        If ChbPlcCom.Checked Then
            ChbPLCIP.Checked = False
        ElseIf Not ChbPLCIP.Checked Then
            ChbPLCIP.Checked = True
        End If
        UpdatePlcTransportControls()
    End Sub
    Private Sub structToText()
        Dim intLoop As Int16
        '**************************
        '通讯参数
        'ComRate.SelectedIndex = Initsetting.SCM_com
        'For intLoop = 0 To ComRate.Items.Count - 1
        '    If cmbRerate.Items.Item(intLoop) = Initsetting.ComRateMode Then
        '        ComRate.SelectedIndex = intLoop
        '        Exit For
        '    End If
        'Next
        'CmbIORecom

        CmbIORecom.SelectedIndex = Initsetting.IOPort
        For intLoop = 0 To cmbIORerate.Items.Count - 1
            If cmbIORerate.Items.Item(intLoop) = Initsetting.IORate Then
                cmbIORerate.SelectedIndex = intLoop
                Exit For
            End If
        Next
        CmbRecom.SelectedIndex = Initsetting.Recom
        For intLoop = 0 To cmbRerate.Items.Count - 1
            If cmbRerate.Items.Item(intLoop) = Initsetting.ReRate Then
                cmbRerate.SelectedIndex = intLoop
                Exit For
            End If
        Next
        cmb_ScanerPort.SelectedIndex = Initsetting.ScanerPort
        For intLoop = 0 To cmb_ScanerRate.Items.Count - 1
            If cmb_ScanerRate.Items.Item(intLoop) = Initsetting.ScanerPort Then
                cmb_ScanerRate.SelectedIndex = intLoop
                Exit For
            End If
        Next
        'ReadInitFile()
        Txt_ScmIP.Text = Initsetting.SCMIpAdd
        txt_ScmPort.Text = Initsetting.SCM_port
        cmb_SCMPort.SelectedIndex = Math.Max(0, Math.Min(cmb_SCMPort.Items.Count - 1, Initsetting.SCMComPort))
        For intLoop = 0 To cmb_SCMRate.Items.Count - 1
            If CInt(cmb_SCMRate.Items.Item(intLoop)) = Initsetting.SCMComRate Then
                cmb_SCMRate.SelectedIndex = intLoop
                Exit For
            End If
        Next
        If cmb_SCMRate.SelectedIndex < 0 Then
            cmb_SCMRate.SelectedIndex = 2
        End If
        cmbRetestPort.SelectedIndex = Math.Max(0, Math.Min(cmbRetestPort.Items.Count - 1, Initsetting.Retest3562Port))
        For intLoop = 0 To cmbRetestRate.Items.Count - 1
            If CInt(cmbRetestRate.Items(intLoop)) = If(Initsetting.Retest3562Rate > 0, Initsetting.Retest3562Rate, 9600) Then
                cmbRetestRate.SelectedIndex = intLoop
                Exit For
            End If
        Next
        If cmbRetestRate.SelectedIndex < 0 Then cmbRetestRate.SelectedIndex = 0
        Txt_PlcIP.Text = If(String.IsNullOrWhiteSpace(Initsetting.PLCIP), "127.0.0.1", Initsetting.PLCIP)
        txt_PlcPort.Text = If(Initsetting.PLCTcpPort > 0, Initsetting.PLCTcpPort.ToString(), "502")
        cmb_PlcPort.SelectedIndex = Math.Max(0, Math.Min(cmb_PlcPort.Items.Count - 1, Initsetting.PLCComPort))
        For intLoop = 0 To cmb_PlcRate.Items.Count - 1
            If CInt(cmb_PlcRate.Items.Item(intLoop)) = If(Initsetting.PLCComRate > 0, Initsetting.PLCComRate, 115200) Then
                cmb_PlcRate.SelectedIndex = intLoop
                Exit For
            End If
        Next
        If Initsetting.PLCMode = 1 Then
            ChbPLCIP.Checked = True
        Else
            ChbPlcCom.Checked = True
        End If
        UpdatePlcTransportControls()
        '*********************************
        '内阻电压上下限
        txtRes1.Text = Initsetting.downResistance
        txtRes2.Text = Initsetting.upResistance
        txtVol1.Text = Initsetting.downVoltage
        txtVol2.Text = Initsetting.upVoltage
        '**********************************
        '数据库IP
        chkAuto.Checked = Initsetting.chkDatabase
        txtDatabase.Text = Initsetting.strDatabase
        '文件名
        txtPath.Text = Initsetting.sPath
        'TempInitsetting.sngV_B
        chkAuto.Checked = Initsetting.autoUpdate
        '电压内阻B值
        txt_V_B.Text = Initsetting.sngV_B
        txt_R_B.Text = Initsetting.sngR_B
        '条码长度
        txtCodeLenth.Text = Initsetting.txtCodeLenth
        'DataBase userID and password
        txtUser.Text = Initsetting.strUserID
        txtpwd.Text = Initsetting.strpwd
        '通道切换时间
        Txt_DelayTime.Text = Initsetting.delayTime
        '探针使用寿命统计
        txtProbelifespan.Text = Initsetting.Probelifespan
        txtProbeUsed.Text = Initsetting.ProbeUsedCount
        '用户管理
        txtPsdAdmin.Text = Initsetting.PsdOfAdmin
        txtPsdOper.Text = Initsetting.PsdOfOper
        txtPsdGuest.Text = Initsetting.PsdOfGuest
        '万用表的ip地址
        txt34461.Text = Initsetting.IpAddOf34461
        'MES api 地址
        txtMesApiAddress.Text = Initsetting.MesApiAddress
        '校准模块阻值
        txtCailiModelValue.Text = Initsetting.cailibrateModelValue
        '超温报警
        txtOverTemperate.Text = Initsetting.overTemperateValue
        '工作站选择
        If Initsetting.stationSelect = 0 Then
            radioOCV0Set.Checked = True
        End If
        If Initsetting.stationSelect = 1 Then
            radioOCV1Set.Checked = True
        End If
        If Initsetting.stationSelect = 2 Then
            radioOCV2Set.Checked = True
        End If

    End Sub
    Private Sub txtRes1_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtRes1.Validated
        If Not IsNumeric(txtRes1.Text) Then
            txtRes1.Clear()
        End If
    End Sub
    Private Sub txtRes2_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtRes2.Validated
        If Not IsNumeric(txtRes2.Text) Then
            txtRes2.Clear()
        End If
    End Sub
    Private Sub txtVol1_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtVol1.Validated
        If Not IsNumeric(txtVol1.Text) Then
            txtVol1.Clear()
        End If
    End Sub
    Private Sub txtVol2_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtVol2.Validated, txtOverTemperate.Validated
        If Not IsNumeric(txtVol2.Text) Then
            txtVol2.Clear()
        End If
    End Sub
    Private Sub txtDatabase_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtDatabase.Validated, txtMesApiAddress.Validated
        If txtDatabase.Text = "" Then
            txtDatabase.Focus()
        End If
    End Sub
    Private Sub TextBox1_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtPath.Validated
        Dim sPath As String
        sPath = Trim(txtPath.Text)
        If String.IsNullOrEmpty(sPath) Then
            txtPath.Text = "C:\"
        End If
    End Sub

    Public Sub New()
        ' 此调用是设计器所必需的。
        InitializeComponent()
        InitializeRetest3562Controls()
        ' 在 InitializeComponent() 调用之后添加任何初始化。
    End Sub

    Private Sub InitializeRetest3562Controls()
        If cmbRetestPort IsNot Nothing Then
            AddHandler btnRetestConnect.Click, AddressOf RetestConnectClick
            Return
        End If
        Dim group As New GroupBox() With {
            .Name = "grpRetest3562",
            .Text = "NG复测3562",
            .Location = New Point(11, 409),
            .Size = New Size(361, 60)
        }
        Dim portLabel As New Label() With {.Text = "串口", .AutoSize = True, .Location = New Point(8, 25)}
        Dim rateLabel As New Label() With {.Text = "波特率", .AutoSize = True, .Location = New Point(122, 25)}
        cmbRetestPort = New ComboBox() With {.Name = "cmbRetest3562Port", .DropDownStyle = ComboBoxStyle.DropDownList, .Location = New Point(42, 20), .Width = 72}
        cmbRetestRate = New ComboBox() With {.Name = "cmbRetest3562Rate", .DropDownStyle = ComboBoxStyle.DropDownList, .Location = New Point(178, 20), .Width = 85}
        btnRetestConnect = New Button() With {.Name = "btnRetest3562Connect", .Text = "测试连接", .Location = New Point(278, 19), .Size = New Size(82, 25)}
        group.Controls.AddRange(New Control() {portLabel, rateLabel, cmbRetestPort, cmbRetestRate, btnRetestConnect})
        AddHandler btnRetestConnect.Click, AddressOf RetestConnectClick
        TabPage1.Controls.Add(group)
    End Sub

    Private Sub RetestConnectClick(ByVal sender As Object, ByVal e As EventArgs)
        If cmbRetestPort Is Nothing OrElse cmbRetestPort.SelectedIndex < 0 OrElse cmbRetestRate.SelectedIndex < 0 Then
            btnRetestConnect.Text = "参数无效"
            Return
        End If
        Try
            Using service As New Retest3562Service()
                service.Connect("COM" & (cmbRetestPort.SelectedIndex + 1).ToString(), CInt(cmbRetestRate.SelectedItem))
            End Using
            btnRetestConnect.Text = "连接成功"
        Catch ex As Exception
            btnRetestConnect.Text = "连接失败"
            MessageBox.Show(ex.Message, "NG复测3562连接失败", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub
    Private Sub txt_V_B_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txt_V_B.Validated
        If Not IsNumeric(txt_V_B.Text) Then
            txt_V_B.Clear()
        End If
    End Sub
    Private Sub txt_R_B_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txt_R_B.Validated
        If Not IsNumeric(txt_R_B.Text) Then
            txt_R_B.Clear()
        End If
    End Sub

    Private Sub Label15_Click(sender As Object, e As EventArgs) Handles Label15.Click

    End Sub

    Private Sub Label17_Click(sender As Object, e As EventArgs) Handles Label17.Click, Label41.Click

    End Sub

    Private Sub Label16_Click(sender As Object, e As EventArgs) Handles Label16.Click

    End Sub

    Private Sub txt_V_B_TextChanged(sender As Object, e As EventArgs) Handles txt_V_B.TextChanged

    End Sub

    Private Sub txt_R_B_TextChanged(sender As Object, e As EventArgs) Handles txt_R_B.TextChanged

    End Sub
End Class
