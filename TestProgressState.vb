Imports System

Public Enum TestProgressStage
    Idle = 0
    Preparing = 1
    CreatingTask = 2
    Testing = 3
    CollectingResults = 4
    Saving = 5
    Uploading = 6
    Completed = 7
    Failed = 8
    Canceled = 9
End Enum

Public NotInheritable Class TestProgressSnapshot
    Public ReadOnly Property Stage As TestProgressStage
    Public ReadOnly Property CompletedCount As Integer
    Public ReadOnly Property TotalCount As Integer
    Public ReadOnly Property CurrentChannel As Integer?
    Public ReadOnly Property ProgressPercent As Integer
    Public ReadOnly Property Elapsed As TimeSpan
    Public ReadOnly Property EstimatedRemaining As TimeSpan?
    Public ReadOnly Property Message As String

    Public Sub New(stage As TestProgressStage,
                   completedCount As Integer,
                   totalCount As Integer,
                   currentChannel As Integer?,
                   progressPercent As Integer,
                   elapsed As TimeSpan,
                   estimatedRemaining As TimeSpan?,
                   message As String)
        Me.Stage = stage
        Me.CompletedCount = completedCount
        Me.TotalCount = totalCount
        Me.CurrentChannel = currentChannel
        Me.ProgressPercent = Math.Max(0, Math.Min(100, progressPercent))
        Me.Elapsed = elapsed
        Me.EstimatedRemaining = estimatedRemaining
        Me.Message = If(message, String.Empty)
    End Sub
End Class

Public NotInheritable Class TestProgressState
    Private _stage As TestProgressStage = TestProgressStage.Idle
    Private _completedCount As Integer
    Private _totalCount As Integer
    Private _currentChannel As Integer?
    Private _progressPercent As Integer
    Private _message As String = String.Empty
    Private _startedAtUtc As DateTime?

    Public Sub Reset(Optional message As String = "等待开始")
        _stage = TestProgressStage.Idle
        _completedCount = 0
        _totalCount = 0
        _currentChannel = Nothing
        _progressPercent = 0
        _message = If(message, String.Empty)
        _startedAtUtc = Nothing
    End Sub

    Public Sub Begin(totalCount As Integer, Optional message As String = "准备测试")
        _stage = TestProgressStage.Preparing
        _completedCount = 0
        _totalCount = Math.Max(0, totalCount)
        _currentChannel = Nothing
        _progressPercent = 0
        _message = If(message, String.Empty)
        _startedAtUtc = DateTime.UtcNow
    End Sub

    Public Sub SetStage(stage As TestProgressStage,
                        Optional message As String = Nothing,
                        Optional currentChannel As Integer? = Nothing,
                        Optional progressPercent As Integer? = Nothing)
        _stage = stage
        If message IsNot Nothing Then _message = message
        If currentChannel.HasValue Then _currentChannel = currentChannel
        If progressPercent.HasValue Then _progressPercent = Math.Max(0, Math.Min(100, progressPercent.Value))
    End Sub

    Public Sub Advance(completedCount As Integer,
                       Optional currentChannel As Integer? = Nothing,
                       Optional message As String = Nothing)
        _completedCount = Math.Max(0, Math.Min(completedCount, _totalCount))
        If currentChannel.HasValue Then _currentChannel = currentChannel
        If message IsNot Nothing Then _message = message
        If _totalCount > 0 Then
            _progressPercent = CInt(Math.Truncate((_completedCount * 100.0R) / _totalCount))
        End If
    End Sub

    Public Sub Complete(Optional message As String = "测试完成")
        _completedCount = _totalCount
        _currentChannel = Nothing
        _progressPercent = 100
        _stage = TestProgressStage.Completed
        _message = If(message, String.Empty)
    End Sub

    Public Sub Fail(message As String)
        _stage = TestProgressStage.Failed
        _message = If(message, "测试失败")
    End Sub

    Public Sub Cancel(message As String)
        _stage = TestProgressStage.Canceled
        _message = If(message, "测试已停止")
    End Sub

    Public Function CreateSnapshot() As TestProgressSnapshot
        Dim elapsed As TimeSpan = GetElapsed()
        Return New TestProgressSnapshot(_stage,
                                        _completedCount,
                                        _totalCount,
                                        _currentChannel,
                                        _progressPercent,
                                        elapsed,
                                        GetEstimatedRemaining(elapsed),
                                        _message)
    End Function

    Private Function GetElapsed() As TimeSpan
        If Not _startedAtUtc.HasValue Then Return TimeSpan.Zero
        Return DateTime.UtcNow.Subtract(_startedAtUtc.Value)
    End Function

    Private Function GetEstimatedRemaining(elapsed As TimeSpan) As TimeSpan?
        If _stage = TestProgressStage.Idle OrElse
           _stage = TestProgressStage.Completed OrElse
           _stage = TestProgressStage.Failed OrElse
           _stage = TestProgressStage.Canceled OrElse
           _progressPercent <= 0 OrElse
           _progressPercent >= 100 Then
            Return Nothing
        End If

        Dim averageSeconds As Double = elapsed.TotalSeconds / _progressPercent * 100.0R
        If averageSeconds <= 0 Then Return Nothing
        Return TimeSpan.FromSeconds(Math.Max(0, averageSeconds - elapsed.TotalSeconds))
    End Function
End Class
