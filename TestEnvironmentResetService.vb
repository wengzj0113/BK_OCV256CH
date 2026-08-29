Public NotInheritable Class TestEnvironmentResetService
    Private Sub New()
    End Sub

    Public Shared Sub ResetAll(middleMachine As SerialMiddleMachineClient,
                               Optional closeInstrumentSessions As Boolean = False,
                               Optional totalTimeoutMs As Integer = 30000)
        Dim deadline As DateTime = DateTime.UtcNow.AddMilliseconds(Math.Max(1, totalTimeoutMs))
        Dim remainingTimeoutMs As Integer

        If middleMachine IsNot Nothing Then
            Try
                remainingTimeoutMs = GetRemainingTimeoutMs(deadline)
                If remainingTimeoutMs > 0 Then
                    middleMachine.ResetToSafeState(remainingTimeoutMs)
                End If
            Catch ex As Exception
                SerialMiddleMachineClient.LogMessage("Environment reset: middle machine failed: " & ex.Message)
            End Try
        End If

        Try
            remainingTimeoutMs = GetRemainingTimeoutMs(deadline)
            If remainingTimeoutMs > 0 Then
                ResetCachedData_3562(remainingTimeoutMs)
            End If
        Catch ex As Exception
            SerialMiddleMachineClient.LogMessage("Environment reset: 3562 failed: " & ex.Message)
        End Try

        Try
            remainingTimeoutMs = GetRemainingTimeoutMs(deadline)
            If remainingTimeoutMs > 0 Then
                ResetCachedData_34461(remainingTimeoutMs)
            End If
        Catch ex As Exception
            SerialMiddleMachineClient.LogMessage("Environment reset: 34461 failed: " & ex.Message)
        End Try

        If closeInstrumentSessions Then
            'ResetCachedData_34461 已发送 ABOR；关闭阶段不再重复发送，避免再次消耗 VISA 超时。
            Close34461Session(False)
        End If
    End Sub

    Private Shared Function GetRemainingTimeoutMs(deadline As DateTime) As Integer
        Return CInt(Math.Max(0, Math.Ceiling((deadline - DateTime.UtcNow).TotalMilliseconds)))
    End Function
End Class
