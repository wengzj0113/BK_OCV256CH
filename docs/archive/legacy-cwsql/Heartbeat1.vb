Imports System.Data.SqlClient
Imports System.Net.Http
Imports System.Text
Imports Newtonsoft.Json
Imports System.Threading
Imports System.Net.NetworkInformation
Imports System.Threading.Tasks

Public Class Heartbeat1
    Private Const InterfaceName As String = "Heartbeat"
    Private ReadOnly _url As String
    Private ReadOnly _client As HttpClient
    Private ReadOnly _timerInterval As Integer
    Private ReadOnly _logger As SimpleLogger
    Private ReadOnly _connectionString As String
    Private ReadOnly _equip_Code As String
    Private _timer As Timer
    Private _isRunning As Integer = 0

    Public Sub New(url As String, connectionString As String, logListBox As ListBox, equip_Code As String, Optional timerInterval As Integer = 10000)
        '复用 Module1 共享 HttpClient 池 (R2-12)
        _client = MesHttpClient
        _logger = New SimpleLogger(logListBox)
        _connectionString = connectionString
        _url = url
        _timerInterval = timerInterval
        _equip_Code = equip_Code
    End Sub

    Public Sub StartTimer()
        If _timer Is Nothing Then
            _timer = New Timer(AddressOf TimerCallback, Nothing, 0, _timerInterval)
        End If
    End Sub

    Public Sub StopTimer()
        If _timer IsNot Nothing Then
            _timer.Dispose()
            _timer = Nothing
        End If
    End Sub

    Private Sub TimerCallback(state As Object)
        If Threading.Interlocked.Exchange(_isRunning, 1) = 1 Then
            Return
        End If

        Task.Run(AddressOf ExecuteHeartbeatTasksAsync).ContinueWith(
            Sub(t)
                Try
                    If t.IsFaulted AndAlso t.Exception IsNot Nothing Then
                        LogError("Error in ExecuteHeartbeatTasksAsync: " & t.Exception.InnerException.Message)
                    End If
                Finally
                    Threading.Interlocked.Exchange(_isRunning, 0)
                End Try
            End Sub, TaskScheduler.Default)
    End Sub

    Private Async Function ExecuteHeartbeatTasksAsync() As Task
        If Await IsNetworkAvailableAsync() Then
            Await UploadPendingDataAsync()
            Await UploadDataAsync()
        Else
            LogWarning("MES network is unavailable.")
        End If
    End Function

    Private Async Function IsNetworkAvailableAsync() As Task(Of Boolean)
        Try
            Using ping As New Ping()
                Dim reply As PingReply = Await ping.SendPingAsync("172.16.110.10")
                Return reply.Status = IPStatus.Success
            End Using
        Catch ex As Exception
            LogError("Error checking network availability: " & ex.Message)
            Return False
        End Try
    End Function

    Private Async Function UploadDataAsync() As Task
        Dim data As New Dictionary(Of String, Object) From {
        {"EquipPC_ID", If(String.IsNullOrEmpty(Initsetting.EquipPC_ID), "HZWL", Initsetting.EquipPC_ID)},
        {"EquipPC_Password", If(String.IsNullOrEmpty(Initsetting.EquipPC_Password), "123456", Initsetting.EquipPC_Password)},
        {"Equip_Code", _equip_Code},
        {"Upload_Time", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")},
        {"Is_Online", True}
    }
        Dim json As String = JsonConvert.SerializeObject(data)
        Dim content As New StringContent(json, Encoding.UTF8, "application/json")
        Try
            Dim response As HttpResponseMessage = Await _client.PostAsync(_url, content)
            Dim responseString As String = Await response.Content.ReadAsStringAsync()
            If Not response.IsSuccessStatusCode OrElse Not IsSuccessfulResponse(responseString) Then
                SaveDataLocally(json)
                Return
            End If
        Catch ex As Exception
            LogError("Error uploading data: " & ex.Message)
            SaveDataLocally(json)
        End Try
    End Function

    Private Sub SaveDataLocally(data As String)
        Try
            EnsureUnsentDataSchema()
            Using connection As New SqlConnection(_connectionString)
                connection.Open()
                Dim command As New SqlCommand("INSERT INTO UnsentData (Data, Upload_Time, InterfaceName, TargetUrl) VALUES (@Data, @Upload_Time, @InterfaceName, @TargetUrl)", connection)
                command.Parameters.AddWithValue("@Data", data)
                command.Parameters.AddWithValue("@Upload_Time", DateTime.Now)
                command.Parameters.AddWithValue("@InterfaceName", InterfaceName)
                command.Parameters.AddWithValue("@TargetUrl", _url)
                command.ExecuteNonQuery()
            End Using
            LogInformation("Data saved locally for future upload.")
        Catch ex As Exception
            LogError("Error saving data locally: " & ex.Message)
        End Try
    End Sub

    Private Function IsSuccessfulResponse(responseString As String) As Boolean
        Try
            Dim response As Dictionary(Of String, Object) = JsonConvert.DeserializeObject(Of Dictionary(Of String, Object))(responseString)
            If response Is Nothing OrElse Not response.ContainsKey("Return_Code") Then
                LogWarning("MES response missing Return_Code: " & responseString)
                Return False
            End If

            Dim returnCode As String = response("Return_Code").ToString()
            Dim msg As String = If(response.ContainsKey("Msg"), response("Msg").ToString(), "")
            If returnCode = "S" Then
                LogInformation("Success: " & msg)
                Return True
            Else
                LogWarning("Error: " & returnCode & " - " & msg)
                Return False
            End If
        Catch ex As Exception
            LogError("Error processing response: " & ex.Message)
            Return False
        End Try
    End Function

    Private Async Function SaveDataLocallyAsync(data As String) As Task
        Try
            EnsureUnsentDataSchema()
            Using connection As New SqlConnection(_connectionString)
                Await connection.OpenAsync()
                Dim command As New SqlCommand("INSERT INTO UnsentData (Data, Upload_Time, InterfaceName, TargetUrl) VALUES (@Data, @Upload_Time, @InterfaceName, @TargetUrl)", connection)
                command.Parameters.AddWithValue("@Data", data)
                command.Parameters.AddWithValue("@Upload_Time", DateTime.Now)
                command.Parameters.AddWithValue("@InterfaceName", InterfaceName)
                command.Parameters.AddWithValue("@TargetUrl", _url)
                Await command.ExecuteNonQueryAsync()
            End Using
        Catch ex As Exception
            LogError("Error saving data locally: " & ex.Message)
        End Try
    End Function

    Private Async Function UploadPendingDataAsync() As Task
        Try
            EnsureUnsentDataSchema()
            Using connection As New SqlConnection(_connectionString)
                Await connection.OpenAsync()
                Dim command As New SqlCommand("SELECT Id, Data FROM UnsentData WHERE Is_Sent = 0 AND InterfaceName = @InterfaceName ORDER BY Id", connection)
                command.Parameters.AddWithValue("@InterfaceName", InterfaceName)
                Using reader As SqlDataReader = Await command.ExecuteReaderAsync()
                    While Await reader.ReadAsync()
                        Dim id As Integer = reader.GetInt32(0)
                        Dim data As String = reader.GetString(1)
                        Await TrySendPendingDataAsync(id, data)
                    End While
                End Using
            End Using
        Catch ex As Exception
            LogError("Error uploading pending data: " & ex.Message)
        End Try
    End Function

    Private Async Function TrySendPendingDataAsync(id As Integer, data As String) As Task
        Try
            Dim content As New StringContent(data, Encoding.UTF8, "application/json")
            Dim response As HttpResponseMessage = Await _client.PostAsync(_url, content)
            Dim responseString As String = Await response.Content.ReadAsStringAsync()
            If response.IsSuccessStatusCode AndAlso IsSuccessfulResponse(responseString) Then
                Await MarkDataAsSentAsync(id)
                LogInformation("Pending data sent successfully.")
            Else
                LogWarning("Failed to send pending data.")
            End If
        Catch ex As Exception
            LogError("Error while sending pending data: " & ex.Message)
        End Try
    End Function

    Private Async Function MarkDataAsSentAsync(id As Integer) As Task
        Try
            Using connection As New SqlConnection(_connectionString)
                Await connection.OpenAsync()
                Dim command As New SqlCommand("UPDATE UnsentData SET Is_Sent = 1 WHERE Id = @Id", connection)
                command.Parameters.AddWithValue("@Id", id)
                Await command.ExecuteNonQueryAsync()
            End Using
        Catch ex As Exception
            LogError("Error marking data as sent: " & ex.Message)
        End Try
    End Function

    Private Sub EnsureUnsentDataSchema()
        Using connection As New SqlConnection(_connectionString)
            connection.Open()
            Dim sql As String =
                "IF OBJECT_ID('dbo.UnsentData', 'U') IS NULL " &
                "CREATE TABLE dbo.UnsentData (" &
                "Id INT IDENTITY(1,1) PRIMARY KEY, " &
                "Data NVARCHAR(MAX) NOT NULL, " &
                "Upload_Time DATETIME NOT NULL, " &
                "Is_Sent BIT NOT NULL DEFAULT 0, " &
                "InterfaceName NVARCHAR(100) NULL, " &
                "TargetUrl NVARCHAR(500) NULL); " &
                "IF COL_LENGTH('dbo.UnsentData', 'InterfaceName') IS NULL " &
                "ALTER TABLE dbo.UnsentData ADD InterfaceName NVARCHAR(100) NULL; " &
                "IF COL_LENGTH('dbo.UnsentData', 'TargetUrl') IS NULL " &
                "ALTER TABLE dbo.UnsentData ADD TargetUrl NVARCHAR(500) NULL;"
            Using command As New SqlCommand(sql, connection)
                command.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Private Sub LogInformation(message As String)
        _logger.LogInformation(message)
    End Sub

    Private Sub LogWarning(message As String)
        _logger.LogWarning(message)
    End Sub

    Private Sub LogError(message As String)
        _logger.LogError(message)
    End Sub
End Class

Public Class SimpleLogger
    Private ReadOnly _listBox As ListBox

    Public Sub New(listBox As ListBox)
        _listBox = listBox
    End Sub

    Public Sub Log(level As String, message As String)
        Dim logMessage As String = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {level}: {message}"
        If _listBox.InvokeRequired Then
            _listBox.Invoke(Sub() _listBox.Items.Add(logMessage))
        Else
            _listBox.Items.Add(logMessage)
        End If
    End Sub

    Public Sub LogInformation(message As String)
        Log("INFO", message)
    End Sub

    Public Sub LogWarning(message As String)
        Log("WARNING", message)
    End Sub

    Public Sub LogError(message As String)
        Log("ERROR", message)
    End Sub
End Class
''主程序调用代码
'Dim heartbeat As New Heartbeat("http://your-mes-url.com", "your-connection-string", yourListBox, "YOUR-EQUIP-CODE")
'heartbeat.StartTimer()
