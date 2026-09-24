Imports System.IO.Ports
Imports System.Text
Imports System.Threading

Public Interface ITrayScannerClient
    ReadOnly Property IsOpen As Boolean
    Sub Open()
    Sub Close()
    Function ReadTrayCode(ByVal timeoutMs As Integer) As String
End Interface

Friend Class TrayScannerClient
    Implements ITrayScannerClient, IDisposable

    Private ReadOnly serialPortTPM As SerialPort
    Private ReadOnly syncRoot As New Object()
    Private ReadOnly ownsPort As Boolean
    Private disposed As Boolean

    Public Sub New(ByVal settings As Initset, Optional ByVal sharedPort As SerialPort = Nothing)
        If sharedPort Is Nothing Then
            serialPortTPM = New SerialPort()
            ownsPort = True
        Else
            serialPortTPM = sharedPort
            ownsPort = False
        End If

        serialPortTPM.PortName = "COM" & (settings.ScanerPort + 1).ToString()
        serialPortTPM.BaudRate = If(settings.ScanerRate > 0, settings.ScanerRate, DefaultScannerBaudRate)
        serialPortTPM.Parity = Parity.None
        serialPortTPM.DataBits = 8
        serialPortTPM.StopBits = StopBits.One
        serialPortTPM.NewLine = ControlChars.Cr
        serialPortTPM.ReadTimeout = 500
        serialPortTPM.WriteTimeout = 500
    End Sub

    Public ReadOnly Property IsOpen As Boolean Implements ITrayScannerClient.IsOpen
        Get
            Return serialPortTPM IsNot Nothing AndAlso serialPortTPM.IsOpen
        End Get
    End Property

    Public Sub Open() Implements ITrayScannerClient.Open
        ThrowIfDisposed()
        SyncLock syncRoot
            If Not serialPortTPM.IsOpen Then serialPortTPM.Open()
        End SyncLock
    End Sub

    Public Sub Close() Implements ITrayScannerClient.Close
        SyncLock syncRoot
            If serialPortTPM IsNot Nothing AndAlso serialPortTPM.IsOpen Then serialPortTPM.Close()
        End SyncLock
    End Sub

    Public Function ReadTrayCode(ByVal timeoutMs As Integer) As String Implements ITrayScannerClient.ReadTrayCode
        ThrowIfDisposed()
        If timeoutMs <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(timeoutMs))

        SyncLock syncRoot
            If Not serialPortTPM.IsOpen Then Throw New InvalidOperationException("托盘扫码枪串口未打开。")
            serialPortTPM.DiscardInBuffer()
            serialPortTPM.DiscardOutBuffer()

            Dim builder As New StringBuilder()
            Dim deadline As DateTime = DateTime.UtcNow.AddMilliseconds(timeoutMs)
            Dim quietSince As DateTime = DateTime.UtcNow
            Do
                If serialPortTPM.BytesToRead > 0 Then
                    Dim chunk As String = serialPortTPM.ReadExisting()
                    builder.Append(chunk)
                    quietSince = DateTime.UtcNow
                    If chunk.IndexOf(ControlChars.Cr) >= 0 OrElse chunk.IndexOf(ControlChars.Lf) >= 0 Then Exit Do
                ElseIf builder.Length > 0 AndAlso (DateTime.UtcNow - quietSince).TotalMilliseconds >= 80 Then
                    Exit Do
                Else
                    Thread.Sleep(5)
                End If
            Loop While DateTime.UtcNow < deadline

            Return builder.ToString().Trim(ControlChars.Cr, ControlChars.Lf, " "c, Microsoft.VisualBasic.ChrW(0))
        End SyncLock
    End Function

    Private Sub ThrowIfDisposed()
        If disposed Then Throw New ObjectDisposedException(NameOf(TrayScannerClient))
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If disposed Then Return
        disposed = True
        Close()
        If ownsPort Then serialPortTPM.Dispose()
    End Sub
End Class
