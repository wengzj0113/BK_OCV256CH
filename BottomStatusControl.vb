Imports System.Drawing
Imports System.Windows.Forms

Public Class BottomStatusControl
    Inherits UserControl

    Private ReadOnly stationLabel As Label
    Private ReadOnly plcLabel As Label
    Private ReadOnly voltageLabel As Label
    Private ReadOnly resistanceLabel As Label
    Private ReadOnly databaseLabel As Label
    Private ReadOnly mesLabel As Label
    Private ReadOnly userLabel As Label
    Private ReadOnly timeLabel As Label

    Public Sub New()
        Dock = DockStyle.Bottom
        Height = 36
        BackColor = UiTheme.HeaderBackground
        Padding = New Padding(12, 0, 12, 0)

        Dim layout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 8,
            .RowCount = 1,
            .BackColor = UiTheme.HeaderBackground,
            .Margin = New Padding(0)
        }
        For i As Integer = 0 To 5
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 12.5F))
        Next
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 15.0F))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 10.0F))

        stationLabel = CreateStatusLabel()
        plcLabel = CreateStatusLabel()
        voltageLabel = CreateStatusLabel()
        resistanceLabel = CreateStatusLabel()
        databaseLabel = CreateStatusLabel()
        mesLabel = CreateStatusLabel()
        userLabel = CreateStatusLabel()
        timeLabel = CreateStatusLabel()
        timeLabel.TextAlign = ContentAlignment.MiddleRight

        layout.Controls.Add(stationLabel, 0, 0)
        layout.Controls.Add(plcLabel, 1, 0)
        layout.Controls.Add(voltageLabel, 2, 0)
        layout.Controls.Add(resistanceLabel, 3, 0)
        layout.Controls.Add(databaseLabel, 4, 0)
        layout.Controls.Add(mesLabel, 5, 0)
        layout.Controls.Add(userLabel, 6, 0)
        layout.Controls.Add(timeLabel, 7, 0)
        Controls.Add(layout)
    End Sub

    Public Sub UpdateStatus(ByVal station As String, ByVal plc As String, ByVal voltageMeter As String, ByVal resistanceMeter As String, ByVal database As String, ByVal mes As String, ByVal userName As String, ByVal timestamp As DateTime)
        stationLabel.Text = FormatStatus("工位", station)
        plcLabel.Text = FormatStatus("PLC", plc)
        voltageLabel.Text = FormatStatus("电压表", voltageMeter)
        resistanceLabel.Text = FormatStatus("内阻仪", resistanceMeter)
        databaseLabel.Text = FormatStatus("数据库", database)
        mesLabel.Text = FormatStatus("MES", mes)
        userLabel.Text = FormatStatus("用户", userName)
        timeLabel.Text = timestamp.ToString("yyyy-MM-dd HH:mm:ss")
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
        Return String.Format("{0}：{1}", title, If(String.IsNullOrWhiteSpace(value), "-", value))
    End Function
End Class
