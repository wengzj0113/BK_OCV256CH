Imports System.Diagnostics

''' <summary>
''' Records startup milestones without depending on the UI thread.
''' </summary>
Public NotInheritable Class StartupTiming
    Private Shared ReadOnly stopwatch As Stopwatch = Stopwatch.StartNew()
    Private Shared ReadOnly writeLock As New Object()

    Private Sub New()
    End Sub

    Public Shared Sub Mark(ByVal phase As String, Optional ByVal detail As String = Nothing)
        If String.IsNullOrWhiteSpace(phase) Then Return

        Dim message As String = String.Format("[STARTUP] {0} +{1} ms{2}",
                                              phase.Trim(),
                                              stopwatch.ElapsedMilliseconds,
                                              If(String.IsNullOrWhiteSpace(detail), String.Empty, " - " & detail.Trim()))
        SyncLock writeLock
            Debug.WriteLine(message)
            AppLog.Write("Startup", message)
        End SyncLock
    End Sub
End Class
