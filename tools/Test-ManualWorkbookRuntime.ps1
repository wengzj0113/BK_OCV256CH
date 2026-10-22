param([string]$AssemblyPath = (Join-Path $PSScriptRoot '..\bin\build-verify-manual-barcode\XL_OCV_256CH.exe'))
$ErrorActionPreference = 'Stop'
$writer = $null
$excel = $null
$book = $null
$books = $null
$sheets = $null
$sheet = $null
$range = $null
$path = Join-Path $env:TEMP ('codex-barcode-writer-' + [guid]::NewGuid().ToString('N') + '.xlsx')
try {
 $assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $AssemblyPath).Path)
 $rowType = $assembly.GetTypes() | Where-Object { $_.Name -eq 'ManualTestWorkbookRow' }
 $writerType = $assembly.GetTypes() | Where-Object { $_.Name -eq 'ManualTestWorkbookWriter' }
 $constructor = $writerType.GetConstructor([type[]]@([string], [datetime]))
 $writer = $constructor.Invoke([object[]]@([string]$path, [datetime]'2026-10-19'))
 $listType = [System.Collections.Generic.List``1].MakeGenericType($rowType)
 for ($round=1; $round -le 3; $round++) {
  $rows = [Activator]::CreateInstance($listType)
  for ($channel=1; $channel -le 2; $channel++) {
   $row = [Activator]::CreateInstance($rowType)
   $row.ChannelNumber = $channel
   $row.BatteryCode = if ($channel -eq 1) { '000012345678901234567890' } else { '=1+1' }
   $row.GroupText = 'A'
   $row.VoltageMillivolts = 1000 + 100 * $channel + 10 * $round
   $row.ResistanceMilliohms = 1 + $channel + 0.25 * $round
   $row.ResultText = 'OK'
   $rows.Add($row)
  }
  $writer.AddRound($round, [datetime]'2026-10-19T12:34:56', $rows)
 }
 $writer.Dispose()
 $writer = $null
 $excel = New-Object -ComObject Excel.Application
 $excel.Visible = $false
 $excel.DisplayAlerts = $false
 $books = $excel.Workbooks
 $book = $books.Open($path)
 $sheets = $book.Worksheets
 if ($sheets.Count -ne 4) { throw 'Expected exactly 4 sheets.' }
 foreach ($name in @('20261019第1次','20261019第2次','20261019第3次','汇总')) {
  $sheet = $sheets.Item($name)
  $codeColumn = if ($name -eq '汇总') { 'J' } else { 'G' }
  foreach ($n in @(2,3)) {
   $range = $sheet.Range($codeColumn + $n)
   $expected = if ($n -eq 2) { '000012345678901234567890' } else { '=1+1' }
   if ($range.Value2 -cne $expected -or $range.HasFormula -or $range.NumberFormat -cne '@') { throw ('Barcode mismatch: ' + $name + ' ' + $codeColumn + $n) }
   [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($range)
   $range = $null
  }
  if ($name -eq '汇总') {
   $range = $sheet.Range('C2','I3')
   $stats = $range.Value2
   foreach ($i in @(1,2)) {
    $expectedStats = @(3, (1010 + 100 * $i), (1030 + 100 * $i), 20, (1.25 + $i), (1.75 + $i), 0.5)
    for ($j=1; $j -le 7; $j++) {
     $actualValue = $stats.GetValue($i,$j)
     $expectedValue = $expectedStats[$j - 1]
     if ([math]::Abs([double]$actualValue - [double]$expectedValue) -gt 0.000001) { throw ('Statistics mismatch ' + $i + ',' + $j) }
    }
   }
   [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($range)
   $range = $null
  }
  [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($sheet)
  $sheet = $null
  Write-Output ('PASS: saved sheet ' + $name + ' preserves both barcode strings as text.')
 }
 Write-Output 'PASS: actual x86 writer AddRound three times, saved/reopened workbook, 4 sheets, both channels summary round count/min/max/deviations verified.'
} finally {
 if ($null -ne $writer) { $writer.Dispose() }
 if ($null -ne $book) { $book.Close($false) }
 if ($null -ne $excel) { $excel.Quit() }
 foreach ($obj in @($range,$sheet,$sheets,$book,$books,$excel)) {
  if ($null -ne $obj -and [Runtime.InteropServices.Marshal]::IsComObject($obj)) { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($obj) }
 }
 if (Test-Path -LiteralPath $path) { Remove-Item -LiteralPath $path }
}

