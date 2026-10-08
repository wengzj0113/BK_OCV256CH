Imports System.IO.Ports
Imports System.Net.Sockets
Imports Modbus.Device

Public Interface IPlcController
    Function Connect() As Boolean
    Function ReadTrayStatus() As Boolean
    Function CloseTray() As Boolean
    Function OpenTray() As Boolean
    Function ResetTray() As Boolean
    Sub Disconnect()
End Interface

Friend NotInheritable Class PlcControllerFactory
    Private Sub New()
    End Sub

    Friend Shared Function Create(settings As Initset) As IPlcController
        If settings.PLCMode = 1 Then
            Return New ModbusTcpPlcController(settings)
        End If
        Return New ModbusRtuPlcController(settings)
    End Function
End Class

Friend MustInherit Class ModbusPlcControllerBase
    Implements IPlcController

    Protected Const DefaultSlaveId As Byte = 1
    Protected Const TrayStatusAddress As UShort = 0
    Protected Const TrayCloseCommandAddress As UShort = 0
    Protected Const TrayOpenCommandAddress As UShort = 1
    Protected Const TrayResetCommandAddress As UShort = 2

    Protected ReadOnly settings As Initset
    Protected slaveId As Byte = DefaultSlaveId

    Protected Sub New(settings As Initset)
        Me.settings = settings
    End Sub

    Public MustOverride Function Connect() As Boolean Implements IPlcController.Connect
    Public MustOverride Function ReadTrayStatus() As Boolean Implements IPlcController.ReadTrayStatus
    Public MustOverride Function CloseTray() As Boolean Implements IPlcController.CloseTray
    Public MustOverride Function OpenTray() As Boolean Implements IPlcController.OpenTray
    Public MustOverride Function ResetTray() As Boolean Implements IPlcController.ResetTray
    Public MustOverride Sub Disconnect() Implements IPlcController.Disconnect

    Protected Shared Function ToResult(values As Boolean()) As Boolean
        Return values IsNot Nothing AndAlso values.Length > 0 AndAlso values(0)
    End Function
End Class

Friend Class ModbusRtuPlcController
    Inherits ModbusPlcControllerBase

    Private serialPort As SerialPort
    Private master As IModbusSerialMaster

    Public Sub New(settings As Initset)
        MyBase.New(settings)
    End Sub

    Public Overrides Function Connect() As Boolean
        Try
            If serialPort Is Nothing Then serialPort = New SerialPort()
            If serialPort.IsOpen Then serialPort.Close()
            serialPort.PortName = "COM" & (settings.PLCComPort + 1).ToString()
            serialPort.BaudRate = DefaultPlcBaudRate
            serialPort.Parity = Parity.Odd
            serialPort.DataBits = 8
            serialPort.StopBits = StopBits.One
            serialPort.ReadTimeout = 1000
            serialPort.WriteTimeout = 1000
            serialPort.Open()
            master = ModbusSerialMaster.CreateRtu(serialPort)
            master.Transport.ReadTimeout = 1000
            master.Transport.WriteTimeout = 1000
            Return True
        Catch ex As Exception
            Disconnect()
            Return False
        End Try
    End Function

    Public Overrides Function ReadTrayStatus() As Boolean
        EnsureConnected()
        Return ToResult(master.ReadCoils(slaveId, TrayStatusAddress, 1))
    End Function

    Public Overrides Function CloseTray() As Boolean
        EnsureConnected()
        master.WriteSingleCoil(slaveId, TrayCloseCommandAddress, True)
        Return WaitForTrayState(True)
    End Function

    Public Overrides Function OpenTray() As Boolean
        EnsureConnected()
        master.WriteSingleCoil(slaveId, TrayOpenCommandAddress, True)
        Return WaitForTrayState(False)
    End Function

    Public Overrides Function ResetTray() As Boolean
        EnsureConnected()
        master.WriteSingleCoil(slaveId, TrayResetCommandAddress, True)
        Return True
    End Function

    Public Overrides Sub Disconnect()
        Try
            If serialPort IsNot Nothing AndAlso serialPort.IsOpen Then serialPort.Close()
        Catch
        End Try
        master = Nothing
    End Sub

    Private Sub EnsureConnected()
        If master Is Nothing OrElse serialPort Is Nothing OrElse Not serialPort.IsOpen Then
            Throw New InvalidOperationException("PLC Modbus RTU 未连接")
        End If
    End Sub

    Private Function WaitForTrayState(expectedClosed As Boolean) As Boolean
        Dim deadline As DateTime = DateTime.UtcNow.AddSeconds(2)
        Do
            If ReadTrayStatus() = expectedClosed Then Return True
            Threading.Thread.Sleep(50)
        Loop While DateTime.UtcNow < deadline
        Return False
    End Function
End Class

Friend Class ModbusTcpPlcController
    Inherits ModbusPlcControllerBase

    Private tcpClient As TcpClient
    Private master As IModbusMaster

    Public Sub New(settings As Initset)
        MyBase.New(settings)
    End Sub

    Public Overrides Function Connect() As Boolean
        Try
            Disconnect()
            If String.IsNullOrWhiteSpace(settings.PLCIP) OrElse settings.PLCTcpPort <= 0 Then Return False
            tcpClient = New TcpClient()
            Dim connectTask = tcpClient.ConnectAsync(settings.PLCIP.Trim(), settings.PLCTcpPort)
            If Not connectTask.Wait(2000) OrElse Not tcpClient.Connected Then
                Disconnect()
                Return False
            End If
            master = ModbusIpMaster.CreateIp(tcpClient)
            master.Transport.ReadTimeout = 1000
            master.Transport.WriteTimeout = 1000
            Return True
        Catch
            Disconnect()
            Return False
        End Try
    End Function

    Public Overrides Function ReadTrayStatus() As Boolean
        EnsureConnected()
        Return ToResult(master.ReadCoils(slaveId, TrayStatusAddress, 1))
    End Function

    Public Overrides Function CloseTray() As Boolean
        EnsureConnected()
        master.WriteSingleCoil(slaveId, TrayCloseCommandAddress, True)
        Return WaitForTrayState(True)
    End Function

    Public Overrides Function OpenTray() As Boolean
        EnsureConnected()
        master.WriteSingleCoil(slaveId, TrayOpenCommandAddress, True)
        Return WaitForTrayState(False)
    End Function

    Public Overrides Function ResetTray() As Boolean
        EnsureConnected()
        master.WriteSingleCoil(slaveId, TrayResetCommandAddress, True)
        Return True
    End Function

    Public Overrides Sub Disconnect()
        Try
            If tcpClient IsNot Nothing Then tcpClient.Close()
        Catch
        End Try
        master = Nothing
        tcpClient = Nothing
    End Sub

    Private Sub EnsureConnected()
        If master Is Nothing OrElse tcpClient Is Nothing OrElse Not tcpClient.Connected Then
            Throw New InvalidOperationException("PLC Modbus TCP 未连接")
        End If
    End Sub

    Private Function WaitForTrayState(expectedClosed As Boolean) As Boolean
        Dim deadline As DateTime = DateTime.UtcNow.AddSeconds(2)
        Do
            If ReadTrayStatus() = expectedClosed Then Return True
            Threading.Thread.Sleep(50)
        Loop While DateTime.UtcNow < deadline
        Return False
    End Function
End Class
