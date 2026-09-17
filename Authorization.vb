Imports System.Windows.Forms

Public Enum UserRole
    Unauthenticated = 0
    Administrator = 1
    [Operator] = 2
End Enum

Public Enum AppPermission
    UseBusinessFunctions = 0
    InitialSettings = 1
    Debug = 2
    UserManagement = 3
End Enum

Public NotInheritable Class AuthorizationService
    Private Shared _currentRole As UserRole = UserRole.Unauthenticated
    Private Shared _currentUserName As String = String.Empty

    Private Sub New()
    End Sub

    Public Shared ReadOnly Property CurrentRole As UserRole
        Get
            Return _currentRole
        End Get
    End Property

    Public Shared ReadOnly Property CurrentUserName As String
        Get
            Return _currentUserName
        End Get
    End Property

    Public Shared Function IsAuthenticated() As Boolean
        Return _currentRole <> UserRole.Unauthenticated AndAlso Not String.IsNullOrWhiteSpace(_currentUserName)
    End Function

    Public Shared Function IsAdministrator() As Boolean
        Return _currentRole = UserRole.Administrator AndAlso IsAuthenticated()
    End Function

    Public Shared Sub SetSession(userName As String, role As UserRole)
        If role = UserRole.Unauthenticated Then
            ResetSession()
            Return
        End If

        If String.IsNullOrWhiteSpace(userName) Then
            Throw New ArgumentException("已登录角色必须包含用户名。", NameOf(userName))
        End If
        If role <> UserRole.Administrator AndAlso role <> UserRole.[Operator] Then
            Throw New ArgumentException("无效的用户角色。", NameOf(role))
        End If

        _currentRole = role
        _currentUserName = userName.Trim()
        operatorName = _currentUserName
        Initsetting.curOperator = _currentUserName
    End Sub

    Public Shared Sub ResetSession()
        _currentRole = UserRole.Unauthenticated
        _currentUserName = String.Empty
        operatorName = String.Empty
        Initsetting.curOperator = String.Empty
    End Sub

    Public Shared Function CanAccess(permission As AppPermission) As Boolean
        Select Case permission
            Case AppPermission.UseBusinessFunctions
                Return True
            Case AppPermission.InitialSettings, AppPermission.Debug, AppPermission.UserManagement
                Return IsAdministrator()
            Case Else
                Return False
        End Select
    End Function

    Public Shared Function RequireAdministrator(owner As IWin32Window) As Boolean
        If IsAdministrator() Then Return True

        MessageBox.Show(owner,
                        "当前用户没有执行此操作的管理员权限。",
                        "权限不足",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning)
        Return False
    End Function
End Class
