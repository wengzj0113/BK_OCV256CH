Imports System.Drawing
Imports System.Windows.Forms

Public Structure ChannelDisplayData
    Public ChannelNo As Integer
    Public Row As Integer
    Public Column As Integer
    Public Barcode As String
    Public Ocv1 As Nullable(Of Single)
    Public Ir1 As Nullable(Of Single)
    Public StatusText As String
    Public ResultText As String
End Structure

Public Class ChannelSelectedEventArgs
    Inherits EventArgs

    Public ReadOnly Property Data As ChannelDisplayData

    Public Sub New(ByVal data As ChannelDisplayData)
        Me.Data = data
    End Sub
End Class

Public Class ChannelTileControl
    Inherits UserControl

    Private ReadOnly channelLabel As Label
    Private ReadOnly toolTip As ToolTip
    Private currentData As ChannelDisplayData

    Public Event ChannelSelected As EventHandler(Of ChannelSelectedEventArgs)

    Public Sub New()
        DoubleBuffered = True
        Margin = New Padding(3)
        Padding = New Padding(2)
        MinimumSize = New Size(44, 30)
        BackColor = UiTheme.CardBackground
        Cursor = Cursors.Hand

        channelLabel = New Label With {
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleCenter,
            .Font = UiTheme.UiFont,
            .ForeColor = UiTheme.TextPrimary,
            .AutoEllipsis = True
        }
        Controls.Add(channelLabel)

        toolTip = New ToolTip With {.InitialDelay = 250, .ReshowDelay = 100, .AutoPopDelay = 10000}
        AddHandler Me.Click, AddressOf HandleSelected
        AddHandler channelLabel.Click, AddressOf HandleSelected
        AddHandler Me.MouseEnter, AddressOf HandleMouseEnter
        AddHandler channelLabel.MouseEnter, AddressOf HandleMouseEnter
    End Sub

    Public Sub UpdateDisplay(ByVal data As ChannelDisplayData)
        currentData = data
        channelLabel.Text = String.Format("{0}-{1}", data.Row, data.Column)

        Dim background As Color
        Dim foreground As Color
        GetStatusColors(data.StatusText, background, foreground)
        BackColor = background
        channelLabel.BackColor = background
        channelLabel.ForeColor = foreground
        toolTip.SetToolTip(Me, BuildToolTipText(data))
        toolTip.SetToolTip(channelLabel, BuildToolTipText(data))
    End Sub

    Private Sub HandleSelected(ByVal sender As Object, ByVal e As EventArgs)
        RaiseEvent ChannelSelected(Me, New ChannelSelectedEventArgs(currentData))
    End Sub

    Private Sub HandleMouseEnter(ByVal sender As Object, ByVal e As EventArgs)
        toolTip.Show(BuildToolTipText(currentData), Me, Math.Max(0, Width \ 2), Height, 10000)
    End Sub

    Private Shared Sub GetStatusColors(ByVal status As String, ByRef background As Color, ByRef foreground As Color)
        Select Case If(status, String.Empty).Trim().ToUpperInvariant()
            Case "PASS", "OK"
                background = ColorTranslator.FromHtml("#DCFCE7")
                foreground = ColorTranslator.FromHtml("#15803D")
            Case "NG", "ERROR"
                background = ColorTranslator.FromHtml("#FEE2E2")
                foreground = ColorTranslator.FromHtml("#DC2626")
            Case "TESTING", "测试中"
                background = ColorTranslator.FromHtml("#DBEAFE")
                foreground = ColorTranslator.FromHtml("#2563EB")
            Case "WAITING", "等待"
                background = ColorTranslator.FromHtml("#FEF3C7")
                foreground = ColorTranslator.FromHtml("#D97706")
            Case "DISABLED", "禁用"
                background = ColorTranslator.FromHtml("#E5E7EB")
                foreground = ColorTranslator.FromHtml("#9CA3AF")
            Case Else
                background = ColorTranslator.FromHtml("#F3F4F6")
                foreground = ColorTranslator.FromHtml("#6B7280")
        End Select
    End Sub

    Private Shared Function BuildToolTipText(ByVal data As ChannelDisplayData) As String
        Dim ocvText As String = If(data.Ocv1.HasValue, data.Ocv1.Value.ToString("0.00") & " mV", "-")
        Dim irText As String = If(data.Ir1.HasValue, data.Ir1.Value.ToString("0.000") & " mΩ", "-")
        Return String.Format("通道：{0}-{1}{Environment.NewLine}条码：{2}{Environment.NewLine}OCV1：{3}{Environment.NewLine}IR1：{4}{Environment.NewLine}状态：{5}{Environment.NewLine}结果：{6}", data.Row, data.Column, If(String.IsNullOrWhiteSpace(data.Barcode), "-", data.Barcode), ocvText, irText, If(String.IsNullOrWhiteSpace(data.StatusText), "空闲", data.StatusText), If(String.IsNullOrWhiteSpace(data.ResultText), "-", data.ResultText))
    End Function
End Class
