<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmInit
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
            If disposing AndAlso colorDialogChannel IsNot Nothing Then
                colorDialogChannel.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.GroupBox9 = New System.Windows.Forms.GroupBox()
        Me.btnPlcConnectTest = New System.Windows.Forms.Button()
        Me.ChbPlcCom = New System.Windows.Forms.CheckBox()
        Me.ChbPLCIP = New System.Windows.Forms.CheckBox()
        Me.cmb_PlcRate = New System.Windows.Forms.ComboBox()
        Me.Label44 = New System.Windows.Forms.Label()
        Me.cmb_PlcPort = New System.Windows.Forms.ComboBox()
        Me.Label45 = New System.Windows.Forms.Label()
        Me.txt_PlcPort = New System.Windows.Forms.TextBox()
        Me.Label46 = New System.Windows.Forms.Label()
        Me.Label47 = New System.Windows.Forms.Label()
        Me.Txt_PlcIP = New System.Windows.Forms.TextBox()
        Me.GroupBox5 = New System.Windows.Forms.GroupBox()
        Me.CmbIORecom = New System.Windows.Forms.ComboBox()
        Me.Label26 = New System.Windows.Forms.Label()
        Me.Label27 = New System.Windows.Forms.Label()
        Me.cmbIORerate = New System.Windows.Forms.ComboBox()
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.btn34461ConnectTest = New System.Windows.Forms.Button()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.txt34461 = New System.Windows.Forms.TextBox()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.btnScmConnectTest = New System.Windows.Forms.Button()
        Me.ChbScmCom = New System.Windows.Forms.CheckBox()
        Me.ChbSCMIP = New System.Windows.Forms.CheckBox()
        Me.cmb_SCMRate = New System.Windows.Forms.ComboBox()
        Me.lbl_SCMRate = New System.Windows.Forms.Label()
        Me.cmb_SCMPort = New System.Windows.Forms.ComboBox()
        Me.lbl_SCMPort = New System.Windows.Forms.Label()
        Me.txt_ScmPort = New System.Windows.Forms.TextBox()
        Me.Label20 = New System.Windows.Forms.Label()
        Me.Label21 = New System.Windows.Forms.Label()
        Me.Txt_ScmIP = New System.Windows.Forms.TextBox()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.btnMain3562ConnectTest = New System.Windows.Forms.Button()
        Me.Txt_DelayTime = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.CmbRecom = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.ComRate = New System.Windows.Forms.ComboBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.cmbRerate = New System.Windows.Forms.ComboBox()
        Me.GroupBox12 = New System.Windows.Forms.GroupBox()
        Me.btnTemperatureConnectTest = New System.Windows.Forms.Button()
        Me.cmb_TemPort = New System.Windows.Forms.ComboBox()
        Me.Label51 = New System.Windows.Forms.Label()
        Me.Label53 = New System.Windows.Forms.Label()
        Me.cmb_TempRate = New System.Windows.Forms.ComboBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.btnScannerConnectTest = New System.Windows.Forms.Button()
        Me.cmb_ScanerPort = New System.Windows.Forms.ComboBox()
        Me.Label22 = New System.Windows.Forms.Label()
        Me.Label23 = New System.Windows.Forms.Label()
        Me.cmb_ScanerRate = New System.Windows.Forms.ComboBox()
        Me.GroupBox10 = New System.Windows.Forms.GroupBox()
        Me.btnRetestConnect = New System.Windows.Forms.Button()
        Me.cmbRetestRate = New System.Windows.Forms.ComboBox()
        Me.cmbRetestPort = New System.Windows.Forms.ComboBox()
        Me.Label49 = New System.Windows.Forms.Label()
        Me.Label48 = New System.Windows.Forms.Label()
        Me.TabPage2 = New System.Windows.Forms.TabPage()
        Me.GroupBox11 = New System.Windows.Forms.GroupBox()
        Me.Label16 = New System.Windows.Forms.Label()
        Me.Label28 = New System.Windows.Forms.Label()
        Me.Label17 = New System.Windows.Forms.Label()
        Me.txtProbeUsed = New System.Windows.Forms.TextBox()
        Me.Label41 = New System.Windows.Forms.Label()
        Me.txtProbelifespan = New System.Windows.Forms.TextBox()
        Me.Label29 = New System.Windows.Forms.Label()
        Me.txtCailiModelValue = New System.Windows.Forms.TextBox()
        Me.txt_V_B = New System.Windows.Forms.TextBox()
        Me.txtCodeLenth = New System.Windows.Forms.TextBox()
        Me.Label36 = New System.Windows.Forms.Label()
        Me.Label31 = New System.Windows.Forms.Label()
        Me.txt_R_B = New System.Windows.Forms.TextBox()
        Me.Label37 = New System.Windows.Forms.Label()
        Me.Label40 = New System.Windows.Forms.Label()
        Me.Label24 = New System.Windows.Forms.Label()
        Me.Label30 = New System.Windows.Forms.Label()
        Me.Label25 = New System.Windows.Forms.Label()
        Me.txtOverTemperate = New System.Windows.Forms.TextBox()
        Me.GroupBox8 = New System.Windows.Forms.GroupBox()
        Me.radioOCV0Set = New System.Windows.Forms.RadioButton()
        Me.radioOCV1Set = New System.Windows.Forms.RadioButton()
        Me.radioOCV2Set = New System.Windows.Forms.RadioButton()
        Me.GroupBox6 = New System.Windows.Forms.GroupBox()
        Me.Label39 = New System.Windows.Forms.Label()
        Me.Label38 = New System.Windows.Forms.Label()
        Me.txtVol1 = New System.Windows.Forms.TextBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.txtRes1 = New System.Windows.Forms.TextBox()
        Me.txtRes2 = New System.Windows.Forms.TextBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.txtVol2 = New System.Windows.Forms.TextBox()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.btnImportTestStandard = New System.Windows.Forms.Button()
        Me.btnSaveTestStandard = New System.Windows.Forms.Button()
        Me.txtPath = New System.Windows.Forms.TextBox()
        Me.Label15 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.TabPage3 = New System.Windows.Forms.TabPage()
        Me.GB_MesDataBase = New System.Windows.Forms.GroupBox()
        Me.TxtMesDatabaseName = New System.Windows.Forms.TextBox()
        Me.TxtMesBaseUser = New System.Windows.Forms.TextBox()
        Me.TxtMesBasePsw = New System.Windows.Forms.TextBox()
        Me.Label42 = New System.Windows.Forms.Label()
        Me.Label43 = New System.Windows.Forms.Label()
        Me.Label52 = New System.Windows.Forms.Label()
        Me.txtMesDatabaseServerIP = New System.Windows.Forms.TextBox()
        Me.Label50 = New System.Windows.Forms.Label()
        Me.GroupBox7 = New System.Windows.Forms.GroupBox()
        Me.txtUser = New System.Windows.Forms.TextBox()
        Me.txtpwd = New System.Windows.Forms.TextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label19 = New System.Windows.Forms.Label()
        Me.txtDatabase = New System.Windows.Forms.TextBox()
        Me.Label18 = New System.Windows.Forms.Label()
        Me.CheckBox1 = New System.Windows.Forms.CheckBox()
        Me.chkAuto = New System.Windows.Forms.CheckBox()
        Me.txtMesApiAddress = New System.Windows.Forms.TextBox()
        Me.Label35 = New System.Windows.Forms.Label()
        Me.TabScmSet = New System.Windows.Forms.TabPage()
        Me.lblScmStatus = New System.Windows.Forms.Label()
        Me.btnScmApply = New System.Windows.Forms.Button()
        Me.btnScmRead = New System.Windows.Forms.Button()
        Me.lblScmPhaseGuardUnit = New System.Windows.Forms.Label()
        Me.txtScmPhaseGuardMs = New System.Windows.Forms.TextBox()
        Me.lblScmPhaseGuardMs = New System.Windows.Forms.Label()
        Me.lblScmTriggerUnit = New System.Windows.Forms.Label()
        Me.txtScmTriggerPulseMs = New System.Windows.Forms.TextBox()
        Me.lblScmTriggerPulseMs = New System.Windows.Forms.Label()
        Me.lblScmAutoStepUnit = New System.Windows.Forms.Label()
        Me.txtScmAutoStepMs = New System.Windows.Forms.TextBox()
        Me.lblScmAutoStepMs = New System.Windows.Forms.Label()
        Me.lblScmChannelUnit = New System.Windows.Forms.Label()
        Me.txtScmChannelSettleMs = New System.Windows.Forms.TextBox()
        Me.lblScmChannelSettleMs = New System.Windows.Forms.Label()
        Me.lblScmRelayUnit = New System.Windows.Forms.Label()
        Me.txtScmRelaySettleMs = New System.Windows.Forms.TextBox()
        Me.lblScmRelaySettleMs = New System.Windows.Forms.Label()
        Me.TabPageChannelColors = New System.Windows.Forms.TabPage()
        Me.GroupBoxChannelColors = New System.Windows.Forms.GroupBox()
        Me.btnResetChannelColors = New System.Windows.Forms.Button()
        Me.btnVoltageNgColor = New System.Windows.Forms.Button()
        Me.txtVoltageNgColor = New System.Windows.Forms.TextBox()
        Me.pnlVoltageNgColor = New System.Windows.Forms.Panel()
        Me.lblVoltageNgColor = New System.Windows.Forms.Label()
        Me.btnSelectedColor = New System.Windows.Forms.Button()
        Me.txtSelectedColor = New System.Windows.Forms.TextBox()
        Me.pnlSelectedColor = New System.Windows.Forms.Panel()
        Me.lblSelectedColor = New System.Windows.Forms.Label()
        Me.btnResistanceNgColor = New System.Windows.Forms.Button()
        Me.txtResistanceNgColor = New System.Windows.Forms.TextBox()
        Me.pnlResistanceNgColor = New System.Windows.Forms.Panel()
        Me.lblResistanceNgColor = New System.Windows.Forms.Label()
        Me.btnIdleColor = New System.Windows.Forms.Button()
        Me.txtIdleColor = New System.Windows.Forms.TextBox()
        Me.pnlIdleColor = New System.Windows.Forms.Panel()
        Me.lblIdleColor = New System.Windows.Forms.Label()
        Me.btnTestingColor = New System.Windows.Forms.Button()
        Me.txtTestingColor = New System.Windows.Forms.TextBox()
        Me.pnlTestingColor = New System.Windows.Forms.Panel()
        Me.lblTestingColor = New System.Windows.Forms.Label()
        Me.btnPassColor = New System.Windows.Forms.Button()
        Me.txtPassColor = New System.Windows.Forms.TextBox()
        Me.pnlPassColor = New System.Windows.Forms.Panel()
        Me.lblPassColor = New System.Windows.Forms.Label()
        Me.lblChannelColorHint = New System.Windows.Forms.Label()
        Me.TabPageInstrumentSettings = New System.Windows.Forms.TabPage()
        Me.GroupBoxInstrument34461 = New System.Windows.Forms.GroupBox()
        Me.lbl34461Range = New System.Windows.Forms.Label()
        Me.lbl34461Nplc = New System.Windows.Forms.Label()
        Me.lbl34461AutoZero = New System.Windows.Forms.Label()
        Me.cmb34461Range = New System.Windows.Forms.ComboBox()
        Me.cmb34461Nplc = New System.Windows.Forms.ComboBox()
        Me.cmb34461AutoZero = New System.Windows.Forms.ComboBox()
        Me.GroupBoxInstrumentMain3562 = New System.Windows.Forms.GroupBox()
        Me.lblMain3562Function = New System.Windows.Forms.Label()
        Me.lblMain3562ResistanceRange = New System.Windows.Forms.Label()
        Me.lblMain3562VoltageRange = New System.Windows.Forms.Label()
        Me.lblMain3562SampleRate = New System.Windows.Forms.Label()
        Me.cmbMain3562Function = New System.Windows.Forms.ComboBox()
        Me.cmbMain3562ResistanceRange = New System.Windows.Forms.ComboBox()
        Me.cmbMain3562VoltageRange = New System.Windows.Forms.ComboBox()
        Me.cmbMain3562SampleRate = New System.Windows.Forms.ComboBox()
        Me.GroupBoxInstrumentRetest3562 = New System.Windows.Forms.GroupBox()
        Me.lblRetest3562Function = New System.Windows.Forms.Label()
        Me.lblRetest3562ResistanceRange = New System.Windows.Forms.Label()
        Me.lblRetest3562VoltageRange = New System.Windows.Forms.Label()
        Me.lblRetest3562SampleRate = New System.Windows.Forms.Label()
        Me.cmbRetest3562Function = New System.Windows.Forms.ComboBox()
        Me.cmbRetest3562ResistanceRange = New System.Windows.Forms.ComboBox()
        Me.cmbRetest3562VoltageRange = New System.Windows.Forms.ComboBox()
        Me.cmbRetest3562SampleRate = New System.Windows.Forms.ComboBox()
        Me.lblInstrumentSettingsNote = New System.Windows.Forms.Label()
        Me.colorDialogChannel = New System.Windows.Forms.ColorDialog()
        Me.CmdOK = New System.Windows.Forms.Button()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.GroupBox13 = New System.Windows.Forms.GroupBox()
        Me.ComboBox1 = New System.Windows.Forms.ComboBox()
        Me.Label54 = New System.Windows.Forms.Label()
        Me.Label55 = New System.Windows.Forms.Label()
        Me.ComboBox2 = New System.Windows.Forms.ComboBox()
        Me.TabControl1.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.GroupBox9.SuspendLayout()
        Me.GroupBox5.SuspendLayout()
        Me.GroupBox4.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox12.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox10.SuspendLayout()
        Me.TabPage2.SuspendLayout()
        Me.GroupBox11.SuspendLayout()
        Me.GroupBox8.SuspendLayout()
        Me.GroupBox6.SuspendLayout()
        Me.TabPage3.SuspendLayout()
        Me.GB_MesDataBase.SuspendLayout()
        Me.GroupBox7.SuspendLayout()
        Me.TabScmSet.SuspendLayout()
        Me.TabPageChannelColors.SuspendLayout()
        Me.GroupBoxChannelColors.SuspendLayout()
        Me.TabPageInstrumentSettings.SuspendLayout()
        Me.GroupBoxInstrument34461.SuspendLayout()
        Me.GroupBoxInstrumentMain3562.SuspendLayout()
        Me.GroupBoxInstrumentRetest3562.SuspendLayout()
        Me.GroupBox13.SuspendLayout()
        Me.SuspendLayout()
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPage1)
        Me.TabControl1.Controls.Add(Me.TabPage2)
        Me.TabControl1.Controls.Add(Me.TabPage3)
        Me.TabControl1.Controls.Add(Me.TabScmSet)
        Me.TabControl1.Controls.Add(Me.TabPageChannelColors)
        Me.TabControl1.Controls.Add(Me.TabPageInstrumentSettings)
        Me.TabControl1.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.TabControl1.HotTrack = True
        Me.TabControl1.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.TabControl1.Location = New System.Drawing.Point(12, 12)
        Me.TabControl1.Multiline = True
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(538, 603)
        Me.TabControl1.TabIndex = 0
        '
        'TabPage1
        '
        Me.TabPage1.BackColor = System.Drawing.Color.LightBlue
        Me.TabPage1.Controls.Add(Me.GroupBox9)
        Me.TabPage1.Controls.Add(Me.GroupBox5)
        Me.TabPage1.Controls.Add(Me.GroupBox4)
        Me.TabPage1.Controls.Add(Me.GroupBox3)
        Me.TabPage1.Controls.Add(Me.GroupBox2)
        Me.TabPage1.Controls.Add(Me.GroupBox12)
        Me.TabPage1.Controls.Add(Me.GroupBox1)
        Me.TabPage1.Controls.Add(Me.GroupBox10)
        Me.TabPage1.Location = New System.Drawing.Point(4, 24)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(530, 575)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "通讯设置"
        '
        'GroupBox9
        '
        Me.GroupBox9.BackColor = System.Drawing.Color.SkyBlue
        Me.GroupBox9.Controls.Add(Me.btnPlcConnectTest)
        Me.GroupBox9.Controls.Add(Me.ChbPlcCom)
        Me.GroupBox9.Controls.Add(Me.ChbPLCIP)
        Me.GroupBox9.Controls.Add(Me.cmb_PlcRate)
        Me.GroupBox9.Controls.Add(Me.Label44)
        Me.GroupBox9.Controls.Add(Me.cmb_PlcPort)
        Me.GroupBox9.Controls.Add(Me.Label45)
        Me.GroupBox9.Controls.Add(Me.txt_PlcPort)
        Me.GroupBox9.Controls.Add(Me.Label46)
        Me.GroupBox9.Controls.Add(Me.Label47)
        Me.GroupBox9.Controls.Add(Me.Txt_PlcIP)
        Me.GroupBox9.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.GroupBox9.Location = New System.Drawing.Point(22, 113)
        Me.GroupBox9.Name = "GroupBox9"
        Me.GroupBox9.Size = New System.Drawing.Size(487, 76)
        Me.GroupBox9.TabIndex = 17
        Me.GroupBox9.TabStop = False
        Me.GroupBox9.Text = "PLC"
        '
        'btnPlcConnectTest
        '
        Me.btnPlcConnectTest.Location = New System.Drawing.Point(390, 20)
        Me.btnPlcConnectTest.Name = "btnPlcConnectTest"
        Me.btnPlcConnectTest.Size = New System.Drawing.Size(82, 25)
        Me.btnPlcConnectTest.TabIndex = 18
        Me.btnPlcConnectTest.Text = "测试连接"
        Me.btnPlcConnectTest.UseVisualStyleBackColor = True
        '
        'ChbPlcCom
        '
        Me.ChbPlcCom.AutoSize = True
        Me.ChbPlcCom.Checked = True
        Me.ChbPlcCom.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ChbPlcCom.Location = New System.Drawing.Point(251, 49)
        Me.ChbPlcCom.Name = "ChbPlcCom"
        Me.ChbPlcCom.Size = New System.Drawing.Size(75, 18)
        Me.ChbPlcCom.TabIndex = 17
        Me.ChbPlcCom.Text = "Com接口"
        Me.ChbPlcCom.UseVisualStyleBackColor = True
        '
        'ChbPLCIP
        '
        Me.ChbPLCIP.AutoSize = True
        Me.ChbPLCIP.Location = New System.Drawing.Point(83, 47)
        Me.ChbPLCIP.Name = "ChbPLCIP"
        Me.ChbPLCIP.Size = New System.Drawing.Size(75, 18)
        Me.ChbPLCIP.TabIndex = 17
        Me.ChbPLCIP.Text = "Tcp接口"
        Me.ChbPLCIP.UseVisualStyleBackColor = True
        '
        'cmb_PlcRate
        '
        Me.cmb_PlcRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmb_PlcRate.FormattingEnabled = True
        Me.cmb_PlcRate.Location = New System.Drawing.Point(293, 21)
        Me.cmb_PlcRate.Name = "cmb_PlcRate"
        Me.cmb_PlcRate.Size = New System.Drawing.Size(90, 22)
        Me.cmb_PlcRate.TabIndex = 16
        '
        'Label44
        '
        Me.Label44.AutoSize = True
        Me.Label44.Location = New System.Drawing.Point(238, 27)
        Me.Label44.Name = "Label44"
        Me.Label44.Size = New System.Drawing.Size(49, 14)
        Me.Label44.TabIndex = 15
        Me.Label44.Text = "波特率"
        '
        'cmb_PlcPort
        '
        Me.cmb_PlcPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmb_PlcPort.FormattingEnabled = True
        Me.cmb_PlcPort.Location = New System.Drawing.Point(79, 19)
        Me.cmb_PlcPort.Name = "cmb_PlcPort"
        Me.cmb_PlcPort.Size = New System.Drawing.Size(100, 22)
        Me.cmb_PlcPort.TabIndex = 14
        '
        'Label45
        '
        Me.Label45.AutoSize = True
        Me.Label45.Location = New System.Drawing.Point(25, 25)
        Me.Label45.Name = "Label45"
        Me.Label45.Size = New System.Drawing.Size(49, 14)
        Me.Label45.TabIndex = 13
        Me.Label45.Text = "串口号"
        '
        'txt_PlcPort
        '
        Me.txt_PlcPort.Location = New System.Drawing.Point(292, 21)
        Me.txt_PlcPort.Name = "txt_PlcPort"
        Me.txt_PlcPort.Size = New System.Drawing.Size(90, 23)
        Me.txt_PlcPort.TabIndex = 12
        Me.txt_PlcPort.Visible = False
        '
        'Label46
        '
        Me.Label46.AutoSize = True
        Me.Label46.Location = New System.Drawing.Point(26, 25)
        Me.Label46.Name = "Label46"
        Me.Label46.Size = New System.Drawing.Size(49, 14)
        Me.Label46.TabIndex = 10
        Me.Label46.Text = "Ip地址"
        Me.Label46.Visible = False
        '
        'Label47
        '
        Me.Label47.AutoSize = True
        Me.Label47.Location = New System.Drawing.Point(237, 27)
        Me.Label47.Name = "Label47"
        Me.Label47.Size = New System.Drawing.Size(49, 14)
        Me.Label47.TabIndex = 10
        Me.Label47.Text = "端口号"
        Me.Label47.Visible = False
        '
        'Txt_PlcIP
        '
        Me.Txt_PlcIP.Location = New System.Drawing.Point(80, 19)
        Me.Txt_PlcIP.Name = "Txt_PlcIP"
        Me.Txt_PlcIP.Size = New System.Drawing.Size(100, 23)
        Me.Txt_PlcIP.TabIndex = 11
        Me.Txt_PlcIP.Visible = False
        '
        'GroupBox5
        '
        Me.GroupBox5.BackColor = System.Drawing.Color.Cyan
        Me.GroupBox5.Controls.Add(Me.CmbIORecom)
        Me.GroupBox5.Controls.Add(Me.Label26)
        Me.GroupBox5.Controls.Add(Me.Label27)
        Me.GroupBox5.Controls.Add(Me.cmbIORerate)
        Me.GroupBox5.Location = New System.Drawing.Point(372, 211)
        Me.GroupBox5.Name = "GroupBox5"
        Me.GroupBox5.Size = New System.Drawing.Size(10, 13)
        Me.GroupBox5.TabIndex = 17
        Me.GroupBox5.TabStop = False
        Me.GroupBox5.Text = "IO控制卡通讯参数"
        '
        'CmbIORecom
        '
        Me.CmbIORecom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CmbIORecom.FormattingEnabled = True
        Me.CmbIORecom.Location = New System.Drawing.Point(80, 21)
        Me.CmbIORecom.Name = "CmbIORecom"
        Me.CmbIORecom.Size = New System.Drawing.Size(90, 22)
        Me.CmbIORecom.TabIndex = 8
        '
        'Label26
        '
        Me.Label26.AutoSize = True
        Me.Label26.Location = New System.Drawing.Point(13, 25)
        Me.Label26.Name = "Label26"
        Me.Label26.Size = New System.Drawing.Size(63, 14)
        Me.Label26.TabIndex = 6
        Me.Label26.Text = "通讯端口"
        '
        'Label27
        '
        Me.Label27.AutoSize = True
        Me.Label27.Location = New System.Drawing.Point(195, 25)
        Me.Label27.Name = "Label27"
        Me.Label27.Size = New System.Drawing.Size(49, 14)
        Me.Label27.TabIndex = 7
        Me.Label27.Text = "波特率"
        '
        'cmbIORerate
        '
        Me.cmbIORerate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbIORerate.FormattingEnabled = True
        Me.cmbIORerate.Items.AddRange(New Object() {"9600", "19200", "38400", "115200"})
        Me.cmbIORerate.Location = New System.Drawing.Point(251, 21)
        Me.cmbIORerate.Name = "cmbIORerate"
        Me.cmbIORerate.Size = New System.Drawing.Size(90, 22)
        Me.cmbIORerate.TabIndex = 9
        '
        'GroupBox4
        '
        Me.GroupBox4.BackColor = System.Drawing.Color.SkyBlue
        Me.GroupBox4.Controls.Add(Me.btn34461ConnectTest)
        Me.GroupBox4.Controls.Add(Me.Label4)
        Me.GroupBox4.Controls.Add(Me.GroupBox13)
        Me.GroupBox4.Controls.Add(Me.txt34461)
        Me.GroupBox4.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.GroupBox4.Location = New System.Drawing.Point(22, 197)
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.Size = New System.Drawing.Size(487, 67)
        Me.GroupBox4.TabIndex = 16
        Me.GroupBox4.TabStop = False
        Me.GroupBox4.Text = "34461A"
        '
        'btn34461ConnectTest
        '
        Me.btn34461ConnectTest.Location = New System.Drawing.Point(390, 27)
        Me.btn34461ConnectTest.Name = "btn34461ConnectTest"
        Me.btn34461ConnectTest.Size = New System.Drawing.Size(82, 25)
        Me.btn34461ConnectTest.TabIndex = 12
        Me.btn34461ConnectTest.Text = "测试连接"
        Me.btn34461ConnectTest.UseVisualStyleBackColor = True
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label4.Location = New System.Drawing.Point(7, 32)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(65, 12)
        Me.Label4.TabIndex = 10
        Me.Label4.Text = "Ip相关参数"
        '
        'txt34461
        '
        Me.txt34461.Location = New System.Drawing.Point(80, 27)
        Me.txt34461.Name = "txt34461"
        Me.txt34461.Size = New System.Drawing.Size(303, 23)
        Me.txt34461.TabIndex = 11
        Me.txt34461.Text = "TCPIP0::192.168.66.81::inst0::INSTR"
        '
        'GroupBox3
        '
        Me.GroupBox3.BackColor = System.Drawing.Color.SkyBlue
        Me.GroupBox3.Controls.Add(Me.btnScmConnectTest)
        Me.GroupBox3.Controls.Add(Me.ChbScmCom)
        Me.GroupBox3.Controls.Add(Me.ChbSCMIP)
        Me.GroupBox3.Controls.Add(Me.cmb_SCMRate)
        Me.GroupBox3.Controls.Add(Me.lbl_SCMRate)
        Me.GroupBox3.Controls.Add(Me.cmb_SCMPort)
        Me.GroupBox3.Controls.Add(Me.lbl_SCMPort)
        Me.GroupBox3.Controls.Add(Me.txt_ScmPort)
        Me.GroupBox3.Controls.Add(Me.Label20)
        Me.GroupBox3.Controls.Add(Me.Label21)
        Me.GroupBox3.Controls.Add(Me.Txt_ScmIP)
        Me.GroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.GroupBox3.Location = New System.Drawing.Point(22, 23)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(487, 82)
        Me.GroupBox3.TabIndex = 15
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "中位机（SCM）"
        '
        'btnScmConnectTest
        '
        Me.btnScmConnectTest.Location = New System.Drawing.Point(390, 20)
        Me.btnScmConnectTest.Name = "btnScmConnectTest"
        Me.btnScmConnectTest.Size = New System.Drawing.Size(82, 25)
        Me.btnScmConnectTest.TabIndex = 18
        Me.btnScmConnectTest.Text = "测试连接"
        Me.btnScmConnectTest.UseVisualStyleBackColor = True
        '
        'ChbScmCom
        '
        Me.ChbScmCom.AutoSize = True
        Me.ChbScmCom.Checked = True
        Me.ChbScmCom.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ChbScmCom.Location = New System.Drawing.Point(252, 55)
        Me.ChbScmCom.Name = "ChbScmCom"
        Me.ChbScmCom.Size = New System.Drawing.Size(75, 18)
        Me.ChbScmCom.TabIndex = 17
        Me.ChbScmCom.Text = "Com接口"
        Me.ChbScmCom.UseVisualStyleBackColor = True
        '
        'ChbSCMIP
        '
        Me.ChbSCMIP.AutoSize = True
        Me.ChbSCMIP.Location = New System.Drawing.Point(80, 55)
        Me.ChbSCMIP.Name = "ChbSCMIP"
        Me.ChbSCMIP.Size = New System.Drawing.Size(75, 18)
        Me.ChbSCMIP.TabIndex = 17
        Me.ChbSCMIP.Text = "Tcp接口"
        Me.ChbSCMIP.UseVisualStyleBackColor = True
        '
        'cmb_SCMRate
        '
        Me.cmb_SCMRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmb_SCMRate.FormattingEnabled = True
        Me.cmb_SCMRate.Location = New System.Drawing.Point(293, 19)
        Me.cmb_SCMRate.Name = "cmb_SCMRate"
        Me.cmb_SCMRate.Size = New System.Drawing.Size(90, 22)
        Me.cmb_SCMRate.TabIndex = 16
        '
        'lbl_SCMRate
        '
        Me.lbl_SCMRate.AutoSize = True
        Me.lbl_SCMRate.Location = New System.Drawing.Point(238, 24)
        Me.lbl_SCMRate.Name = "lbl_SCMRate"
        Me.lbl_SCMRate.Size = New System.Drawing.Size(49, 14)
        Me.lbl_SCMRate.TabIndex = 15
        Me.lbl_SCMRate.Text = "波特率"
        '
        'cmb_SCMPort
        '
        Me.cmb_SCMPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmb_SCMPort.FormattingEnabled = True
        Me.cmb_SCMPort.Location = New System.Drawing.Point(80, 19)
        Me.cmb_SCMPort.Name = "cmb_SCMPort"
        Me.cmb_SCMPort.Size = New System.Drawing.Size(100, 22)
        Me.cmb_SCMPort.TabIndex = 14
        '
        'lbl_SCMPort
        '
        Me.lbl_SCMPort.AutoSize = True
        Me.lbl_SCMPort.Location = New System.Drawing.Point(24, 24)
        Me.lbl_SCMPort.Name = "lbl_SCMPort"
        Me.lbl_SCMPort.Size = New System.Drawing.Size(49, 14)
        Me.lbl_SCMPort.TabIndex = 13
        Me.lbl_SCMPort.Text = "串口号"
        '
        'txt_ScmPort
        '
        Me.txt_ScmPort.Location = New System.Drawing.Point(293, 19)
        Me.txt_ScmPort.Name = "txt_ScmPort"
        Me.txt_ScmPort.Size = New System.Drawing.Size(90, 23)
        Me.txt_ScmPort.TabIndex = 12
        Me.txt_ScmPort.Visible = False
        '
        'Label20
        '
        Me.Label20.AutoSize = True
        Me.Label20.Location = New System.Drawing.Point(24, 24)
        Me.Label20.Name = "Label20"
        Me.Label20.Size = New System.Drawing.Size(49, 14)
        Me.Label20.TabIndex = 10
        Me.Label20.Text = "Ip地址"
        Me.Label20.Visible = False
        '
        'Label21
        '
        Me.Label21.AutoSize = True
        Me.Label21.Location = New System.Drawing.Point(238, 24)
        Me.Label21.Name = "Label21"
        Me.Label21.Size = New System.Drawing.Size(49, 14)
        Me.Label21.TabIndex = 10
        Me.Label21.Text = "端口号"
        Me.Label21.Visible = False
        '
        'Txt_ScmIP
        '
        Me.Txt_ScmIP.Location = New System.Drawing.Point(80, 19)
        Me.Txt_ScmIP.Name = "Txt_ScmIP"
        Me.Txt_ScmIP.Size = New System.Drawing.Size(100, 23)
        Me.Txt_ScmIP.TabIndex = 11
        Me.Txt_ScmIP.Visible = False
        '
        'GroupBox2
        '
        Me.GroupBox2.BackColor = System.Drawing.Color.SkyBlue
        Me.GroupBox2.Controls.Add(Me.btnMain3562ConnectTest)
        Me.GroupBox2.Controls.Add(Me.Txt_DelayTime)
        Me.GroupBox2.Controls.Add(Me.Label1)
        Me.GroupBox2.Controls.Add(Me.Label3)
        Me.GroupBox2.Controls.Add(Me.CmbRecom)
        Me.GroupBox2.Controls.Add(Me.Label2)
        Me.GroupBox2.Controls.Add(Me.ComRate)
        Me.GroupBox2.Controls.Add(Me.Label5)
        Me.GroupBox2.Controls.Add(Me.Label6)
        Me.GroupBox2.Controls.Add(Me.cmbRerate)
        Me.GroupBox2.Location = New System.Drawing.Point(22, 272)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(487, 88)
        Me.GroupBox2.TabIndex = 14
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "主3562"
        '
        'btnMain3562ConnectTest
        '
        Me.btnMain3562ConnectTest.Location = New System.Drawing.Point(390, 18)
        Me.btnMain3562ConnectTest.Name = "btnMain3562ConnectTest"
        Me.btnMain3562ConnectTest.Size = New System.Drawing.Size(82, 25)
        Me.btnMain3562ConnectTest.TabIndex = 20
        Me.btnMain3562ConnectTest.Text = "测试连接"
        Me.btnMain3562ConnectTest.UseVisualStyleBackColor = True
        '
        'Txt_DelayTime
        '
        Me.Txt_DelayTime.Location = New System.Drawing.Point(293, 51)
        Me.Txt_DelayTime.Name = "Txt_DelayTime"
        Me.Txt_DelayTime.Size = New System.Drawing.Size(90, 23)
        Me.Txt_DelayTime.TabIndex = 19
        Me.Txt_DelayTime.Text = "100"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(224, 56)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(63, 14)
        Me.Label1.TabIndex = 18
        Me.Label1.Text = "选通时间"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(387, 56)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(21, 14)
        Me.Label3.TabIndex = 17
        Me.Label3.Text = "mS"
        '
        'CmbRecom
        '
        Me.CmbRecom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CmbRecom.FormattingEnabled = True
        Me.CmbRecom.Location = New System.Drawing.Point(80, 19)
        Me.CmbRecom.Name = "CmbRecom"
        Me.CmbRecom.Size = New System.Drawing.Size(100, 22)
        Me.CmbRecom.TabIndex = 8
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(5, 56)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(63, 14)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "速率模式"
        '
        'ComRate
        '
        Me.ComRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComRate.FormattingEnabled = True
        Me.ComRate.Location = New System.Drawing.Point(80, 52)
        Me.ComRate.Name = "ComRate"
        Me.ComRate.Size = New System.Drawing.Size(100, 22)
        Me.ComRate.TabIndex = 2
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(9, 23)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(63, 14)
        Me.Label5.TabIndex = 6
        Me.Label5.Text = "通讯端口"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(238, 23)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(49, 14)
        Me.Label6.TabIndex = 7
        Me.Label6.Text = "波特率"
        '
        'cmbRerate
        '
        Me.cmbRerate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbRerate.FormattingEnabled = True
        Me.cmbRerate.Location = New System.Drawing.Point(293, 19)
        Me.cmbRerate.Name = "cmbRerate"
        Me.cmbRerate.Size = New System.Drawing.Size(90, 22)
        Me.cmbRerate.TabIndex = 9
        '
        'GroupBox12
        '
        Me.GroupBox12.BackColor = System.Drawing.Color.SkyBlue
        Me.GroupBox12.Controls.Add(Me.btnTemperatureConnectTest)
        Me.GroupBox12.Controls.Add(Me.cmb_TemPort)
        Me.GroupBox12.Controls.Add(Me.Label51)
        Me.GroupBox12.Controls.Add(Me.Label53)
        Me.GroupBox12.Controls.Add(Me.cmb_TempRate)
        Me.GroupBox12.Location = New System.Drawing.Point(22, 500)
        Me.GroupBox12.Name = "GroupBox12"
        Me.GroupBox12.Size = New System.Drawing.Size(487, 59)
        Me.GroupBox12.TabIndex = 13
        Me.GroupBox12.TabStop = False
        Me.GroupBox12.Text = "温度采集模块"
        '
        'btnTemperatureConnectTest
        '
        Me.btnTemperatureConnectTest.Location = New System.Drawing.Point(390, 20)
        Me.btnTemperatureConnectTest.Name = "btnTemperatureConnectTest"
        Me.btnTemperatureConnectTest.Size = New System.Drawing.Size(82, 25)
        Me.btnTemperatureConnectTest.TabIndex = 10
        Me.btnTemperatureConnectTest.Text = "测试连接"
        Me.btnTemperatureConnectTest.UseVisualStyleBackColor = True
        '
        'cmb_TemPort
        '
        Me.cmb_TemPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmb_TemPort.FormattingEnabled = True
        Me.cmb_TemPort.Location = New System.Drawing.Point(79, 21)
        Me.cmb_TemPort.Name = "cmb_TemPort"
        Me.cmb_TemPort.Size = New System.Drawing.Size(100, 22)
        Me.cmb_TemPort.TabIndex = 8
        '
        'Label51
        '
        Me.Label51.AutoSize = True
        Me.Label51.Location = New System.Drawing.Point(10, 25)
        Me.Label51.Name = "Label51"
        Me.Label51.Size = New System.Drawing.Size(63, 14)
        Me.Label51.TabIndex = 6
        Me.Label51.Text = "通讯端口"
        '
        'Label53
        '
        Me.Label53.AutoSize = True
        Me.Label53.Location = New System.Drawing.Point(238, 25)
        Me.Label53.Name = "Label53"
        Me.Label53.Size = New System.Drawing.Size(49, 14)
        Me.Label53.TabIndex = 7
        Me.Label53.Text = "波特率"
        '
        'cmb_TempRate
        '
        Me.cmb_TempRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmb_TempRate.FormattingEnabled = True
        Me.cmb_TempRate.Items.AddRange(New Object() {"9600", "19200", "38400"})
        Me.cmb_TempRate.Location = New System.Drawing.Point(293, 21)
        Me.cmb_TempRate.Name = "cmb_TempRate"
        Me.cmb_TempRate.Size = New System.Drawing.Size(90, 22)
        Me.cmb_TempRate.TabIndex = 9
        '
        'GroupBox1
        '
        Me.GroupBox1.BackColor = System.Drawing.Color.SkyBlue
        Me.GroupBox1.Controls.Add(Me.btnScannerConnectTest)
        Me.GroupBox1.Controls.Add(Me.cmb_ScanerPort)
        Me.GroupBox1.Controls.Add(Me.Label22)
        Me.GroupBox1.Controls.Add(Me.Label23)
        Me.GroupBox1.Controls.Add(Me.cmb_ScanerRate)
        Me.GroupBox1.Location = New System.Drawing.Point(22, 433)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(487, 59)
        Me.GroupBox1.TabIndex = 13
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "扫描枪"
        '
        'btnScannerConnectTest
        '
        Me.btnScannerConnectTest.Location = New System.Drawing.Point(390, 20)
        Me.btnScannerConnectTest.Name = "btnScannerConnectTest"
        Me.btnScannerConnectTest.Size = New System.Drawing.Size(82, 25)
        Me.btnScannerConnectTest.TabIndex = 10
        Me.btnScannerConnectTest.Text = "测试连接"
        Me.btnScannerConnectTest.UseVisualStyleBackColor = True
        '
        'cmb_ScanerPort
        '
        Me.cmb_ScanerPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmb_ScanerPort.FormattingEnabled = True
        Me.cmb_ScanerPort.Location = New System.Drawing.Point(79, 21)
        Me.cmb_ScanerPort.Name = "cmb_ScanerPort"
        Me.cmb_ScanerPort.Size = New System.Drawing.Size(100, 22)
        Me.cmb_ScanerPort.TabIndex = 8
        '
        'Label22
        '
        Me.Label22.AutoSize = True
        Me.Label22.Location = New System.Drawing.Point(10, 25)
        Me.Label22.Name = "Label22"
        Me.Label22.Size = New System.Drawing.Size(63, 14)
        Me.Label22.TabIndex = 6
        Me.Label22.Text = "通讯端口"
        '
        'Label23
        '
        Me.Label23.AutoSize = True
        Me.Label23.Location = New System.Drawing.Point(238, 25)
        Me.Label23.Name = "Label23"
        Me.Label23.Size = New System.Drawing.Size(49, 14)
        Me.Label23.TabIndex = 7
        Me.Label23.Text = "波特率"
        '
        'cmb_ScanerRate
        '
        Me.cmb_ScanerRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmb_ScanerRate.FormattingEnabled = True
        Me.cmb_ScanerRate.Location = New System.Drawing.Point(293, 21)
        Me.cmb_ScanerRate.Name = "cmb_ScanerRate"
        Me.cmb_ScanerRate.Size = New System.Drawing.Size(90, 22)
        Me.cmb_ScanerRate.TabIndex = 9
        '
        'GroupBox10
        '
        Me.GroupBox10.BackColor = System.Drawing.Color.SkyBlue
        Me.GroupBox10.Controls.Add(Me.btnRetestConnect)
        Me.GroupBox10.Controls.Add(Me.cmbRetestRate)
        Me.GroupBox10.Controls.Add(Me.cmbRetestPort)
        Me.GroupBox10.Controls.Add(Me.Label49)
        Me.GroupBox10.Controls.Add(Me.Label48)
        Me.GroupBox10.Location = New System.Drawing.Point(22, 368)
        Me.GroupBox10.Name = "GroupBox10"
        Me.GroupBox10.Size = New System.Drawing.Size(487, 57)
        Me.GroupBox10.TabIndex = 18
        Me.GroupBox10.TabStop = False
        Me.GroupBox10.Text = "NG复测3562"
        '
        'btnRetestConnect
        '
        Me.btnRetestConnect.Location = New System.Drawing.Point(409, 20)
        Me.btnRetestConnect.Name = "btnRetestConnect"
        Me.btnRetestConnect.Size = New System.Drawing.Size(72, 25)
        Me.btnRetestConnect.TabIndex = 24
        Me.btnRetestConnect.Text = "测试连接"
        Me.btnRetestConnect.UseVisualStyleBackColor = True
        '
        'cmbRetestRate
        '
        Me.cmbRetestRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbRetestRate.FormattingEnabled = True
        Me.cmbRetestRate.Location = New System.Drawing.Point(293, 22)
        Me.cmbRetestRate.Name = "cmbRetestRate"
        Me.cmbRetestRate.Size = New System.Drawing.Size(90, 22)
        Me.cmbRetestRate.TabIndex = 21
        '
        'cmbRetestPort
        '
        Me.cmbRetestPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbRetestPort.FormattingEnabled = True
        Me.cmbRetestPort.Location = New System.Drawing.Point(79, 17)
        Me.cmbRetestPort.Name = "cmbRetestPort"
        Me.cmbRetestPort.Size = New System.Drawing.Size(100, 22)
        Me.cmbRetestPort.TabIndex = 20
        '
        'Label49
        '
        Me.Label49.AutoSize = True
        Me.Label49.Location = New System.Drawing.Point(238, 25)
        Me.Label49.Name = "Label49"
        Me.Label49.Size = New System.Drawing.Size(49, 14)
        Me.Label49.TabIndex = 23
        Me.Label49.Text = "波特率"
        '
        'Label48
        '
        Me.Label48.AutoSize = True
        Me.Label48.Location = New System.Drawing.Point(8, 25)
        Me.Label48.Name = "Label48"
        Me.Label48.Size = New System.Drawing.Size(35, 14)
        Me.Label48.TabIndex = 22
        Me.Label48.Text = "串口"
        '
        'TabPage2
        '
        Me.TabPage2.BackColor = System.Drawing.Color.LightBlue
        Me.TabPage2.Controls.Add(Me.GroupBox11)
        Me.TabPage2.Controls.Add(Me.GroupBox8)
        Me.TabPage2.Controls.Add(Me.GroupBox6)
        Me.TabPage2.Controls.Add(Me.txtPath)
        Me.TabPage2.Controls.Add(Me.Label15)
        Me.TabPage2.Controls.Add(Me.Label8)
        Me.TabPage2.Location = New System.Drawing.Point(4, 24)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage2.Size = New System.Drawing.Size(530, 575)
        Me.TabPage2.TabIndex = 1
        Me.TabPage2.Text = "参数设置"
        '
        'GroupBox11
        '
        Me.GroupBox11.BackColor = System.Drawing.Color.SkyBlue
        Me.GroupBox11.Controls.Add(Me.Label16)
        Me.GroupBox11.Controls.Add(Me.Label28)
        Me.GroupBox11.Controls.Add(Me.Label17)
        Me.GroupBox11.Controls.Add(Me.txtProbeUsed)
        Me.GroupBox11.Controls.Add(Me.Label41)
        Me.GroupBox11.Controls.Add(Me.txtProbelifespan)
        Me.GroupBox11.Controls.Add(Me.Label29)
        Me.GroupBox11.Controls.Add(Me.txtCailiModelValue)
        Me.GroupBox11.Controls.Add(Me.txt_V_B)
        Me.GroupBox11.Controls.Add(Me.txtCodeLenth)
        Me.GroupBox11.Controls.Add(Me.Label36)
        Me.GroupBox11.Controls.Add(Me.Label31)
        Me.GroupBox11.Controls.Add(Me.txt_R_B)
        Me.GroupBox11.Controls.Add(Me.Label37)
        Me.GroupBox11.Controls.Add(Me.Label40)
        Me.GroupBox11.Controls.Add(Me.Label24)
        Me.GroupBox11.Controls.Add(Me.Label30)
        Me.GroupBox11.Controls.Add(Me.Label25)
        Me.GroupBox11.Controls.Add(Me.txtOverTemperate)
        Me.GroupBox11.Location = New System.Drawing.Point(15, 333)
        Me.GroupBox11.Name = "GroupBox11"
        Me.GroupBox11.Size = New System.Drawing.Size(494, 195)
        Me.GroupBox11.TabIndex = 29
        Me.GroupBox11.TabStop = False
        Me.GroupBox11.Text = "其他参数设置"
        '
        'Label16
        '
        Me.Label16.AutoSize = True
        Me.Label16.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label16.Location = New System.Drawing.Point(65, 35)
        Me.Label16.Name = "Label16"
        Me.Label16.Size = New System.Drawing.Size(56, 14)
        Me.Label16.TabIndex = 14
        Me.Label16.Text = "电压B值"
        '
        'Label28
        '
        Me.Label28.AutoSize = True
        Me.Label28.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label28.Location = New System.Drawing.Point(212, 120)
        Me.Label28.Name = "Label28"
        Me.Label28.Size = New System.Drawing.Size(21, 14)
        Me.Label28.TabIndex = 20
        Me.Label28.Text = "位"
        '
        'Label17
        '
        Me.Label17.AutoSize = True
        Me.Label17.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label17.Location = New System.Drawing.Point(268, 35)
        Me.Label17.Name = "Label17"
        Me.Label17.Size = New System.Drawing.Size(56, 14)
        Me.Label17.TabIndex = 15
        Me.Label17.Text = "内阻B值"
        '
        'txtProbeUsed
        '
        Me.txtProbeUsed.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtProbeUsed.Location = New System.Drawing.Point(332, 151)
        Me.txtProbeUsed.Name = "txtProbeUsed"
        Me.txtProbeUsed.Size = New System.Drawing.Size(75, 23)
        Me.txtProbeUsed.TabIndex = 24
        Me.txtProbeUsed.Text = "0"
        Me.txtProbeUsed.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label41
        '
        Me.Label41.AutoSize = True
        Me.Label41.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label41.Location = New System.Drawing.Point(262, 73)
        Me.Label41.Name = "Label41"
        Me.Label41.Size = New System.Drawing.Size(63, 14)
        Me.Label41.TabIndex = 15
        Me.Label41.Text = "报警温度"
        '
        'txtProbelifespan
        '
        Me.txtProbelifespan.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtProbelifespan.Location = New System.Drawing.Point(137, 151)
        Me.txtProbelifespan.Name = "txtProbelifespan"
        Me.txtProbelifespan.Size = New System.Drawing.Size(75, 23)
        Me.txtProbelifespan.TabIndex = 25
        Me.txtProbelifespan.Tag = ""
        Me.txtProbelifespan.Text = "30000"
        Me.txtProbelifespan.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label29
        '
        Me.Label29.AutoSize = True
        Me.Label29.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label29.Location = New System.Drawing.Point(212, 157)
        Me.Label29.Name = "Label29"
        Me.Label29.Size = New System.Drawing.Size(21, 14)
        Me.Label29.TabIndex = 19
        Me.Label29.Text = "次"
        '
        'txtCailiModelValue
        '
        Me.txtCailiModelValue.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtCailiModelValue.Location = New System.Drawing.Point(137, 68)
        Me.txtCailiModelValue.Name = "txtCailiModelValue"
        Me.txtCailiModelValue.Size = New System.Drawing.Size(75, 23)
        Me.txtCailiModelValue.TabIndex = 26
        Me.txtCailiModelValue.Text = "3.75"
        Me.txtCailiModelValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txt_V_B
        '
        Me.txt_V_B.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txt_V_B.Location = New System.Drawing.Point(137, 30)
        Me.txt_V_B.Name = "txt_V_B"
        Me.txt_V_B.Size = New System.Drawing.Size(75, 23)
        Me.txt_V_B.TabIndex = 16
        Me.txt_V_B.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txtCodeLenth
        '
        Me.txtCodeLenth.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtCodeLenth.Location = New System.Drawing.Point(137, 111)
        Me.txtCodeLenth.Name = "txtCodeLenth"
        Me.txtCodeLenth.Size = New System.Drawing.Size(75, 23)
        Me.txtCodeLenth.TabIndex = 26
        Me.txtCodeLenth.Text = "24"
        Me.txtCodeLenth.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label36
        '
        Me.Label36.AutoSize = True
        Me.Label36.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label36.Location = New System.Drawing.Point(211, 77)
        Me.Label36.Name = "Label36"
        Me.Label36.Size = New System.Drawing.Size(23, 12)
        Me.Label36.TabIndex = 20
        Me.Label36.Text = "mΩ"
        '
        'Label31
        '
        Me.Label31.AutoSize = True
        Me.Label31.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label31.Location = New System.Drawing.Point(262, 157)
        Me.Label31.Name = "Label31"
        Me.Label31.Size = New System.Drawing.Size(63, 14)
        Me.Label31.TabIndex = 21
        Me.Label31.Text = "使用次数"
        '
        'txt_R_B
        '
        Me.txt_R_B.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txt_R_B.Location = New System.Drawing.Point(329, 30)
        Me.txt_R_B.Name = "txt_R_B"
        Me.txt_R_B.Size = New System.Drawing.Size(75, 23)
        Me.txt_R_B.TabIndex = 17
        Me.txt_R_B.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label37
        '
        Me.Label37.AutoSize = True
        Me.Label37.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label37.Location = New System.Drawing.Point(30, 73)
        Me.Label37.Name = "Label37"
        Me.Label37.Size = New System.Drawing.Size(91, 14)
        Me.Label37.TabIndex = 23
        Me.Label37.Text = "校准模块阻值"
        '
        'Label40
        '
        Me.Label40.AutoSize = True
        Me.Label40.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label40.Location = New System.Drawing.Point(406, 73)
        Me.Label40.Name = "Label40"
        Me.Label40.Size = New System.Drawing.Size(21, 14)
        Me.Label40.TabIndex = 11
        Me.Label40.Text = "℃"
        '
        'Label24
        '
        Me.Label24.AutoSize = True
        Me.Label24.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label24.Location = New System.Drawing.Point(58, 157)
        Me.Label24.Name = "Label24"
        Me.Label24.Size = New System.Drawing.Size(63, 14)
        Me.Label24.TabIndex = 22
        Me.Label24.Text = "探针寿命"
        '
        'Label30
        '
        Me.Label30.AutoSize = True
        Me.Label30.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label30.Location = New System.Drawing.Point(408, 157)
        Me.Label30.Name = "Label30"
        Me.Label30.Size = New System.Drawing.Size(21, 14)
        Me.Label30.TabIndex = 18
        Me.Label30.Text = "次"
        '
        'Label25
        '
        Me.Label25.AutoSize = True
        Me.Label25.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label25.Location = New System.Drawing.Point(58, 114)
        Me.Label25.Name = "Label25"
        Me.Label25.Size = New System.Drawing.Size(63, 14)
        Me.Label25.TabIndex = 23
        Me.Label25.Text = "条码长度"
        '
        'txtOverTemperate
        '
        Me.txtOverTemperate.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtOverTemperate.Location = New System.Drawing.Point(329, 68)
        Me.txtOverTemperate.Name = "txtOverTemperate"
        Me.txtOverTemperate.Size = New System.Drawing.Size(75, 23)
        Me.txtOverTemperate.TabIndex = 9
        Me.txtOverTemperate.Text = "50"
        Me.txtOverTemperate.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'GroupBox8
        '
        Me.GroupBox8.BackColor = System.Drawing.Color.SkyBlue
        Me.GroupBox8.Controls.Add(Me.radioOCV0Set)
        Me.GroupBox8.Controls.Add(Me.radioOCV1Set)
        Me.GroupBox8.Controls.Add(Me.radioOCV2Set)
        Me.GroupBox8.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.GroupBox8.Location = New System.Drawing.Point(15, 15)
        Me.GroupBox8.Name = "GroupBox8"
        Me.GroupBox8.Size = New System.Drawing.Size(494, 78)
        Me.GroupBox8.TabIndex = 28
        Me.GroupBox8.TabStop = False
        Me.GroupBox8.Text = "工作站选择"
        '
        'radioOCV0Set
        '
        Me.radioOCV0Set.AutoSize = True
        Me.radioOCV0Set.Checked = True
        Me.radioOCV0Set.Location = New System.Drawing.Point(55, 32)
        Me.radioOCV0Set.Name = "radioOCV0Set"
        Me.radioOCV0Set.Size = New System.Drawing.Size(53, 18)
        Me.radioOCV0Set.TabIndex = 0
        Me.radioOCV0Set.TabStop = True
        Me.radioOCV0Set.Text = "OCV0"
        Me.radioOCV0Set.UseVisualStyleBackColor = True
        '
        'radioOCV1Set
        '
        Me.radioOCV1Set.AutoSize = True
        Me.radioOCV1Set.Location = New System.Drawing.Point(211, 32)
        Me.radioOCV1Set.Name = "radioOCV1Set"
        Me.radioOCV1Set.Size = New System.Drawing.Size(53, 18)
        Me.radioOCV1Set.TabIndex = 0
        Me.radioOCV1Set.Text = "OCV1"
        Me.radioOCV1Set.UseVisualStyleBackColor = True
        '
        'radioOCV2Set
        '
        Me.radioOCV2Set.AutoSize = True
        Me.radioOCV2Set.Location = New System.Drawing.Point(367, 32)
        Me.radioOCV2Set.Name = "radioOCV2Set"
        Me.radioOCV2Set.Size = New System.Drawing.Size(53, 18)
        Me.radioOCV2Set.TabIndex = 0
        Me.radioOCV2Set.Text = "OCV2"
        Me.radioOCV2Set.UseVisualStyleBackColor = True
        '
        'GroupBox6
        '
        Me.GroupBox6.BackColor = System.Drawing.Color.SkyBlue
        Me.GroupBox6.Controls.Add(Me.Label39)
        Me.GroupBox6.Controls.Add(Me.Label38)
        Me.GroupBox6.Controls.Add(Me.txtVol1)
        Me.GroupBox6.Controls.Add(Me.Label9)
        Me.GroupBox6.Controls.Add(Me.txtRes1)
        Me.GroupBox6.Controls.Add(Me.txtRes2)
        Me.GroupBox6.Controls.Add(Me.Label10)
        Me.GroupBox6.Controls.Add(Me.Label11)
        Me.GroupBox6.Controls.Add(Me.Label14)
        Me.GroupBox6.Controls.Add(Me.txtVol2)
        Me.GroupBox6.Controls.Add(Me.Label13)
        Me.GroupBox6.Controls.Add(Me.Label12)
        Me.GroupBox6.Controls.Add(Me.btnImportTestStandard)
        Me.GroupBox6.Controls.Add(Me.btnSaveTestStandard)
        Me.GroupBox6.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.GroupBox6.Location = New System.Drawing.Point(15, 134)
        Me.GroupBox6.Name = "GroupBox6"
        Me.GroupBox6.Size = New System.Drawing.Size(494, 158)
        Me.GroupBox6.TabIndex = 27
        Me.GroupBox6.TabStop = False
        Me.GroupBox6.Text = "内阻电压量程"
        '
        'Label39
        '
        Me.Label39.AutoSize = True
        Me.Label39.Location = New System.Drawing.Point(267, 30)
        Me.Label39.Name = "Label39"
        Me.Label39.Size = New System.Drawing.Size(49, 14)
        Me.Label39.TabIndex = 28
        Me.Label39.Text = "最大值"
        '
        'Label38
        '
        Me.Label38.AutoSize = True
        Me.Label38.Location = New System.Drawing.Point(103, 27)
        Me.Label38.Name = "Label38"
        Me.Label38.Size = New System.Drawing.Size(49, 14)
        Me.Label38.TabIndex = 28
        Me.Label38.Text = "最小值"
        '
        'txtVol1
        '
        Me.txtVol1.Location = New System.Drawing.Point(98, 93)
        Me.txtVol1.Name = "txtVol1"
        Me.txtVol1.Size = New System.Drawing.Size(75, 23)
        Me.txtVol1.TabIndex = 8
        Me.txtVol1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(34, 53)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(63, 14)
        Me.Label9.TabIndex = 1
        Me.Label9.Text = "内阻量程"
        '
        'txtRes1
        '
        Me.txtRes1.Location = New System.Drawing.Point(98, 49)
        Me.txtRes1.Name = "txtRes1"
        Me.txtRes1.Size = New System.Drawing.Size(75, 23)
        Me.txtRes1.TabIndex = 2
        Me.txtRes1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'txtRes2
        '
        Me.txtRes2.Location = New System.Drawing.Point(256, 53)
        Me.txtRes2.Name = "txtRes2"
        Me.txtRes2.Size = New System.Drawing.Size(75, 23)
        Me.txtRes2.TabIndex = 4
        Me.txtRes2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(206, 56)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(28, 14)
        Me.Label10.TabIndex = 5
        Me.Label10.Text = "---"
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Location = New System.Drawing.Point(331, 58)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(28, 14)
        Me.Label11.TabIndex = 6
        Me.Label11.Text = "mΩ"
        '
        'Label14
        '
        Me.Label14.AutoSize = True
        Me.Label14.Location = New System.Drawing.Point(34, 97)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(63, 14)
        Me.Label14.TabIndex = 7
        Me.Label14.Text = "电压量程"
        '
        'txtVol2
        '
        Me.txtVol2.Location = New System.Drawing.Point(256, 97)
        Me.txtVol2.Name = "txtVol2"
        Me.txtVol2.Size = New System.Drawing.Size(75, 23)
        Me.txtVol2.TabIndex = 9
        Me.txtVol2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Location = New System.Drawing.Point(206, 100)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(28, 14)
        Me.Label13.TabIndex = 10
        Me.Label13.Text = "---"
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Location = New System.Drawing.Point(331, 102)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(21, 14)
        Me.Label12.TabIndex = 11
        Me.Label12.Text = "mV"
        '
        'btnImportTestStandard
        '
        Me.btnImportTestStandard.Location = New System.Drawing.Point(383, 47)
        Me.btnImportTestStandard.Name = "btnImportTestStandard"
        Me.btnImportTestStandard.Size = New System.Drawing.Size(84, 30)
        Me.btnImportTestStandard.TabIndex = 29
        Me.btnImportTestStandard.Text = "导入标准"
        Me.btnImportTestStandard.UseVisualStyleBackColor = True
        '
        'btnSaveTestStandard
        '
        Me.btnSaveTestStandard.Location = New System.Drawing.Point(383, 86)
        Me.btnSaveTestStandard.Name = "btnSaveTestStandard"
        Me.btnSaveTestStandard.Size = New System.Drawing.Size(84, 30)
        Me.btnSaveTestStandard.TabIndex = 30
        Me.btnSaveTestStandard.Text = "保存标准"
        Me.btnSaveTestStandard.UseVisualStyleBackColor = True
        '
        'txtPath
        '
        Me.txtPath.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtPath.Location = New System.Drawing.Point(42, 493)
        Me.txtPath.Name = "txtPath"
        Me.txtPath.Size = New System.Drawing.Size(279, 23)
        Me.txtPath.TabIndex = 13
        Me.txtPath.Visible = False
        '
        'Label15
        '
        Me.Label15.AutoSize = True
        Me.Label15.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label15.Location = New System.Drawing.Point(39, 475)
        Me.Label15.Name = "Label15"
        Me.Label15.Size = New System.Drawing.Size(105, 14)
        Me.Label15.TabIndex = 12
        Me.Label15.Text = "保存文件路径："
        Me.Label15.Visible = False
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label8.Location = New System.Drawing.Point(107, 204)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(0, 14)
        Me.Label8.TabIndex = 0
        '
        'TabPage3
        '
        Me.TabPage3.BackColor = System.Drawing.Color.LightBlue
        Me.TabPage3.Controls.Add(Me.GB_MesDataBase)
        Me.TabPage3.Controls.Add(Me.GroupBox7)
        Me.TabPage3.Controls.Add(Me.CheckBox1)
        Me.TabPage3.Controls.Add(Me.chkAuto)
        Me.TabPage3.Controls.Add(Me.txtMesApiAddress)
        Me.TabPage3.Controls.Add(Me.Label35)
        Me.TabPage3.Location = New System.Drawing.Point(4, 24)
        Me.TabPage3.Name = "TabPage3"
        Me.TabPage3.Size = New System.Drawing.Size(530, 575)
        Me.TabPage3.TabIndex = 2
        Me.TabPage3.Text = "数据库设置"
        '
        'GB_MesDataBase
        '
        Me.GB_MesDataBase.BackColor = System.Drawing.Color.SkyBlue
        Me.GB_MesDataBase.Controls.Add(Me.TxtMesDatabaseName)
        Me.GB_MesDataBase.Controls.Add(Me.TxtMesBaseUser)
        Me.GB_MesDataBase.Controls.Add(Me.TxtMesBasePsw)
        Me.GB_MesDataBase.Controls.Add(Me.Label42)
        Me.GB_MesDataBase.Controls.Add(Me.Label43)
        Me.GB_MesDataBase.Controls.Add(Me.Label52)
        Me.GB_MesDataBase.Controls.Add(Me.txtMesDatabaseServerIP)
        Me.GB_MesDataBase.Controls.Add(Me.Label50)
        Me.GB_MesDataBase.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.GB_MesDataBase.Location = New System.Drawing.Point(90, 260)
        Me.GB_MesDataBase.Name = "GB_MesDataBase"
        Me.GB_MesDataBase.Size = New System.Drawing.Size(352, 171)
        Me.GB_MesDataBase.TabIndex = 8
        Me.GB_MesDataBase.TabStop = False
        Me.GB_MesDataBase.Text = "Mes数据库设置"
        '
        'TxtMesDatabaseName
        '
        Me.TxtMesDatabaseName.Location = New System.Drawing.Point(100, 70)
        Me.TxtMesDatabaseName.Name = "TxtMesDatabaseName"
        Me.TxtMesDatabaseName.Size = New System.Drawing.Size(206, 23)
        Me.TxtMesDatabaseName.TabIndex = 5
        '
        'TxtMesBaseUser
        '
        Me.TxtMesBaseUser.Location = New System.Drawing.Point(100, 98)
        Me.TxtMesBaseUser.Name = "TxtMesBaseUser"
        Me.TxtMesBaseUser.Size = New System.Drawing.Size(206, 23)
        Me.TxtMesBaseUser.TabIndex = 5
        '
        'TxtMesBasePsw
        '
        Me.TxtMesBasePsw.Location = New System.Drawing.Point(100, 126)
        Me.TxtMesBasePsw.Name = "TxtMesBasePsw"
        Me.TxtMesBasePsw.Size = New System.Drawing.Size(206, 23)
        Me.TxtMesBasePsw.TabIndex = 7
        Me.TxtMesBasePsw.UseSystemPasswordChar = True
        '
        'Label42
        '
        Me.Label42.AutoSize = True
        Me.Label42.Location = New System.Drawing.Point(32, 46)
        Me.Label42.Name = "Label42"
        Me.Label42.Size = New System.Drawing.Size(63, 14)
        Me.Label42.TabIndex = 1
        Me.Label42.Text = "服务器IP"
        '
        'Label43
        '
        Me.Label43.AutoSize = True
        Me.Label43.Location = New System.Drawing.Point(56, 130)
        Me.Label43.Name = "Label43"
        Me.Label43.Size = New System.Drawing.Size(35, 14)
        Me.Label43.TabIndex = 6
        Me.Label43.Text = "密码"
        '
        'Label52
        '
        Me.Label52.AutoSize = True
        Me.Label52.Location = New System.Drawing.Point(44, 74)
        Me.Label52.Name = "Label52"
        Me.Label52.Size = New System.Drawing.Size(49, 14)
        Me.Label52.TabIndex = 4
        Me.Label52.Text = "数据库"
        '
        'txtMesDatabaseServerIP
        '
        Me.txtMesDatabaseServerIP.Location = New System.Drawing.Point(100, 42)
        Me.txtMesDatabaseServerIP.Name = "txtMesDatabaseServerIP"
        Me.txtMesDatabaseServerIP.Size = New System.Drawing.Size(206, 23)
        Me.txtMesDatabaseServerIP.TabIndex = 2
        '
        'Label50
        '
        Me.Label50.AutoSize = True
        Me.Label50.Location = New System.Drawing.Point(44, 102)
        Me.Label50.Name = "Label50"
        Me.Label50.Size = New System.Drawing.Size(49, 14)
        Me.Label50.TabIndex = 4
        Me.Label50.Text = "用户名"
        '
        'GroupBox7
        '
        Me.GroupBox7.BackColor = System.Drawing.Color.SkyBlue
        Me.GroupBox7.Controls.Add(Me.txtUser)
        Me.GroupBox7.Controls.Add(Me.txtpwd)
        Me.GroupBox7.Controls.Add(Me.Label7)
        Me.GroupBox7.Controls.Add(Me.Label19)
        Me.GroupBox7.Controls.Add(Me.txtDatabase)
        Me.GroupBox7.Controls.Add(Me.Label18)
        Me.GroupBox7.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.GroupBox7.Location = New System.Drawing.Point(90, 84)
        Me.GroupBox7.Name = "GroupBox7"
        Me.GroupBox7.Size = New System.Drawing.Size(352, 156)
        Me.GroupBox7.TabIndex = 8
        Me.GroupBox7.TabStop = False
        Me.GroupBox7.Text = "本地数据库设置"
        '
        'txtUser
        '
        Me.txtUser.Location = New System.Drawing.Point(100, 76)
        Me.txtUser.Name = "txtUser"
        Me.txtUser.Size = New System.Drawing.Size(206, 23)
        Me.txtUser.TabIndex = 5
        '
        'txtpwd
        '
        Me.txtpwd.Location = New System.Drawing.Point(100, 105)
        Me.txtpwd.Name = "txtpwd"
        Me.txtpwd.Size = New System.Drawing.Size(206, 23)
        Me.txtpwd.TabIndex = 7
        Me.txtpwd.UseSystemPasswordChar = True
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(34, 49)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(63, 14)
        Me.Label7.TabIndex = 1
        Me.Label7.Text = "服务器IP"
        '
        'Label19
        '
        Me.Label19.AutoSize = True
        Me.Label19.Location = New System.Drawing.Point(58, 109)
        Me.Label19.Name = "Label19"
        Me.Label19.Size = New System.Drawing.Size(35, 14)
        Me.Label19.TabIndex = 6
        Me.Label19.Text = "密码"
        '
        'txtDatabase
        '
        Me.txtDatabase.Location = New System.Drawing.Point(100, 45)
        Me.txtDatabase.Name = "txtDatabase"
        Me.txtDatabase.Size = New System.Drawing.Size(206, 23)
        Me.txtDatabase.TabIndex = 2
        '
        'Label18
        '
        Me.Label18.AutoSize = True
        Me.Label18.Location = New System.Drawing.Point(46, 80)
        Me.Label18.Name = "Label18"
        Me.Label18.Size = New System.Drawing.Size(49, 14)
        Me.Label18.TabIndex = 4
        Me.Label18.Text = "用户名"
        '
        'CheckBox1
        '
        Me.CheckBox1.AutoSize = True
        Me.CheckBox1.Checked = True
        Me.CheckBox1.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CheckBox1.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.CheckBox1.Location = New System.Drawing.Point(90, 452)
        Me.CheckBox1.Name = "CheckBox1"
        Me.CheckBox1.Size = New System.Drawing.Size(229, 18)
        Me.CheckBox1.TabIndex = 3
        Me.CheckBox1.Text = "是否测试完成后数据自动上存MES"
        Me.CheckBox1.UseVisualStyleBackColor = True
        '
        'chkAuto
        '
        Me.chkAuto.AutoSize = True
        Me.chkAuto.Checked = True
        Me.chkAuto.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkAuto.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.chkAuto.Location = New System.Drawing.Point(90, 46)
        Me.chkAuto.Name = "chkAuto"
        Me.chkAuto.Size = New System.Drawing.Size(250, 18)
        Me.chkAuto.TabIndex = 3
        Me.chkAuto.Text = "是否测试完成后数据自动上存数据库"
        Me.chkAuto.UseVisualStyleBackColor = True
        '
        'txtMesApiAddress
        '
        Me.txtMesApiAddress.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtMesApiAddress.Location = New System.Drawing.Point(192, 485)
        Me.txtMesApiAddress.Name = "txtMesApiAddress"
        Me.txtMesApiAddress.Size = New System.Drawing.Size(228, 23)
        Me.txtMesApiAddress.TabIndex = 2
        '
        'Label35
        '
        Me.Label35.AutoSize = True
        Me.Label35.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label35.Location = New System.Drawing.Point(109, 488)
        Me.Label35.Name = "Label35"
        Me.Label35.Size = New System.Drawing.Size(77, 14)
        Me.Label35.TabIndex = 1
        Me.Label35.Text = "MESAPI地址"
        '
        'TabScmSet
        '
        Me.TabScmSet.BackColor = System.Drawing.Color.LightBlue
        Me.TabScmSet.Controls.Add(Me.lblScmStatus)
        Me.TabScmSet.Controls.Add(Me.btnScmApply)
        Me.TabScmSet.Controls.Add(Me.btnScmRead)
        Me.TabScmSet.Controls.Add(Me.lblScmPhaseGuardUnit)
        Me.TabScmSet.Controls.Add(Me.txtScmPhaseGuardMs)
        Me.TabScmSet.Controls.Add(Me.lblScmPhaseGuardMs)
        Me.TabScmSet.Controls.Add(Me.lblScmTriggerUnit)
        Me.TabScmSet.Controls.Add(Me.txtScmTriggerPulseMs)
        Me.TabScmSet.Controls.Add(Me.lblScmTriggerPulseMs)
        Me.TabScmSet.Controls.Add(Me.lblScmAutoStepUnit)
        Me.TabScmSet.Controls.Add(Me.txtScmAutoStepMs)
        Me.TabScmSet.Controls.Add(Me.lblScmAutoStepMs)
        Me.TabScmSet.Controls.Add(Me.lblScmChannelUnit)
        Me.TabScmSet.Controls.Add(Me.txtScmChannelSettleMs)
        Me.TabScmSet.Controls.Add(Me.lblScmChannelSettleMs)
        Me.TabScmSet.Controls.Add(Me.lblScmRelayUnit)
        Me.TabScmSet.Controls.Add(Me.txtScmRelaySettleMs)
        Me.TabScmSet.Controls.Add(Me.lblScmRelaySettleMs)
        Me.TabScmSet.Location = New System.Drawing.Point(4, 24)
        Me.TabScmSet.Name = "TabScmSet"
        Me.TabScmSet.Padding = New System.Windows.Forms.Padding(3)
        Me.TabScmSet.Size = New System.Drawing.Size(530, 575)
        Me.TabScmSet.TabIndex = 4
        Me.TabScmSet.Text = "控制板设置"
        '
        'lblScmStatus
        '
        Me.lblScmStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.lblScmStatus.Location = New System.Drawing.Point(118, 357)
        Me.lblScmStatus.Name = "lblScmStatus"
        Me.lblScmStatus.Size = New System.Drawing.Size(310, 90)
        Me.lblScmStatus.TabIndex = 17
        '
        'btnScmApply
        '
        Me.btnScmApply.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.btnScmApply.Location = New System.Drawing.Point(283, 302)
        Me.btnScmApply.Name = "btnScmApply"
        Me.btnScmApply.Size = New System.Drawing.Size(145, 32)
        Me.btnScmApply.TabIndex = 16
        Me.btnScmApply.Text = "应用到 SCM 并保存"
        Me.btnScmApply.UseVisualStyleBackColor = True
        '
        'btnScmRead
        '
        Me.btnScmRead.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.btnScmRead.Location = New System.Drawing.Point(118, 302)
        Me.btnScmRead.Name = "btnScmRead"
        Me.btnScmRead.Size = New System.Drawing.Size(145, 32)
        Me.btnScmRead.TabIndex = 15
        Me.btnScmRead.Text = "读取 SCM 当前值"
        Me.btnScmRead.UseVisualStyleBackColor = True
        '
        'lblScmPhaseGuardUnit
        '
        Me.lblScmPhaseGuardUnit.AutoSize = True
        Me.lblScmPhaseGuardUnit.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblScmPhaseGuardUnit.Location = New System.Drawing.Point(376, 260)
        Me.lblScmPhaseGuardUnit.Name = "lblScmPhaseGuardUnit"
        Me.lblScmPhaseGuardUnit.Size = New System.Drawing.Size(21, 14)
        Me.lblScmPhaseGuardUnit.TabIndex = 14
        Me.lblScmPhaseGuardUnit.Text = "ms"
        '
        'txtScmPhaseGuardMs
        '
        Me.txtScmPhaseGuardMs.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtScmPhaseGuardMs.Location = New System.Drawing.Point(263, 252)
        Me.txtScmPhaseGuardMs.Name = "txtScmPhaseGuardMs"
        Me.txtScmPhaseGuardMs.Size = New System.Drawing.Size(100, 23)
        Me.txtScmPhaseGuardMs.TabIndex = 13
        '
        'lblScmPhaseGuardMs
        '
        Me.lblScmPhaseGuardMs.AutoSize = True
        Me.lblScmPhaseGuardMs.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblScmPhaseGuardMs.Location = New System.Drawing.Point(121, 260)
        Me.lblScmPhaseGuardMs.Name = "lblScmPhaseGuardMs"
        Me.lblScmPhaseGuardMs.Size = New System.Drawing.Size(91, 14)
        Me.lblScmPhaseGuardMs.TabIndex = 12
        Me.lblScmPhaseGuardMs.Text = "遍末保护时间"
        '
        'lblScmTriggerUnit
        '
        Me.lblScmTriggerUnit.AutoSize = True
        Me.lblScmTriggerUnit.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblScmTriggerUnit.Location = New System.Drawing.Point(376, 220)
        Me.lblScmTriggerUnit.Name = "lblScmTriggerUnit"
        Me.lblScmTriggerUnit.Size = New System.Drawing.Size(21, 14)
        Me.lblScmTriggerUnit.TabIndex = 11
        Me.lblScmTriggerUnit.Text = "ms"
        '
        'txtScmTriggerPulseMs
        '
        Me.txtScmTriggerPulseMs.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtScmTriggerPulseMs.Location = New System.Drawing.Point(263, 212)
        Me.txtScmTriggerPulseMs.Name = "txtScmTriggerPulseMs"
        Me.txtScmTriggerPulseMs.Size = New System.Drawing.Size(100, 23)
        Me.txtScmTriggerPulseMs.TabIndex = 10
        '
        'lblScmTriggerPulseMs
        '
        Me.lblScmTriggerPulseMs.AutoSize = True
        Me.lblScmTriggerPulseMs.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblScmTriggerPulseMs.Location = New System.Drawing.Point(121, 220)
        Me.lblScmTriggerPulseMs.Name = "lblScmTriggerPulseMs"
        Me.lblScmTriggerPulseMs.Size = New System.Drawing.Size(91, 14)
        Me.lblScmTriggerPulseMs.TabIndex = 9
        Me.lblScmTriggerPulseMs.Text = "触发脉宽时间"
        '
        'lblScmAutoStepUnit
        '
        Me.lblScmAutoStepUnit.AutoSize = True
        Me.lblScmAutoStepUnit.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblScmAutoStepUnit.Location = New System.Drawing.Point(376, 180)
        Me.lblScmAutoStepUnit.Name = "lblScmAutoStepUnit"
        Me.lblScmAutoStepUnit.Size = New System.Drawing.Size(21, 14)
        Me.lblScmAutoStepUnit.TabIndex = 8
        Me.lblScmAutoStepUnit.Text = "ms"
        '
        'txtScmAutoStepMs
        '
        Me.txtScmAutoStepMs.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtScmAutoStepMs.Location = New System.Drawing.Point(263, 172)
        Me.txtScmAutoStepMs.Name = "txtScmAutoStepMs"
        Me.txtScmAutoStepMs.Size = New System.Drawing.Size(100, 23)
        Me.txtScmAutoStepMs.TabIndex = 7
        '
        'lblScmAutoStepMs
        '
        Me.lblScmAutoStepMs.AutoSize = True
        Me.lblScmAutoStepMs.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblScmAutoStepMs.Location = New System.Drawing.Point(121, 180)
        Me.lblScmAutoStepMs.Name = "lblScmAutoStepMs"
        Me.lblScmAutoStepMs.Size = New System.Drawing.Size(91, 14)
        Me.lblScmAutoStepMs.TabIndex = 6
        Me.lblScmAutoStepMs.Text = "仪表等待时间"
        '
        'lblScmChannelUnit
        '
        Me.lblScmChannelUnit.AutoSize = True
        Me.lblScmChannelUnit.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblScmChannelUnit.Location = New System.Drawing.Point(376, 140)
        Me.lblScmChannelUnit.Name = "lblScmChannelUnit"
        Me.lblScmChannelUnit.Size = New System.Drawing.Size(21, 14)
        Me.lblScmChannelUnit.TabIndex = 5
        Me.lblScmChannelUnit.Text = "ms"
        '
        'txtScmChannelSettleMs
        '
        Me.txtScmChannelSettleMs.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtScmChannelSettleMs.Location = New System.Drawing.Point(263, 132)
        Me.txtScmChannelSettleMs.Name = "txtScmChannelSettleMs"
        Me.txtScmChannelSettleMs.Size = New System.Drawing.Size(100, 23)
        Me.txtScmChannelSettleMs.TabIndex = 4
        '
        'lblScmChannelSettleMs
        '
        Me.lblScmChannelSettleMs.AutoSize = True
        Me.lblScmChannelSettleMs.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblScmChannelSettleMs.Location = New System.Drawing.Point(121, 140)
        Me.lblScmChannelSettleMs.Name = "lblScmChannelSettleMs"
        Me.lblScmChannelSettleMs.Size = New System.Drawing.Size(91, 14)
        Me.lblScmChannelSettleMs.TabIndex = 3
        Me.lblScmChannelSettleMs.Text = "通道稳定时间"
        '
        'lblScmRelayUnit
        '
        Me.lblScmRelayUnit.AutoSize = True
        Me.lblScmRelayUnit.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblScmRelayUnit.Location = New System.Drawing.Point(376, 100)
        Me.lblScmRelayUnit.Name = "lblScmRelayUnit"
        Me.lblScmRelayUnit.Size = New System.Drawing.Size(21, 14)
        Me.lblScmRelayUnit.TabIndex = 2
        Me.lblScmRelayUnit.Text = "ms"
        '
        'txtScmRelaySettleMs
        '
        Me.txtScmRelaySettleMs.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtScmRelaySettleMs.Location = New System.Drawing.Point(263, 92)
        Me.txtScmRelaySettleMs.Name = "txtScmRelaySettleMs"
        Me.txtScmRelaySettleMs.Size = New System.Drawing.Size(100, 23)
        Me.txtScmRelaySettleMs.TabIndex = 1
        '
        'lblScmRelaySettleMs
        '
        Me.lblScmRelaySettleMs.AutoSize = True
        Me.lblScmRelaySettleMs.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblScmRelaySettleMs.Location = New System.Drawing.Point(121, 100)
        Me.lblScmRelaySettleMs.Name = "lblScmRelaySettleMs"
        Me.lblScmRelaySettleMs.Size = New System.Drawing.Size(105, 14)
        Me.lblScmRelaySettleMs.TabIndex = 0
        Me.lblScmRelaySettleMs.Text = "继电器稳定时间"
        '
        'TabPageChannelColors
        '
        Me.TabPageChannelColors.BackColor = System.Drawing.Color.LightBlue
        Me.TabPageChannelColors.Controls.Add(Me.GroupBoxChannelColors)
        Me.TabPageChannelColors.Location = New System.Drawing.Point(4, 24)
        Me.TabPageChannelColors.Name = "TabPageChannelColors"
        Me.TabPageChannelColors.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPageChannelColors.Size = New System.Drawing.Size(530, 575)
        Me.TabPageChannelColors.TabIndex = 5
        Me.TabPageChannelColors.Text = "通道颜色"
        '
        'GroupBoxChannelColors
        '
        Me.GroupBoxChannelColors.Controls.Add(Me.btnResetChannelColors)
        Me.GroupBoxChannelColors.Controls.Add(Me.btnVoltageNgColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.txtVoltageNgColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.pnlVoltageNgColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.lblVoltageNgColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.btnSelectedColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.txtSelectedColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.pnlSelectedColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.lblSelectedColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.btnResistanceNgColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.txtResistanceNgColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.pnlResistanceNgColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.lblResistanceNgColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.btnIdleColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.txtIdleColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.pnlIdleColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.lblIdleColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.btnTestingColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.txtTestingColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.pnlTestingColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.lblTestingColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.btnPassColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.txtPassColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.pnlPassColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.lblPassColor)
        Me.GroupBoxChannelColors.Controls.Add(Me.lblChannelColorHint)
        Me.GroupBoxChannelColors.Location = New System.Drawing.Point(8, 8)
        Me.GroupBoxChannelColors.Name = "GroupBoxChannelColors"
        Me.GroupBoxChannelColors.Size = New System.Drawing.Size(516, 530)
        Me.GroupBoxChannelColors.TabIndex = 0
        Me.GroupBoxChannelColors.TabStop = False
        Me.GroupBoxChannelColors.Text = "通道状态颜色"
        '
        'btnResetChannelColors
        '
        Me.btnResetChannelColors.Location = New System.Drawing.Point(314, 438)
        Me.btnResetChannelColors.Name = "btnResetChannelColors"
        Me.btnResetChannelColors.Size = New System.Drawing.Size(105, 30)
        Me.btnResetChannelColors.TabIndex = 25
        Me.btnResetChannelColors.Text = "恢复默认"
        Me.btnResetChannelColors.UseVisualStyleBackColor = True
        '
        'btnVoltageNgColor
        '
        Me.btnVoltageNgColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.btnVoltageNgColor.Location = New System.Drawing.Point(344, 326)
        Me.btnVoltageNgColor.Name = "btnVoltageNgColor"
        Me.btnVoltageNgColor.Size = New System.Drawing.Size(75, 27)
        Me.btnVoltageNgColor.TabIndex = 20
        Me.btnVoltageNgColor.Text = "选择颜色"
        Me.btnVoltageNgColor.UseVisualStyleBackColor = True
        '
        'txtVoltageNgColor
        '
        Me.txtVoltageNgColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtVoltageNgColor.Location = New System.Drawing.Point(230, 329)
        Me.txtVoltageNgColor.Name = "txtVoltageNgColor"
        Me.txtVoltageNgColor.ReadOnly = True
        Me.txtVoltageNgColor.Size = New System.Drawing.Size(100, 23)
        Me.txtVoltageNgColor.TabIndex = 19
        Me.txtVoltageNgColor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'pnlVoltageNgColor
        '
        Me.pnlVoltageNgColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlVoltageNgColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.pnlVoltageNgColor.Location = New System.Drawing.Point(178, 326)
        Me.pnlVoltageNgColor.Name = "pnlVoltageNgColor"
        Me.pnlVoltageNgColor.Size = New System.Drawing.Size(42, 27)
        Me.pnlVoltageNgColor.TabIndex = 18
        '
        'lblVoltageNgColor
        '
        Me.lblVoltageNgColor.AutoSize = True
        Me.lblVoltageNgColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblVoltageNgColor.Location = New System.Drawing.Point(82, 335)
        Me.lblVoltageNgColor.Name = "lblVoltageNgColor"
        Me.lblVoltageNgColor.Size = New System.Drawing.Size(126, 14)
        Me.lblVoltageNgColor.TabIndex = 17
        Me.lblVoltageNgColor.Text = "电压 NG（橘红色）"
        '
        'btnSelectedColor
        '
        Me.btnSelectedColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.btnSelectedColor.Location = New System.Drawing.Point(344, 381)
        Me.btnSelectedColor.Name = "btnSelectedColor"
        Me.btnSelectedColor.Size = New System.Drawing.Size(75, 27)
        Me.btnSelectedColor.TabIndex = 24
        Me.btnSelectedColor.Text = "选择颜色"
        Me.btnSelectedColor.UseVisualStyleBackColor = True
        '
        'txtSelectedColor
        '
        Me.txtSelectedColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtSelectedColor.Location = New System.Drawing.Point(230, 384)
        Me.txtSelectedColor.Name = "txtSelectedColor"
        Me.txtSelectedColor.ReadOnly = True
        Me.txtSelectedColor.Size = New System.Drawing.Size(100, 23)
        Me.txtSelectedColor.TabIndex = 23
        Me.txtSelectedColor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'pnlSelectedColor
        '
        Me.pnlSelectedColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlSelectedColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.pnlSelectedColor.Location = New System.Drawing.Point(178, 381)
        Me.pnlSelectedColor.Name = "pnlSelectedColor"
        Me.pnlSelectedColor.Size = New System.Drawing.Size(42, 27)
        Me.pnlSelectedColor.TabIndex = 22
        '
        'lblSelectedColor
        '
        Me.lblSelectedColor.AutoSize = True
        Me.lblSelectedColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblSelectedColor.Location = New System.Drawing.Point(82, 390)
        Me.lblSelectedColor.Name = "lblSelectedColor"
        Me.lblSelectedColor.Size = New System.Drawing.Size(105, 14)
        Me.lblSelectedColor.TabIndex = 21
        Me.lblSelectedColor.Text = "选定（铁灰色）"
        '
        'btnResistanceNgColor
        '
        Me.btnResistanceNgColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.btnResistanceNgColor.Location = New System.Drawing.Point(344, 271)
        Me.btnResistanceNgColor.Name = "btnResistanceNgColor"
        Me.btnResistanceNgColor.Size = New System.Drawing.Size(75, 27)
        Me.btnResistanceNgColor.TabIndex = 16
        Me.btnResistanceNgColor.Text = "选择颜色"
        Me.btnResistanceNgColor.UseVisualStyleBackColor = True
        '
        'txtResistanceNgColor
        '
        Me.txtResistanceNgColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtResistanceNgColor.Location = New System.Drawing.Point(230, 274)
        Me.txtResistanceNgColor.Name = "txtResistanceNgColor"
        Me.txtResistanceNgColor.ReadOnly = True
        Me.txtResistanceNgColor.Size = New System.Drawing.Size(100, 23)
        Me.txtResistanceNgColor.TabIndex = 15
        Me.txtResistanceNgColor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'pnlResistanceNgColor
        '
        Me.pnlResistanceNgColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlResistanceNgColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.pnlResistanceNgColor.Location = New System.Drawing.Point(178, 271)
        Me.pnlResistanceNgColor.Name = "pnlResistanceNgColor"
        Me.pnlResistanceNgColor.Size = New System.Drawing.Size(42, 27)
        Me.pnlResistanceNgColor.TabIndex = 14
        '
        'lblResistanceNgColor
        '
        Me.lblResistanceNgColor.AutoSize = True
        Me.lblResistanceNgColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblResistanceNgColor.Location = New System.Drawing.Point(82, 280)
        Me.lblResistanceNgColor.Name = "lblResistanceNgColor"
        Me.lblResistanceNgColor.Size = New System.Drawing.Size(112, 14)
        Me.lblResistanceNgColor.TabIndex = 13
        Me.lblResistanceNgColor.Text = "内阻 NG（红色）"
        '
        'btnIdleColor
        '
        Me.btnIdleColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.btnIdleColor.Location = New System.Drawing.Point(344, 216)
        Me.btnIdleColor.Name = "btnIdleColor"
        Me.btnIdleColor.Size = New System.Drawing.Size(75, 27)
        Me.btnIdleColor.TabIndex = 12
        Me.btnIdleColor.Text = "选择颜色"
        Me.btnIdleColor.UseVisualStyleBackColor = True
        '
        'txtIdleColor
        '
        Me.txtIdleColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtIdleColor.Location = New System.Drawing.Point(230, 219)
        Me.txtIdleColor.Name = "txtIdleColor"
        Me.txtIdleColor.ReadOnly = True
        Me.txtIdleColor.Size = New System.Drawing.Size(100, 23)
        Me.txtIdleColor.TabIndex = 11
        Me.txtIdleColor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'pnlIdleColor
        '
        Me.pnlIdleColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlIdleColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.pnlIdleColor.Location = New System.Drawing.Point(178, 216)
        Me.pnlIdleColor.Name = "pnlIdleColor"
        Me.pnlIdleColor.Size = New System.Drawing.Size(42, 27)
        Me.pnlIdleColor.TabIndex = 10
        '
        'lblIdleColor
        '
        Me.lblIdleColor.AutoSize = True
        Me.lblIdleColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblIdleColor.Location = New System.Drawing.Point(82, 225)
        Me.lblIdleColor.Name = "lblIdleColor"
        Me.lblIdleColor.Size = New System.Drawing.Size(91, 14)
        Me.lblIdleColor.TabIndex = 9
        Me.lblIdleColor.Text = "空闲（灰色）"
        '
        'btnTestingColor
        '
        Me.btnTestingColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.btnTestingColor.Location = New System.Drawing.Point(344, 161)
        Me.btnTestingColor.Name = "btnTestingColor"
        Me.btnTestingColor.Size = New System.Drawing.Size(75, 27)
        Me.btnTestingColor.TabIndex = 8
        Me.btnTestingColor.Text = "选择颜色"
        Me.btnTestingColor.UseVisualStyleBackColor = True
        '
        'txtTestingColor
        '
        Me.txtTestingColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtTestingColor.Location = New System.Drawing.Point(230, 164)
        Me.txtTestingColor.Name = "txtTestingColor"
        Me.txtTestingColor.ReadOnly = True
        Me.txtTestingColor.Size = New System.Drawing.Size(100, 23)
        Me.txtTestingColor.TabIndex = 7
        Me.txtTestingColor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'pnlTestingColor
        '
        Me.pnlTestingColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlTestingColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.pnlTestingColor.Location = New System.Drawing.Point(178, 161)
        Me.pnlTestingColor.Name = "pnlTestingColor"
        Me.pnlTestingColor.Size = New System.Drawing.Size(42, 27)
        Me.pnlTestingColor.TabIndex = 6
        '
        'lblTestingColor
        '
        Me.lblTestingColor.AutoSize = True
        Me.lblTestingColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblTestingColor.Location = New System.Drawing.Point(82, 170)
        Me.lblTestingColor.Name = "lblTestingColor"
        Me.lblTestingColor.Size = New System.Drawing.Size(105, 14)
        Me.lblTestingColor.TabIndex = 5
        Me.lblTestingColor.Text = "测试中（蓝色）"
        '
        'btnPassColor
        '
        Me.btnPassColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.btnPassColor.Location = New System.Drawing.Point(344, 106)
        Me.btnPassColor.Name = "btnPassColor"
        Me.btnPassColor.Size = New System.Drawing.Size(75, 27)
        Me.btnPassColor.TabIndex = 4
        Me.btnPassColor.Text = "选择颜色"
        Me.btnPassColor.UseVisualStyleBackColor = True
        '
        'txtPassColor
        '
        Me.txtPassColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtPassColor.Location = New System.Drawing.Point(230, 109)
        Me.txtPassColor.Name = "txtPassColor"
        Me.txtPassColor.ReadOnly = True
        Me.txtPassColor.Size = New System.Drawing.Size(100, 23)
        Me.txtPassColor.TabIndex = 3
        Me.txtPassColor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'pnlPassColor
        '
        Me.pnlPassColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlPassColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.pnlPassColor.Location = New System.Drawing.Point(178, 106)
        Me.pnlPassColor.Name = "pnlPassColor"
        Me.pnlPassColor.Size = New System.Drawing.Size(42, 27)
        Me.pnlPassColor.TabIndex = 2
        '
        'lblPassColor
        '
        Me.lblPassColor.AutoSize = True
        Me.lblPassColor.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblPassColor.Location = New System.Drawing.Point(82, 115)
        Me.lblPassColor.Name = "lblPassColor"
        Me.lblPassColor.Size = New System.Drawing.Size(91, 14)
        Me.lblPassColor.TabIndex = 1
        Me.lblPassColor.Text = "PASS（绿色）"
        '
        'lblChannelColorHint
        '
        Me.lblChannelColorHint.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblChannelColorHint.Location = New System.Drawing.Point(80, 57)
        Me.lblChannelColorHint.Name = "lblChannelColorHint"
        Me.lblChannelColorHint.Size = New System.Drawing.Size(338, 36)
        Me.lblChannelColorHint.TabIndex = 0
        Me.lblChannelColorHint.Text = "点击选择颜色修改通道格颜色，点击恢复默认恢复约定颜色。保存后应用到主页面和测试过程。"
        '
        'TabPageInstrumentSettings
        '
        Me.TabPageInstrumentSettings.BackColor = System.Drawing.Color.LightBlue
        Me.TabPageInstrumentSettings.Controls.Add(Me.GroupBoxInstrument34461)
        Me.TabPageInstrumentSettings.Controls.Add(Me.GroupBoxInstrumentMain3562)
        Me.TabPageInstrumentSettings.Controls.Add(Me.GroupBoxInstrumentRetest3562)
        Me.TabPageInstrumentSettings.Controls.Add(Me.lblInstrumentSettingsNote)
        Me.TabPageInstrumentSettings.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.TabPageInstrumentSettings.Location = New System.Drawing.Point(4, 24)
        Me.TabPageInstrumentSettings.Name = "TabPageInstrumentSettings"
        Me.TabPageInstrumentSettings.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPageInstrumentSettings.Size = New System.Drawing.Size(530, 575)
        Me.TabPageInstrumentSettings.TabIndex = 6
        Me.TabPageInstrumentSettings.Text = "仪表参数设置"
        '
        'GroupBoxInstrument34461
        '
        Me.GroupBoxInstrument34461.BackColor = System.Drawing.Color.SkyBlue
        Me.GroupBoxInstrument34461.Controls.Add(Me.lbl34461Range)
        Me.GroupBoxInstrument34461.Controls.Add(Me.lbl34461Nplc)
        Me.GroupBoxInstrument34461.Controls.Add(Me.lbl34461AutoZero)
        Me.GroupBoxInstrument34461.Controls.Add(Me.cmb34461Range)
        Me.GroupBoxInstrument34461.Controls.Add(Me.cmb34461Nplc)
        Me.GroupBoxInstrument34461.Controls.Add(Me.cmb34461AutoZero)
        Me.GroupBoxInstrument34461.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.GroupBoxInstrument34461.Location = New System.Drawing.Point(17, 26)
        Me.GroupBoxInstrument34461.Name = "GroupBoxInstrument34461"
        Me.GroupBoxInstrument34461.Size = New System.Drawing.Size(498, 115)
        Me.GroupBoxInstrument34461.TabIndex = 0
        Me.GroupBoxInstrument34461.TabStop = False
        Me.GroupBoxInstrument34461.Text = "34461A 数字万用表（电压采样）"
        '
        'lbl34461Range
        '
        Me.lbl34461Range.AutoSize = True
        Me.lbl34461Range.Location = New System.Drawing.Point(12, 44)
        Me.lbl34461Range.Name = "lbl34461Range"
        Me.lbl34461Range.Size = New System.Drawing.Size(63, 14)
        Me.lbl34461Range.TabIndex = 0
        Me.lbl34461Range.Text = "电压量程"
        '
        'lbl34461Nplc
        '
        Me.lbl34461Nplc.AutoSize = True
        Me.lbl34461Nplc.Location = New System.Drawing.Point(176, 44)
        Me.lbl34461Nplc.Name = "lbl34461Nplc"
        Me.lbl34461Nplc.Size = New System.Drawing.Size(119, 14)
        Me.lbl34461Nplc.TabIndex = 1
        Me.lbl34461Nplc.Text = "积分时间（NPLC）"
        '
        'lbl34461AutoZero
        '
        Me.lbl34461AutoZero.AutoSize = True
        Me.lbl34461AutoZero.Location = New System.Drawing.Point(340, 44)
        Me.lbl34461AutoZero.Name = "lbl34461AutoZero"
        Me.lbl34461AutoZero.Size = New System.Drawing.Size(63, 14)
        Me.lbl34461AutoZero.TabIndex = 2
        Me.lbl34461AutoZero.Text = "自动归零"
        '
        'cmb34461Range
        '
        Me.cmb34461Range.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmb34461Range.Items.AddRange(New Object() {"100 mV", "1 V", "10 V", "100 V", "1000 V"})
        Me.cmb34461Range.Location = New System.Drawing.Point(12, 66)
        Me.cmb34461Range.Name = "cmb34461Range"
        Me.cmb34461Range.Size = New System.Drawing.Size(145, 22)
        Me.cmb34461Range.TabIndex = 3
        '
        'cmb34461Nplc
        '
        Me.cmb34461Nplc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmb34461Nplc.Items.AddRange(New Object() {"0.001", "0.002", "0.006", "0.02", "0.06", "0.2", "1", "10", "100"})
        Me.cmb34461Nplc.Location = New System.Drawing.Point(176, 66)
        Me.cmb34461Nplc.Name = "cmb34461Nplc"
        Me.cmb34461Nplc.Size = New System.Drawing.Size(145, 22)
        Me.cmb34461Nplc.TabIndex = 4
        '
        'cmb34461AutoZero
        '
        Me.cmb34461AutoZero.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmb34461AutoZero.Items.AddRange(New Object() {"开启", "关闭", "单次归零"})
        Me.cmb34461AutoZero.Location = New System.Drawing.Point(340, 66)
        Me.cmb34461AutoZero.Name = "cmb34461AutoZero"
        Me.cmb34461AutoZero.Size = New System.Drawing.Size(125, 22)
        Me.cmb34461AutoZero.TabIndex = 5
        '
        'GroupBoxInstrumentMain3562
        '
        Me.GroupBoxInstrumentMain3562.BackColor = System.Drawing.Color.SkyBlue
        Me.GroupBoxInstrumentMain3562.Controls.Add(Me.lblMain3562Function)
        Me.GroupBoxInstrumentMain3562.Controls.Add(Me.lblMain3562ResistanceRange)
        Me.GroupBoxInstrumentMain3562.Controls.Add(Me.lblMain3562VoltageRange)
        Me.GroupBoxInstrumentMain3562.Controls.Add(Me.lblMain3562SampleRate)
        Me.GroupBoxInstrumentMain3562.Controls.Add(Me.cmbMain3562Function)
        Me.GroupBoxInstrumentMain3562.Controls.Add(Me.cmbMain3562ResistanceRange)
        Me.GroupBoxInstrumentMain3562.Controls.Add(Me.cmbMain3562VoltageRange)
        Me.GroupBoxInstrumentMain3562.Controls.Add(Me.cmbMain3562SampleRate)
        Me.GroupBoxInstrumentMain3562.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.GroupBoxInstrumentMain3562.Location = New System.Drawing.Point(17, 166)
        Me.GroupBoxInstrumentMain3562.Name = "GroupBoxInstrumentMain3562"
        Me.GroupBoxInstrumentMain3562.Size = New System.Drawing.Size(498, 153)
        Me.GroupBoxInstrumentMain3562.TabIndex = 1
        Me.GroupBoxInstrumentMain3562.TabStop = False
        Me.GroupBoxInstrumentMain3562.Text = "主3562A"
        '
        'lblMain3562Function
        '
        Me.lblMain3562Function.AutoSize = True
        Me.lblMain3562Function.Location = New System.Drawing.Point(12, 24)
        Me.lblMain3562Function.Name = "lblMain3562Function"
        Me.lblMain3562Function.Size = New System.Drawing.Size(63, 14)
        Me.lblMain3562Function.TabIndex = 0
        Me.lblMain3562Function.Text = "测量功能"
        '
        'lblMain3562ResistanceRange
        '
        Me.lblMain3562ResistanceRange.AutoSize = True
        Me.lblMain3562ResistanceRange.Location = New System.Drawing.Point(260, 24)
        Me.lblMain3562ResistanceRange.Name = "lblMain3562ResistanceRange"
        Me.lblMain3562ResistanceRange.Size = New System.Drawing.Size(63, 14)
        Me.lblMain3562ResistanceRange.TabIndex = 1
        Me.lblMain3562ResistanceRange.Text = "内阻量程"
        '
        'lblMain3562VoltageRange
        '
        Me.lblMain3562VoltageRange.AutoSize = True
        Me.lblMain3562VoltageRange.Location = New System.Drawing.Point(12, 91)
        Me.lblMain3562VoltageRange.Name = "lblMain3562VoltageRange"
        Me.lblMain3562VoltageRange.Size = New System.Drawing.Size(63, 14)
        Me.lblMain3562VoltageRange.TabIndex = 2
        Me.lblMain3562VoltageRange.Text = "电压量程"
        '
        'lblMain3562SampleRate
        '
        Me.lblMain3562SampleRate.AutoSize = True
        Me.lblMain3562SampleRate.Location = New System.Drawing.Point(260, 91)
        Me.lblMain3562SampleRate.Name = "lblMain3562SampleRate"
        Me.lblMain3562SampleRate.Size = New System.Drawing.Size(63, 14)
        Me.lblMain3562SampleRate.TabIndex = 3
        Me.lblMain3562SampleRate.Text = "采样速度"
        '
        'cmbMain3562Function
        '
        Me.cmbMain3562Function.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbMain3562Function.Items.AddRange(New Object() {"内阻 + 电压（ΩV）", "仅内阻（Ω）"})
        Me.cmbMain3562Function.Location = New System.Drawing.Point(12, 46)
        Me.cmbMain3562Function.Name = "cmbMain3562Function"
        Me.cmbMain3562Function.Size = New System.Drawing.Size(220, 22)
        Me.cmbMain3562Function.TabIndex = 4
        '
        'cmbMain3562ResistanceRange
        '
        Me.cmbMain3562ResistanceRange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbMain3562ResistanceRange.Items.AddRange(New Object() {"3 mΩ", "30 mΩ", "300 mΩ", "3 Ω", "30 Ω", "300 Ω", "3 kΩ"})
        Me.cmbMain3562ResistanceRange.Location = New System.Drawing.Point(260, 46)
        Me.cmbMain3562ResistanceRange.Name = "cmbMain3562ResistanceRange"
        Me.cmbMain3562ResistanceRange.Size = New System.Drawing.Size(210, 22)
        Me.cmbMain3562ResistanceRange.TabIndex = 5
        '
        'cmbMain3562VoltageRange
        '
        Me.cmbMain3562VoltageRange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbMain3562VoltageRange.Items.AddRange(New Object() {"6 V", "60 V", "100 V（BT3562A）"})
        Me.cmbMain3562VoltageRange.Location = New System.Drawing.Point(12, 113)
        Me.cmbMain3562VoltageRange.Name = "cmbMain3562VoltageRange"
        Me.cmbMain3562VoltageRange.Size = New System.Drawing.Size(220, 22)
        Me.cmbMain3562VoltageRange.TabIndex = 6
        '
        'cmbMain3562SampleRate
        '
        Me.cmbMain3562SampleRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbMain3562SampleRate.Items.AddRange(New Object() {"超高速（EX.FAST）", "高速（FAST）", "中速（MEDIUM）", "低速（SLOW）"})
        Me.cmbMain3562SampleRate.Location = New System.Drawing.Point(260, 113)
        Me.cmbMain3562SampleRate.Name = "cmbMain3562SampleRate"
        Me.cmbMain3562SampleRate.Size = New System.Drawing.Size(210, 22)
        Me.cmbMain3562SampleRate.TabIndex = 7
        '
        'GroupBoxInstrumentRetest3562
        '
        Me.GroupBoxInstrumentRetest3562.BackColor = System.Drawing.Color.SkyBlue
        Me.GroupBoxInstrumentRetest3562.Controls.Add(Me.lblRetest3562Function)
        Me.GroupBoxInstrumentRetest3562.Controls.Add(Me.lblRetest3562ResistanceRange)
        Me.GroupBoxInstrumentRetest3562.Controls.Add(Me.lblRetest3562VoltageRange)
        Me.GroupBoxInstrumentRetest3562.Controls.Add(Me.lblRetest3562SampleRate)
        Me.GroupBoxInstrumentRetest3562.Controls.Add(Me.cmbRetest3562Function)
        Me.GroupBoxInstrumentRetest3562.Controls.Add(Me.cmbRetest3562ResistanceRange)
        Me.GroupBoxInstrumentRetest3562.Controls.Add(Me.cmbRetest3562VoltageRange)
        Me.GroupBoxInstrumentRetest3562.Controls.Add(Me.cmbRetest3562SampleRate)
        Me.GroupBoxInstrumentRetest3562.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.GroupBoxInstrumentRetest3562.Location = New System.Drawing.Point(17, 344)
        Me.GroupBoxInstrumentRetest3562.Name = "GroupBoxInstrumentRetest3562"
        Me.GroupBoxInstrumentRetest3562.Size = New System.Drawing.Size(498, 153)
        Me.GroupBoxInstrumentRetest3562.TabIndex = 2
        Me.GroupBoxInstrumentRetest3562.TabStop = False
        Me.GroupBoxInstrumentRetest3562.Text = "复测3562A"
        '
        'lblRetest3562Function
        '
        Me.lblRetest3562Function.AutoSize = True
        Me.lblRetest3562Function.Location = New System.Drawing.Point(12, 24)
        Me.lblRetest3562Function.Name = "lblRetest3562Function"
        Me.lblRetest3562Function.Size = New System.Drawing.Size(63, 14)
        Me.lblRetest3562Function.TabIndex = 0
        Me.lblRetest3562Function.Text = "测量功能"
        '
        'lblRetest3562ResistanceRange
        '
        Me.lblRetest3562ResistanceRange.AutoSize = True
        Me.lblRetest3562ResistanceRange.Location = New System.Drawing.Point(260, 24)
        Me.lblRetest3562ResistanceRange.Name = "lblRetest3562ResistanceRange"
        Me.lblRetest3562ResistanceRange.Size = New System.Drawing.Size(63, 14)
        Me.lblRetest3562ResistanceRange.TabIndex = 1
        Me.lblRetest3562ResistanceRange.Text = "内阻量程"
        '
        'lblRetest3562VoltageRange
        '
        Me.lblRetest3562VoltageRange.AutoSize = True
        Me.lblRetest3562VoltageRange.Location = New System.Drawing.Point(12, 91)
        Me.lblRetest3562VoltageRange.Name = "lblRetest3562VoltageRange"
        Me.lblRetest3562VoltageRange.Size = New System.Drawing.Size(63, 14)
        Me.lblRetest3562VoltageRange.TabIndex = 2
        Me.lblRetest3562VoltageRange.Text = "电压量程"
        '
        'lblRetest3562SampleRate
        '
        Me.lblRetest3562SampleRate.AutoSize = True
        Me.lblRetest3562SampleRate.Location = New System.Drawing.Point(260, 91)
        Me.lblRetest3562SampleRate.Name = "lblRetest3562SampleRate"
        Me.lblRetest3562SampleRate.Size = New System.Drawing.Size(63, 14)
        Me.lblRetest3562SampleRate.TabIndex = 3
        Me.lblRetest3562SampleRate.Text = "采样速度"
        '
        'cmbRetest3562Function
        '
        Me.cmbRetest3562Function.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbRetest3562Function.Items.AddRange(New Object() {"内阻 + 电压（ΩV）"})
        Me.cmbRetest3562Function.Location = New System.Drawing.Point(12, 46)
        Me.cmbRetest3562Function.Name = "cmbRetest3562Function"
        Me.cmbRetest3562Function.Size = New System.Drawing.Size(220, 22)
        Me.cmbRetest3562Function.TabIndex = 4
        '
        'cmbRetest3562ResistanceRange
        '
        Me.cmbRetest3562ResistanceRange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbRetest3562ResistanceRange.Items.AddRange(New Object() {"3 mΩ", "30 mΩ", "300 mΩ", "3 Ω", "30 Ω", "300 Ω", "3 kΩ"})
        Me.cmbRetest3562ResistanceRange.Location = New System.Drawing.Point(260, 46)
        Me.cmbRetest3562ResistanceRange.Name = "cmbRetest3562ResistanceRange"
        Me.cmbRetest3562ResistanceRange.Size = New System.Drawing.Size(210, 22)
        Me.cmbRetest3562ResistanceRange.TabIndex = 5
        '
        'cmbRetest3562VoltageRange
        '
        Me.cmbRetest3562VoltageRange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbRetest3562VoltageRange.Items.AddRange(New Object() {"6 V", "60 V", "100 V（BT3562A）"})
        Me.cmbRetest3562VoltageRange.Location = New System.Drawing.Point(12, 113)
        Me.cmbRetest3562VoltageRange.Name = "cmbRetest3562VoltageRange"
        Me.cmbRetest3562VoltageRange.Size = New System.Drawing.Size(220, 22)
        Me.cmbRetest3562VoltageRange.TabIndex = 6
        '
        'cmbRetest3562SampleRate
        '
        Me.cmbRetest3562SampleRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbRetest3562SampleRate.Items.AddRange(New Object() {"超高速（EX.FAST）", "高速（FAST）", "中速（MEDIUM）", "低速（SLOW）"})
        Me.cmbRetest3562SampleRate.Location = New System.Drawing.Point(260, 113)
        Me.cmbRetest3562SampleRate.Name = "cmbRetest3562SampleRate"
        Me.cmbRetest3562SampleRate.Size = New System.Drawing.Size(210, 22)
        Me.cmbRetest3562SampleRate.TabIndex = 7
        '
        'lblInstrumentSettingsNote
        '
        Me.lblInstrumentSettingsNote.AutoSize = True
        Me.lblInstrumentSettingsNote.Location = New System.Drawing.Point(21, 528)
        Me.lblInstrumentSettingsNote.Name = "lblInstrumentSettingsNote"
        Me.lblInstrumentSettingsNote.Size = New System.Drawing.Size(392, 14)
        Me.lblInstrumentSettingsNote.TabIndex = 3
        Me.lblInstrumentSettingsNote.Text = "仅内阻模式只影响主3562A；正式测试的电压仍由34461A采集。"
        '
        'CmdOK
        '
        Me.CmdOK.Location = New System.Drawing.Point(171, 633)
        Me.CmdOK.Name = "CmdOK"
        Me.CmdOK.Size = New System.Drawing.Size(79, 34)
        Me.CmdOK.TabIndex = 1
        Me.CmdOK.Text = "确定"
        Me.CmdOK.UseVisualStyleBackColor = True
        '
        'Button1
        '
        Me.Button1.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Button1.Location = New System.Drawing.Point(300, 633)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 34)
        Me.Button1.TabIndex = 2
        Me.Button1.Text = "取消"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'GroupBox13
        '
        Me.GroupBox13.BackColor = System.Drawing.Color.Cyan
        Me.GroupBox13.Controls.Add(Me.ComboBox1)
        Me.GroupBox13.Controls.Add(Me.Label54)
        Me.GroupBox13.Controls.Add(Me.Label55)
        Me.GroupBox13.Controls.Add(Me.ComboBox2)
        Me.GroupBox13.Location = New System.Drawing.Point(350, 14)
        Me.GroupBox13.Name = "GroupBox13"
        Me.GroupBox13.Size = New System.Drawing.Size(10, 13)
        Me.GroupBox13.TabIndex = 17
        Me.GroupBox13.TabStop = False
        Me.GroupBox13.Text = "IO控制卡通讯参数"
        '
        'ComboBox1
        '
        Me.ComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBox1.FormattingEnabled = True
        Me.ComboBox1.Location = New System.Drawing.Point(80, 21)
        Me.ComboBox1.Name = "ComboBox1"
        Me.ComboBox1.Size = New System.Drawing.Size(90, 22)
        Me.ComboBox1.TabIndex = 8
        '
        'Label54
        '
        Me.Label54.AutoSize = True
        Me.Label54.Location = New System.Drawing.Point(13, 25)
        Me.Label54.Name = "Label54"
        Me.Label54.Size = New System.Drawing.Size(63, 14)
        Me.Label54.TabIndex = 6
        Me.Label54.Text = "通讯端口"
        '
        'Label55
        '
        Me.Label55.AutoSize = True
        Me.Label55.Location = New System.Drawing.Point(195, 25)
        Me.Label55.Name = "Label55"
        Me.Label55.Size = New System.Drawing.Size(49, 14)
        Me.Label55.TabIndex = 7
        Me.Label55.Text = "波特率"
        '
        'ComboBox2
        '
        Me.ComboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.ComboBox2.FormattingEnabled = True
        Me.ComboBox2.Items.AddRange(New Object() {"9600", "19200", "38400", "115200"})
        Me.ComboBox2.Location = New System.Drawing.Point(251, 21)
        Me.ComboBox2.Name = "ComboBox2"
        Me.ComboBox2.Size = New System.Drawing.Size(90, 22)
        Me.ComboBox2.TabIndex = 9
        '
        'frmInit
        '
        Me.AcceptButton = Me.CmdOK
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(224, Byte), Integer))
        Me.CancelButton = Me.Button1
        Me.ClientSize = New System.Drawing.Size(552, 693)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.CmdOK)
        Me.Controls.Add(Me.TabControl1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.Name = "frmInit"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Tag = "设置"
        Me.Text = "初始设置"
        Me.TopMost = False
        Me.TabControl1.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.GroupBox9.ResumeLayout(False)
        Me.GroupBox9.PerformLayout()
        Me.GroupBox5.ResumeLayout(False)
        Me.GroupBox5.PerformLayout()
        Me.GroupBox4.ResumeLayout(False)
        Me.GroupBox4.PerformLayout()
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.GroupBox12.ResumeLayout(False)
        Me.GroupBox12.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox10.ResumeLayout(False)
        Me.GroupBox10.PerformLayout()
        Me.TabPage2.ResumeLayout(False)
        Me.TabPage2.PerformLayout()
        Me.GroupBox11.ResumeLayout(False)
        Me.GroupBox11.PerformLayout()
        Me.GroupBox8.ResumeLayout(False)
        Me.GroupBox8.PerformLayout()
        Me.GroupBox6.ResumeLayout(False)
        Me.GroupBox6.PerformLayout()
        Me.TabPage3.ResumeLayout(False)
        Me.TabPage3.PerformLayout()
        Me.GB_MesDataBase.ResumeLayout(False)
        Me.GB_MesDataBase.PerformLayout()
        Me.GroupBox7.ResumeLayout(False)
        Me.GroupBox7.PerformLayout()
        Me.TabScmSet.ResumeLayout(False)
        Me.TabScmSet.PerformLayout()
        Me.TabPageChannelColors.ResumeLayout(False)
        Me.GroupBoxChannelColors.ResumeLayout(False)
        Me.GroupBoxChannelColors.PerformLayout()
        Me.TabPageInstrumentSettings.ResumeLayout(False)
        Me.TabPageInstrumentSettings.PerformLayout()
        Me.GroupBoxInstrument34461.ResumeLayout(False)
        Me.GroupBoxInstrument34461.PerformLayout()
        Me.GroupBoxInstrumentMain3562.ResumeLayout(False)
        Me.GroupBoxInstrumentMain3562.PerformLayout()
        Me.GroupBoxInstrumentRetest3562.ResumeLayout(False)
        Me.GroupBoxInstrumentRetest3562.PerformLayout()
        Me.GroupBox13.ResumeLayout(False)
        Me.GroupBox13.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents TabControl1 As System.Windows.Forms.TabControl
    Friend WithEvents TabPage1 As System.Windows.Forms.TabPage
    Friend WithEvents TabPage2 As System.Windows.Forms.TabPage
    Friend WithEvents TabPage3 As System.Windows.Forms.TabPage
    Friend WithEvents CmdOK As System.Windows.Forms.Button
    Friend WithEvents cmbRerate As System.Windows.Forms.ComboBox
    Friend WithEvents CmbRecom As System.Windows.Forms.ComboBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents ComRate As System.Windows.Forms.ComboBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtDatabase As System.Windows.Forms.TextBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents txtVol2 As System.Windows.Forms.TextBox
    Friend WithEvents txtVol1 As System.Windows.Forms.TextBox
    Friend WithEvents Label14 As System.Windows.Forms.Label
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents txtRes2 As System.Windows.Forms.TextBox
    Friend WithEvents txtRes1 As System.Windows.Forms.TextBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents txtPath As System.Windows.Forms.TextBox
    Friend WithEvents Label15 As System.Windows.Forms.Label
    Friend WithEvents chkAuto As System.Windows.Forms.CheckBox
    Friend WithEvents txt_R_B As System.Windows.Forms.TextBox
    Friend WithEvents txt_V_B As System.Windows.Forms.TextBox
    Friend WithEvents Label17 As System.Windows.Forms.Label
    Friend WithEvents Label16 As System.Windows.Forms.Label
    Friend WithEvents txtpwd As System.Windows.Forms.TextBox
    Friend WithEvents Label19 As System.Windows.Forms.Label
    Friend WithEvents txtUser As System.Windows.Forms.TextBox
    Friend WithEvents Label18 As System.Windows.Forms.Label
    Friend WithEvents txt_ScmPort As System.Windows.Forms.TextBox
    Friend WithEvents Txt_ScmIP As System.Windows.Forms.TextBox
    Friend WithEvents Label21 As System.Windows.Forms.Label
    Friend WithEvents Label20 As System.Windows.Forms.Label
    Friend WithEvents cmb_SCMRate As ComboBox
    Friend WithEvents lbl_SCMRate As Label
    Friend WithEvents cmb_SCMPort As ComboBox
    Friend WithEvents lbl_SCMPort As Label
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents btnScmConnectTest As Button
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents btnMain3562ConnectTest As Button
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents btnScannerConnectTest As Button
    Friend WithEvents cmb_ScanerPort As ComboBox
    Friend WithEvents Label22 As Label
    Friend WithEvents Label23 As Label
    Friend WithEvents cmb_ScanerRate As ComboBox
    Friend WithEvents Txt_DelayTime As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents GroupBox5 As GroupBox
    Friend WithEvents CmbIORecom As ComboBox
    Friend WithEvents Label26 As Label
    Friend WithEvents Label27 As Label
    Friend WithEvents cmbIORerate As ComboBox
    Friend WithEvents GroupBox4 As GroupBox
    Friend WithEvents btn34461ConnectTest As Button
    Friend WithEvents Label4 As Label
    Friend WithEvents txt34461 As TextBox
    Friend WithEvents txtProbeUsed As TextBox
    Friend WithEvents txtProbelifespan As TextBox
    Friend WithEvents txtCodeLenth As TextBox
    Friend WithEvents Label31 As Label
    Friend WithEvents Label24 As Label
    Friend WithEvents Label25 As Label
    Friend WithEvents Label30 As Label
    Friend WithEvents Label29 As Label
    Friend WithEvents Label28 As Label
    Friend WithEvents CheckBox1 As CheckBox
    Friend WithEvents txtMesApiAddress As TextBox
    Friend WithEvents Label35 As Label
    Friend WithEvents GroupBox6 As GroupBox
    Friend WithEvents btnImportTestStandard As Button
    Friend WithEvents btnSaveTestStandard As Button
    Friend WithEvents txtCailiModelValue As TextBox
    Friend WithEvents Label37 As Label
    Friend WithEvents Label36 As Label
    Friend WithEvents Label39 As Label
    Friend WithEvents Label38 As Label
    Friend WithEvents txtOverTemperate As TextBox
    Friend WithEvents Label40 As Label
    Friend WithEvents Label41 As Label
    Friend WithEvents GroupBox8 As GroupBox
    Friend WithEvents radioOCV1Set As RadioButton
    Friend WithEvents radioOCV2Set As RadioButton
    Friend WithEvents radioOCV0Set As RadioButton
    Friend WithEvents GroupBox9 As GroupBox
    Friend WithEvents btnPlcConnectTest As Button
    Friend WithEvents cmb_PlcRate As ComboBox
    Friend WithEvents Label44 As Label
    Friend WithEvents cmb_PlcPort As ComboBox
    Friend WithEvents Label45 As Label
    Friend WithEvents txt_PlcPort As TextBox
    Friend WithEvents Label46 As Label
    Friend WithEvents Label47 As Label
    Friend WithEvents Txt_PlcIP As TextBox
    Friend WithEvents ChbScmCom As CheckBox
    Friend WithEvents ChbSCMIP As CheckBox
    Friend WithEvents ChbPlcCom As CheckBox
    Friend WithEvents ChbPLCIP As CheckBox
    Friend WithEvents cmbRetestPort As ComboBox
    Friend WithEvents cmbRetestRate As ComboBox
    Friend WithEvents btnRetestConnect As Button
    Friend WithEvents GroupBox10 As GroupBox
    Friend WithEvents Label48 As Label
    Friend WithEvents Label49 As Label
    Friend WithEvents lblScmRelaySettleMs As Label
    Friend WithEvents txtScmRelaySettleMs As TextBox
    Friend WithEvents lblScmRelayUnit As Label
    Friend WithEvents lblScmChannelSettleMs As Label
    Friend WithEvents txtScmChannelSettleMs As TextBox
    Friend WithEvents lblScmChannelUnit As Label
    Friend WithEvents lblScmAutoStepMs As Label
    Friend WithEvents txtScmAutoStepMs As TextBox
    Friend WithEvents lblScmAutoStepUnit As Label
    Friend WithEvents lblScmTriggerPulseMs As Label
    Friend WithEvents txtScmTriggerPulseMs As TextBox
    Friend WithEvents lblScmTriggerUnit As Label
    Friend WithEvents lblScmPhaseGuardMs As Label
    Friend WithEvents txtScmPhaseGuardMs As TextBox
    Friend WithEvents lblScmPhaseGuardUnit As Label
    Friend WithEvents btnScmRead As Button
    Friend WithEvents btnScmApply As Button
    Friend WithEvents lblScmStatus As Label
    Friend WithEvents TabScmSet As TabPage
    Friend WithEvents GB_MesDataBase As GroupBox
    Friend WithEvents TxtMesBaseUser As TextBox
    Friend WithEvents TxtMesBasePsw As TextBox
    Friend WithEvents Label42 As Label
    Friend WithEvents Label43 As Label
    Friend WithEvents txtMesDatabaseServerIP As TextBox
    Friend WithEvents Label50 As Label
    Friend WithEvents GroupBox7 As GroupBox
    Friend WithEvents TxtMesDatabaseName As TextBox
    Friend WithEvents Label52 As Label
    Friend WithEvents TabPageChannelColors As TabPage
    Friend WithEvents TabPageInstrumentSettings As TabPage
    Friend WithEvents GroupBoxInstrument34461 As GroupBox
    Friend WithEvents GroupBoxInstrumentMain3562 As GroupBox
    Friend WithEvents GroupBoxInstrumentRetest3562 As GroupBox
    Friend WithEvents lbl34461Range As Label
    Friend WithEvents lbl34461Nplc As Label
    Friend WithEvents lbl34461AutoZero As Label
    Friend WithEvents cmb34461Range As ComboBox
    Friend WithEvents cmb34461Nplc As ComboBox
    Friend WithEvents cmb34461AutoZero As ComboBox
    Friend WithEvents lblMain3562Function As Label
    Friend WithEvents lblMain3562ResistanceRange As Label
    Friend WithEvents lblMain3562VoltageRange As Label
    Friend WithEvents lblMain3562SampleRate As Label
    Friend WithEvents cmbMain3562Function As ComboBox
    Friend WithEvents cmbMain3562ResistanceRange As ComboBox
    Friend WithEvents cmbMain3562VoltageRange As ComboBox
    Friend WithEvents cmbMain3562SampleRate As ComboBox
    Friend WithEvents lblRetest3562Function As Label
    Friend WithEvents lblRetest3562ResistanceRange As Label
    Friend WithEvents lblRetest3562VoltageRange As Label
    Friend WithEvents lblRetest3562SampleRate As Label
    Friend WithEvents cmbRetest3562Function As ComboBox
    Friend WithEvents cmbRetest3562ResistanceRange As ComboBox
    Friend WithEvents cmbRetest3562VoltageRange As ComboBox
    Friend WithEvents cmbRetest3562SampleRate As ComboBox
    Friend WithEvents lblInstrumentSettingsNote As Label
    Friend WithEvents GroupBoxChannelColors As GroupBox
    Friend WithEvents lblChannelColorHint As Label
    Friend WithEvents lblPassColor As Label
    Friend WithEvents pnlPassColor As Panel
    Friend WithEvents txtPassColor As TextBox
    Friend WithEvents btnPassColor As Button
    Friend WithEvents lblTestingColor As Label
    Friend WithEvents pnlTestingColor As Panel
    Friend WithEvents txtTestingColor As TextBox
    Friend WithEvents btnTestingColor As Button
    Friend WithEvents lblIdleColor As Label
    Friend WithEvents pnlIdleColor As Panel
    Friend WithEvents txtIdleColor As TextBox
    Friend WithEvents btnIdleColor As Button
    Friend WithEvents lblResistanceNgColor As Label
    Friend WithEvents pnlResistanceNgColor As Panel
    Friend WithEvents txtResistanceNgColor As TextBox
    Friend WithEvents btnResistanceNgColor As Button
    Friend WithEvents lblVoltageNgColor As Label
    Friend WithEvents pnlVoltageNgColor As Panel
    Friend WithEvents txtVoltageNgColor As TextBox
    Friend WithEvents btnVoltageNgColor As Button
    Friend WithEvents lblSelectedColor As Label
    Friend WithEvents pnlSelectedColor As Panel
    Friend WithEvents txtSelectedColor As TextBox
    Friend WithEvents btnSelectedColor As Button
    Friend WithEvents btnResetChannelColors As Button
    Friend WithEvents colorDialogChannel As ColorDialog
    Friend WithEvents GroupBox11 As GroupBox
    Friend WithEvents GroupBox12 As GroupBox
    Friend WithEvents btnTemperatureConnectTest As Button
    Friend WithEvents cmb_TemPort As ComboBox
    Friend WithEvents Label51 As Label
    Friend WithEvents Label53 As Label
    Friend WithEvents cmb_TempRate As ComboBox
    Friend WithEvents GroupBox13 As GroupBox
    Friend WithEvents ComboBox1 As ComboBox
    Friend WithEvents Label54 As Label
    Friend WithEvents Label55 As Label
    Friend WithEvents ComboBox2 As ComboBox
End Class
