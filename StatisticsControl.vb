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
        Padding = New Padding(12)
        Dock = DockStyle.Top
        Height = 96

        totalValue = CreateValueLabel(UiTheme.Primary)
        passValue = CreateValueLabel(UiTheme.Success)
        ngValue = CreateValueLabel(UiTheme.Danger)
        testingValue = CreateValueLabel(UiTheme.Primary)
        idleValue = CreateValueLabel(UiTheme.TextSecondary)

        Dim layout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 5,
            .RowCount = 2,
            .BackColor = UiTheme.CardBackground,
            .CellBorderStyle = TableLayoutPanelCellBorderStyle.None
        }
        For i As Integer = 0 To 4
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, If(i = 0, 18.0F, 20.5F)))
        Next
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 24.0F))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))

        AddMetric(layout, 0, "通道总数", totalValue)
        AddMetric(layout, 1, "PASS", passValue)
        AddMetric(layout, 2, "NG", ngValue)
        AddMetric(layout, 3, "测试中", testingValue)
        AddMetric(layout, 4, "空闲", idleValue)
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
        Return New Label With {
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Font = New Font("Microsoft YaHei UI", 20.0F, FontStyle.Regular),
            .ForeColor = color,
            .AutoEllipsis = True
        }
    End Function

    Private Shared Sub AddMetric(ByVal layout As TableLayoutPanel, ByVal column As Integer, ByVal title As String, ByVal value As Label)
        Dim titleLabel As New Label With {
            .Text = title,
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Font = UiTheme.UiFontBold,
            .ForeColor = UiTheme.TextSecondary
        }
        layout.Controls.Add(titleLabel, column, 0)
        layout.Controls.Add(value, column, 1)
    End Sub

    Private Shared Function FormatMetric(ByVal count As Integer, ByVal rate As Double) As String
        Return String.Format("{0}  ({1:0.0}%)", count, rate)
    End Function
End Class
