Imports System.Globalization
Imports System.IO.Ports
Imports System.Text

Public Class Retest3562Service
    Implements IDisposable

    Private ReadOnly syncRoot As New Object()
    Private meter As SerialPort
    Private disposed As Boolean

    Public ReadOnly Property IsConnected As Boolean
        Get
            SyncLock syncRoot
                Return meter IsNot Nothing AndAlso meter.IsOpen
            End SyncLock
        End Get
    End Property

    Public ReadOnly Property PortName As String
        Get
            SyncLock syncRoot
                Return If(meter Is Nothing, String.Empty, meter.PortName)
            End SyncLock
        End Get
    End Property

    Public Sub Connect(ByVal portName As String, ByVal baudRate As Integer)
        If String.IsNullOrWhiteSpace(portName) Then Throw New ArgumentException("复测3562串口不能为空。", NameOf(portName))
        If baudRate <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(baudRate))

        SyncLock syncRoot
            ThrowIfDisposed()
            DisconnectInternal()
            meter = New SerialPort(portName.Trim(), baudRate, Parity.None, 8, StopBits.One) With {
                .ReadTimeout = 3000,
                .WriteTimeout = 3000,
                .NewLine = vbLf,
                .Encoding = Encoding.ASCII
            }
            meter.Open()
        End SyncLock
    End Sub

    Public Function Measure() As RetestMeasurement
        SyncLock syncRoot
            ThrowIfDisposed()
            If meter Is Nothing OrElse Not meter.IsOpen Then Throw New InvalidOperationException("复测3562未连接。")

            meter.DiscardInBuffer()
            meter.DiscardOutBuffer()
            meter.Write(":FETC?" & vbLf)
            Dim response As String = meter.ReadLine()
            Dim fields() As String = response.Trim().Split(","c)
            If fields.Length < 2 Then Throw New FormatException("复测3562返回数据字段不足：" & response)

            Dim resistanceOhm As Double
            Dim voltage As Double
            If Not Double.TryParse(fields(0).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, resistanceOhm) Then
                Throw New FormatException("复测3562内阻数据无效：" & fields(0))
            End If
            If Not Double.TryParse(fields(1).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, voltage) Then
                Throw New FormatException("复测3562电压数据无效：" & fields(1))
            End If

            Dim result As New RetestMeasurement() With {
                .Resistance = CSng(resistanceOhm * 1000.0R),
                .Voltage = CSng(voltage),
                .MeasuredAt = DateTime.Now
            }
            result.Result = If(IsNG(result.Voltage, result.Resistance) = 0, "OK", "NG")
            Return result
        End SyncLock
    End Function

    Public Sub Disconnect()
        SyncLock syncRoot
            DisconnectInternal()
        End SyncLock
    End Sub

    Private Sub DisconnectInternal()
        If meter Is Nothing Then Return
        Try
            If meter.IsOpen Then meter.Close()
        Finally
            meter.Dispose()
            meter = Nothing
        End Try
    End Sub

    Private Sub ThrowIfDisposed()
        If disposed Then Throw New ObjectDisposedException(NameOf(Retest3562Service))
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        SyncLock syncRoot
            If disposed Then Return
            DisconnectInternal()
            disposed = True
        End SyncLock
    End Sub
End Class


