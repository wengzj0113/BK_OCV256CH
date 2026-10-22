Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Data
Imports System.Globalization
Imports System.Linq
Imports System.Security.Cryptography
Imports System.Text

Public NotInheritable Class ManualTestBarcodePlan
    Public ReadOnly Property TrayCode As String
    Public ReadOnly Property IsSimulated As Boolean
    Public ReadOnly Property Codes As IReadOnlyDictionary(Of Integer, String)

    Private Sub New(tray As String, simulated As Boolean, values As Dictionary(Of Integer, String))
        TrayCode = tray
        IsSimulated = simulated
        Codes = New ReadOnlyDictionary(Of Integer, String)(New Dictionary(Of Integer, String)(values))
    End Sub

    Public Shared Function Create(tray As String, cells As DataTable, capturedAt As DateTime) As ManualTestBarcodePlan
        tray = If(tray, String.Empty).Trim()
        If tray.Length = 0 OrElse tray.Length > 100 Then Throw New ArgumentException("托盘码长度必须为 1–100 个字符。")
        If cells Is Nothing Then Throw New ArgumentNullException(NameOf(cells))
        Dim values As New Dictionary(Of Integer, String)()
        Dim channels As New HashSet(Of Integer)()
        Dim seenCodes As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each row As DataRow In cells.Rows
            If row.IsNull("ChannelNo") Then Throw New InvalidOperationException("托盘数据包含空通道号。")
            Dim channel As Integer = Convert.ToInt32(row("ChannelNo"), CultureInfo.InvariantCulture)
            If channel < 1 OrElse channel > 256 OrElse Not channels.Add(channel) Then
                Throw New InvalidOperationException("托盘数据包含非法或重复通道 CH" & channel.ToString())
            End If
            Dim code As String = If(row.IsNull("BatteryCode"), String.Empty, Convert.ToString(row("BatteryCode"))).Trim()
            If code.Length = 0 Then Continue For
            If code.Length > 100 OrElse Not seenCodes.Add(code) Then Throw New InvalidOperationException("电芯条码过长或重复：" & code)
            values.Add(channel, code)
        Next
        Dim simulated As Boolean = values.Count = 0
        If simulated Then
            Dim trayKey As String
            Using hash As SHA256 = SHA256.Create()
                trayKey = BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(tray.ToUpperInvariant()))).Replace("-", "").Substring(0, 16)
            End Using
            For channel As Integer = 1 To 256
                values.Add(channel, String.Format(CultureInfo.InvariantCulture, "TEST-{0:yyyyMMdd}-{1}-{2}", capturedAt, trayKey, channel))
            Next
        End If
        Return New ManualTestBarcodePlan(tray, simulated, values)
    End Function

    Public Function ValidateChannels(selected As IEnumerable(Of Integer)) As List(Of Integer)
        Dim result As List(Of Integer) = If(selected, Enumerable.Empty(Of Integer)()).Distinct().OrderBy(Function(c) c).ToList()
        If result.Count = 0 Then Throw New InvalidOperationException("请先选择至少一个通道。")
        Dim missing = result.Where(Function(c) Not Codes.ContainsKey(c)).ToList()
        If missing.Count > 0 Then Throw New InvalidOperationException("以下所选通道没有电芯条码，请调整选择：" & String.Join(",", missing.Select(Function(c) "CH" & c)))
        Return result
    End Function
End Class

Public NotInheritable Class ManualTestRunContext
    Public ReadOnly Property Plan As ManualTestBarcodePlan
    Public ReadOnly Property Channels As IReadOnlyList(Of Integer)
    Public ReadOnly Property OcvNumber As Integer
    Public ReadOnly Property OcvStageCode As String
    Public ReadOnly Property OperatorName As String
    Public ReadOnly Property StartedAt As DateTime
    Public ReadOnly Property RunId As String = Guid.NewGuid().ToString("N")
    Public ReadOnly Property ProcessNumber As String
    Public ReadOnly Property Temperature As String
    Public ReadOnly Property ProbeRemaining As String
    Public ReadOnly Property MesEnabled As Boolean
    Public ReadOnly Property MesAddress As String
    Public ReadOnly Property MesUser As String
    Public ReadOnly Property MesPassword As String
    Public ReadOnly Property VoltageLowerMillivolts As Double
    Public ReadOnly Property VoltageUpperMillivolts As Double
    Public ReadOnly Property ResistanceLower As Double
    Public ReadOnly Property ResistanceUpper As Double

    Public Sub New(plan As ManualTestBarcodePlan, selected As IEnumerable(Of Integer), ocv As Integer,
                   user As String, started As DateTime, Optional process As String = "", Optional temperature As String = "",
                   Optional probe As String = "", Optional mesEnabled As Boolean = False, Optional mesAddress As String = "",
                   Optional mesUser As String = "", Optional mesPassword As String = "", Optional minVoltage As Double = 0,
                   Optional maxVoltage As Double = Double.MaxValue, Optional minResistance As Double = 0,
                   Optional maxResistance As Double = Double.MaxValue)
        If plan Is Nothing Then Throw New ArgumentNullException(NameOf(plan))
        If String.IsNullOrWhiteSpace(user) Then Throw New ArgumentException("当前未登录，不能保存测试任务。")
        Me.Plan = plan
        Channels = Array.AsReadOnly(plan.ValidateChannels(selected).ToArray())
        OcvNumber = ocv
        OcvStageCode = "OCV" & Math.Max(0, ocv - 1).ToString(CultureInfo.InvariantCulture)
        OperatorName = user
        StartedAt = started
        ProcessNumber = process
        Me.Temperature = temperature
        ProbeRemaining = probe
        Me.MesEnabled = mesEnabled
        Me.MesAddress = If(mesAddress, String.Empty).Trim()
        Me.MesUser = mesUser
        Me.MesPassword = mesPassword
        VoltageLowerMillivolts = minVoltage
        VoltageUpperMillivolts = maxVoltage
        ResistanceLower = minResistance
        ResistanceUpper = maxResistance
    End Sub
End Class

Public NotInheritable Class ManualTestRoundSnapshot
    Public ReadOnly Property Context As ManualTestRunContext
    Public ReadOnly Property RoundNumber As Integer
    Public ReadOnly Property StartedAt As DateTime
    Public ReadOnly Property CapturedAt As DateTime
    Private ReadOnly measurements As Bat_info()
    Private ReadOnly verdicts As Dictionary(Of Integer, Integer)

    Private Sub New(context As ManualTestRunContext, number As Integer, source As Bat_info(), started As DateTime, ended As DateTime)
        If context Is Nothing Then Throw New ArgumentNullException(NameOf(context))
        If source Is Nothing OrElse source.Length < 256 Then Throw New ArgumentException("测量数据不足 256 通道。")
        If number < 1 Then Throw New ArgumentOutOfRangeException(NameOf(number))
        Me.Context = context
        RoundNumber = number
        StartedAt = started
        CapturedAt = ended
        measurements = New Bat_info(255) {}
        verdicts = New Dictionary(Of Integer, Integer)()
        For Each channel As Integer In context.Channels
            Dim item = source(channel - 1)
            item.BatCode = context.Plan.Codes(channel)
            item.TestTime = ended
            Dim invalidVoltage As Boolean = Single.IsNaN(item.Voltage) OrElse Single.IsInfinity(item.Voltage)
            Dim invalidResistance As Boolean = Single.IsNaN(item.Resistance) OrElse Single.IsInfinity(item.Resistance)
            Dim code As Integer = 0
            If invalidVoltage OrElse CDbl(item.Voltage) * 1000.0R < context.VoltageLowerMillivolts OrElse CDbl(item.Voltage) * 1000.0R > context.VoltageUpperMillivolts Then code = 1
            If invalidResistance OrElse item.Resistance >= InstrumentCacheParser.InvalidResistanceMilliohm OrElse item.Resistance < context.ResistanceLower OrElse item.Resistance > context.ResistanceUpper Then code = code Or 2
            item.Result = If(code = 0, "OK", "NG")
            measurements(channel - 1) = item
            verdicts.Add(channel, code)
        Next
    End Sub

    Friend Shared Function Capture(context As ManualTestRunContext, number As Integer, source As Bat_info(), started As DateTime, ended As DateTime) As ManualTestRoundSnapshot
        Return New ManualTestRoundSnapshot(context, number, source, started, ended)
    End Function

    Friend Function CopyMeasurements() As Bat_info()
        Return CType(measurements.Clone(), Bat_info())
    End Function

    Public Function NgCode(channel As Integer) As Integer
        Return verdicts(channel)
    End Function

    Public Function CreateResultInputs() As List(Of OcvTestResultInput)
        Dim inputs As New List(Of OcvTestResultInput)()
        For Each channel As Integer In Context.Channels
            Dim item As Bat_info = measurements(channel - 1)
            Dim code As Integer = verdicts(channel)
            inputs.Add(New OcvTestResultInput With {
                .ChannelNo = channel, .BatteryCode = item.BatCode, .ExecutionOrder = inputs.Count + 1,
                .VoltageVolt = Convert.ToDecimal(item.Voltage), .RawResistanceMilliohm = Convert.ToDecimal(item.RawResistanceMilliohm),
                .CalibrationValueMilliohm = Convert.ToDecimal(item.CalibrationValueMilliohm), .FinalResistanceMilliohm = Convert.ToDecimal(item.FinalResistanceMilliohm),
                .IsNg = code <> 0, .NgReason = If(code = 0, Nothing, If(code = 1, "电压超限", If(code = 2, "内阻超限或无效", "电压及内阻超限或无效"))),
                .ResultSource = If(Context.Plan.IsSimulated, "Debug", "ManualTest")})
        Next
        Return inputs
    End Function
End Class

Friend NotInheritable Class ManualTestResultService
    Private Sub New()
    End Sub

    Public Shared Sub SaveRounds(rounds As IList(Of ManualTestRoundSnapshot), connectionString As String)
        Using service As New OcvDataService(connectionString)
            For Each round As ManualTestRoundSnapshot In rounds
                Dim context = round.Context
                Dim key = "MANUAL-" & context.RunId & "-" & round.RoundNumber.ToString(CultureInfo.InvariantCulture)
                Dim taskId As Long? = Nothing
                Try
                    Dim task = service.CreateTestTask(New OcvCreateTestTaskRequest With {
                        .TaskCode = key, .IdempotencyKey = key, .TrayCode = context.Plan.TrayCode, .OcvStageCode = context.OcvStageCode,
                        .TestType = If(context.Plan.IsSimulated, "Debug", "Manual"),
                        .SelectedChannelsJson = "[" & String.Join(",", context.Channels) & "]",
                        .FirstChannelNo = context.Channels.Min(), .LastChannelNo = context.Channels.Max(), .ChannelCount = context.Channels.Count,
                        .EquipmentCode = "XL_OCV_256CH", .WorkstationName = Environment.MachineName, .OperatorName = context.OperatorName, .IsRetest = False})
                    If task.Rows.Count = 0 Then Throw New InvalidOperationException("数据库未返回测试任务编号。")
                    taskId = Convert.ToInt64(task.Rows(0)("TestTaskId"))
                    service.SaveTestResults(taskId.Value, round.CapturedAt, round.CreateResultInputs())
                    service.CompleteTestTask(taskId.Value)
                Catch ex As Exception
                    If taskId.HasValue Then
                        Try
                            service.CompleteTestTask(taskId.Value, "Failed", ex.Message)
                        Catch statusException As Exception
                            AppLog.Write("ManualTest", "标记保存失败任务时出错：" & statusException.Message)
                        End Try
                    End If
                    Throw New InvalidOperationException("第 " & round.RoundNumber.ToString() & " 轮入库失败；此前成功写入的轮次已保留，本次不上传 MES：" & ex.Message, ex)
                End Try
            Next
        End Using
    End Sub
End Class
