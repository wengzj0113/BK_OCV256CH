Imports System.Linq

Public Class TestChannelSelection
    Public Property SelectedPhysicalChannels As List(Of Integer)
    Public Property G1PhysicalChannels As List(Of Integer)
    Public Property G2PhysicalChannels As List(Of Integer)
    Public Property G2LocalChannels As List(Of Integer)
    Public Property ExpectedHiokiCount As Integer
    Public Property ExpectedDmmCount As Integer

    Public Sub New()
        SelectedPhysicalChannels = New List(Of Integer)()
        G1PhysicalChannels = New List(Of Integer)()
        G2PhysicalChannels = New List(Of Integer)()
        G2LocalChannels = New List(Of Integer)()
    End Sub

    Public Function ToMiddleMachinePlan() As MiddleMachineChannelPlan
        Dim plan As New MiddleMachineChannelPlan()
        plan.SelectedPhysicalChannels.AddRange(SelectedPhysicalChannels)
        plan.G1PhysicalChannels.AddRange(G1PhysicalChannels)
        plan.G2PhysicalChannels.AddRange(G2PhysicalChannels)
        plan.G2LocalChannels.AddRange(G2LocalChannels)
        plan.ExecutionOrder.AddRange(SelectedPhysicalChannels)
        plan.ExpectedHiokiCount = ExpectedHiokiCount
        plan.ExpectedDmmCount = ExpectedDmmCount
        Return plan
    End Function
End Class

Public Class TestChannelPlanService
    Public Function BuildRangeSelection(startChannel As Integer, endChannel As Integer) As TestChannelSelection
        If startChannel > endChannel Then
            Throw New ArgumentException("Start channel cannot be greater than end channel.")
        End If

        Dim channels As New List(Of Integer)()
        For channel As Integer = startChannel To endChannel
            channels.Add(channel)
        Next
        Return BuildSparseSelection(channels)
    End Function

    Public Function BuildSparseSelection(channelNumbers As IEnumerable(Of Integer)) As TestChannelSelection
        If channelNumbers Is Nothing Then
            Throw New ArgumentNullException("channelNumbers")
        End If

        Dim selected As List(Of Integer) = channelNumbers.
            Select(Function(channel) CInt(channel)).
            Distinct().
            OrderBy(Function(channel) channel).
            ToList()

        If selected.Count = 0 Then
            Throw New InvalidOperationException("Channel selection is empty.")
        End If

        For Each channel As Integer In selected
            ValidatePhysicalChannel(channel)
        Next

        Dim selection As New TestChannelSelection()
        selection.SelectedPhysicalChannels.AddRange(selected)
        selection.G1PhysicalChannels.AddRange(selected.Where(Function(channel) channel <= 128))
        selection.G2PhysicalChannels.AddRange(selected.Where(Function(channel) channel > 128))

        For Each channel As Integer In selection.G2PhysicalChannels
            selection.G2LocalChannels.Add(channel - 128)
        Next

        selection.ExpectedHiokiCount = selection.G1PhysicalChannels.Count + selection.G2PhysicalChannels.Count
        selection.ExpectedDmmCount = selection.G2PhysicalChannels.Count + selection.G1PhysicalChannels.Count
        Return selection
    End Function

    Public Function BuildRetestSelection(retestFlags As Int16()) As TestChannelSelection
        If retestFlags Is Nothing Then
            Throw New ArgumentNullException("retestFlags")
        End If

        Dim channels As New List(Of Integer)()
        Dim count As Integer = Math.Min(retestFlags.Length, CalibrationService.ChannelCount)
        For i As Integer = 0 To count - 1
            If retestFlags(i) = 1 Then
                channels.Add(i + 1)
            End If
        Next

        Return BuildSparseSelection(channels)
    End Function

    Private Shared Sub ValidatePhysicalChannel(channel As Integer)
        If channel < 1 OrElse channel > CalibrationService.ChannelCount Then
            Throw New ArgumentOutOfRangeException("channel", "Channel must be between 1 and 256.")
        End If
    End Sub
End Class
