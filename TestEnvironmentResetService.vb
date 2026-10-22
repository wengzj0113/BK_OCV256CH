Public NotInheritable Class TestEnvironmentResetResult
    Private ReadOnly resetFailures As New List(Of String)()

    Public ReadOnly Property Failures As IReadOnlyList(Of String)
        Get
            Return resetFailures.AsReadOnly()
        End Get
    End Property

    Public ReadOnly Property Succeeded As Boolean
        Get
            Return resetFailures.Count = 0
        End Get
    End Property

    Friend Sub AddFailure(deviceName As String, message As String)
        Dim detail As String = If(String.IsNullOrWhiteSpace(message), "未知错误", message.Trim())
        resetFailures.Add(deviceName & "复位失败：" & detail)
    End Sub
End Class

Public NotInheritable Class TestEnvironmentResetService
    Private Sub New()
    End Sub

    Public Shared Sub ResetAll(middleMachine As SerialMiddleMachineClient,
                               Optional closeInstrumentSessions As Boolean = False,
                               Optional totalTimeoutMs As Integer = 30000)
        Dim ignoredResult As TestEnvironmentResetResult = ResetAllWithResult(middleMachine, closeInstrumentSessions, totalTimeoutMs)
    End Sub

    Public Shared Function ResetAllWithResult(middleMachine As SerialMiddleMachineClient,
                                              Optional closeInstrumentSessions As Boolean = False,
                                              Optional totalTimeoutMs As Integer = 30000) As TestEnvironmentResetResult
        Dim result As New TestEnvironmentResetResult()
        Dim deadline As DateTime = DateTime.UtcNow.AddMilliseconds(Math.Max(1, totalTimeoutMs))
        Dim remainingTimeoutMs As Integer

        If middleMachine IsNot Nothing Then
            Try
                remainingTimeoutMs = GetRemainingTimeoutMs(deadline)
                If remainingTimeoutMs > 0 Then
                    middleMachine.ResetToSafeState(remainingTimeoutMs)
                Else
                    result.AddFailure("中位机", "整体复位超时")
                End If
            Catch ex As Exception
                SerialMiddleMachineClient.LogMessage("Environment reset: middle machine failed: " & ex.Message)
                result.AddFailure("中位机", ex.Message)
            End Try
        Else
            result.AddFailure("中位机", "串口客户端未初始化")
        End If

        Try
            remainingTimeoutMs = GetRemainingTimeoutMs(deadline)
            If remainingTimeoutMs > 0 Then
                Dim resetError As String = ResetCachedData_3562ForReset(remainingTimeoutMs)
                If Not String.IsNullOrWhiteSpace(resetError) Then result.AddFailure("3562", resetError)
            Else
                result.AddFailure("3562", "整体复位超时")
            End If
        Catch ex As Exception
            SerialMiddleMachineClient.LogMessage("Environment reset: 3562 failed: " & ex.Message)
            result.AddFailure("3562", ex.Message)
        End Try

        Try
            remainingTimeoutMs = GetRemainingTimeoutMs(deadline)
            If remainingTimeoutMs > 0 Then
                Dim resetErrors As List(Of String) = ResetCachedData_34461ForReset(remainingTimeoutMs)
                For Each resetError As String In resetErrors
                    result.AddFailure("34461", resetError)
                Next
            Else
                result.AddFailure("34461", "整体复位超时")
            End If
        Catch ex As Exception
            SerialMiddleMachineClient.LogMessage("Environment reset: 34461 failed: " & ex.Message)
            result.AddFailure("34461", ex.Message)
        End Try

        If closeInstrumentSessions Then
            'ResetCachedData_34461 已发送 ABOR；关闭阶段不再重复发送，避免再次消耗 VISA 超时。
            Try
                Close34461Session(False)
            Catch ex As Exception
                SerialMiddleMachineClient.LogMessage("Environment reset: closing 34461 failed: " & ex.Message)
                result.AddFailure("34461", "关闭会话失败：" & ex.Message)
            End Try
        End If

        Return result
    End Function

    Private Shared Function GetRemainingTimeoutMs(deadline As DateTime) As Integer
        Return CInt(Math.Max(0, Math.Ceiling((deadline - DateTime.UtcNow).TotalMilliseconds)))
    End Function
End Class
