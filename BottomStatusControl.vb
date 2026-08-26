Imports System.Drawing
Imports System.Windows.Forms

Public Class BottomStatusControl
    Inherits UserControl

    Private ReadOnly stationLabel As Label
    Private ReadOnly plcLabel As Label
    Private ReadOnly controlBoardLabel As Label
    Private ReadOnly voltageLabel As Label
    Private ReadOnly resistanceLabel As Label
    Private ReadOnly databaseLabel As Label
    Private ReadOnly mesLabel As Label
    Private ReadOnly userLabel As Label
    Private ReadOnly timeLabel As Label

    Public Sub New()
        Dock = DockStyle.Bottom
        Height = 40
        BackColor = UiTheme.HeaderBackground
        Padding = New Padding(24, 0, 24, 0)

        Dim layout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 9,
            .RowCount = 1,
            .BackColor = UiTheme.HeaderBackground,
            .Margin = New Padding(0),
            .Padding = New Padding(0)
        }
        Dim widths() As Single = {10.0F, 12.0F, 12.0F, 11.0F, 11.0F, 11.0F, 11.0F, 12.0F, 10.0F}
        For Each width As Single In widths
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, width))
        Next

        stationLabel = CreateStatusLabel()
        plcLabel = CreateStatusLabel()
        controlBoardLabel = CreateStatusLabel()
        voltageLabel = CreateStatusLabel()
        resistanceLabel = CreateStatusLabel()
        databaseLabel = CreateStatusLabel()
        mesLabel = CreateStatusLabel()
        userLabel = CreateStatusLabel()
        timeLabel = CreateStatusLabel()
        timeLabel.TextAlign = ContentAlignment.MiddleRight

        layout.Controls.Add(stationLabel, 0, 0)
        layout.Controls.Add(controlBoardLabel, 1, 0)
        layout.Controls.Add(plcLabel, 2, 0)
        layout.Controls.Add(voltageLabel, 3, 0)
        layout.Controls.Add(resistanceLabel, 4, 0)
        layout.Controls.Add(databaseLabel, 5, 0)
        layout.Controls.Add(mesLabel, 6, 0)
        layout.Controls.Add(userLabel, 7, 0)
        layout.Controls.Add(timeLabel, 8, 0)
        Controls.Add(layout)
    End Sub

    Public Sub UpdateStatus(ByVal station As String, ByVal controlBoard As String, ByVal plc As String, ByVal voltageMeter As String, ByVal resistanceMeter As String, ByVal database As String, ByVal mes As String, ByVal userName As String, ByVal timestamp As DateTime)
        stationLabel.Text = FormatStatus("工位", station)
        controlBoardLabel.Text = FormatStatus("控制模块", controlBoard)
        plcLabel.Text = FormatStatus("PLC", plc)
        voltageLabel.Text = FormatStatus("电压表", voltageMeter)
        resistanceLabel.Text = FormatStatus("内阻仪", resistanceMeter)
        databaseLabel.Text = FormatStatus("数据库", database)
        mesLabel.Text = FormatStatus("MES", mes)
        userLabel.Text = FormatStatus("当前用户", userName)
        timeLabel.Text = timestamp.ToString("yyyy-MM-dd HH:mm:ss")

        ApplyConnectionColor(stationLabel, station)
        ApplyConnectionColor(controlBoardLabel, controlBoard)
        ApplyConnectionColor(plcLabel, plc)
        ApplyConnectionColor(voltageLabel, voltageMeter)
        ApplyConnectionColor(resistanceLabel, resistanceMeter)
        ApplyConnectionColor(databaseLabel, database)
        ApplyConnectionColor(mesLabel, mes)
    End Sub

    Private Shared Function CreateStatusLabel() As Label
        Return New Label With {
            .Dock = DockStyle.Fill,
            .ForeColor = Color.White,
            .Font = UiTheme.UiFont,
            .TextAlign = ContentAlignment.MiddleLeft,
            .AutoEllipsis = True
        }
    End Function

    Private Shared Function FormatStatus(ByVal title As String, ByVal value As String) As String
        Return "● " & title & "：" & If(String.IsNullOrWhiteSpace(value), "-", value)
    End Function

    Private Shared Sub ApplyConnectionColor(ByVal label As Label, ByVal value As String)
        If label Is Nothing Then Return
        Dim normalized As String = If(value, String.Empty)
        If normalized.Contains("未连接") OrElse normalized.Contains("断开") OrElse normalized.Contains("异常") Then
            label.ForeColor = Color.FromArgb(255, 112, 112)
        ElseIf normalized.Contains("检测") OrElse normalized.Contains("连接中") Then
            label.ForeColor = Color.FromArgb(255, 204, 92)
        Else
            label.ForeColor = Color.FromArgb(54, 205, 125)
        End If
    End Sub
End Class
