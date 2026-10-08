Imports System.Collections.Generic
Imports System.Globalization

Public NotInheritable Class DebugBarcodePolicy
    Private Const MaximumChannel As Integer = 256

    Private Sub New()
    End Sub

    Public Shared Function CreateCodes(ByVal channels As IEnumerable(Of Integer),
                                       ByVal runDate As DateTime) As IDictionary(Of Integer, String)
        If channels Is Nothing Then Throw New ArgumentNullException(NameOf(channels))

        Dim result As New Dictionary(Of Integer, String)()
        Dim dateText As String = runDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
        For Each channel As Integer In channels
            If channel < 1 OrElse channel > MaximumChannel Then
                Throw New ArgumentOutOfRangeException(NameOf(channels), "通道号必须在1到256之间。")
            End If
            If Not result.ContainsKey(channel) Then
                result.Add(channel, String.Format(CultureInfo.InvariantCulture, "TEST-{0}-{1}", dateText, channel))
            End If
        Next

        Return result
    End Function
End Class
