Public Class frmName
    Public mc As String
    Private Sub Cmd_OK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cmd_OK.Click
        If check() Then
            mc = TextBox1.Text
            Me.Close()
        End If
    End Sub
    Private Function check() As Boolean
        check = True
        If TextBox1.Text <> "" Then
            Dim service As New OcvDataService()
            If service.CheckTrayCodeExists(TextBox1.Text) Then
                MsgBox("该名称已经存在，请录入新名称")
                TextBox1.Focus()
                check = False
                Exit Function
            End If
        Else
            MsgBox("名称不能为空")
            TextBox1.Focus()
            check = False
            Exit Function
        End If
    End Function

    Private Sub Cmd_NO_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cmd_NO.Click
        mc = ""
        Me.Close()
    End Sub
End Class