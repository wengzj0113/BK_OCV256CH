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
        If codes.Count = 0 Then Throw New InvalidOperationException("本托盘没有有效电芯码。")

        Dim measurement As List(Of Integer)
        If automaticMode Then
            measurement = Enumerable.Range(1, 256).ToList()
        Else
            measurement = If(selectedChannels, Enumerable.Empty(Of Integer)()).Distinct().OrderBy(Function(channel) channel).ToList()
            If measurement.Count = 0 Then Throw New InvalidOperationException("请先选择至少一个待测试通道。")
            For Each channel As Integer In measurement
                If channel < 1 OrElse channel > 256 Then Throw New InvalidOperationException("所选通道超出范围：CH" & channel)
                If Not codes.ContainsKey(channel) Then Throw New InvalidOperationException("所选通道缺少电芯码：CH" & channel)
            Next
        End If
        Dim occupied As List(Of Integer) = measurement.Where(Function(channel) codes.ContainsKey(channel)).ToList()
        Return New TrayTestPlan(measurement, occupied, codes)
    End Function
End Class
