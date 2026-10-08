Imports System.Globalization
Imports System.IO
Imports System.Text

Public NotInheritable Class TestStandardProfile
    Public Property ResistanceMin As Decimal
    Public Property ResistanceMax As Decimal
    Public Property VoltageMin As Decimal
    Public Property VoltageMax As Decimal

    Public Sub Validate()
        If ResistanceMin < Decimal.Zero OrElse ResistanceMax < Decimal.Zero OrElse
           VoltageMin < Decimal.Zero OrElse VoltageMax < Decimal.Zero Then
            Throw New ArgumentOutOfRangeException(NameOf(TestStandardProfile), "测试标准的数值不能小于零。")
        End If
        If ResistanceMin > ResistanceMax Then
            Throw New ArgumentException("内阻最小值不能大于最大值。", NameOf(ResistanceMin))
        End If
        If VoltageMin > VoltageMax Then
            Throw New ArgumentException("电压最小值不能大于最大值。", NameOf(VoltageMin))
        End If
    End Sub
End Class

Public NotInheritable Class TestStandardFileStore
    Private Const FormatName As String = "XL-OCV-TestStandard"
    Private Const CurrentVersion As String = "1"
    Private Shared ReadOnly Utf8WithoutBom As New UTF8Encoding(False, True)
    Private Shared ReadOnly Utf8WithBomDetection As New UTF8Encoding(True, True)

    Public Sub New(applicationRoot As String)
        If String.IsNullOrWhiteSpace(applicationRoot) Then
            Throw New ArgumentException("应用目录不能为空。", NameOf(applicationRoot))
        End If
        DirectoryPath = Path.Combine(Path.GetFullPath(applicationRoot), "SETS")
    End Sub

    Public ReadOnly Property DirectoryPath As String

    Public Function Save(profile As TestStandardProfile,
                         standardName As String,
                         overwrite As Boolean) As String
        If profile Is Nothing Then Throw New ArgumentNullException(NameOf(profile))
        profile.Validate()

        Dim safeName As String = NormalizeStandardName(standardName)
        Dim targetPath As String = Path.Combine(DirectoryPath, safeName & ".set")
        Directory.CreateDirectory(DirectoryPath)

        If File.Exists(targetPath) AndAlso Not overwrite Then
            Throw New IOException("同名测试标准已存在。")
        End If

        Dim tempPath As String = targetPath & "." & Guid.NewGuid().ToString("N") & ".tmp"
        Try
            File.WriteAllText(tempPath, Serialize(profile), Utf8WithoutBom)
            If File.Exists(targetPath) Then
                File.Replace(tempPath, targetPath, Nothing)
            Else
                File.Move(tempPath, targetPath)
            End If
            Return targetPath
        Finally
            If File.Exists(tempPath) Then File.Delete(tempPath)
        End Try
    End Function

    Public Function Read(filePath As String) As TestStandardProfile
        If String.IsNullOrWhiteSpace(filePath) Then
            Throw New ArgumentException("请选择测试标准文件。", NameOf(filePath))
        End If
        If Not String.Equals(Path.GetExtension(filePath), ".set", StringComparison.OrdinalIgnoreCase) Then
            Throw New FormatException("测试标准文件扩展名必须为 .set。")
        End If

        Dim content As String = Utf8WithBomDetection.GetString(File.ReadAllBytes(filePath))
        If content.Length > 0 AndAlso content(0) = ChrW(&HFEFF) Then
            content = content.Substring(1)
        End If

        Dim values As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        For Each line As String In content.Split({vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries)
            Dim trimmedLine As String = line.Trim()
            If trimmedLine.Length = 0 OrElse trimmedLine.StartsWith("#", StringComparison.Ordinal) OrElse
               trimmedLine.StartsWith(";", StringComparison.Ordinal) Then
                Continue For
            End If

            Dim separator As Integer = trimmedLine.IndexOf("="c)
            If separator <= 0 OrElse separator <> trimmedLine.LastIndexOf("="c) Then
                Throw New FormatException("测试标准文件包含无效的键值行。")
            End If

            Dim key As String = trimmedLine.Substring(0, separator).Trim()
            Dim value As String = trimmedLine.Substring(separator + 1).Trim()
            If values.ContainsKey(key) Then Throw New FormatException("测试标准文件包含重复字段：" & key)
            values.Add(key, value)
        Next

        Dim format As String = RequiredValue(values, "Format")
        Dim version As String = RequiredValue(values, "Version")
        If Not String.Equals(format, FormatName, StringComparison.Ordinal) Then
            Throw New FormatException("文件不是 XL-OCV 测试标准。")
        End If
        If Not String.Equals(version, CurrentVersion, StringComparison.Ordinal) Then
            Throw New FormatException("不支持的测试标准文件版本：" & version)
        End If

        Dim allowedKeys As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            "Format", "Version", "ResistanceMinMilliOhm", "ResistanceMaxMilliOhm",
            "VoltageMinMilliVolt", "VoltageMaxMilliVolt"
        }
        For Each key As String In values.Keys
            If Not allowedKeys.Contains(key) Then Throw New FormatException("测试标准文件包含不支持的字段：" & key)
        Next

        Dim profile As New TestStandardProfile With {
            .ResistanceMin = ParseDecimal(RequiredValue(values, "ResistanceMinMilliOhm"), "ResistanceMinMilliOhm"),
            .ResistanceMax = ParseDecimal(RequiredValue(values, "ResistanceMaxMilliOhm"), "ResistanceMaxMilliOhm"),
            .VoltageMin = ParseDecimal(RequiredValue(values, "VoltageMinMilliVolt"), "VoltageMinMilliVolt"),
            .VoltageMax = ParseDecimal(RequiredValue(values, "VoltageMaxMilliVolt"), "VoltageMaxMilliVolt")
        }
        profile.Validate()
        Return profile
    End Function

    Private Shared Function NormalizeStandardName(standardName As String) As String
        If String.IsNullOrWhiteSpace(standardName) Then Throw New ArgumentException("测试标准名称不能为空。", NameOf(standardName))

        Dim safeName As String = standardName.Trim()
        If String.Equals(Path.GetExtension(safeName), ".set", StringComparison.OrdinalIgnoreCase) Then
            safeName = Path.GetFileNameWithoutExtension(safeName)
        End If
        If safeName.Length = 0 OrElse safeName = "." OrElse safeName = ".." OrElse
           Not String.Equals(Path.GetFileName(safeName), safeName, StringComparison.Ordinal) OrElse
           safeName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 Then
            Throw New ArgumentException("测试标准名称不能包含路径或文件名非法字符。", NameOf(standardName))
        End If
        Return safeName
    End Function

    Private Shared Function Serialize(profile As TestStandardProfile) As String
        Return String.Join(Environment.NewLine, {
            "Format=" & FormatName,
            "Version=" & CurrentVersion,
            "ResistanceMinMilliOhm=" & profile.ResistanceMin.ToString("G29", CultureInfo.InvariantCulture),
            "ResistanceMaxMilliOhm=" & profile.ResistanceMax.ToString("G29", CultureInfo.InvariantCulture),
            "VoltageMinMilliVolt=" & profile.VoltageMin.ToString("G29", CultureInfo.InvariantCulture),
            "VoltageMaxMilliVolt=" & profile.VoltageMax.ToString("G29", CultureInfo.InvariantCulture),
            String.Empty
        })
    End Function

    Private Shared Function RequiredValue(values As IDictionary(Of String, String), key As String) As String
        Dim value As String = Nothing
        If Not values.TryGetValue(key, value) Then Throw New FormatException("测试标准文件缺少字段：" & key)
        Return value
    End Function

    Private Shared Function ParseDecimal(value As String, key As String) As Decimal
        Dim parsed As Decimal
        Const styles As NumberStyles = NumberStyles.AllowLeadingWhite Or NumberStyles.AllowTrailingWhite Or
                                       NumberStyles.AllowLeadingSign Or NumberStyles.AllowDecimalPoint
        If Not Decimal.TryParse(value, styles, CultureInfo.InvariantCulture, parsed) Then
            Throw New FormatException("测试标准字段不是有效数字：" & key)
        End If
        Return parsed
    End Function
End Class
