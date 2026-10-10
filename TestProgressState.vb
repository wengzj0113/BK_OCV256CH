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
    Private Shared ReadOnly EstimatedDuration As TimeSpan = TimeSpan.FromSeconds(90)
    Private _stage As TestProgressStage = TestProgressStage.Idle
    Private _completedCount As Integer
    Private _totalCount As Integer
    Private _currentChannel As Integer?
    Private _message As String = String.Empty
    Private _startedAtUtc As DateTime?
    Private _endedAtUtc As DateTime?

    Public Sub Reset(Optional message As String = "等待开始")
        _stage = TestProgressStage.Idle
        _completedCount = 0
        _totalCount = 0
        _currentChannel = Nothing
        _message = If(message, String.Empty)
        _startedAtUtc = Nothing
        _endedAtUtc = Nothing
    End Sub

    Public Sub Begin(totalCount As Integer, Optional message As String = "准备测试")
        _stage = TestProgressStage.Preparing
        _completedCount = 0
        _totalCount = Math.Max(0, totalCount)
        _currentChannel = Nothing
        _message = If(message, String.Empty)
        _startedAtUtc = DateTime.UtcNow
        _endedAtUtc = Nothing
    End Sub

    Public Sub SetStage(stage As TestProgressStage,
                        Optional message As String = Nothing,
                        Optional currentChannel As Integer? = Nothing,
                        Optional progressPercent As Integer? = Nothing)
        _stage = stage
        If message IsNot Nothing Then _message = message
        If currentChannel.HasValue Then _currentChannel = currentChannel
    End Sub

    Public Sub Advance(completedCount As Integer,
                       Optional currentChannel As Integer? = Nothing,
                       Optional message As String = Nothing)
        _completedCount = Math.Max(0, Math.Min(completedCount, _totalCount))
        If currentChannel.HasValue Then _currentChannel = currentChannel
        If message IsNot Nothing Then _message = message
    End Sub

    Public Sub Complete(Optional message As String = "测试完成")
        _completedCount = _totalCount
        _currentChannel = Nothing
        _stage = TestProgressStage.Completed
        _message = If(message, String.Empty)
        _endedAtUtc = DateTime.UtcNow
    End Sub

    Public Sub Fail(message As String)
        _stage = TestProgressStage.Failed
        _message = If(message, "测试失败")
        _endedAtUtc = DateTime.UtcNow
    End Sub

    Public Sub Cancel(message As String)
        _stage = TestProgressStage.Canceled
        _message = If(message, "测试已停止")
        _endedAtUtc = DateTime.UtcNow
    End Sub

    Public Function CreateSnapshot() As TestProgressSnapshot
        Dim elapsed As TimeSpan = GetElapsed()
        Return New TestProgressSnapshot(_stage,
                                        _completedCount,
                                        _totalCount,
                                        _currentChannel,
                                        GetProgressPercent(elapsed),
                                        elapsed,
                                        GetEstimatedRemaining(elapsed),
                                        _message)
    End Function

    Private Function GetElapsed() As TimeSpan
        If Not _startedAtUtc.HasValue Then Return TimeSpan.Zero
        Dim endTime As DateTime = If(_endedAtUtc, DateTime.UtcNow)
        Return TimeSpan.FromTicks(Math.Max(0, endTime.Subtract(_startedAtUtc.Value).Ticks))
    End Function

    Private Function GetProgressPercent(elapsed As TimeSpan) As Integer
        If _stage = TestProgressStage.Idle Then Return 0
        If _stage = TestProgressStage.Completed Then Return 100
        If Not _startedAtUtc.HasValue Then Return 0

        Dim percent As Integer = CInt(Math.Floor(elapsed.TotalSeconds / EstimatedDuration.TotalSeconds * 100.0R))
        Return Math.Max(0, Math.Min(99, percent))
    End Function

    Private Function GetEstimatedRemaining(elapsed As TimeSpan) As TimeSpan?
        If _stage = TestProgressStage.Idle OrElse
           _stage = TestProgressStage.Completed OrElse
           _stage = TestProgressStage.Failed OrElse
           _stage = TestProgressStage.Canceled Then
            Return Nothing
        End If

        Return TimeSpan.FromSeconds(Math.Ceiling(Math.Max(0, EstimatedDuration.TotalSeconds - elapsed.TotalSeconds)))
    End Function
End Class
