Imports System.IO.Ports
Imports Modbus.Device

''' <summary>
''' Reads the first two wired NTC temperature channels through the configured Modbus RTU port.
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
                Throw New InvalidOperationException("温度模块返回的寄存器数量不是2。")
            End If

            EnsureCelsiusUnit()

            Dim readings(CInt(TemperatureRegisterCount) - 1) As NtcTemperatureReading
            For index As Integer = 0 To readings.Length - 1
                readings(index) = DecodeRegister(registers(index))
            Next
            Return readings
        End SyncLock
    End Function

    Public Shared Function DecodeRegister(registerValue As UShort) As NtcTemperatureReading
        If registerValue = &H7FFFUS Then
            Return New NtcTemperatureReading With {.IsValid = False}
        End If

        Dim signedValue As Integer = CInt(registerValue)
        If signedValue >= &H8000 Then signedValue -= &H10000
        Dim temperature As Decimal = CDec(signedValue) / 10D
        Return New NtcTemperatureReading With {.IsValid = True, .TemperatureCelsius = temperature}
    End Function

    Private Sub EnsureCelsiusUnit()
        Dim unitRegisters As UShort() = master.ReadHoldingRegisters(SlaveAddress, &H85US, 1US)
        If unitRegisters Is Nothing OrElse unitRegisters.Length <> 1 Then
            Throw New InvalidOperationException("温度模块未返回温度单位设置。")
        End If

        ValidateTemperatureUnit(unitRegisters(0))
    End Sub

    Friend Shared Sub ValidateTemperatureUnit(unitValue As UShort)
        If unitValue <> 0US Then Throw New NtcTemperatureUnitException(unitValue)
    End Sub

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

Public NotInheritable Class NtcTemperatureUnitException
    Inherits InvalidOperationException

    Public Sub New(unitValue As UShort)
        MyBase.New(If(unitValue = 1US,
                      "温度模块通信正常，但当前单位为华氏度。请通过模块配置软件将温度单位改为摄氏度后重试。",
                      "温度模块通信正常，但返回了未知温度单位设置（" & unitValue.ToString() & "）。请检查模块配置。"))
        Me.UnitValue = unitValue
    End Sub

    Public ReadOnly Property UnitValue As UShort
End Class

Public Structure NtcTemperatureReading
    Public Property IsValid As Boolean
    Public Property TemperatureCelsius As Decimal
End Structure
