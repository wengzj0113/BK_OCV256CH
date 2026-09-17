param(
    [switch]$StaticOnly,
    [string]$AssemblyPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'bin\AnyCPU\XL_OCV_256CH.exe')
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

function Read-Source([string]$name) {
    $path = Join-Path $root $name
    if (-not (Test-Path -LiteralPath $path)) { throw "Missing source file: $name" }
    return Get-Content -Raw -LiteralPath $path
}

function Assert-Contains([string]$source, [string]$pattern, [string]$message) {
    if ($source -notmatch $pattern) { throw $message }
}

function Assert-NotContains([string]$source, [string]$pattern, [string]$message) {
    if ($source -match $pattern) { throw $message }
}

$authorization = Read-Source 'Authorization.vb'
$accountStore = Read-Source 'UserAccountStore.vb'
$mdi = Read-Source 'MDIForm.vb'
$login = Read-Source 'FrmLoginNew.vb'
$userManage = Read-Source 'frmUserManage.vb'
$designer = Read-Source 'frmUserManage.Designer.vb'
$frmInit = Read-Source 'frmInit.vb'
$frmDebug = Read-Source 'frmDebug.vb'

Assert-Contains $authorization 'Public Enum UserRole' 'Authorization must define UserRole.'
Assert-Contains $authorization 'Public Enum AppPermission' 'Authorization must define AppPermission.'
Assert-Contains $authorization 'Public Shared Function CanAccess' 'Authorization must centralize permission decisions.'
Assert-Contains $accountStore 'Role' 'UserAccountStore must persist account roles.'
Assert-Contains $login 'account\.Role' 'Login must use the authenticated account role.'
Assert-Contains $userManage 'ComboBoxRole' 'User management must expose a role selector.'
Assert-Contains $designer 'ComboBoxRole' 'Role selector must be designer-owned.'
Assert-Contains $userManage 'LicenseManager\.UsageMode' 'User management form must remain loadable in the designer.'
Assert-Contains $frmInit 'LicenseManager\.UsageMode' 'Initial settings form must remain loadable in the designer.'
Assert-Contains $frmDebug 'LicenseManager\.UsageMode' 'Debug form must remain loadable in the designer.'
Assert-Contains $mdi 'RequireAdministrator' 'MDI administrator handlers must enforce permissions.'
Assert-Contains $mdi 'logoutMenu' 'MDI must expose an explicit logout entry.'
Assert-NotContains $login 'selectedUsername\s*=\s*["'']系统管理员["'']' 'Login must not infer role from username text.'

foreach ($methodName in @('init_Click', 'btnInit_Click', 'btnDebug_Click', 'ToolDebug_Click', 'userMenage_Click')) {
    $method = [regex]::Match($mdi, "Private Sub $methodName\b.*?\n    End Sub", [System.Text.RegularExpressions.RegexOptions]::Singleline).Value
    if ([string]::IsNullOrWhiteSpace($method) -or $method -notmatch 'RequireAdministrator') {
        throw "$methodName does not enforce administrator access at its event entry."
    }
}

if ($StaticOnly) {
    Write-Output 'Authorization static contract: PASS'
    exit 0
}

Add-Type -AssemblyName System.Windows.Forms
if (-not (Test-Path -LiteralPath $AssemblyPath)) { throw "Assembly not found: $AssemblyPath" }
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $AssemblyPath))
$serviceType = $assembly.GetType('software.AuthorizationService', $true)
$roleType = $assembly.GetType('software.UserRole', $true)
$permissionType = $assembly.GetType('software.AppPermission', $true)
$canAccess = $serviceType.GetMethod('CanAccess', [Reflection.BindingFlags] 'Public, Static')
$setSession = $serviceType.GetMethod('SetSession', [Reflection.BindingFlags] 'Public, Static')
$resetSession = $serviceType.GetMethod('ResetSession', [Reflection.BindingFlags] 'Public, Static')
if ($null -eq $canAccess -or $null -eq $setSession -or $null -eq $resetSession) {
    throw 'AuthorizationService public session or policy methods are missing.'
}

$business = [Enum]::Parse($permissionType, 'UseBusinessFunctions')
$adminPermission = [Enum]::Parse($permissionType, 'InitialSettings')
$unauthenticated = [Enum]::Parse($roleType, 'Unauthenticated')
$operator = [Enum]::Parse($roleType, 'Operator')
$administrator = [Enum]::Parse($roleType, 'Administrator')

$parseLine = $assembly.GetType('software.UserAccountStore', $true).GetMethod('ParseLine', [Reflection.BindingFlags] 'Public, Static')
$adminName = ([char]0x7CFB).ToString() + ([char]0x7EDF).ToString() + ([char]0x7BA1).ToString() + ([char]0x7406).ToString() + ([char]0x5458).ToString()
$operatorName = ([char]0x64CD).ToString() + ([char]0x4F5C).ToString() + ([char]0x5458).ToString()
$legacyAdmin = $parseLine.Invoke($null, @($adminName + ',hash'))
$legacyOperator = $parseLine.Invoke($null, @($operatorName + ',hash'))
$newAdmin = $parseLine.Invoke($null, @($adminName + ',hash,Administrator'))
$unknownRole = $parseLine.Invoke($null, @('temporary,hash,UnknownRole'))
$unknownAdminRole = $parseLine.Invoke($null, @($adminName + ',hash,UnknownRole'))
$numericUnknownRole = $parseLine.Invoke($null, @('numeric,hash,99'))
if ($legacyAdmin.Role.ToString() -ne 'Administrator') { throw 'Legacy administrator record did not map to Administrator.' }
if ($legacyOperator.Role.ToString() -ne 'Operator') { throw 'Legacy operator record did not map to Operator.' }
if ($newAdmin.Role.ToString() -ne 'Administrator') { throw 'Three-column administrator record did not map to Administrator.' }
if ($unknownRole.Role.ToString() -ne 'Operator') { throw 'Unknown role did not safely map to Operator.' }
if ($unknownAdminRole.Role.ToString() -ne 'Operator') { throw 'Unknown role must not elevate the administrator username.' }
if ($numericUnknownRole.Role.ToString() -ne 'Operator') { throw 'Numeric unknown role must safely map to Operator.' }

foreach ($role in @($unauthenticated, $operator, $administrator)) {
    $setSession.Invoke($null, @('test-user', $role)) | Out-Null
    $businessAllowed = [bool]$canAccess.Invoke($null, @($business))
    $adminAllowed = [bool]$canAccess.Invoke($null, @($adminPermission))
    if (-not $businessAllowed) { throw "Business permission denied for role $role." }
    if (($role -eq $administrator) -ne $adminAllowed) { throw "Administrator permission mismatch for role $role." }
}

$mdiType = $assembly.GetType('software.MDIForm', $true)
$form = [Activator]::CreateInstance($mdiType)
try {
    $applyPermissions = $mdiType.GetMethod('ApplyOperatorPermissions', [Reflection.BindingFlags] 'Instance, NonPublic')
    function Find-ToolStripItem($items, [string]$name) {
        foreach ($item in $items) {
            if ($item.Name -eq $name) { return $item }
            if ($item -is [System.Windows.Forms.ToolStripDropDownItem]) {
                $found = Find-ToolStripItem $item.DropDownItems $name
                if ($null -ne $found) { return $found }
            }
        }
        return $null
    }
    $initField = Find-ToolStripItem $form.MainMenuStrip.Items 'init'
    $debugField = Find-ToolStripItem $form.MainMenuStrip.Items 'btnDebug'
    $userManageField = Find-ToolStripItem $form.MainMenuStrip.Items 'userMenage'
    $logoutField = Find-ToolStripItem $form.MainMenuStrip.Items 'logoutMenu'
    foreach ($role in @($unauthenticated, $operator, $administrator)) {
        $setSession.Invoke($null, @('test-user', $role)) | Out-Null
        $applyPermissions.Invoke($form, $null) | Out-Null
        $adminEnabled = $initField.Enabled -and $debugField.Enabled -and $userManageField.Enabled
        if (($role -eq $administrator) -ne $adminEnabled) { throw "Administrator UI state mismatch for role $role." }
    }

    $setSession.Invoke($null, @('test-user', $administrator)) | Out-Null
    $logoutHandler = $mdiType.GetMethod('logoutMenu_Click', [Reflection.BindingFlags] 'Instance, NonPublic')
    $logoutHandler.Invoke($form, @($null, [EventArgs]::Empty)) | Out-Null
    $applyPermissions.Invoke($form, $null) | Out-Null
    if ($serviceType.GetProperty('CurrentRole').GetValue($null, $null).ToString() -ne 'Unauthenticated') { throw 'Logout did not reset the role.' }
    if ($logoutField.Enabled) { throw 'Logout menu remained enabled after logout.' }
}
finally {
    $form.Dispose()
}

$resetSession.Invoke($null, $null) | Out-Null
Write-Output 'Authorization contract: PASS'
