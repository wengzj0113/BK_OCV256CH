Imports System.Globalization
Imports System.Linq
Imports System.Security.Cryptography
Imports System.Text

Public Enum CalibrationValueSource
    Database
    LocalCache
    Uncalibrated
End Enum

Public NotInheritable Class CalibratedResistanceResult
    Public Sub New(rawResistanceMilliohm As Double,
                   compensationMilliohm As Double,
                   finalResistanceMilliohm As Double,
                   deviceChannel As Integer)
        Me.RawResistanceMilliohm = rawResistanceMilliohm
        Me.CompensationMilliohm = compensationMilliohm
        Me.FinalResistanceMilliohm = finalResistanceMilliohm
        Me.DeviceChannel = deviceChannel
    End Sub

    Public ReadOnly Property RawResistanceMilliohm As Double
    Public ReadOnly Property CompensationMilliohm As Double
    Public ReadOnly Property FinalResistanceMilliohm As Double
    Public ReadOnly Property DeviceChannel As Integer
End Class

Public NotInheritable Class CalibrationSnapshot
    Private ReadOnly compensationByDeviceChannel() As Double
    Private ReadOnly sourceByDeviceChannel() As CalibrationValueSource

    Friend Sub New(values As Double(),
                   sources As CalibrationValueSource(),
                   sourceDescription As String,
                   cachePath As String,
                   databaseAvailable As Boolean)
        If values Is Nothing OrElse values.Length <> CalibrationService.ChannelCount Then
            Throw New ArgumentException("校准快照必须正好包含256个通道。", NameOf(values))
        End If
        If sources Is Nothing OrElse sources.Length <> CalibrationService.ChannelCount Then
            Throw New ArgumentException("校准快照必须为256个通道标记来源。", NameOf(sources))
        End If

        compensationByDeviceChannel = CType(values.Clone(), Double())
        sourceByDeviceChannel = CType(sources.Clone(), CalibrationValueSource())
        For i As Integer = 0 To compensationByDeviceChannel.Length - 1
            Dim value As Double = compensationByDeviceChannel(i)
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) OrElse
               Math.Abs(value) > CalibrationService.MaxAbsCalibrationValue Then
                Throw New ArgumentOutOfRangeException(NameOf(values), "校准补偿值超出有效范围。")
            End If
        Next

        Me.SourceDescription = If(sourceDescription, String.Empty)
        Me.CachePath = If(cachePath, String.Empty)
        Me.DatabaseAvailable = databaseAvailable
        Me.LoadedAt = DateTime.Now
        Me.Fingerprint = ComputeFingerprint(compensationByDeviceChannel)
        Me.UncalibratedChannelCount = sourceByDeviceChannel.Count(Function(source) source = CalibrationValueSource.Uncalibrated)
    End Sub

    Public ReadOnly Property SourceDescription As String
    Public ReadOnly Property CachePath As String
    Public ReadOnly Property DatabaseAvailable As Boolean
    Public ReadOnly Property LoadedAt As DateTime
    Public ReadOnly Property Fingerprint As String
    Public ReadOnly Property UncalibratedChannelCount As Integer

    Public Function GetCompensationByDeviceChannel(deviceChannel As Integer) As Double
        ValidateDeviceChannel(deviceChannel)
        Return compensationByDeviceChannel(deviceChannel - 1)
    End Function

    Public Function GetCompensationByStandardChannel(standardChannel As Integer) As Double
        Return GetCompensationByDeviceChannel(TrayChannelMapping.ToDeviceChannel(standardChannel))
    End Function

    Public Function GetSourceByDeviceChannel(deviceChannel As Integer) As CalibrationValueSource
        ValidateDeviceChannel(deviceChannel)
        Return sourceByDeviceChannel(deviceChannel - 1)
    End Function

    Public Function GetCompensationByIndex0(channelIndex0 As Integer) As Double
        If channelIndex0 < 0 OrElse channelIndex0 >= CalibrationService.ChannelCount Then
            Throw New ArgumentOutOfRangeException(NameOf(channelIndex0))
        End If
        Return compensationByDeviceChannel(channelIndex0)
    End Function

    Public Function CopyValues() As Double()
        Return CType(compensationByDeviceChannel.Clone(), Double())
    End Function

    Public Function ApplyResistance(rawResistanceMilliohm As Double,
                                    deviceChannel As Integer) As CalibratedResistanceResult
        If Double.IsNaN(rawResistanceMilliohm) OrElse Double.IsInfinity(rawResistanceMilliohm) Then
            Throw New ArgumentOutOfRangeException(NameOf(rawResistanceMilliohm))
        End If
        Dim compensation As Double = GetCompensationByDeviceChannel(deviceChannel)
        Return New CalibratedResistanceResult(rawResistanceMilliohm,
                                              compensation,
                                              rawResistanceMilliohm + compensation,
                                              deviceChannel)
    End Function

    Private Shared Sub ValidateDeviceChannel(deviceChannel As Integer)
        If deviceChannel < 1 OrElse deviceChannel > CalibrationService.ChannelCount Then
            Throw New ArgumentOutOfRangeException(NameOf(deviceChannel), "设备通道号必须在1到256之间。")
        End If
    End Sub

    Private Shared Function ComputeFingerprint(values As Double()) As String
        Dim builder As New StringBuilder()
        For Each value As Double In values
            builder.Append(value.ToString("R", CultureInfo.InvariantCulture)).Append(";"c)
        Next

        Using sha As SHA256 = SHA256.Create()
            Dim hash As Byte() = sha.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()))
            Return BitConverter.ToString(hash).Replace("-", String.Empty)
        End Using
    End Function
End Class
