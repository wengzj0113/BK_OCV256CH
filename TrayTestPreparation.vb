Imports System.Collections.Generic
Imports System.Data
Imports System.Linq

Public NotInheritable Class TrayTestPlan
    Public ReadOnly Property MeasurementChannels As IReadOnlyList(Of Integer)
    Public ReadOnly Property OccupiedChannels As IReadOnlyList(Of Integer)
    Public ReadOnly Property BatteryCodes As IReadOnlyDictionary(Of Integer, String)

    Public Sub New(ByVal measurement As IEnumerable(Of Integer), ByVal occupied As IEnumerable(Of Integer), ByVal codes As IDictionary(Of Integer, String))
        MeasurementChannels = Array.AsReadOnly(measurement.ToArray())
        OccupiedChannels = Array.AsReadOnly(occupied.ToArray())
        BatteryCodes = New System.Collections.ObjectModel.ReadOnlyDictionary(Of Integer, String)(New Dictionary(Of Integer, String)(codes))
    End Sub
End Class

Public NotInheritable Class NoTestableTrayChannelsException
    Inherits InvalidOperationException

    Public Sub New(ByVal message As String)
        MyBase.New(message)
    End Sub
End Class

Public NotInheritable Class TrayTestPreparation
    Private Sub New()
    End Sub

    Public Shared Function Prepare(ByVal trayCells As DataTable,
                                   ByVal selectedChannels As IEnumerable(Of Integer),
                                   ByVal automaticMode As Boolean) As TrayTestPlan
        If trayCells Is Nothing Then Throw New ArgumentNullException(NameOf(trayCells))
        Dim codes As New Dictionary(Of Integer, String)()
        Dim seenChannels As New HashSet(Of Integer)()
        Dim seenCodes As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each row As DataRow In trayCells.Rows
            If row.IsNull("ChannelNo") Then Throw New InvalidOperationException("托盘数据包含空通道号。")
            Dim channel As Integer = Convert.ToInt32(row("ChannelNo"))
            If channel < 1 OrElse channel > 256 Then Throw New InvalidOperationException("托盘数据包含非法通道 CH" & channel)
            If Not seenChannels.Add(channel) Then Throw New InvalidOperationException("托盘数据包含重复通道 CH" & channel)
            Dim code As String = If(row.IsNull("BatteryCode"), String.Empty, Convert.ToString(row("BatteryCode"))).Trim()
            If code.Length = 0 Then Continue For
            If Not seenCodes.Add(code) Then Throw New InvalidOperationException("托盘数据包含重复电芯码：" & code)
            codes.Add(channel, code)
        Next
        If codes.Count = 0 Then Throw New NoTestableTrayChannelsException("本托盘没有有效电芯码。")

        Dim requested As List(Of Integer)
        If automaticMode Then
            requested = codes.Keys.OrderBy(Function(channel) channel).ToList()
        Else
            requested = If(selectedChannels, Enumerable.Empty(Of Integer)()).Distinct().OrderBy(Function(channel) channel).ToList()
            If requested.Count = 0 Then Throw New InvalidOperationException("请先选择至少一个待测试通道。")
            For Each channel As Integer In requested
                If channel < 1 OrElse channel > 256 Then Throw New InvalidOperationException("所选通道超出范围：CH" & channel)
            Next
        End If

        Dim measurement As List(Of Integer) = requested.Where(Function(channel) codes.ContainsKey(channel)).ToList()
        If measurement.Count = 0 Then
            Dim unavailableChannels As String = String.Join(",", requested.Select(Function(channel) "CH" & channel.ToString()))
            Throw New NoTestableTrayChannelsException("所选通道没有有效电芯码：" & unavailableChannels)
        End If

        Dim occupied As List(Of Integer) = measurement.ToList()
        Return New TrayTestPlan(measurement, occupied, codes)
    End Function
End Class
