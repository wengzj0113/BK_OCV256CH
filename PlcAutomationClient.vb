Imports System.IO.Ports
Imports System.Text
Imports System.Threading

Public Interface IPlcAutomationClient
    ReadOnly Property IsOpen As Boolean
    Sub Open()
    Sub Close()
    Function ReadR150() As Boolean
    Function ReadR151() As Boolean
    Function ReadR152() As Boolean
    Sub WriteR160()
    Sub WriteR161()
    Sub WriteR162()
    Sub WriteNgSelection(ByVal ngChannelNumber As Integer)
End Interface

Friend Class PlcAutomationClient
    Implements IPlcAutomationClient, IDisposable

    Private ReadOnly serialPortPLC As SerialPort
    Private ReadOnly syncRoot As New Object()
    Private ReadOnly ownsPort As Boolean
    Private disposed As Boolean

    Public Sub New(ByVal settings As Initset, Optional ByVal sharedPort As SerialPort = Nothing)
        If sharedPort Is Nothing Then
            serialPortPLC = New SerialPort()
            ownsPort = True
        Else
            serialPortPLC = sharedPort
            ownsPort = False
        End If

        Configure(settings)
    End Sub

    Public ReadOnly Property IsOpen As Boolean Implements IPlcAutomationClient.IsOpen
        Get
            Return serialPortPLC IsNot Nothing AndAlso serialPortPLC.IsOpen
        End Get
    End Property

    Public Sub Open() Implements IPlcAutomationClient.Open
        ThrowIfDisposed()
        SyncLock syncRoot
            ConfigurePortIfNeeded()
            If Not serialPortPLC.IsOpen Then serialPortPLC.Open()
        End SyncLock
    End Sub

    Public Sub Close() Implements IPlcAutomationClient.Close
        SyncLock syncRoot
            If serialPortPLC IsNot Nothing AndAlso serialPortPLC.IsOpen Then serialPortPLC.Close()
        End SyncLock
    End Sub

    Public Function ReadR150() As Boolean Implements IPlcAutomationClient.ReadR150
        Return PlcAutomationProtocol.ParseSignal(SendAndReceive(PlcAutomationProtocol.WithCr(PlcAutomationProtocol.ReadR150Frame)))
    End Function

    Public Function ReadR151() As Boolean Implements IPlcAutomationClient.ReadR151
        Return PlcAutomationProtocol.ParseSignal(SendAndReceive(PlcAutomationProtocol.WithCr(PlcAutomationProtocol.ReadR151Frame)))
    End Function

    Public Function ReadR152() As Boolean Implements IPlcAutomationClient.ReadR152
        Return PlcAutomationProtocol.ParseSignal(SendAndReceive(PlcAutomationProtocol.WithCr(PlcAutomationProtocol.ReadR152Frame)))
    End Function

    Public Sub WriteR160() Implements IPlcAutomationClient.WriteR160
        WriteAndRequireAck(PlcAutomationProtocol.WithCr(PlcAutomationProtocol.WriteR160Frame), "WC")
    End Sub

    Public Sub WriteR161() Implements IPlcAutomationClient.WriteR161
        WriteAndRequireAck(PlcAutomationProtocol.WithCr(PlcAutomationProtocol.WriteR161Frame), "WC")
    End Sub

    Public Sub WriteR162() Implements IPlcAutomationClient.WriteR162
        WriteAndRequireAck(PlcAutomationProtocol.WithCr(PlcAutomationProtocol.WriteR162Frame), "WC")
    End Sub

    Public Sub WriteNgSelection(ByVal ngChannelNumber As Integer) Implements IPlcAutomationClient.WriteNgSelection
        WriteAndRequireAck(PlcAutomationProtocol.BuildNgSelectionFrame(ngChannelNumber), "WD")
    End Sub

    Private Sub WriteAndRequireAck(ByVal frame As String, ByVal expectedCommand As String)
        Dim response As String = SendAndReceive(frame)
        If Not PlcAutomationProtocol.IsWriteAcknowledged(response, expectedCommand) Then
            Throw New PlcProtocolException("PLC写入未收到有效响应。")
        End If
    End Sub

    Private Function SendAndReceive(ByVal frame As String) As String
        ThrowIfDisposed()
        If String.IsNullOrWhiteSpace(frame) Then Throw New ArgumentException("PLC报文不能为空。", NameOf(frame))

        SyncLock syncRoot
            If Not serialPortPLC.IsOpen Then Throw New InvalidOperationException("PLC串口未打开。")
            serialPortPLC.DiscardInBuffer()
            serialPortPLC.DiscardOutBuffer()
            serialPortPLC.Write(frame)

            Dim builder As New StringBuilder()
            Dim deadline As DateTime = DateTime.UtcNow.AddMilliseconds(Math.Max(100, serialPortPLC.ReadTimeout))
            Dim quietSince As DateTime = DateTime.UtcNow
            Do
                If serialPortPLC.BytesToRead > 0 Then
                    builder.Append(serialPortPLC.ReadExisting())
                    quietSince = DateTime.UtcNow
                ElseIf builder.Length > 0 AndAlso (DateTime.UtcNow - quietSince).TotalMilliseconds >= 50 Then
                    Exit Do
                Else
                    Thread.Sleep(5)
                End If
            Loop While DateTime.UtcNow < deadline

            If builder.Length = 0 Then Throw New TimeoutException("PLC响应超时。")
            Return builder.ToString()
        End SyncLock
    End Function

    Private Sub Configure(ByVal settings As Initset)
        If settings.PLCMode = 1 Then
            Throw New InvalidOperationException("PLC自动ASCII流程不支持TCP模式。")
        End If

        serialPortPLC.PortName = "COM" & (settings.PLCComPort + 1).ToString()
        serialPortPLC.BaudRate = DefaultPlcBaudRate
        serialPortPLC.Parity = Parity.Odd
        serialPortPLC.DataBits = 8
        serialPortPLC.StopBits = StopBits.One
        serialPortPLC.NewLine = ControlChars.Cr
        serialPortPLC.ReadTimeout = 1000
        serialPortPLC.WriteTimeout = 1000
    End Sub

    Private Sub ConfigurePortIfNeeded()
        If String.IsNullOrWhiteSpace(serialPortPLC.PortName) Then
            Throw New InvalidOperationException("PLC串口号未配置。")
        End If
    End Sub

    Private Sub ThrowIfDisposed()
        If disposed Then Throw New ObjectDisposedException(NameOf(PlcAutomationClient))
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If disposed Then Return
        disposed = True
        Close()
        If ownsPort Then serialPortPLC.Dispose()
    End Sub
End Class
