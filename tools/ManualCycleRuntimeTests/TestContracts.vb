Imports System.Collections.Generic

Public Interface IPlcAutomationClient
    ReadOnly Property IsOpen As Boolean
    Sub Open()
    Sub Close()
    Function ReadR150() As Boolean
    Function ReadR151() As Boolean
    Function ReadR152() As Boolean
    Function ReadX1F() As Boolean
    Sub WriteR160()
    Sub WriteR161()
    Sub WriteR162()
    Sub WriteNgSelection(ByVal channel As Integer, Optional ByVal responseReceived As System.Action(Of String) = Nothing)
End Interface

Public Interface ITrayScannerClient
    ReadOnly Property IsOpen As Boolean
    Sub Open()
    Sub Close()
    Sub TriggerScan()
    Function ReadTrayCode(ByVal timeoutMs As Integer) As String
End Interface
