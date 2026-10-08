param(
    [Parameter(Mandatory = $true)]
    [string]$AssemblyPath
)

$ErrorActionPreference = 'Stop'
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $AssemblyPath).Path)
$policyType = $assembly.GetType('software.InitSettingsApplyPolicy', $false)
if ($null -eq $policyType) {
    throw 'software.InitSettingsApplyPolicy is not present in the application assembly.'
}
$kindType = $assembly.GetType('software.InitSettingsChangeKind', $true)
$initsetType = $assembly.GetType('software.Module1+Initset', $true)
$classify = $policyType.GetMethod('Classify', [Reflection.BindingFlags]'Public,Static')
if ($null -eq $classify) { throw 'InitSettingsApplyPolicy.Classify was not found.' }

function New-Settings {
    return [Activator]::CreateInstance($initsetType)
}

function Set-Setting($settings, [string]$name, $value) {
    $field = $initsetType.GetField($name, [Reflection.BindingFlags]'Instance,Public,NonPublic')
    if ($null -eq $field) { throw "Initset field '$name' was not found." }
    $field.SetValue($settings, $value)
    return ,$settings
}

function Assert-HasKind($actual, [string]$name, [bool]$expected) {
    $flag = [int][Enum]::Parse($kindType, $name)
    $hasFlag = (([int]$actual -band $flag) -eq $flag)
    if ($hasFlag -ne $expected) { throw "Expected category '$name' presence=$expected, actual=$actual." }
}

function Assert-Policy([string]$methodName, [string]$fieldName, $value, [bool]$expected) {
    $method = $policyType.GetMethod($methodName, [Reflection.BindingFlags]'Public,Static')
    if ($null -eq $method) { throw "InitSettingsApplyPolicy.$methodName was not found." }
    $changed = Set-Setting (New-Settings) $fieldName $value
    $actual = [bool]$method.Invoke($null, [object[]]@((New-Settings), $changed))
    if ($actual -ne $expected) { throw "$methodName for '$fieldName' expected $expected, got $actual." }
}

$baseline = New-Settings
$none = $classify.Invoke($null, [object[]]@($baseline, $baseline))
if ([int]$none -ne 0) { throw "Unchanged settings must classify as None, got $none." }

$display = Set-Setting $baseline 'ChannelPassColorArgb' 12345
$displayKind = $classify.Invoke($null, [object[]]@((New-Settings), $display))
Assert-HasKind $displayKind 'Display' $true
Assert-HasKind $displayKind 'TestRules' $false
Assert-HasKind $displayKind 'DeviceConnection' $false

$rules = Set-Setting (New-Settings) 'upVoltage' ([single]4200)
$rulesKind = $classify.Invoke($null, [object[]]@((New-Settings), $rules))
Assert-HasKind $rulesKind 'TestRules' $true
Assert-HasKind $rulesKind 'DeviceConnection' $false

$connection = Set-Setting (New-Settings) 'SCMComPort' 2
$connectionKind = $classify.Invoke($null, [object[]]@((New-Settings), $connection))
Assert-HasKind $connectionKind 'DeviceConnection' $true
Assert-HasKind $connectionKind 'Instrument' $false

$instrument = Set-Setting (New-Settings) 'Dmm34461Nplc' '1'
$instrumentKind = $classify.Invoke($null, [object[]]@((New-Settings), $instrument))
Assert-HasKind $instrumentKind 'Instrument' $true
Assert-HasKind $instrumentKind 'DeviceConnection' $false

$external = Set-Setting (New-Settings) 'MesApiAddress' 'http://127.0.0.1/api'
$externalKind = $classify.Invoke($null, [object[]]@((New-Settings), $external))
Assert-HasKind $externalKind 'ExternalServices' $true

$station = Set-Setting (New-Settings) 'stationSelect' 2
$stationKind = $classify.Invoke($null, [object[]]@((New-Settings), $station))
Assert-HasKind $stationKind 'Station' $true

Assert-Policy 'RequiresScmRecheck' 'SCMComRate' 57600 $true
Assert-Policy 'RequiresScmRecheck' 'ScanerRate' 19200 $false
Assert-Policy 'RequiresDmmRecheck' 'Dmm34461Nplc' '1' $true
Assert-Policy 'RequiresDmmRecheck' 'Main3562Function' 'RES' $false
Assert-Policy 'RequiresMain3562Recheck' 'Main3562SampleRate' 'MED' $true
Assert-Policy 'RequiresRetest3562Recheck' 'Retest3562Port' 5 $true
Assert-Policy 'RequiresPlcClientRestart' 'PLCComPort' 2 $true
Assert-Policy 'RequiresScannerClientRestart' 'ScanerPort' 3 $true
Assert-Policy 'RequiresTemperatureClientReset' 'TemperatureComRate' 19200 $true
Assert-Policy 'RequiresDatabaseRecheck' 'strDatabase' 'Data Source=local' $true
Assert-Policy 'RequiresMesRecheck' 'MesApiAddress' 'http://localhost/api' $true

Write-Output 'Init settings change classification: PASS'
