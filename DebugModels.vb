Imports System

Public Enum DebugDeviceId
    ControlBoard
    MiddleMachine
    Hioki3562
    Keysight34461
    PlcTray
    IoCard
    Scanner
    Retest3562
    Database
    Mes
End Enum

Public Enum DebugDeviceState
    Pending
    Testing
    Connected
    Failed
    Managed
    Unavailable
End Enum

Public Class DebugDeviceResult
    Public Property DeviceId As DebugDeviceId
    Public Property State As DebugDeviceState
    Public Property Message As String
    Public Property InterfaceDescription As String
    Public Property CheckedAt As DateTime
    Public Property ElapsedMilliseconds As Long

    Public Shared Function PendingResult(deviceId As DebugDeviceId, interfaceDescription As String) As DebugDeviceResult
        Return New DebugDeviceResult With {
            .DeviceId = deviceId,
            .State = DebugDeviceState.Pending,
            .Message = "待检查",
            .InterfaceDescription = interfaceDescription,
            .CheckedAt = DateTime.MinValue,
            .ElapsedMilliseconds = 0
        }
    End Function
End Class

Public Class DebugLogEntry
    Public Property Timestamp As DateTime
    Public Property Level As String
    Public Property Message As String
End Class

Public Class DebugDeviceEventArgs
    Inherits EventArgs

    Public Sub New(deviceId As DebugDeviceId)
        Me.DeviceId = deviceId
    End Sub

    Public ReadOnly Property DeviceId As DebugDeviceId
End Class

Public Class DebugCommunicationRequestEventArgs
    Inherits EventArgs

    Public Sub New(interfaceName As String, payload As Byte(), displayMode As String)
        Me.InterfaceName = interfaceName
        Me.Payload = If(payload, New Byte() {})
        Me.DisplayMode = displayMode
    End Sub

    Public ReadOnly Property InterfaceName As String
    Public ReadOnly Property Payload As Byte()
    Public ReadOnly Property DisplayMode As String
End Class

Public Class DebugManualActionEventArgs
    Inherits EventArgs

    Public Sub New(actionName As String, value As Integer)
        Me.ActionName = actionName
        Me.Value = value
    End Sub

    Public ReadOnly Property ActionName As String
    Public ReadOnly Property Value As Integer
End Class
