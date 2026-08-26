Imports System.Globalization
Imports System.Data
Imports System.Data.SqlClient
Imports System.IO

Public Class CalibrationService
    Public Const ChannelCount As Integer = 256
    Public Const DefaultFileName As String = "CalibrateArr.csv"
    Public Const MaxAbsCalibrationValue As Double = 1000.0

    Private ReadOnly filePath As String

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

    Public ReadOnly Property CalibrationFilePath As String
        Get
            Return filePath
        End Get
    End Property

    Public Function LoadValues() As Double()
        Dim values(ChannelCount - 1) As Double
        If Not File.Exists(filePath) Then
            SaveValues(values, False)
        Else
            Dim lines As String() = File.ReadAllLines(filePath)
            For i As Integer = 0 To Math.Min(lines.Length, ChannelCount) - 1
                Dim value As Double
                If Double.TryParse(lines(i).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, value) OrElse
                   Double.TryParse(lines(i).Trim(), value) Then
                    values(i) = value
                Else
                    values(i) = 0
                End If
            Next
        End If

        OverlayDatabaseValues(values)

        Return values
    End Function

    Public Sub LoadIntoGlobalArray()
        Dim values As Double() = LoadValues()
        EnsureGlobalArray()
        For i As Integer = 0 To ChannelCount - 1
            calibrateValueArray(i) = values(i)
        Next
    End Sub

    Public Sub SaveFromGlobalArray(Optional createBackup As Boolean = True)
        EnsureGlobalArray()
        Dim values(ChannelCount - 1) As Double
        For i As Integer = 0 To ChannelCount - 1
            values(i) = calibrateValueArray(i)
        Next
        SaveValues(values, createBackup)
        SaveValuesToDatabase(values)
    End Sub

    Public Sub SaveValues(values As Double(), Optional createBackup As Boolean = True)
        ValidateValues(values)
        Dim directory As String = IO.Path.GetDirectoryName(filePath)
        If Not String.IsNullOrEmpty(directory) AndAlso Not IO.Directory.Exists(directory) Then
            IO.Directory.CreateDirectory(directory)
        End If

        If createBackup AndAlso File.Exists(filePath) Then
            Dim backupPath As String = filePath & "." & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".bak"
            File.Copy(filePath, backupPath, True)
        End If

        Using writer As New StreamWriter(filePath, False)
            For i As Integer = 0 To ChannelCount - 1
                writer.WriteLine(values(i).ToString(CultureInfo.InvariantCulture))
            Next
        End Using
    End Sub

    Public Function CalculateCompensation(standardResistance As Double, measuredResistance As Double) As Double
        Return standardResistance - measuredResistance
    End Function

    Public Sub SetGlobalValue(channelIndex0 As Integer, value As Double)
        ValidateChannelIndex(channelIndex0)
        ValidateValue(value)
        EnsureGlobalArray()
        calibrateValueArray(channelIndex0) = value
    End Sub

    Public Function GetGlobalValue(channelIndex0 As Integer) As Double
        ValidateChannelIndex(channelIndex0)
        EnsureGlobalArray()
        Return calibrateValueArray(channelIndex0)
    End Function

    Private Sub EnsureGlobalArray()
        If calibrateValueArray Is Nothing OrElse calibrateValueArray.Length < ChannelCount Then
            ReDim calibrateValueArray(ChannelCount - 1)
        End If
    End Sub

    Private Sub OverlayDatabaseValues(values As Double())
        Try
            Dim dataService As New OcvDataService()
            Dim table As DataTable = dataService.GetActiveCalibrationValues()
            For Each row As DataRow In table.Rows
                If IsDBNull(row("ChannelNo")) OrElse IsDBNull(row("CalibrationValueMilliohm")) Then
                    Continue For
                End If

                Dim channelNo As Integer = Convert.ToInt32(row("ChannelNo"))
                If channelNo < 1 OrElse channelNo > ChannelCount Then
                    Continue For
                End If

                values(channelNo - 1) = Convert.ToDouble(row("CalibrationValueMilliohm"))
            Next
        Catch ex As SqlException
            ' Keep the local CSV fallback available when OCV_DATA is not initialized yet.
        Catch ex As InvalidOperationException
        End Try
    End Sub

    Private Sub SaveValuesToDatabase(values As Double())
        Try
            Dim dataService As New OcvDataService()
            Dim activeValues As Dictionary(Of Integer, Double) = LoadActiveCalibrationValueMap(dataService)
            For i As Integer = 0 To ChannelCount - 1
                Dim channelNo As Integer = i + 1
                If activeValues.ContainsKey(channelNo) AndAlso Math.Abs(activeValues(channelNo) - values(i)) < 0.000001 Then
                    Continue For
                End If

                dataService.SaveChannelCalibration(i + 1,
                                                   Convert.ToDecimal(values(i)),
                                                   Nothing,
                                                   Nothing,
                                                   Initsetting.curOperator,
                                                   "Saved from upper-computer calibration page.")
            Next
        Catch ex As SqlException
            Throw New InvalidOperationException("保存校准值到 OCV_DATA 失败：" & ex.Message, ex)
        End Try
    End Sub

    Private Function LoadActiveCalibrationValueMap(dataService As OcvDataService) As Dictionary(Of Integer, Double)
        Dim values As New Dictionary(Of Integer, Double)()
        Dim table As DataTable = dataService.GetActiveCalibrationValues()
        For Each row As DataRow In table.Rows
            If IsDBNull(row("ChannelNo")) OrElse IsDBNull(row("CalibrationValueMilliohm")) Then
                Continue For
            End If

            Dim channelNo As Integer = Convert.ToInt32(row("ChannelNo"))
            If Not values.ContainsKey(channelNo) Then
                values.Add(channelNo, Convert.ToDouble(row("CalibrationValueMilliohm")))
            End If
        Next
        Return values
    End Function

    Private Sub ValidateValues(values As Double())
        If values Is Nothing OrElse values.Length <> ChannelCount Then
            Throw New ArgumentException("校准值必须正好包含256个通道。")
        End If

        For i As Integer = 0 To values.Length - 1
            ValidateValue(values(i))
        Next
    End Sub

    Private Sub ValidateValue(value As Double)
        If Double.IsNaN(value) OrElse Double.IsInfinity(value) OrElse Math.Abs(value) > MaxAbsCalibrationValue Then
            Throw New ArgumentOutOfRangeException("value", "校准补偿值超出允许范围。")
        End If
    End Sub

    Private Sub ValidateChannelIndex(channelIndex0 As Integer)
        If channelIndex0 < 0 OrElse channelIndex0 >= ChannelCount Then
            Throw New ArgumentOutOfRangeException("channelIndex0", "通道索引必须在0到255之间。")
        End If
    End Sub
End Class
