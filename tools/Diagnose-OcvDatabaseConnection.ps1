Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Data

$form = New-Object System.Windows.Forms.Form
$form.Text = 'OCV_DATA Database Connection Check'
$form.StartPosition = 'CenterScreen'
$form.Size = New-Object System.Drawing.Size(760, 590)
$form.MinimumSize = New-Object System.Drawing.Size(760, 590)
$form.Font = New-Object System.Drawing.Font('Microsoft YaHei UI', 9)

$serverLabel = New-Object System.Windows.Forms.Label
$serverLabel.Text = 'SQL Server (IP, hostname, instance, or IP,port)'
$serverLabel.Location = New-Object System.Drawing.Point(16, 18)
$serverLabel.AutoSize = $true
$form.Controls.Add($serverLabel)

$serverBox = New-Object System.Windows.Forms.TextBox
$serverBox.Location = New-Object System.Drawing.Point(20, 43)
$serverBox.Size = New-Object System.Drawing.Size(700, 25)
$serverBox.Text = '127.0.0.1\SQLEXPRESS'
$form.Controls.Add($serverBox)

$userLabel = New-Object System.Windows.Forms.Label
$userLabel.Text = 'SQL Login'
$userLabel.Location = New-Object System.Drawing.Point(16, 78)
$userLabel.AutoSize = $true
$form.Controls.Add($userLabel)

$userBox = New-Object System.Windows.Forms.TextBox
$userBox.Location = New-Object System.Drawing.Point(20, 102)
$userBox.Size = New-Object System.Drawing.Size(330, 25)
$userBox.Text = 'xlsuper'
$form.Controls.Add($userBox)

$passwordLabel = New-Object System.Windows.Forms.Label
$passwordLabel.Text = 'Password'
$passwordLabel.Location = New-Object System.Drawing.Point(370, 78)
$passwordLabel.AutoSize = $true
$form.Controls.Add($passwordLabel)

$passwordBox = New-Object System.Windows.Forms.TextBox
$passwordBox.Location = New-Object System.Drawing.Point(374, 102)
$passwordBox.Size = New-Object System.Drawing.Size(346, 25)
$passwordBox.UseSystemPasswordChar = $true
$form.Controls.Add($passwordBox)

$testButton = New-Object System.Windows.Forms.Button
$testButton.Text = 'Test connection'
$testButton.Location = New-Object System.Drawing.Point(20, 145)
$testButton.Size = New-Object System.Drawing.Size(150, 34)
$form.Controls.Add($testButton)

$copyButton = New-Object System.Windows.Forms.Button
$copyButton.Text = 'Copy results'
$copyButton.Location = New-Object System.Drawing.Point(182, 145)
$copyButton.Size = New-Object System.Drawing.Size(120, 34)
$form.Controls.Add($copyButton)

$resultBox = New-Object System.Windows.Forms.TextBox
$resultBox.Location = New-Object System.Drawing.Point(20, 194)
$resultBox.Size = New-Object System.Drawing.Size(700, 340)
$resultBox.Multiline = $true
$resultBox.ReadOnly = $true
$resultBox.ScrollBars = 'Vertical'
$resultBox.WordWrap = $true
$resultBox.Font = New-Object System.Drawing.Font('Consolas', 9)
$resultBox.Anchor = 'Top,Bottom,Left,Right'
$form.Controls.Add($resultBox)

$testButton.Add_Click({
    $server = $serverBox.Text.Trim()
    $login = $userBox.Text.Trim()
    $password = $passwordBox.Text
    $resultBox.Clear()

    if ([string]::IsNullOrWhiteSpace($server) -or [string]::IsNullOrWhiteSpace($login)) {
        $resultBox.Text = 'Enter the SQL Server address and SQL Login.'
        return
    }

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add('Target: ' + $server + ' / database OCV_DATA / login ' + $login)
    $lines.Add('Password is hidden and is not included in these results.')
    $lines.Add('')

    $hostName = $server -replace '^tcp:', ''
    $port = 1433
    $hasExplicitPort = $false
    $isNamedInstance = $false
    if ($hostName -match ',(\d+)$') {
        $port = [int]$Matches[1]
        $hasExplicitPort = $true
        $hostName = $hostName.Substring(0, $hostName.LastIndexOf(','))
    }
    if ($hostName.Contains('\')) {
        $isNamedInstance = $true
        $hostName = $hostName.Split('\')[0]
    }

    $resolved = $false
    try {
        if ($hostName -eq '.' -or $hostName -eq '(local)') { $hostName = 'localhost' }
        $addresses = [System.Net.Dns]::GetHostAddresses($hostName)
        $addressText = (($addresses | ForEach-Object { $_.IPAddressToString }) -join ', ')
        $lines.Add('[1/3] DNS: OK - ' + $hostName + ' -> ' + $addressText)
        $resolved = $true
    } catch {
        $lines.Add('[1/3] DNS: FAILED - ' + $_.Exception.Message)
    }

    if ($resolved -and ($hasExplicitPort -or -not $isNamedInstance)) {
        $tcpClient = New-Object System.Net.Sockets.TcpClient
        $asyncResult = $null
        try {
            $asyncResult = $tcpClient.BeginConnect($hostName, $port, $null, $null)
            if (-not $asyncResult.AsyncWaitHandle.WaitOne(3000, $false)) {
                throw ('TCP connection to ' + $hostName + ':' + $port + ' timed out after 3 seconds.')
            }
            $tcpClient.EndConnect($asyncResult)
            $lines.Add('[2/3] TCP: OK - ' + $hostName + ':' + $port + ' is reachable.')
        } catch {
            $lines.Add('[2/3] TCP: FAILED - ' + $_.Exception.Message)
        } finally {
            if ($null -ne $asyncResult) { $asyncResult.AsyncWaitHandle.Close() }
            $tcpClient.Close()
        }
    } elseif ($resolved) {
        $lines.Add('[2/3] TCP: SKIPPED - named instance uses SQL Browser or a configured static port.')
    } else {
        $lines.Add('[2/3] TCP: SKIPPED - DNS did not resolve the server.')
    }

    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
    $builder['Data Source'] = $server
    $builder['Initial Catalog'] = 'OCV_DATA'
    $builder['User ID'] = $login
    $builder['Password'] = $password
    $builder['Integrated Security'] = $false
    $builder['Connect Timeout'] = 5
    $builder['Persist Security Info'] = $false

    $lines.Add('SqlClient will use login ' + $builder['User ID'] + ' on ' + $builder['Data Source'] + ' / ' + $builder['Initial Catalog'] + '.')
    $connection = New-Object -TypeName System.Data.SqlClient.SqlConnection -ArgumentList $builder.ConnectionString
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = 'SELECT @@VERSION AS SqlVersion, DB_NAME() AS DatabaseName, ORIGINAL_LOGIN() AS LoginName'
        $command.CommandTimeout = 10
        $reader = $command.ExecuteReader()
        [void]$reader.Read()
        $version = [string]$reader['SqlVersion']
        $databaseName = [string]$reader['DatabaseName']
        $actualLogin = [string]$reader['LoginName']
        $reader.Close()
        $command.Dispose()
        $lines.Add('[3/3] SQL login/database: OK - connected to ' + $databaseName + ' as ' + $actualLogin + '.')
        $lines.Add('SQL version: ' + (($version -split "`r?`n")[0]))

        $healthCommand = $connection.CreateCommand()
        $healthCommand.CommandText = 'dbo.usp_CheckDatabaseHealth'
        $healthCommand.CommandType = [System.Data.CommandType]::StoredProcedure
        $healthCommand.CommandTimeout = 15
        try {
            $healthReader = $healthCommand.ExecuteReader()
            $failedChecks = New-Object System.Collections.Generic.List[string]
            $checkCount = 0
            while ($healthReader.Read()) {
                $checkCount++
                if (-not [Convert]::ToBoolean($healthReader['IsOk'])) {
                    $failedChecks.Add(([string]$healthReader['CheckName']) + ': ' + [string]$healthReader['Detail'])
                }
            }
            $healthReader.Close()
            if ($checkCount -eq 0) {
                $lines.Add('Application health check: FAILED - the procedure returned no checks.')
            } elseif ($failedChecks.Count -eq 0) {
                $lines.Add('Application health check: OK - all ' + $checkCount + ' checks passed.')
            } else {
                $lines.Add('Application health check: FAILED - ' + ($failedChecks -join '; '))
            }
        } catch {
            $lines.Add('Application health check: FAILED - ' + $_.Exception.Message)
            $lines.Add('SQL login and access to OCV_DATA succeeded; check that the required schema/procedure is installed and the login can execute it.')
        } finally {
            $healthCommand.Dispose()
        }
    } catch {
        $lines.Add('[3/3] SQL login/database: FAILED')
        $lines.Add($_.Exception.Message)
        if ($_.Exception -is [System.Data.SqlClient.SqlException]) {
            foreach ($sqlError in $_.Exception.Errors) {
                $lines.Add('SQL error ' + $sqlError.Number + ', state ' + $sqlError.State + ', class ' + $sqlError.Class + ': ' + $sqlError.Message)
            }
        }
    } finally {
        $connection.Dispose()
        $password = $null
        $builder['Password'] = ''
    }

    $lines.Add('')
    $lines.Add('Interpretation:')
    $lines.Add('- DNS failure: check the server name or use its IP address.')
    $lines.Add('- TCP failure: check whether 127.0.0.1 targets the installed instance. Named instances may need 127.0.0.1\INSTANCE or 127.0.0.1,PORT.')
    $lines.Add('- On this PC, also confirm the SQL Server instance service is running and TCP/IP is enabled for that instance.')
    $lines.Add('- TCP works but SQL login fails: check mixed authentication, login status/password, and OCV_DATA mapping.')
    $lines.Add('- SQL login works but health check fails: network and credentials work; check the database schema/procedure and permissions.')
    $resultBox.Text = $lines -join [Environment]::NewLine
})

$copyButton.Add_Click({
    if (-not [string]::IsNullOrWhiteSpace($resultBox.Text)) {
        [System.Windows.Forms.Clipboard]::SetText($resultBox.Text)
    }
})

[void]$form.ShowDialog()
