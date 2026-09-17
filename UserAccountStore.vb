Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Text

Public Class UserAccount
    Public Property UserName As String
    Public Property PasswordHash As String
    Public Property Role As UserRole
End Class

Public NotInheritable Class UserAccountStore
    Private Shared ReadOnly Utf8Bom As New UTF8Encoding(True)

    Private Sub New()
    End Sub

    Public Shared ReadOnly Property FilePath As String
        Get
            Return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "UserData.txt")
        End Get
    End Property

    Public Shared Sub EnsureFile()
        If File.Exists(FilePath) Then Return

        Dim lines As New List(Of String) From {
            "用户名,密码哈希,角色",
            "系统管理员," & FrmLoginNew.HashPassword("admin") & ",Administrator",
            "操作员," & FrmLoginNew.HashPassword("operator") & ",Operator"
        }
        File.WriteAllLines(FilePath, lines, Utf8Bom)
    End Sub

    Public Shared Function LoadAccounts() As List(Of UserAccount)
        EnsureFile()
        Dim accounts As New List(Of UserAccount)()

        For Each line As String In File.ReadAllLines(FilePath, Encoding.UTF8)
            Dim account As UserAccount = ParseLine(line)
            If account IsNot Nothing Then accounts.Add(account)
        Next

        Return accounts
    End Function

    Public Shared Function ParseLine(line As String) As UserAccount
        If String.IsNullOrWhiteSpace(line) OrElse line.TrimStart().StartsWith("用户名") Then Return Nothing

        Dim parts As String() = line.Split(","c)
        If parts.Length < 2 OrElse String.IsNullOrWhiteSpace(parts(0)) Then Return Nothing

        Dim role As UserRole = UserRole.[Operator]
        If parts.Length >= 3 Then
            Dim parsedRole As UserRole
            If [Enum].TryParse(parts(2).Trim(), True, parsedRole) AndAlso (parsedRole = UserRole.Administrator OrElse parsedRole = UserRole.[Operator]) Then
                role = parsedRole
            End If
        ElseIf parts(0).Trim() = "系统管理员" Then
            role = UserRole.Administrator
        End If

        Return New UserAccount With {
            .UserName = parts(0).Trim(),
            .PasswordHash = parts(1).Trim(),
            .Role = role
        }
    End Function

    Public Shared Function Authenticate(userName As String, password As String) As UserAccount
        If String.IsNullOrWhiteSpace(userName) OrElse String.IsNullOrWhiteSpace(password) Then Return Nothing

        Dim accounts As List(Of UserAccount) = LoadAccounts()
        Dim account As UserAccount = accounts.FirstOrDefault(Function(item) item.UserName = userName.Trim())
        If account Is Nothing Then Return Nothing

        Dim inputHash As String = FrmLoginNew.HashPassword(password)
        Dim passwordMatches As Boolean = account.PasswordHash.Length = 64 AndAlso account.PasswordHash = inputHash
        Dim legacyPlainTextMatches As Boolean = account.PasswordHash = password
        If Not passwordMatches AndAlso Not legacyPlainTextMatches Then Return Nothing

        If legacyPlainTextMatches Then
            account.PasswordHash = inputHash
            SaveAccounts(accounts)
        End If

        Return account
    End Function

    Public Shared Sub SaveAccounts(accounts As IEnumerable(Of UserAccount))
        Dim lines As New List(Of String) From {"用户名,密码哈希,角色"}
        For Each account As UserAccount In accounts
            lines.Add(account.UserName & "," & account.PasswordHash & "," & account.Role.ToString())
        Next
        File.WriteAllLines(FilePath, lines, Utf8Bom)
    End Sub

    Public Shared Sub AddOrUpdate(userName As String, password As String, role As UserRole)
        If String.IsNullOrWhiteSpace(userName) Then Throw New ArgumentException("用户名不能为空。", NameOf(userName))
        If userName.Contains(","c) Then Throw New ArgumentException("用户名不能包含逗号。", NameOf(userName))
        If String.IsNullOrEmpty(password) Then Throw New ArgumentException("密码不能为空。", NameOf(password))
        If role = UserRole.Unauthenticated Then Throw New ArgumentException("用户角色无效。", NameOf(role))

        Dim accounts As List(Of UserAccount) = LoadAccounts()
        Dim account As UserAccount = accounts.FirstOrDefault(Function(item) item.UserName = userName.Trim())
        If account Is Nothing Then
            accounts.Add(New UserAccount With {.UserName = userName.Trim(), .PasswordHash = FrmLoginNew.HashPassword(password), .Role = role})
        Else
            account.PasswordHash = FrmLoginNew.HashPassword(password)
            account.Role = role
        End If
        SaveAccounts(accounts)
    End Sub

    Public Shared Function Delete(userName As String) As Boolean
        If String.IsNullOrWhiteSpace(userName) OrElse userName.Trim() = "系统管理员" Then Return False

        Dim accounts As List(Of UserAccount) = LoadAccounts()
        Dim removed As Integer = accounts.RemoveAll(Function(item) item.UserName = userName.Trim())
        If removed = 0 Then Return False
        SaveAccounts(accounts)
        Return True
    End Function
End Class
