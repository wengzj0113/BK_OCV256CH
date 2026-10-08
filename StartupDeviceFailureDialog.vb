Public Class StartupDeviceFailureDialog
    Public Sub New(ByVal message As String)
        InitializeComponent()
        lblMessage.Text = message
    End Sub

    Private Sub StartupDeviceFailureDialog_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        autoCloseTimer.Start()
    End Sub

    Private Sub autoCloseTimer_Tick(sender As Object, e As EventArgs) Handles autoCloseTimer.Tick
        autoCloseTimer.Stop()
        Close()
    End Sub

    Private Sub StartupDeviceFailureDialog_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        autoCloseTimer.Stop()
    End Sub
End Class
