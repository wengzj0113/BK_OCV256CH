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
$frmTest = Read-Source 'frmTest.vb'

Assert-Contains $authorization 'Public Enum UserRole' 'Authorization must define UserRole.'
Assert-Contains $authorization 'Public Enum AppPermission' 'Authorization must define AppPermission.'
Assert-Contains $authorization 'Public Shared Function CanAccess' 'Authorization must centralize permission decisions.'
Assert-Contains $authorization 'Engineer\s*=\s*3' 'Authorization must define the engineer role.'
foreach ($permissionName in @('ManageTestStandards', 'CalibrateChannels', 'DeviceDiagnostics', 'ManageProcessParameters', 'ManageSystemSettings', 'ExportLogs')) {
    Assert-Contains $authorization ([regex]::Escape($permissionName)) "Authorization must define $permissionName."
}
Assert-Contains $authorization 'RequireAuthenticatedUser' 'Business actions must require an authenticated user.'
Assert-Contains $authorization 'RequirePermission' 'Protected actions must use a central permission guard.'
Assert-Contains $accountStore 'Role' 'UserAccountStore must persist account roles.'
Assert-Contains $login 'account\.Role' 'Login must use the authenticated account role.'
Assert-Contains $userManage 'ComboBoxRole' 'User management must expose a role selector.'
Assert-Contains $designer 'ComboBoxRole' 'Role selector must be designer-owned.'
$engineerLabel = ([char]0x5DE5).ToString() + ([char]0x7A0B).ToString() + ([char]0x5E08).ToString()
Assert-Contains $designer $engineerLabel 'Role selector must offer the engineer role.'
Assert-Contains $userManage 'LicenseManager\.UsageMode' 'User management form must remain loadable in the designer.'
Assert-Contains $frmInit 'LicenseManager\.UsageMode' 'Initial settings form must remain loadable in the designer.'
Assert-Contains $frmDebug 'LicenseManager\.UsageMode' 'Debug form must remain loadable in the designer.'
Assert-Contains $mdi 'RequireAdministrator' 'MDI administrator handlers must enforce permissions.'
Assert-Contains $mdi 'ManageProcessParameters' 'MDI process settings entry must use the engineer permission.'
Assert-Contains $mdi 'DeviceDiagnostics' 'MDI diagnostics entry must use the engineer permission.'
Assert-Contains $mdi 'CalibrateChannels' 'MDI calibration entry must use the engineer permission.'
Assert-Contains $mdi 'RequireAuthenticatedUser' 'MDI business handlers must enforce authentication.'
Assert-Contains (Read-Source 'FrmManualTest.vb') 'RequireAuthenticatedUser' 'Manual test must enforce authentication at its action entry.'
Assert-Contains $mdi 'logoutMenu' 'MDI must expose an explicit logout entry.'
Assert-NotContains $mdi 'If Not ready Then SetTestEntryEnabled\(False\)' 'Database health checks must not disable business test entries.'
$adminUsernameAssignmentPattern = 'selectedUsername\s*=\s*[' + '"' + "'" + ']系统管理员[' + '"' + "'" + ']'
Assert-NotContains $login $adminUsernameAssignmentPattern 'Login must not infer role from username text.'
Assert-Contains $frmTest 'AuthorizationService\.CurrentUserName' 'OCV_DATA test tasks must use the authenticated session user.'
Assert-NotContains $frmTest '\.OperatorName\s*=\s*Initsetting\.curOperator' 'OCV_DATA test tasks must not use the stale compatibility operator field directly.'

$expectedEntryPermissions = @{
    'init_Click' = 'ManageProcessParameters'
    'btnInit_Click' = 'ManageProcessParameters'
    'btnDebug_Click' = 'DeviceDiagnostics'
    'ToolDebug_Click' = 'DeviceDiagnostics'
    'userMenage_Click' = 'RequireAdministrator'
}
foreach ($methodName in $expectedEntryPermissions.Keys) {
    $method = [regex]::Match($mdi, "Private Sub $methodName\b.*?\n    End Sub", [System.Text.RegularExpressions.RegexOptions]::Singleline).Value
    if ([string]::IsNullOrWhiteSpace($method) -or $method -notmatch [regex]::Escape($expectedEntryPermissions[$methodName])) {
        throw "$methodName does not enforce $($expectedEntryPermissions[$methodName]) at its event entry."
    }
}
Assert-Contains $frmInit 'ApplyRoleSpecificTabs' 'Initial settings must restrict visible pages by role.'
Assert-Contains $frmInit 'GroupBox8\.Visible\s*=\s*False' 'Engineer settings must hide workstation selection.'
Assert-Contains $frmInit 'txtPath\.Visible\s*=\s*False' 'Engineer settings must hide the system output path.'
Assert-Contains $frmInit 'EngineerSaveProcessSettings' 'Engineer settings must use the restricted process save path.'
Assert-Contains $frmInit 'EngineerProcessSettingsPolicy\.ApplyTo' 'Engineer save must pass only process settings through the whitelist policy.'
Assert-Contains $frmInit 'ManageTestStandards' 'Test-standard import and save must check permission.'
Assert-Contains (Read-Source 'FrmChannelCalibration.vb') 'CalibrateChannels' 'Calibration form must enforce calibration permissions.'
Assert-Contains (Read-Source 'frmDebug.vb') 'DeviceDiagnostics' 'Debug form must enforce diagnostics permissions.'
Assert-Contains (Read-Source 'FrmLogQuery.vb') 'ExportLogs' 'Log export must enforce its role permission.'

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
$engineer = [Enum]::Parse($roleType, 'Engineer')
$administrator = [Enum]::Parse($roleType, 'Administrator')

$parseLine = $assembly.GetType('software.UserAccountStore', $true).GetMethod('ParseLine', [Reflection.BindingFlags] 'Public, Static')
$adminName = ([char]0x7CFB).ToString() + ([char]0x7EDF).ToString() + ([char]0x7BA1).ToString() + ([char]0x7406).ToString() + ([char]0x5458).ToString()
$operatorName = ([char]0x64CD).ToString() + ([char]0x4F5C).ToString() + ([char]0x5458).ToString()
$legacyAdmin = $parseLine.Invoke($null, @($adminName + ',hash'))
$legacyOperator = $parseLine.Invoke($null, @($operatorName + ',hash'))
$newAdmin = $parseLine.Invoke($null, @($adminName + ',hash,Administrator'))
$newEngineer = $parseLine.Invoke($null, @('engineer-user,hash,Engineer'))
$unknownRole = $parseLine.Invoke($null, @('temporary,hash,UnknownRole'))
$unknownAdminRole = $parseLine.Invoke($null, @($adminName + ',hash,UnknownRole'))
$numericUnknownRole = $parseLine.Invoke($null, @('numeric,hash,99'))
if ($legacyAdmin.Role.ToString() -ne 'Administrator') { throw 'Legacy administrator record did not map to Administrator.' }
if ($legacyOperator.Role.ToString() -ne 'Operator') { throw 'Legacy operator record did not map to Operator.' }
if ($newAdmin.Role.ToString() -ne 'Administrator') { throw 'Three-column administrator record did not map to Administrator.' }
if ($newEngineer.Role.ToString() -ne 'Engineer') { throw 'Three-column engineer record did not map to Engineer.' }
if ($unknownRole.Role.ToString() -ne 'Operator') { throw 'Unknown role did not safely map to Operator.' }
if ($unknownAdminRole.Role.ToString() -ne 'Operator') { throw 'Unknown role must not elevate the administrator username.' }
if ($numericUnknownRole.Role.ToString() -ne 'Operator') { throw 'Numeric unknown role must safely map to Operator.' }

foreach ($role in @($unauthenticated, $operator, $engineer, $administrator)) {
    $setSession.Invoke($null, @('test-user', $role)) | Out-Null
    $businessAllowed = [bool]$canAccess.Invoke($null, @($business))
    $adminAllowed = [bool]$canAccess.Invoke($null, @($adminPermission))
    if (($role -eq $unauthenticated) -eq $businessAllowed) { throw "Business permission mismatch for role $role." }
    if (($role -eq $administrator) -ne $adminAllowed) { throw "Administrator permission mismatch for role $role." }

    foreach ($permissionName in @('ManageTestStandards', 'CalibrateChannels', 'DeviceDiagnostics', 'ManageProcessParameters', 'ExportLogs')) {
        $permission = [Enum]::Parse($permissionType, $permissionName)
        $allowed = [bool]$canAccess.Invoke($null, @($permission))
        $shouldAllow = $role -eq $engineer -or $role -eq $administrator
        if ($allowed -ne $shouldAllow) { throw "$permissionName permission mismatch for role $role." }
    }

    foreach ($permissionName in @('ManageSystemSettings', 'UserManagement')) {
        $permission = [Enum]::Parse($permissionType, $permissionName)
        $allowed = [bool]$canAccess.Invoke($null, @($permission))
        if ($allowed -ne ($role -eq $administrator)) { throw "$permissionName permission mismatch for role $role." }
    }

    $unknownPermission = [Enum]::ToObject($permissionType, 999)
    if ([bool]$canAccess.Invoke($null, @($unknownPermission))) { throw "Unknown permissions must be denied for role $role." }
}

$userManageType = $assembly.GetType('software.frmUserManage', $true)
$userManageForm = [Activator]::CreateInstance($userManageType)
try {
    $roleCombo = $userManageType.GetProperty('ComboBoxRole', [Reflection.BindingFlags] 'Instance, Public, NonPublic').GetValue($userManageForm, $null)
    $selectedRoleMethod = $userManageType.GetMethod('SelectedRole', [Reflection.BindingFlags] 'Instance, NonPublic')
    foreach ($expectedRole in @(@{Index = 0; Name = 'Operator'}, @{Index = 1; Name = 'Engineer'}, @{Index = 2; Name = 'Administrator'})) {
        $roleCombo.SelectedIndex = $expectedRole.Index
        $actualRole = $selectedRoleMethod.Invoke($userManageForm, $null).ToString()
        if ($actualRole -ne $expectedRole.Name) { throw "Role selector index $($expectedRole.Index) mapped to $actualRole instead of $($expectedRole.Name)." }
    }
}
finally {
    $userManageForm.Dispose()
}

$initsetType = $assembly.GetType('software.Module1+Initset', $true)
$settings = [Activator]::CreateInstance($initsetType)
foreach ($fieldName in @('PLCIP', 'SCMIpAdd', 'strDatabase', 'strpwd', 'PsdOfAdmin', 'stationSelect', 'ChannelPassColorArgb', 'Dmm34461VoltageRange')) {
    $field = $initsetType.GetField($fieldName, [Reflection.BindingFlags] 'Instance, Public, NonPublic')
    if ($null -eq $field) { throw "Initset protected field $fieldName is missing." }
    $field.SetValue($settings, $(switch ($field.FieldType.FullName) {
        'System.String' { 'preserve-' + $fieldName }
        'System.Int32' { 17 }
        default { throw "Unsupported test field type for $fieldName." }
    }))
}
$originalProtectedValues = @{}
foreach ($fieldName in @('PLCIP', 'SCMIpAdd', 'strDatabase', 'strpwd', 'PsdOfAdmin', 'stationSelect', 'ChannelPassColorArgb', 'Dmm34461VoltageRange')) {
    $originalProtectedValues[$fieldName] = $initsetType.GetField($fieldName, [Reflection.BindingFlags] 'Instance, Public, NonPublic').GetValue($settings)
}
$profileType = $assembly.GetType('software.TestStandardProfile', $true)
$profile = [Activator]::CreateInstance($profileType)
$profileType.GetProperty('ResistanceMin').SetValue($profile, [decimal] 1.0, $null)
$profileType.GetProperty('ResistanceMax').SetValue($profile, [decimal] 3.0, $null)
$profileType.GetProperty('VoltageMin').SetValue($profile, [decimal] 2500, $null)
$profileType.GetProperty('VoltageMax').SetValue($profile, [decimal] 4200, $null)
$settingsPolicyType = $assembly.GetType('software.EngineerProcessSettingsPolicy', $true)
$applyEngineerSettings = $settingsPolicyType.GetMethod('ApplyTo', [Reflection.BindingFlags] 'NonPublic, Static')
if ($null -eq $applyEngineerSettings) { throw 'Engineer process settings whitelist policy is missing.' }
$updatedSettings = $applyEngineerSettings.Invoke($null, @($settings, $profile, [single] 1.25, [single] -2.5, [decimal] 250, 10000, 12, 24, [single] 3.75, 50))
foreach ($fieldName in $originalProtectedValues.Keys) {
    $actualValue = $initsetType.GetField($fieldName, [Reflection.BindingFlags] 'Instance, Public, NonPublic').GetValue($updatedSettings)
    if ($actualValue -ne $originalProtectedValues[$fieldName]) { throw "Engineer process save changed protected setting $fieldName." }
}
if ($initsetType.GetField('upResistance', [Reflection.BindingFlags] 'Instance, Public, NonPublic').GetValue($updatedSettings) -ne [single] 3.0) { throw 'Engineer process save did not update the approved resistance limit.' }
if ($initsetType.GetField('downVoltage', [Reflection.BindingFlags] 'Instance, Public, NonPublic').GetValue($updatedSettings) -ne [single] 2500) { throw 'Engineer process save did not update the approved voltage limit.' }

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
    $calibrationField = Find-ToolStripItem $form.MainMenuStrip.Items 'calibrationMenu'
    $logoutField = Find-ToolStripItem $form.MainMenuStrip.Items 'logoutMenu'
    foreach ($role in @($unauthenticated, $operator, $engineer, $administrator)) {
        $setSession.Invoke($null, @('test-user', $role)) | Out-Null
        $applyPermissions.Invoke($form, $null) | Out-Null
        $processEnabled = $initField.Enabled
        $engineerToolsEnabled = $debugField.Enabled -and $calibrationField.Enabled
        $userManageEnabled = $userManageField.Enabled
        if (($role -eq $engineer -or $role -eq $administrator) -ne $processEnabled) { throw "Process settings UI state mismatch for role $role." }
        if (($role -eq $engineer -or $role -eq $administrator) -ne $engineerToolsEnabled) { throw "Engineer tools UI state mismatch for role $role." }
        if (($role -eq $administrator) -ne $userManageEnabled) { throw "User management UI state mismatch for role $role." }
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

$frmInitType = $assembly.GetType('software.frmInit', $true)
$engineerSettingsForm = [Activator]::CreateInstance($frmInitType)
try {
    $applyRoleTabs = $frmInitType.GetMethod('ApplyRoleSpecificTabs', [Reflection.BindingFlags] 'Instance, NonPublic')
    $tabControl = $frmInitType.GetProperty('TabControl1', [Reflection.BindingFlags] 'Instance, Public, NonPublic').GetValue($engineerSettingsForm, $null)
    $setSession.Invoke($null, @('engineer-user', $engineer)) | Out-Null
    $applyRoleTabs.Invoke($engineerSettingsForm, $null) | Out-Null
    if ($tabControl.TabPages.Count -ne 1 -or $tabControl.TabPages[0].Name -ne 'TabPage2') { throw 'Engineer settings did not restrict the initial settings form to the process parameter page.' }
}
finally {
    $engineerSettingsForm.Dispose()
}

$adminSettingsForm = [Activator]::CreateInstance($frmInitType)
try {
    $setSession.Invoke($null, @('admin-user', $administrator)) | Out-Null
    $applyRoleTabs.Invoke($adminSettingsForm, $null) | Out-Null
    $adminTabControl = $frmInitType.GetProperty('TabControl1', [Reflection.BindingFlags] 'Instance, Public, NonPublic').GetValue($adminSettingsForm, $null)
    if ($adminTabControl.TabPages.Count -le 1) { throw 'Administrator did not retain full initial settings access.' }
}
finally {
    $adminSettingsForm.Dispose()
}

$resetSession.Invoke($null, $null) | Out-Null
Write-Output 'Authorization contract: PASS'
