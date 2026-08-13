Imports System.IO
Imports System.Text

'用户管理窗体。
'修复记录：
' 1) 旧版写入 UserData.csv(明文),与 FrmLoginNew 登录验证所用的 UserData.txt(哈希) 不一致,
'    导致"菜单改密码后下次登录仍然旧密码"。本版本统一写入 UserData.txt,并使用 SHA256 哈希。
' 2) 旧版 File.ReadAllLines/WriteAllLines 使用默认编码,产生 GBK 乱码。统一用 UTF-8(带 BOM)。
' 3) 旧版 btnChangePWD 实现是"先删后加"两步拼接,易出错;现统一通过 AddOrUpdateUser 完成。
' 4) 增加空值校验与提示。
Public Class frmUserManage
    '与 FrmLoginNew 共用同一份用户数据文件、同一种哈希算法
    Private userDataPath As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) & "\UserData.txt"
    Private utf8Bom As New UTF8Encoding(True)

    Private Sub frmUserManage_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CheckAndCreateUserFile()
        LoadUsernames()
        If ComboBoxUsername.Items.Count > 0 Then
            ComboBoxUsername.SelectedIndex = 0
        End If
    End Sub

    '如果用户文件不存在,创建一个并写入默认账号(系统管理员 / 操作员),首次部署后必须修改默认密码
    Private Sub CheckAndCreateUserFile()
        If Not File.Exists(userDataPath) Then
            Using writer As New StreamWriter(userDataPath, False, utf8Bom)
                writer.WriteLine("用户名,密码哈希")
                writer.WriteLine("系统管理员," & FrmLoginNew.HashPassword("admin"))
                writer.WriteLine("操作员," & FrmLoginNew.HashPassword("operator"))
            End Using
        End If
    End Sub

    '从用户文件读取所有用户名(跳过表头),填充到下拉框
    Private Sub LoadUsernames()
        ComboBoxUsername.Items.Clear()
        If Not File.Exists(userDataPath) Then Return

        Dim lines As String() = File.ReadAllLines(userDataPath, Encoding.UTF8)
        Dim isHeader As Boolean = True
        For Each line As String In lines
            Dim trimmed As String = line.Trim()
            If trimmed.Length = 0 Then Continue For
            Dim parts As String() = line.Split(","c)
            If parts.Length = 0 OrElse String.IsNullOrWhiteSpace(parts(0)) Then Continue For

            If isHeader Then
                isHeader = False   '跳过表头(用户名,密码哈希)
                Continue For
            End If
            ComboBoxUsername.Items.Add(parts(0).Trim())
        Next
    End Sub

    '新增或修改用户:已存在则更新密码,不存在则追加。供"添加用户""修改密码"两个按钮共用。
    Private Sub AddOrUpdateUser()
        Dim username As String = ComboBoxUsername.Text.Trim()
        Dim password As String = PasswordTextBox.Text

        If String.IsNullOrEmpty(username) Then
            MessageBox.Show("用户名不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        If username.Contains(","c) Then
            MessageBox.Show("用户名不能包含逗号!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        If String.IsNullOrEmpty(password) Then
            MessageBox.Show("密码不能为空!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim lines As List(Of String) = File.ReadAllLines(userDataPath, Encoding.UTF8).ToList()
        '允许文件第一行是表头,也允许没有表头
        Dim dataStart As Integer = 0
        If lines.Count > 0 AndAlso lines(0).StartsWith("用户名") Then
            dataStart = 1
        End If

        Dim passwordHash As String = FrmLoginNew.HashPassword(password)
        Dim userExists As Boolean = False
        For i As Integer = dataStart To lines.Count - 1
            Dim parts As String() = lines(i).Split(","c)
            If parts.Length >= 1 AndAlso parts(0).Trim() = username Then
                lines(i) = username & "," & passwordHash
                userExists = True
                Exit For
            End If
        Next
        If Not userExists Then
            '首次创建时若文件没有任何内容,补一行表头
            If lines.Count = 0 Then
                lines.Add("用户名,密码哈希")
            End If
            lines.Add(username & "," & passwordHash)
        End If

        File.WriteAllLines(userDataPath, lines, utf8Bom)
        LoadUsernames()
        '保持当前用户被选中,密码框清空
        ComboBoxUsername.Text = username
        PasswordTextBox.Text = ""

        Dim action As String = If(userExists, "密码已更新", "用户已添加")
        MessageBox.Show($"'{username}' {action}。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    '删除用户。系统管理员禁止删除(避免锁死系统)
    Private Sub DeleteUser()
        Dim username As String = ComboBoxUsername.Text.Trim()
        If String.IsNullOrEmpty(username) Then
            MessageBox.Show("请先选择要删除的用户!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        If username = "系统管理员" Then
            MessageBox.Show("系统管理员不能被删除,请重新操作!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim confirm As DialogResult = MessageBox.Show($"你确定要删除用户 '{username}' 吗?", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If confirm <> DialogResult.Yes Then Return

        Dim lines As List(Of String) = File.ReadAllLines(userDataPath, Encoding.UTF8).ToList()
        Dim dataStart As Integer = If(lines.Count > 0 AndAlso lines(0).StartsWith("用户名"), 1, 0)
        Dim removed As Boolean = False
        For i As Integer = lines.Count - 1 To dataStart Step -1
            Dim parts As String() = lines(i).Split(","c)
            If parts.Length >= 1 AndAlso parts(0).Trim() = username Then
                lines.RemoveAt(i)
                removed = True
            End If
        Next

        If removed Then
            File.WriteAllLines(userDataPath, lines, utf8Bom)
            LoadUsernames()
            If ComboBoxUsername.Items.Count > 0 Then
                ComboBoxUsername.SelectedIndex = 0
            Else
                ComboBoxUsername.Text = ""
            End If
            PasswordTextBox.Text = ""
            MessageBox.Show($"用户 '{username}' 已删除。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        AddOrUpdateUser()
    End Sub

    Private Sub btnChangePWD_Click(sender As Object, e As EventArgs) Handles btnChangePWD.Click
        '修改密码(已存在则更新)。若选中的用户不存在,仍可借此"补建"该用户并设置密码
        AddOrUpdateUser()
    End Sub

    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        DeleteUser()
    End Sub

    Private Sub ComboBoxUsername_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxUsername.SelectedIndexChanged
        PasswordTextBox.Text = ""
    End Sub
End Class
