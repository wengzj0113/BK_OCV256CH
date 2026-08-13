Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

Public Class SocketClient
    Private clientSocket As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    '保护 clientSocket 的 Send / Receive 并发访问 (R2-06)
    Private ReadOnly socketLock As New Object()
    Private receiveBuffer As Byte() = New Byte(1024) {} ' Buffer for receiving data
    '停止线程
    Private cts As CancellationTokenSource = Nothing
    Private cancellationToken As CancellationToken = Nothing
    Public connectState As Boolean

    Public Sub New()
        connectState = False
    End Sub

    Public Sub ConnectToServer()
        Try
            Dim serverIP As IPAddress = IPAddress.Parse("192.168.69.101")
            Dim serverEndPoint As New IPEndPoint(serverIP, 1030) ' Replace with your server's IP and port

            SyncLock socketLock
                clientSocket.BeginConnect(serverEndPoint, AddressOf ConnectCallback, Nothing)
            End SyncLock
            connectState = True
            '开启线程
            cts = New CancellationTokenSource()
            cancellationToken = cts.Token

            Dim thread As New Thread(Sub() DoWork(cancellationToken))
            thread.IsBackground = True
            thread.Start()

        Catch ex As Exception
            '非 UI 线程,不再 MessageBox.Show 跨线程弹框 (R2-05)
            Diagnostics.Debug.WriteLine("SocketClient ConnectToServer error: " & ex.Message)
        End Try
    End Sub

    Private Sub DoWork(token As CancellationToken)
        ' 单次轮询间隔 Sleep,不再连续两次 Sleep 浪费 2s (R2-05)
        Const POLL_INTERVAL_MS As Integer = 2000
        While Not token.IsCancellationRequested
            Try
                Dim bytesToSend As Byte() = GetSignalTrayTempertureAndIOStatus()
                Dim result As String = BitConverter.ToString(bytesToSend).Replace("-", " ")
                send111 = result
                SyncLock socketLock
                    If clientSocket.Connected Then
                        clientSocket.BeginSend(bytesToSend, 0, bytesToSend.Length, SocketFlags.None, AddressOf SendCallback, Nothing)
                    End If
                End SyncLock
                Thread.Sleep(POLL_INTERVAL_MS)
            Catch ex As Exception
                Diagnostics.Debug.WriteLine("SocketClient DoWork error: " & ex.Message)
                Thread.Sleep(POLL_INTERVAL_MS)
            End Try
        End While
    End Sub

    Private Sub ConnectCallback(ar As IAsyncResult)
        Try
            SyncLock socketLock
                clientSocket.EndConnect(ar)
                clientSocket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, AddressOf ReceiveCallback, Nothing)
            End SyncLock
        Catch ex As Exception
            Diagnostics.Debug.WriteLine("SocketClient ConnectCallback error: " & ex.Message)
        End Try
    End Sub

    Private Sub ReceiveCallback(ar As IAsyncResult)
        Try
            Dim bytesRead As Integer = 0
            SyncLock socketLock
                bytesRead = clientSocket.EndReceive(ar)
                If bytesRead > 0 Then
                    Dim receivedData As String = Encoding.ASCII.GetString(receiveBuffer, 0, bytesRead)
                    Dim newArray(bytesRead - 1) As Byte
                    Array.Copy(receiveBuffer, newArray, bytesRead)
                    If newArray.Length = 22 Then
                        dataOfTemPeraturesAndIO = newArray
                    End If
                    Dim result As String = BitConverter.ToString(newArray).Replace("-", " ")
                    rec111 = result
                    ' Continue listening for more data
                    clientSocket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, AddressOf ReceiveCallback, Nothing)
                End If
            End SyncLock
        Catch ex As Exception
            Diagnostics.Debug.WriteLine("SocketClient ReceiveCallback error: " & ex.Message)
        End Try
    End Sub

    Private Sub TimerCallback(state As Object)
        ' This method is called by the timer every second
        Try
            ' Send data to the server
            Dim bytesToSend As Byte() = GetSignalTrayTempertureAndIOStatus()
            System.Threading.Thread.Sleep(1000)
            Dim result As String = BitConverter.ToString(bytesToSend).Replace("-", " ")
            send111 = result
            System.Threading.Thread.Sleep(1000)
            clientSocket.BeginSend(bytesToSend, 0, bytesToSend.Length, SocketFlags.None, AddressOf SendCallback, Nothing)
        Catch ex As Exception
            MessageBox.Show("Error in TimerCallback: " & ex.Message)
        End Try

    End Sub
    Private Sub ReadComm16()
        ' This method is called by the timer every second
        Try
            ' Send data to the server
            Dim bytesToSend As Byte() = GetSignalTrayTempertureAndIOStatus()
            System.Threading.Thread.Sleep(1000)
            Dim result As String = BitConverter.ToString(bytesToSend).Replace("-", " ")
            send111 = result
            System.Threading.Thread.Sleep(1000)
            clientSocket.BeginSend(bytesToSend, 0, bytesToSend.Length, SocketFlags.None, AddressOf SendCallback, Nothing)
        Catch ex As Exception
            MessageBox.Show("Error in TimerCallback: " & ex.Message)
        End Try

    End Sub

    Private Sub SendCallback(ar As IAsyncResult)
        Try
            clientSocket.EndSend(ar)
        Catch ex As Exception
            MessageBox.Show("Error in SendCallback: " & ex.Message)
        End Try
    End Sub

    Public Sub Close()
        Try
            If cts IsNot Nothing Then
                cts.Cancel()
            End If
            If clientSocket IsNot Nothing Then
                clientSocket.Close()
            End If
        Finally
            If cts IsNot Nothing Then
                cts.Dispose()
                cts = Nothing
            End If
        End Try
    End Sub

    ''' <summary>
    ''' 10查询多通道测试完成命令 15h
    ''' </summary>
    ''' <returns></returns>
    Public Function GetSignalMutiChannelTestFinish() As Byte()
        Dim sendByte As Byte() = New Byte(8) {}
        sendByte(0) = &H0
        sendByte(1) = &HBB
        sendByte(2) = &HCC
        sendByte(3) = &HEE
        '命令号H15
        sendByte(4) = &H15
        '命令内容
        sendByte(5) = &H0
        sendByte(6) = &H0
        sendByte(7) = &H0
        Return sendByte
    End Function
    ''' <summary>
    ''' 命令16h，查询IO状态信息
    ''' </summary>
    ''' <returns></returns>
    Public Function GetSignalTrayTempertureAndIOStatus() As Byte()
        Dim sendByte As Byte() = New Byte(8) {}
        sendByte(0) = &H0
        sendByte(1) = &HBB
        sendByte(2) = &HCC
        sendByte(3) = &HEE
        '命令号H16
        sendByte(4) = &H16
        '命令内容长度 为0
        sendByte(5) = &H0
        sendByte(6) = &H0
        sendByte(7) = &H0
        Return sendByte
    End Function

    ''' <summary>
    ''' 发送上位机与MES系统连接状态,下发超温保护值 17h
    ''' </summary>
    ''' <returns></returns>
    Public Function SetOverTempertureAndMesStatus(ByVal ConnectStatus As Byte, ByVal setTemperateVal As Int16) As Byte()
        Dim sendByte As Byte() = New Byte(11) {}
        sendByte(0) = &H0
        sendByte(1) = &HBB
        sendByte(2) = &HCC
        sendByte(3) = &HEE
        '命令号H17
        sendByte(4) = &H17
        '命令内容长度 为1
        sendByte(5) = &H4
        sendByte(6) = &H0
        sendByte(7) = &H0
        '数据内容 4 字节
        sendByte(8) = ConnectStatus  '=1 连接  =0 断开
        '温度数据
        Dim byteArray() As Byte = BitConverter.GetBytes(setTemperateVal)
        sendByte(9) = byteArray(0)
        sendByte(10) = byteArray(1)
        sendByte(11) = &H0
        Return sendByte
    End Function

End Class

Module Program
    Sub Main()
        Dim client As New SocketClient()

        ' Keep the program running
        MessageBox.Show("Press Enter to exit...")
        Console.ReadLine()

        ' Close the client socket when done
        client.Close()
    End Sub
End Module

