Public Class FrmLoginNew

    '登录成功后的操作员类型 (1=系统管理员, 2=操作员),由 main() 传给 MDIForm
    Public Property OperTypeNum As Integer = 2

    Private Sub FrmLoginNew_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CheckAndCreateCSVFile()
        LoadUsernames()
        If ComboBoxUsername.Items.Count > 0 Then ComboBoxUsername.SelectedIndex = 0
    End Sub

    Private Sub CheckAndCreateCSVFile()
        UserAccountStore.EnsureFile()
    End Sub

    Private Sub LoadUsernames()
        ComboBoxUsername.Items.Clear()
        For Each account As UserAccount In UserAccountStore.LoadAccounts()
            ComboBoxUsername.Items.Add(account.UserName)
        Next
    End Sub

    Private Sub LoginButton_Click(sender As Object, e As EventArgs) Handles LoginButton.Click
        Dim selectedUsername As String = ComboBoxUsername.Text
        Dim password As String = PasswordTextBox.Text

        ' Validate input
        If String.IsNullOrWhiteSpace(selectedUsername) OrElse String.IsNullOrWhiteSpace(password) Then
            MessageBox.Show("请输入正确的用户密码.", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        Dim account As UserAccount = UserAccountStore.Authenticate(selectedUsername, password)
        If account IsNot Nothing Then
            Dim previousUser As String = AuthorizationService.CurrentUserName
            AuthorizationService.SetSession(account.UserName, account.Role)
            OperTypeNum = CInt(account.Role)
            BusinessLogService.Write("操作",
                                     "成功",
                                     If(String.IsNullOrWhiteSpace(previousUser), "登录", "切换用户"),
                                     account.UserName,
                                     If(String.IsNullOrWhiteSpace(previousUser), "用户登录成功", "用户切换成功"),
                                     "角色：" & account.Role.ToString(),
                                     "用户认证")
            'R2-17:登录窗是 main() 里 ShowDialog 打开的模态窗,设 DialogResult=OK 让它返回,
            '        主窗 MDIForm 由 main() 在 Application.Run 中创建,登录窗关闭不再导致程序退出
            Me.DialogResult = DialogResult.OK
        Else
            BusinessLogService.Write("操作", "失败", "登录", selectedUsername, "用户登录失败", "身份验证未通过。", "用户认证", selectedUsername)
            ' Failed login
            MessageBox.Show("错误的用户名或密码！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Dispose()
    End Sub

End Class
