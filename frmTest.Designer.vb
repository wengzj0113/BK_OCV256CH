<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmTest
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
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
        Me.components = New System.ComponentModel.Container()
        Me.SaveFileDialog1 = New System.Windows.Forms.SaveFileDialog()
        Me.Pic_Bat = New System.Windows.Forms.PictureBox()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.Timer2 = New System.Windows.Forms.Timer(Me.components)
        Me.Timer4 = New System.Windows.Forms.Timer(Me.components)
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.txtMinR = New System.Windows.Forms.TextBox()
        Me.txtMaxR = New System.Windows.Forms.TextBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.txtMinV = New System.Windows.Forms.TextBox()
        Me.txtMaxV = New System.Windows.Forms.TextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.btnStop = New System.Windows.Forms.Button()
        Me.btnNGTest = New System.Windows.Forms.Button()
        Me.CmdStart = New System.Windows.Forms.Button()
        Me.lblBarNum = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.txtLastNum = New System.Windows.Forms.TextBox()
        Me.txtFirstNum = New System.Windows.Forms.TextBox()
        Me.txtTray = New System.Windows.Forms.TextBox()
        Me.grpChannelSelection = New System.Windows.Forms.GroupBox()
        Me.btnChannelSelectSingle = New System.Windows.Forms.Button()
        Me.btnChannelRectangle = New System.Windows.Forms.Button()
        Me.btnChannelClear = New System.Windows.Forms.Button()
        Me.btnChannelSelectAll = New System.Windows.Forms.Button()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.Rbtn1 = New System.Windows.Forms.RadioButton()
        Me.CmdBatInput = New System.Windows.Forms.Button()
        Me.cmbShow = New System.Windows.Forms.ComboBox()
        Me.Rbtn3 = New System.Windows.Forms.RadioButton()
        Me.Rbtn2 = New System.Windows.Forms.RadioButton()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.btnOpen = New System.Windows.Forms.Button()
        Me.btnTrayState = New System.Windows.Forms.Button()
        Me.GroupBox6 = New System.Windows.Forms.GroupBox()
        Me.btn_ChCalibrate = New System.Windows.Forms.Button()
        Me.cmb_ChSelect = New System.Windows.Forms.ComboBox()
        Me.txtJiajuR = New System.Windows.Forms.TextBox()
        Me.txtCaliR = New System.Windows.Forms.TextBox()
        Me.txtTestR = New System.Windows.Forms.TextBox()
        Me.GroupBox5 = New System.Windows.Forms.GroupBox()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.LabelRun = New System.Windows.Forms.Label()
        Me.LabelAlarm = New System.Windows.Forms.Label()
        Me.Label28 = New System.Windows.Forms.Label()
        Me.Label17 = New System.Windows.Forms.Label()
        Me.smokeAlarm1 = New System.Windows.Forms.TextBox()
        Me.Label19 = New System.Windows.Forms.Label()
        Me.smokeAlarm2 = New System.Windows.Forms.TextBox()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.t4 = New System.Windows.Forms.TextBox()
        Me.t3 = New System.Windows.Forms.TextBox()
        Me.t2 = New System.Windows.Forms.TextBox()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.t1 = New System.Windows.Forms.TextBox()
        Me.Label16 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label15 = New System.Windows.Forms.Label()
        Me.GroupBox7 = New System.Windows.Forms.GroupBox()
        Me.txtAlarm = New System.Windows.Forms.TextBox()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.GroupBox8 = New System.Windows.Forms.GroupBox()
        Me.Label29 = New System.Windows.Forms.Label()
        Me.txtRemain = New System.Windows.Forms.TextBox()
        Me.Label26 = New System.Windows.Forms.Label()
        Me.txtUsed = New System.Windows.Forms.TextBox()
        Me.txtTotle = New System.Windows.Forms.TextBox()
        Me.channelGrid = New software.ChannelGridControl()
        Me.statisticsControl = New software.StatisticsControl()
        Me.Label2 = New System.Windows.Forms.Label()
        CType(Me.Pic_Bat, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox3.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.grpChannelSelection.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox5.SuspendLayout()
        Me.Panel3.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.GroupBox7.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.GroupBox8.SuspendLayout()
        Me.SuspendLayout()
        '
        'Pic_Bat
        '
        Me.Pic_Bat.BackColor = System.Drawing.Color.White
        Me.Pic_Bat.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Pic_Bat.Location = New System.Drawing.Point(299, 6)
        Me.Pic_Bat.Name = "Pic_Bat"
        Me.Pic_Bat.Size = New System.Drawing.Size(969, 890)
        Me.Pic_Bat.TabIndex = 0
        Me.Pic_Bat.TabStop = False
        Me.Pic_Bat.Visible = False
        '
        'Timer1
        '
        Me.Timer1.Interval = 1000
        '
        'Timer2
        '
        Me.Timer2.Interval = 1000
        '
        'Timer4
        '
        Me.Timer4.Interval = 1000
        '
        'GroupBox3
        '
        Me.GroupBox3.BackColor = System.Drawing.Color.White
        Me.GroupBox3.Controls.Add(Me.Label12)
        Me.GroupBox3.Controls.Add(Me.txtMinR)
        Me.GroupBox3.Controls.Add(Me.txtMaxR)
        Me.GroupBox3.Controls.Add(Me.Label9)
        Me.GroupBox3.Controls.Add(Me.Label8)
        Me.GroupBox3.Controls.Add(Me.txtMinV)
        Me.GroupBox3.Controls.Add(Me.txtMaxV)
        Me.GroupBox3.Controls.Add(Me.Label7)
        Me.GroupBox3.Location = New System.Drawing.Point(2, 279)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(306, 89)
        Me.GroupBox3.TabIndex = 49
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "合格范围设置"
        '
        'Label12
        '
        Me.Label12.Location = New System.Drawing.Point(10, 56)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(75, 21)
        Me.Label12.TabIndex = 57
        Me.Label12.Text = "内阻(mΩ):"
        Me.Label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'txtMinR
        '
        Me.txtMinR.BackColor = System.Drawing.Color.White
        Me.txtMinR.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtMinR.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.txtMinR.Location = New System.Drawing.Point(93, 56)
        Me.txtMinR.Name = "txtMinR"
        Me.txtMinR.ReadOnly = True
        Me.txtMinR.Size = New System.Drawing.Size(70, 21)
        Me.txtMinR.TabIndex = 54
        '
        'txtMaxR
        '
        Me.txtMaxR.BackColor = System.Drawing.Color.White
        Me.txtMaxR.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtMaxR.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.txtMaxR.Location = New System.Drawing.Point(190, 56)
        Me.txtMaxR.Name = "txtMaxR"
        Me.txtMaxR.ReadOnly = True
        Me.txtMaxR.Size = New System.Drawing.Size(75, 21)
        Me.txtMaxR.TabIndex = 53
        '
        'Label9
        '
        Me.Label9.Location = New System.Drawing.Point(93, 14)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(70, 12)
        Me.Label9.TabIndex = 52
        Me.Label9.Text = "Min"
        Me.Label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label8
        '
        Me.Label8.Location = New System.Drawing.Point(190, 14)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(75, 12)
        Me.Label8.TabIndex = 51
        Me.Label8.Text = "Max"
        Me.Label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'txtMinV
        '
        Me.txtMinV.BackColor = System.Drawing.Color.White
        Me.txtMinV.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtMinV.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.txtMinV.Location = New System.Drawing.Point(93, 29)
        Me.txtMinV.Name = "txtMinV"
        Me.txtMinV.ReadOnly = True
        Me.txtMinV.Size = New System.Drawing.Size(70, 21)
        Me.txtMinV.TabIndex = 50
        '
        'txtMaxV
        '
        Me.txtMaxV.BackColor = System.Drawing.Color.White
        Me.txtMaxV.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtMaxV.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.txtMaxV.Location = New System.Drawing.Point(190, 29)
        Me.txtMaxV.Name = "txtMaxV"
        Me.txtMaxV.ReadOnly = True
        Me.txtMaxV.Size = New System.Drawing.Size(75, 21)
        Me.txtMaxV.TabIndex = 49
        '
        'Label7
        '
        Me.Label7.Location = New System.Drawing.Point(10, 29)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(75, 21)
        Me.Label7.TabIndex = 48
        Me.Label7.Text = "电压(mV):"
        Me.Label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'GroupBox1
        '
        Me.GroupBox1.BackColor = System.Drawing.Color.White
        Me.GroupBox1.Controls.Add(Me.btnClose)
        Me.GroupBox1.Controls.Add(Me.btnStop)
        Me.GroupBox1.Controls.Add(Me.btnOpen)
        Me.GroupBox1.Controls.Add(Me.btnNGTest)
        Me.GroupBox1.Controls.Add(Me.btnTrayState)
        Me.GroupBox1.Controls.Add(Me.CmdStart)
        Me.GroupBox1.Controls.Add(Me.lblBarNum)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.Label5)
        Me.GroupBox1.Controls.Add(Me.Label4)
        Me.GroupBox1.Controls.Add(Me.txtLastNum)
        Me.GroupBox1.Controls.Add(Me.txtFirstNum)
        Me.GroupBox1.Controls.Add(Me.txtTray)
        Me.GroupBox1.Location = New System.Drawing.Point(2, 57)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(306, 157)
        Me.GroupBox1.TabIndex = 56
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "控制板"
        '
        'btnStop
        '
        Me.btnStop.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.btnStop.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.btnStop.Location = New System.Drawing.Point(101, 74)
        Me.btnStop.Name = "btnStop"
        Me.btnStop.Size = New System.Drawing.Size(70, 25)
        Me.btnStop.TabIndex = 65
        Me.btnStop.Text = "停止测试"
        Me.btnStop.UseVisualStyleBackColor = False
        '
        'btnNGTest
        '
        Me.btnNGTest.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.btnNGTest.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.btnNGTest.Location = New System.Drawing.Point(185, 74)
        Me.btnNGTest.Name = "btnNGTest"
        Me.btnNGTest.Size = New System.Drawing.Size(70, 25)
        Me.btnNGTest.TabIndex = 56
        Me.btnNGTest.Text = "NG点测试"
        Me.btnNGTest.UseVisualStyleBackColor = False
        '
        'CmdStart
        '
        Me.CmdStart.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.CmdStart.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.CmdStart.Location = New System.Drawing.Point(16, 74)
        Me.CmdStart.Name = "CmdStart"
        Me.CmdStart.Size = New System.Drawing.Size(70, 25)
        Me.CmdStart.TabIndex = 56
        Me.CmdStart.Text = "启动测试"
        Me.CmdStart.UseVisualStyleBackColor = False
        '
        'lblBarNum
        '
        Me.lblBarNum.AutoSize = True
        Me.lblBarNum.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblBarNum.Location = New System.Drawing.Point(98, 49)
        Me.lblBarNum.Name = "lblBarNum"
        Me.lblBarNum.Size = New System.Drawing.Size(49, 14)
        Me.lblBarNum.TabIndex = 50
        Me.lblBarNum.Text = "intsum"
        '
        'Label3
        '
        Me.Label3.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label3.Location = New System.Drawing.Point(4, 201)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(10, 21)
        Me.Label3.TabIndex = 42
        Me.Label3.Text = "首电池位置"
        Me.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.Label3.Visible = False
        '
        'Label5
        '
        Me.Label5.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label5.Location = New System.Drawing.Point(-1, 49)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(87, 21)
        Me.Label5.TabIndex = 47
        Me.Label5.Text = "待测试通道数"
        Me.Label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'Label4
        '
        Me.Label4.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label4.Location = New System.Drawing.Point(4, 226)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(10, 21)
        Me.Label4.TabIndex = 43
        Me.Label4.Text = "尾电池位置"
        Me.Label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.Label4.Visible = False
        '
        'txtLastNum
        '
        Me.txtLastNum.BackColor = System.Drawing.Color.White
        Me.txtLastNum.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtLastNum.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.txtLastNum.Location = New System.Drawing.Point(87, 226)
        Me.txtLastNum.Name = "txtLastNum"
        Me.txtLastNum.ReadOnly = True
        Me.txtLastNum.Size = New System.Drawing.Size(10, 21)
        Me.txtLastNum.TabIndex = 46
        Me.txtLastNum.Visible = False
        '
        'txtFirstNum
        '
        Me.txtFirstNum.BackColor = System.Drawing.Color.White
        Me.txtFirstNum.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtFirstNum.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.txtFirstNum.Location = New System.Drawing.Point(87, 201)
        Me.txtFirstNum.Name = "txtFirstNum"
        Me.txtFirstNum.ReadOnly = True
        Me.txtFirstNum.Size = New System.Drawing.Size(10, 21)
        Me.txtFirstNum.TabIndex = 45
        Me.txtFirstNum.Visible = False
        '
        'txtTray
        '
        Me.txtTray.BackColor = System.Drawing.Color.White
        Me.txtTray.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtTray.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.txtTray.Location = New System.Drawing.Point(63, 17)
        Me.txtTray.Name = "txtTray"
        Me.txtTray.ReadOnly = True
        Me.txtTray.Size = New System.Drawing.Size(227, 21)
        Me.txtTray.TabIndex = 44
        '
        'grpChannelSelection
        '
        Me.grpChannelSelection.BackColor = System.Drawing.Color.White
        Me.grpChannelSelection.Controls.Add(Me.btnChannelSelectSingle)
        Me.grpChannelSelection.Controls.Add(Me.btnChannelRectangle)
        Me.grpChannelSelection.Controls.Add(Me.btnChannelClear)
        Me.grpChannelSelection.Controls.Add(Me.btnChannelSelectAll)
        Me.grpChannelSelection.Location = New System.Drawing.Point(2, 220)
        Me.grpChannelSelection.Name = "grpChannelSelection"
        Me.grpChannelSelection.Size = New System.Drawing.Size(306, 53)
        Me.grpChannelSelection.TabIndex = 72
        Me.grpChannelSelection.TabStop = False
        Me.grpChannelSelection.Text = "通道选择"
        '
        'btnChannelSelectSingle
        '
        Me.btnChannelSelectSingle.Location = New System.Drawing.Point(221, 16)
        Me.btnChannelSelectSingle.Name = "btnChannelSelectSingle"
        Me.btnChannelSelectSingle.Size = New System.Drawing.Size(62, 28)
        Me.btnChannelSelectSingle.TabIndex = 3
        Me.btnChannelSelectSingle.Text = "单选"
        Me.btnChannelSelectSingle.UseVisualStyleBackColor = True
        '
        'btnChannelRectangle
        '
        Me.btnChannelRectangle.Location = New System.Drawing.Point(153, 16)
        Me.btnChannelRectangle.Name = "btnChannelRectangle"
        Me.btnChannelRectangle.Size = New System.Drawing.Size(62, 28)
        Me.btnChannelRectangle.TabIndex = 2
        Me.btnChannelRectangle.Text = "框选"
        Me.btnChannelRectangle.UseVisualStyleBackColor = True
        '
        'btnChannelClear
        '
        Me.btnChannelClear.Location = New System.Drawing.Point(85, 16)
        Me.btnChannelClear.Name = "btnChannelClear"
        Me.btnChannelClear.Size = New System.Drawing.Size(62, 28)
        Me.btnChannelClear.TabIndex = 1
        Me.btnChannelClear.Text = "清除"
        Me.btnChannelClear.UseVisualStyleBackColor = True
        '
        'btnChannelSelectAll
        '
        Me.btnChannelSelectAll.Location = New System.Drawing.Point(17, 16)
        Me.btnChannelSelectAll.Name = "btnChannelSelectAll"
        Me.btnChannelSelectAll.Size = New System.Drawing.Size(62, 28)
        Me.btnChannelSelectAll.TabIndex = 0
        Me.btnChannelSelectAll.Text = "全选"
        Me.btnChannelSelectAll.UseVisualStyleBackColor = True
        '
        'GroupBox2
        '
        Me.GroupBox2.BackColor = System.Drawing.Color.White
        Me.GroupBox2.Controls.Add(Me.Rbtn1)
        Me.GroupBox2.Controls.Add(Me.CmdBatInput)
        Me.GroupBox2.Controls.Add(Me.cmbShow)
        Me.GroupBox2.Controls.Add(Me.Rbtn3)
        Me.GroupBox2.Controls.Add(Me.Rbtn2)
        Me.GroupBox2.Location = New System.Drawing.Point(2, 12)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(306, 39)
        Me.GroupBox2.TabIndex = 58
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "工作站选择"
        '
        'Rbtn1
        '
        Me.Rbtn1.AutoSize = True
        Me.Rbtn1.Checked = True
        Me.Rbtn1.Enabled = False
        Me.Rbtn1.Font = New System.Drawing.Font("宋体", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Rbtn1.Location = New System.Drawing.Point(102, 12)
        Me.Rbtn1.Name = "Rbtn1"
        Me.Rbtn1.Size = New System.Drawing.Size(61, 20)
        Me.Rbtn1.TabIndex = 47
        Me.Rbtn1.TabStop = True
        Me.Rbtn1.Text = "OCV1"
        Me.Rbtn1.UseVisualStyleBackColor = True
        '
        'CmdBatInput
        '
        Me.CmdBatInput.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.CmdBatInput.Location = New System.Drawing.Point(185, 66)
        Me.CmdBatInput.Name = "CmdBatInput"
        Me.CmdBatInput.Size = New System.Drawing.Size(107, 25)
        Me.CmdBatInput.TabIndex = 52
        Me.CmdBatInput.Text = "逐个扫描电池条形码"
        Me.CmdBatInput.UseVisualStyleBackColor = True
        Me.CmdBatInput.Visible = False
        '
        'cmbShow
        '
        Me.cmbShow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbShow.FormattingEnabled = True
        Me.cmbShow.Location = New System.Drawing.Point(10, 47)
        Me.cmbShow.Name = "cmbShow"
        Me.cmbShow.Size = New System.Drawing.Size(174, 20)
        Me.cmbShow.TabIndex = 51
        '
        'Rbtn3
        '
        Me.Rbtn3.AutoSize = True
        Me.Rbtn3.Enabled = False
        Me.Rbtn3.Font = New System.Drawing.Font("宋体", 12.0!, System.Drawing.FontStyle.Bold)
        Me.Rbtn3.Location = New System.Drawing.Point(104, 13)
        Me.Rbtn3.Name = "Rbtn3"
        Me.Rbtn3.Size = New System.Drawing.Size(61, 20)
        Me.Rbtn3.TabIndex = 49
        Me.Rbtn3.Text = "OCV3"
        Me.Rbtn3.UseVisualStyleBackColor = True
        Me.Rbtn3.Visible = False
        '
        'Rbtn2
        '
        Me.Rbtn2.AutoSize = True
        Me.Rbtn2.Enabled = False
        Me.Rbtn2.Font = New System.Drawing.Font("宋体", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Rbtn2.Location = New System.Drawing.Point(107, 13)
        Me.Rbtn2.Name = "Rbtn2"
        Me.Rbtn2.Size = New System.Drawing.Size(61, 20)
        Me.Rbtn2.TabIndex = 48
        Me.Rbtn2.Text = "OCV2"
        Me.Rbtn2.UseVisualStyleBackColor = True
        '
        'btnClose
        '
        Me.btnClose.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.btnClose.Location = New System.Drawing.Point(185, 109)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(70, 25)
        Me.btnClose.TabIndex = 63
        Me.btnClose.Text = "闭合托盘"
        Me.btnClose.UseVisualStyleBackColor = False
        '
        'btnOpen
        '
        Me.btnOpen.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.btnOpen.Location = New System.Drawing.Point(102, 109)
        Me.btnOpen.Name = "btnOpen"
        Me.btnOpen.Size = New System.Drawing.Size(70, 25)
        Me.btnOpen.TabIndex = 62
        Me.btnOpen.Text = "开启托盘"
        Me.btnOpen.UseVisualStyleBackColor = False
        '
        'btnTrayState
        '
        Me.btnTrayState.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.btnTrayState.Location = New System.Drawing.Point(16, 109)
        Me.btnTrayState.Name = "btnTrayState"
        Me.btnTrayState.Size = New System.Drawing.Size(70, 25)
        Me.btnTrayState.TabIndex = 60
        Me.btnTrayState.Text = "托盘状态"
        Me.btnTrayState.UseVisualStyleBackColor = False
        '
        'GroupBox6
        '
        Me.GroupBox6.Location = New System.Drawing.Point(0, 0)
        Me.GroupBox6.Name = "GroupBox6"
        Me.GroupBox6.Size = New System.Drawing.Size(200, 100)
        Me.GroupBox6.TabIndex = 0
        Me.GroupBox6.TabStop = False
        '
        'btn_ChCalibrate
        '
        Me.btn_ChCalibrate.Location = New System.Drawing.Point(0, 0)
        Me.btn_ChCalibrate.Name = "btn_ChCalibrate"
        Me.btn_ChCalibrate.Size = New System.Drawing.Size(75, 23)
        Me.btn_ChCalibrate.TabIndex = 0
        '
        'cmb_ChSelect
        '
        Me.cmb_ChSelect.Location = New System.Drawing.Point(0, 0)
        Me.cmb_ChSelect.Name = "cmb_ChSelect"
        Me.cmb_ChSelect.Size = New System.Drawing.Size(121, 20)
        Me.cmb_ChSelect.TabIndex = 0
        '
        'txtJiajuR
        '
        Me.txtJiajuR.Location = New System.Drawing.Point(0, 0)
        Me.txtJiajuR.Name = "txtJiajuR"
        Me.txtJiajuR.Size = New System.Drawing.Size(100, 21)
        Me.txtJiajuR.TabIndex = 0
        '
        'txtCaliR
        '
        Me.txtCaliR.Location = New System.Drawing.Point(0, 0)
        Me.txtCaliR.Name = "txtCaliR"
        Me.txtCaliR.Size = New System.Drawing.Size(100, 21)
        Me.txtCaliR.TabIndex = 0
        '
        'txtTestR
        '
        Me.txtTestR.Location = New System.Drawing.Point(0, 0)
        Me.txtTestR.Name = "txtTestR"
        Me.txtTestR.Size = New System.Drawing.Size(100, 21)
        Me.txtTestR.TabIndex = 0
        '
        'GroupBox5
        '
        Me.GroupBox5.BackColor = System.Drawing.Color.White
        Me.GroupBox5.Controls.Add(Me.Panel3)
        Me.GroupBox5.Controls.Add(Me.Panel2)
        Me.GroupBox5.Location = New System.Drawing.Point(2, 374)
        Me.GroupBox5.Name = "GroupBox5"
        Me.GroupBox5.Size = New System.Drawing.Size(306, 143)
        Me.GroupBox5.TabIndex = 59
        Me.GroupBox5.TabStop = False
        Me.GroupBox5.Text = "报警指示"
        '
        'Panel3
        '
        Me.Panel3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Panel3.Controls.Add(Me.LabelRun)
        Me.Panel3.Controls.Add(Me.LabelAlarm)
        Me.Panel3.Controls.Add(Me.Label28)
        Me.Panel3.Controls.Add(Me.Label17)
        Me.Panel3.Controls.Add(Me.smokeAlarm1)
        Me.Panel3.Controls.Add(Me.Label19)
        Me.Panel3.Controls.Add(Me.smokeAlarm2)
        Me.Panel3.Location = New System.Drawing.Point(2, 97)
        Me.Panel3.Margin = New System.Windows.Forms.Padding(2)
        Me.Panel3.Name = "Panel3"
        Me.Panel3.Size = New System.Drawing.Size(294, 43)
        Me.Panel3.TabIndex = 57
        '
        'LabelRun
        '
        Me.LabelRun.AutoSize = True
        Me.LabelRun.BackColor = System.Drawing.Color.LimeGreen
        Me.LabelRun.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LabelRun.Font = New System.Drawing.Font("微软雅黑", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.LabelRun.Location = New System.Drawing.Point(219, 10)
        Me.LabelRun.Name = "LabelRun"
        Me.LabelRun.Size = New System.Drawing.Size(62, 19)
        Me.LabelRun.TabIndex = 52
        Me.LabelRun.Text = "  运行中  "
        '
        'LabelAlarm
        '
        Me.LabelAlarm.AutoSize = True
        Me.LabelAlarm.BackColor = System.Drawing.Color.Red
        Me.LabelAlarm.Font = New System.Drawing.Font("微软雅黑", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.LabelAlarm.Location = New System.Drawing.Point(222, 10)
        Me.LabelAlarm.Name = "LabelAlarm"
        Me.LabelAlarm.Size = New System.Drawing.Size(56, 17)
        Me.LabelAlarm.TabIndex = 51
        Me.LabelAlarm.Text = "急停报警"
        '
        'Label28
        '
        Me.Label28.AutoSize = True
        Me.Label28.Location = New System.Drawing.Point(165, 13)
        Me.Label28.Name = "Label28"
        Me.Label28.Size = New System.Drawing.Size(53, 12)
        Me.Label28.TabIndex = 52
        Me.Label28.Text = "设备状态"
        '
        'Label17
        '
        Me.Label17.AutoSize = True
        Me.Label17.Location = New System.Drawing.Point(81, 13)
        Me.Label17.Name = "Label17"
        Me.Label17.Size = New System.Drawing.Size(35, 12)
        Me.Label17.TabIndex = 52
        Me.Label17.Text = "烟感2"
        '
        'smokeAlarm1
        '
        Me.smokeAlarm1.BackColor = System.Drawing.Color.LimeGreen
        Me.smokeAlarm1.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.smokeAlarm1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.smokeAlarm1.Location = New System.Drawing.Point(41, 6)
        Me.smokeAlarm1.Multiline = True
        Me.smokeAlarm1.Name = "smokeAlarm1"
        Me.smokeAlarm1.ReadOnly = True
        Me.smokeAlarm1.Size = New System.Drawing.Size(32, 27)
        Me.smokeAlarm1.TabIndex = 49
        '
        'Label19
        '
        Me.Label19.AutoSize = True
        Me.Label19.Location = New System.Drawing.Point(4, 13)
        Me.Label19.Name = "Label19"
        Me.Label19.Size = New System.Drawing.Size(35, 12)
        Me.Label19.TabIndex = 51
        Me.Label19.Text = "烟感1"
        '
        'smokeAlarm2
        '
        Me.smokeAlarm2.BackColor = System.Drawing.Color.LimeGreen
        Me.smokeAlarm2.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.smokeAlarm2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.smokeAlarm2.Location = New System.Drawing.Point(121, 6)
        Me.smokeAlarm2.Multiline = True
        Me.smokeAlarm2.Name = "smokeAlarm2"
        Me.smokeAlarm2.ReadOnly = True
        Me.smokeAlarm2.Size = New System.Drawing.Size(34, 26)
        Me.smokeAlarm2.TabIndex = 50
        '
        'Panel2
        '
        Me.Panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Panel2.Controls.Add(Me.t4)
        Me.Panel2.Controls.Add(Me.t3)
        Me.Panel2.Controls.Add(Me.t2)
        Me.Panel2.Controls.Add(Me.Label14)
        Me.Panel2.Controls.Add(Me.t1)
        Me.Panel2.Controls.Add(Me.Label16)
        Me.Panel2.Controls.Add(Me.Label1)
        Me.Panel2.Controls.Add(Me.Label15)
        Me.Panel2.Location = New System.Drawing.Point(2, 20)
        Me.Panel2.Margin = New System.Windows.Forms.Padding(2)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(294, 72)
        Me.Panel2.TabIndex = 57
        '
        't4
        '
        Me.t4.BackColor = System.Drawing.Color.White
        Me.t4.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.t4.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.t4.Location = New System.Drawing.Point(199, 43)
        Me.t4.Name = "t4"
        Me.t4.ReadOnly = True
        Me.t4.Size = New System.Drawing.Size(87, 21)
        Me.t4.TabIndex = 53
        '
        't3
        '
        Me.t3.BackColor = System.Drawing.Color.White
        Me.t3.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.t3.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.t3.Location = New System.Drawing.Point(52, 43)
        Me.t3.Name = "t3"
        Me.t3.ReadOnly = True
        Me.t3.Size = New System.Drawing.Size(87, 21)
        Me.t3.TabIndex = 53
        '
        't2
        '
        Me.t2.BackColor = System.Drawing.Color.White
        Me.t2.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.t2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.t2.Location = New System.Drawing.Point(199, 6)
        Me.t2.Name = "t2"
        Me.t2.ReadOnly = True
        Me.t2.Size = New System.Drawing.Size(87, 21)
        Me.t2.TabIndex = 49
        '
        'Label14
        '
        Me.Label14.Location = New System.Drawing.Point(152, 6)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(45, 21)
        Me.Label14.TabIndex = 52
        Me.Label14.Text = "温度2"
        Me.Label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        't1
        '
        Me.t1.BackColor = System.Drawing.Color.White
        Me.t1.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.t1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.t1.Location = New System.Drawing.Point(52, 6)
        Me.t1.Name = "t1"
        Me.t1.ReadOnly = True
        Me.t1.Size = New System.Drawing.Size(87, 21)
        Me.t1.TabIndex = 49
        '
        'Label16
        '
        Me.Label16.Location = New System.Drawing.Point(152, 43)
        Me.Label16.Name = "Label16"
        Me.Label16.Size = New System.Drawing.Size(45, 21)
        Me.Label16.TabIndex = 56
        Me.Label16.Text = "温度4"
        Me.Label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(1, 6)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(45, 21)
        Me.Label1.TabIndex = 51
        Me.Label1.Text = "温度1"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'Label15
        '
        Me.Label15.Location = New System.Drawing.Point(1, 43)
        Me.Label15.Name = "Label15"
        Me.Label15.Size = New System.Drawing.Size(45, 21)
        Me.Label15.TabIndex = 55
        Me.Label15.Text = "温度3"
        Me.Label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'GroupBox7
        '
        Me.GroupBox7.BackColor = System.Drawing.Color.White
        Me.GroupBox7.Controls.Add(Me.txtAlarm)
        Me.GroupBox7.Location = New System.Drawing.Point(2, 585)
        Me.GroupBox7.Margin = New System.Windows.Forms.Padding(2)
        Me.GroupBox7.Name = "GroupBox7"
        Me.GroupBox7.Padding = New System.Windows.Forms.Padding(2)
        Me.GroupBox7.Size = New System.Drawing.Size(306, 123)
        Me.GroupBox7.TabIndex = 64
        Me.GroupBox7.TabStop = False
        Me.GroupBox7.Text = "事件日志"
        '
        'txtAlarm
        '
        Me.txtAlarm.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtAlarm.Location = New System.Drawing.Point(2, 16)
        Me.txtAlarm.Margin = New System.Windows.Forms.Padding(2)
        Me.txtAlarm.Multiline = True
        Me.txtAlarm.Name = "txtAlarm"
        Me.txtAlarm.Size = New System.Drawing.Size(302, 105)
        Me.txtAlarm.TabIndex = 0
        '
        'Button2
        '
        Me.Button2.BackColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.Button2.Font = New System.Drawing.Font("微软雅黑", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Button2.ForeColor = System.Drawing.Color.Black
        Me.Button2.Location = New System.Drawing.Point(95, 713)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(133, 42)
        Me.Button2.TabIndex = 1
        Me.Button2.Text = "报警复位"
        Me.Button2.UseVisualStyleBackColor = False
        '
        'Panel1
        '
        Me.Panel1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Panel1.AutoScroll = True
        Me.Panel1.AutoScrollMinSize = New System.Drawing.Size(0, 0)
        Me.Panel1.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(246, Byte), Integer), CType(CType(250, Byte), Integer))
        Me.Panel1.Controls.Add(Me.Button2)
        Me.Panel1.Controls.Add(Me.GroupBox8)
        Me.Panel1.Controls.Add(Me.GroupBox7)
        Me.Panel1.Controls.Add(Me.GroupBox5)
        Me.Panel1.Controls.Add(Me.GroupBox2)
        Me.Panel1.Controls.Add(Me.GroupBox1)
        Me.Panel1.Controls.Add(Me.grpChannelSelection)
        Me.Panel1.Controls.Add(Me.GroupBox3)
        Me.Panel1.Location = New System.Drawing.Point(12, 12)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(342, 745)
        Me.Panel1.TabIndex = 15
        '
        'GroupBox8
        '
        Me.GroupBox8.BackColor = System.Drawing.Color.White
        Me.GroupBox8.Controls.Add(Me.Label29)
        Me.GroupBox8.Controls.Add(Me.txtRemain)
        Me.GroupBox8.Controls.Add(Me.Label26)
        Me.GroupBox8.Controls.Add(Me.txtUsed)
        Me.GroupBox8.Controls.Add(Me.txtTotle)
        Me.GroupBox8.Location = New System.Drawing.Point(2, 523)
        Me.GroupBox8.Name = "GroupBox8"
        Me.GroupBox8.Size = New System.Drawing.Size(306, 56)
        Me.GroupBox8.TabIndex = 65
        Me.GroupBox8.TabStop = False
        Me.GroupBox8.Text = "探针使用次数统计"
        '
        'Label29
        '
        Me.Label29.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label29.Location = New System.Drawing.Point(238, 22)
        Me.Label29.Name = "Label29"
        Me.Label29.Size = New System.Drawing.Size(25, 21)
        Me.Label29.TabIndex = 56
        Me.Label29.Text = "次"
        Me.Label29.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'txtRemain
        '
        Me.txtRemain.BackColor = System.Drawing.Color.White
        Me.txtRemain.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtRemain.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.txtRemain.Location = New System.Drawing.Point(125, 22)
        Me.txtRemain.Name = "txtRemain"
        Me.txtRemain.ReadOnly = True
        Me.txtRemain.Size = New System.Drawing.Size(107, 21)
        Me.txtRemain.TabIndex = 53
        '
        'Label26
        '
        Me.Label26.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label26.Location = New System.Drawing.Point(10, 22)
        Me.Label26.Name = "Label26"
        Me.Label26.Size = New System.Drawing.Size(105, 21)
        Me.Label26.TabIndex = 55
        Me.Label26.Text = "剩下使用次数 ="
        Me.Label26.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'txtUsed
        '
        Me.txtUsed.BackColor = System.Drawing.Color.White
        Me.txtUsed.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtUsed.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.txtUsed.Location = New System.Drawing.Point(124, 22)
        Me.txtUsed.Name = "txtUsed"
        Me.txtUsed.ReadOnly = True
        Me.txtUsed.Size = New System.Drawing.Size(10, 21)
        Me.txtUsed.TabIndex = 53
        '
        'txtTotle
        '
        Me.txtTotle.BackColor = System.Drawing.Color.White
        Me.txtTotle.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtTotle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.txtTotle.Location = New System.Drawing.Point(125, 22)
        Me.txtTotle.Name = "txtTotle"
        Me.txtTotle.ReadOnly = True
        Me.txtTotle.Size = New System.Drawing.Size(10, 21)
        Me.txtTotle.TabIndex = 53
        Me.txtTotle.Text = "30000"
        '
        'channelGrid
        '
        Me.channelGrid.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.channelGrid.BackColor = System.Drawing.Color.White
        Me.channelGrid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.channelGrid.Location = New System.Drawing.Point(392, 116)
        Me.channelGrid.Name = "channelGrid"
        Me.channelGrid.Padding = New System.Windows.Forms.Padding(10, 8, 10, 10)
        Me.channelGrid.SelectionMode = software.ChannelSelectionMode.SingleChannel
        Me.channelGrid.Size = New System.Drawing.Size(1060, 774)
        Me.channelGrid.TabIndex = 71
        '
        'statisticsControl
        '
        Me.statisticsControl.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.statisticsControl.BackColor = System.Drawing.Color.White
        Me.statisticsControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.statisticsControl.Location = New System.Drawing.Point(392, 12)
        Me.statisticsControl.Name = "statisticsControl"
        Me.statisticsControl.Padding = New System.Windows.Forms.Padding(16, 8, 16, 8)
        Me.statisticsControl.Size = New System.Drawing.Size(1060, 96)
        Me.statisticsControl.TabIndex = 70
        '
        'Label2
        '
        Me.Label2.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label2.Location = New System.Drawing.Point(-6, 18)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(54, 21)
        Me.Label2.TabIndex = 47
        Me.Label2.Text = "托盘码"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'frmTest
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(246, Byte), Integer), CType(CType(250, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(1464, 902)
        Me.Controls.Add(Me.channelGrid)
        Me.Controls.Add(Me.statisticsControl)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.Pic_Bat)
        Me.Name = "frmTest"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "测试"
        CType(Me.Pic_Bat, System.ComponentModel.ISupportInitialize).EndInit()
        BuildDesignerLayout()
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.grpChannelSelection.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.GroupBox5.ResumeLayout(False)
        Me.Panel3.ResumeLayout(False)
        Me.Panel3.PerformLayout()
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        Me.GroupBox7.ResumeLayout(False)
        Me.GroupBox7.PerformLayout()
        Me.Panel1.ResumeLayout(False)
        Me.GroupBox8.ResumeLayout(False)
        Me.GroupBox8.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Private Sub BuildDesignerLayout()
        Me.rootLayout = New TableLayoutPanel With {
            .BackColor = UiTheme.PageBackground,
            .ColumnCount = 2,
            .Dock = DockStyle.Fill,
            .Margin = New Padding(0),
            .Padding = New Padding(0),
            .RowCount = 1
        }
        Me.rootLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 350.0F))
        Me.rootLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        Me.rootLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))

        Me.rightLayout = New TableLayoutPanel With {
            .BackColor = UiTheme.PageBackground,
            .ColumnCount = 1,
            .Dock = DockStyle.Fill,
            .Margin = New Padding(0),
            .Padding = New Padding(0),
            .RowCount = 2
        }
        Me.rightLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        Me.rightLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 14.0F))
        Me.rightLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 86.0F))
        Me.statisticsControl.Dock = DockStyle.Fill
        Me.statisticsControl.Margin = New Padding(0)
        Me.channelGrid.Dock = DockStyle.Fill
        Me.channelGrid.Margin = New Padding(0)
        Me.rightLayout.Controls.Add(Me.statisticsControl, 0, 0)
        Me.rightLayout.Controls.Add(Me.channelGrid, 0, 1)

        Me.leftContentPanel = New TableLayoutPanel With {
            .AutoSize = True,
            .AutoSizeMode = AutoSizeMode.GrowAndShrink,
            .BackColor = UiTheme.PageBackground,
            .ColumnCount = 1,
            .Dock = DockStyle.Top,
            .Margin = New Padding(0),
            .Padding = New Padding(UiTheme.LeftContentPadding),
            .RowCount = 0
        }

        Me.grpTestMode = New GroupBox With {
            .BackColor = UiTheme.CardBackground,
            .Font = UiTheme.UiFontBold,
            .ForeColor = UiTheme.Primary,
            .Name = "grpTestMode",
            .Padding = New Padding(6, 15, 6, 6),
            .Size = New Size(334, 50),
            .TabStop = False,
            .Text = "测试控制"
        }
        Me.testModeLayout = New TableLayoutPanel With {
            .ColumnCount = 2,
            .Dock = DockStyle.Fill,
            .Margin = New Padding(0),
            .RowCount = 1
        }
        Me.testModeLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        Me.testModeLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        Me.chkAutoTest = New CheckBox With {
            .Appearance = Appearance.Button,
            .AutoSize = False,
            .BackColor = UiTheme.Primary,
            .Dock = DockStyle.Fill,
            .FlatStyle = FlatStyle.Flat,
            .Font = UiTheme.UiFontBold,
            .ForeColor = Color.White,
            .Name = "chkAutoTest",
            .Text = "自动",
            .TextAlign = ContentAlignment.MiddleCenter,
            .UseVisualStyleBackColor = False
        }
        Me.chkAutoTest.FlatAppearance.BorderColor = UiTheme.Primary
        Me.chkAutoTest.FlatAppearance.BorderSize = 1
        Me.chkManualTest = New CheckBox With {
            .Appearance = Appearance.Button,
            .AutoSize = False,
            .BackColor = UiTheme.CardBackground,
            .Dock = DockStyle.Fill,
            .FlatStyle = FlatStyle.Flat,
            .Font = UiTheme.UiFontBold,
            .ForeColor = UiTheme.TextPrimary,
            .Name = "chkManualTest",
            .Text = "手动",
            .TextAlign = ContentAlignment.MiddleCenter,
            .UseVisualStyleBackColor = False
        }
        Me.chkManualTest.FlatAppearance.BorderColor = UiTheme.CardBorder
        Me.chkManualTest.FlatAppearance.BorderSize = 1
        Me.chkAutoTest.Checked = True
        Me.testModeLayout.Controls.Add(Me.chkAutoTest, 0, 0)
        Me.testModeLayout.Controls.Add(Me.chkManualTest, 1, 0)
        Me.grpTestMode.Controls.Add(Me.testModeLayout)
        AddHandler Me.chkAutoTest.CheckedChanged, AddressOf TestModeCheckedChanged
        AddHandler Me.chkManualTest.CheckedChanged, AddressOf TestModeCheckedChanged

        Me.currentStationButton = New RadioButton With {
            .Appearance = Appearance.Button,
            .AutoSize = False,
            .BackColor = UiTheme.Primary,
            .Dock = DockStyle.Top,
            .FlatStyle = FlatStyle.Flat,
            .Font = UiTheme.UiFontTitle,
            .ForeColor = Color.White,
            .Height = UiTheme.LeftStationButtonHeight,
            .Name = "currentStationButton",
            .Text = "OCV1",
            .TextAlign = ContentAlignment.MiddleCenter,
            .UseVisualStyleBackColor = False
        }
        Me.currentStationButton.FlatAppearance.BorderColor = UiTheme.Primary
        Me.currentStationButton.FlatAppearance.BorderSize = 1

        ConfigureDesignerCard(Me.GroupBox1)
        ConfigureDesignerCard(Me.GroupBox3)
        ConfigureDesignerCard(Me.GroupBox5)
        ConfigureDesignerCard(Me.GroupBox7)
        ConfigureDesignerCard(Me.GroupBox8)
        ConfigureDesignerCard(Me.grpChannelSelection)
        Me.GroupBox2.Visible = False
        Me.GroupBox2.Enabled = False
        Me.GroupBox6.Visible = False
        Me.txtUsed.Visible = False
        Me.txtTotle.Visible = False
        ConfigureDesignerInput(Me.txtTray)
        ConfigureDesignerInput(Me.txtFirstNum)
        ConfigureDesignerInput(Me.txtLastNum)
        ConfigureDesignerInput(Me.txtMinV)
        ConfigureDesignerInput(Me.txtMaxV)
        ConfigureDesignerInput(Me.txtMinR)
        ConfigureDesignerInput(Me.txtMaxR)
        ConfigureDesignerInput(Me.txtTestR)
        ConfigureDesignerInput(Me.txtCaliR)
        ConfigureDesignerInput(Me.txtJiajuR)
        ConfigureDesignerInput(Me.txtAlarm)
        ConfigureDesignerInput(Me.t1)
        ConfigureDesignerInput(Me.t2)
        ConfigureDesignerInput(Me.t3)
        ConfigureDesignerInput(Me.t4)
        ConfigureDesignerInput(Me.txtRemain)
        For Each label As Label In {Me.Label1, Me.Label2, Me.Label5, Me.Label7, Me.Label12, Me.Label14, Me.Label15, Me.Label16, Me.Label17, Me.Label19, Me.Label26, Me.Label28, Me.Label29}
            ConfigureDesignerLabel(label)
        Next
        ConfigureDesignerButton(Me.CmdStart, UiTheme.Primary, Color.White, UiTheme.Primary)
        ConfigureDesignerButton(Me.btnStop, Color.White, UiTheme.Danger, UiTheme.Danger)
        ConfigureDesignerButton(Me.btnNGTest, UiTheme.Warning, Color.White, UiTheme.Warning)
        ConfigureDesignerButton(Me.btnTrayState, Color.White, UiTheme.TextPrimary, UiTheme.CardBorder)
        ConfigureDesignerButton(Me.btnOpen, Color.White, UiTheme.TextPrimary, UiTheme.CardBorder)
        ConfigureDesignerButton(Me.btnClose, Color.White, UiTheme.TextPrimary, UiTheme.CardBorder)
        ConfigureDesignerButton(Me.btnChannelSelectAll, UiTheme.Primary, Color.White, UiTheme.Primary)
        ConfigureDesignerButton(Me.btnChannelClear, Color.White, UiTheme.TextPrimary, UiTheme.CardBorder)
        ConfigureDesignerButton(Me.btnChannelRectangle, Color.White, UiTheme.TextPrimary, UiTheme.CardBorder)
        ConfigureDesignerButton(Me.btnChannelSelectSingle, Color.White, UiTheme.TextPrimary, UiTheme.CardBorder)
        ConfigureDesignerButton(Me.Button2, Color.White, UiTheme.Danger, UiTheme.Danger)

        BuildDesignerTestModeLayout()
        BuildDesignerControlBoardLayout()
        BuildDesignerChannelSelectionLayout()
        BuildDesignerRangeLayout()
        BuildDesignerAlarmLayout()
        BuildDesignerProbeUsageLayout()
        BuildDesignerEventLogLayout()

        Me.resetAlarmLayout = New TableLayoutPanel With {
            .ColumnCount = 1,
            .Dock = DockStyle.Top,
            .Margin = New Padding(0),
            .RowCount = 1,
            .Size = New Size(334, 38)
        }
        Me.resetAlarmLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        Me.resetAlarmLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 34.0F))
        Me.Button2.Dock = DockStyle.None
        Me.Button2.Anchor = AnchorStyles.None
        Me.Button2.Margin = New Padding(0)
        Me.Button2.Size = New Size(133, 34)
        Me.resetAlarmLayout.Controls.Add(Me.Button2, 0, 0)

        AddLeftContentItem(Me.currentStationButton)
        AddLeftContentItem(Me.grpTestMode)
        AddLeftContentItem(Me.GroupBox1)
        AddLeftContentItem(Me.grpChannelSelection)
        AddLeftContentItem(Me.GroupBox3)
        AddLeftContentItem(Me.GroupBox5)
        AddLeftContentItem(Me.GroupBox8)
        AddLeftContentItem(Me.GroupBox7)
        AddLeftContentItem(Me.resetAlarmLayout)
        Me.Panel1.Controls.Clear()
        Me.Panel1.BackColor = UiTheme.PageBackground
        Me.Panel1.AutoScroll = True
        Me.Panel1.Dock = DockStyle.Fill
        Me.Panel1.Margin = New Padding(0)
        Me.Panel1.Padding = New Padding(0)
        Me.Panel1.Controls.Add(Me.GroupBox2)
        Me.Panel1.Controls.Add(Me.leftContentPanel)
        Me.leftContentPanel.BringToFront()
        Me.Controls.Clear()
        Me.Pic_Bat.Visible = False
        Me.Controls.Add(Me.Pic_Bat)
        Me.Controls.Add(Me.rootLayout)
        Me.rootLayout.Controls.Add(Me.Panel1, 0, 0)
        Me.rootLayout.Controls.Add(Me.rightLayout, 1, 0)
    End Sub

    Private Sub AddLeftContentItem(ByVal control As Control)
        control.Dock = DockStyle.Top
        control.Margin = New Padding(0, 0, 0, UiTheme.LeftSectionGap)
        Me.leftContentPanel.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        Me.leftContentPanel.Controls.Add(control, 0, Me.leftContentPanel.RowCount)
        Me.leftContentPanel.RowCount += 1
    End Sub

    Private Sub ConfigureDesignerCard(ByVal card As GroupBox)
        card.AutoSize = True
        card.AutoSizeMode = AutoSizeMode.GrowAndShrink
        card.BackColor = UiTheme.CardBackground
        card.Dock = DockStyle.Top
        card.FlatStyle = FlatStyle.Flat
        card.Font = UiTheme.UiFontBold
        card.ForeColor = UiTheme.Primary
        card.Margin = New Padding(0)
        card.MinimumSize = New Size(0, 0)
        card.Padding = New Padding(6, 15, 6, 6)
    End Sub

    Private Sub ConfigureDesignerInput(ByVal input As TextBox)
        input.BackColor = UiTheme.CardBackground
        input.BorderStyle = BorderStyle.FixedSingle
        input.Font = UiTheme.UiFont
        input.ForeColor = UiTheme.TextPrimary
    End Sub

    Private Sub ConfigureDesignerLabel(ByVal label As Label)
        label.BackColor = UiTheme.CardBackground
        label.Font = UiTheme.UiFont
        label.ForeColor = UiTheme.TextPrimary
    End Sub

    Private Sub ConfigureDesignerButton(ByVal button As Button, ByVal backColor As Color, ByVal foreColor As Color, ByVal borderColor As Color)
        button.BackColor = backColor
        button.FlatStyle = FlatStyle.Flat
        button.Font = UiTheme.UiFontBold
        button.ForeColor = foreColor
        button.UseVisualStyleBackColor = False
        button.FlatAppearance.BorderColor = borderColor
        button.FlatAppearance.BorderSize = 1
    End Sub

    Private Sub ConfigureDesignerTable(ByVal table As TableLayoutPanel, ByVal columns As Integer, ByVal rows As Integer)
        table.AutoSize = True
        table.AutoSizeMode = AutoSizeMode.GrowAndShrink
        table.BackColor = UiTheme.CardBackground
        table.ColumnCount = columns
        table.Dock = DockStyle.Top
        table.Margin = New Padding(0)
        table.Padding = New Padding(0)
        table.RowCount = rows
        For index As Integer = 0 To columns - 1
            table.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F / columns))
        Next
        For index As Integer = 0 To rows - 1
            table.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        Next
    End Sub

    Private Sub PutDesignerControl(ByVal parent As Control, ByVal table As TableLayoutPanel, ByVal control As Control, ByVal column As Integer, ByVal row As Integer, Optional ByVal columnSpan As Integer = 1)
        parent.Controls.Remove(control)
        control.Dock = DockStyle.Fill
        control.Margin = New Padding(2)
        table.Controls.Add(control, column, row)
        If columnSpan > 1 Then table.SetColumnSpan(control, columnSpan)
    End Sub

    Private Sub BuildDesignerTestModeLayout()
        Me.testModeLayout.ColumnStyles.Clear()
        Me.testModeLayout.RowStyles.Clear()
        Me.testModeLayout.ColumnCount = 2
        Me.testModeLayout.RowCount = 1
        Me.testModeLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        Me.testModeLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        Me.testModeLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
    End Sub

    Private Sub BuildDesignerControlBoardLayout()
        Me.controlBoardLayout = New TableLayoutPanel()
        ConfigureDesignerTable(Me.controlBoardLayout, 3, 4)
        PutDesignerControl(Me.GroupBox1, Me.controlBoardLayout, Me.Label2, 0, 0)
        PutDesignerControl(Me.GroupBox1, Me.controlBoardLayout, Me.txtTray, 1, 0, 2)
        PutDesignerControl(Me.GroupBox1, Me.controlBoardLayout, Me.Label5, 0, 1)
        PutDesignerControl(Me.GroupBox1, Me.controlBoardLayout, Me.lblBarNum, 1, 1)
        PutDesignerControl(Me.GroupBox1, Me.controlBoardLayout, Me.CmdStart, 0, 2)
        PutDesignerControl(Me.GroupBox1, Me.controlBoardLayout, Me.btnStop, 1, 2)
        PutDesignerControl(Me.GroupBox1, Me.controlBoardLayout, Me.btnNGTest, 2, 2)
        PutDesignerControl(Me.GroupBox1, Me.controlBoardLayout, Me.btnTrayState, 0, 3)
        PutDesignerControl(Me.GroupBox1, Me.controlBoardLayout, Me.btnOpen, 1, 3)
        PutDesignerControl(Me.GroupBox1, Me.controlBoardLayout, Me.btnClose, 2, 3)
        Me.Label2.TextAlign = ContentAlignment.MiddleRight
        Me.Label5.TextAlign = ContentAlignment.MiddleRight
        Me.GroupBox1.Controls.Add(Me.controlBoardLayout)
    End Sub

    Private Sub BuildDesignerChannelSelectionLayout()
        Me.channelSelectionLayout = New TableLayoutPanel()
        ConfigureDesignerTable(Me.channelSelectionLayout, 4, 1)
        PutDesignerControl(Me.grpChannelSelection, Me.channelSelectionLayout, Me.btnChannelSelectAll, 0, 0)
        PutDesignerControl(Me.grpChannelSelection, Me.channelSelectionLayout, Me.btnChannelClear, 1, 0)
        PutDesignerControl(Me.grpChannelSelection, Me.channelSelectionLayout, Me.btnChannelRectangle, 2, 0)
        PutDesignerControl(Me.grpChannelSelection, Me.channelSelectionLayout, Me.btnChannelSelectSingle, 3, 0)
        Me.grpChannelSelection.Controls.Add(Me.channelSelectionLayout)
    End Sub

    Private Sub BuildDesignerRangeLayout()
        Me.rangeSettingsLayout = New TableLayoutPanel()
        ConfigureDesignerTable(Me.rangeSettingsLayout, 3, 3)
        Me.rangeSettingsLayout.ColumnStyles.Clear()
        Me.rangeSettingsLayout.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        Me.rangeSettingsLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        Me.rangeSettingsLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        PutDesignerControl(Me.GroupBox3, Me.rangeSettingsLayout, Me.Label9, 1, 0)
        PutDesignerControl(Me.GroupBox3, Me.rangeSettingsLayout, Me.Label8, 2, 0)
        PutDesignerControl(Me.GroupBox3, Me.rangeSettingsLayout, Me.Label7, 0, 1)
        PutDesignerControl(Me.GroupBox3, Me.rangeSettingsLayout, Me.txtMinV, 1, 1)
        PutDesignerControl(Me.GroupBox3, Me.rangeSettingsLayout, Me.txtMaxV, 2, 1)
        PutDesignerControl(Me.GroupBox3, Me.rangeSettingsLayout, Me.Label12, 0, 2)
        PutDesignerControl(Me.GroupBox3, Me.rangeSettingsLayout, Me.txtMinR, 1, 2)
        PutDesignerControl(Me.GroupBox3, Me.rangeSettingsLayout, Me.txtMaxR, 2, 2)
        Me.Label7.TextAlign = ContentAlignment.MiddleRight
        Me.Label12.TextAlign = ContentAlignment.MiddleRight
        Me.Label9.TextAlign = ContentAlignment.MiddleCenter
        Me.Label8.TextAlign = ContentAlignment.MiddleCenter
        Me.GroupBox3.Controls.Add(Me.rangeSettingsLayout)
    End Sub

    Private Sub BuildDesignerAlarmLayout()
        Me.alarmSectionLayout = New TableLayoutPanel()
        ConfigureDesignerTable(Me.alarmSectionLayout, 1, 2)
        Me.Panel2.BorderStyle = BorderStyle.None
        Me.Panel2.AutoSize = True
        Me.Panel2.AutoSizeMode = AutoSizeMode.GrowAndShrink
        Me.Panel2.Margin = New Padding(0)
        Me.Panel2.Padding = New Padding(0)
        Me.Panel3.BorderStyle = BorderStyle.None
        Me.Panel3.AutoSize = True
        Me.Panel3.AutoSizeMode = AutoSizeMode.GrowAndShrink
        Me.Panel3.Margin = New Padding(0, 4, 0, 0)
        Me.Panel3.Padding = New Padding(0)
        PutDesignerControl(Me.GroupBox5, Me.alarmSectionLayout, Me.Panel2, 0, 0)
        PutDesignerControl(Me.GroupBox5, Me.alarmSectionLayout, Me.Panel3, 0, 1)
        BuildDesignerTemperatureLayout()
        BuildDesignerAlarmStatusLayout()
        Me.GroupBox5.Controls.Add(Me.alarmSectionLayout)
    End Sub

    Private Sub BuildDesignerTemperatureLayout()
        Me.alarmTemperatureLayout = New TableLayoutPanel()
        ConfigureDesignerTable(Me.alarmTemperatureLayout, 4, 2)
        PutDesignerControl(Me.Panel2, Me.alarmTemperatureLayout, Me.Label1, 0, 0)
        PutDesignerControl(Me.Panel2, Me.alarmTemperatureLayout, Me.t1, 1, 0)
        PutDesignerControl(Me.Panel2, Me.alarmTemperatureLayout, Me.Label14, 2, 0)
        PutDesignerControl(Me.Panel2, Me.alarmTemperatureLayout, Me.t2, 3, 0)
        PutDesignerControl(Me.Panel2, Me.alarmTemperatureLayout, Me.Label15, 0, 1)
        PutDesignerControl(Me.Panel2, Me.alarmTemperatureLayout, Me.t3, 1, 1)
        PutDesignerControl(Me.Panel2, Me.alarmTemperatureLayout, Me.Label16, 2, 1)
        PutDesignerControl(Me.Panel2, Me.alarmTemperatureLayout, Me.t4, 3, 1)
        Me.Panel2.Controls.Add(Me.alarmTemperatureLayout)
    End Sub

    Private Sub BuildDesignerAlarmStatusLayout()
        Me.alarmStatusLayout = New TableLayoutPanel()
        ConfigureDesignerTable(Me.alarmStatusLayout, 3, 1)
        Me.alarmStatusLayout.ColumnStyles.Clear()
        Me.alarmStatusLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 24.0F))
        Me.alarmStatusLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 24.0F))
        Me.alarmStatusLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 52.0F))
        Me.smokeOneLayout = New FlowLayoutPanel With {.AutoSize = True, .AutoSizeMode = AutoSizeMode.GrowAndShrink, .Dock = DockStyle.Fill, .FlowDirection = FlowDirection.LeftToRight, .Margin = New Padding(2), .Padding = New Padding(0), .WrapContents = False}
        Me.smokeTwoLayout = New FlowLayoutPanel With {.AutoSize = True, .AutoSizeMode = AutoSizeMode.GrowAndShrink, .Dock = DockStyle.Fill, .FlowDirection = FlowDirection.LeftToRight, .Margin = New Padding(2), .Padding = New Padding(0), .WrapContents = False}
        Me.deviceStatusLayout = New FlowLayoutPanel With {.AutoSize = True, .AutoSizeMode = AutoSizeMode.GrowAndShrink, .Dock = DockStyle.Fill, .FlowDirection = FlowDirection.LeftToRight, .Margin = New Padding(2), .Padding = New Padding(0), .WrapContents = False}
        Me.Label19.Margin = New Padding(2)
        Me.smokeAlarm1.Margin = New Padding(2)
        Me.Label17.Margin = New Padding(2)
        Me.smokeAlarm2.Margin = New Padding(2)
        Me.Label28.Margin = New Padding(2)
        Me.LabelRun.Margin = New Padding(2)
        Me.LabelAlarm.Margin = New Padding(2)
        Me.smokeOneLayout.Controls.Add(Me.Label19)
        Me.smokeOneLayout.Controls.Add(Me.smokeAlarm1)
        Me.smokeTwoLayout.Controls.Add(Me.Label17)
        Me.smokeTwoLayout.Controls.Add(Me.smokeAlarm2)
        Me.deviceStatusLayout.Controls.Add(Me.Label28)
        Me.deviceStatusLayout.Controls.Add(Me.LabelRun)
        Me.deviceStatusLayout.Controls.Add(Me.LabelAlarm)
        Me.alarmStatusLayout.Controls.Add(Me.smokeOneLayout, 0, 0)
        Me.alarmStatusLayout.Controls.Add(Me.smokeTwoLayout, 1, 0)
        Me.alarmStatusLayout.Controls.Add(Me.deviceStatusLayout, 2, 0)
        Me.Panel3.Controls.Add(Me.alarmStatusLayout)
    End Sub

    Private Sub BuildDesignerProbeUsageLayout()
        Me.probeUsageLayout = New TableLayoutPanel()
        ConfigureDesignerTable(Me.probeUsageLayout, 3, 1)
        Me.probeUsageLayout.ColumnStyles.Clear()
        Me.probeUsageLayout.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        Me.probeUsageLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        Me.probeUsageLayout.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        PutDesignerControl(Me.GroupBox8, Me.probeUsageLayout, Me.Label26, 0, 0)
        PutDesignerControl(Me.GroupBox8, Me.probeUsageLayout, Me.txtRemain, 1, 0)
        PutDesignerControl(Me.GroupBox8, Me.probeUsageLayout, Me.Label29, 2, 0)
        Me.Label26.TextAlign = ContentAlignment.MiddleRight
        Me.Label29.TextAlign = ContentAlignment.MiddleLeft
        Me.GroupBox8.Controls.Add(Me.probeUsageLayout)
    End Sub

    Private Sub BuildDesignerEventLogLayout()
        Me.GroupBox7.AutoSize = False
        Me.GroupBox7.Height = 82
        Me.GroupBox7.MinimumSize = New Size(0, 82)
        Me.txtAlarm.Dock = DockStyle.Fill
        Me.txtAlarm.Margin = New Padding(0)
    End Sub
    Private rootLayout As TableLayoutPanel
    Private rightLayout As TableLayoutPanel
    Private leftContentPanel As TableLayoutPanel
    Private resetAlarmLayout As TableLayoutPanel
    Private testModeLayout As TableLayoutPanel
    Private controlBoardLayout As TableLayoutPanel
    Private channelSelectionLayout As TableLayoutPanel
    Private rangeSettingsLayout As TableLayoutPanel
    Private alarmSectionLayout As TableLayoutPanel
    Private alarmTemperatureLayout As TableLayoutPanel
    Private alarmStatusLayout As TableLayoutPanel
    Private probeUsageLayout As TableLayoutPanel
    Private smokeOneLayout As FlowLayoutPanel
    Private smokeTwoLayout As FlowLayoutPanel
    Private deviceStatusLayout As FlowLayoutPanel
    Private grpTestMode As GroupBox
    Private chkAutoTest As CheckBox
    Private chkManualTest As CheckBox
    Private currentStationButton As RadioButton
    Friend WithEvents Pic_Bat As System.Windows.Forms.PictureBox
    Friend WithEvents SaveFileDialog1 As System.Windows.Forms.SaveFileDialog
    Friend WithEvents Timer1 As Timer
    Friend WithEvents Timer2 As Timer
    Friend WithEvents Timer4 As Timer
    Friend WithEvents statisticsControl As StatisticsControl
    Friend WithEvents channelGrid As ChannelGridControl
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents Label12 As Label
    Friend WithEvents txtMinR As TextBox
    Friend WithEvents txtMaxR As TextBox
    Friend WithEvents Label9 As Label
    Friend WithEvents Label8 As Label
    Friend WithEvents txtMinV As TextBox
    Friend WithEvents txtMaxV As TextBox
    Friend WithEvents Label7 As Label
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents grpChannelSelection As GroupBox
    Friend WithEvents btnChannelSelectAll As Button
    Friend WithEvents btnChannelClear As Button
    Friend WithEvents btnChannelRectangle As Button
    Friend WithEvents btnChannelSelectSingle As Button
    Friend WithEvents btnStop As Button
    Friend WithEvents btnNGTest As Button
    Friend WithEvents CmdStart As Button
    Friend WithEvents lblBarNum As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents txtLastNum As TextBox
    Friend WithEvents txtFirstNum As TextBox
    Friend WithEvents txtTray As TextBox
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents Rbtn1 As RadioButton
    Friend WithEvents CmdBatInput As Button
    Friend WithEvents cmbShow As ComboBox
    Friend WithEvents Rbtn3 As RadioButton
    Friend WithEvents Rbtn2 As RadioButton
    Friend WithEvents btnClose As Button
    Friend WithEvents GroupBox6 As GroupBox
    Friend WithEvents btn_ChCalibrate As Button
    Friend WithEvents cmb_ChSelect As ComboBox
    Friend WithEvents txtJiajuR As TextBox
    Friend WithEvents txtCaliR As TextBox
    Friend WithEvents txtTestR As TextBox
    Friend WithEvents btnOpen As Button
    Friend WithEvents btnTrayState As Button
    Friend WithEvents GroupBox5 As GroupBox
    Friend WithEvents Panel3 As Panel
    Friend WithEvents LabelRun As Label
    Friend WithEvents LabelAlarm As Label
    Friend WithEvents Label28 As Label
    Friend WithEvents Label17 As Label
    Friend WithEvents smokeAlarm1 As TextBox
    Friend WithEvents Label19 As Label
    Friend WithEvents smokeAlarm2 As TextBox
    Friend WithEvents Panel2 As Panel
    Friend WithEvents t4 As TextBox
    Friend WithEvents t3 As TextBox
    Friend WithEvents t2 As TextBox
    Friend WithEvents Label14 As Label
    Friend WithEvents t1 As TextBox
    Friend WithEvents Label16 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents Label15 As Label
    Friend WithEvents GroupBox7 As GroupBox
    Friend WithEvents Button2 As Button
    Friend WithEvents txtAlarm As TextBox
    Friend WithEvents Panel1 As Panel
    Friend WithEvents GroupBox8 As GroupBox
    Friend WithEvents Label29 As Label
    Friend WithEvents txtRemain As TextBox
    Friend WithEvents Label26 As Label
    Friend WithEvents txtUsed As TextBox
    Friend WithEvents txtTotle As TextBox
    Friend WithEvents Label5 As Label
    Friend WithEvents Label2 As Label
End Class
