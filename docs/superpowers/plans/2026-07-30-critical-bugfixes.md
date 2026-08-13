# 关键 Bug 修复执行计划

> **给后续执行者:** 实施本计划时必须按任务逐项推进，推荐使用 `superpowers:subagent-driven-development`，也可以使用 `superpowers:executing-plans`。每个步骤使用复选框 `- [ ]` 跟踪进度。

**目标:** 修复两轮审查发现的生产阻断类问题：MES 离线缓存错发和数据丢失、上传成功判定错误、数组越界、数据库连接不稳定、SQL 拼接、定时器重入、Socket 关闭异常、心跳定时器生命周期问题。

**架构思路:** 保持现有 WinForms/VB.NET 结构不做大拆大改，通过小型兼容辅助方法修复关键路径。优先修数据丢失，再修运行时崩溃，最后修长期运行稳定性问题。尽量保持现有界面行为和数据库兼容。

**技术栈:** VB.NET WinForms、.NET Framework 4.8、SQL Server、`System.Data.SqlClient`、`System.Threading.Timer`、`HttpClient`、串口/Socket 设备通信。

---

## 文件范围

- 修改: `ResultDataUploadAssembly.vb` - 结果上传的成功/失败判定、离线缓存路由。
- 修改: `AlarmUpload.vb` - 报警上传的离线缓存路由、定时器串行化、成功判定。
- 修改: `EnergyUpload.vb` - 能耗上传的离线缓存路由、定时器串行化、成功判定。
- 修改: `StateAndStopReasonUpload.vb` - 状态/停机原因上传的离线缓存路由、定时器串行化、成功判定。
- 修改: `WIPInStationCheck.vb` - WIP 进站校验上传的离线缓存路由、定时器串行化、成功判定。
- 修改: `SqlServerHelper.vb` - 增加可复用的待上传数据表帮助方法，保存接口名和目标 URL。
- 修改: `Module1.vb` - 修复复测数组越界，必要时增加本地数据库连接字符串辅助逻辑。
- 修改: `clsDB.vb` - 修复核心数据路径上的高风险 SQL 拼接，并在触及的方法里减少全局连接复用。
- 修改: `SocketClient.vb` - 修复取消令牌归属和关闭生命周期。
- 修改: `Heartbeat1.vb` - 将定时器保存为字段，并防止心跳回调重入。
- 可选新增: `CriticalBugfixSmokeTests.vb` 或一个小型控制台/手工测试入口，前提是能用 Visual Studio MSBuild 编译项目。

## 约束与假设

- 该项目依赖 COM 引用，`dotnet build` 无法完成构建，需要使用 .NET Framework 版 MSBuild 或 Visual Studio。
- 本轮不做大型 UI 重构。
- 数据库迁移必须兼容已有 `UnsentData` 行。
- 当前工作区不是 Git 仓库，风险编辑前建议手动备份，或先初始化源码管理。

---

### 任务 1: 增加带接口路由的离线缓存

**文件:**
- 修改: `SqlServerHelper.vb`
- 后续任务修改: 所有使用 `UnsentData` 的上传类

- [ ] **步骤 1: 增加表结构升级方法**

在 `SqlServerHelper` 中增加公共方法：如果 `UnsentData` 不存在则创建；如果缺少路由字段则补充字段。

```vb
Public Sub EnsureUnsentDataSchema()
    Using connection As New SqlConnection(connectionString)
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
```

- [ ] **步骤 2: 增加带路由信息的插入方法**

```vb
Public Sub InsertUnsentData(data As String, interfaceName As String, targetUrl As String)
    EnsureUnsentDataSchema()
    Using connection As New SqlConnection(connectionString)
        connection.Open()
        Using command As New SqlCommand("INSERT INTO UnsentData (Data, Upload_Time, InterfaceName, TargetUrl) VALUES (@Data, @Upload_Time, @InterfaceName, @TargetUrl)", connection)
            command.Parameters.Add("@Data", SqlDbType.NVarChar).Value = data
            command.Parameters.Add("@Upload_Time", SqlDbType.DateTime).Value = DateTime.Now
            command.Parameters.Add("@InterfaceName", SqlDbType.NVarChar, 100).Value = interfaceName
            command.Parameters.Add("@TargetUrl", SqlDbType.NVarChar, 500).Value = targetUrl
            command.ExecuteNonQuery()
        End Using
    End Using
End Sub
```

- [ ] **步骤 3: 增加按接口过滤的查询方法和标记已发送方法**

```vb
Public Function GetUnsentData(interfaceName As String) As List(Of PendingUpload)
    EnsureUnsentDataSchema()
    Dim items As New List(Of PendingUpload)
    Using connection As New SqlConnection(connectionString)
        connection.Open()
        Using command As New SqlCommand("SELECT Id, Data, ISNULL(InterfaceName, ''), ISNULL(TargetUrl, '') FROM UnsentData WHERE Is_Sent = 0 AND InterfaceName = @InterfaceName ORDER BY Id", connection)
            command.Parameters.Add("@InterfaceName", SqlDbType.NVarChar, 100).Value = interfaceName
            Using reader As SqlDataReader = command.ExecuteReader()
                While reader.Read()
                    items.Add(New PendingUpload With {
                        .Id = reader.GetInt32(0),
                        .JsonData = reader.GetString(1),
                        .InterfaceName = reader.GetString(2),
                        .TargetUrl = reader.GetString(3)
                    })
                End While
            End Using
        End Using
    End Using
    Return items
End Function

Public Sub MarkUnsentDataAsSent(id As Integer)
    Using connection As New SqlConnection(connectionString)
        connection.Open()
        Using command As New SqlCommand("UPDATE UnsentData SET Is_Sent = 1 WHERE Id = @Id", connection)
            command.Parameters.Add("@Id", SqlDbType.Int).Value = id
            command.ExecuteNonQuery()
        End Using
    End Using
End Sub
```

- [ ] **步骤 4: 扩展 `PendingUpload`**

```vb
Public Class PendingUpload
    Public Property Id As Integer
    Public Property JsonData As String
    Public Property InterfaceName As String
    Public Property TargetUrl As String
End Class
```

- [ ] **步骤 5: 手工验证数据库结构**

在 SQL Server 中执行：

```sql
EXEC sp_help 'dbo.UnsentData';
```

预期结果：表中包含 `Id`、`Data`、`Upload_Time`、`Is_Sent`、`InterfaceName`、`TargetUrl` 字段。

---

### 任务 2: 修复上传成功判定和离线缓存路由

**文件:**
- 修改: `ResultDataUploadAssembly.vb`
- 修改: `AlarmUpload.vb`
- 修改: `EnergyUpload.vb`
- 修改: `StateAndStopReasonUpload.vb`
- 修改: `WIPInStationCheck.vb`

- [ ] **步骤 1: 每个上传类定义接口名常量**

`ResultDataUploadAssembly.vb` 示例：

```vb
Private Const InterfaceName As String = "ResultDataUploadAssembly"
Private Const url As String = "http://172.16.110.10:9015/api/ResultDataUploadAssembly"
```

其他类使用对应名称：

```vb
AlarmUpload
EnergyUpload
StateAndStopReasonUpload
WIPInStationCheck
```

- [ ] **步骤 2: 替换本地保存逻辑**

将直接 `INSERT INTO UnsentData` 的代码替换为：

```vb
Private Sub SaveDataLocally(data As String)
    Dim helper As New SqlServerHelper()
    helper.InsertUnsentData(data, InterfaceName, url)
End Sub
```

- [ ] **步骤 3: 让响应处理方法返回 Boolean**

每个上传类使用以下结构：

```vb
Private Function IsSuccessfulResponse(responseString As String) As Boolean
    Try
        Dim response As Dictionary(Of String, Object) = JsonConvert.DeserializeObject(Of Dictionary(Of String, Object))(responseString)
        If response Is Nothing OrElse Not response.ContainsKey("Return_Code") Then
            AddToOutput("MES response missing Return_Code: " & responseString)
            Return False
        End If

        Dim returnCode As String = response("Return_Code").ToString()
        Dim msg As String = If(response.ContainsKey("Msg"), response("Msg").ToString(), "")
        If returnCode = "S" Then
            AddToOutput("Success: " & InterfaceName & " - " & msg)
            Return True
        End If

        AddToOutput("Error: " & InterfaceName & " Return_Code=" & returnCode & " - " & msg)
        Return False
    Catch ex As Exception
        AddToOutput("Error parsing MES response: " & ex.Message)
        Return False
    End Try
End Function
```

- [ ] **步骤 4: HTTP 失败或业务失败时都保存到本地**

使用以下模式：

```vb
Dim response As HttpResponseMessage = Await client.PostAsync(url, content)
Dim responseString As String = Await response.Content.ReadAsStringAsync()
If Not response.IsSuccessStatusCode OrElse Not IsSuccessfulResponse(responseString) Then
    SaveDataLocally(json)
    Return
End If
```

- [ ] **步骤 5: 只重传当前接口对应的缓存行**

```vb
Private Async Function UploadPendingDataAsync() As Task
    Dim helper As New SqlServerHelper()
    For Each item As PendingUpload In helper.GetUnsentData(InterfaceName)
        Try
            Dim content As New StringContent(item.JsonData, Encoding.UTF8, "application/json")
            Dim response As HttpResponseMessage = Await client.PostAsync(url, content)
            Dim responseString As String = Await response.Content.ReadAsStringAsync()
            If response.IsSuccessStatusCode AndAlso IsSuccessfulResponse(responseString) Then
                helper.MarkUnsentDataAsSent(item.Id)
            End If
        Catch ex As Exception
            AddToOutput("Error sending pending " & InterfaceName & ": " & ex.Message)
        End Try
    Next
End Function
```

- [ ] **步骤 6: 移除过早停止定时器的逻辑**

在 `ResultDataUploadAssembly.vb` 中移除单次 POST 后的 `StopTimer()`，除非调用方明确要求只执行一次上传。

- [ ] **步骤 7: 使用种子数据验证**

手工插入两行：

```sql
INSERT INTO UnsentData (Data, Upload_Time, Is_Sent, InterfaceName, TargetUrl)
VALUES
('{"test":"alarm"}', GETDATE(), 0, 'AlarmUpload', 'http://172.16.110.10:9015/api/AlarmUpload'),
('{"test":"result"}', GETDATE(), 0, 'ResultDataUploadAssembly', 'http://172.16.110.10:9015/api/ResultDataUploadAssembly');
```

只运行结果上传重试。预期结果：只尝试/标记 `ResultDataUploadAssembly` 行，`AlarmUpload` 行保持未发送。

---

### 任务 3: 防止定时器异步回调重入

**文件:**
- 修改: 所有上传定时器类
- 修改: `Heartbeat1.vb`

- [ ] **步骤 1: 增加运行状态门闩字段**

```vb
Private _isRunning As Integer = 0
```

- [ ] **步骤 2: 保护每个定时器回调**

```vb
Private Async Function TimerCallback() As Task
    If Threading.Interlocked.Exchange(_isRunning, 1) = 1 Then
        Return
    End If

    Try
        If IsNetworkAvailable() Then
            Await UploadPendingDataAsync()
            Await UploadDataAsync()
        Else
            AddToOutput("MES network is unavailable.")
        End If
    Finally
        Threading.Interlocked.Exchange(_isRunning, 0)
    End Try
End Function
```

对于仍使用 `Async Sub(state) Await TimerCallback()` 的类，可以保留外层包装，但实际工作必须都在带门闩的 `TimerCallback` 内完成。

- [ ] **步骤 3: 验证不会重入**

临时在某个上传方法中增加 15 秒延迟，定时间隔保持 10 秒。预期结果：上一次执行未结束时，不会启动重复上传。

---

### 任务 4: 修复复测数组越界

**文件:**
- 修改: `Module1.vb:1843-1845`

- [ ] **步骤 1: 替换不安全的数组复制**

将：

```vb
Dim retestArray1 As Int16() = New Int16(255) {}
ReDim retestArray1(63)
Array.Copy(retestArray, retestArray1, retestArray.Length)
```

替换为：

```vb
Dim retestArray1(63) As Int16
Array.Copy(retestArray, retestArray1, Math.Min(retestArray.Length, retestArray1.Length))
```

- [ ] **步骤 2: 验证批量读取路径**

运行会调用 `Trans_BatchReadData_3561` 的操作。预期结果：不再出现 `Array.Copy` 的 `ArgumentException`，前 64 通道的返回行为保持不变。

---

### 任务 5: 修复 `SocketClient` 生命周期

**文件:**
- 修改: `SocketClient.vb`

- [ ] **步骤 1: 将取消源保存到字段**

将局部声明：

```vb
Dim cts As New CancellationTokenSource()
Dim token As CancellationToken = cts.Token
```

替换为：

```vb
cts = New CancellationTokenSource()
cancellationToken = cts.Token
```

启动线程时使用：

```vb
Dim thread As New Thread(Sub() DoWork(cancellationToken))
thread.IsBackground = True
thread.Start()
```

- [ ] **步骤 2: 让 `Close` 先取消并且支持空值**

```vb
Public Sub Close()
    Try
        If cts IsNot Nothing Then
            cts.Cancel()
        End If
        If clientSocket IsNot Nothing Then
            clientSocket.Close()
        End If
    Finally
        If cts IsNot Nothing Then
            cts.Dispose()
            cts = Nothing
        End If
    End Try
End Sub
```

- [ ] **步骤 3: 验证关闭行为**

调用 `ConnectToServer`，等待两秒，再调用 `Close`。预期结果：没有 `NullReferenceException`，发送循环退出。

---

### 任务 6: 修复心跳定时器生命周期

**文件:**
- 修改: `Heartbeat1.vb`

- [ ] **步骤 1: 增加定时器字段**

```vb
Private _timer As Timer
Private _isRunning As Integer = 0
```

- [ ] **步骤 2: 保存定时器引用**

```vb
Public Sub StartTimer()
    If _timer Is Nothing Then
        _timer = New Timer(AddressOf TimerCallback, Nothing, 0, _timerInterval)
    End If
End Sub
```

- [ ] **步骤 3: 增加停止方法**

```vb
Public Sub StopTimer()
    If _timer IsNot Nothing Then
        _timer.Dispose()
        _timer = Nothing
    End If
End Sub
```

- [ ] **步骤 4: 防止回调重入**

```vb
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
```

- [ ] **步骤 5: 验证**

启动心跳并让程序空闲至少 5 个定时间隔。预期结果：心跳持续触发，且不会并发重入。

---

### 任务 7: 参数化核心 SQL，并减少全局连接使用

**文件:**
- 修改: `clsDB.vb`
- 修改: `SqlServerHelper.vb`

- [ ] **步骤 1: 优先修复这些方法**

这些方法直接影响生产数据，优先处理：

```text
GetGrade4LVname
Ups4BarCode
Query4BarCode
BatchQueryData
TraySelect
ExitRecord
IsExists
```

- [ ] **步骤 2: 改造 `IsExists`**

```vb
Public Function IsExists(ByVal code As String) As Boolean
    Using connection As New SqlConnection(pubConnectionString)
        connection.Open()
        Using command As New SqlCommand("SELECT 1 FROM cw_trayMain WHERE cTrayCode = @Code", connection)
            command.Parameters.Add("@Code", SqlDbType.NVarChar, 100).Value = code
            Using reader As SqlDataReader = command.ExecuteReader()
                Return reader.HasRows
            End Using
        End Using
    End Using
End Function
```

- [ ] **步骤 3: 改造 `Query4BarCode`**

由于当前方法返回 `SqlDataReader`，如果一次性改调用方风险较高，第一阶段建议新增返回 `DataTable` 的安全方法：

```vb
Public Function Query4BarCodeTable(Optional ByVal BarCode As String = "", Optional ByVal strTray As String = "") As DataTable
    Dim table As New DataTable()
    Using connection As New SqlConnection(pubConnectionString)
        connection.Open()
        Using command As New SqlCommand("SELECT Fld_Tray, fld_Smcode, Fld_VolA, Fld_ResiA, Fld_VolB, Fld_ResiB, Fld_VolC, Fld_ResiC, Fld_VolD, Fld_ResiD FROM CW_tblData_Info WHERE fld_Smcode = @BarCode", connection)
            command.Parameters.Add("@BarCode", SqlDbType.NVarChar, 100).Value = BarCode
            Using adapter As New SqlDataAdapter(command)
                adapter.Fill(table)
            End Using
        End Using
    End Using
    Return table
End Function
```

然后逐个更新调用方，改用返回 `DataTable` 的方法。

- [ ] **步骤 4: 改造 `Ups4BarCode`**

SQL 字段名根据 OCV 选择，但值必须全部使用参数：

```vb
Dim voltageField As String
Dim resistanceField As String
Dim timeField As String
Select Case OCV
    Case 1
        voltageField = "Fld_VolA" : resistanceField = "Fld_ResiA" : timeField = "Fld_TimeA"
    Case 2
        voltageField = "Fld_VolB" : resistanceField = "Fld_ResiB" : timeField = "Fld_TimeB"
    Case 3
        voltageField = "Fld_VolC" : resistanceField = "Fld_ResiC" : timeField = "Fld_TimeC"
    Case Else
        voltageField = "Fld_VolD" : resistanceField = "Fld_ResiD" : timeField = "Fld_TimeD"
End Select

Dim sql As String = "UPDATE dbo.CW_tblData_Info SET Fld_Tray = @Tray, " & voltageField & " = @Voltage, " & resistanceField & " = @Resistance, " & timeField & " = @TestTime WHERE Fld_SmCode = @BatCode"
```

为 `@Tray`、`@Voltage`、`@Resistance`、`@TestTime`、`@BatCode` 添加参数。

- [ ] **步骤 5: 使用特殊字符验证**

使用包含单引号的测试条码，例如：

```text
ABC'123
```

预期结果：没有 SQL 语法错误，也不会误改其他行。

---

### 任务 8: 验证与发布检查清单

**文件:**
- 仅当本轮包含构建/发布设置修复时，修改 `MP-60A-RV-64Mem.vbproj`

- [ ] **步骤 1: 使用 Visual Studio MSBuild 构建**

在 Developer Command Prompt 中执行：

```bat
msbuild MP-60A-RV-64Mem.vbproj /t:Rebuild /p:Configuration=Debug /p:Platform=x86
```

预期结果：构建成功，或只剩已知历史警告。

- [ ] **步骤 2: 模拟上传失败**

断开 MES，或临时将某个上传 URL 指向不可达地址。逐个触发上传。预期结果：每个上传类都写入一条 `UnsentData`，且 `InterfaceName` 正确。

- [ ] **步骤 3: 模拟上传恢复**

恢复 MES。启动各上传类。预期结果：每个类只发送自己接口对应的待上传行，并且只有在 HTTP 成功且 `Return_Code = S` 时才标记为已发送。

- [ ] **步骤 4: 硬件冒烟测试**

执行：

```text
1. 登录
2. 连接扫码枪
3. 连接 SCM Socket
4. 运行一次正常测试流程
5. 运行一次复测/批量读取流程
6. 确认 MES 上传和本地日志
```

预期结果：界面不卡死、没有重复上传行、没有 `Array.Copy` 异常、没有 Socket 关闭异常。

- [ ] **步骤 5: 回归搜索**

发布前执行：

```powershell
Select-String -Path '*.vb' -Pattern 'INSERT INTO UnsentData|SELECT Id, Data FROM UnsentData|UPDATE UnsentData SET Is_Sent|Array.Copy\(retestArray|Dim timer As New Timer|Dim cts As New CancellationTokenSource'
```

预期结果：旧的不安全模式已经移除，或在代码注释中明确说明为什么保留。

---

## 提交计划

如果已初始化源码管理，建议按以下顺序提交：

```bash
git add SqlServerHelper.vb
git commit -m "fix: add routed MES offline cache"

git add *Upload*.vb WIPInStationCheck.vb
git commit -m "fix: retry MES uploads only after business success"

git add Module1.vb
git commit -m "fix: bound retest array copy"

git add SocketClient.vb Heartbeat1.vb
git commit -m "fix: stabilize timers and socket shutdown"

git add clsDB.vb
git commit -m "fix: parameterize core database queries"
```

## 回滚计划

- 数据库结构变更只做加字段：即使应用代码回滚，`InterfaceName` 和 `TargetUrl` 字段也可以保留。
- 如果某个上传修复引发异常，先禁用对应上传定时器，并人工检查 `UnsentData` 后再决定是否标记已发送。
- 回滚期间不要删除待上传行；保留数据，等路由修复恢复后再重放。

## 自检

- 需求覆盖：两轮审查里的必须修复 bug 均已覆盖在任务 1-7 中。
- 占位检查：没有依赖 TBD/TODO 之类占位描述的步骤。
- 类型一致性：`PendingUpload.InterfaceName` 和 `PendingUpload.TargetUrl` 先定义后使用；上传类统一使用 `InterfaceName` 常量。
