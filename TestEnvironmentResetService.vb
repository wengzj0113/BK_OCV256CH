Public NotInheritable Class TestEnvironmentResetService
    Private Sub New()
    End Sub

    Public Shared Sub ResetAll(middleMachine As SerialMiddleMachineClient,
                               Optional closeInstrumentSessions As Boolean = False)
        If middleMachine IsNot Nothing Then
            Try
                middleMachine.ResetToSafeState()
            Catch ex As Exception
                SerialMiddleMachineClient.LogMessage("Environment reset: middle machine failed: " & ex.Message)
            End Try
        End If

        Try
            ResetCachedData_3562()
        Catch ex As Exception
            SerialMiddleMachineClient.LogMessage("Environment reset: 3562 failed: " & ex.Message)
        End Try

        Try
            ResetCachedData_34461()
        Catch ex As Exception
            SerialMiddleMachineClient.LogMessage("Environment reset: 34461 failed: " & ex.Message)
        End Try

        If closeInstrumentSessions Then
            Close34461Session()
        End If
    End Sub
End Class
