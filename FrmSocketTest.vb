Imports System.Runtime.CompilerServices
Imports System.Threading

Public Class FrmSocketTest
    Private client As SocketClient
    Private connState As Boolean = False
    Dim timer As Timer
    Private Sub TimerCallback(state As Object)
        Textsend.Text = ""
        TextReceive.Text = ""
        System.Threading.Thread.Sleep(1500)
        Textsend.Text = send111
        TextReceive.Text = rec111
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        If (connState <> True) Then
            client = New SocketClient()
            client.ConnectToServer()
            connState = client.connectState
        Else
            MsgBox("请不要重复连接！")
        End If

    End Sub

    Private Sub FrmSocketTest_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim timer16H As New Timer(AddressOf TimerCallback, Nothing, 0, 1000)
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        client.Close()

        Me.Close()
    End Sub
End Class