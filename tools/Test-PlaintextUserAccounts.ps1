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

$accountStore = Read-Source 'UserAccountStore.vb'
$login = Read-Source 'FrmLoginNew.vb'

Assert-Contains $accountStore 'Public Property Password As String' 'UserAccount must expose a plaintext Password property.'
Assert-Contains $accountStore 'account\.Password\s*=\s*password' 'Authentication must compare the stored password directly.'
Assert-Contains $accountStore '\.Password\s*=\s*password' 'Account updates must store the supplied password directly.'
Assert-NotContains $accountStore 'PasswordHash' 'UserAccountStore must not retain hash-named storage.'
Assert-NotContains $accountStore 'HashPassword' 'UserAccountStore must not call the hash helper.'
Assert-NotContains $accountStore 'System\.Security\.Cryptography' 'UserAccountStore must not depend on cryptography for account passwords.'
Assert-NotContains $login 'HashPassword' 'Login form must not hash account passwords.'
Assert-NotContains $login 'System\.Security\.Cryptography' 'Login form must not import cryptography for account passwords.'

if ($StaticOnly) {
    Write-Output 'Plaintext account static contract: PASS'
    exit 0
}

if (-not (Test-Path -LiteralPath $AssemblyPath)) { throw "Assembly not found: $AssemblyPath" }

$assemblyDirectory = Split-Path -Parent (Resolve-Path -LiteralPath $AssemblyPath)
$tempDirectory = Join-Path ([IO.Path]::GetTempPath()) ('xl-ocv-plaintext-test-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $tempDirectory | Out-Null

try {
    Get-ChildItem -LiteralPath $assemblyDirectory -File | Where-Object { $_.Extension -in @('.exe', '.dll', '.config') } | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $tempDirectory $_.Name)
    }

    $testAssemblyPath = Join-Path $tempDirectory (Split-Path -Leaf $AssemblyPath)
    $assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $testAssemblyPath))
    $storeType = $assembly.GetType('software.UserAccountStore', $true)
    $roleType = $assembly.GetType('software.UserRole', $true)
    $parseLine = $storeType.GetMethod('ParseLine', [Reflection.BindingFlags] 'Public, Static')
    $ensureFile = $storeType.GetMethod('EnsureFile', [Reflection.BindingFlags] 'Public, Static')
    $addOrUpdate = $storeType.GetMethod('AddOrUpdate', [Reflection.BindingFlags] 'Public, Static')
    $authenticate = $storeType.GetMethod('Authenticate', [Reflection.BindingFlags] 'Public, Static')
    $loadAccounts = $storeType.GetMethod('LoadAccounts', [Reflection.BindingFlags] 'Public, Static')
    $filePath = $storeType.GetProperty('FilePath', [Reflection.BindingFlags] 'Public, Static')
    $userDataPath = [string]$filePath.GetValue($null, $null)

    $operatorRole = [Enum]::Parse($roleType, 'Operator')
    $parsed = $parseLine.Invoke($null, @('test-user,plain-pass,Operator'))
    if ($parsed.Password -ne 'plain-pass') { throw 'ParseLine did not preserve the plaintext password.' }

    if (Test-Path -LiteralPath $userDataPath) { Remove-Item -LiteralPath $userDataPath -Force }
    $ensureFile.Invoke($null, $null) | Out-Null
    $addOrUpdate.Invoke($null, @('test-user', 'plain-pass', $operatorRole)) | Out-Null

    $authenticated = $authenticate.Invoke($null, @('test-user', 'plain-pass'))
    if ($null -eq $authenticated) { throw 'Plaintext password authentication failed.' }
    $rejected = $authenticate.Invoke($null, @('test-user', 'wrong-pass'))
    if ($null -ne $rejected) { throw 'Wrong plaintext password was accepted.' }

    $savedLine = Get-Content -LiteralPath $userDataPath | Where-Object { $_ -match '^test-user,' } | Select-Object -First 1
    if ([string]::IsNullOrWhiteSpace($savedLine)) { throw 'Updated test account was not written.' }
    $savedParts = $savedLine -split ','
    if ($savedParts.Count -lt 3 -or $savedParts[1] -ne 'plain-pass') { throw 'UserData.txt did not preserve the plaintext password.' }
    if ($savedParts[1] -match '^[0-9a-f]{64}$') { throw 'UserData.txt still contains a hash for the test account.' }

    $loaded = $loadAccounts.Invoke($null, $null)
    $loadedTest = $loaded | Where-Object { $_.UserName -eq 'test-user' } | Select-Object -First 1
    if ($null -eq $loadedTest -or $loadedTest.Password -ne 'plain-pass') { throw 'Loaded account did not expose the plaintext password.' }
}
finally {
    Remove-Item -LiteralPath $tempDirectory -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Output 'Plaintext account contract: PASS'
