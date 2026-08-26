Imports System.Drawing
Imports System.Windows.Forms

Public Structure ChannelStatistics
    Public Total As Integer
    Public PassCount As Integer
    Public NgCount As Integer
    Public TestingCount As Integer
    Public IdleCount As Integer
    Public ReadOnly Property PassRate As Double
        Get
            Return RateFor(PassCount)
        End Get
    End Property
    Public ReadOnly Property NgRate As Double
        Get
            Return RateFor(NgCount)
        End Get
    End Property
    Public ReadOnly Property TestingRate As Double
        Get
            Return RateFor(TestingCount)
        End Get
    End Property
    Public ReadOnly Property IdleRate As Double
        Get
            Return RateFor(IdleCount)
        End Get
    End Property
    Private Function RateFor(ByVal count As Integer) As Double
        If Total <= 0 Then Return 0.0R
        Return count * 100.0R / Total
    End Function
End Structure

Public Class StatisticsControl
    Inherits UserControl
    Private ReadOnly totalValue As Label
    Private ReadOnly passValue As Label
    Private ReadOnly ngValue As Label
    Private ReadOnly testingValue As Label
    Private ReadOnly idleValue As Label

    Public Sub New()
        DoubleBuffered = True
        BackColor = UiTheme.CardBackground
        BorderStyle = BorderStyle.FixedSingle
        Padding = New Padding(16, 8, 16, 8)
        Dock = DockStyle.Fill
        AutoSize = False
        totalValue = CreateValueLabel(UiTheme.Primary)
        passValue = CreateValueLabel(UiTheme.Success)
        ngValue = CreateValueLabel(UiTheme.Danger)
        testingValue = CreateValueLabel(UiTheme.Primary)
        idleValue = CreateValueLabel(UiTheme.TextSecondary)

        Dim layout As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 6, .RowCount = 2, .BackColor = UiTheme.CardBackground, .CellBorderStyle = TableLayoutPanelCellBorderStyle.None}
        Dim widths() As Single = {14.0F, 17.0F, 17.0F, 17.0F, 17.0F, 18.0F}
        For i As Integer = 0 To widths.Length - 1
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, widths(i)))
        Next
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 34.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 66.0F))
        AddMetric(layout, 0, "通道总数", totalValue)
        AddMetric(layout, 1, "PASS", passValue)
        AddMetric(layout, 2, "NG", ngValue)
        AddMetric(layout, 3, "测试中", testingValue)
        AddMetric(layout, 4, "空闲", idleValue)
        AddLegend(layout, 5)
        Controls.Add(layout)
    End Sub

    Public Sub UpdateStatistics(ByVal statistics As ChannelStatistics)
        totalValue.Text = statistics.Total.ToString()
        passValue.Text = FormatMetric(statistics.PassCount, statistics.PassRate)
        ngValue.Text = FormatMetric(statistics.NgCount, statistics.NgRate)
        testingValue.Text = FormatMetric(statistics.TestingCount, statistics.TestingRate)
        idleValue.Text = FormatMetric(statistics.IdleCount, statistics.IdleRate)
    End Sub

    Private Shared Function CreateValueLabel(ByVal color As Color) As Label
        Return New Label With {.Dock = DockStyle.Fill, .TextAlign = ContentAlignment.MiddleLeft, .Font = UiTheme.UiFontMetric, .ForeColor = color, .AutoEllipsis = False, .Padding = New Padding(0, 0, 4, 0)}
    End Function

    Private Shared Sub AddMetric(ByVal layout As TableLayoutPanel, ByVal column As Integer, ByVal title As String, ByVal value As Label)
        Dim titleLabel As New Label With {.Text = title, .Dock = DockStyle.Fill, .TextAlign = ContentAlignment.MiddleLeft, .Font = UiTheme.UiFontBold, .ForeColor = UiTheme.TextSecondary}
        layout.Controls.Add(titleLabel, column, 0)
        layout.Controls.Add(value, column, 1)
    End Sub

    Private Shared Sub AddLegend(ByVal layout As TableLayoutPanel, ByVal column As Integer)
        Dim legend As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 2, .RowCount = 2, .BackColor = UiTheme.CardBackground, .Padding = New Padding(4, 4, 0, 0)}
        legend.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        legend.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        legend.RowStyles.Add(New RowStyle(SizeType.Percent, 50.0F))
        legend.RowStyles.Add(New RowStyle(SizeType.Percent, 50.0F))
        AddLegendItem(legend, 0, 0, "PASS", UiTheme.Success)
        AddLegendItem(legend, 1, 0, "NG", UiTheme.Danger)
        AddLegendItem(legend, 0, 1, "测试中", UiTheme.Primary)
        AddLegendItem(legend, 1, 1, "空闲", UiTheme.TextSecondary)
        layout.Controls.Add(legend, column, 0)
        layout.SetRowSpan(legend, 2)
    End Sub

    Private Shared Sub AddLegendItem(ByVal legend As TableLayoutPanel, ByVal column As Integer, ByVal row As Integer, ByVal caption As String, ByVal color As Color)
        Dim item As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .FlowDirection = FlowDirection.LeftToRight, .WrapContents = False, .Padding = New Padding(0, 2, 0, 0)}
        item.Controls.Add(New Label With {.AutoSize = False, .Size = New Size(11, 11), .BackColor = color, .Margin = New Padding(0, 2, 4, 0)})
        item.Controls.Add(New Label With {.AutoSize = True, .Text = caption, .Font = UiTheme.UiFont, .ForeColor = UiTheme.TextPrimary})
        legend.Controls.Add(item, column, row)
    End Sub

    Private Shared Function FormatMetric(ByVal count As Integer, ByVal rate As Double) As String
        Return String.Format("{0}  ({1:0.0}%)", count, rate)
    End Function
End Class
