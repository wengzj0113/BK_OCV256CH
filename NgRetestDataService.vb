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
                    If Not reader.Read() Then Throw New InvalidOperationException("未找到该电池的NG历史记录。")
                    Dim source As New NgRetestSource() With {
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
                command.Parameters.Add("@RetestResistance", SqlDbType.Real).Value = request.Measurement.Resistance
                command.Parameters.Add("@RetestResult", SqlDbType.NVarChar, 10).Value = request.Measurement.Result
                command.Parameters.Add("@RetestTime", SqlDbType.DateTime2).Value = request.Measurement.MeasuredAt
                command.Parameters.Add("@OverwriteOriginal", SqlDbType.Bit).Value = request.OverwriteOriginal
                command.Parameters.Add("@OperatorName", SqlDbType.NVarChar, 100).Value = If(request.OperatorName, String.Empty)
                command.Parameters.Add("@MeterPort", SqlDbType.NVarChar, 20).Value = If(request.MeterPort, String.Empty)
                command.ExecuteNonQuery()
            End Using
        End Using
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


