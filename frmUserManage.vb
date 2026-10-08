Imports System.Linq

Public Class frmUserManage
    Private Sub frmUserManage_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Not (Me.DesignMode OrElse System.ComponentModel.LicenseManager.UsageMode = System.ComponentModel.LicenseUsageMode.Designtime) AndAlso Not AuthorizationService.RequireAdministrator(Me) Then
            BeginInvoke(New MethodInvoker(AddressOf Close))
            Return
        End If

        UserAccountStore.EnsureFile()
        LoadUsernames()
        If ComboBoxRole.Items.Count > 0 Then ComboBoxRole.SelectedIndex = 0
    End Sub

    Private Sub LoadUsernames()
        ComboBoxUsername.Items.Clear()
        For Each account As UserAccount In UserAccountStore.LoadAccounts()
            ComboBoxUsername.Items.Add(account.UserName)
        Next

        If ComboBoxUsername.Items.Count > 0 Then
            ComboBoxUsername.SelectedIndex = 0
        End If
    End Sub

    Private Function SelectedRole() As UserRole
        Select Case ComboBoxRole.SelectedIndex
            Case 1
                Return UserRole.Engineer
            Case 2
                Return UserRole.Administrator
            Case Else
                Return UserRole.[Operator]
        End Select
    End Function

    Private Sub LoadSelectedRole()
        Dim account As UserAccount = UserAccountStore.LoadAccounts().FirstOrDefault(Function(item) item.UserName = ComboBoxUsername.Text.Trim())
        If account Is Nothing Then
            ComboBoxRole.SelectedIndex = 0
        Else
            Select Case account.Role
                Case UserRole.Engineer
                    ComboBoxRole.SelectedIndex = 1
                Case UserRole.Administrator
                    ComboBoxRole.SelectedIndex = 2
                Case Else
                    ComboBoxRole.SelectedIndex = 0
            End Select
        End If
    End Sub

    Private Sub AddOrUpdateUser()
        If Not AuthorizationService.RequireAdministrator(Me) Then Return

        Dim userName As String = ComboBoxUsername.Text.Trim()
        Dim password As String = PasswordTextBox.Text
        If String.IsNullOrWhiteSpace(userName) OrElse String.IsNullOrEmpty(password) Then
            MessageBox.Show("用户名和密码不能为空。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim role As UserRole = SelectedRole()
        If userName = AuthorizationService.CurrentUserName AndAlso role <> AuthorizationService.CurrentRole Then
            MessageBox.Show("不能在当前登录状态下修改自己的角色，请先退出登录后再操作。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Dim isExistingUser As Boolean = UserAccountStore.LoadAccounts().Any(Function(account) String.Equals(account.UserName, userName, StringComparison.OrdinalIgnoreCase))
            UserAccountStore.AddOrUpdate(userName, password, role)
            BusinessLogService.Write("操作", "成功", If(isExistingUser, "修改用户", "新增用户"), userName, If(isExistingUser, "用户信息已修改。", "已新增用户。"), "角色：" & role.ToString() & "；密码不记录到日志。", "用户管理")
            LoadUsernames()
            ComboBoxUsername.Text = userName
            PasswordTextBox.Clear()
            MessageBox.Show("用户信息已保存。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As ArgumentException
            BusinessLogService.Write("操作", "失败", "保存用户", userName, "用户信息保存失败。", ex.Message & "；密码不记录到日志。", "用户管理")
            MessageBox.Show(ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            BusinessLogService.Write("操作", "失败", "保存用户", userName, "用户信息保存失败。", ex.Message & "；密码不记录到日志。", "用户管理")
            MessageBox.Show("保存用户失败：" & ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub DeleteUser()
        If Not AuthorizationService.RequireAdministrator(Me) Then Return

        Dim userName As String = ComboBoxUsername.Text.Trim()
        If String.IsNullOrWhiteSpace(userName) Then Return
        If userName = AuthorizationService.CurrentUserName Then
            MessageBox.Show("不能删除当前登录用户。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        If userName = "系统管理员" Then
            MessageBox.Show("系统管理员不能被删除。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If MessageBox.Show("确定删除用户“" & userName & "”吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
        If UserAccountStore.Delete(userName) Then
            BusinessLogService.Write("操作", "成功", "删除用户", userName, "用户已删除。", "", "用户管理")
            LoadUsernames()
            PasswordTextBox.Clear()
            MessageBox.Show("用户已删除。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        AddOrUpdateUser()
    End Sub

    Private Sub btnChangePWD_Click(sender As Object, e As EventArgs) Handles btnChangePWD.Click
        AddOrUpdateUser()
    End Sub

    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        DeleteUser()
    End Sub

    Private Sub ComboBoxUsername_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxUsername.SelectedIndexChanged
        PasswordTextBox.Clear()
        LoadSelectedRole()
    End Sub
End Class
