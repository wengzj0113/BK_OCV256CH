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
        Me.components = New System.ComponentModel.Container()
        Me.statisticsLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.totalTitle = New System.Windows.Forms.Label()
        Me.passTitle = New System.Windows.Forms.Label()
        Me.ngTitle = New System.Windows.Forms.Label()
        Me.testingTitle = New System.Windows.Forms.Label()
        Me.idleTitle = New System.Windows.Forms.Label()
        Me.totalValue = New System.Windows.Forms.Label()
        Me.passValue = New System.Windows.Forms.Label()
        Me.ngValue = New System.Windows.Forms.Label()
        Me.testingValue = New System.Windows.Forms.Label()
        Me.idleValue = New System.Windows.Forms.Label()
        Me.legend = New System.Windows.Forms.TableLayoutPanel()
        Me.passLegend = New System.Windows.Forms.FlowLayoutPanel()
        Me.ngLegend = New System.Windows.Forms.FlowLayoutPanel()
        Me.testingLegend = New System.Windows.Forms.FlowLayoutPanel()
        Me.idleLegend = New System.Windows.Forms.FlowLayoutPanel()
        Me.passSwatch = New System.Windows.Forms.Label()
        Me.ngSwatch = New System.Windows.Forms.Label()
        Me.testingSwatch = New System.Windows.Forms.Label()
        Me.idleSwatch = New System.Windows.Forms.Label()
        Me.passLegendText = New System.Windows.Forms.Label()
        Me.ngLegendText = New System.Windows.Forms.Label()
        Me.testingLegendText = New System.Windows.Forms.Label()
        Me.idleLegendText = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        Me.statisticsLayout.SuspendLayout()
        Me.legend.SuspendLayout()
        Me.totalTitle.BackColor = System.Drawing.Color.White
        Me.totalTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.totalTitle.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.totalTitle.ForeColor = System.Drawing.Color.FromArgb(99, 112, 132)
        Me.totalTitle.Name = "totalTitle"
        Me.totalTitle.Text = "通道总数"
        Me.totalTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.passTitle.BackColor = System.Drawing.Color.White
        Me.passTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.passTitle.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.passTitle.ForeColor = System.Drawing.Color.FromArgb(99, 112, 132)
        Me.passTitle.Name = "passTitle"
        Me.passTitle.Text = "PASS"
        Me.passTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.ngTitle.BackColor = System.Drawing.Color.White
        Me.ngTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ngTitle.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.ngTitle.ForeColor = System.Drawing.Color.FromArgb(99, 112, 132)
        Me.ngTitle.Name = "ngTitle"
        Me.ngTitle.Text = "NG"
        Me.ngTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.testingTitle.BackColor = System.Drawing.Color.White
        Me.testingTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.testingTitle.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.testingTitle.ForeColor = System.Drawing.Color.FromArgb(99, 112, 132)
        Me.testingTitle.Name = "testingTitle"
        Me.testingTitle.Text = "测试中"
        Me.testingTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.idleTitle.BackColor = System.Drawing.Color.White
        Me.idleTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.idleTitle.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.idleTitle.ForeColor = System.Drawing.Color.FromArgb(99, 112, 132)
        Me.idleTitle.Name = "idleTitle"
        Me.idleTitle.Text = "空闲"
        Me.idleTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.totalValue.AutoEllipsis = False
        Me.totalValue.Dock = System.Windows.Forms.DockStyle.Fill
        Me.totalValue.Font = New System.Drawing.Font("Microsoft YaHei UI", 20.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.totalValue.ForeColor = System.Drawing.Color.FromArgb(23, 105, 232)
        Me.totalValue.Name = "totalValue"
        Me.totalValue.Padding = New System.Windows.Forms.Padding(0, 0, 4, 0)
        Me.totalValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.passValue.AutoEllipsis = False
        Me.passValue.Dock = System.Windows.Forms.DockStyle.Fill
        Me.passValue.Font = New System.Drawing.Font("Microsoft YaHei UI", 20.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.passValue.ForeColor = System.Drawing.Color.FromArgb(31, 164, 99)
        Me.passValue.Name = "passValue"
        Me.passValue.Padding = New System.Windows.Forms.Padding(0, 0, 4, 0)
        Me.passValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.ngValue.AutoEllipsis = False
        Me.ngValue.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ngValue.Font = New System.Drawing.Font("Microsoft YaHei UI", 20.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.ngValue.ForeColor = System.Drawing.Color.FromArgb(239, 62, 62)
        Me.ngValue.Name = "ngValue"
        Me.ngValue.Padding = New System.Windows.Forms.Padding(0, 0, 4, 0)
        Me.ngValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.testingValue.AutoEllipsis = False
        Me.testingValue.Dock = System.Windows.Forms.DockStyle.Fill
        Me.testingValue.Font = New System.Drawing.Font("Microsoft YaHei UI", 20.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.testingValue.ForeColor = System.Drawing.Color.FromArgb(23, 105, 232)
        Me.testingValue.Name = "testingValue"
        Me.testingValue.Padding = New System.Windows.Forms.Padding(0, 0, 4, 0)
        Me.testingValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.idleValue.AutoEllipsis = False
        Me.idleValue.Dock = System.Windows.Forms.DockStyle.Fill
        Me.idleValue.Font = New System.Drawing.Font("Microsoft YaHei UI", 20.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.idleValue.ForeColor = System.Drawing.Color.FromArgb(99, 112, 132)
        Me.idleValue.Name = "idleValue"
        Me.idleValue.Padding = New System.Windows.Forms.Padding(0, 0, 4, 0)
        Me.idleValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.passLegend.Dock = System.Windows.Forms.DockStyle.Fill
        Me.passLegend.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight
        Me.passLegend.Margin = New System.Windows.Forms.Padding(0)
        Me.passLegend.Name = "passLegend"
        Me.passLegend.Padding = New System.Windows.Forms.Padding(0, 2, 0, 0)
        Me.passLegend.WrapContents = False
        Me.passSwatch.BackColor = System.Drawing.Color.FromArgb(31, 164, 99)
        Me.passSwatch.Margin = New System.Windows.Forms.Padding(0, 2, 4, 0)
        Me.passSwatch.Name = "passSwatch"
        Me.passSwatch.Size = New System.Drawing.Size(11, 11)
        Me.passLegendText.AutoSize = True
        Me.passLegendText.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.passLegendText.ForeColor = System.Drawing.Color.FromArgb(36, 49, 66)
        Me.passLegendText.Name = "passLegendText"
        Me.passLegendText.Text = "PASS"
        Me.passLegend.Controls.Add(Me.passSwatch)
        Me.passLegend.Controls.Add(Me.passLegendText)
        Me.ngLegend.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ngLegend.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight
        Me.ngLegend.Margin = New System.Windows.Forms.Padding(0)
        Me.ngLegend.Name = "ngLegend"
        Me.ngLegend.Padding = New System.Windows.Forms.Padding(0, 2, 0, 0)
        Me.ngLegend.WrapContents = False
        Me.ngSwatch.BackColor = System.Drawing.Color.FromArgb(239, 62, 62)
        Me.ngSwatch.Margin = New System.Windows.Forms.Padding(0, 2, 4, 0)
        Me.ngSwatch.Name = "ngSwatch"
        Me.ngSwatch.Size = New System.Drawing.Size(11, 11)
        Me.ngLegendText.AutoSize = True
        Me.ngLegendText.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.ngLegendText.ForeColor = System.Drawing.Color.FromArgb(36, 49, 66)
        Me.ngLegendText.Name = "ngLegendText"
        Me.ngLegendText.Text = "NG"
        Me.ngLegend.Controls.Add(Me.ngSwatch)
        Me.ngLegend.Controls.Add(Me.ngLegendText)
        Me.testingLegend.Dock = System.Windows.Forms.DockStyle.Fill
        Me.testingLegend.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight
        Me.testingLegend.Margin = New System.Windows.Forms.Padding(0)
        Me.testingLegend.Name = "testingLegend"
        Me.testingLegend.Padding = New System.Windows.Forms.Padding(0, 2, 0, 0)
        Me.testingLegend.WrapContents = False
        Me.testingSwatch.BackColor = System.Drawing.Color.FromArgb(23, 105, 232)
        Me.testingSwatch.Margin = New System.Windows.Forms.Padding(0, 2, 4, 0)
        Me.testingSwatch.Name = "testingSwatch"
        Me.testingSwatch.Size = New System.Drawing.Size(11, 11)
        Me.testingLegendText.AutoSize = True
        Me.testingLegendText.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.testingLegendText.ForeColor = System.Drawing.Color.FromArgb(36, 49, 66)
        Me.testingLegendText.Name = "testingLegendText"
        Me.testingLegendText.Text = "测试中"
        Me.testingLegend.Controls.Add(Me.testingSwatch)
        Me.testingLegend.Controls.Add(Me.testingLegendText)
        Me.idleLegend.Dock = System.Windows.Forms.DockStyle.Fill
        Me.idleLegend.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight
        Me.idleLegend.Margin = New System.Windows.Forms.Padding(0)
        Me.idleLegend.Name = "idleLegend"
        Me.idleLegend.Padding = New System.Windows.Forms.Padding(0, 2, 0, 0)
        Me.idleLegend.WrapContents = False
        Me.idleSwatch.BackColor = System.Drawing.Color.FromArgb(99, 112, 132)
        Me.idleSwatch.Margin = New System.Windows.Forms.Padding(0, 2, 4, 0)
        Me.idleSwatch.Name = "idleSwatch"
        Me.idleSwatch.Size = New System.Drawing.Size(11, 11)
        Me.idleLegendText.AutoSize = True
        Me.idleLegendText.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.idleLegendText.ForeColor = System.Drawing.Color.FromArgb(36, 49, 66)
        Me.idleLegendText.Name = "idleLegendText"
        Me.idleLegendText.Text = "空闲"
        Me.idleLegend.Controls.Add(Me.idleSwatch)
        Me.idleLegend.Controls.Add(Me.idleLegendText)
        Me.statisticsLayout.BackColor = System.Drawing.Color.White
        Me.statisticsLayout.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.None
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
        Me.statisticsLayout.Padding = New System.Windows.Forms.Padding(0)
        Me.statisticsLayout.RowCount = 2
        Me.statisticsLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34.0!))
        Me.statisticsLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 66.0!))
        Me.statisticsLayout.Size = New System.Drawing.Size(528, 80)
        Me.statisticsLayout.TabIndex = 0
        Me.statisticsLayout.SetRowSpan(Me.legend, 2)
        Me.legend.BackColor = System.Drawing.Color.White
        Me.legend.ColumnCount = 2
        Me.legend.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.legend.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.legend.Controls.Add(Me.passLegend, 0, 0)
        Me.legend.Controls.Add(Me.ngLegend, 1, 0)
        Me.legend.Controls.Add(Me.testingLegend, 0, 1)
        Me.legend.Controls.Add(Me.idleLegend, 1, 1)
        Me.legend.Dock = System.Windows.Forms.DockStyle.Fill
        Me.legend.Margin = New System.Windows.Forms.Padding(0)
        Me.legend.Name = "legend"
        Me.legend.Padding = New System.Windows.Forms.Padding(4, 4, 0, 0)
        Me.legend.RowCount = 2
        Me.legend.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.legend.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.legend.Size = New System.Drawing.Size(95, 80)
        Me.legend.TabIndex = 1
        Me.DoubleBuffered = True
        Me.BackColor = System.Drawing.Color.White
        Me.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Controls.Add(Me.statisticsLayout)
        Me.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Name = "StatisticsControl"
        Me.Padding = New System.Windows.Forms.Padding(16, 8, 16, 8)
        Me.Size = New System.Drawing.Size(560, 96)
        Me.statisticsLayout.ResumeLayout(False)
        Me.legend.ResumeLayout(False)
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
End Class
