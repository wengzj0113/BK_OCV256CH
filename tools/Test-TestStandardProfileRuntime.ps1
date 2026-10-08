param(
    [Parameter(Mandatory = $true)]
    [string]$AssemblyPath
)

$ErrorActionPreference = 'Stop'
$resolvedAssemblyPath = (Resolve-Path -LiteralPath $AssemblyPath).Path
$assembly = [System.Reflection.Assembly]::LoadFrom($resolvedAssemblyPath)
$profileType = $assembly.GetType('software.TestStandardProfile', $false)
if ($null -eq $profileType) {
    throw 'software.TestStandardProfile is not present in the application assembly.'
}
$storeType = $assembly.GetType('software.TestStandardFileStore', $false)
if ($null -eq $storeType) {
    throw 'software.TestStandardFileStore is not present in the application assembly.'
}

function Assert-True([bool]$condition, [string]$message) {
    if (-not $condition) { throw $message }
}

function Invoke-ExpectFailure([scriptblock]$action, [string]$message) {
    $failed = $false
    try { & $action } catch { $failed = $true }
    if (-not $failed) { throw $message }
}

function New-TestProfile([decimal]$resistanceMin, [decimal]$resistanceMax, [decimal]$voltageMin, [decimal]$voltageMax) {
    $profile = [Activator]::CreateInstance($profileType)
    $profileType.GetProperty('ResistanceMin').SetValue($profile, $resistanceMin, $null)
    $profileType.GetProperty('ResistanceMax').SetValue($profile, $resistanceMax, $null)
    $profileType.GetProperty('VoltageMin').SetValue($profile, $voltageMin, $null)
    $profileType.GetProperty('VoltageMax').SetValue($profile, $voltageMax, $null)
    return $profile
}

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('ocv-test-standard-' + [Guid]::NewGuid().ToString('N'))
try {
    $storeConstructor = $storeType.GetConstructor([type[]]@([string]))
    $constructorArgs = New-Object object[] 1
    $constructorArgs[0] = [string]$tempRoot
    $store = $storeConstructor.Invoke($constructorArgs)
    $directoryPath = $storeType.GetProperty('DirectoryPath').GetValue($store, $null)
    Assert-True ($directoryPath -eq (Join-Path $tempRoot 'SETS')) 'The profile store must resolve standards under the application SETS directory.'

    $profile = New-TestProfile ([decimal]3.25) ([decimal]80.5) ([decimal]2500) ([decimal]4200)
    $savedPath = $storeType.GetMethod('Save').Invoke($store, @($profile, 'Cell-A', $false))
    Assert-True (Test-Path -LiteralPath $directoryPath -PathType Container) 'Saving a profile must create the SETS directory.'
    Assert-True ([System.IO.Path]::GetFileName($savedPath) -eq 'Cell-A.set') 'Saving must append the .set extension.'

    $text = [System.IO.File]::ReadAllText($savedPath, [System.Text.Encoding]::UTF8)
    foreach ($expectedLine in @(
        'Format=XL-OCV-TestStandard',
        'Version=1',
        'ResistanceMinMilliOhm=3.25',
        'ResistanceMaxMilliOhm=80.5',
        'VoltageMinMilliVolt=2500',
        'VoltageMaxMilliVolt=4200'
    )) {
        Assert-True ($text.Contains($expectedLine)) "Saved standard is missing '$expectedLine'."
    }
    Assert-True (-not $text.Contains('3,25')) 'Serialized decimal values must use invariant decimal separators.'

    $loaded = $storeType.GetMethod('Read').Invoke($store, @($savedPath))
    foreach ($propertyName in @('ResistanceMin', 'ResistanceMax', 'VoltageMin', 'VoltageMax')) {
        $expected = $profileType.GetProperty($propertyName).GetValue($profile, $null)
        $actual = $profileType.GetProperty($propertyName).GetValue($loaded, $null)
        Assert-True ([decimal]$actual -eq [decimal]$expected) "Round-trip changed $propertyName."
    }

    Invoke-ExpectFailure { $storeType.GetMethod('Save').Invoke($store, @($profile, 'Cell-A', $false)) | Out-Null } 'Saving an existing standard without overwrite permission must fail.'
    $replacement = New-TestProfile ([decimal]4) ([decimal]90) ([decimal]2400) ([decimal]4300)
    $storeType.GetMethod('Save').Invoke($store, @($replacement, 'Cell-A', $true)) | Out-Null
    $reloaded = $storeType.GetMethod('Read').Invoke($store, @($savedPath))
    Assert-True ([decimal]$profileType.GetProperty('ResistanceMin').GetValue($reloaded, $null) -eq [decimal]4) 'Overwrite must replace the previous profile.'

    foreach ($invalid in @(
        (New-TestProfile ([decimal]-1) ([decimal]10) ([decimal]0) ([decimal]10)),
        (New-TestProfile ([decimal]10) ([decimal]9) ([decimal]0) ([decimal]10)),
        (New-TestProfile ([decimal]0) ([decimal]10) ([decimal]11) ([decimal]10))
    )) {
        Invoke-ExpectFailure { $storeType.GetMethod('Save').Invoke($store, @($invalid, 'Invalid', $true)) | Out-Null } 'Invalid profile values must be rejected.'
    }

    $badFiles = @{
        'missing.set' = "Format=XL-OCV-TestStandard`nVersion=1`nResistanceMinMilliOhm=0`nResistanceMaxMilliOhm=1`nVoltageMinMilliVolt=0`n"
        'duplicate.set' = "Format=XL-OCV-TestStandard`nVersion=1`nResistanceMinMilliOhm=0`nResistanceMinMilliOhm=0`nResistanceMaxMilliOhm=1`nVoltageMinMilliVolt=0`nVoltageMaxMilliVolt=1`n"
        'version.set' = "Format=XL-OCV-TestStandard`nVersion=9`nResistanceMinMilliOhm=0`nResistanceMaxMilliOhm=1`nVoltageMinMilliVolt=0`nVoltageMaxMilliVolt=1`n"
        'number.set' = "Format=XL-OCV-TestStandard`nVersion=1`nResistanceMinMilliOhm=oops`nResistanceMaxMilliOhm=1`nVoltageMinMilliVolt=0`nVoltageMaxMilliVolt=1`n"
        'inverted.set' = "Format=XL-OCV-TestStandard`nVersion=1`nResistanceMinMilliOhm=2`nResistanceMaxMilliOhm=1`nVoltageMinMilliVolt=0`nVoltageMaxMilliVolt=1`n"
        'corrupt.set' = "not a standard file"
    }
    foreach ($entry in $badFiles.GetEnumerator()) {
        $badPath = Join-Path $directoryPath $entry.Key
        [System.IO.File]::WriteAllText($badPath, $entry.Value, (New-Object System.Text.UTF8Encoding($false)))
        Invoke-ExpectFailure { $storeType.GetMethod('Read').Invoke($store, @($badPath)) | Out-Null } "Malformed file '$($entry.Key)' must be rejected."
    }

    Write-Output 'Test standard profile runtime verification passed.'
} finally {
    if (Test-Path -LiteralPath $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force
    }
}
