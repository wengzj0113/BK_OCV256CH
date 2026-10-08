Imports System.Data
Imports System.Data.SqlClient
Imports System.Linq

Public Class OcvDataService
    Implements IDisposable

    Public Const DatabaseName As String = "OCV_DATA"

    Private ReadOnly connectionString As String
    Private disposed As Boolean = False

    Public Sub New()
        Me.New(BuildConnectionString())
    End Sub

    Public Sub New(ByVal sqlConnectionString As String)
        If String.IsNullOrWhiteSpace(sqlConnectionString) Then
            Throw New ArgumentException("SQL Server connection string is required.", NameOf(sqlConnectionString))
        End If

        connectionString = sqlConnectionString
    End Sub

    Public Shared Function BuildConnectionString() As String
        Dim builder As New SqlConnectionStringBuilder()
        builder.DataSource = Initsetting.strDatabase
        builder.InitialCatalog = DatabaseName
        builder.UserID = Initsetting.strUserID
        builder.Password = Initsetting.strpwd
        builder.ConnectTimeout = 5
        builder.MultipleActiveResultSets = False
        builder.PersistSecurityInfo = False
        Return builder.ConnectionString
    End Function

    Public Function CheckDatabaseHealth() As OcvDatabaseHealthResult
        Dim result As New OcvDatabaseHealthResult()

        Try
            Using connection As New SqlConnection(connectionString)
                connection.Open()
                Using command As SqlCommand = CreateStoredProcedureCommand(connection, "dbo.usp_CheckDatabaseHealth")
                    Using reader As SqlDataReader = command.ExecuteReader()
                        While reader.Read()
                            Dim item As New OcvDatabaseHealthItem()
                            item.CheckName = Convert.ToString(reader("CheckName"))
                            item.IsOk = Convert.ToBoolean(reader("IsOk"))
                            item.Detail = Convert.ToString(reader("Detail"))
                            result.Items.Add(item)
                        End While
                    End Using
                End Using
            End Using

            result.IsSuccess = result.Items.Count > 0 AndAlso result.Items.All(Function(item) item.IsOk)
            result.Message = If(result.IsSuccess, "OCV_DATA database health check passed.", BuildHealthErrorMessage(result.Items))
        Catch ex As Exception
            result.IsSuccess = False
            result.Message = "OCV_DATA database health check failed: " & ex.Message
        End Try

        Return result
    End Function

    Public Function RegisterTray(ByVal trayCode As String,
                                 Optional ByVal sourceType As String = "UpperComputer",
                                 Optional ByVal operatorName As String = Nothing) As DataTable
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As SqlCommand = CreateStoredProcedureCommand(connection, "dbo.usp_RegisterTray")
                AddParameter(command, "@TrayCode", SqlDbType.NVarChar, 100, trayCode)
                AddParameter(command, "@SourceType", SqlDbType.NVarChar, 30, sourceType)
                AddParameter(command, "@OperatorName", SqlDbType.NVarChar, 100, operatorName)

                Dim trayIdParameter As SqlParameter = command.Parameters.Add("@TrayId", SqlDbType.BigInt)
                trayIdParameter.Direction = ParameterDirection.Output
                command.Parameters.Add("@ReturnRow", SqlDbType.Bit).Value = True

                Return ExecuteDataTable(command)
            End Using
        End Using
    End Function

    Public Function SaveTrayCells(ByVal trayCode As String,
                                  ByVal cells As IEnumerable(Of OcvTrayCellInput),
                                  Optional ByVal operatorName As String = Nothing,
                                  Optional ByVal workstationName As String = Nothing) As DataTable
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As SqlCommand = CreateStoredProcedureCommand(connection, "dbo.usp_SaveTrayCells")
                AddParameter(command, "@TrayCode", SqlDbType.NVarChar, 100, trayCode)
                AddStructuredParameter(command, "@Cells", "dbo.TrayCellInputType", BuildTrayCellTable(cells))
                AddParameter(command, "@OperatorName", SqlDbType.NVarChar, 100, operatorName)
                AddParameter(command, "@WorkstationName", SqlDbType.NVarChar, 100, workstationName)
                Return ExecuteDataTable(command)
            End Using
        End Using
    End Function

    Public Function GetTrayCellsByTrayCode(ByVal trayCode As String) As DataTable
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As SqlCommand = CreateStoredProcedureCommand(connection, "dbo.usp_GetTrayCellsByTrayCode")
                AddParameter(command, "@TrayCode", SqlDbType.NVarChar, 100, trayCode)
                Return ExecuteDataTable(command)
            End Using
        End Using
    End Function

    Public Function CreateTestTask(ByVal request As OcvCreateTestTaskRequest) As DataTable
        If request Is Nothing Then
            Throw New ArgumentNullException(NameOf(request))
        End If

        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As SqlCommand = CreateStoredProcedureCommand(connection, "dbo.usp_CreateTestTask")
                AddParameter(command, "@TaskCode", SqlDbType.NVarChar, 100, request.TaskCode)
                AddParameter(command, "@IdempotencyKey", SqlDbType.NVarChar, 200, request.IdempotencyKey)
                AddParameter(command, "@TrayCode", SqlDbType.NVarChar, 100, request.TrayCode)
                AddParameter(command, "@OcvStageCode", SqlDbType.NVarChar, 20, request.OcvStageCode)
                AddParameter(command, "@TestType", SqlDbType.NVarChar, 30, request.TestType)
                AddParameter(command, "@SelectedChannelsJson", SqlDbType.NVarChar, -1, request.SelectedChannelsJson)
                AddParameter(command, "@FirstChannelNo", SqlDbType.SmallInt, request.FirstChannelNo)
                AddParameter(command, "@LastChannelNo", SqlDbType.SmallInt, request.LastChannelNo)
                AddParameter(command, "@ChannelCount", SqlDbType.Int, request.ChannelCount)
                AddParameter(command, "@EquipmentCode", SqlDbType.NVarChar, 50, request.EquipmentCode)
                AddParameter(command, "@WorkstationName", SqlDbType.NVarChar, 100, request.WorkstationName)
                AddParameter(command, "@OperatorName", SqlDbType.NVarChar, 100, request.OperatorName)
                command.Parameters.Add("@IsRetest", SqlDbType.Bit).Value = request.IsRetest
                AddParameter(command, "@TestSpecId", SqlDbType.Int, request.TestSpecId)
                Return ExecuteDataTable(command)
            End Using
        End Using
    End Function

    Public Function SaveTestResults(ByVal testTaskId As Long,
                                    ByVal results As IEnumerable(Of OcvTestResultInput)) As DataTable
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As SqlCommand = CreateStoredProcedureCommand(connection, "dbo.usp_SaveTestResults")
                command.Parameters.Add("@TestTaskId", SqlDbType.BigInt).Value = testTaskId
                AddStructuredParameter(command, "@Results", "dbo.TestResultInputType", BuildTestResultTable(results))
                Return ExecuteDataTable(command)
            End Using
        End Using
    End Function

    Public Function CompleteTestTask(ByVal testTaskId As Long,
                                     Optional ByVal status As String = "Completed",
                                     Optional ByVal errorMessage As String = Nothing) As DataTable
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As SqlCommand = CreateStoredProcedureCommand(connection, "dbo.usp_CompleteTestTask")
                command.Parameters.Add("@TestTaskId", SqlDbType.BigInt).Value = testTaskId
                AddParameter(command, "@Status", SqlDbType.NVarChar, 30, status)
                AddParameter(command, "@ErrorMessage", SqlDbType.NVarChar, -1, errorMessage)
                Return ExecuteDataTable(command)
            End Using
        End Using
    End Function

    Public Function GetTrayLatestStageResults(ByVal trayCode As String,
                                              Optional ByVal ocvStageCode As String = Nothing,
                                              Optional ByVal debugMode As Boolean = False) As DataTable
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As SqlCommand = CreateStoredProcedureCommand(connection, "dbo.usp_GetTrayLatestStageResults")
                AddParameter(command, "@TrayCode", SqlDbType.NVarChar, 100, trayCode)
                AddParameter(command, "@OcvStageCode", SqlDbType.NVarChar, 20, ocvStageCode)
                command.Parameters.Add("@DebugMode", SqlDbType.Bit).Value = debugMode
                Return ExecuteDataTable(command)
            End Using
        End Using
    End Function

    Public Function GetTrayListByDate(ByVal startTime As DateTime,
                                      ByVal endTime As DateTime,
                                      Optional ByVal ocvStageCode As String = Nothing,
                                      Optional ByVal debugMode As Boolean = False) As DataTable
        Dim sql As String =
            "SELECT DISTINCT TrayCode " &
            "FROM dbo.vw_CellTestSummary " &
            "WHERE ResultCreatedAt >= @StartTime AND ResultCreatedAt < @EndTime " &
            "AND (@OcvStageCode IS NULL OR OcvStage = @OcvStageCode) " &
            "AND ((@DebugMode = 1 AND TestType = N'Debug') OR (@DebugMode = 0 AND TestType <> N'Debug')) " &
            "ORDER BY TrayCode"

        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As New SqlCommand(sql, connection)
                command.CommandType = CommandType.Text
                command.CommandTimeout = 30
                command.Parameters.Add("@StartTime", SqlDbType.DateTime2).Value = startTime
                command.Parameters.Add("@EndTime", SqlDbType.DateTime2).Value = endTime
                AddParameter(command, "@OcvStageCode", SqlDbType.NVarChar, 20, ocvStageCode)
                command.Parameters.Add("@DebugMode", SqlDbType.Bit).Value = debugMode
                Return ExecuteDataTable(command)
            End Using
        End Using
    End Function

    Public Function EnqueueUpload(ByVal idempotencyKey As String,
                                  ByVal uploadType As String,
                                  ByVal businessKey As String,
                                  ByVal payloadJson As String,
                                  Optional ByVal testTaskId As Long? = Nothing) As DataTable
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As SqlCommand = CreateStoredProcedureCommand(connection, "dbo.usp_EnqueueUpload")
                AddParameter(command, "@IdempotencyKey", SqlDbType.NVarChar, 200, idempotencyKey)
                AddParameter(command, "@UploadType", SqlDbType.NVarChar, 50, uploadType)
                AddParameter(command, "@BusinessKey", SqlDbType.NVarChar, 200, businessKey)
                AddParameter(command, "@TestTaskId", SqlDbType.BigInt, testTaskId)
                AddParameter(command, "@PayloadJson", SqlDbType.NVarChar, -1, payloadJson)
                Return ExecuteDataTable(command)
            End Using
        End Using
    End Function

    Public Function GetActiveCalibrationValues() As DataTable
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As SqlCommand = CreateStoredProcedureCommand(connection, "dbo.usp_GetActiveCalibrationValues")
                Return ExecuteDataTable(command)
            End Using
        End Using
    End Function

    Public Function GetPendingUploads(ByVal uploadType As String,
                                      ByVal batchSize As Integer,
                                      ByVal lockedBy As String,
                                      Optional ByVal lockSeconds As Integer = 120) As DataTable
        'usp_GetPendingUploads 不支持 @UploadType 过滤,这里用内联 SQL 实现按类型过滤的并发安全锁定
        Dim sql As String =
            "UPDATE TOP (@BatchSize) dbo.UploadOutbox WITH (READPAST, UPDLOCK, ROWLOCK) " &
            "SET Status = N'Uploading', LockedBy = @LockedBy, LockedAt = sysdatetime(), UpdatedAt = sysdatetime() " &
            "OUTPUT inserted.* " &
            "WHERE UploadType = @UploadType " &
            "AND Status IN (N'Pending', N'Failed', N'Uploading') " &
            "AND NextRetryAt <= sysdatetime() " &
            "AND (Status <> N'Uploading' OR LockedAt IS NULL OR dateadd(second, @LockSeconds, LockedAt) < sysdatetime())"
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As New SqlCommand(sql, connection)
                command.CommandType = CommandType.Text
                command.CommandTimeout = 30
                AddParameter(command, "@UploadType", SqlDbType.NVarChar, 50, uploadType)
                AddParameter(command, "@BatchSize", SqlDbType.Int, batchSize)
                AddParameter(command, "@LockedBy", SqlDbType.NVarChar, 100, lockedBy)
                AddParameter(command, "@LockSeconds", SqlDbType.Int, lockSeconds)
                Return ExecuteDataTable(command)
            End Using
        End Using
    End Function

    Public Sub MarkUploadSent(ByVal uploadOutboxId As Long)
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As SqlCommand = CreateStoredProcedureCommand(connection, "dbo.usp_MarkUploadSent")
                command.Parameters.Add("@UploadOutboxId", SqlDbType.BigInt).Value = uploadOutboxId
                command.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Sub MarkUploadFailed(ByVal uploadOutboxId As Long,
                                ByVal errorMessage As String,
                                Optional ByVal retryDelaySeconds As Integer = 60)
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As SqlCommand = CreateStoredProcedureCommand(connection, "dbo.usp_MarkUploadFailed")
                command.Parameters.Add("@UploadOutboxId", SqlDbType.BigInt).Value = uploadOutboxId
                AddParameter(command, "@ErrorMessage", SqlDbType.NVarChar, -1, errorMessage)
                command.Parameters.Add("@RetryDelaySeconds", SqlDbType.Int).Value = retryDelaySeconds
                command.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Sub SaveMesUploadLog(ByVal uploadOutboxId As Long?,
                                ByVal uploadType As String,
                                ByVal isSuccess As Boolean,
                                Optional ByVal testTaskId As Long? = Nothing,
                                Optional ByVal businessKey As String = Nothing,
                                Optional ByVal requestJson As String = Nothing,
                                Optional ByVal responseJson As String = Nothing,
                                Optional ByVal errorMessage As String = Nothing)
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As SqlCommand = CreateStoredProcedureCommand(connection, "dbo.usp_SaveMesUploadLog")
                command.Parameters.Add("@UploadOutboxId", SqlDbType.BigInt).Value = If(uploadOutboxId.HasValue, CType(uploadOutboxId.Value, Object), DBNull.Value)
                command.Parameters.Add("@TestTaskId", SqlDbType.BigInt).Value = If(testTaskId.HasValue, CType(testTaskId.Value, Object), DBNull.Value)
                AddParameter(command, "@UploadType", SqlDbType.NVarChar, 50, uploadType)
                AddParameter(command, "@BusinessKey", SqlDbType.NVarChar, 200, businessKey)
                AddParameter(command, "@RequestJson", SqlDbType.NVarChar, -1, requestJson)
                AddParameter(command, "@ResponseJson", SqlDbType.NVarChar, -1, responseJson)
                command.Parameters.Add("@IsSuccess", SqlDbType.Bit).Value = isSuccess
                AddParameter(command, "@ErrorMessage", SqlDbType.NVarChar, -1, errorMessage)
                command.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    '----------------- 分档规则系统 (R2-15) -----------------

    ''' <summary>
    ''' 返回所有活跃的分档方案 (GradeScheme)。
    ''' </summary>
    Public Function GetGradeSchemes() As DataTable
        Dim sql As String =
            "SELECT gs.GradeSchemeId, gs.SchemeName, os.StageCode AS OcvStageCode, gs.IsActive " &
            "FROM dbo.GradeScheme gs " &
            "LEFT JOIN dbo.OcvStage os ON os.OcvStageId = gs.OcvStageId " &
            "WHERE gs.IsActive = 1 " &
            "ORDER BY gs.SchemeName"
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As New SqlCommand(sql, connection)
                command.CommandType = CommandType.Text
                command.CommandTimeout = 30
                Return ExecuteDataTable(command)
            End Using
        End Using
    End Function

    ''' <summary>
    ''' 返回指定方案下的所有分档规则 (GradeRule),按 GradeCode + SortOrder 排序。
    ''' </summary>
    Public Function GetGradeRules(ByVal schemeName As String) As DataTable
        Dim sql As String =
            "SELECT gr.GradeCode, gr.FieldName, gr.LowerLimit, gr.UpperLimit, gr.SortOrder, " &
            "os.LegacyColumnSuffix " &
            "FROM dbo.GradeRule gr " &
            "INNER JOIN dbo.GradeScheme gs ON gs.GradeSchemeId = gr.GradeSchemeId " &
            "LEFT JOIN dbo.OcvStage os ON os.OcvStageId = gs.OcvStageId " &
            "WHERE gs.SchemeName = @SchemeName AND gs.IsActive = 1 " &
            "ORDER BY gr.GradeCode, gr.SortOrder"
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As New SqlCommand(sql, connection)
                command.CommandType = CommandType.Text
                command.CommandTimeout = 30
                AddParameter(command, "@SchemeName", SqlDbType.NVarChar, 100, schemeName)
                Return ExecuteDataTable(command)
            End Using
        End Using
    End Function

    ''' <summary>
    ''' 返回指定方案下的所有档位名称 (去重)。
    ''' </summary>
    Public Function GetGradeCodes(ByVal schemeName As String) As DataTable
        Dim sql As String =
            "SELECT DISTINCT gr.GradeCode " &
            "FROM dbo.GradeRule gr " &
            "INNER JOIN dbo.GradeScheme gs ON gs.GradeSchemeId = gr.GradeSchemeId " &
            "WHERE gs.SchemeName = @SchemeName AND gs.IsActive = 1 " &
            "ORDER BY gr.GradeCode"
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As New SqlCommand(sql, connection)
                command.CommandType = CommandType.Text
                command.CommandTimeout = 30
                AddParameter(command, "@SchemeName", SqlDbType.NVarChar, 100, schemeName)
                Return ExecuteDataTable(command)
            End Using
        End Using
    End Function

    ''' <summary>
    ''' 返回所有活跃的可选分档字段 (GradeSelectableField)。
    ''' </summary>
    Public Function GetGradeSelectableFields() As DataTable
        Dim sql As String =
            "SELECT FieldName, DisplayName, UnitName, IsActive " &
            "FROM dbo.GradeSelectableField " &
            "WHERE IsActive = 1 " &
            "ORDER BY DisplayName"
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As New SqlCommand(sql, connection)
                command.CommandType = CommandType.Text
                command.CommandTimeout = 3
                Return ExecuteDataTable(command)
            End Using
        End Using
    End Function

    '----------------- 存在性校验 (R2-15 替代 clsDB.IsExists) -----------------

    ''' <summary>
    ''' 检查托盘编码是否已存在。替代 clsDB.IsExists (R2-15)。
    ''' </summary>
    Public Function CheckTrayCodeExists(trayCode As String) As Boolean
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As SqlCommand = CreateStoredProcedureCommand(connection, "dbo.usp_CheckTrayCodeExists")
                Dim existsParam As SqlParameter = command.Parameters.Add("@Exists", SqlDbType.Bit)
                existsParam.Direction = ParameterDirection.Output
                AddParameter(command, "@TrayCode", SqlDbType.NVarChar, 100, trayCode)
                command.ExecuteNonQuery()
                Return CBool(existsParam.Value)
            End Using
        End Using
    End Function

    ''' <summary>
    ''' 检查分档方案名是否已存在。isActive 为 Nothing 表示不限,True 仅查活跃,False 仅查非活跃。
    ''' 替代 clsDB.IsExists (R2-15)。
    ''' </summary>
    Public Function CheckGradeSchemeExists(schemeName As String, Optional isActive As Boolean? = Nothing) As Boolean
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As SqlCommand = CreateStoredProcedureCommand(connection, "dbo.usp_CheckGradeSchemeExists")
                Dim existsParam As SqlParameter = command.Parameters.Add("@Exists", SqlDbType.Bit)
                existsParam.Direction = ParameterDirection.Output
                AddParameter(command, "@SchemeName", SqlDbType.NVarChar, 100, schemeName)
                Dim isActiveParam As SqlParameter = command.Parameters.Add("@IsActive", SqlDbType.Bit)
                isActiveParam.Value = If(isActive.HasValue, CType(isActive.Value, Object), DBNull.Value)
                command.ExecuteNonQuery()
                Return CBool(existsParam.Value)
            End Using
        End Using
    End Function

    ''' <summary>
    ''' 删除指定方案下的所有分档规则,并停用方案本身。
    ''' </summary>
    Public Sub DeleteGradeRules(ByVal schemeName As String)
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using transaction As SqlTransaction = connection.BeginTransaction()
                Try
                    Dim schemeId As Object = Nothing
                    Using cmd As New SqlCommand(
                        "SELECT GradeSchemeId FROM dbo.GradeScheme WHERE SchemeName = @SchemeName AND IsActive = 1",
                        connection, transaction)
                        AddParameter(cmd, "@SchemeName", SqlDbType.NVarChar, 100, schemeName)
                        schemeId = cmd.ExecuteScalar()
                    End Using
                    If schemeId IsNot Nothing AndAlso schemeId IsNot DBNull.Value Then
                        Using cmd As New SqlCommand(
                            "DELETE FROM dbo.GradeRule WHERE GradeSchemeId = @GradeSchemeId",
                            connection, transaction)
                            cmd.Parameters.Add("@GradeSchemeId", SqlDbType.Int).Value = Convert.ToInt32(schemeId)
                            cmd.ExecuteNonQuery()
                        End Using
                    End If
                    transaction.Commit()
                Catch ex As Exception
                    transaction.Rollback()
                    Throw
                End Try
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' 保存一条分档规则。如果方案不存在则自动创建。
    ''' </summary>
    Public Sub SaveGradeRule(ByVal schemeName As String,
                              ByVal gradeCode As String,
                              ByVal fieldName As String,
                              ByVal lowerLimit As Decimal?,
                              ByVal upperLimit As Decimal?,
                              Optional ByVal sortOrder As Integer = 0)
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using transaction As SqlTransaction = connection.BeginTransaction()
                Try
                    Dim schemeId As Integer
                    Using cmd As New SqlCommand(
                        "SELECT GradeSchemeId FROM dbo.GradeScheme WHERE SchemeName = @SchemeName AND IsActive = 1",
                        connection, transaction)
                        AddParameter(cmd, "@SchemeName", SqlDbType.NVarChar, 100, schemeName)
                        Dim result As Object = cmd.ExecuteScalar()
                        If result Is Nothing OrElse result Is DBNull.Value Then
                            Using insCmd As New SqlCommand(
                                "INSERT INTO dbo.GradeScheme(SchemeName, IsActive) VALUES (@SchemeName, 1); SELECT CAST(SCOPE_IDENTITY() AS INT)",
                                connection, transaction)
                                AddParameter(insCmd, "@SchemeName", SqlDbType.NVarChar, 100, schemeName)
                                schemeId = Convert.ToInt32(insCmd.ExecuteScalar())
                            End Using
                        Else
                            schemeId = Convert.ToInt32(result)
                        End If
                    End Using
                    Using cmd As New SqlCommand(
                        "INSERT INTO dbo.GradeRule(GradeSchemeId, GradeCode, FieldName, LowerLimit, UpperLimit, SortOrder) " &
                        "VALUES (@GradeSchemeId, @GradeCode, @FieldName, @LowerLimit, @UpperLimit, @SortOrder)",
                        connection, transaction)
                        cmd.Parameters.Add("@GradeSchemeId", SqlDbType.Int).Value = schemeId
                        AddParameter(cmd, "@GradeCode", SqlDbType.NVarChar, 50, gradeCode)
                        AddParameter(cmd, "@FieldName", SqlDbType.NVarChar, 50, fieldName)
                        If lowerLimit.HasValue Then
                            cmd.Parameters.Add("@LowerLimit", SqlDbType.Decimal).Value = lowerLimit.Value
                            CType(cmd.Parameters("@LowerLimit"), SqlParameter).Precision = 18
                            CType(cmd.Parameters("@LowerLimit"), SqlParameter).Scale = 9
                        Else
                            cmd.Parameters.Add("@LowerLimit", SqlDbType.Decimal).Value = DBNull.Value
                        End If
                        If upperLimit.HasValue Then
                            cmd.Parameters.Add("@UpperLimit", SqlDbType.Decimal).Value = upperLimit.Value
                            CType(cmd.Parameters("@UpperLimit"), SqlParameter).Precision = 18
                            CType(cmd.Parameters("@UpperLimit"), SqlParameter).Scale = 9
                        Else
                            cmd.Parameters.Add("@UpperLimit", SqlDbType.Decimal).Value = DBNull.Value
                        End If
                        cmd.Parameters.Add("@SortOrder", SqlDbType.Int).Value = sortOrder
                        cmd.ExecuteNonQuery()
                    End Using
                    transaction.Commit()
                Catch ex As Exception
                    transaction.Rollback()
                    Throw
                End Try
            End Using
        End Using
    End Sub

    Public Function SaveChannelCalibration(ByVal channelNo As Integer,
                                           ByVal calibrationValueMilliohm As Decimal,
                                           Optional ByVal standardValue As Decimal? = Nothing,
                                           Optional ByVal measuredValue As Decimal? = Nothing,
                                           Optional ByVal operatorName As String = Nothing,
                                           Optional ByVal remark As String = Nothing) As DataTable
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As SqlCommand = CreateStoredProcedureCommand(connection, "dbo.usp_SaveChannelCalibration")
                command.Parameters.Add("@ChannelNo", SqlDbType.SmallInt).Value = channelNo
                AddParameter(command, "@StandardValue", SqlDbType.Decimal, standardValue, 18, 6)
                AddParameter(command, "@MeasuredValue", SqlDbType.Decimal, measuredValue, 18, 6)
                AddParameter(command, "@CalibrationValueMilliohm", SqlDbType.Decimal, calibrationValueMilliohm, 18, 6)
                AddParameter(command, "@OperatorName", SqlDbType.NVarChar, 100, operatorName)
                AddParameter(command, "@Remark", SqlDbType.NVarChar, 500, remark)
                Return ExecuteDataTable(command)
            End Using
        End Using
    End Function

    Public Sub SaveDeviceCommLog(ByVal deviceName As String,
                                 ByVal direction As String,
                                 Optional ByVal commandName As String = Nothing,
                                 Optional ByVal frameHex As String = Nothing,
                                 Optional ByVal isSuccess As Boolean = True,
                                 Optional ByVal errorMessage As String = Nothing,
                                 Optional ByVal testTaskId As Long? = Nothing)
        Using connection As New SqlConnection(connectionString)
            connection.Open()
            Using command As SqlCommand = CreateStoredProcedureCommand(connection, "dbo.usp_SaveDeviceCommLog")
                AddParameter(command, "@TestTaskId", SqlDbType.BigInt, testTaskId)
                AddParameter(command, "@DeviceName", SqlDbType.NVarChar, 50, deviceName)
                AddParameter(command, "@Direction", SqlDbType.NVarChar, 10, direction)
                AddParameter(command, "@CommandName", SqlDbType.NVarChar, 100, commandName)
                AddParameter(command, "@FrameHex", SqlDbType.NVarChar, -1, frameHex)
                command.Parameters.Add("@IsSuccess", SqlDbType.Bit).Value = isSuccess
                AddParameter(command, "@ErrorMessage", SqlDbType.NVarChar, -1, errorMessage)
                command.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Private Shared Function CreateStoredProcedureCommand(ByVal connection As SqlConnection,
                                                         ByVal procedureName As String) As SqlCommand
        Dim command As New SqlCommand(procedureName, connection)
        command.CommandType = CommandType.StoredProcedure
        command.CommandTimeout = 30
        Return command
    End Function

    Private Shared Sub AddParameter(ByVal command As SqlCommand,
                                    ByVal name As String,
                                    ByVal type As SqlDbType,
                                    ByVal size As Integer,
                                    ByVal value As Object)
        Dim parameter As SqlParameter = command.Parameters.Add(name, type, size)
        parameter.Value = If(value Is Nothing, DBNull.Value, value)
    End Sub

    Private Shared Sub AddParameter(ByVal command As SqlCommand,
                                    ByVal name As String,
                                    ByVal type As SqlDbType,
                                    ByVal value As Object)
        Dim parameter As SqlParameter = command.Parameters.Add(name, type)
        parameter.Value = If(value Is Nothing, DBNull.Value, value)
    End Sub

    Private Shared Sub AddParameter(ByVal command As SqlCommand,
                                    ByVal name As String,
                                    ByVal type As SqlDbType,
                                    ByVal value As Decimal?,
                                    ByVal precision As Byte,
                                    ByVal scale As Byte)
        Dim parameter As SqlParameter = command.Parameters.Add(name, type)
        parameter.Precision = precision
        parameter.Scale = scale
        parameter.Value = If(value.HasValue, CType(value.Value, Object), DBNull.Value)
    End Sub

    Private Shared Sub AddStructuredParameter(ByVal command As SqlCommand,
                                              ByVal name As String,
                                              ByVal typeName As String,
                                              ByVal table As DataTable)
        Dim parameter As SqlParameter = command.Parameters.Add(name, SqlDbType.Structured)
        parameter.TypeName = typeName
        parameter.Value = table
    End Sub

    Private Shared Function ExecuteDataTable(ByVal command As SqlCommand) As DataTable
        Dim table As New DataTable()
        Using adapter As New SqlDataAdapter(command)
            adapter.Fill(table)
        End Using
        Return table
    End Function

    Private Shared Function BuildTrayCellTable(ByVal cells As IEnumerable(Of OcvTrayCellInput)) As DataTable
        Dim table As New DataTable()
        table.Columns.Add("ChannelNo", GetType(Short))
        table.Columns.Add("BatteryCode", GetType(String))
        table.Columns.Add("SourceType", GetType(String))
        table.Columns.Add("RowNo", GetType(Byte))
        table.Columns.Add("ColNo", GetType(Byte))

        If cells IsNot Nothing Then
            For Each cell As OcvTrayCellInput In cells
                Dim row As DataRow = table.NewRow()
                row("ChannelNo") = CShort(cell.ChannelNo)
                row("BatteryCode") = DbValue(cell.BatteryCode)
                row("SourceType") = DbValue(cell.SourceType)
                row("RowNo") = If(cell.RowNo.HasValue, CType(cell.RowNo.Value, Object), DBNull.Value)
                row("ColNo") = If(cell.ColNo.HasValue, CType(cell.ColNo.Value, Object), DBNull.Value)
                table.Rows.Add(row)
            Next
        End If

        Return table
    End Function

    Private Shared Function BuildTestResultTable(ByVal results As IEnumerable(Of OcvTestResultInput)) As DataTable
        Dim table As New DataTable()
        table.Columns.Add("ChannelNo", GetType(Short))
        table.Columns.Add("BatteryCode", GetType(String))
        table.Columns.Add("ExecutionOrder", GetType(Integer))
        table.Columns.Add("VoltageVolt", GetType(Decimal))
        table.Columns.Add("RawResistanceMilliohm", GetType(Decimal))
        table.Columns.Add("CalibrationValueMilliohm", GetType(Decimal))
        table.Columns.Add("FinalResistanceMilliohm", GetType(Decimal))
        table.Columns.Add("IsNg", GetType(Boolean))
        table.Columns.Add("NgReason", GetType(String))
        table.Columns.Add("ResultSource", GetType(String))

        If results IsNot Nothing Then
            For Each result As OcvTestResultInput In results
                Dim row As DataRow = table.NewRow()
                row("ChannelNo") = CShort(result.ChannelNo)
                row("BatteryCode") = DbValue(result.BatteryCode)
                row("ExecutionOrder") = If(result.ExecutionOrder.HasValue, CType(result.ExecutionOrder.Value, Object), DBNull.Value)
                row("VoltageVolt") = If(result.VoltageVolt.HasValue, CType(result.VoltageVolt.Value, Object), DBNull.Value)
                row("RawResistanceMilliohm") = If(result.RawResistanceMilliohm.HasValue, CType(result.RawResistanceMilliohm.Value, Object), DBNull.Value)
                row("CalibrationValueMilliohm") = If(result.CalibrationValueMilliohm.HasValue, CType(result.CalibrationValueMilliohm.Value, Object), DBNull.Value)
                row("FinalResistanceMilliohm") = If(result.FinalResistanceMilliohm.HasValue, CType(result.FinalResistanceMilliohm.Value, Object), DBNull.Value)
                row("IsNg") = result.IsNg
                row("NgReason") = DbValue(result.NgReason)
                row("ResultSource") = DbValue(result.ResultSource)
                table.Rows.Add(row)
            Next
        End If

        Return table
    End Function

    Private Shared Function DbValue(ByVal value As String) As Object
        If String.IsNullOrWhiteSpace(value) Then
            Return DBNull.Value
        End If

        Return value
    End Function

    Private Shared Function BuildHealthErrorMessage(ByVal items As IEnumerable(Of OcvDatabaseHealthItem)) As String
        Dim failedItems As IEnumerable(Of OcvDatabaseHealthItem) = items.Where(Function(item) Not item.IsOk)
        If Not failedItems.Any() Then
            Return "OCV_DATA database health check returned no successful item."
        End If

        Return "OCV_DATA database health check failed: " &
               String.Join("; ", failedItems.Select(Function(item) item.CheckName & "=" & item.Detail))
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If disposed Then
            Return
        End If
        disposed = True
    End Sub
End Class

Public Class OcvDatabaseHealthResult
    Public Property IsSuccess As Boolean
    Public Property Message As String
    Public ReadOnly Property Items As List(Of OcvDatabaseHealthItem)

    Public Sub New()
        Items = New List(Of OcvDatabaseHealthItem)()
    End Sub
End Class

Public Class OcvDatabaseHealthItem
    Public Property CheckName As String
    Public Property IsOk As Boolean
    Public Property Detail As String
End Class

Public Class OcvTrayCellInput
    Public Property ChannelNo As Integer
    Public Property BatteryCode As String
    Public Property SourceType As String
    Public Property RowNo As Byte?
    Public Property ColNo As Byte?
End Class

Public Class OcvCreateTestTaskRequest
    Public Property TaskCode As String
    Public Property IdempotencyKey As String
    Public Property TrayCode As String
    Public Property OcvStageCode As String
    Public Property TestType As String
    Public Property SelectedChannelsJson As String
    Public Property FirstChannelNo As Integer?
    Public Property LastChannelNo As Integer?
    Public Property ChannelCount As Integer?
    Public Property EquipmentCode As String
    Public Property WorkstationName As String
    Public Property OperatorName As String
    Public Property IsRetest As Boolean
    Public Property TestSpecId As Integer?
End Class

Public Class OcvTestResultInput
    Public Property ChannelNo As Integer
    Public Property BatteryCode As String
    Public Property ExecutionOrder As Integer?
    Public Property VoltageVolt As Decimal?
    Public Property RawResistanceMilliohm As Decimal?
    Public Property CalibrationValueMilliohm As Decimal?
    Public Property FinalResistanceMilliohm As Decimal?
    Public Property IsNg As Boolean
    Public Property NgReason As String
    Public Property ResultSource As String
End Class
