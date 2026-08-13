Imports System.Data
Imports System.Data.SqlClient

''' <summary>
''' 兼容层:历史代码 (AlarmUpload / ResultDataUploadAssembly / WIPInStationCheck /
''' StateAndStopReasonUpload / EnergyUpload) 仍通过 SqlServerHelper.InsertUnsentData /
''' GetUnsentData / MarkUnsentDataAsSent 等接口写"失败重试缓存"。
''' R2-15 起这些方法内部转调到 OCV_DATA.UploadOutbox (usp_EnqueueUpload /
''' usp_GetPendingUploads / usp_MarkUploadSent / usp_MarkUploadFailed),
''' 仅保留兼容层,后续版本会逐步删除上传类的直接依赖。
''' </summary>
<Obsolete("SqlServerHelper 已迁移到 OCV_DATA.UploadOutbox,新增代码请直接使用 OcvDataService.EnqueueUpload / GetPendingUploads / MarkUploadSent / MarkUploadFailed。")>
Public Class SqlServerHelper
    Public adoConn As SqlConnection
    Public adoComm As SqlCommand
    Public DataSource As String '数据源
    Public DataServer As String '数据服务器
    Public UserID As String '用户名
    Public Userpwd As String '用户密码
    Public DataBaseStation As Boolean '数据库状态
    Private connectionString As String

    'LockedBy 标识,表示"通过 SqlServerHelper 兼容层取走的记录",
    'usp_GetPendingUploads 会将其置为 Uploading 状态,防止其他消费者重复取。
    Private Const CompatLockedBy As String = "SqlServerHelper.CompatLayer"

    Public Sub New()
        '保留构造函数以兼容历史调用方 (frmTest / FrmMesTask 等仍在 New SqlServerHelper())
        DataSource = "CWSQL"
        DataServer = Initsetting.strDatabase
        UserID = Initsetting.strUserID
        Userpwd = Initsetting.strpwd
        connectionString = "server=" & DataServer & ";" & "DataBase=" & DataSource & ";" & "User ID=" & UserID & ";" & "Password=" & Userpwd & ""
        pubConnectionString = connectionString
        SyncLock DBOpenLock
            If myConnection.State = ConnectionState.Closed Then
                myConnection.ConnectionString = connectionString
            End If
            DataBaseStation = (myConnection.State = ConnectionState.Open)
        End SyncLock
    End Sub

    Private Shared ReadOnly DBOpenLock As New Object()

    Public Function open(ByVal DataBaseStation As Boolean) As Boolean
        SyncLock DBOpenLock
            Try
                If myConnection.State = ConnectionState.Closed Then
                    If String.IsNullOrEmpty(myConnection.ConnectionString) Then
                        myConnection.ConnectionString = pubConnectionString
                    End If
                    myConnection.Open()
                End If
                open = (myConnection.State = ConnectionState.Open)
            Catch Err As Exception
                open = False
            End Try
        End SyncLock
    End Function

    ''' <summary>
    ''' 写入一条失败重试记录。
    ''' 内部调用 OcvDataService.EnqueueUpload -> dbo.UploadOutbox。
    ''' IdempotencyKey 由 InterfaceName + Data 内容的稳定哈希组成,
    ''' 保证同一 InterfaceName 的同一条 JSON 只会在 UploadOutbox 出现一次。
    ''' </summary>
    <Obsolete("请改用 OcvDataService.EnqueueUpload。")>
    Public Sub InsertUnsentData(data As String, interfaceName As String, targetUrl As String)
        Try
            Dim idempotencyKey As String = BuildIdempotencyKey(interfaceName, data)
            Using service As New OcvDataService()
                service.EnqueueUpload(idempotencyKey, interfaceName, targetUrl, data)
            End Using
        Catch ex As Exception
            '兼容层失败仅记录,不抛出,以免阻塞上传主流程
            Diagnostics.Debug.WriteLine("SqlServerHelper.InsertUnsentData -> UploadOutbox 失败: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' 兼容旧 API:读取本类未上传记录。
    ''' 内部走 usp_GetPendingUploads,LockedBy = "SqlServerHelper.CompatLayer" 防止并发重复取。
    ''' </summary>
    <Obsolete("请改用 OcvDataService.GetPendingUploads。")>
    Public Function GetUnsentData(interfaceName As String) As List(Of PendingUpload)
        Dim items As New List(Of PendingUpload)
        Try
            Using service As New OcvDataService()
                Dim table As DataTable = service.GetPendingUploads(interfaceName, 100, CompatLockedBy, 120)
                For Each row As DataRow In table.Rows
                    items.Add(New PendingUpload With {
                        .Id = Convert.ToInt64(row("UploadOutboxId")),
                        .JsonData = Convert.ToString(row("PayloadJson")),
                        .InterfaceName = Convert.ToString(row("UploadType")),
                        .TargetUrl = If(row.Table.Columns.Contains("BusinessKey"), Convert.ToString(row("BusinessKey")), String.Empty)
                    })
                Next
            End Using
        Catch ex As Exception
            Diagnostics.Debug.WriteLine("SqlServerHelper.GetUnsentData -> UploadOutbox 失败: " & ex.Message)
        End Try
        Return items
    End Function

    <Obsolete("请改用 OcvDataService.MarkUploadSent。")>
    Public Sub MarkUnsentDataAsSent(id As Long)
        Try
            Using service As New OcvDataService()
                service.MarkUploadSent(id)
            End Using
        Catch ex As Exception
            Diagnostics.Debug.WriteLine("SqlServerHelper.MarkUnsentDataAsSent -> UploadOutbox 失败: " & ex.Message)
        End Try
    End Sub

    '----------------- PendingUploads 旧表兼容 (R2 早期遗留,目前已无调用方) -----------------

    <Obsolete("PendingUploads 旧表已废弃,请改用 OcvDataService.EnqueueUpload。")>
    Public Sub InsertPendingUpload(jsonData As String, interfaceName As String)
        InsertUnsentData(jsonData, interfaceName, String.Empty)
    End Sub

    <Obsolete("PendingUploads 旧表已废弃,请改用 OcvDataService.GetPendingUploads。")>
    Public Function GetPendingUploads() As List(Of PendingUpload)
        'PendingUploads 旧表无 UploadType 字段,这里用通用占位类型读取
        Return GetUnsentData("PendingUploadLegacy")
    End Function

    <Obsolete("PendingUploads 旧表已废弃,请改用 OcvDataService.MarkUploadSent。")>
    Public Sub DeletePendingUpload(id As Integer)
        MarkUnsentDataAsSent(CLng(id))
    End Sub

    '----------------- 仍走旧表的运维辅助方法 (保留签名以便 MDIForm 健康检查兼容) -----------------

    <Obsolete("UnsentData 旧表已迁移,新代码请用 UploadOutbox。")>
    Public Sub EnsureUnsentDataSchema()
        '兼容层:不再在 CWSQL 建表,但保留调用入口以避免破坏历史引用
    End Sub

    <Obsolete("UnsentData 旧表已迁移,新代码请用 UploadOutbox。")>
    Public Function GetLegacyUnknownUnsentCount() As Integer
        '兼容层:不再读取旧表,固定返回 0
        Return 0
    End Function

    <Obsolete("UnsentData 旧表已迁移,新代码请用 UploadOutbox。")>
    Public Function GetLegacyUnknownUnsentData() As List(Of PendingUpload)
        Return New List(Of PendingUpload)()
    End Function

    ''' <summary>
    ''' 生成 UploadOutbox 的 IdempotencyKey。
    ''' 旧 UnsentData 没有业务主键,这里用 InterfaceName + Data SHA1 拼接,
    ''' 保证"同一上传类 + 同一 JSON 内容"只产生一条 outbox 记录,达到去重效果。
    ''' </summary>
    Private Shared Function BuildIdempotencyKey(interfaceName As String, data As String) As String
        Dim hash As String = ComputeShortHash(data)
        Return interfaceName & ":" & hash
    End Function

    Private Shared Function ComputeShortHash(value As String) As String
        If String.IsNullOrEmpty(value) Then
            Return "empty"
        End If
        Using sha As New System.Security.Cryptography.SHA1CryptoServiceProvider()
            Dim bytes() As Byte = System.Text.Encoding.UTF8.GetBytes(value)
            Dim hashBytes() As Byte = sha.ComputeHash(bytes)
            Dim sb As New System.Text.StringBuilder(40)
            For Each b As Byte In hashBytes
                sb.Append(b.ToString("x2"))
            Next
            Return sb.ToString()
        End Using
    End Function
End Class

Public Class PendingUpload
    Public Property Id As Long
    Public Property JsonData As String
    Public Property InterfaceName As String
    Public Property TargetUrl As String
End Class