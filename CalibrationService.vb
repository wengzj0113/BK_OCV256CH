Imports System.Data
Imports System.Data.SqlClient
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Linq

Public Class CalibrationService
    Public Const ChannelCount As Integer = 256
    Public Const DefaultFileName As String = "CalibrateArr.csv"
    Public Const MaxAbsCalibrationValue As Double = 1000.0

    Private Shared ReadOnly sharedInstanceValue As New CalibrationService()
    Private Shared ReadOnly snapshotLock As New Object()
    Private Shared currentSnapshotValue As CalibrationSnapshot
    Private Shared calibrationAcquisitionInProgressValue As Boolean
    Private Shared barcodeRetestInProgressValue As Boolean

    Private ReadOnly filePath As String
    Private lastCacheWriteWarningValue As String = String.Empty

    Public Sub New(Optional path As String = Nothing)
        If String.IsNullOrWhiteSpace(path) Then
            Dim dataDirectory As String = IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "XL_OCV_256CH")
            IO.Directory.CreateDirectory(dataDirectory)
            filePath = IO.Path.Combine(dataDirectory, DefaultFileName)
        Else
            filePath = path
        End If
    End Sub

    Public Shared ReadOnly Property SharedInstance As CalibrationService
        Get
            Return sharedInstanceValue
        End Get
    End Property

    Public Shared ReadOnly Property HasCurrentSnapshot As Boolean
        Get
            SyncLock snapshotLock
                Return currentSnapshotValue IsNot Nothing
            End SyncLock
        End Get
    End Property

    Public Shared ReadOnly Property IsCalibrationAcquisitionInProgress As Boolean
        Get
            SyncLock snapshotLock
                Return calibrationAcquisitionInProgressValue
            End SyncLock
        End Get
    End Property

    Public Shared ReadOnly Property IsBarcodeRetestInProgress As Boolean
        Get
            SyncLock snapshotLock
                Return barcodeRetestInProgressValue
            End SyncLock
        End Get
    End Property

    Public Shared Function TryBeginCalibrationAcquisition() As Boolean
        SyncLock snapshotLock
            If isRuning OrElse calibrationAcquisitionInProgressValue OrElse barcodeRetestInProgressValue Then Return False
            calibrationAcquisitionInProgressValue = True
            Return True
        End SyncLock
    End Function

    Public Shared Sub EndCalibrationAcquisition()
        SyncLock snapshotLock
            calibrationAcquisitionInProgressValue = False
        End SyncLock
    End Sub

    Public Shared Function TryBeginBarcodeRetest() As Boolean
        SyncLock snapshotLock
            If isRuning OrElse calibrationAcquisitionInProgressValue OrElse barcodeRetestInProgressValue Then Return False
            barcodeRetestInProgressValue = True
            Return True
        End SyncLock
    End Function

    Public Shared Sub EndBarcodeRetest()
        SyncLock snapshotLock
            barcodeRetestInProgressValue = False
        End SyncLock
    End Sub

    Public ReadOnly Property CalibrationFilePath As String
        Get
            Return filePath
        End Get
    End Property

    Private ReadOnly Property CacheInvalidationPath As String
        Get
            Return filePath & ".invalid"
        End Get
    End Property

    Public ReadOnly Property CurrentSnapshot As CalibrationSnapshot
        Get
            SyncLock snapshotLock
                If currentSnapshotValue Is Nothing Then
                    Throw New InvalidOperationException("校准快照尚未载入。")
                End If
                Return currentSnapshotValue
            End SyncLock
        End Get
    End Property

    Public ReadOnly Property LastCacheWriteWarning As String
        Get
            Return lastCacheWriteWarningValue
        End Get
    End Property

    Public Function LoadSnapshot() As CalibrationSnapshot
        Dim cachedValues As Double() = Nothing
        Dim cacheError As String = String.Empty
        Dim hasValidCache As Boolean = TryReadCache(cachedValues, cacheError)
        Dim databaseValues As Dictionary(Of Integer, Double) = Nothing
        Dim databaseAvailable As Boolean = False

        Try
            databaseValues = LoadActiveCalibrationValueMap()
            databaseAvailable = True
        Catch ex As SqlException
            Trace.WriteLine("Unable to load OCV_DATA calibration values: " & ex.Message)
        Catch ex As InvalidOperationException
            'Missing database configuration is an availability failure. Invalid calibration
            'data uses ArgumentException and must fail closed instead of falling back to cache.
            Trace.WriteLine("Unable to load OCV_DATA calibration values: " & ex.Message)
        End Try

        If Not databaseAvailable AndAlso Not hasValidCache Then
            Dim detail As String = If(String.IsNullOrWhiteSpace(cacheError), "本地校准缓存不存在。", cacheError)
            Throw New InvalidOperationException("数据库校准值不可用，且没有有效的本地校准缓存；已阻止启动测试。" & detail)
        End If

        Dim values(ChannelCount - 1) As Double
        Dim sources(ChannelCount - 1) As CalibrationValueSource
        Dim databaseChannels As Integer = 0
        Dim cacheChannels As Integer = 0
        Dim uncalibratedChannels As Integer = 0

        For i As Integer = 0 To ChannelCount - 1
            Dim channelNo As Integer = i + 1
            If databaseAvailable AndAlso databaseValues.ContainsKey(channelNo) Then
                values(i) = databaseValues(channelNo)
                sources(i) = CalibrationValueSource.Database
                databaseChannels += 1
            ElseIf hasValidCache Then
                values(i) = cachedValues(i)
                sources(i) = CalibrationValueSource.LocalCache
                cacheChannels += 1
            Else
                values(i) = 0.0R
                sources(i) = CalibrationValueSource.Uncalibrated
                uncalibratedChannels += 1
            End If
        Next

        Dim sourceDescription As String = BuildSourceDescription(databaseAvailable,
                                                                 databaseChannels,
                                                                 cacheChannels,
                                                                 uncalibratedChannels)
        Dim snapshot As New CalibrationSnapshot(values, sources, sourceDescription, filePath, databaseAvailable)
        lastCacheWriteWarningValue = String.Empty
        If databaseAvailable AndAlso (databaseChannels > 0 OrElse hasValidCache) Then
            Try
                WriteCacheAtomically(snapshot.CopyValues(), False)
            Catch ex As Exception
                lastCacheWriteWarningValue = "已载入数据库校准值，但更新本地缓存失败：" & ex.Message
                InvalidateCache(ex.Message)
                Trace.WriteLine(lastCacheWriteWarningValue)
            End Try
        End If

        PublishSnapshot(snapshot)
        Return snapshot
    End Function

    Public Function LoadValues() As Double()
        Return LoadSnapshot().CopyValues()
    End Function

    Public Sub SaveValues(values As Double(), Optional createBackup As Boolean = True)
        ValidateValues(values)
        Dim normalizedValues(ChannelCount - 1) As Double
        Dim databaseValues(ChannelCount - 1) As Decimal
        For i As Integer = 0 To ChannelCount - 1
            Dim normalized As Decimal = Decimal.Round(Convert.ToDecimal(values(i)), 6, MidpointRounding.AwayFromZero)
            normalizedValues(i) = Convert.ToDouble(normalized)
            databaseValues(i) = normalized
        Next

        Try
            Dim dataService As New OcvDataService()
            dataService.SaveCalibrationValuesBatch(databaseValues,
                                                   Initsetting.curOperator,
                                                   "Saved from upper-computer calibration page.")
        Catch ex As Exception
            Throw New InvalidOperationException("保存校准值到 OCV_DATA 失败；当前生效快照未更改：" & ex.Message, ex)
        End Try

        Dim sources(ChannelCount - 1) As CalibrationValueSource
        For i As Integer = 0 To ChannelCount - 1
            sources(i) = CalibrationValueSource.Database
        Next

        lastCacheWriteWarningValue = String.Empty
        Try
            WriteCacheAtomically(normalizedValues, createBackup)
        Catch ex As Exception
            lastCacheWriteWarningValue = "校准值已保存到 OCV_DATA 并已在当前进程生效，但本地缓存更新失败：" & ex.Message
            InvalidateCache(ex.Message)
            Trace.WriteLine(lastCacheWriteWarningValue)
        End Try

        Dim sourceDescription As String = If(String.IsNullOrWhiteSpace(lastCacheWriteWarningValue),
                                             "OCV_DATA 数据库",
                                             "OCV_DATA 数据库（本地缓存更新失败）")
        PublishSnapshot(New CalibrationSnapshot(normalizedValues, sources, sourceDescription, filePath, True))
    End Sub

    Public Function CalculateCompensation(standardResistance As Double, measuredResistance As Double) As Double
        Return standardResistance - measuredResistance
    End Function

    Public Shared Function MapStandardChannelValuesToDeviceChannels(standardChannelValues As Double()) As Double()
        If standardChannelValues Is Nothing OrElse standardChannelValues.Length <> ChannelCount Then
            Throw New ArgumentException("校准值必须正好包含256个托盘标准通道。", NameOf(standardChannelValues))
        End If

        Dim deviceChannelValues(ChannelCount - 1) As Double
        For standardChannel As Integer = 1 To ChannelCount
            Dim deviceChannel As Integer = TrayChannelMapping.ToDeviceChannel(standardChannel)
            deviceChannelValues(deviceChannel - 1) = standardChannelValues(standardChannel - 1)
        Next
        Return deviceChannelValues
    End Function

    Private Function TryReadCache(ByRef values As Double(), ByRef errorMessage As String) As Boolean
        If File.Exists(CacheInvalidationPath) Then
            errorMessage = "本地校准缓存已标记为过期，数据库不可用时不能作为校准来源。"
            Return False
        End If
        If Not File.Exists(filePath) Then
            errorMessage = "本地校准缓存文件不存在。"
            Return False
        End If

        Try
            Dim lines As String() = File.ReadAllLines(filePath)
            If lines.Length <> ChannelCount Then
                errorMessage = String.Format(CultureInfo.CurrentCulture,
                                             "本地校准缓存应有256行，实际为{0}行。",
                                             lines.Length)
                Return False
            End If

            Dim parsed(ChannelCount - 1) As Double
            For i As Integer = 0 To ChannelCount - 1
                Dim value As Double
                If Not Double.TryParse(lines(i).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, value) AndAlso
                   Not Double.TryParse(lines(i).Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, value) Then
                    errorMessage = String.Format(CultureInfo.CurrentCulture, "本地校准缓存第{0}行不是有效数值。", i + 1)
                    Return False
                End If
                ValidateValue(value)
                parsed(i) = value
            Next

            values = parsed
            errorMessage = String.Empty
            Return True
        Catch ex As Exception
            errorMessage = "读取本地校准缓存失败：" & ex.Message
            Return False
        End Try
    End Function

    Private Function LoadActiveCalibrationValueMap() As Dictionary(Of Integer, Double)
        Dim values As New Dictionary(Of Integer, Double)()
        Dim dataService As New OcvDataService()
        Dim table As DataTable = dataService.GetActiveCalibrationValues()
        For Each row As DataRow In table.Rows
            If IsDBNull(row("ChannelNo")) OrElse IsDBNull(row("CalibrationValueMilliohm")) Then
                Continue For
            End If

            Dim channelNo As Integer = Convert.ToInt32(row("ChannelNo"))
            If channelNo < 1 OrElse channelNo > ChannelCount Then Continue For

            Dim value As Double = Convert.ToDouble(row("CalibrationValueMilliohm"))
            ValidateValue(value)
            values(channelNo) = value
        Next
        Return values
    End Function

    Private Function BuildSourceDescription(databaseAvailable As Boolean,
                                            databaseChannels As Integer,
                                            cacheChannels As Integer,
                                            uncalibratedChannels As Integer) As String
        If Not databaseAvailable Then
            Return "本地 CSV 缓存（数据库不可用）"
        End If
        If databaseChannels = ChannelCount Then Return "OCV_DATA 数据库"
        If databaseChannels > 0 AndAlso cacheChannels > 0 Then
            Return String.Format(CultureInfo.CurrentCulture,
                                 "混合来源：数据库{0}通道，本地缓存{1}通道，未校准{2}通道",
                                 databaseChannels,
                                 cacheChannels,
                                 uncalibratedChannels)
        End If
        If cacheChannels > 0 Then
            Return String.Format(CultureInfo.CurrentCulture,
                                 "本地 CSV 缓存（数据库没有有效校准记录，未校准{0}通道）",
                                 uncalibratedChannels)
        End If
        Return String.Format(CultureInfo.CurrentCulture,
                             "OCV_DATA 可用，但没有已校准通道（未校准{0}通道）",
                             uncalibratedChannels)
    End Function

    Private Sub WriteCacheAtomically(values As Double(), createBackup As Boolean)
        ValidateValues(values)
        Dim directory As String = IO.Path.GetDirectoryName(filePath)
        If Not String.IsNullOrEmpty(directory) AndAlso Not IO.Directory.Exists(directory) Then
            IO.Directory.CreateDirectory(directory)
        End If

        Dim temporaryPath As String = filePath & "." & Guid.NewGuid().ToString("N") & ".tmp"
        Dim backupPath As String = Nothing
        Try
            Using writer As New StreamWriter(temporaryPath, False, New System.Text.UTF8Encoding(False))
                For i As Integer = 0 To ChannelCount - 1
                    writer.WriteLine(values(i).ToString("R", CultureInfo.InvariantCulture))
                Next
            End Using

            If File.Exists(filePath) Then
                If createBackup Then
                    backupPath = filePath & "." & DateTime.Now.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture) & ".bak"
                End If
                File.Replace(temporaryPath, filePath, backupPath)
            Else
                File.Move(temporaryPath, filePath)
            End If
            If File.Exists(CacheInvalidationPath) Then File.Delete(CacheInvalidationPath)
        Finally
            If File.Exists(temporaryPath) Then File.Delete(temporaryPath)
        End Try
    End Sub

    Private Sub InvalidateCache(reason As String)
        Try
            File.WriteAllText(CacheInvalidationPath,
                              "Cache refresh failed: " & If(reason, String.Empty),
                              New System.Text.UTF8Encoding(False))
        Catch ex As Exception
            Trace.WriteLine("Unable to mark calibration cache invalid: " & ex.Message)
            Try
                If File.Exists(filePath) Then File.Delete(filePath)
            Catch deleteException As Exception
                Trace.WriteLine("Unable to remove stale calibration cache: " & deleteException.Message)
            End Try
        End Try
    End Sub

    Private Shared Sub PublishSnapshot(snapshot As CalibrationSnapshot)
        If snapshot Is Nothing Then Throw New ArgumentNullException(NameOf(snapshot))
        SyncLock snapshotLock
            currentSnapshotValue = snapshot
            For i As Integer = 0 To ChannelCount - 1
                calibrateValueArray(i) = snapshot.GetCompensationByIndex0(i)
            Next
        End SyncLock
    End Sub

    Private Sub ValidateValues(values As Double())
        If values Is Nothing OrElse values.Length <> ChannelCount Then
            Throw New ArgumentException("校准值必须正好包含256个通道。", NameOf(values))
        End If

        For i As Integer = 0 To values.Length - 1
            ValidateValue(values(i))
        Next
    End Sub

    Private Sub ValidateValue(value As Double)
        If Double.IsNaN(value) OrElse Double.IsInfinity(value) OrElse Math.Abs(value) > MaxAbsCalibrationValue Then
            Throw New ArgumentOutOfRangeException(NameOf(value), "校准补偿值超出允许范围。")
        End If
    End Sub

End Class
