Imports System.IO.Ports
Imports System.Threading

''' <summary>
''' 控制板串口传输层。帧格式继续由现有控制板协议生成，串口仅负责替代原 TCP 传输。
''' </summary>
Public Class SerialControlBoardClient
    Private ReadOnly port As SerialPort
    Private ReadOnly syncRoot As New Object()

    Public Sub New(controlPort As SerialPort)
        port = controlPort
    End Sub

    Public Function SendAndReceive(frame As Byte(), Optional timeoutMs As Integer = 2000) As Byte()
        If port Is Nothing OrElse Not port.IsOpen Then Throw New InvalidOperationException("控制板串口未连接")
        If frame Is Nothing OrElse frame.Length = 0 Then Return New Byte() {}
        SyncLock syncRoot
            port.ReadTimeout = Math.Max(100, timeoutMs)
            port.DiscardInBuffer()
            port.Write(frame, 0, frame.Length)
            Dim result As New List(Of Byte)()
            Dim started As Integer = Environment.TickCount
            Do
                If port.BytesToRead > 0 Then
                    Dim buffer(255) As Byte
                    Dim count As Integer = port.Read(buffer, 0, Math.Min(buffer.Length, port.BytesToRead))
                    For i As Integer = 0 To count - 1
                        result.Add(buffer(i))
                    Next
                    started = Environment.TickCount
                ElseIf result.Count > 0 AndAlso Environment.TickCount - started >= 80 Then
                    Exit Do
                Else
                    Thread.Sleep(10)
                End If
            Loop While Environment.TickCount - started < timeoutMs
            Return result.ToArray()
        End SyncLock
    End Function

    Public Function ReadTemperatureAndIoStatus() As Byte()
        Return SendAndReceive(BuildCommand(Function(c) c.GetSignalTrayTempertureAndIOStatus()))
    End Function

    Public Function SetOverTemperatureAndMesStatus(connectStatus As Byte, temperatureValue As Short) As Byte()
        Return SendAndReceive(BuildCommand(Function(c) c.SetOverTempertureAndMesStatus(connectStatus, temperatureValue)))
    End Function

    Private Shared Function BuildCommand(builder As Func(Of SocketCtrolCmd, Byte())) As Byte()
        Using dummySocket As New Net.Sockets.Socket(Net.Sockets.AddressFamily.InterNetwork, Net.Sockets.SocketType.Stream, Net.Sockets.ProtocolType.Tcp)
            Dim cmd As New SocketCtrolCmd(dummySocket, Nothing, 0, Nothing)
            Return builder(cmd)
        End Using
    End Function
End Class
