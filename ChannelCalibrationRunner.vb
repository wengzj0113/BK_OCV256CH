Public Class ChannelCalibrationResult
    Public Property ChannelIndex0 As Integer
    Public Property MeasuredResistance As Double
    Public Property StandardResistance As Double
    Public Property Compensation As Double
    Public Property Success As Boolean
    Public Property Message As String
    Public Property CalibratedAt As DateTime
End Class

Public Class ChannelCalibrationRunner
    Private Const MiddleMachineTimeoutMs As Integer = 300000

    Private ReadOnly service As CalibrationService
    Private middleMachineClient As SerialMiddleMachineClient

    Public Sub New(service As CalibrationService)
        Me.service = service
    End Sub

    Public Function CalibrateSingleChannel(channelIndex0 As Integer, standardResistance As Double, Optional keepFixtureClosed As Boolean = False) As ChannelCalibrationResult
        Dim result As New ChannelCalibrationResult With {
            .ChannelIndex0 = channelIndex0,
            .StandardResistance = standardResistance,
            .CalibratedAt = DateTime.Now
        }

        Try
            Dim plan As MiddleMachineChannelPlan = BuildChannelPlan(New List(Of Integer) From {channelIndex0})
            GetMiddleMachineClient().RunPlan(plan, MiddleMachineTimeoutMs)

            Dim voltages As List(Of Single) = Nothing
            Dim resistances As List(Of Single) = Nothing
            Dim readFlag As Integer = Trans_ReadCachedData_3562(1, voltages, resistances)
            If readFlag <> 0 OrElse resistances Is Nothing OrElse resistances.Count = 0 Then
                Throw New InvalidOperationException("Read 3562 resistance data failed.")
            End If

            result.MeasuredResistance = resistances(0)
            result.Compensation = service.CalculateCompensation(standardResistance, result.MeasuredResistance)
            service.SetGlobalValue(channelIndex0, result.Compensation)
            result.Success = True
            result.Message = "Calibration completed."
        Catch ex As Exception
            result.Success = False
            result.Message = ex.Message
        Finally
            ResetEnvironment()
        End Try

        Return result
    End Function

    Public Function CalibrateChannels(channelIndexes0 As IList(Of Integer), standardResistance As Double) As List(Of ChannelCalibrationResult)
        If channelIndexes0 Is Nothing OrElse channelIndexes0.Count = 0 Then
            Throw New InvalidOperationException("No channels selected.")
        End If

        Try
            Dim plan As MiddleMachineChannelPlan = BuildChannelPlan(channelIndexes0)
            GetMiddleMachineClient().RunPlan(plan, MiddleMachineTimeoutMs)

            Dim voltages As List(Of Single) = Nothing
            Dim resistances As List(Of Single) = Nothing
            Dim readFlag As Integer = Trans_ReadCachedData_3562(plan.ExecutionOrder.Count, voltages, resistances)
            If readFlag <> 0 OrElse resistances Is Nothing OrElse resistances.Count < plan.ExecutionOrder.Count Then
                Throw New InvalidOperationException("Read cached 3562 resistance data failed.")
            End If

            Dim results As New List(Of ChannelCalibrationResult)()
            For i As Integer = 0 To plan.ExecutionOrder.Count - 1
                Dim channelIndex0 As Integer = plan.ExecutionOrder(i) - 1
                Dim measuredResistance As Double = resistances(i)
                Dim compensation As Double = service.CalculateCompensation(standardResistance, measuredResistance)
                service.SetGlobalValue(channelIndex0, compensation)
                results.Add(New ChannelCalibrationResult With {
                    .ChannelIndex0 = channelIndex0,
                    .MeasuredResistance = measuredResistance,
                    .StandardResistance = standardResistance,
                    .Compensation = compensation,
                    .Success = True,
                    .Message = "Calibration completed.",
                    .CalibratedAt = DateTime.Now
                })
            Next

            Return results
        Finally
            ResetEnvironment()
        End Try
    End Function

    Public Sub CloseFixture()
    End Sub

    Public Sub OpenFixture()
    End Sub

    Public Sub StopCurrentCalibration()
        If middleMachineClient Is Nothing Then
            middleMachineClient = GetMiddleMachineClient()
        End If
        ResetEnvironment()
    End Sub

    Private Sub ResetEnvironment()
        TestEnvironmentResetService.ResetAll(middleMachineClient)
    End Sub

    Private Function GetMiddleMachineClient() As SerialMiddleMachineClient
        If middleMachineClient Is Nothing Then
            middleMachineClient = New SerialMiddleMachineClient(frmMDI.Com_SCM)
        End If

        Return middleMachineClient
    End Function

    Private Function BuildChannelPlan(channelIndexes0 As IList(Of Integer)) As MiddleMachineChannelPlan
        Dim physicalChannels As New List(Of Integer)()
        For Each channelIndex0 As Integer In channelIndexes0
            Dim physicalChannel As Integer = channelIndex0 + 1
            If physicalChannel < 1 OrElse physicalChannel > CalibrationService.ChannelCount Then
                Throw New ArgumentOutOfRangeException("channelIndexes0")
            End If
            physicalChannels.Add(physicalChannel)
        Next
        physicalChannels.Sort()

        Dim plan As New MiddleMachineChannelPlan()
        For Each physicalChannel As Integer In physicalChannels
            plan.ExecutionOrder.Add(physicalChannel)
            If physicalChannel <= 128 Then
                plan.G1PhysicalChannels.Add(physicalChannel)
            Else
                plan.G2PhysicalChannels.Add(physicalChannel)
            End If
        Next
        Return plan
    End Function
End Class
