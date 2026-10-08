Imports System.Globalization

Public NotInheritable Class InstrumentCacheParseResult
        Public Sub New()
            Voltages = New List(Of Single)()
            Resistances = New List(Of Single)()
        End Sub

        Public ReadOnly Property Voltages As List(Of Single)
        Public ReadOnly Property Resistances As List(Of Single)
        Public Property InvalidRecordCount As Integer
        Public Property Success As Boolean

        Public ReadOnly Property ValidRecordCount As Integer
            Get
                Return Resistances.Count
            End Get
        End Property
End Class

Public NotInheritable Class InstrumentCacheParser
        Private Sub New()
        End Sub

        Public Shared Function Parse3562MemoryData(text As String,
                                                    expectedCount As Integer,
                                                    Optional resistanceOnly As Boolean = False) As InstrumentCacheParseResult
            Dim result As New InstrumentCacheParseResult()
            If expectedCount <= 0 OrElse String.IsNullOrWhiteSpace(text) Then
                Return result
            End If

            '3562 可能使用 CR、LF 或 CRLF 作为每条记录的结束符，统一为 LF 后逐条解析。
            Dim lines As String() = text.Replace(vbCr, vbLf).Split({vbLf}, StringSplitOptions.RemoveEmptyEntries)
            For Each line As String In lines
                Dim record As String = line.Trim()
                If record.Equals("END", StringComparison.OrdinalIgnoreCase) Then Exit For

                Dim fields As String() = record.Split(","c)
                If fields.Length < If(resistanceOnly, 2, 3) Then
                    result.InvalidRecordCount += 1
                    Continue For
                End If

                Dim recordNumber As Integer
                Dim resistanceOhm As Double
                If Not Integer.TryParse(fields(0).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, recordNumber) OrElse
                   Not TryParse3562Number(fields(1), resistanceOhm) Then
                    result.InvalidRecordCount += 1
                    Continue For
                End If

                Dim voltageValue As Double = 0.0R
                If Not resistanceOnly AndAlso Not TryParse3562Number(fields(2), voltageValue) Then
                    result.InvalidRecordCount += 1
                    Continue For
                End If

                result.Resistances.Add(CSng(resistanceOhm * 1000.0R))
                result.Voltages.Add(CSng(voltageValue))
            Next

            '严格校验：有效记录必须与本次测试计划完全一致，且不能掩盖格式异常记录。
            result.Success = result.ValidRecordCount = expectedCount AndAlso result.InvalidRecordCount = 0
            Return result
        End Function

        Private Shared Function TryParse3562Number(text As String, ByRef value As Double) As Boolean
            If text Is Nothing Then Return False

            Dim normalized As String = text.Trim()
            If normalized.Length = 0 Then Return False

            '仪表可能在符号与数字之间填充空格，例如 "-   0.04E+0"。
            Dim firstCharacter As Char = normalized(0)
            If firstCharacter = "-"c OrElse firstCharacter = "+"c Then
                normalized = firstCharacter & normalized.Substring(1).TrimStart()
            End If

            Return Double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, value)
        End Function
End Class
