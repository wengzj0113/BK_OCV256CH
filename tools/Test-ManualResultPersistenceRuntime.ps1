param(
    [Parameter(Mandatory=$true)][string]$ConnectionString,
    [string]$AssemblyPath = ''
)
$ErrorActionPreference='Stop'
if(!$AssemblyPath) { $AssemblyPath=Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) '..\bin\build-verify-manual-barcode\XL_OCV_256CH.exe' }
# Load the actual x86 .NET Framework application without starting its UI or reading application config.
if([IntPtr]::Size -ne 4) {
    $x86=Join-Path $env:WINDIR 'SysWOW64\WindowsPowerShell\v1.0\powershell.exe'
    & $x86 -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -ConnectionString $ConnectionString -AssemblyPath $AssemblyPath
    if($LASTEXITCODE -ne 0) { throw 'x86 persistence verification failed.' }
    exit 0
}
Add-Type -AssemblyName System.Data
$builder=New-Object Data.SqlClient.SqlConnectionStringBuilder($ConnectionString)
if(!$builder.DataSource.StartsWith('(localdb)\',[StringComparison]::OrdinalIgnoreCase) -or
   $builder.DataSource -eq '(localdb)\MSSQLLocalDB' -or $builder.InitialCatalog -ne 'OCV_DATA') {
    throw 'Use an explicit dedicated named LocalDB instance and the disposable OCV_DATA database.'
}
$resolvedAssembly=(Resolve-Path -LiteralPath $AssemblyPath).Path
$assembly=[Reflection.Assembly]::LoadFrom($resolvedAssembly)
$flags=[Reflection.BindingFlags]'Public,NonPublic,Static,Instance'
$types=$assembly.GetTypes()
$planType=$types | Where-Object Name -eq 'ManualTestBarcodePlan' | Select-Object -First 1
$contextType=$types | Where-Object Name -eq 'ManualTestRunContext' | Select-Object -First 1
$roundType=$types | Where-Object Name -eq 'ManualTestRoundSnapshot' | Select-Object -First 1
$serviceType=$types | Where-Object Name -eq 'ManualTestResultService' | Select-Object -First 1
$measurementType=$types | Where-Object { $_.Name -eq 'Bat_info' -and $_.DeclaringType.Name -eq 'Module1' } | Select-Object -First 1
if(!$planType -or !$contextType -or !$roundType -or !$serviceType -or !$measurementType) { throw 'Required application persistence types not found.' }
$createPlan=$planType.GetMethod('Create',$flags)
$capture=$roundType.GetMethod('Capture',$flags)
$save=$serviceType.GetMethod('SaveRounds',$flags)
$listType=[System.Collections.Generic.List``1].MakeGenericType($roundType)
$prefix='RUNTIME-MANUAL-'+[Guid]::NewGuid().ToString('N')
$realTray=$prefix+'-REAL'
$debugTray=$prefix+'-DEBUG'
$code=$prefix+'-CELL'
$connection=New-Object Data.SqlClient.SqlConnection($ConnectionString)
$connection.Open()
function Query([string]$sql) {
    $command=$connection.CreateCommand()
    $command.CommandText=$sql
    [void]$command.Parameters.AddWithValue('@real',$realTray)
    [void]$command.Parameters.AddWithValue('@debug',$debugTray)
    [void]$command.Parameters.AddWithValue('@code',$code)
    $table=New-Object Data.DataTable
    $reader=$command.ExecuteReader()
    try { $table.Load($reader) } finally { $reader.Dispose();$command.Dispose() }
    return ,$table
}
function Assert($condition,[string]$message) { if(!$condition) { throw $message } }
function Measurements([single]$voltage,[double]$raw,[double]$cal,[double]$final) {
    $array=[Array]::CreateInstance($measurementType,256)
    $item=[Activator]::CreateInstance($measurementType)
    foreach($entry in @{Voltage=$voltage;Resistance=[single]$final;RawResistanceMilliohm=$raw;CalibrationValueMilliohm=$cal;FinalResistanceMilliohm=$final}.GetEnumerator()) {
        $measurementType.GetField($entry.Key,$flags).SetValue($item,$entry.Value)
    }
    $array.SetValue($item,0)
    return ,$array
}
function Context($plan) {
    $args=[object[]]@($plan.psobject.BaseObject,[int[]]@(1),1,'PersistenceVerifier',[datetime]::Now,'','','',$false,'','','',0.0,5000.0,0.0,10.0)
    return $contextType.GetConstructors()[0].Invoke($args)
}
try {
    # Real tray mappings are pre-existing fixture data, as with a registered production tray.
    [void](Query "INSERT dbo.Tray(TrayCode,SourceType) VALUES(@real,N'RuntimeVerification'); INSERT dbo.TrayCell(TrayId,ChannelNo,RowNo,ColNo,BatteryCode,SourceType) SELECT TrayId,1,1,1,@code,N'RuntimeVerification' FROM dbo.Tray WHERE TrayCode=@real;")
    $cells=New-Object Data.DataTable
    [void]$cells.Columns.Add('ChannelNo',[int]);[void]$cells.Columns.Add('BatteryCode',[string])
    [void]$cells.Rows.Add(1,$code)
    $plan=$createPlan.Invoke($null,[object[]]@($realTray,$cells.psobject.BaseObject,[datetime]::Now))
    $context=Context $plan
    $rounds=[Activator]::CreateInstance($listType)
    $first=Measurements 3.5 1.0 0.1 1.1
    $second=Measurements 3.75 2.0 0.25 2.25
    $rounds.Add($capture.Invoke($null,[object[]]@($context.psobject.BaseObject,1,$first.psobject.BaseObject,[datetime]::Now,[datetime]::Now)))
    $rounds.Add($capture.Invoke($null,[object[]]@($context.psobject.BaseObject,2,$second.psobject.BaseObject,[datetime]::Now,[datetime]::Now)))
    $save.Invoke($null,[object[]]@($rounds.psobject.BaseObject,$ConnectionString))
    $tasks=Query 'SELECT TestTaskId,TestType,Status,OperatorName FROM dbo.TestTask WHERE TrayCode=@real;'
    Assert ($tasks.Rows.Count -eq 2) 'Real SaveRounds did not create two tasks.'
    foreach($task in $tasks.Rows) { Assert ($task.TestType -eq 'Manual' -and $task.Status -eq 'Completed') 'Real round task type/status is incorrect.' }
    $results=Query 'SELECT tr.*,tt.TaskCode FROM dbo.TestResult tr JOIN dbo.TestTask tt ON tt.TestTaskId=tr.TestTaskId WHERE tr.BatteryCode=@code;'
    Assert ($results.Rows.Count -eq 1) 'Repeated real barcode did not have exactly one current result.'
    $result=$results.Rows[0]
    Assert ($result.VoltageVolt -eq [decimal]3.75 -and $result.RawResistanceMilliohm -eq [decimal]2 -and $result.CalibrationValueMilliohm -eq [decimal]0.25 -and $result.FinalResistanceMilliohm -eq [decimal]2.25) 'Current real result is not the final round measurement/calibration.'
    Assert ($result.ResultSource -eq 'ManualTest' -and $result.TaskCode.EndsWith('-2')) 'Real result has incorrect source or round task.'
    $mappings=Query 'SELECT BatteryCode FROM dbo.TrayCell WHERE TrayId=(SELECT TrayId FROM dbo.Tray WHERE TrayCode=@real);'
    Assert ($mappings.Rows.Count -eq 1 -and $mappings.Rows[0].BatteryCode -eq $code) 'Real tray mapping changed.'
    Write-Output 'PASS: actual SaveRounds saves two completed Manual tasks and one latest ManualTest barcode result with final calibration values.'

    $empty=$cells.Clone()
    $debugPlan=$createPlan.Invoke($null,[object[]]@($debugTray,$empty.psobject.BaseObject,[datetime]::Now))
    Assert ($debugPlan.IsSimulated) 'Empty plan was not simulated.'
    $debugContext=Context $debugPlan
    $debugRounds=[Activator]::CreateInstance($listType)
    $debugRounds.Add($capture.Invoke($null,[object[]]@($debugContext.psobject.BaseObject,1,$second.psobject.BaseObject,[datetime]::Now,[datetime]::Now)))
    $save.Invoke($null,[object[]]@($debugRounds.psobject.BaseObject,$ConnectionString))
    $debugResults=Query 'SELECT tr.BatteryCode,tr.ResultSource,tt.TestType,tt.Status FROM dbo.TestResult tr JOIN dbo.TestTask tt ON tt.TestTaskId=tr.TestTaskId WHERE tr.TrayCode=@debug;'
    Assert ($debugResults.Rows.Count -eq 1) 'Simulated result count is incorrect.'
    $debug=$debugResults.Rows[0]
    Assert ($debug.BatteryCode.StartsWith('TEST-') -and $debug.ResultSource -eq 'Debug' -and $debug.TestType -eq 'Debug' -and $debug.Status -eq 'Completed') 'Simulated task/result classification is incorrect.'
    $debugCells=Query 'SELECT tc.* FROM dbo.TrayCell tc JOIN dbo.Tray t ON t.TrayId=tc.TrayId WHERE t.TrayCode=@debug;'
    Assert ($debugCells.Rows.Count -eq 0) 'Simulated barcode was registered into TrayCell.'
    Write-Output 'PASS: actual empty-plan SaveRounds saves a completed Debug task/Debug result and registers no simulated TrayCell.'
    Write-Output ('PASS: x86 application reflection persistence completed against '+$builder.DataSource+'.')
} catch {
    $failure=$_.Exception
    while($failure.InnerException) { $failure=$failure.InnerException }
    throw $failure
} finally {
    # The application service commits internally, so remove only this harness unique fixtures.
    [void](Query 'DELETE FROM dbo.OCV_RETEST_DATA WHERE BatteryCode=@code; DELETE FROM dbo.TestResult WHERE TrayCode IN(@real,@debug); DELETE FROM dbo.TestTask WHERE TrayCode IN(@real,@debug); DELETE tc FROM dbo.TrayCell tc JOIN dbo.Tray t ON t.TrayId=tc.TrayId WHERE t.TrayCode IN(@real,@debug); DELETE FROM dbo.Tray WHERE TrayCode IN(@real,@debug); DELETE FROM dbo.OperatorLog WHERE Detail LIKE N''%''+@real+N''%'' OR Detail LIKE N''%''+@debug+N''%'';')
    $connection.Dispose()
}