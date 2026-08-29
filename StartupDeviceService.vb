Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>
''' Runs startup device checks away from the WinForms UI thread.
''' </summary>
Public NotInheritable Class StartupDeviceCheckResult
    Public Property Scm As Short = -1
    Public Property Dmm34461 As Short = -1
    Public Property Resistance3561 As Short = -1
    Public Property Plc As Short = -1
    Public Property Mes As Short = -1
    Public ReadOnly Property Errors As New List(Of String)()
End Class

Public NotInheritable Class StartupDeviceService
    Private Shared running As Integer

    Private Sub New()
    End Sub

    Public Shared Sub StartAsync(ByVal owner As MDIForm,
                                 ByVal completed As Action(Of StartupDeviceCheckResult))
        If owner Is Nothing OrElse completed Is Nothing Then Return
        If Interlocked.Exchange(running, 1) = 1 Then Return

        Task.Run(Sub()
                     Dim result As StartupDeviceCheckResult = Nothing
                     Try
                         result = DeviceStartupCheck()
                     Catch ex As Exception
                         result = New StartupDeviceCheckResult()
                         result.Errors.Add(ex.Message)
                         AppLog.Write("Startup", "[DEVICE] startup service exception: " & ex.Message)
                     Finally
                         Interlocked.Exchange(running, 0)
                     End Try

                     Try
                         If owner.IsDisposed Then Return
                         owner.BeginInvoke(New Action(Sub() completed(result)))
                     Catch ex As Exception
                         Debug.WriteLine("Startup device completion callback failed: " & ex.Message)
                     End Try
                 End Sub)
    End Sub

    Public Shared Function DeviceStartupCheck() As StartupDeviceCheckResult
        Dim result As New StartupDeviceCheckResult()
        StartupTiming.Mark("device-check-start")

        result.Scm = RunCheck("SCM", Function() CheckCom_SCM(), result)
        result.Dmm34461 = RunCheck("34461", Function() CheckCom_34461(), result)
        result.Resistance3561 = RunCheck("3561", Function() CheckCom_3561(True), result)
        result.Plc = RunCheck("PLC", Function() CheckPlc(), result)
        result.Mes = RunCheck("MES", Function() CheckTcp_MES(), result)

        StartupTiming.Mark("device-check-end", String.Format("SCM={0};34461={1};3561={2};PLC={3};MES={4}",
                                                               result.Scm,
                                                               result.Dmm34461,
                                                               result.Resistance3561,
                                                               result.Plc,
                                                               result.Mes))
        Return result
    End Function

    Private Shared Function RunCheck(ByVal name As String,
                                     ByVal operation As Func(Of Short),
                                     ByVal result As StartupDeviceCheckResult) As Short
        Try
            Return operation()
        Catch ex As Exception
            result.Errors.Add(name & "失败: " & ex.Message)
            AppLog.Write("Startup", "[DEVICE] " & name & " failed: " & ex.Message)
            Return -1
        End Try
    End Function
End Class
