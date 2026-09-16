<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class StatisticsControl
    Inherits System.Windows.Forms.UserControl

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.statisticsLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.totalTitle = New System.Windows.Forms.Label()
        Me.totalValue = New System.Windows.Forms.Label()
        Me.passTitle = New System.Windows.Forms.Label()
        Me.passValue = New System.Windows.Forms.Label()
        Me.ngTitle = New System.Windows.Forms.Label()
        Me.ngValue = New System.Windows.Forms.Label()
        Me.testingTitle = New System.Windows.Forms.Label()
        Me.testingValue = New System.Windows.Forms.Label()
        Me.idleTitle = New System.Windows.Forms.Label()
        Me.idleValue = New System.Windows.Forms.Label()
        Me.legend = New System.Windows.Forms.TableLayoutPanel()
        Me.passLegend = New System.Windows.Forms.FlowLayoutPanel()
        Me.passSwatch = New System.Windows.Forms.Label()
        Me.passLegendText = New System.Windows.Forms.Label()
        Me.ngLegend = New System.Windows.Forms.FlowLayoutPanel()
        Me.ngSwatch = New System.Windows.Forms.Label()
        Me.ngLegendText = New System.Windows.Forms.Label()
        Me.testingLegend = New System.Windows.Forms.FlowLayoutPanel()
        Me.testingSwatch = New System.Windows.Forms.Label()
        Me.testingLegendText = New System.Windows.Forms.Label()
        Me.idleLegend = New System.Windows.Forms.FlowLayoutPanel()
        Me.idleSwatch = New System.Windows.Forms.Label()
        Me.idleLegendText = New System.Windows.Forms.Label()
        Me.voltageLegend = New System.Windows.Forms.FlowLayoutPanel()
        Me.voltageSwatch = New System.Windows.Forms.Label()
        Me.voltageLegendText = New System.Windows.Forms.Label()
        Me.statisticsLayout.SuspendLayout()
        Me.legend.SuspendLayout()
        Me.passLegend.SuspendLayout()
        Me.ngLegend.SuspendLayout()
        Me.testingLegend.SuspendLayout()
        Me.idleLegend.SuspendLayout()
        Me.voltageLegend.SuspendLayout()
        Me.SuspendLayout()
        '
        'statisticsLayout
        '
        Me.statisticsLayout.BackColor = System.Drawing.Color.White
        Me.statisticsLayout.ColumnCount = 6
        Me.statisticsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.0!))
        Me.statisticsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17.0!))
        Me.statisticsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17.0!))
        Me.statisticsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17.0!))
        Me.statisticsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17.0!))
        Me.statisticsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 18.0!))
        Me.statisticsLayout.Controls.Add(Me.totalTitle, 0, 0)
        Me.statisticsLayout.Controls.Add(Me.totalValue, 0, 1)
        Me.statisticsLayout.Controls.Add(Me.passTitle, 1, 0)
        Me.statisticsLayout.Controls.Add(Me.passValue, 1, 1)
        Me.statisticsLayout.Controls.Add(Me.ngTitle, 2, 0)
        Me.statisticsLayout.Controls.Add(Me.ngValue, 2, 1)
        Me.statisticsLayout.Controls.Add(Me.testingTitle, 3, 0)
        Me.statisticsLayout.Controls.Add(Me.testingValue, 3, 1)
        Me.statisticsLayout.Controls.Add(Me.idleTitle, 4, 0)
        Me.statisticsLayout.Controls.Add(Me.idleValue, 4, 1)
        Me.statisticsLayout.Controls.Add(Me.legend, 5, 0)
        Me.statisticsLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.statisticsLayout.Location = New System.Drawing.Point(16, 8)
        Me.statisticsLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.statisticsLayout.Name = "statisticsLayout"
        Me.statisticsLayout.RowCount = 2
        Me.statisticsLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34.0!))
        Me.statisticsLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 66.0!))
        Me.statisticsLayout.Size = New System.Drawing.Size(1500, 859)
        Me.statisticsLayout.TabIndex = 0
        '
        'totalTitle
        '
        Me.totalTitle.BackColor = System.Drawing.Color.White
        Me.totalTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.totalTitle.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.totalTitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(99, Byte), Integer), CType(CType(112, Byte), Integer), CType(CType(132, Byte), Integer))
        Me.totalTitle.Location = New System.Drawing.Point(3, 0)
        Me.totalTitle.Name = "totalTitle"
        Me.totalTitle.Size = New System.Drawing.Size(204, 292)
        Me.totalTitle.TabIndex = 0
        Me.totalTitle.Text = "通道总数"
        Me.totalTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'totalValue
        '
        Me.totalValue.Dock = System.Windows.Forms.DockStyle.Fill
        Me.totalValue.Font = New System.Drawing.Font("Microsoft YaHei UI", 20.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.totalValue.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.totalValue.Location = New System.Drawing.Point(3, 292)
        Me.totalValue.Name = "totalValue"
        Me.totalValue.Padding = New System.Windows.Forms.Padding(0, 0, 4, 0)
        Me.totalValue.Size = New System.Drawing.Size(204, 567)
        Me.totalValue.TabIndex = 1
        Me.totalValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'passTitle
        '
        Me.passTitle.BackColor = System.Drawing.Color.White
        Me.passTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.passTitle.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.passTitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(99, Byte), Integer), CType(CType(112, Byte), Integer), CType(CType(132, Byte), Integer))
        Me.passTitle.Location = New System.Drawing.Point(213, 0)
        Me.passTitle.Name = "passTitle"
        Me.passTitle.Size = New System.Drawing.Size(249, 292)
        Me.passTitle.TabIndex = 2
        Me.passTitle.Text = "PASS"
        Me.passTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'passValue
        '
        Me.passValue.Dock = System.Windows.Forms.DockStyle.Fill
        Me.passValue.Font = New System.Drawing.Font("Microsoft YaHei UI", 20.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.passValue.ForeColor = System.Drawing.Color.FromArgb(CType(CType(24, Byte), Integer), CType(CType(169, Byte), Integer), CType(CType(104, Byte), Integer))
        Me.passValue.Location = New System.Drawing.Point(213, 292)
        Me.passValue.Name = "passValue"
        Me.passValue.Padding = New System.Windows.Forms.Padding(0, 0, 4, 0)
        Me.passValue.Size = New System.Drawing.Size(249, 567)
        Me.passValue.TabIndex = 3
        Me.passValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'ngTitle
        '
        Me.ngTitle.BackColor = System.Drawing.Color.White
        Me.ngTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ngTitle.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.ngTitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(99, Byte), Integer), CType(CType(112, Byte), Integer), CType(CType(132, Byte), Integer))
        Me.ngTitle.Location = New System.Drawing.Point(468, 0)
        Me.ngTitle.Name = "ngTitle"
        Me.ngTitle.Size = New System.Drawing.Size(249, 292)
        Me.ngTitle.TabIndex = 4
        Me.ngTitle.Text = "NG"
        Me.ngTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'ngValue
        '
        Me.ngValue.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ngValue.Font = New System.Drawing.Font("Microsoft YaHei UI", 20.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.ngValue.ForeColor = System.Drawing.Color.FromArgb(CType(CType(239, Byte), Integer), CType(CType(61, Byte), Integer), CType(CType(67, Byte), Integer))
        Me.ngValue.Location = New System.Drawing.Point(468, 292)
        Me.ngValue.Name = "ngValue"
        Me.ngValue.Padding = New System.Windows.Forms.Padding(0, 0, 4, 0)
        Me.ngValue.Size = New System.Drawing.Size(249, 567)
        Me.ngValue.TabIndex = 5
        Me.ngValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'testingTitle
        '
        Me.testingTitle.BackColor = System.Drawing.Color.White
        Me.testingTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.testingTitle.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.testingTitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(99, Byte), Integer), CType(CType(112, Byte), Integer), CType(CType(132, Byte), Integer))
        Me.testingTitle.Location = New System.Drawing.Point(723, 0)
        Me.testingTitle.Name = "testingTitle"
        Me.testingTitle.Size = New System.Drawing.Size(249, 292)
        Me.testingTitle.TabIndex = 6
        Me.testingTitle.Text = "测试中"
        Me.testingTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'testingValue
        '
        Me.testingValue.Dock = System.Windows.Forms.DockStyle.Fill
        Me.testingValue.Font = New System.Drawing.Font("Microsoft YaHei UI", 20.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.testingValue.ForeColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.testingValue.Location = New System.Drawing.Point(723, 292)
        Me.testingValue.Name = "testingValue"
        Me.testingValue.Padding = New System.Windows.Forms.Padding(0, 0, 4, 0)
        Me.testingValue.Size = New System.Drawing.Size(249, 567)
        Me.testingValue.TabIndex = 7
        Me.testingValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'idleTitle
        '
        Me.idleTitle.BackColor = System.Drawing.Color.White
        Me.idleTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.idleTitle.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.idleTitle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(99, Byte), Integer), CType(CType(112, Byte), Integer), CType(CType(132, Byte), Integer))
        Me.idleTitle.Location = New System.Drawing.Point(978, 0)
        Me.idleTitle.Name = "idleTitle"
        Me.idleTitle.Size = New System.Drawing.Size(249, 292)
        Me.idleTitle.TabIndex = 8
        Me.idleTitle.Text = "空闲"
        Me.idleTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'idleValue
        '
        Me.idleValue.Dock = System.Windows.Forms.DockStyle.Fill
        Me.idleValue.Font = New System.Drawing.Font("Microsoft YaHei UI", 20.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.idleValue.ForeColor = System.Drawing.Color.FromArgb(CType(CType(101, Byte), Integer), CType(CType(113, Byte), Integer), CType(CType(132, Byte), Integer))
        Me.idleValue.Location = New System.Drawing.Point(978, 292)
        Me.idleValue.Name = "idleValue"
        Me.idleValue.Padding = New System.Windows.Forms.Padding(0, 0, 4, 0)
        Me.idleValue.Size = New System.Drawing.Size(249, 567)
        Me.idleValue.TabIndex = 9
        Me.idleValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'legend
        '
        Me.legend.BackColor = System.Drawing.Color.White
        Me.legend.ColumnCount = 2
        Me.legend.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.legend.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.legend.Controls.Add(Me.passLegend, 0, 0)
        Me.legend.Controls.Add(Me.ngLegend, 1, 0)
        Me.legend.Controls.Add(Me.testingLegend, 0, 1)
        Me.legend.Controls.Add(Me.idleLegend, 1, 1)
        Me.legend.Controls.Add(Me.voltageLegend, 0, 2)
        Me.legend.Dock = System.Windows.Forms.DockStyle.Fill
        Me.legend.Location = New System.Drawing.Point(1230, 0)
        Me.legend.Margin = New System.Windows.Forms.Padding(0)
        Me.legend.Name = "legend"
        Me.legend.Padding = New System.Windows.Forms.Padding(4, 4, 0, 0)
        Me.legend.RowCount = 3
        Me.statisticsLayout.SetRowSpan(Me.legend, 2)
        Me.legend.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333!))
        Me.legend.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333!))
        Me.legend.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333!))
        Me.legend.Size = New System.Drawing.Size(270, 859)
        Me.legend.TabIndex = 1
        '
        'passLegend
        '
        Me.passLegend.Controls.Add(Me.passSwatch)
        Me.passLegend.Controls.Add(Me.passLegendText)
        Me.passLegend.Dock = System.Windows.Forms.DockStyle.Fill
        Me.passLegend.Location = New System.Drawing.Point(4, 4)
        Me.passLegend.Margin = New System.Windows.Forms.Padding(0)
        Me.passLegend.Name = "passLegend"
        Me.passLegend.Padding = New System.Windows.Forms.Padding(0, 2, 0, 0)
        Me.passLegend.Size = New System.Drawing.Size(133, 285)
        Me.passLegend.TabIndex = 0
        Me.passLegend.WrapContents = False
        '
        'passSwatch
        '
        Me.passSwatch.BackColor = System.Drawing.Color.FromArgb(CType(CType(24, Byte), Integer), CType(CType(169, Byte), Integer), CType(CType(104, Byte), Integer))
        Me.passSwatch.Location = New System.Drawing.Point(0, 4)
        Me.passSwatch.Margin = New System.Windows.Forms.Padding(0, 2, 4, 0)
        Me.passSwatch.Name = "passSwatch"
        Me.passSwatch.Size = New System.Drawing.Size(11, 11)
        Me.passSwatch.TabIndex = 0
        '
        'passLegendText
        '
        Me.passLegendText.AutoSize = True
        Me.passLegendText.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.passLegendText.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.passLegendText.Location = New System.Drawing.Point(18, 2)
        Me.passLegendText.Name = "passLegendText"
        Me.passLegendText.Size = New System.Drawing.Size(37, 17)
        Me.passLegendText.TabIndex = 1
        Me.passLegendText.Text = "PASS"
        '
        'ngLegend
        '
        Me.ngLegend.Controls.Add(Me.ngSwatch)
        Me.ngLegend.Controls.Add(Me.ngLegendText)
        Me.ngLegend.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ngLegend.Location = New System.Drawing.Point(137, 4)
        Me.ngLegend.Margin = New System.Windows.Forms.Padding(0)
        Me.ngLegend.Name = "ngLegend"
        Me.ngLegend.Padding = New System.Windows.Forms.Padding(0, 2, 0, 0)
        Me.ngLegend.Size = New System.Drawing.Size(133, 285)
        Me.ngLegend.TabIndex = 1
        Me.ngLegend.WrapContents = False
        '
        'ngSwatch
        '
        Me.ngSwatch.BackColor = System.Drawing.Color.FromArgb(CType(CType(239, Byte), Integer), CType(CType(61, Byte), Integer), CType(CType(67, Byte), Integer))
        Me.ngSwatch.Location = New System.Drawing.Point(0, 4)
        Me.ngSwatch.Margin = New System.Windows.Forms.Padding(0, 2, 4, 0)
        Me.ngSwatch.Name = "ngSwatch"
        Me.ngSwatch.Size = New System.Drawing.Size(11, 11)
        Me.ngSwatch.TabIndex = 0
        '
        'ngLegendText
        '
        Me.ngLegendText.AutoSize = True
        Me.ngLegendText.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.ngLegendText.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.ngLegendText.Location = New System.Drawing.Point(18, 2)
        Me.ngLegendText.Name = "ngLegendText"
        Me.ngLegendText.Size = New System.Drawing.Size(55, 17)
        Me.ngLegendText.TabIndex = 1
        Me.ngLegendText.Text = "内阻 NG"
        '
        'testingLegend
        '
        Me.testingLegend.Controls.Add(Me.testingSwatch)
        Me.testingLegend.Controls.Add(Me.testingLegendText)
        Me.testingLegend.Dock = System.Windows.Forms.DockStyle.Fill
        Me.testingLegend.Location = New System.Drawing.Point(4, 289)
        Me.testingLegend.Margin = New System.Windows.Forms.Padding(0)
        Me.testingLegend.Name = "testingLegend"
        Me.testingLegend.Padding = New System.Windows.Forms.Padding(0, 2, 0, 0)
        Me.testingLegend.Size = New System.Drawing.Size(133, 285)
        Me.testingLegend.TabIndex = 2
        Me.testingLegend.WrapContents = False
        '
        'testingSwatch
        '
        Me.testingSwatch.BackColor = System.Drawing.Color.FromArgb(CType(CType(23, Byte), Integer), CType(CType(105, Byte), Integer), CType(CType(232, Byte), Integer))
        Me.testingSwatch.Location = New System.Drawing.Point(0, 4)
        Me.testingSwatch.Margin = New System.Windows.Forms.Padding(0, 2, 4, 0)
        Me.testingSwatch.Name = "testingSwatch"
        Me.testingSwatch.Size = New System.Drawing.Size(11, 11)
        Me.testingSwatch.TabIndex = 0
        '
        'testingLegendText
        '
        Me.testingLegendText.AutoSize = True
        Me.testingLegendText.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.testingLegendText.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.testingLegendText.Location = New System.Drawing.Point(18, 2)
        Me.testingLegendText.Name = "testingLegendText"
        Me.testingLegendText.Size = New System.Drawing.Size(44, 17)
        Me.testingLegendText.TabIndex = 1
        Me.testingLegendText.Text = "测试中"
        '
        'idleLegend
        '
        Me.idleLegend.Controls.Add(Me.idleSwatch)
        Me.idleLegend.Controls.Add(Me.idleLegendText)
        Me.idleLegend.Dock = System.Windows.Forms.DockStyle.Fill
        Me.idleLegend.Location = New System.Drawing.Point(137, 289)
        Me.idleLegend.Margin = New System.Windows.Forms.Padding(0)
        Me.idleLegend.Name = "idleLegend"
        Me.idleLegend.Padding = New System.Windows.Forms.Padding(0, 2, 0, 0)
        Me.idleLegend.Size = New System.Drawing.Size(133, 285)
        Me.idleLegend.TabIndex = 3
        Me.idleLegend.WrapContents = False
        '
        'idleSwatch
        '
        Me.idleSwatch.BackColor = System.Drawing.Color.FromArgb(CType(CType(101, Byte), Integer), CType(CType(113, Byte), Integer), CType(CType(132, Byte), Integer))
        Me.idleSwatch.Location = New System.Drawing.Point(0, 4)
        Me.idleSwatch.Margin = New System.Windows.Forms.Padding(0, 2, 4, 0)
        Me.idleSwatch.Name = "idleSwatch"
        Me.idleSwatch.Size = New System.Drawing.Size(11, 11)
        Me.idleSwatch.TabIndex = 0
        '
        'idleLegendText
        '
        Me.idleLegendText.AutoSize = True
        Me.idleLegendText.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.idleLegendText.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.idleLegendText.Location = New System.Drawing.Point(18, 2)
        Me.idleLegendText.Name = "idleLegendText"
        Me.idleLegendText.Size = New System.Drawing.Size(32, 17)
        Me.idleLegendText.TabIndex = 1
        Me.idleLegendText.Text = "空闲"
        '
        'voltageLegend
        '
        Me.voltageLegend.Controls.Add(Me.voltageSwatch)
        Me.voltageLegend.Controls.Add(Me.voltageLegendText)
        Me.voltageLegend.Dock = System.Windows.Forms.DockStyle.Fill
        Me.voltageLegend.Location = New System.Drawing.Point(4, 574)
        Me.voltageLegend.Margin = New System.Windows.Forms.Padding(0)
        Me.voltageLegend.Name = "voltageLegend"
        Me.voltageLegend.Padding = New System.Windows.Forms.Padding(0, 2, 0, 0)
        Me.voltageLegend.Size = New System.Drawing.Size(133, 285)
        Me.voltageLegend.TabIndex = 4
        Me.voltageLegend.WrapContents = False
        '
        'voltageSwatch
        '
        Me.voltageSwatch.BackColor = System.Drawing.Color.FromArgb(CType(CType(242, Byte), Integer), CType(CType(118, Byte), Integer), CType(CType(61, Byte), Integer))
        Me.voltageSwatch.Location = New System.Drawing.Point(0, 4)
        Me.voltageSwatch.Margin = New System.Windows.Forms.Padding(0, 2, 4, 0)
        Me.voltageSwatch.Name = "voltageSwatch"
        Me.voltageSwatch.Size = New System.Drawing.Size(11, 11)
        Me.voltageSwatch.TabIndex = 0
        '
        'voltageLegendText
        '
        Me.voltageLegendText.AutoSize = True
        Me.voltageLegendText.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.voltageLegendText.ForeColor = System.Drawing.Color.FromArgb(CType(CType(36, Byte), Integer), CType(CType(49, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.voltageLegendText.Location = New System.Drawing.Point(18, 2)
        Me.voltageLegendText.Name = "voltageLegendText"
        Me.voltageLegendText.Size = New System.Drawing.Size(55, 17)
        Me.voltageLegendText.TabIndex = 1
        Me.voltageLegendText.Text = "电压 NG"
        '
        'StatisticsControl
        '
        Me.BackColor = System.Drawing.Color.White
        Me.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Controls.Add(Me.statisticsLayout)
        Me.DoubleBuffered = True
        Me.Name = "StatisticsControl"
        Me.Padding = New System.Windows.Forms.Padding(16, 8, 16, 8)
        Me.Size = New System.Drawing.Size(1532, 875)
        Me.statisticsLayout.ResumeLayout(False)
        Me.legend.ResumeLayout(False)
        Me.passLegend.ResumeLayout(False)
        Me.passLegend.PerformLayout()
        Me.ngLegend.ResumeLayout(False)
        Me.ngLegend.PerformLayout()
        Me.testingLegend.ResumeLayout(False)
        Me.testingLegend.PerformLayout()
        Me.idleLegend.ResumeLayout(False)
        Me.idleLegend.PerformLayout()
        Me.voltageLegend.ResumeLayout(False)
        Me.voltageLegend.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Private statisticsLayout As System.Windows.Forms.TableLayoutPanel
    Private totalTitle As System.Windows.Forms.Label
    Private passTitle As System.Windows.Forms.Label
    Private ngTitle As System.Windows.Forms.Label
    Private testingTitle As System.Windows.Forms.Label
    Private idleTitle As System.Windows.Forms.Label
    Private totalValue As System.Windows.Forms.Label
    Private passValue As System.Windows.Forms.Label
    Private ngValue As System.Windows.Forms.Label
    Private testingValue As System.Windows.Forms.Label
    Private idleValue As System.Windows.Forms.Label
    Private legend As System.Windows.Forms.TableLayoutPanel
    Private passLegend As System.Windows.Forms.FlowLayoutPanel
    Private ngLegend As System.Windows.Forms.FlowLayoutPanel
    Private testingLegend As System.Windows.Forms.FlowLayoutPanel
    Private idleLegend As System.Windows.Forms.FlowLayoutPanel
    Private passSwatch As System.Windows.Forms.Label
    Private ngSwatch As System.Windows.Forms.Label
    Private testingSwatch As System.Windows.Forms.Label
    Private idleSwatch As System.Windows.Forms.Label
    Private passLegendText As System.Windows.Forms.Label
    Private ngLegendText As System.Windows.Forms.Label
    Private testingLegendText As System.Windows.Forms.Label
    Private idleLegendText As System.Windows.Forms.Label
    Private voltageLegend As System.Windows.Forms.FlowLayoutPanel
    Private voltageSwatch As System.Windows.Forms.Label
    Private voltageLegendText As System.Windows.Forms.Label
End Class
