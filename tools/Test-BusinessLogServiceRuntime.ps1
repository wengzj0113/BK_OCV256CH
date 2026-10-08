param(
    [Parameter(Mandatory = $true)]
    [string]$AssemblyPath
)

$ErrorActionPreference = 'Stop'
$resolvedAssemblyPath = (Resolve-Path -LiteralPath $AssemblyPath).Path
$assembly = [System.Reflection.Assembly]::LoadFrom($resolvedAssemblyPath)
$serviceType = $assembly.GetType('software.BusinessLogService', $true)
$entryType = $assembly.GetType('software.BusinessLogEntry', $true)
$nonPublicStatic = [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::NonPublic

function Assert-True([bool]$condition, [string]$message) {
    if (-not $condition) { throw $message }
}

$parseAlarm = $serviceType.GetMethod('ParseAlarmEntry', $nonPublicStatic)
$isDuplicate = $serviceType.GetMethod('IsDuplicateAlarm', $nonPublicStatic)
$csvCell = $serviceType.GetMethod('CsvCell', $nonPublicStatic)
Assert-True ($null -ne $parseAlarm) 'Alarm text parsing must be centralized in ParseAlarmEntry.'
Assert-True ($null -ne $isDuplicate) 'Legacy alarm deduplication must use IsDuplicateAlarm.'
Assert-True ($null -ne $csvCell) 'CSV values must pass through CsvCell.'

$timestampText = [DateTime]::Now.AddSeconds(-1).ToString('yyyy-MM-dd HH:mm:ss', [Globalization.CultureInfo]::InvariantCulture)
$expectedTime = [DateTime]::ParseExact($timestampText, 'yyyy-MM-dd HH:mm:ss', [Globalization.CultureInfo]::InvariantCulture)
$newAlarmText = $timestampText + ':报警发生：急停报警'
$legacyAlarmText = $timestampText + ': ' + $timestampText + ':报警发生：急停报警'
$fallbackTime = [DateTime]::Now
$newEntry = $parseAlarm.Invoke($null, @($newAlarmText, $fallbackTime))
$legacyEntry = $parseAlarm.Invoke($null, @($legacyAlarmText, $fallbackTime))

Assert-True ($newEntry.Summary -eq '报警发生：急停报警') 'New alarm parsing must remove the entire timestamp, including its time-of-day colons.'
Assert-True ($legacyEntry.Summary -eq $newEntry.Summary) 'Legacy and structured alarm parsing must produce the same summary.'
Assert-True ($newEntry.OccurredAt -eq $expectedTime) 'Structured alarm parsing must retain the source timestamp.'
Assert-True ($legacyEntry.OccurredAt -eq $expectedTime) 'Legacy duplicate parsing must retain the first source timestamp.'

$entryListType = [System.Collections.Generic.List``1].MakeGenericType($entryType)
$entries = [Activator]::CreateInstance($entryListType)
$entries.Add($newEntry)
$duplicate = $isDuplicate.Invoke($null, @($entries, $legacyEntry))
Assert-True ([bool]$duplicate) 'A structured alarm and its legacy text row must be recognized as one event.'

foreach ($formula in @('=1+1', '+1+1', '-1+1', '@SUM(A1:A2)')) {
    $cell = [string]$csvCell.Invoke($null, @($formula))
    $safePrefix = [string][char]34 + [string][char]39
    Assert-True ($cell.StartsWith($safePrefix, [StringComparison]::Ordinal)) "CSV formula prefix '$formula' must be exported as text."
}

$quotedCell = [string]$csvCell.Invoke($null, @('text, "quoted"'))
Assert-True ($quotedCell -eq '"text, ""quoted"""') 'CSV comma and quote escaping must remain valid after formula protection.'

Write-Output 'Business log service runtime verification passed.'
