Imports System.Globalization

Public NotInheritable Class InstrumentCacheParseResult
        Public Sub New()
            Voltages = New List(Of Single)()
            Resistances = New List(Of Single)()
        End Sub

        Public ReadOnly Property Voltages As List(Of Single)
        Public ReadOnly Property Resistances As List(Of Single)
        Public Property InvalidRecordCount As Integer
        Public Property FallbackRecordCount As Integer
        Public Property ActualRecordCount As Integer
        Public Property BatchFallbackReason As String = String.Empty
        Public Property Success As Boolean

        Public ReadOnly Property ValidRecordCount As Integer
            Get
                Return Resistances.Count - FallbackRecordCount
            End Get
        End Property
End Class

Public NotInheritable Class InstrumentCacheParser
        Public Const InvalidResistanceMilliohm As Single = 10000.0F

        Private Sub New()
        End Sub

        Public Shared Function NormalizeResistanceMilliohm(value As Double) As Single
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) OrElse
               Math.Abs(value) >= InvalidResistanceMilliohm Then Return InvalidResistanceMilliohm
            Return CSng(value)
        End Function

        Public Shared Function Parse3562MemoryData(text As String,
                                                    expectedCount As Integer,
                                                    Optional resistanceOnly As Boolean = False) As InstrumentCacheParseResult
            Dim result As New InstrumentCacheParseResult()
            If expectedCount <= 0 Then Return result

            '3562 可能使用 CR、LF 或 CRLF 作为每条记录的结束符，统一为 LF 后逐条解析。
            Dim lines As String() = If(text, String.Empty).Replace(vbCr, vbLf).Split({vbLf}, StringSplitOptions.RemoveEmptyEntries)
            Dim previousRecordNumber As Integer? = Nothing
            Dim positionsReliable As Boolean = True
            For Each line As String In lines
                Dim record As String = line.Trim()
                If record.Length = 0 Then Continue For
                If record.Equals("END", StringComparison.OrdinalIgnoreCase) Then Exit For
                result.ActualRecordCount += 1
                Dim fields As String() = record.Split(","c)
                Dim invalidRecord As Boolean = fields.Length < If(resistanceOnly, 2, 3)
                Dim recordNumber As Integer
                If Not Integer.TryParse(fields(0).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, recordNumber) Then
                    positionsReliable = False
                    invalidRecord = True
                Else
                    If previousRecordNumber.HasValue AndAlso CLng(recordNumber) <> CLng(previousRecordNumber.Value) + 1L Then
                        positionsReliable = False
                    End If
                    previousRecordNumber = recordNumber
                End If

                Dim resistanceOhm As Double
                Dim resistanceMilliohm As Single = InvalidResistanceMilliohm
                If fields.Length < 2 OrElse Not TryParse3562Number(fields(1), resistanceOhm) Then
                    invalidRecord = True
                Else
                    resistanceMilliohm = NormalizeResistanceMilliohm(resistanceOhm * 1000.0R)
                    If resistanceMilliohm = InvalidResistanceMilliohm Then invalidRecord = True
                End If

                Dim voltageValue As Double = 0.0R
                If Not resistanceOnly Then
                    If fields.Length < 3 OrElse Not TryParse3562Number(fields(2), voltageValue) OrElse
                       Double.IsNaN(voltageValue) OrElse Double.IsInfinity(voltageValue) OrElse
                       Math.Abs(voltageValue) > Single.MaxValue Then
                        invalidRecord = True
                        voltageValue = 0.0R
                    End If
                End If
                If invalidRecord Then
                    resistanceMilliohm = InvalidResistanceMilliohm
                    result.InvalidRecordCount += 1
                    result.FallbackRecordCount += 1
                End If
                result.Resistances.Add(resistanceMilliohm)
                result.Voltages.Add(CSng(voltageValue))
            Next

            '条数或序号不可靠时不能猜测缺失点属于哪个通道，整批替代以保持两遍映射位置。
            If result.ActualRecordCount <> expectedCount OrElse Not positionsReliable Then
                result.BatchFallbackReason = If(result.ActualRecordCount <> expectedCount,
                    String.Format(CultureInfo.InvariantCulture, "缓存条数不符：预期{0}，实际{1}", expectedCount, result.ActualRecordCount),
                    "缓存记录序号不连续或无效")
                result.Resistances.Clear()
                result.Voltages.Clear()
                For index As Integer = 1 To expectedCount
                    result.Resistances.Add(InvalidResistanceMilliohm)
                    result.Voltages.Add(0.0F)
                Next
                result.FallbackRecordCount = expectedCount
                result.InvalidRecordCount = Math.Max(result.InvalidRecordCount, Math.Max(1, Math.Abs(expectedCount - result.ActualRecordCount)))
            End If
            result.Success = result.Resistances.Count = expectedCount
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
