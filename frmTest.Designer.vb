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
        Me.rangeSettingsLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.txtMinV = New System.Windows.Forms.TextBox()
        Me.txtMaxV = New System.Windows.Forms.TextBox()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.txtMinR = New System.Windows.Forms.TextBox()
        Me.txtMaxR = New System.Windows.Forms.TextBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.controlBoardLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtTray = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.lblBarNum = New System.Windows.Forms.Label()
        Me.CmdStart = New System.Windows.Forms.Button()
        Me.btnStop = New System.Windows.Forms.Button()
        Me.btnNGTest = New System.Windows.Forms.Button()
        Me.btnTrayState = New System.Windows.Forms.Button()
        Me.btnOpen = New System.Windows.Forms.Button()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.txtLastNum = New System.Windows.Forms.TextBox()
        Me.txtFirstNum = New System.Windows.Forms.TextBox()
        Me.grpChannelSelection = New System.Windows.Forms.GroupBox()
        Me.channelSelectionLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.btnChannelSelectAll = New System.Windows.Forms.Button()
        Me.btnChannelClear = New System.Windows.Forms.Button()
        Me.btnChannelRectangle = New System.Windows.Forms.Button()
        Me.btnChannelSelectSingle = New System.Windows.Forms.Button()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.Rbtn1 = New System.Windows.Forms.RadioButton()
        Me.CmdBatInput = New System.Windows.Forms.Button()
        Me.cmbShow = New System.Windows.Forms.ComboBox()
        Me.Rbtn3 = New System.Windows.Forms.RadioButton()
        Me.Rbtn2 = New System.Windows.Forms.RadioButton()
        Me.GroupBox6 = New System.Windows.Forms.GroupBox()
        Me.btn_ChCalibrate = New System.Windows.Forms.Button()
        Me.cmb_ChSelect = New System.Windows.Forms.ComboBox()
        Me.txtJiajuR = New System.Windows.Forms.TextBox()
        Me.txtCaliR = New System.Windows.Forms.TextBox()
        Me.txtTestR = New System.Windows.Forms.TextBox()
        Me.GroupBox5 = New System.Windows.Forms.GroupBox()
        Me.alarmSectionLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.alarmTemperatureLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.t1 = New System.Windows.Forms.TextBox()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.t2 = New System.Windows.Forms.TextBox()
        Me.Label15 = New System.Windows.Forms.Label()
        Me.t3 = New System.Windows.Forms.TextBox()
        Me.Label16 = New System.Windows.Forms.Label()
        Me.t4 = New System.Windows.Forms.TextBox()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.alarmStatusLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.smokeOneLayout = New System.Windows.Forms.FlowLayoutPanel()
        Me.Label19 = New System.Windows.Forms.Label()
        Me.smokeAlarm1 = New System.Windows.Forms.TextBox()
        Me.smokeTwoLayout = New System.Windows.Forms.FlowLayoutPanel()
        Me.Label17 = New System.Windows.Forms.Label()
        Me.smokeAlarm2 = New System.Windows.Forms.TextBox()
        Me.deviceStatusLayout = New System.Windows.Forms.FlowLayoutPanel()
        Me.Label28 = New System.Windows.Forms.Label()
        Me.LabelRun = New System.Windows.Forms.Label()
        Me.LabelAlarm = New System.Windows.Forms.Label()
        Me.GroupBox7 = New System.Windows.Forms.GroupBox()
        Me.txtAlarm = New System.Windows.Forms.TextBox()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.txtUsed = New System.Windows.Forms.TextBox()
        Me.leftContentPanel = New System.Windows.Forms.TableLayoutPanel()
        Me.currentStationButton = New System.Windows.Forms.RadioButton()
        Me.grpTestMode = New System.Windows.Forms.GroupBox()
        Me.testModeLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.chkAutoTest = New System.Windows.Forms.CheckBox()
        Me.chkManualTest = New System.Windows.Forms.CheckBox()
        Me.GroupBox8 = New System.Windows.Forms.GroupBox()
        Me.probeUsageLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.Label26 = New System.Windows.Forms.Label()
        Me.txtRemain = New System.Windows.Forms.TextBox()
        Me.Label29 = New System.Windows.Forms.Label()
        Me.resetAlarmLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.txtTotle = New System.Windows.Forms.TextBox()
        Me.channelGrid = New software.ChannelGridControl()
        Me.statisticsControl = New software.StatisticsControl()
        Me.rootLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.rightLayout = New System.Windows.Forms.TableLayoutPanel()
        CType(Me.Pic_Bat, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox3.SuspendLayout()
        Me.rangeSettingsLayout.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.controlBoardLayout.SuspendLayout()
        Me.grpChannelSelection.SuspendLayout()
        Me.channelSelectionLayout.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox5.SuspendLayout()
        Me.alarmSectionLayout.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.alarmTemperatureLayout.SuspendLayout()
        Me.Panel3.SuspendLayout()
        Me.alarmStatusLayout.SuspendLayout()
        Me.smokeOneLayout.SuspendLayout()
        Me.smokeTwoLayout.SuspendLayout()
        Me.deviceStatusLayout.SuspendLayout()
        Me.GroupBox7.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.leftContentPanel.SuspendLayout()
        Me.grpTestMode.SuspendLayout()
        Me.testModeLayout.SuspendLayout()
        Me.GroupBox8.SuspendLayout()
        Me.probeUsageLayout.SuspendLayout()
        Me.resetAlarmLayout.SuspendLayout()
        Me.rootLayout.SuspendLayout()
        Me.rightLayout.SuspendLayout()
        Me.SuspendLayout()
        '
        'Pic_Bat
        '
        Me.Pic_Bat.BackColor = System.Drawing.Color.White
        Me.Pic_Bat.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.Pic_Bat.Location = New System.Drawing.Point(353, 6)
        Me.Pic_Bat.Name = "Pic_Bat"
        Me.Pic_Bat.Size = New System.Drawing.Size(915, 890)
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
        Me.GroupBox3.AutoSize = True
        Me.GroupBox3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.GroupBox3.BackColor = System.Drawing.Color.White
        Me.GroupBox3.Controls.Add(Me.rangeSettingsLayout)
        Me.GroupBox3.Dock = System.Windows.Forms.DockStyle.Top
        Me.GroupBox3.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.GroupBox3.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.GroupBox3.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.GroupBox3.Location = New System.Drawing.Point(6, 368)
        Me.GroupBox3.Margin = New System.Windows.Forms.Padding(0, 0, 0, 5)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Padding = New System.Windows.Forms.Padding(6, 15, 6, 6)
        Me.GroupBox3.Size = New System.Drawing.Size(338, 107)
        Me.GroupBox3.TabIndex = 49
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "合格范围设置"
        '
        'rangeSettingsLayout
        '
        Me.rangeSettingsLayout.AutoSize = True
        Me.rangeSettingsLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.rangeSettingsLayout.BackColor = System.Drawing.Color.White
        Me.rangeSettingsLayout.ColumnCount = 3
        Me.rangeSettingsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.rangeSettingsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.rangeSettingsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.rangeSettingsLayout.Controls.Add(Me.Label9, 1, 0)
        Me.rangeSettingsLayout.Controls.Add(Me.Label8, 2, 0)
        Me.rangeSettingsLayout.Controls.Add(Me.Label7, 0, 1)
        Me.rangeSettingsLayout.Controls.Add(Me.txtMinV, 1, 1)
        Me.rangeSettingsLayout.Controls.Add(Me.txtMaxV, 2, 1)
        Me.rangeSettingsLayout.Controls.Add(Me.Label12, 0, 2)
        Me.rangeSettingsLayout.Controls.Add(Me.txtMinR, 1, 2)
        Me.rangeSettingsLayout.Controls.Add(Me.txtMaxR, 2, 2)
        Me.rangeSettingsLayout.Dock = System.Windows.Forms.DockStyle.Top
        Me.rangeSettingsLayout.Location = New System.Drawing.Point(6, 31)
        Me.rangeSettingsLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.rangeSettingsLayout.Name = "rangeSettingsLayout"
        Me.rangeSettingsLayout.RowCount = 3
        Me.rangeSettingsLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.rangeSettingsLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.rangeSettingsLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.rangeSettingsLayout.Size = New System.Drawing.Size(326, 70)
        Me.rangeSettingsLayout.TabIndex = 0
        '
        'Label9
        '
        Me.Label9.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label9.Location = New System.Drawing.Point(81, 2)
        Me.Label9.Margin = New System.Windows.Forms.Padding(2)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(119, 12)
        Me.Label9.TabIndex = 52
        Me.Label9.Text = "Min"
        Me.Label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label8
        '
        Me.Label8.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label8.Location = New System.Drawing.Point(204, 2)
        Me.Label8.Margin = New System.Windows.Forms.Padding(2)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(120, 12)
        Me.Label8.TabIndex = 51
        Me.Label8.Text = "Max"
        Me.Label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label7
        '
        Me.Label7.BackColor = System.Drawing.Color.White
        Me.Label7.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label7.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label7.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.Label7.Location = New System.Drawing.Point(2, 18)
        Me.Label7.Margin = New System.Windows.Forms.Padding(2)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(75, 23)
        Me.Label7.TabIndex = 48
        Me.Label7.Text = "电压(mV):"
        Me.Label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'txtMinV
        '
        Me.txtMinV.BackColor = System.Drawing.Color.White
        Me.txtMinV.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtMinV.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtMinV.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtMinV.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.txtMinV.Location = New System.Drawing.Point(81, 18)
        Me.txtMinV.Margin = New System.Windows.Forms.Padding(2)
        Me.txtMinV.Name = "txtMinV"
        Me.txtMinV.ReadOnly = True
        Me.txtMinV.Size = New System.Drawing.Size(119, 23)
        Me.txtMinV.TabIndex = 50
        '
        'txtMaxV
        '
        Me.txtMaxV.BackColor = System.Drawing.Color.White
        Me.txtMaxV.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtMaxV.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtMaxV.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtMaxV.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.txtMaxV.Location = New System.Drawing.Point(204, 18)
        Me.txtMaxV.Margin = New System.Windows.Forms.Padding(2)
        Me.txtMaxV.Name = "txtMaxV"
        Me.txtMaxV.ReadOnly = True
        Me.txtMaxV.Size = New System.Drawing.Size(120, 23)
        Me.txtMaxV.TabIndex = 49
        '
        'Label12
        '
        Me.Label12.BackColor = System.Drawing.Color.White
        Me.Label12.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label12.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label12.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.Label12.Location = New System.Drawing.Point(2, 45)
        Me.Label12.Margin = New System.Windows.Forms.Padding(2)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(75, 23)
        Me.Label12.TabIndex = 57
        Me.Label12.Text = "内阻(mΩ):"
        Me.Label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'txtMinR
        '
        Me.txtMinR.BackColor = System.Drawing.Color.White
        Me.txtMinR.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtMinR.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtMinR.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtMinR.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.txtMinR.Location = New System.Drawing.Point(81, 45)
        Me.txtMinR.Margin = New System.Windows.Forms.Padding(2)
        Me.txtMinR.Name = "txtMinR"
        Me.txtMinR.ReadOnly = True
        Me.txtMinR.Size = New System.Drawing.Size(119, 23)
        Me.txtMinR.TabIndex = 54
        '
        'txtMaxR
        '
        Me.txtMaxR.BackColor = System.Drawing.Color.White
        Me.txtMaxR.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtMaxR.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtMaxR.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtMaxR.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.txtMaxR.Location = New System.Drawing.Point(204, 45)
        Me.txtMaxR.Margin = New System.Windows.Forms.Padding(2)
        Me.txtMaxR.Name = "txtMaxR"
        Me.txtMaxR.ReadOnly = True
        Me.txtMaxR.Size = New System.Drawing.Size(120, 23)
        Me.txtMaxR.TabIndex = 53
        '
        'GroupBox1
        '
        Me.GroupBox1.AutoSize = True
        Me.GroupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.GroupBox1.BackColor = System.Drawing.Color.White
        Me.GroupBox1.Controls.Add(Me.controlBoardLayout)
        Me.GroupBox1.Dock = System.Windows.Forms.DockStyle.Top
        Me.GroupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.GroupBox1.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.GroupBox1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.GroupBox1.Location = New System.Drawing.Point(6, 142)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(0, 0, 0, 5)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(6, 15, 6, 6)
        Me.GroupBox1.Size = New System.Drawing.Size(338, 147)
        Me.GroupBox1.TabIndex = 56
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "控制板"
        '
        'controlBoardLayout
        '
        Me.controlBoardLayout.AutoSize = True
        Me.controlBoardLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.controlBoardLayout.BackColor = System.Drawing.Color.White
        Me.controlBoardLayout.ColumnCount = 3
        Me.controlBoardLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.3333!))
        Me.controlBoardLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.3333!))
        Me.controlBoardLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.3333!))
        Me.controlBoardLayout.Controls.Add(Me.Label2, 0, 0)
        Me.controlBoardLayout.Controls.Add(Me.txtTray, 1, 0)
        Me.controlBoardLayout.Controls.Add(Me.Label5, 0, 1)
        Me.controlBoardLayout.Controls.Add(Me.lblBarNum, 1, 1)
        Me.controlBoardLayout.Controls.Add(Me.CmdStart, 0, 2)
        Me.controlBoardLayout.Controls.Add(Me.btnStop, 1, 2)
        Me.controlBoardLayout.Controls.Add(Me.btnNGTest, 2, 2)
        Me.controlBoardLayout.Controls.Add(Me.btnTrayState, 0, 3)
        Me.controlBoardLayout.Controls.Add(Me.btnOpen, 1, 3)
        Me.controlBoardLayout.Controls.Add(Me.btnClose, 2, 3)
        Me.controlBoardLayout.Dock = System.Windows.Forms.DockStyle.Top
        Me.controlBoardLayout.Location = New System.Drawing.Point(6, 31)
        Me.controlBoardLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.controlBoardLayout.Name = "controlBoardLayout"
        Me.controlBoardLayout.RowCount = 4
        Me.controlBoardLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.controlBoardLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.controlBoardLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.controlBoardLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.controlBoardLayout.Size = New System.Drawing.Size(326, 110)
        Me.controlBoardLayout.TabIndex = 47
        '
        'Label2
        '
        Me.Label2.BackColor = System.Drawing.Color.White
        Me.Label2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label2.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.Label2.Location = New System.Drawing.Point(2, 2)
        Me.Label2.Margin = New System.Windows.Forms.Padding(2)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(104, 23)
        Me.Label2.TabIndex = 47
        Me.Label2.Text = "托盘码"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'txtTray
        '
        Me.txtTray.BackColor = System.Drawing.Color.White
        Me.txtTray.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.controlBoardLayout.SetColumnSpan(Me.txtTray, 2)
        Me.txtTray.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtTray.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtTray.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.txtTray.Location = New System.Drawing.Point(110, 2)
        Me.txtTray.Margin = New System.Windows.Forms.Padding(2)
        Me.txtTray.Name = "txtTray"
        Me.txtTray.ReadOnly = True
        Me.txtTray.Size = New System.Drawing.Size(214, 23)
        Me.txtTray.TabIndex = 44
        '
        'Label5
        '
        Me.Label5.BackColor = System.Drawing.Color.White
        Me.Label5.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label5.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label5.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.Label5.Location = New System.Drawing.Point(2, 29)
        Me.Label5.Margin = New System.Windows.Forms.Padding(2)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(104, 21)
        Me.Label5.TabIndex = 47
        Me.Label5.Text = "待测试通道数"
        Me.Label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'lblBarNum
        '
        Me.lblBarNum.AutoSize = True
        Me.lblBarNum.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblBarNum.Font = New System.Drawing.Font("宋体", 10.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.lblBarNum.Location = New System.Drawing.Point(110, 29)
        Me.lblBarNum.Margin = New System.Windows.Forms.Padding(2)
        Me.lblBarNum.Name = "lblBarNum"
        Me.lblBarNum.Size = New System.Drawing.Size(104, 21)
        Me.lblBarNum.TabIndex = 50
        Me.lblBarNum.Text = "intsum"
        '
        'CmdStart
        '
        Me.CmdStart.BackColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.CmdStart.Dock = System.Windows.Forms.DockStyle.Fill
        Me.CmdStart.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.CmdStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.CmdStart.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.CmdStart.ForeColor = System.Drawing.Color.White
        Me.CmdStart.Location = New System.Drawing.Point(2, 54)
        Me.CmdStart.Margin = New System.Windows.Forms.Padding(2)
        Me.CmdStart.Name = "CmdStart"
        Me.CmdStart.Size = New System.Drawing.Size(104, 25)
        Me.CmdStart.TabIndex = 56
        Me.CmdStart.Text = "启动测试"
        Me.CmdStart.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.CmdStart.UseVisualStyleBackColor = False
        '
        'btnStop
        '
        Me.btnStop.BackColor = System.Drawing.Color.White
        Me.btnStop.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnStop.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(239, Byte), Integer), CType(CType(62, Byte), Integer), CType(CType(62, Byte), Integer))
        Me.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnStop.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.btnStop.ForeColor = System.Drawing.Color.FromArgb(CType(CType(239, Byte), Integer), CType(CType(62, Byte), Integer), CType(CType(62, Byte), Integer))
        Me.btnStop.Location = New System.Drawing.Point(110, 54)
        Me.btnStop.Margin = New System.Windows.Forms.Padding(2)
        Me.btnStop.Name = "btnStop"
        Me.btnStop.Size = New System.Drawing.Size(104, 25)
        Me.btnStop.TabIndex = 65
        Me.btnStop.Text = "停止测试"
        Me.btnStop.UseVisualStyleBackColor = False
        '
        'btnNGTest
        '
        Me.btnNGTest.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(164, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.btnNGTest.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnNGTest.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(164, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.btnNGTest.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnNGTest.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.btnNGTest.ForeColor = System.Drawing.Color.White
        Me.btnNGTest.Location = New System.Drawing.Point(218, 54)
        Me.btnNGTest.Margin = New System.Windows.Forms.Padding(2)
        Me.btnNGTest.Name = "btnNGTest"
        Me.btnNGTest.Size = New System.Drawing.Size(106, 25)
        Me.btnNGTest.TabIndex = 56
        Me.btnNGTest.Text = "NG点测试"
        Me.btnNGTest.UseVisualStyleBackColor = False
        '
        'btnTrayState
        '
        Me.btnTrayState.BackColor = System.Drawing.Color.White
        Me.btnTrayState.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnTrayState.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(231, Byte), Integer), CType(CType(235, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.btnTrayState.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnTrayState.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.btnTrayState.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.btnTrayState.Location = New System.Drawing.Point(2, 83)
        Me.btnTrayState.Margin = New System.Windows.Forms.Padding(2)
        Me.btnTrayState.Name = "btnTrayState"
        Me.btnTrayState.Size = New System.Drawing.Size(104, 25)
        Me.btnTrayState.TabIndex = 60
        Me.btnTrayState.Text = "托盘状态"
        Me.btnTrayState.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnTrayState.UseVisualStyleBackColor = False
        '
        'btnOpen
        '
        Me.btnOpen.BackColor = System.Drawing.Color.White
        Me.btnOpen.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnOpen.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(231, Byte), Integer), CType(CType(235, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.btnOpen.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnOpen.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.btnOpen.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.btnOpen.Location = New System.Drawing.Point(110, 83)
        Me.btnOpen.Margin = New System.Windows.Forms.Padding(2)
        Me.btnOpen.Name = "btnOpen"
        Me.btnOpen.Size = New System.Drawing.Size(104, 25)
        Me.btnOpen.TabIndex = 62
        Me.btnOpen.Text = "开启托盘"
        Me.btnOpen.UseVisualStyleBackColor = False
        '
        'btnClose
        '
        Me.btnClose.BackColor = System.Drawing.Color.White
        Me.btnClose.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(231, Byte), Integer), CType(CType(235, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnClose.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.btnClose.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.btnClose.Location = New System.Drawing.Point(218, 83)
        Me.btnClose.Margin = New System.Windows.Forms.Padding(2)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(106, 25)
        Me.btnClose.TabIndex = 63
        Me.btnClose.Text = "闭合托盘"
        Me.btnClose.UseVisualStyleBackColor = False
        '
        'Label3
        '
        Me.Label3.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label3.Location = New System.Drawing.Point(152, 845)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(10, 10)
        Me.Label3.TabIndex = 42
        Me.Label3.Text = "首电池位置"
        Me.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.Label3.Visible = False
        '
        'Label4
        '
        Me.Label4.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label4.Location = New System.Drawing.Point(86, 844)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(10, 10)
        Me.Label4.TabIndex = 43
        Me.Label4.Text = "尾电池位置"
        Me.Label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.Label4.Visible = False
        '
        'txtLastNum
        '
        Me.txtLastNum.BackColor = System.Drawing.Color.White
        Me.txtLastNum.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtLastNum.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtLastNum.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.txtLastNum.Location = New System.Drawing.Point(132, 844)
        Me.txtLastNum.Name = "txtLastNum"
        Me.txtLastNum.ReadOnly = True
        Me.txtLastNum.Size = New System.Drawing.Size(10, 23)
        Me.txtLastNum.TabIndex = 46
        Me.txtLastNum.Visible = False
        '
        'txtFirstNum
        '
        Me.txtFirstNum.BackColor = System.Drawing.Color.White
        Me.txtFirstNum.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtFirstNum.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtFirstNum.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.txtFirstNum.Location = New System.Drawing.Point(198, 845)
        Me.txtFirstNum.Name = "txtFirstNum"
        Me.txtFirstNum.ReadOnly = True
        Me.txtFirstNum.Size = New System.Drawing.Size(10, 23)
        Me.txtFirstNum.TabIndex = 45
        Me.txtFirstNum.Visible = False
        '
        'grpChannelSelection
        '
        Me.grpChannelSelection.AutoSize = True
        Me.grpChannelSelection.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.grpChannelSelection.BackColor = System.Drawing.Color.White
        Me.grpChannelSelection.Controls.Add(Me.channelSelectionLayout)
        Me.grpChannelSelection.Dock = System.Windows.Forms.DockStyle.Top
        Me.grpChannelSelection.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.grpChannelSelection.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.grpChannelSelection.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.grpChannelSelection.Location = New System.Drawing.Point(6, 294)
        Me.grpChannelSelection.Margin = New System.Windows.Forms.Padding(0, 0, 0, 5)
        Me.grpChannelSelection.Name = "grpChannelSelection"
        Me.grpChannelSelection.Padding = New System.Windows.Forms.Padding(6, 15, 6, 6)
        Me.grpChannelSelection.Size = New System.Drawing.Size(338, 69)
        Me.grpChannelSelection.TabIndex = 72
        Me.grpChannelSelection.TabStop = False
        Me.grpChannelSelection.Text = "通道选择"
        '
        'channelSelectionLayout
        '
        Me.channelSelectionLayout.AutoSize = True
        Me.channelSelectionLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.channelSelectionLayout.BackColor = System.Drawing.Color.White
        Me.channelSelectionLayout.ColumnCount = 4
        Me.channelSelectionLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.channelSelectionLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.channelSelectionLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.channelSelectionLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.channelSelectionLayout.Controls.Add(Me.btnChannelSelectAll, 0, 0)
        Me.channelSelectionLayout.Controls.Add(Me.btnChannelClear, 1, 0)
        Me.channelSelectionLayout.Controls.Add(Me.btnChannelRectangle, 2, 0)
        Me.channelSelectionLayout.Controls.Add(Me.btnChannelSelectSingle, 3, 0)
        Me.channelSelectionLayout.Dock = System.Windows.Forms.DockStyle.Top
        Me.channelSelectionLayout.Location = New System.Drawing.Point(6, 31)
        Me.channelSelectionLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.channelSelectionLayout.Name = "channelSelectionLayout"
        Me.channelSelectionLayout.RowCount = 1
        Me.channelSelectionLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.channelSelectionLayout.Size = New System.Drawing.Size(326, 32)
        Me.channelSelectionLayout.TabIndex = 0
        '
        'btnChannelSelectAll
        '
        Me.btnChannelSelectAll.BackColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.btnChannelSelectAll.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnChannelSelectAll.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.btnChannelSelectAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnChannelSelectAll.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.btnChannelSelectAll.ForeColor = System.Drawing.Color.White
        Me.btnChannelSelectAll.Location = New System.Drawing.Point(2, 2)
        Me.btnChannelSelectAll.Margin = New System.Windows.Forms.Padding(2)
        Me.btnChannelSelectAll.Name = "btnChannelSelectAll"
        Me.btnChannelSelectAll.Size = New System.Drawing.Size(77, 28)
        Me.btnChannelSelectAll.TabIndex = 0
        Me.btnChannelSelectAll.Text = "全选"
        Me.btnChannelSelectAll.UseVisualStyleBackColor = False
        '
        'btnChannelClear
        '
        Me.btnChannelClear.BackColor = System.Drawing.Color.White
        Me.btnChannelClear.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnChannelClear.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(231, Byte), Integer), CType(CType(235, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.btnChannelClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnChannelClear.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.btnChannelClear.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.btnChannelClear.Location = New System.Drawing.Point(83, 2)
        Me.btnChannelClear.Margin = New System.Windows.Forms.Padding(2)
        Me.btnChannelClear.Name = "btnChannelClear"
        Me.btnChannelClear.Size = New System.Drawing.Size(77, 28)
        Me.btnChannelClear.TabIndex = 1
        Me.btnChannelClear.Text = "清除"
        Me.btnChannelClear.UseVisualStyleBackColor = False
        '
        'btnChannelRectangle
        '
        Me.btnChannelRectangle.BackColor = System.Drawing.Color.White
        Me.btnChannelRectangle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnChannelRectangle.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(231, Byte), Integer), CType(CType(235, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.btnChannelRectangle.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnChannelRectangle.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.btnChannelRectangle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.btnChannelRectangle.Location = New System.Drawing.Point(164, 2)
        Me.btnChannelRectangle.Margin = New System.Windows.Forms.Padding(2)
        Me.btnChannelRectangle.Name = "btnChannelRectangle"
        Me.btnChannelRectangle.Size = New System.Drawing.Size(77, 28)
        Me.btnChannelRectangle.TabIndex = 2
        Me.btnChannelRectangle.Text = "框选"
        Me.btnChannelRectangle.UseVisualStyleBackColor = False
        '
        'btnChannelSelectSingle
        '
        Me.btnChannelSelectSingle.BackColor = System.Drawing.Color.White
        Me.btnChannelSelectSingle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnChannelSelectSingle.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(231, Byte), Integer), CType(CType(235, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.btnChannelSelectSingle.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnChannelSelectSingle.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.btnChannelSelectSingle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.btnChannelSelectSingle.Location = New System.Drawing.Point(245, 2)
        Me.btnChannelSelectSingle.Margin = New System.Windows.Forms.Padding(2)
        Me.btnChannelSelectSingle.Name = "btnChannelSelectSingle"
        Me.btnChannelSelectSingle.Size = New System.Drawing.Size(79, 28)
        Me.btnChannelSelectSingle.TabIndex = 3
        Me.btnChannelSelectSingle.Text = "单选"
        Me.btnChannelSelectSingle.UseVisualStyleBackColor = False
        '
        'GroupBox2
        '
        Me.GroupBox2.BackColor = System.Drawing.Color.White
        Me.GroupBox2.Controls.Add(Me.Rbtn1)
        Me.GroupBox2.Controls.Add(Me.CmdBatInput)
        Me.GroupBox2.Controls.Add(Me.cmbShow)
        Me.GroupBox2.Controls.Add(Me.Rbtn3)
        Me.GroupBox2.Controls.Add(Me.Rbtn2)
        Me.GroupBox2.Enabled = False
        Me.GroupBox2.Location = New System.Drawing.Point(2, 12)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(306, 39)
        Me.GroupBox2.TabIndex = 58
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "工作站选择"
        Me.GroupBox2.Visible = False
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
        'GroupBox6
        '
        Me.GroupBox6.Location = New System.Drawing.Point(0, 0)
        Me.GroupBox6.Name = "GroupBox6"
        Me.GroupBox6.Size = New System.Drawing.Size(200, 100)
        Me.GroupBox6.TabIndex = 0
        Me.GroupBox6.TabStop = False
        Me.GroupBox6.Visible = False
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
        Me.txtJiajuR.BackColor = System.Drawing.Color.White
        Me.txtJiajuR.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtJiajuR.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtJiajuR.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.txtJiajuR.Location = New System.Drawing.Point(0, 0)
        Me.txtJiajuR.Name = "txtJiajuR"
        Me.txtJiajuR.Size = New System.Drawing.Size(100, 23)
        Me.txtJiajuR.TabIndex = 0
        '
        'txtCaliR
        '
        Me.txtCaliR.BackColor = System.Drawing.Color.White
        Me.txtCaliR.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtCaliR.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtCaliR.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.txtCaliR.Location = New System.Drawing.Point(0, 0)
        Me.txtCaliR.Name = "txtCaliR"
        Me.txtCaliR.Size = New System.Drawing.Size(100, 23)
        Me.txtCaliR.TabIndex = 0
        '
        'txtTestR
        '
        Me.txtTestR.BackColor = System.Drawing.Color.White
        Me.txtTestR.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtTestR.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtTestR.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.txtTestR.Location = New System.Drawing.Point(0, 0)
        Me.txtTestR.Name = "txtTestR"
        Me.txtTestR.Size = New System.Drawing.Size(100, 23)
        Me.txtTestR.TabIndex = 0
        '
        'GroupBox5
        '
        Me.GroupBox5.AutoSize = True
        Me.GroupBox5.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.GroupBox5.BackColor = System.Drawing.Color.White
        Me.GroupBox5.Controls.Add(Me.alarmSectionLayout)
        Me.GroupBox5.Dock = System.Windows.Forms.DockStyle.Top
        Me.GroupBox5.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.GroupBox5.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.GroupBox5.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.GroupBox5.Location = New System.Drawing.Point(6, 480)
        Me.GroupBox5.Margin = New System.Windows.Forms.Padding(0, 0, 0, 5)
        Me.GroupBox5.Name = "GroupBox5"
        Me.GroupBox5.Padding = New System.Windows.Forms.Padding(6, 15, 6, 6)
        Me.GroupBox5.Size = New System.Drawing.Size(338, 134)
        Me.GroupBox5.TabIndex = 59
        Me.GroupBox5.TabStop = False
        Me.GroupBox5.Text = "报警指示"
        '
        'alarmSectionLayout
        '
        Me.alarmSectionLayout.AutoSize = True
        Me.alarmSectionLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.alarmSectionLayout.BackColor = System.Drawing.Color.White
        Me.alarmSectionLayout.ColumnCount = 1
        Me.alarmSectionLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.alarmSectionLayout.Controls.Add(Me.Panel2, 0, 0)
        Me.alarmSectionLayout.Controls.Add(Me.Panel3, 0, 1)
        Me.alarmSectionLayout.Dock = System.Windows.Forms.DockStyle.Top
        Me.alarmSectionLayout.Location = New System.Drawing.Point(6, 31)
        Me.alarmSectionLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.alarmSectionLayout.Name = "alarmSectionLayout"
        Me.alarmSectionLayout.RowCount = 2
        Me.alarmSectionLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.alarmSectionLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.alarmSectionLayout.Size = New System.Drawing.Size(326, 97)
        Me.alarmSectionLayout.TabIndex = 0
        '
        'Panel2
        '
        Me.Panel2.AutoSize = True
        Me.Panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.Panel2.Controls.Add(Me.alarmTemperatureLayout)
        Me.Panel2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel2.Location = New System.Drawing.Point(2, 2)
        Me.Panel2.Margin = New System.Windows.Forms.Padding(2)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(322, 54)
        Me.Panel2.TabIndex = 57
        '
        'alarmTemperatureLayout
        '
        Me.alarmTemperatureLayout.AutoSize = True
        Me.alarmTemperatureLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.alarmTemperatureLayout.BackColor = System.Drawing.Color.White
        Me.alarmTemperatureLayout.ColumnCount = 4
        Me.alarmTemperatureLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.alarmTemperatureLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.alarmTemperatureLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.alarmTemperatureLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.0!))
        Me.alarmTemperatureLayout.Controls.Add(Me.Label1, 0, 0)
        Me.alarmTemperatureLayout.Controls.Add(Me.t1, 1, 0)
        Me.alarmTemperatureLayout.Controls.Add(Me.Label14, 2, 0)
        Me.alarmTemperatureLayout.Controls.Add(Me.t2, 3, 0)
        Me.alarmTemperatureLayout.Controls.Add(Me.Label15, 0, 1)
        Me.alarmTemperatureLayout.Controls.Add(Me.t3, 1, 1)
        Me.alarmTemperatureLayout.Controls.Add(Me.Label16, 2, 1)
        Me.alarmTemperatureLayout.Controls.Add(Me.t4, 3, 1)
        Me.alarmTemperatureLayout.Dock = System.Windows.Forms.DockStyle.Top
        Me.alarmTemperatureLayout.Location = New System.Drawing.Point(0, 0)
        Me.alarmTemperatureLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.alarmTemperatureLayout.Name = "alarmTemperatureLayout"
        Me.alarmTemperatureLayout.RowCount = 2
        Me.alarmTemperatureLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.alarmTemperatureLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.alarmTemperatureLayout.Size = New System.Drawing.Size(322, 54)
        Me.alarmTemperatureLayout.TabIndex = 0
        '
        'Label1
        '
        Me.Label1.BackColor = System.Drawing.Color.White
        Me.Label1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label1.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.Label1.Location = New System.Drawing.Point(2, 2)
        Me.Label1.Margin = New System.Windows.Forms.Padding(2)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(76, 23)
        Me.Label1.TabIndex = 51
        Me.Label1.Text = "温度1"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        't1
        '
        Me.t1.BackColor = System.Drawing.Color.White
        Me.t1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.t1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.t1.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.t1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.t1.Location = New System.Drawing.Point(82, 2)
        Me.t1.Margin = New System.Windows.Forms.Padding(2)
        Me.t1.Name = "t1"
        Me.t1.ReadOnly = True
        Me.t1.Size = New System.Drawing.Size(76, 23)
        Me.t1.TabIndex = 49
        '
        'Label14
        '
        Me.Label14.BackColor = System.Drawing.Color.White
        Me.Label14.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label14.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label14.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.Label14.Location = New System.Drawing.Point(162, 2)
        Me.Label14.Margin = New System.Windows.Forms.Padding(2)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(76, 23)
        Me.Label14.TabIndex = 52
        Me.Label14.Text = "温度2"
        Me.Label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        't2
        '
        Me.t2.BackColor = System.Drawing.Color.White
        Me.t2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.t2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.t2.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.t2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.t2.Location = New System.Drawing.Point(242, 2)
        Me.t2.Margin = New System.Windows.Forms.Padding(2)
        Me.t2.Name = "t2"
        Me.t2.ReadOnly = True
        Me.t2.Size = New System.Drawing.Size(78, 23)
        Me.t2.TabIndex = 49
        '
        'Label15
        '
        Me.Label15.BackColor = System.Drawing.Color.White
        Me.Label15.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label15.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label15.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.Label15.Location = New System.Drawing.Point(2, 29)
        Me.Label15.Margin = New System.Windows.Forms.Padding(2)
        Me.Label15.Name = "Label15"
        Me.Label15.Size = New System.Drawing.Size(76, 23)
        Me.Label15.TabIndex = 55
        Me.Label15.Text = "温度3"
        Me.Label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        't3
        '
        Me.t3.BackColor = System.Drawing.Color.White
        Me.t3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.t3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.t3.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.t3.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.t3.Location = New System.Drawing.Point(82, 29)
        Me.t3.Margin = New System.Windows.Forms.Padding(2)
        Me.t3.Name = "t3"
        Me.t3.ReadOnly = True
        Me.t3.Size = New System.Drawing.Size(76, 23)
        Me.t3.TabIndex = 53
        '
        'Label16
        '
        Me.Label16.BackColor = System.Drawing.Color.White
        Me.Label16.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label16.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label16.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.Label16.Location = New System.Drawing.Point(162, 29)
        Me.Label16.Margin = New System.Windows.Forms.Padding(2)
        Me.Label16.Name = "Label16"
        Me.Label16.Size = New System.Drawing.Size(76, 23)
        Me.Label16.TabIndex = 56
        Me.Label16.Text = "温度4"
        Me.Label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        't4
        '
        Me.t4.BackColor = System.Drawing.Color.White
        Me.t4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.t4.Dock = System.Windows.Forms.DockStyle.Fill
        Me.t4.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.t4.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.t4.Location = New System.Drawing.Point(242, 29)
        Me.t4.Margin = New System.Windows.Forms.Padding(2)
        Me.t4.Name = "t4"
        Me.t4.ReadOnly = True
        Me.t4.Size = New System.Drawing.Size(78, 23)
        Me.t4.TabIndex = 53
        '
        'Panel3
        '
        Me.Panel3.AutoSize = True
        Me.Panel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.Panel3.Controls.Add(Me.alarmStatusLayout)
        Me.Panel3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel3.Location = New System.Drawing.Point(2, 60)
        Me.Panel3.Margin = New System.Windows.Forms.Padding(2)
        Me.Panel3.Name = "Panel3"
        Me.Panel3.Size = New System.Drawing.Size(322, 35)
        Me.Panel3.TabIndex = 57
        '
        'alarmStatusLayout
        '
        Me.alarmStatusLayout.AutoSize = True
        Me.alarmStatusLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.alarmStatusLayout.BackColor = System.Drawing.Color.White
        Me.alarmStatusLayout.ColumnCount = 3
        Me.alarmStatusLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24.0!))
        Me.alarmStatusLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24.0!))
        Me.alarmStatusLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 52.0!))
        Me.alarmStatusLayout.Controls.Add(Me.smokeOneLayout, 0, 0)
        Me.alarmStatusLayout.Controls.Add(Me.smokeTwoLayout, 1, 0)
        Me.alarmStatusLayout.Controls.Add(Me.deviceStatusLayout, 2, 0)
        Me.alarmStatusLayout.Dock = System.Windows.Forms.DockStyle.Top
        Me.alarmStatusLayout.Location = New System.Drawing.Point(0, 0)
        Me.alarmStatusLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.alarmStatusLayout.Name = "alarmStatusLayout"
        Me.alarmStatusLayout.RowCount = 1
        Me.alarmStatusLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.alarmStatusLayout.Size = New System.Drawing.Size(322, 35)
        Me.alarmStatusLayout.TabIndex = 0
        '
        'smokeOneLayout
        '
        Me.smokeOneLayout.AutoSize = True
        Me.smokeOneLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.smokeOneLayout.Controls.Add(Me.Label19)
        Me.smokeOneLayout.Controls.Add(Me.smokeAlarm1)
        Me.smokeOneLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.smokeOneLayout.Location = New System.Drawing.Point(2, 2)
        Me.smokeOneLayout.Margin = New System.Windows.Forms.Padding(2)
        Me.smokeOneLayout.Name = "smokeOneLayout"
        Me.smokeOneLayout.Size = New System.Drawing.Size(73, 31)
        Me.smokeOneLayout.TabIndex = 0
        Me.smokeOneLayout.WrapContents = False
        '
        'Label19
        '
        Me.Label19.AutoSize = True
        Me.Label19.BackColor = System.Drawing.Color.White
        Me.Label19.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label19.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.Label19.Location = New System.Drawing.Point(2, 2)
        Me.Label19.Margin = New System.Windows.Forms.Padding(2)
        Me.Label19.Name = "Label19"
        Me.Label19.Size = New System.Drawing.Size(39, 17)
        Me.Label19.TabIndex = 51
        Me.Label19.Text = "烟感1"
        '
        'smokeAlarm1
        '
        Me.smokeAlarm1.BackColor = System.Drawing.Color.LimeGreen
        Me.smokeAlarm1.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.smokeAlarm1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.smokeAlarm1.Location = New System.Drawing.Point(45, 2)
        Me.smokeAlarm1.Margin = New System.Windows.Forms.Padding(2)
        Me.smokeAlarm1.Multiline = True
        Me.smokeAlarm1.Name = "smokeAlarm1"
        Me.smokeAlarm1.ReadOnly = True
        Me.smokeAlarm1.Size = New System.Drawing.Size(32, 27)
        Me.smokeAlarm1.TabIndex = 49
        '
        'smokeTwoLayout
        '
        Me.smokeTwoLayout.AutoSize = True
        Me.smokeTwoLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.smokeTwoLayout.Controls.Add(Me.Label17)
        Me.smokeTwoLayout.Controls.Add(Me.smokeAlarm2)
        Me.smokeTwoLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.smokeTwoLayout.Location = New System.Drawing.Point(79, 2)
        Me.smokeTwoLayout.Margin = New System.Windows.Forms.Padding(2)
        Me.smokeTwoLayout.Name = "smokeTwoLayout"
        Me.smokeTwoLayout.Size = New System.Drawing.Size(73, 31)
        Me.smokeTwoLayout.TabIndex = 1
        Me.smokeTwoLayout.WrapContents = False
        '
        'Label17
        '
        Me.Label17.AutoSize = True
        Me.Label17.BackColor = System.Drawing.Color.White
        Me.Label17.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label17.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.Label17.Location = New System.Drawing.Point(2, 2)
        Me.Label17.Margin = New System.Windows.Forms.Padding(2)
        Me.Label17.Name = "Label17"
        Me.Label17.Size = New System.Drawing.Size(39, 17)
        Me.Label17.TabIndex = 52
        Me.Label17.Text = "烟感2"
        '
        'smokeAlarm2
        '
        Me.smokeAlarm2.BackColor = System.Drawing.Color.LimeGreen
        Me.smokeAlarm2.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.smokeAlarm2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.smokeAlarm2.Location = New System.Drawing.Point(45, 2)
        Me.smokeAlarm2.Margin = New System.Windows.Forms.Padding(2)
        Me.smokeAlarm2.Multiline = True
        Me.smokeAlarm2.Name = "smokeAlarm2"
        Me.smokeAlarm2.ReadOnly = True
        Me.smokeAlarm2.Size = New System.Drawing.Size(34, 26)
        Me.smokeAlarm2.TabIndex = 50
        '
        'deviceStatusLayout
        '
        Me.deviceStatusLayout.AutoSize = True
        Me.deviceStatusLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.deviceStatusLayout.Controls.Add(Me.Label28)
        Me.deviceStatusLayout.Controls.Add(Me.LabelRun)
        Me.deviceStatusLayout.Controls.Add(Me.LabelAlarm)
        Me.deviceStatusLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.deviceStatusLayout.Location = New System.Drawing.Point(156, 2)
        Me.deviceStatusLayout.Margin = New System.Windows.Forms.Padding(2)
        Me.deviceStatusLayout.Name = "deviceStatusLayout"
        Me.deviceStatusLayout.Size = New System.Drawing.Size(164, 31)
        Me.deviceStatusLayout.TabIndex = 2
        Me.deviceStatusLayout.WrapContents = False
        '
        'Label28
        '
        Me.Label28.AutoSize = True
        Me.Label28.BackColor = System.Drawing.Color.White
        Me.Label28.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label28.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.Label28.Location = New System.Drawing.Point(2, 2)
        Me.Label28.Margin = New System.Windows.Forms.Padding(2)
        Me.Label28.Name = "Label28"
        Me.Label28.Size = New System.Drawing.Size(56, 17)
        Me.Label28.TabIndex = 52
        Me.Label28.Text = "设备状态"
        '
        'LabelRun
        '
        Me.LabelRun.AutoSize = True
        Me.LabelRun.BackColor = System.Drawing.Color.LimeGreen
        Me.LabelRun.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LabelRun.Font = New System.Drawing.Font("微软雅黑", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.LabelRun.Location = New System.Drawing.Point(62, 2)
        Me.LabelRun.Margin = New System.Windows.Forms.Padding(2)
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
        Me.LabelAlarm.Location = New System.Drawing.Point(128, 2)
        Me.LabelAlarm.Margin = New System.Windows.Forms.Padding(2)
        Me.LabelAlarm.Name = "LabelAlarm"
        Me.LabelAlarm.Size = New System.Drawing.Size(56, 17)
        Me.LabelAlarm.TabIndex = 51
        Me.LabelAlarm.Text = "急停报警"
        '
        'GroupBox7
        '
        Me.GroupBox7.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.GroupBox7.BackColor = System.Drawing.Color.White
        Me.GroupBox7.Controls.Add(Me.txtAlarm)
        Me.GroupBox7.Dock = System.Windows.Forms.DockStyle.Top
        Me.GroupBox7.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.GroupBox7.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.GroupBox7.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.GroupBox7.Location = New System.Drawing.Point(6, 688)
        Me.GroupBox7.Margin = New System.Windows.Forms.Padding(0, 0, 0, 5)
        Me.GroupBox7.MinimumSize = New System.Drawing.Size(0, 82)
        Me.GroupBox7.Name = "GroupBox7"
        Me.GroupBox7.Padding = New System.Windows.Forms.Padding(6, 15, 6, 6)
        Me.GroupBox7.Size = New System.Drawing.Size(338, 82)
        Me.GroupBox7.TabIndex = 64
        Me.GroupBox7.TabStop = False
        Me.GroupBox7.Text = "事件日志"
        '
        'txtAlarm
        '
        Me.txtAlarm.BackColor = System.Drawing.Color.White
        Me.txtAlarm.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtAlarm.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtAlarm.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtAlarm.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.txtAlarm.Location = New System.Drawing.Point(6, 31)
        Me.txtAlarm.Margin = New System.Windows.Forms.Padding(0)
        Me.txtAlarm.Multiline = True
        Me.txtAlarm.Name = "txtAlarm"
        Me.txtAlarm.Size = New System.Drawing.Size(326, 45)
        Me.txtAlarm.TabIndex = 0
        '
        'Button2
        '
        Me.Button2.Anchor = System.Windows.Forms.AnchorStyles.None
        Me.Button2.BackColor = System.Drawing.Color.White
        Me.Button2.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(239, Byte), Integer), CType(CType(62, Byte), Integer), CType(CType(62, Byte), Integer))
        Me.Button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.Button2.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Button2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(239, Byte), Integer), CType(CType(62, Byte), Integer), CType(CType(62, Byte), Integer))
        Me.Button2.Location = New System.Drawing.Point(102, 2)
        Me.Button2.Margin = New System.Windows.Forms.Padding(0)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(133, 34)
        Me.Button2.TabIndex = 1
        Me.Button2.Text = "报警复位"
        Me.Button2.UseVisualStyleBackColor = False
        '
        'Panel1
        '
        Me.Panel1.AutoScroll = True
        Me.Panel1.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(246, Byte), Integer), CType(CType(250, Byte), Integer))
        Me.Panel1.Controls.Add(Me.txtUsed)
        Me.Panel1.Controls.Add(Me.leftContentPanel)
        Me.Panel1.Controls.Add(Me.txtTotle)
        Me.Panel1.Controls.Add(Me.GroupBox2)
        Me.Panel1.Controls.Add(Me.txtFirstNum)
        Me.Panel1.Controls.Add(Me.Label4)
        Me.Panel1.Controls.Add(Me.Label3)
        Me.Panel1.Controls.Add(Me.txtLastNum)
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel1.Location = New System.Drawing.Point(0, 0)
        Me.Panel1.Margin = New System.Windows.Forms.Padding(0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(350, 902)
        Me.Panel1.TabIndex = 15
        '
        'txtUsed
        '
        Me.txtUsed.BackColor = System.Drawing.Color.White
        Me.txtUsed.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtUsed.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.txtUsed.Location = New System.Drawing.Point(216, 845)
        Me.txtUsed.Name = "txtUsed"
        Me.txtUsed.ReadOnly = True
        Me.txtUsed.Size = New System.Drawing.Size(10, 21)
        Me.txtUsed.TabIndex = 53
        Me.txtUsed.Visible = False
        '
        'leftContentPanel
        '
        Me.leftContentPanel.AutoSize = True
        Me.leftContentPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.leftContentPanel.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(246, Byte), Integer), CType(CType(250, Byte), Integer))
        Me.leftContentPanel.ColumnCount = 1
        Me.leftContentPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 338.0!))
        Me.leftContentPanel.Controls.Add(Me.currentStationButton, 0, 0)
        Me.leftContentPanel.Controls.Add(Me.grpTestMode, 0, 1)
        Me.leftContentPanel.Controls.Add(Me.GroupBox1, 0, 2)
        Me.leftContentPanel.Controls.Add(Me.grpChannelSelection, 0, 3)
        Me.leftContentPanel.Controls.Add(Me.GroupBox3, 0, 4)
        Me.leftContentPanel.Controls.Add(Me.GroupBox5, 0, 5)
        Me.leftContentPanel.Controls.Add(Me.GroupBox8, 0, 6)
        Me.leftContentPanel.Controls.Add(Me.GroupBox7, 0, 7)
        Me.leftContentPanel.Controls.Add(Me.resetAlarmLayout, 0, 8)
        Me.leftContentPanel.Dock = System.Windows.Forms.DockStyle.Top
        Me.leftContentPanel.Location = New System.Drawing.Point(0, 0)
        Me.leftContentPanel.Margin = New System.Windows.Forms.Padding(0)
        Me.leftContentPanel.Name = "leftContentPanel"
        Me.leftContentPanel.Padding = New System.Windows.Forms.Padding(6)
        Me.leftContentPanel.RowCount = 9
        Me.leftContentPanel.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.leftContentPanel.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.leftContentPanel.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.leftContentPanel.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.leftContentPanel.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.leftContentPanel.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.leftContentPanel.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.leftContentPanel.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.leftContentPanel.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.leftContentPanel.Size = New System.Drawing.Size(350, 819)
        Me.leftContentPanel.TabIndex = 59
        '
        'currentStationButton
        '
        Me.currentStationButton.Appearance = System.Windows.Forms.Appearance.Button
        Me.currentStationButton.BackColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.currentStationButton.Dock = System.Windows.Forms.DockStyle.Top
        Me.currentStationButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.currentStationButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.currentStationButton.Font = New System.Drawing.Font("Microsoft YaHei UI", 14.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.currentStationButton.ForeColor = System.Drawing.Color.White
        Me.currentStationButton.Location = New System.Drawing.Point(6, 6)
        Me.currentStationButton.Margin = New System.Windows.Forms.Padding(0, 0, 0, 5)
        Me.currentStationButton.Name = "currentStationButton"
        Me.currentStationButton.Size = New System.Drawing.Size(338, 56)
        Me.currentStationButton.TabIndex = 0
        Me.currentStationButton.Text = "OCV1"
        Me.currentStationButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.currentStationButton.UseVisualStyleBackColor = False
        '
        'grpTestMode
        '
        Me.grpTestMode.BackColor = System.Drawing.Color.White
        Me.grpTestMode.Controls.Add(Me.testModeLayout)
        Me.grpTestMode.Dock = System.Windows.Forms.DockStyle.Top
        Me.grpTestMode.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.grpTestMode.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.grpTestMode.Location = New System.Drawing.Point(6, 67)
        Me.grpTestMode.Margin = New System.Windows.Forms.Padding(0, 0, 0, 5)
        Me.grpTestMode.Name = "grpTestMode"
        Me.grpTestMode.Padding = New System.Windows.Forms.Padding(6, 15, 6, 6)
        Me.grpTestMode.Size = New System.Drawing.Size(338, 70)
        Me.grpTestMode.TabIndex = 1
        Me.grpTestMode.TabStop = False
        Me.grpTestMode.Text = "测试控制"
        '
        'testModeLayout
        '
        Me.testModeLayout.ColumnCount = 2
        Me.testModeLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.testModeLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.testModeLayout.Controls.Add(Me.chkAutoTest, 0, 0)
        Me.testModeLayout.Controls.Add(Me.chkManualTest, 1, 0)
        Me.testModeLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.testModeLayout.Location = New System.Drawing.Point(6, 31)
        Me.testModeLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.testModeLayout.Name = "testModeLayout"
        Me.testModeLayout.RowCount = 1
        Me.testModeLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.testModeLayout.Size = New System.Drawing.Size(326, 33)
        Me.testModeLayout.TabIndex = 0
        '
        'chkAutoTest
        '
        Me.chkAutoTest.Appearance = System.Windows.Forms.Appearance.Button
        Me.chkAutoTest.BackColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.chkAutoTest.Checked = True
        Me.chkAutoTest.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkAutoTest.Dock = System.Windows.Forms.DockStyle.Fill
        Me.chkAutoTest.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.chkAutoTest.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.chkAutoTest.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.chkAutoTest.ForeColor = System.Drawing.Color.White
        Me.chkAutoTest.Location = New System.Drawing.Point(3, 3)
        Me.chkAutoTest.Name = "chkAutoTest"
        Me.chkAutoTest.Size = New System.Drawing.Size(157, 27)
        Me.chkAutoTest.TabIndex = 0
        Me.chkAutoTest.Text = "自动"
        Me.chkAutoTest.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.chkAutoTest.UseVisualStyleBackColor = False
        '
        'chkManualTest
        '
        Me.chkManualTest.Appearance = System.Windows.Forms.Appearance.Button
        Me.chkManualTest.BackColor = System.Drawing.Color.White
        Me.chkManualTest.Dock = System.Windows.Forms.DockStyle.Fill
        Me.chkManualTest.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(CType(CType(231, Byte), Integer), CType(CType(235, Byte), Integer), CType(CType(240, Byte), Integer))
        Me.chkManualTest.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.chkManualTest.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.chkManualTest.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.chkManualTest.Location = New System.Drawing.Point(166, 3)
        Me.chkManualTest.Name = "chkManualTest"
        Me.chkManualTest.Size = New System.Drawing.Size(157, 27)
        Me.chkManualTest.TabIndex = 1
        Me.chkManualTest.Text = "手动"
        Me.chkManualTest.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.chkManualTest.UseVisualStyleBackColor = False
        '
        'GroupBox8
        '
        Me.GroupBox8.AutoSize = True
        Me.GroupBox8.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.GroupBox8.BackColor = System.Drawing.Color.White
        Me.GroupBox8.Controls.Add(Me.probeUsageLayout)
        Me.GroupBox8.Dock = System.Windows.Forms.DockStyle.Top
        Me.GroupBox8.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.GroupBox8.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.GroupBox8.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.GroupBox8.Location = New System.Drawing.Point(6, 619)
        Me.GroupBox8.Margin = New System.Windows.Forms.Padding(0, 0, 0, 5)
        Me.GroupBox8.Name = "GroupBox8"
        Me.GroupBox8.Padding = New System.Windows.Forms.Padding(6, 15, 6, 6)
        Me.GroupBox8.Size = New System.Drawing.Size(338, 64)
        Me.GroupBox8.TabIndex = 65
        Me.GroupBox8.TabStop = False
        Me.GroupBox8.Text = "探针使用次数统计"
        '
        'probeUsageLayout
        '
        Me.probeUsageLayout.AutoSize = True
        Me.probeUsageLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.probeUsageLayout.BackColor = System.Drawing.Color.White
        Me.probeUsageLayout.ColumnCount = 3
        Me.probeUsageLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.probeUsageLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.probeUsageLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
        Me.probeUsageLayout.Controls.Add(Me.Label26, 0, 0)
        Me.probeUsageLayout.Controls.Add(Me.txtRemain, 1, 0)
        Me.probeUsageLayout.Controls.Add(Me.Label29, 2, 0)
        Me.probeUsageLayout.Dock = System.Windows.Forms.DockStyle.Top
        Me.probeUsageLayout.Location = New System.Drawing.Point(6, 31)
        Me.probeUsageLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.probeUsageLayout.Name = "probeUsageLayout"
        Me.probeUsageLayout.RowCount = 1
        Me.probeUsageLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
        Me.probeUsageLayout.Size = New System.Drawing.Size(326, 27)
        Me.probeUsageLayout.TabIndex = 54
        '
        'Label26
        '
        Me.Label26.BackColor = System.Drawing.Color.White
        Me.Label26.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label26.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label26.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.Label26.Location = New System.Drawing.Point(2, 2)
        Me.Label26.Margin = New System.Windows.Forms.Padding(2)
        Me.Label26.Name = "Label26"
        Me.Label26.Size = New System.Drawing.Size(105, 23)
        Me.Label26.TabIndex = 55
        Me.Label26.Text = "剩下使用次数 ="
        Me.Label26.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'txtRemain
        '
        Me.txtRemain.BackColor = System.Drawing.Color.White
        Me.txtRemain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtRemain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtRemain.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtRemain.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.txtRemain.Location = New System.Drawing.Point(111, 2)
        Me.txtRemain.Margin = New System.Windows.Forms.Padding(2)
        Me.txtRemain.Name = "txtRemain"
        Me.txtRemain.ReadOnly = True
        Me.txtRemain.Size = New System.Drawing.Size(184, 23)
        Me.txtRemain.TabIndex = 53
        '
        'Label29
        '
        Me.Label29.BackColor = System.Drawing.Color.White
        Me.Label29.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Label29.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Label29.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.Label29.Location = New System.Drawing.Point(299, 2)
        Me.Label29.Margin = New System.Windows.Forms.Padding(2)
        Me.Label29.Name = "Label29"
        Me.Label29.Size = New System.Drawing.Size(25, 23)
        Me.Label29.TabIndex = 56
        Me.Label29.Text = "次"
        Me.Label29.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'resetAlarmLayout
        '
        Me.resetAlarmLayout.ColumnCount = 1
        Me.resetAlarmLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.resetAlarmLayout.Controls.Add(Me.Button2, 0, 0)
        Me.resetAlarmLayout.Dock = System.Windows.Forms.DockStyle.Top
        Me.resetAlarmLayout.Location = New System.Drawing.Point(6, 775)
        Me.resetAlarmLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.resetAlarmLayout.Name = "resetAlarmLayout"
        Me.resetAlarmLayout.RowCount = 1
        Me.resetAlarmLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38.0!))
        Me.resetAlarmLayout.Size = New System.Drawing.Size(338, 38)
        Me.resetAlarmLayout.TabIndex = 73
        '
        'txtTotle
        '
        Me.txtTotle.BackColor = System.Drawing.Color.White
        Me.txtTotle.Font = New System.Drawing.Font("宋体", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.txtTotle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.txtTotle.Location = New System.Drawing.Point(232, 845)
        Me.txtTotle.Name = "txtTotle"
        Me.txtTotle.ReadOnly = True
        Me.txtTotle.Size = New System.Drawing.Size(10, 21)
        Me.txtTotle.TabIndex = 53
        Me.txtTotle.Text = "30000"
        Me.txtTotle.Visible = False
        '
        'channelGrid
        '
        Me.channelGrid.BackColor = System.Drawing.Color.White
        Me.channelGrid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.channelGrid.Dock = System.Windows.Forms.DockStyle.Fill
        Me.channelGrid.Location = New System.Drawing.Point(0, 126)
        Me.channelGrid.Margin = New System.Windows.Forms.Padding(0)
        Me.channelGrid.Name = "channelGrid"
        Me.channelGrid.Padding = New System.Windows.Forms.Padding(10, 8, 10, 10)
        Me.channelGrid.SelectionMode = software.ChannelSelectionMode.SingleChannel
        Me.channelGrid.Size = New System.Drawing.Size(1114, 776)
        Me.channelGrid.TabIndex = 71
        '
        'statisticsControl
        '
        Me.statisticsControl.BackColor = System.Drawing.Color.White
        Me.statisticsControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.statisticsControl.Dock = System.Windows.Forms.DockStyle.Fill
        Me.statisticsControl.Location = New System.Drawing.Point(0, 0)
        Me.statisticsControl.Margin = New System.Windows.Forms.Padding(0)
        Me.statisticsControl.Name = "statisticsControl"
        Me.statisticsControl.Padding = New System.Windows.Forms.Padding(16, 8, 16, 8)
        Me.statisticsControl.Size = New System.Drawing.Size(1114, 126)
        Me.statisticsControl.TabIndex = 70
        '
        'rootLayout
        '
        Me.rootLayout.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(246, Byte), Integer), CType(CType(250, Byte), Integer))
        Me.rootLayout.ColumnCount = 2
        Me.rootLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 350.0!))
        Me.rootLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.rootLayout.Controls.Add(Me.Panel1, 0, 0)
        Me.rootLayout.Controls.Add(Me.rightLayout, 1, 0)
        Me.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.rootLayout.Location = New System.Drawing.Point(0, 0)
        Me.rootLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.rootLayout.Name = "rootLayout"
        Me.rootLayout.RowCount = 1
        Me.rootLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.rootLayout.Size = New System.Drawing.Size(1464, 902)
        Me.rootLayout.TabIndex = 1
        '
        'rightLayout
        '
        Me.rightLayout.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(246, Byte), Integer), CType(CType(250, Byte), Integer))
        Me.rightLayout.ColumnCount = 1
        Me.rightLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.rightLayout.Controls.Add(Me.channelGrid, 0, 1)
        Me.rightLayout.Controls.Add(Me.statisticsControl, 0, 0)
        Me.rightLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.rightLayout.Location = New System.Drawing.Point(350, 0)
        Me.rightLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.rightLayout.Name = "rightLayout"
        Me.rightLayout.RowCount = 2
        Me.rightLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.0!))
        Me.rightLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 86.0!))
        Me.rightLayout.Size = New System.Drawing.Size(1114, 902)
        Me.rightLayout.TabIndex = 16
        '
        'frmTest
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(246, Byte), Integer), CType(CType(250, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(1464, 902)
        Me.Controls.Add(Me.rootLayout)
        Me.Controls.Add(Me.Pic_Bat)
        Me.DoubleBuffered = True
        Me.Name = "frmTest"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "测试"
        CType(Me.Pic_Bat, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.rangeSettingsLayout.ResumeLayout(False)
        Me.rangeSettingsLayout.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.controlBoardLayout.ResumeLayout(False)
        Me.controlBoardLayout.PerformLayout()
        Me.grpChannelSelection.ResumeLayout(False)
        Me.grpChannelSelection.PerformLayout()
        Me.channelSelectionLayout.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.GroupBox5.ResumeLayout(False)
        Me.GroupBox5.PerformLayout()
        Me.alarmSectionLayout.ResumeLayout(False)
        Me.alarmSectionLayout.PerformLayout()
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        Me.alarmTemperatureLayout.ResumeLayout(False)
        Me.alarmTemperatureLayout.PerformLayout()
        Me.Panel3.ResumeLayout(False)
        Me.Panel3.PerformLayout()
        Me.alarmStatusLayout.ResumeLayout(False)
        Me.alarmStatusLayout.PerformLayout()
        Me.smokeOneLayout.ResumeLayout(False)
        Me.smokeOneLayout.PerformLayout()
        Me.smokeTwoLayout.ResumeLayout(False)
        Me.smokeTwoLayout.PerformLayout()
        Me.deviceStatusLayout.ResumeLayout(False)
        Me.deviceStatusLayout.PerformLayout()
        Me.GroupBox7.ResumeLayout(False)
        Me.GroupBox7.PerformLayout()
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.leftContentPanel.ResumeLayout(False)
        Me.leftContentPanel.PerformLayout()
        Me.grpTestMode.ResumeLayout(False)
        Me.testModeLayout.ResumeLayout(False)
        Me.GroupBox8.ResumeLayout(False)
        Me.GroupBox8.PerformLayout()
        Me.probeUsageLayout.ResumeLayout(False)
        Me.probeUsageLayout.PerformLayout()
        Me.resetAlarmLayout.ResumeLayout(False)
        Me.rootLayout.ResumeLayout(False)
        Me.rightLayout.ResumeLayout(False)
        Me.ResumeLayout(False)

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
    Friend WithEvents chkAutoTest As CheckBox
    Friend WithEvents chkManualTest As CheckBox
    Private currentStationButton As RadioButton
    Friend WithEvents Pic_Bat As System.Windows.Forms.PictureBox
    Friend WithEvents SaveFileDialog1 As System.Windows.Forms.SaveFileDialog
    Friend WithEvents Timer1 As Timer
    Friend WithEvents Timer2 As Timer
    Friend WithEvents Timer4 As Timer
    Friend WithEvents statisticsControl As Global.software.StatisticsControl
    Friend WithEvents channelGrid As Global.software.ChannelGridControl
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
