Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>
''' Runs startup device checks away from the WinForms UI thread.
''' </summary>
Public NotInheritable Class StartupDeviceCheckResult
    Public Property Scm As Short = -1
    Public Property ScmChecked As Boolean = True
    Public Property Dmm34461 As Short = -1
    Public Property Dmm34461Checked As Boolean = True
    Public Property Resistance3561 As Short = -1
    Public Property Resistance3561Checked As Boolean = True
    Public Property Retest3562 As Short = -1
    Public Property Retest3562Checked As Boolean = False
    Public Property Plc As Short = -1
    Public Property PlcChecked As Boolean = True
    Public Property Mes As Short = -1
    Public Property MesChecked As Boolean = True
    Public ReadOnly Property Errors As New List(Of String)()
    Public ReadOnly Property FailureReasons As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

    Public ReadOnly Property HasFailures As Boolean
        Get
            Return Scm <> 1 OrElse
                   Dmm34461 <> 0 OrElse
                   Resistance3561 <> 1 OrElse
                   (Retest3562Checked AndAlso Retest3562 <> 1) OrElse
                   Plc <> 1 OrElse
                   Mes <> 1
        End Get
    End Property
End Class

Public NotInheritable Class StartupDeviceCheckOptions
    Public Property CheckScm As Boolean = True
    Public Property CheckDmm34461 As Boolean = True
    Public Property CheckResistance3561 As Boolean = True
    Public Property ReportRetest3562Failures As Boolean = False
    Public Property CheckPlc As Boolean = True
    Public Property CheckMes As Boolean = True
    Public Property CheckRetest3562 As Boolean = True
End Class

Public NotInheritable Class StartupDeviceService
    Private Shared running As Integer

    Private Sub New()
    End Sub

    Public Shared ReadOnly Property IsRunning As Boolean
        Get
            Return Interlocked.CompareExchange(running, 0, 0) = 1
        End Get
    End Property

    Public Shared Function StartAsync(ByVal owner As MDIForm,
                                      ByVal completed As Action(Of StartupDeviceCheckResult),
                                      Optional ByVal options As StartupDeviceCheckOptions = Nothing) As Boolean
        If owner Is Nothing OrElse completed Is Nothing Then Return False
        If Interlocked.Exchange(running, 1) = 1 Then Return False

        Task.Run(Sub()
                     Dim result As StartupDeviceCheckResult = Nothing
                     Try
                         result = DeviceStartupCheck(options)
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
        Return True
    End Function

    Public Shared Function DeviceStartupCheck(Optional ByVal options As StartupDeviceCheckOptions = Nothing) As StartupDeviceCheckResult
        If options Is Nothing Then options = New StartupDeviceCheckOptions()
        Dim result As New StartupDeviceCheckResult()
        StartupTiming.Mark("device-check-start")

        result.ScmChecked = options.CheckScm
        result.Scm = If(options.CheckScm, RunCheck("SCM", 1, Function() CheckCom_SCM(True), result), CShort(1))
        result.Dmm34461Checked = options.CheckDmm34461
        result.Dmm34461 = If(options.CheckDmm34461, RunCheck("34461", 0, Function() InitializeConfigured34461AtStartup(), result), CShort(0))
        result.Resistance3561Checked = options.CheckResistance3561
        result.Resistance3561 = If(options.CheckResistance3561, RunCheck("3561", 1, Function() InitializeMain3562AtStartup(), result), CShort(1))
        result.Retest3562Checked = options.CheckRetest3562 AndAlso options.ReportRetest3562Failures
        If options.CheckRetest3562 Then
            If options.ReportRetest3562Failures Then
                result.Retest3562 = RunCheck("Retest3562", 1, Function() InitializeRetest3562AtStartup(), result)
            Else
                result.Retest3562 = InitializeRetest3562AtStartup()
            End If
        Else
            result.Retest3562 = 1
        End If
        result.PlcChecked = options.CheckPlc
        result.Plc = If(options.CheckPlc, RunCheck("PLC", 1, Function() CheckPlc(), result), CShort(1))
        result.MesChecked = options.CheckMes
        result.Mes = If(options.CheckMes, RunCheck("MES", 1, Function() CheckTcp_MES(), result), CShort(1))

        StartupTiming.Mark("device-check-end", String.Format("SCM={0};34461={1};3561={2};PLC={3};MES={4}",
                                                               result.Scm,
                                                               result.Dmm34461,
                                                               result.Resistance3561,
                                                               result.Plc,
                                                               result.Mes))
        Return result
    End Function

    Private Shared Function InitializeMain3562AtStartup() As Short
        Try
            'ConnectFor3561 verifies the model identity, resets the meter and calls InitRes,
            'which applies the persisted 3562 settings before startup is marked successful.
            Return If(ConnectFor3561(True) = 0, CShort(1), CShort(-1))
        Catch ex As Exception
            SerialMiddleMachineClient.LogMessage("3562 startup initialization failed: " & ex.Message)
            Return -1
        End Try
    End Function

    Private Shared Function InitializeRetest3562AtStartup() As Short
        Try
            Using meter As New Retest3562Service()
                meter.Connect("COM" & (Initsetting.Retest3562Port + 1).ToString())
            End Using
            SerialMiddleMachineClient.LogMessage("Retest 3562 startup settings applied.")
            Return 1
        Catch ex As Exception
            '复测仪不参与正常生产启动；失败记日志即可，不阻止主测试工位启动。
            SerialMiddleMachineClient.LogMessage("Retest 3562 startup initialization skipped: " & ex.Message)
            Return -1
        End Try
    End Function

    Private Shared Function RunCheck(ByVal name As String,
                                     ByVal successCode As Short,
                                     ByVal operation As Func(Of Short),
                                     ByVal result As StartupDeviceCheckResult) As Short
        Try
            Dim status As Short = operation()
            If status <> successCode AndAlso Not result.FailureReasons.ContainsKey(name) Then
                RecordFailure(name, "检查返回失败码 " & status.ToString(), result)
            End If
            Return status
        Catch ex As Exception
            RecordFailure(name, ex.Message, result)
            Return -1
        End Try
    End Function

    Private Shared Sub RecordFailure(ByVal name As String,
                                      ByVal reason As String,
                                      ByVal result As StartupDeviceCheckResult)
        Dim detail As String = If(String.IsNullOrWhiteSpace(reason), "检查失败", reason.Trim())
        result.FailureReasons(name) = detail
        result.Errors.Add(name & "失败: " & detail)
        AppLog.Write("Startup", "[DEVICE] " & name & " failed: " & detail)
    End Sub
End Class
