Imports System.Windows.Forms

Public Enum UserRole
    Unauthenticated = 0
    Administrator = 1
    [Operator] = 2
    Engineer = 3
End Enum

Public Enum AppPermission
    UseBusinessFunctions = 0
    InitialSettings = 1
    ManageSystemSettings = InitialSettings
    Debug = 2
    DeviceDiagnostics = Debug
    UserManagement = 3
    ManageTestStandards = 4
    CalibrateChannels = 5
    ManageProcessParameters = 6
    ExportLogs = 7
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

    Public Shared Function IsEngineer() As Boolean
        Return _currentRole = UserRole.Engineer AndAlso IsAuthenticated()
    End Function

    Public Shared Function RequireAuthenticatedUser(owner As IWin32Window) As Boolean
        If IsAuthenticated() Then Return True

        MessageBox.Show(owner,
                        "当前未登录，请先登录后再执行测试。",
                        "需要登录",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information)
        Return False
    End Function

    Public Shared Sub SetSession(userName As String, role As UserRole)
        If role = UserRole.Unauthenticated Then
            ResetSession()
            Return
        End If

        If String.IsNullOrWhiteSpace(userName) Then
            Throw New ArgumentException("已登录角色必须包含用户名。", NameOf(userName))
        End If
        If role <> UserRole.Administrator AndAlso role <> UserRole.[Operator] AndAlso role <> UserRole.Engineer Then
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
                Return IsAuthenticated()
            Case AppPermission.InitialSettings, AppPermission.UserManagement
                Return IsAdministrator()
            Case AppPermission.Debug, AppPermission.ManageTestStandards, AppPermission.CalibrateChannels,
                 AppPermission.ManageProcessParameters, AppPermission.ExportLogs
                Return IsEngineer() OrElse IsAdministrator()
            Case Else
                Return False
        End Select
    End Function

    Public Shared Function RequirePermission(permission As AppPermission, owner As IWin32Window) As Boolean
        If CanAccess(permission) Then Return True

        Dim permissionName As String = "此操作"
        Select Case permission
            Case AppPermission.InitialSettings
                permissionName = "系统设置"
            Case AppPermission.UserManagement
                permissionName = "用户管理"
            Case AppPermission.Debug
                permissionName = "设备调试"
            Case AppPermission.ManageTestStandards
                permissionName = "测试标准维护"
            Case AppPermission.CalibrateChannels
                permissionName = "通道校准"
            Case AppPermission.ManageProcessParameters
                permissionName = "工艺参数维护"
            Case AppPermission.ExportLogs
                permissionName = "日志导出"
        End Select

        MessageBox.Show(owner,
                        "当前用户没有执行" & permissionName & "的权限。",
                        "权限不足",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning)
        Return False
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
