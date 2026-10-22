param(
    [string]$AssemblyPath = (Join-Path $PSScriptRoot '..\bin\build-verify-r160-r151\XL_OCV_256CH.exe'),
    [string]$SqlServer = '.'
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $AssemblyPath).Path)
$inputType = $assembly.GetType('software.OcvTestResultInput', $true)
$serviceType = $assembly.GetType('software.OcvDataService', $true)
$listType = [System.Collections.Generic.List``1].MakeGenericType($inputType)
$connectionString = "Data Source=$SqlServer;Initial Catalog=OCV_DATA;Integrated Security=True;Connect Timeout=3;Encrypt=False"
$service = [Activator]::CreateInstance($serviceType, [object[]]@($connectionString))
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$connection.Open()
try {
    $input = [Activator]::CreateInstance($inputType)
    $input.ChannelNo = 7
    $input.BatteryCode = 'NUMERIC-RANGE-PROBE'
    $input.VoltageVolt = [decimal]3.5
    $input.RawResistanceMilliohm = [decimal]10000
    $input.CalibrationValueMilliohm = [decimal]0
    $input.FinalResistanceMilliohm = [decimal]10000
    $input.IsNg = $true
    $input.ResultSource = 'FormalTest'
    $items = [Activator]::CreateInstance($listType)
    $items.Add($input)
    $build = $serviceType.GetMethod('BuildTestResultTable', [Reflection.BindingFlags]'Static,NonPublic')
    $buildArgs = New-Object object[] 1
    $buildArgs[0] = $items
    $table = $build.Invoke($null, $buildArgs)
    $command = $connection.CreateCommand()
    $command.CommandText = 'SELECT COUNT(*) FROM @Results'
    $parameter = $command.Parameters.Add('@Results', [System.Data.SqlDbType]::Structured)
    $parameter.TypeName = 'dbo.TestResultInputType'
    $parameter.Value = $table
    try {
        if ($command.ExecuteScalar() -ne 1) { throw 'The normal 10000 mOhm result was not accepted.' }
    } finally { $command.Dispose() }

    # This input cannot fit decimal(18,6). SQL rejects the TVP before executing
    # the procedure. The nonexistent task ID also prevents persistent writes.
    $input.RawResistanceMilliohm = [decimal]1000000000000
    $saveArgs = New-Object object[] 2
    $saveArgs[0] = [long]-1
    $saveArgs[1] = $items
    $failure = $null
    try { $null = $serviceType.GetMethod('SaveTestResults').Invoke($service, $saveArgs) }
    catch { $failure = $_.Exception.InnerException }
    if ($null -eq $failure) { throw 'The oversized result unexpectedly succeeded.' }
    foreach ($required in @('ChannelNo=7', 'RawResistanceMilliohm=1000000000000', 'decimal(18,6)')) {
        if (-not $failure.Message.Contains($required)) { throw "Numeric overflow must identify $required. Actual: $($failure.Message)" }
    }
    if ($failure.InnerException -isnot [System.Data.SqlClient.SqlException]) { throw 'The original SQL exception must be preserved.' }
    Write-Output 'PASS: 10000 mOhm is accepted; overflow identifies channel, field, value and actual SQL precision without database writes.'
} finally {
    $connection.Dispose()
    $service.Dispose()
}
