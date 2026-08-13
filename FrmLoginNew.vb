Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

Public Class FrmLoginNew

    '登录成功后的操作员类型 (1=系统管理员, 2=操作员),由 main() 传给 MDIForm
    Public Property OperTypeNum As Integer = 2

    Private csvFilePath As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) & "\UserData.txt"
    '用于密码哈希的固定盐(R2-08)。真实生产环境应改为每个用户独立随机盐并与用户名一同存储
    Friend Const PasswordSalt As String = "XL_OCV_256_Salt_2024"

    '把密码按 SHA256(salt + password) 哈希 (R2-08)。Friend 让 frmUserManage 等同程序集可复用同一套算法
    Friend Shared Function HashPassword(password As String) As String
        Using sha As SHA256 = SHA256.Create()
            Dim bytes() As Byte = sha.ComputeHash(Encoding.UTF8.GetBytes(PasswordSalt & password))
            Dim sb As New StringBuilder()
            For i As Integer = 0 To bytes.Length - 1
                sb.Append(bytes(i).ToString("x2"))
            Next
            Return sb.ToString()
        End Using
    End Function

    Private Sub FrmLoginNew_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Populate ComboBox with usernames from CSV file
        ComboBoxUsername.SelectedIndex = 0
        CheckAndCreateCSVFile()
        LoadUsernames()
    End Sub
    Private Sub CheckAndCreateCSVFile()
        If Not File.Exists(csvFilePath) Then
            ' Create new CSV file with headers:用户名,SHA256 哈希后的密码(R2-08)
            Using writer As New StreamWriter(csvFilePath)
                writer.WriteLine("用户名,密码哈希")
                '初始化默认管理员账号,密码为 admin 的哈希值,首次部署后必须手动修改
                writer.WriteLine("系统管理员," & HashPassword("admin"))
                writer.WriteLine("操作员," & HashPassword("operator"))
            End Using
        End If
    End Sub
    Private Sub LoadUsernames()
        ComboBoxUsername.Items.Clear()

        Dim lines As String() = File.ReadAllLines(csvFilePath)
        Dim i As Integer
        For Each line As String In lines
            Dim parts As String() = line.Split(","c)
            Dim storedUsername As String = parts(0)
            If (i > 0) Then
                ComboBoxUsername.Items.Add(storedUsername)
            End If
            i = i + 1
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

        ' Authenticate user using CSV file
        If AuthenticateUser(selectedUsername, password) Then
            ' Successful login
            Initsetting.curOperator = selectedUsername
            If (selectedUsername = "系统管理员") Then
                OperTypeNum = 1
            Else
                OperTypeNum = 2
            End If
            'R2-17:登录窗是 main() 里 ShowDialog 打开的模态窗,设 DialogResult=OK 让它返回,
            '        主窗 MDIForm 由 main() 在 Application.Run 中创建,登录窗关闭不再导致程序退出
            Me.DialogResult = DialogResult.OK
        Else
            ' Failed login
            MessageBox.Show("错误的用户名或密码！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    Private Function AuthenticateUser(username As String, password As String) As Boolean
        Dim lines As String() = File.ReadAllLines(csvFilePath)
        Dim inputHash As String = HashPassword(password)

        For Each line As String In lines
            Dim parts As String() = line.Split(","c)
            If parts.Length < 2 Then Continue For
            Dim storedUsername As String = parts(0)
            Dim storedPasswordHash As String = parts(1)

            '兼容老格式:若存储值非 64 位哈希(老明文)则按明文比对一次,登录后由用户主动修改密码升级为哈希存储 (R2-08)
            If username = storedUsername Then
                If storedPasswordHash.Length = 64 AndAlso storedPasswordHash = inputHash Then
                    Return True
                ElseIf storedPasswordHash = password Then
                    '明文匹配成功,顺便把这一行升级为哈希存储
                    UpgradePasswordToHash(username, password)
                    Return True
                End If
            End If
        Next

        Return False ' Authentication failed
    End Function

    '将指定用户名行的明文密码就地升级为哈希存储 (R2-08)
    Private Sub UpgradePasswordToHash(username As String, plainPassword As String)
        Try
            Dim lines As List(Of String) = File.ReadAllLines(csvFilePath).ToList()
            For i As Integer = 0 To lines.Count - 1
                Dim parts As String() = lines(i).Split(","c)
                If parts.Length >= 2 AndAlso parts(0) = username AndAlso parts(1) = plainPassword Then
                    lines(i) = $"{username},{HashPassword(plainPassword)}"
                End If
            Next
            File.WriteAllLines(csvFilePath, lines)
        Catch
        End Try
    End Sub

    Private Sub AddUser()
        Dim username As String = ComboBoxUsername.Text
        Dim password As String = PasswordTextBox.Text

        'If (username = "") Then
        '    MessageBox.Show("用户名不能为空.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        '    Exit Sub
        'End If
        'Using writer As New StreamWriter(csvFilePath, True)
        '    writer.WriteLine($"{username},{password}")
        'End Using

        '' Refresh the ComboBox with updated usernames
        'LoadUsernames()
        '新增/修改用户时存储哈希密码而非明文 (R2-08)
        Dim lines As List(Of String) = File.ReadAllLines(csvFilePath).ToList()
        Dim userExists As Boolean = False
        Dim passwordHash As String = HashPassword(password)

        For i As Integer = lines.Count - 1 To 0 Step -1
            Dim parts As String() = lines(i).Split(","c)
            Dim storedUsername As String = parts(0)

            If username = storedUsername Then
                lines(i) = $"{username},{passwordHash}" ' Update existing user with hash
                userExists = True
            End If
        Next
        If Not userExists Then
            lines.Add($"{username},{passwordHash}") ' Add new user with hash
        End If

        File.WriteAllLines(csvFilePath, lines)

        ' Refresh the ComboBox with updated usernames
        LoadUsernames()
    End Sub

    Private Sub DeleteUser1(username As String)
        Dim lines As List(Of String) = File.ReadAllLines(csvFilePath).ToList()

        For i As Integer = lines.Count - 1 To 0 Step -1
            Dim parts As String() = lines(i).Split(","c)
            Dim storedUsername As String = parts(0)

            If username = storedUsername Then
                lines.RemoveAt(i)
            End If
        Next

        File.WriteAllLines(csvFilePath, lines)

        ' Refresh the ComboBox with updated usernames
        LoadUsernames()
    End Sub
    Private Sub DeleteUser(username As String)
        Dim confirmResult As DialogResult = MessageBox.Show($"你确定要删除用户 '{username}'吗?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

        If confirmResult = DialogResult.Yes Then
            Dim lines As List(Of String) = File.ReadAllLines(csvFilePath).ToList()

            For i As Integer = lines.Count - 1 To 0 Step -1
                Dim parts As String() = lines(i).Split(","c)
                Dim storedUsername As String = parts(0)

                If username = storedUsername Then
                    lines.RemoveAt(i)
                End If
            Next

            File.WriteAllLines(csvFilePath, lines)

            ' Refresh the ComboBox with updated usernames
            LoadUsernames()
        End If
    End Sub


    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        AddUser()
        ComboBoxUsername.SelectedIndex = 0
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Dispose()
    End Sub

    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        DeleteUser(ComboBoxUsername.Text)
        ComboBoxUsername.SelectedIndex = 0
    End Sub


    ' Other functions like Forgot Password, Register, etc.
End Class


