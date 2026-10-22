Imports System.Drawing
Imports System.Windows.Forms

Public Class BottomStatusControl
    Inherits UserControl

    Public Sub New()
        InitializeComponent()
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

    Private Shared Function FormatStatus(ByVal title As String, ByVal value As String) As String
        Return "● " & title & "：" & If(String.IsNullOrWhiteSpace(value), "-", value)
    End Function

    Private Shared Sub ApplyConnectionColor(ByVal label As Label, ByVal value As String)
        If label Is Nothing Then Return
        Dim normalized As String = If(value, String.Empty)
        If normalized.Contains("未连接") OrElse normalized.Contains("断开") OrElse normalized.Contains("异常") Then
            label.ForeColor = Color.FromArgb(255, 112, 112)
        ElseIf normalized.Contains("检测") OrElse normalized.Contains("连接中") OrElse normalized.Contains("待响应") Then
            label.ForeColor = Color.FromArgb(255, 204, 92)
        Else
            label.ForeColor = Color.FromArgb(54, 205, 125)
        End If
    End Sub
End Class
