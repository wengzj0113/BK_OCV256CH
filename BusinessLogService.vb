Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text

Public Class BusinessLogEntry
    Public Property OccurredAt As DateTime
    Public Property Category As String
    Public Property Level As String
    Public Property UserName As String
    Public Property Action As String
    Public Property Target As String
    Public Property Summary As String
    Public Property Details As String
    Public Property Source As String
    Public Property IsLegacy As Boolean
End Class

''' <summary>追加式、按日分片的本地业务审计日志。</summary>
Public NotInheritable Class BusinessLogService
    Private Shared ReadOnly writeLock As New Object()
    Private Shared ReadOnly issueLock As New Object()
    Private Shared ReadOnly fileEncoding As New UTF8Encoding(False)
    Private Shared lastIssueText As String = String.Empty

    Private Sub New()
    End Sub

    Public Shared ReadOnly Property LogDirectory As String
        Get
            Return AppLog.LogDirectory
        End Get
    End Property

    Public Shared ReadOnly Property LastIssueMessage As String
        Get
            SyncLock issueLock
                Return lastIssueText
            End SyncLock
        End Get
    End Property

    Public Shared Sub Write(category As String,
                            level As String,
                            action As String,
                            target As String,
                            summary As String,
                            Optional details As String = "",
                            Optional source As String = "",
                            Optional userName As String = "")
        WriteAt(DateTime.Now, category, level, action, target, summary, details, source, userName)
    End Sub

    Public Shared Sub WriteAlarm(alarmText As String)
        If String.IsNullOrWhiteSpace(alarmText) Then Return
        Dim entry As BusinessLogEntry = ParseAlarmEntry(alarmText, DateTime.Now)
        WriteAt(entry.OccurredAt, entry.Category, entry.Level, entry.Action, entry.Target, entry.Summary,
                "原始报警文本：" & alarmText.Trim(), entry.Source)
    End Sub

    Private Shared Sub WriteAt(occurredAt As DateTime,
                               category As String,
                               level As String,
                               action As String,
                               target As String,
                               summary As String,
                               details As String,
                               source As String,
                               Optional userName As String = "")
        Try
            Dim entry As New BusinessLogEntry With {
                .OccurredAt = occurredAt,
                .Category = Normalize(category, "操作"),
                .Level = Normalize(level, "成功"),
                .UserName = If(Not String.IsNullOrWhiteSpace(userName), userName.Trim(), If(String.IsNullOrWhiteSpace(AuthorizationService.CurrentUserName), "系统", AuthorizationService.CurrentUserName)),
                .Action = If(action, String.Empty),
                .Target = If(target, String.Empty),
                .Summary = If(summary, String.Empty),
                .Details = If(details, String.Empty),
                .Source = If(source, String.Empty)
            }
            Dim fileName As String = "Business_" & occurredAt.ToString("yyyyMMdd", CultureInfo.InvariantCulture) & ".log"
            Dim filePath As String = Path.Combine(LogDirectory, fileName)
            Dim line As String = Serialize(entry) & Environment.NewLine

            SyncLock writeLock
                If Not Directory.Exists(LogDirectory) Then Directory.CreateDirectory(LogDirectory)
                File.AppendAllText(filePath, line, fileEncoding)
            End SyncLock
        Catch ex As Exception
            RecordIssue("业务日志写入失败：" & ex.Message)
        End Try
    End Sub

    Friend Shared Function ParseAlarmEntry(alarmText As String, fallbackTime As DateTime) As BusinessLogEntry
        Dim summary As String = If(alarmText, String.Empty).Trim()
        Dim occurredAt As DateTime = fallbackTime
        Dim parsedTime As DateTime
        Dim foundTimestamp As Boolean = False

        While ExtractLeadingTimestamp(summary, parsedTime)
            If Not foundTimestamp Then
                occurredAt = parsedTime
                foundTimestamp = True
            End If
        End While

        Dim reset As Boolean = summary.Contains("复位")
        Return New BusinessLogEntry With {
            .OccurredAt = occurredAt,
            .Category = "报警",
            .Level = If(reset, "已复位", "报警"),
            .UserName = "系统",
            .Action = If(reset, "报警复位", "报警发生"),
            .Target = "设备报警",
            .Summary = summary,
            .Source = "报警监控"
        }
    End Function

    Private Shared Sub RecordIssue(message As String)
        SyncLock issueLock
            lastIssueText = message
        End SyncLock
        Try
            System.Diagnostics.Trace.TraceError(message)
        Catch
            '日志诊断本身不能影响业务操作。
        End Try
    End Sub

    Public Shared Function Query(fromTime As DateTime,
                                 toTime As DateTime,
                                 Optional category As String = "全部",
                                 Optional level As String = "全部状态",
                                 Optional userName As String = "",
                                 Optional keyword As String = "") As List(Of BusinessLogEntry)
        Dim entries As New List(Of BusinessLogEntry)()
        LoadBusinessFiles(entries, fromTime, toTime)
        LoadLegacyAlarmFiles(entries, fromTime, toTime)

        Dim dateEntries As IEnumerable(Of BusinessLogEntry) = entries.Where(Function(entry) entry.OccurredAt >= fromTime AndAlso entry.OccurredAt <= toTime)
        Return FilterEntries(dateEntries, category, level, userName, keyword)
    End Function

    Friend Shared Function FilterEntries(entries As IEnumerable(Of BusinessLogEntry),
                                         Optional category As String = "全部",
                                         Optional level As String = "全部状态",
                                         Optional userName As String = "",
                                         Optional keyword As String = "") As List(Of BusinessLogEntry)
        If entries Is Nothing Then Return New List(Of BusinessLogEntry)()
        Dim queryText As String = If(keyword, String.Empty).Trim()
        Dim selectedCategory As String = If(category, "全部").Trim()
        Dim selectedLevel As String = If(level, "全部状态").Trim()
        Dim selectedUser As String = If(userName, String.Empty).Trim()

        Return entries.Where(Function(entry)
                                 If selectedCategory <> "全部" AndAlso selectedCategory <> "全部类型" AndAlso Not String.Equals(entry.Category, selectedCategory, StringComparison.OrdinalIgnoreCase) Then Return False
                                 If selectedLevel = "异常" Then
                                     If entry.Level <> "失败" AndAlso entry.Level <> "报警" Then Return False
                                 ElseIf selectedLevel <> "全部状态" AndAlso Not String.Equals(entry.Level, selectedLevel, StringComparison.OrdinalIgnoreCase) Then
                                     Return False
                                 End If
                                 If selectedUser <> "" AndAlso entry.UserName.IndexOf(selectedUser, StringComparison.OrdinalIgnoreCase) < 0 Then Return False
                                 If queryText <> "" Then
                                     Dim searchable As String = String.Join(" ", {entry.Category, entry.Level, entry.UserName, entry.Action, entry.Target, entry.Summary, entry.Details, entry.Source})
                                     If searchable.IndexOf(queryText, StringComparison.OrdinalIgnoreCase) < 0 Then Return False
                                 End If
                                 Return True
                             End Function).OrderByDescending(Function(entry) entry.OccurredAt).ThenBy(Function(entry) entry.Category).ToList()
    End Function

    Public Shared Function ToCsv(entries As IEnumerable(Of BusinessLogEntry)) As String
        Dim builder As New StringBuilder()
        builder.AppendLine("发生时间,类型,状态,操作人,动作,对象,事件摘要,详情,来源")
        For Each entry As BusinessLogEntry In entries
            builder.AppendLine(String.Join(",", {
                CsvCell(entry.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)),
                CsvCell(entry.Category), CsvCell(entry.Level), CsvCell(entry.UserName), CsvCell(entry.Action), CsvCell(entry.Target), CsvCell(entry.Summary), CsvCell(entry.Details), CsvCell(entry.Source)
            }))
        Next
        Return builder.ToString()
    End Function

    Private Shared Sub LoadBusinessFiles(entries As List(Of BusinessLogEntry), fromTime As DateTime, toTime As DateTime)
        If Not Directory.Exists(LogDirectory) Then Return

        Dim businessFiles As String()
        Try
            businessFiles = Directory.GetFiles(LogDirectory, "Business_*.log", SearchOption.TopDirectoryOnly)
        Catch ex As Exception
            RecordIssue("无法枚举业务日志文件：" & ex.Message)
            Return
        End Try

        For Each filePath As String In businessFiles
            Try
                Dim name As String = Path.GetFileNameWithoutExtension(filePath)
                Dim fileDate As DateTime
                If name.Length >= 17 AndAlso DateTime.TryParseExact(name.Substring(9, 8), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, fileDate) Then
                    If fileDate.Date < fromTime.Date OrElse fileDate.Date > toTime.Date Then Continue For
                End If

                For Each line As String In File.ReadLines(filePath, Encoding.UTF8)
                    Dim entry As BusinessLogEntry = Deserialize(line)
                    If entry IsNot Nothing AndAlso entry.OccurredAt >= fromTime AndAlso entry.OccurredAt <= toTime Then entries.Add(entry)
                Next
            Catch ex As Exception
                RecordIssue("无法读取业务日志文件 " & Path.GetFileName(filePath) & "：" & ex.Message)
            End Try
        Next
    End Sub

    Private Shared Sub LoadLegacyAlarmFiles(entries As List(Of BusinessLogEntry), fromTime As DateTime, toTime As DateTime)
        Dim paths As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AlarmLog.txt"),
            Path.Combine(Environment.CurrentDirectory, "AlarmLog.txt")
        }

        For Each path As String In paths
            If Not File.Exists(path) Then Continue For
            Try
                For Each line As String In File.ReadLines(path, Encoding.UTF8)
                    If String.IsNullOrWhiteSpace(line) Then Continue For
                    Dim legacyEntry As BusinessLogEntry = ParseAlarmEntry(line, File.GetLastWriteTime(path))
                    If legacyEntry.OccurredAt < fromTime OrElse legacyEntry.OccurredAt > toTime Then Continue For
                    legacyEntry.UserName = "历史记录"
                    legacyEntry.Details = "从旧版 AlarmLog.txt 导入的历史报警记录。原始内容：" & line
                    legacyEntry.Source = "历史报警文件"
                    legacyEntry.IsLegacy = True
                    If IsDuplicateAlarm(entries, legacyEntry) Then Continue For
                    entries.Add(legacyEntry)
                Next
            Catch ex As Exception
                RecordIssue("无法读取旧报警文件 " & System.IO.Path.GetFileName(path) & "：" & ex.Message)
            End Try
        Next
    End Sub

    Friend Shared Function IsDuplicateAlarm(entries As IEnumerable(Of BusinessLogEntry), candidate As BusinessLogEntry) As Boolean
        If entries Is Nothing OrElse candidate Is Nothing Then Return False
        Return entries.Any(Function(existing)
                               If existing.Category <> "报警" OrElse candidate.Category <> "报警" Then Return False
                               If Math.Abs((existing.OccurredAt - candidate.OccurredAt).TotalSeconds) > 2 Then Return False
                               Return NormalizeForMatch(existing.Summary) = NormalizeForMatch(candidate.Summary)
                           End Function)
    End Function

    Private Shared Function ExtractLeadingTimestamp(ByRef text As String, ByRef occurredAt As DateTime) As Boolean
        Dim searchFrom As Integer = 0
        Dim bestSeparator As Integer = -1
        Dim bestTimestamp As DateTime = DateTime.MinValue
        While searchFrom < text.Length
            Dim separator As Integer = text.IndexOf(":"c, searchFrom)
            If separator < 0 Then Exit While
            Dim candidate As DateTime
            If DateTime.TryParse(text.Substring(0, separator).Trim(), CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, candidate) Then
                bestSeparator = separator
                bestTimestamp = candidate
            End If
            searchFrom = separator + 1
        End While

        If bestSeparator < 0 Then Return False
        occurredAt = bestTimestamp
        text = text.Substring(bestSeparator + 1).Trim()
        Return True
    End Function

    Private Shared Function Serialize(entry As BusinessLogEntry) As String
        Return String.Join(vbTab, {
            entry.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
            Escape(entry.Category), Escape(entry.Level), Escape(entry.UserName), Escape(entry.Action), Escape(entry.Target), Escape(entry.Summary), Escape(entry.Details), Escape(entry.Source)
        })
    End Function

    Private Shared Function Deserialize(line As String) As BusinessLogEntry
        If String.IsNullOrWhiteSpace(line) Then Return Nothing
        Dim fields As String() = line.Split(ControlChars.Tab)
        If fields.Length <> 9 Then Return Nothing
        Dim occurredAt As DateTime
        If Not DateTime.TryParseExact(fields(0), "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, occurredAt) Then Return Nothing
        Return New BusinessLogEntry With {
            .OccurredAt = occurredAt,
            .Category = Unescape(fields(1)), .Level = Unescape(fields(2)), .UserName = Unescape(fields(3)), .Action = Unescape(fields(4)),
            .Target = Unescape(fields(5)), .Summary = Unescape(fields(6)), .Details = Unescape(fields(7)), .Source = Unescape(fields(8))
        }
    End Function

    Private Shared Function Escape(value As String) As String
        Return If(value, String.Empty).Replace("\", "\\").Replace(vbTab, "\t").Replace(vbCr, "\r").Replace(vbLf, "\n")
    End Function

    Private Shared Function Unescape(value As String) As String
        Dim result As New StringBuilder()
        Dim index As Integer = 0
        While index < value.Length
            If value(index) = "\"c AndAlso index + 1 < value.Length Then
                index += 1
                Select Case value(index)
                    Case "t"c : result.Append(ControlChars.Tab)
                    Case "r"c : result.Append(ControlChars.Cr)
                    Case "n"c : result.Append(ControlChars.Lf)
                    Case Else : result.Append(value(index))
                End Select
            Else
                result.Append(value(index))
            End If
            index += 1
        End While
        Return result.ToString()
    End Function

    Friend Shared Function CsvCell(value As String) As String
        Dim quote As String = ChrW(34).ToString()
        Dim cellValue As String = If(value, String.Empty)
        Dim firstMeaningful As String = cellValue.TrimStart(" "c, ControlChars.Tab, ControlChars.Cr, ControlChars.Lf)
        If firstMeaningful <> String.Empty AndAlso "=+-@".IndexOf(firstMeaningful(0)) >= 0 Then
            cellValue = "'" & cellValue
        End If
        Return quote & cellValue.Replace(quote, quote & quote) & quote
    End Function

    Private Shared Function Normalize(value As String, fallback As String) As String
        Return If(String.IsNullOrWhiteSpace(value), fallback, value.Trim())
    End Function

    Private Shared Function NormalizeForMatch(value As String) As String
        Return If(value, String.Empty).Replace(" ", String.Empty).Replace("　", String.Empty).Trim().ToLowerInvariant()
    End Function
End Class
