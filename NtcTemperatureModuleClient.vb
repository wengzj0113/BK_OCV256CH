Imports System.IO.Ports
Imports Modbus.Device

''' <summary>
''' Reads the four NTC temperature channels through the configured Modbus RTU port.
''' </summary>
Public NotInheritable Class NtcTemperatureModuleClient
    Implements IDisposable

    Private Const SlaveAddress As Byte = 1
    Private Const FirstTemperatureRegister As UShort = 0
    Private Const TemperatureRegisterCount As UShort = 2

    Private ReadOnly serialPort As SerialPort
    Private ReadOnly master As IModbusSerialMaster
    Private ReadOnly syncRoot As New Object()

    Public Sub New(portIndex As Integer)
        If portIndex < 0 OrElse portIndex > 63 Then
            Throw New ArgumentOutOfRangeException(NameOf(portIndex), "温度采集模块串口号配置无效。")
        End If
        serialPort = New SerialPort("COM" & (portIndex + 1).ToString(),
                                    DefaultTemperatureBaudRate,
                                    Parity.None,
                                    8,
                                    StopBits.One) With {
                                        .ReadTimeout = 1000,
                                        .WriteTimeout = 1000
                                    }
        serialPort.Open()
        master = ModbusSerialMaster.CreateRtu(serialPort)
        master.Transport.ReadTimeout = 1000
        master.Transport.WriteTimeout = 1000
    End Sub

    Public Function ReadTemperatures() As NtcTemperatureReading()
        SyncLock syncRoot
            Dim registers As UShort() = master.ReadInputRegisters(SlaveAddress,
                                                                 FirstTemperatureRegister,
                                                                 TemperatureRegisterCount)
            If registers Is Nothing OrElse registers.Length <> TemperatureRegisterCount Then
                Throw New InvalidOperationException("温度模块返回的寄存器数量不是4。")
            End If

            Dim readings(CInt(TemperatureRegisterCount) - 1) As NtcTemperatureReading
            For index As Integer = 0 To readings.Length - 1
                readings(index) = DecodeRegister(registers(index))
            Next
            Return readings
        End SyncLock
    End Function

    Public Shared Function DecodeRegister(registerValue As UShort) As NtcTemperatureReading
        If registerValue = UShort.MaxValue Then
            Return New NtcTemperatureReading With {.IsValid = False}
        End If

        Dim magnitude As Integer = registerValue And &H7FFFUS
        Dim temperature As Decimal = CDec(magnitude) / 10D
        If (registerValue And &H8000US) <> 0US Then temperature = -temperature
        Return New NtcTemperatureReading With {.IsValid = True, .TemperatureCelsius = temperature}
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        SyncLock syncRoot
            Try
                If serialPort IsNot Nothing Then
                    If serialPort.IsOpen Then serialPort.Close()
                    serialPort.Dispose()
                End If
            Catch
            End Try
        End SyncLock
    End Sub
End Class

Public Structure NtcTemperatureReading
    Public Property IsValid As Boolean
    Public Property TemperatureCelsius As Decimal
End Structure
