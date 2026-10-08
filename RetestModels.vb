Imports System

Public Class NgRetestSource
    Public Property BatteryCode As String
    Public Property TrayCode As String
    Public Property ChannelNo As Integer
    Public Property TrayPosition As String
    Public Property OriginalVoltage As Single
    Public Property OriginalResistance As Single
    Public Property OriginalResult As String
    Public Property OriginalTestTime As DateTime
End Class
Public Class RetestMeasurement
    Public Property Voltage As Single
    Public Property VoltageMeasured As Boolean
    Public Property Resistance As Single
    Public Property Result As String
    Public Property MeasuredAt As DateTime
End Class

Public Class NgRetestSaveRequest
    Public Property Source As NgRetestSource
    Public Property Measurement As RetestMeasurement
    Public Property OverwriteOriginal As Boolean
    Public Property OperatorName As String
    Public Property MeterPort As String
End Class

