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
        Me.totalValue = CreateValueLabel(UiTheme.Primary)
        Me.passValue = CreateValueLabel(UiTheme.Success)
        Me.ngValue = CreateValueLabel(UiTheme.Danger)
        Me.testingValue = CreateValueLabel(UiTheme.Primary)
        Me.idleValue = CreateValueLabel(UiTheme.TextSecondary)
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
        ConfigureTitleLabel(Me.totalTitle, "通道总数")
        ConfigureTitleLabel(Me.passTitle, "PASS")
        ConfigureTitleLabel(Me.ngTitle, "NG")
        ConfigureTitleLabel(Me.testingTitle, "测试中")
        ConfigureTitleLabel(Me.idleTitle, "空闲")
        ConfigureLegendItem(Me.passLegend, Me.passSwatch, Me.passLegendText, "PASS", UiTheme.Success)
        ConfigureLegendItem(Me.ngLegend, Me.ngSwatch, Me.ngLegendText, "NG", UiTheme.Danger)
        ConfigureLegendItem(Me.testingLegend, Me.testingSwatch, Me.testingLegendText, "测试中", UiTheme.Primary)
        ConfigureLegendItem(Me.idleLegend, Me.idleSwatch, Me.idleLegendText, "空闲", UiTheme.TextSecondary)
        Me.statisticsLayout.BackColor = UiTheme.CardBackground
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
        Me.legend.BackColor = UiTheme.CardBackground
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
        Me.BackColor = UiTheme.CardBackground
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

    Private Function CreateValueLabel(ByVal color As Color) As System.Windows.Forms.Label
        Return New System.Windows.Forms.Label With {
            .AutoEllipsis = False,
            .Dock = System.Windows.Forms.DockStyle.Fill,
            .Font = UiTheme.UiFontMetric,
            .ForeColor = color,
            .Padding = New System.Windows.Forms.Padding(0, 0, 4, 0),
            .TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        }
    End Function

    Private Sub ConfigureTitleLabel(ByVal label As System.Windows.Forms.Label, ByVal text As String)
        label.BackColor = UiTheme.CardBackground
        label.Dock = System.Windows.Forms.DockStyle.Fill
        label.Font = UiTheme.UiFontBold
        label.ForeColor = UiTheme.TextSecondary
        label.Text = text
        label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
    End Sub

    Private Sub ConfigureLegendItem(ByVal flow As System.Windows.Forms.FlowLayoutPanel,
                                    ByVal swatch As System.Windows.Forms.Label,
                                    ByVal caption As System.Windows.Forms.Label,
                                    ByVal text As String,
                                    ByVal color As Color)
        flow.Dock = System.Windows.Forms.DockStyle.Fill
        flow.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight
        flow.Margin = New System.Windows.Forms.Padding(0)
        flow.Padding = New System.Windows.Forms.Padding(0, 2, 0, 0)
        flow.WrapContents = False
        swatch.BackColor = color
        swatch.Margin = New System.Windows.Forms.Padding(0, 2, 4, 0)
        swatch.Size = New System.Drawing.Size(11, 11)
        caption.AutoSize = True
        caption.Font = UiTheme.UiFont
        caption.ForeColor = UiTheme.TextPrimary
        caption.Text = text
        flow.Controls.Add(swatch)
        flow.Controls.Add(caption)
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
