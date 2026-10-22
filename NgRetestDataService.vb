Imports System.Data
Imports System.Data.SqlClient

Public Class NgRetestDataService
    Private ReadOnly connectionString As String

    Public Sub New()
        connectionString = OcvDataService.BuildConnectionString()
    End Sub

    Public Function LoadByBatteryCode(ByVal batteryCode As String) As NgRetestSource
        If String.IsNullOrWhiteSpace(batteryCode) Then Throw New ArgumentException("电池条码不能为空。", NameOf(batteryCode))

        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As New SqlCommand("dbo.usp_GetNgRetestSourceByBatteryCode", connection)
                command.CommandType = CommandType.StoredProcedure
                command.CommandTimeout = 10
                command.Parameters.Add("@BatteryCode", SqlDbType.NVarChar, 100).Value = batteryCode.Trim()
                Using reader As SqlDataReader = command.ExecuteReader()
                    If Not reader.Read() Then Return Nothing
                    Dim source As New NgRetestSource() With {
                        .SourceTestResultId = Convert.ToInt64(reader("TestResultId")),
                        .BatteryCode = ReadString(reader, "BatteryCode"),
                        .TrayCode = ReadString(reader, "TrayCode"),
                        .ChannelNo = ReadInt(reader, "ChannelNo"),
                        .OriginalVoltage = ReadSingle(reader, "OriginalVoltage"),
                        .OriginalResistance = ReadSingle(reader, "OriginalResistance"),
                        .OriginalResult = ReadString(reader, "OriginalResult"),
                        .OriginalTestTime = ReadDate(reader, "OriginalTestTime")
                    }
                    source.TrayPosition = ReadString(reader, "TrayPosition")
                    If source.TrayPosition = String.Empty Then source.TrayPosition = BuildTrayPosition(source.ChannelNo)
                    If Not String.Equals(source.OriginalResult, "NG", StringComparison.OrdinalIgnoreCase) Then
                        Throw New InvalidOperationException("该电池原始结果不是NG，不能复测。")
                    End If
                    Return source
                End Using
            End Using
        End Using
    End Function

    Public Sub SaveRetest(ByVal request As NgRetestSaveRequest)
        If request Is Nothing OrElse request.Source Is Nothing OrElse request.Measurement Is Nothing Then
            Throw New ArgumentNullException(NameOf(request))
        End If
        If request.RetestOperationId = Guid.Empty Then
            Throw New ArgumentException("复测保存请求缺少操作 ID。", NameOf(request))
        End If

        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As New SqlCommand("dbo.usp_SaveNgRetest", connection)
                command.CommandType = CommandType.StoredProcedure
                command.CommandTimeout = 10
                command.Parameters.Add("@BatteryCode", SqlDbType.NVarChar, 100).Value = request.Source.BatteryCode
                command.Parameters.Add("@TrayCode", SqlDbType.NVarChar, 100).Value = request.Source.TrayCode
                command.Parameters.Add("@ChannelNo", SqlDbType.SmallInt).Value = request.Source.ChannelNo
                command.Parameters.Add("@TrayPosition", SqlDbType.NVarChar, 20).Value = request.Source.TrayPosition
                command.Parameters.Add("@OriginalVoltage", SqlDbType.Real).Value = request.Source.OriginalVoltage
                command.Parameters.Add("@OriginalResistance", SqlDbType.Real).Value = request.Source.OriginalResistance
                command.Parameters.Add("@OriginalResult", SqlDbType.NVarChar, 10).Value = request.Source.OriginalResult
                command.Parameters.Add("@RetestVoltage", SqlDbType.Real).Value = request.Measurement.Voltage
                command.Parameters.Add("@RetestResistance", SqlDbType.Real).Value = CSng(request.Measurement.FinalResistanceMilliohm)
                AddDecimalParameter(command, "@RetestRawResistanceMilliohm", request.Measurement.RawResistanceMilliohm)
                AddDecimalParameter(command, "@CalibrationValueMilliohm", request.Measurement.CalibrationValueMilliohm)
                AddDecimalParameter(command, "@RetestFinalResistanceMilliohm", request.Measurement.FinalResistanceMilliohm)
                command.Parameters.Add("@RetestResult", SqlDbType.NVarChar, 10).Value = request.Measurement.Result
                command.Parameters.Add("@RetestTime", SqlDbType.DateTime2).Value = request.Measurement.MeasuredAt
                command.Parameters.Add("@OverwriteOriginal", SqlDbType.Bit).Value = request.OverwriteOriginal
                command.Parameters.Add("@OperatorName", SqlDbType.NVarChar, 100).Value = If(request.OperatorName, String.Empty)
                command.Parameters.Add("@MeterPort", SqlDbType.NVarChar, 20).Value = If(request.MeterPort, String.Empty)
                command.Parameters.Add("@SourceTestResultId", SqlDbType.BigInt).Value = request.Source.SourceTestResultId
                command.Parameters.Add("@RetestOperationId", SqlDbType.UniqueIdentifier).Value = request.RetestOperationId
                command.Parameters.Add("@CalibrationSnapshotFingerprint", SqlDbType.Char, 64).Value =
                    If(String.IsNullOrWhiteSpace(request.Measurement.CalibrationSnapshotFingerprint), CType(DBNull.Value, Object), request.Measurement.CalibrationSnapshotFingerprint)
                command.Parameters.Add("@CalibrationSnapshotSource", SqlDbType.NVarChar, 200).Value =
                    If(String.IsNullOrWhiteSpace(request.Measurement.CalibrationSnapshotSource), CType(DBNull.Value, Object), request.Measurement.CalibrationSnapshotSource)
                command.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Private Shared Sub AddDecimalParameter(ByVal command As SqlCommand, ByVal name As String, ByVal value As Double)
        Dim parameter = command.Parameters.Add(name, SqlDbType.Decimal)
        parameter.Precision = 18
        parameter.Scale = 6
        parameter.Value = Convert.ToDecimal(value)
    End Sub

    Private Shared Function BuildTrayPosition(ByVal channelNo As Integer) As String
        If channelNo <= 0 Then Return String.Empty
        Dim row As Integer = ((channelNo - 1) \ 16) + 1
        Dim col As Integer = ((channelNo - 1) Mod 16) + 1
        Return row.ToString() & "-" & col.ToString()
    End Function

    Private Shared Function ReadString(ByVal reader As SqlDataReader, ByVal name As String) As String
        Return If(reader(name) Is DBNull.Value, String.Empty, Convert.ToString(reader(name)).Trim())
    End Function

    Private Shared Function ReadInt(ByVal reader As SqlDataReader, ByVal name As String) As Integer
        Return If(reader(name) Is DBNull.Value, 0, Convert.ToInt32(reader(name)))
    End Function

    Private Shared Function ReadSingle(ByVal reader As SqlDataReader, ByVal name As String) As Single
        Return If(reader(name) Is DBNull.Value, 0.0F, Convert.ToSingle(reader(name)))
    End Function

    Private Shared Function ReadDate(ByVal reader As SqlDataReader, ByVal name As String) As DateTime
        Return If(reader(name) Is DBNull.Value, DateTime.MinValue, Convert.ToDateTime(reader(name)))
    End Function
End Class


