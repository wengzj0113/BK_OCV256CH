Imports System.Collections.Concurrent
Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports System.Threading

Public NotInheritable Class AutomaticEventLogWriter
    Implements IDisposable

    Private NotInheritable Class LogEntry
        Public Sub New(ByVal filePath As String, ByVal line As String)
            Me.FilePath = filePath
            Me.Line = line
        End Sub

        Public ReadOnly Property FilePath As String
        Public ReadOnly Property Line As String
    End Class

    Private ReadOnly entries As BlockingCollection(Of LogEntry)
    Private ReadOnly appendEntry As Action(Of String, String)
    Private ReadOnly worker As Thread
    Private disposed As Integer

    Public Sub New(Optional ByVal appendAction As Action(Of String, String) = Nothing)
        entries = New BlockingCollection(Of LogEntry)(New ConcurrentQueue(Of LogEntry)(), 256)
        appendEntry = If(appendAction, AddressOf AppendToFile)
        worker = New Thread(AddressOf DrainQueue) With {
            .IsBackground = True,
            .Name = "Automatic event log writer"
        }
        worker.Start()
    End Sub

    Public Function TryEnqueue(ByVal filePath As String, ByVal line As String) As Boolean
        If Volatile.Read(disposed) <> 0 OrElse String.IsNullOrWhiteSpace(filePath) OrElse String.IsNullOrWhiteSpace(line) Then Return False
        Try
            Return entries.TryAdd(New LogEntry(filePath, line))
        Catch ex As InvalidOperationException
            Return False
        End Try
    End Function

    Public Sub Complete()
        If Interlocked.Exchange(disposed, 1) <> 0 Then Return
        entries.CompleteAdding()
    End Sub

    Public Function WaitForDrain(ByVal timeoutMilliseconds As Integer) As Boolean
        Complete()
        Return worker.Join(Math.Max(0, timeoutMilliseconds))
    End Function

    Private Sub DrainQueue()
        For Each entry As LogEntry In entries.GetConsumingEnumerable()
            Try
                appendEntry(entry.FilePath, entry.Line)
            Catch ex As Exception
                Debug.WriteLine("Automatic event log write failed: " & ex.Message)
            End Try
        Next
    End Sub

    Private Shared Sub AppendToFile(ByVal filePath As String, ByVal line As String)
        Dim directoryPath As String = Path.GetDirectoryName(filePath)
        If Not String.IsNullOrWhiteSpace(directoryPath) Then Directory.CreateDirectory(directoryPath)
        File.AppendAllText(filePath, line & Environment.NewLine, New UTF8Encoding(False))
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Complete()
    End Sub
End Class
