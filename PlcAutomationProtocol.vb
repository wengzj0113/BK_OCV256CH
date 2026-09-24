Imports System.Globalization

Public NotInheritable Class PlcAutomationProtocol
    Private Sub New()
    End Sub

    Public Const SignalValueIndex As Integer = 6
    Public Const ReadR150Frame As String = "%01#CSR0150**"
    Public Const ReadR151Frame As String = "%01#RCSR0151**"
    Public Const WriteR160Frame As String = "%01#WCSR01601**"
    Public Const WriteR161Frame As String = "%01#WCSR01611**"

    Public Shared Function WithCr(ByVal frame As String) As String
        If frame Is Nothing Then Throw New ArgumentNullException(NameOf(frame))
        Return frame.TrimEnd(ControlChars.Cr, ControlChars.Lf) & ControlChars.Cr
    End Function

    Public Shared Function BuildDtGradeFrame(ByVal channelNumber As Integer, ByVal grade As Integer) As String
        If channelNumber < 1 OrElse channelNumber > 256 Then
            Throw New ArgumentOutOfRangeException(NameOf(channelNumber), "通道号必须在1到256之间。")
        End If
        If grade < 0 OrElse grade > 99 Then
            Throw New ArgumentOutOfRangeException(NameOf(grade), "档位必须在0到99之间。")
        End If

        Dim dtAddress As String = (1000 + channelNumber).ToString(CultureInfo.InvariantCulture)
        Dim gradeText As String = grade.ToString("00", CultureInfo.InvariantCulture)
        Return WithCr("%01#WD" & dtAddress & "0" & dtAddress & gradeText & "00**")
    End Function

    Public Shared Function ParseSignal(ByVal response As String) As Boolean
        If String.IsNullOrWhiteSpace(response) Then
            Throw New PlcProtocolException("PLC返回为空。")
        End If

        Dim normalized As String = response.Trim(ControlChars.Cr, ControlChars.Lf, " "c, Microsoft.VisualBasic.ChrW(0))
        If normalized.Length <= SignalValueIndex Then
            Throw New PlcProtocolException("PLC返回长度不足：" & response)
        End If

        Dim value As Char = normalized(SignalValueIndex)
        If value <> "0"c AndAlso value <> "1"c Then
            Throw New PlcProtocolException("PLC信号值无效：" & response)
        End If
        Return value = "1"c
    End Function

    Public Shared Function IsWriteAcknowledged(ByVal response As String) As Boolean
        If String.IsNullOrWhiteSpace(response) Then Return False
        Dim normalized As String = response.Trim(ControlChars.Cr, ControlChars.Lf, " "c, Microsoft.VisualBasic.ChrW(0))
        Return normalized.Length > 0
    End Function
End Class

Public Class PlcProtocolException
    Inherits InvalidOperationException

    Public Sub New(ByVal message As String)
        MyBase.New(message)
    End Sub
End Class
