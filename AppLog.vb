Imports System.IO
Imports System.Linq
Imports System.Text

''' <summary>
''' 应用统一日志服务。新日志写入用户可写目录，同时保留读取旧程序目录日志的兼容能力。
''' </summary>
Public NotInheritable Class AppLog
    Private Sub New()
    End Sub

    Public Shared ReadOnly Property LogDirectory As String
        Get
            Return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "XK-OCV-256CH", "logFile")
        End Get
    End Property

    Private Shared ReadOnly Property LegacyLogDirectory As String
        Get
            Return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logFile")
        End Get
    End Property

    Public Shared Sub Write(category As String, message As String)
        Try
            If Not Directory.Exists(LogDirectory) Then Directory.CreateDirectory(LogDirectory)
            Dim safeCategory As String = If(String.IsNullOrWhiteSpace(category), "Application", category.Trim())
            Dim path As String = System.IO.Path.Combine(LogDirectory, safeCategory & "_" & DateTime.Now.ToString("yyyyMMdd") & ".log")
            Dim line As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & " " & If(message, String.Empty) & Environment.NewLine
            File.AppendAllText(path, line, Encoding.UTF8)
        Catch
        End Try
    End Sub

    Public Shared Function GetLogFiles() As List(Of String)
        Dim files As New List(Of String)()
        AddLogFiles(files, LogDirectory)
        If Not String.Equals(LogDirectory, LegacyLogDirectory, StringComparison.OrdinalIgnoreCase) Then
            AddLogFiles(files, LegacyLogDirectory)
        End If
        Return files.Distinct(StringComparer.OrdinalIgnoreCase).OrderByDescending(Function(path) File.GetLastWriteTime(path)).ToList()
    End Function

    Public Shared Function ReadLogFile(path As String) As String
        If String.IsNullOrWhiteSpace(path) OrElse Not File.Exists(path) Then Return String.Empty
        Try
            Return File.ReadAllText(path, Encoding.UTF8)
        Catch ex As Exception
            Return "读取日志失败：" & ex.Message
        End Try
    End Function

    Private Shared Sub AddLogFiles(files As List(Of String), directoryPath As String)
        If Not Directory.Exists(directoryPath) Then Return
        Try
            files.AddRange(Directory.GetFiles(directoryPath, "*.log", SearchOption.TopDirectoryOnly))
            files.AddRange(Directory.GetFiles(directoryPath, "*.txt", SearchOption.TopDirectoryOnly))
        Catch
        End Try
    End Sub
End Class
