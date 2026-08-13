Imports System.Data
Imports System.Data.SqlClient
Imports System.IO.MemoryMappedFiles
Imports System.Text
Imports System.Threading

Public Class frmInput
    Dim obj_bat As Bat_struct
    Dim intNumber As Short
    Dim PainFlag As Boolean
    Private ObjOldWidth As Long  '保存窗体的原始宽度
    Private ObjOldHeight As Long '保存窗体的原始高度
    Private ObjOldFont As Single '保存窗体的原始字体比
    Public Rect_Bat_txt(MAX_CHANNEL - 1) As Rectangle
    Public Rect_Bat_lbl(MAX_CHANNEL - 1) As Rectangle
    Dim ReadBarCode As MemoryMappedFile
    Private lastLoadedTrayCode As String = ""
    Private isLoadingTrayCodes As Boolean = False
    Private suppressTrayTextChanged As Boolean = False


    Public Sub ResizeInit(ByVal Form1 As Form)
        Dim Obj As Control
        ObjOldWidth = Form1.Width
        ObjOldHeight = Form1.Height
        ObjOldFont = Form1.Font.Size / ObjOldHeight
        On Error Resume Next
        For Each Obj In Form1.Controls
            If Obj.Tag = "" Then
                Obj.Tag = "000"
            End If
            Obj.Tag = Obj.Tag & Obj.Left & " " & Obj.Top & " " & Obj.Width & " " & Obj.Height & " "
        Next Obj
        On Error GoTo 0
    End Sub

    '按比例改变表单内各元件的大小，
    '在调用ReSizeForm前先调用ReSizeInit函数
    Public Sub ResizeForm(ByVal Form1 As Form)
        Dim Pos(4) As Double
        Dim i As Long, TempPos As Long, StartPos As Long
        Dim Obj As Control
        Dim ScaleX As Double, ScaleY As Double

        ScaleX = Form1.Width / ObjOldWidth
        '保存窗体宽度缩放比例
        ScaleY = Form1.Height / ObjOldHeight
        '保存窗体高度缩放比例
        On Error Resume Next

        For Each Obj In Me.Controls
            StartPos = 4
            For i = 0 To 4
                '读取控件的原始位置与大小
                TempPos = InStr(StartPos, Obj.Tag, " ", vbTextCompare)
                If TempPos > 0 Then
                    Pos(i) = Mid(Obj.Tag, StartPos, TempPos - StartPos)
                    StartPos = TempPos + 1
                Else
                    Pos(i) = 0
                End If
                '根据控件的原始位置及窗体改变大
                '小的比例对控件重新定位与改变大小
                SetObjPosition(Obj, Pos(0) * ScaleX, Pos(1) * ScaleY, Pos(2) * ScaleX, Pos(3) * ScaleY)
                Obj.Font = New Font("宋体", ObjOldFont * Me.Height)
            Next i
        Next Obj

        On Error GoTo 0
    End Sub
    Private Sub SetObjPosition(ByVal Obj As Control, ByVal leftX As Integer, ByVal topY As Integer, ByVal widthX As Integer, ByVal heightY As Integer)
        Obj.Left = leftX
        Obj.Top = topY
        Obj.Width = widthX
        Obj.Height = heightY
    End Sub
    Private Sub frmInput_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        'If PainFlag = False Then Exit Sub
        'Resize
        Call ResizeForm(Me)
        txtbarNum.Visible = False
        lblBatCode.Visible = False
        'Panel1.Width = Me.Width
        'PictureBox1.Top = Panel1.Top + Panel1.Size.Height
        PictureBox1.Left = 0.01 * Me.Width
        PictureBox1.Left = GroupBox1.Left

        PictureBox1.Width = 0.96 * Me.Width
        PictureBox1.Width = GroupBox1.Width
        PictureBox1.Height = (Me.ClientSize.Height - PictureBox1.Top) * 0.96
        PainFlag = True
        FreshBat(intPaintFlag)
    End Sub
    Dim intPaintFlag As Short
    Private Sub FreshBat(ByVal intFlag As Short)
        Dim bmp As Bitmap
        Dim obj_bat As Bat_struct
        Dim I, J As Int16
        Dim P1 As Point
        Dim P2 As Point
        Dim intTemp As Int16
        intPaintFlag = intFlag
        Try
            bmp = New Bitmap(PictureBox1.Width, PictureBox1.Height)
        Catch ex As Exception
            Exit Sub
        End Try
        PictureBox1.Image = bmp
        Dim G As Graphics
        G = Graphics.FromImage(bmp)

        G.Clear(Color.LightBlue)
        obj_bat.Left = PictureBox1.Left
        obj_bat.Top = PictureBox1.Top
        obj_bat.Hight = PictureBox1.Size.Height / intRow
        obj_bat.width = PictureBox1.Size.Width / intCol
        'debug B
        'G.DrawString("1000", Me.Font, Brushes.Black, New Point(obj_bat.Left * 0.5, Pic_bat.Top * 0.5))
        For I = 0 To intCol - 1
            For J = 0 To intRow - 1
                intTemp = I * intRow + J
                '电池的头部
                P1 = New Point(obj_bat.width * I + obj_bat.width / 4, obj_bat.Hight * J + 4)
                P2 = New Point(obj_bat.width * I + obj_bat.width / 4, obj_bat.Hight * J + obj_bat.Hight / 9)
                '电池头部矩形
                Rect_Bat_lbl(intTemp) = New Rectangle(P1.X, P1.Y, obj_bat.width / 3.2, P2.Y - P1.Y)
                G.DrawRectangle(New Pen(Brushes.Black), Rect_Bat_lbl(intTemp))
                G.FillRectangle(New SolidBrush(Color.White), Rect_Bat_lbl(intTemp))
                '电池-------------------------------------------------------------------------------
                P1 = New Point(obj_bat.width * I, obj_bat.Hight * J + obj_bat.Hight / 9)
                P2 = New Point(obj_bat.width * I, obj_bat.Hight * J + obj_bat.Hight)
                '电池矩形
                Rect_Bat_txt(intTemp) = New Rectangle(P1.X, P1.Y, obj_bat.width * 0.9, P2.Y - P1.Y)
                G.DrawRectangle(New Pen(Brushes.Black), Rect_Bat_txt(intTemp))
                G.FillRectangle(New SolidBrush(Color.White), Rect_Bat_txt(intTemp))
                '----------------------------------------------------------------------------------------
                If intFlag = 2 Then
                    Dim temp_string As String
                    Dim obj_Font As Font
                    obj_Font = Me.Font
                    temp_string = RecieveData(intTemp).BatCode
                    G.DrawString(temp_string, obj_Font, Brushes.Black, Rect_Bat_txt(intTemp))
                End If
            Next J
        Next I
        '刷新了 PictureBox1 控件
        PictureBox1.Invalidate()

        '    For I = 0 To intRow - 1
        '        For J = 0 To intCol - 1
        '            P1 = New Point(obj_bat.width * J, obj_bat.Hight * I + obj_bat.Hight / 9)
        '            Box = New Rectangle(P1, New Size(obj_bat.width, obj_bat.Hight))
        '            obj_pen = New Pen(Brushes.Black, 1)
        '            If intFlag = 2 Then
        '                I = intNumber \ intCol '行
        '                J = intNumber Mod intCol '列
        '                Dim temp_string As String
        '                Dim P3 As Point
        '                Dim obj_Font As Font
        '                obj_Font = Me.Font
        '                temp_string = CellCode(intNumber)

        '                P3.X = obj_bat.Hight * I
        '                P3.Y = obj_bat.width * J
        '                G.DrawString(temp_string, obj_Font, Brushes.Black, Box)
        '            End If
        '            If blnDataBase = False Then
        '                G.DrawRectangle(obj_pen, Box)
        '            Else
        '                G.DrawRectangle(obj_pen, Box)
        '            End If
        '        Next
        '    Next
        '    Exit Sub

    End Sub
    Private Sub PictureBox1_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PictureBox1.MouseDown
        Dim intX, intY As Integer
        Dim I, J As Short

        txtbarNum.Visible = True
        lblBatCode.Visible = True
        '*********************************
        '鼠标点击的位置
        intX = e.Location.X
        intY = e.Location.Y
        '*********************************
        '单个电池的大小范围
        With obj_bat
            .width = CType(sender, PictureBox).ClientRectangle.Width / intCol
            .Hight = CType(sender, PictureBox).ClientRectangle.Height / intRow
        End With
        '定位lblBatCode和txtbarNum
        For J = 0 To intCol - 1
            For I = 0 To intRow - 1
                '单个电池的位置
                obj_bat.Top = obj_bat.Hight * I
                obj_bat.Left = obj_bat.width * J
                '定位lblBatCode
                If (intX >= obj_bat.width * J And intX <= obj_bat.width * (J + 1)) _
                    And (intY >= obj_bat.Hight * I And intY <= obj_bat.Hight * (I + 1)) Then
                    With lblBatCode
                        .Width = obj_bat.width
                        .Top = PictureBox1.Top + obj_bat.Hight * I
                        .Left = PictureBox1.Left + obj_bat.width * J
                    End With
                    If lblBatCode.Left + lblBatCode.Width > PictureBox1.Left + PictureBox1.Width Then
                        lblBatCode.Left = PictureBox1.Left + PictureBox1.Width - lblBatCode.Width
                    End If
                    lblBatCode.Text = CStr(J + 1) & "— " & CStr(I + 1)
                    '定位txtbarNum
                    txtbarNum.Top = lblBatCode.Top + lblBatCode.Height
                    txtbarNum.Left = lblBatCode.Left
                    txtbarNum.Height = obj_bat.Hight - lblBatCode.Height
                    txtbarNum.Focus()
                    txtbarNum.Text = RecieveData(8 * J + I).BatCode
                    txtbarNum.SelectionStart = 0
                    txtbarNum.SelectionLength = Len(RecieveData(8 * J + I).BatCode)
                    '第n个电池
                    lblBatCode.Tag = I * J + J
                    Exit Sub
                End If
            Next
        Next

    End Sub
    Private Sub txtbarNum_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtbarNum.KeyDown
        Dim I, J As Integer
        Dim intNumber As Integer
        Dim temp_string As String

        intNumber = Val(Trim(lblBatCode.Tag))
        If e.KeyCode = 13 Then
            I = intNumber \ intRow  '列
            J = intNumber Mod intRow  '行
            '定位lblBatCode的位置
            If I <= intCol Then
                lblBatCode.Left = PictureBox1.Left + obj_bat.width * (I)
                If lblBatCode.Left + lblBatCode.Width > PictureBox1.Left + PictureBox1.Width Then
                    lblBatCode.Left = PictureBox1.Left + PictureBox1.Width - lblBatCode.Width
                End If
            Else
                Exit Sub
            End If
            lblBatCode.Top = PictureBox1.Top + obj_bat.Hight * (J)
            lblBatCode.Text = CStr(I + 1) & "— " & CStr(J + 1)
            '定位txtbarNum的位置
            txtbarNum.Width = lblBatCode.Width
            txtbarNum.Left = lblBatCode.Left
            txtbarNum.Top = lblBatCode.Top + lblBatCode.Height
            txtbarNum.Height = obj_bat.Hight - lblBatCode.Height
            temp_string = Trim(txtbarNum.Text)
            txtbarNum.Text = RecieveData(16 * I + J).BatCode
            txtbarNum.SelectionStart = 0
            txtbarNum.SelectionLength = Len(RecieveData(16 * I + J).BatCode)
            '电池编码数组
            RecieveData(intNumber).BatCode = Trim(txtbarNum.Text)
            '*****************************************
            lblBatCode.Tag = Val(Trim(lblBatCode.Tag)) + 1
            '打印电池编码
            'FreshBat(2)
            '显示首尾电池编码
            txtFirst.Text = RecieveData(0).BatCode
            txtLast.Text = RecieveData(intNumber).BatCode
        ElseIf e.KeyCode = 27 Then
            txtbarNum.Visible = False
            lblBatCode.Visible = False
            Exit Sub
        End If
    End Sub
    Private Sub LaunchCWtray()
        Try
            ' 指定要启动的程序路径
            Dim exePath As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) & "\TrayScan\CWtray.exe" ' 请将路径替换为 CWtray.exe 的实际路径

            ' 创建一个新的进程实例并启动 CWtray.exe
            Dim procInfo As New ProcessStartInfo(exePath)
            Process.Start(procInfo)
        Catch ex As Exception
            ' 处理启动程序异常
            MessageBox.Show("启动程序时出现错误：" & ex.Message)
        End Try
    End Sub
    Private Sub frmInput_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        '**********************
        '把数组清空
        Array.Clear(testArray, 0, testArray.Length)
        Array.Clear(retestArray, 0, retestArray.Length)
        ' 将元素个数设为0
        Array.Resize(testArray, 0)
        Array.Resize(retestArray, 0)
        'init form 
        Call ResizeInit(Me)
        '清空电池编码
        For i = 0 To UBound(RecieveData)
            RecieveData(i).BatCode = ""
        Next
        '**********************
        txtbarNum.Visible = False
        lblBatCode.Visible = False
        '读取条码文件
        GetBarCodeFile()
        '刷新界面
        FreshBat(2)
        IsExitFlag = False
        ReadBarCode = MemoryMappedFile.CreateNew("MpScanGui_ReturnInfo", 100 * 512)
        '-----------------------------------------------------------------------
        '启动保存电池条码进程，读取扫码软件的共享内存 weng 20231224
        saveCodeTask = Tasks.Task.Factory.StartNew(Sub()
                                        ThreadSaveCode(0)

                                    End Sub)
        '--------------------------------------------------------------------------
        '启动扫码程序
        LaunchCWtray()
    End Sub

    Function MapReadBarCode(ByVal IndexKey As Integer) As String
        ' 创建一个内存映射文件的名称  
        Dim mapName As String = "MpScanGui_ReturnInfo"
        ' 用于存储从内存映射文件读取的文本  
        Dim contentRead As String = String.Empty
        Try
            ' 打开已存在的内存映射文件以进行读取  
            ' 创建一个访问器来读取文件  
            Using accessor As MemoryMappedViewAccessor = ReadBarCode.CreateViewAccessor()
                ' 创建一个缓冲区来存储读取的字节  
                Dim buffer As Byte() = New Byte(100 * 512) {}
                ' 从内存映射文件读取字节到缓冲区  
                accessor.ReadArray(0, buffer, 0, buffer.Length)
                ' 将字节转换为字符串  
                contentRead = Encoding.UTF8.GetString(buffer)
            End Using
        Catch ex As Exception
        Finally
            ' 删除内存映射文件（可选）  

        End Try
        Return contentRead
    End Function

    ' 声明一个委托，用于在 UI 线程上更新控件
    Private Delegate Sub UpdateTextBoxDelegate(text As String)

    ' 更新 txtTray 控件的方法
    Private Sub UpdateTxtTray(text As String)
        If txtTray.InvokeRequired Then
            Dim d As New UpdateTextBoxDelegate(AddressOf UpdateTxtTray)
            Me.Invoke(d, New Object() {text})
        Else
            txtTray.Text = text
        End If
    End Sub
    Private Sub UpdatetxtFirst(text As String)
        If txtTray.InvokeRequired Then
            Dim d As New UpdateTextBoxDelegate(AddressOf UpdatetxtFirst)
            Me.Invoke(d, New Object() {text})
        Else
            txtFirst.Text = text
        End If
    End Sub
    Private Sub UpdateTxtLast(text As String)
        If txtTray.InvokeRequired Then
            Dim d As New UpdateTextBoxDelegate(AddressOf UpdateTxtLast)
            Me.Invoke(d, New Object() {text})
        Else
            txtLast.Text = text
        End If
    End Sub


    Private Sub ThreadSaveCode(GuiKey As Integer)

        Dim AppSave As EventWaitHandle = Nothing
        Try
            AppSave = New EventWaitHandle(False, EventResetMode.AutoReset, "MPScan_APP_Save")
        Catch ex As Exception
            Return
        End Try
        Do
            If IsExitFlag = True Then
                AppSave.Dispose()
                Exit Do
            End If
            Dim IsRec = AppSave.WaitOne(2000)
            If IsExitFlag = True Then
                AppSave.Dispose()
                Exit Do
            End If
            If IsRec Then
                Try
                    Dim TrayCode As String = ""

                    Dim Ret As String = MapReadBarCode(GuiKey)
                    '清除字符串中的空字符
                    Ret = Ret.Replace(vbNullChar.ToString(), "")

                    Dim Arr = Ret.Split("~")
                    If Arr.Length >= 3 Then
                        TrayCode = Arr(2).Trim()
                        UpdateTxtTray(TrayCode)
                    End If
                    If False AndAlso Arr.Length >= 4 Then
                        Dim Title As String = ""
                        Dim Count As Integer = 0
                        Dim BarCode() As String = {}
                        Title = Arr(0) '标题
                        Count = Arr(1) '条码个数
                        TrayCode = Arr(2) '托盘码
                        'BarCode卫最终收取的条码数组
                        BarCode = Arr(3).Split("$") '分割电池条码，放在数组barcode中
                        SaveToDB1(Title, Count, TrayCode, BarCode) '把相关信息保存起来
                        '统计有效条码个数
                        Dim count1 As Integer = BarCode.Count(Function(s) s.Length > 2)
                        ' 调用更新 txtTray 的方法
                        UpdateTxtTray(TrayCode)
                        UpdatetxtFirst("01")
                        UpdateTxtLast(count1.ToString())

                    End If
                Catch ex As Exception

                End Try
            Else

            End If
        Loop

    End Sub
    Public IsExitFlag As Boolean = False
    '保存 ThreadSaveCode 的任务引用,FormClosing 时等待其退出再释放资源 (R2-07)
    Private saveCodeTask As System.Threading.Tasks.Task = Nothing
    Private Sub SaveToDB1(title As String, count As Integer, trayCode As String, barCode() As String)
        '把电池条码保存到全局变量
        For i = 0 To barCode.Length - 1
            RecieveData(i).BatCode = barCode(i)
        Next
    End Sub

    Private Sub CmdFresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdFresh.Click
        Dim intNumber As Short
        Dim strTempF As String, strTempL As String
        Dim fixstr As String = ""
        Dim i As Integer
        txtbarNum.Visible = False
        lblBatCode.Visible = False
        If txtTray.Text = "" Or txtFirst.Text = "" Or txtLast.Text = "" Then
            MsgBox("请输入托盘号 、 首条形码 、 尾条形码！", vbOKOnly)
            Exit Sub
        End If
        If IsNumeric(txtFirst.Text.Last) = False Then
            MsgBox("首电池条形码录入有误！", vbOKOnly)
            Exit Sub
        End If
        '如果txtLast不为空，则根据首尾自动生成
        strTempF = txtFirst.Text
        strTempL = txtLast.Text
        '把需要测试的点位放在测试数组中

        Dim kk As Short = Short.Parse(strTempL) - Short.Parse(strTempF)
        ReDim testArray(kk)
        Dim index = 0
        For i = Short.Parse(strTempF) - 1 To Short.Parse(strTempL) - 1
            testArray(index) = i ' 添加元素到数组
            index += 1
        Next
        If strTempL <> "" Then
            intNumber = GetBarCodeNum(strTempL, fixstr) - GetBarCodeNum(strTempF, fixstr)
            If intNumber + 1 > MAX_CHANNEL Then
                MsgBox("最后一个电池的条形码已改变！")
            End If
            If intNumber < 1 Then
                MsgBox("电池的条形码录入有误！")
                Exit Sub
            End If
        End If
        If RecieveData(0).BatCode = "" Then
            MsgBox("电池条码没有完成扫码，请重新扫码输入。", vbOKOnly, "提示")
            '条形码更新到global变量
            If intNumber + 1 > MAX_CHANNEL Then
                For i = 0 To MAX_CHANNEL - 1
                    RecieveData(i).BatCode = fixstr & Format(Mid(strTempF, Len(fixstr) + 1, Len(strTempF) - Len(fixstr)) + i, "000")
                Next
            Else
                For i = 0 To intNumber
                    RecieveData(i).BatCode = fixstr & Format(Mid(strTempF, Len(fixstr) + 1, Len(strTempF) - Len(fixstr)) + i, "000")
                Next
            End If
        Else
            MsgBox("已经成功导入电池的条形码！", vbOKOnly, "提示")
        End If
        '更新界面
        FreshBat(2)
        If Not SaveCurrentTrayCellsToOcvData() Then
            Exit Sub
        End If
        '把条码保存到文件
        SaveBarcode()
        Me.Close()
    End Sub

    Private Sub txtTray_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtTray.KeyDown
        If e.KeyCode = 13 Then
            If Trim(txtTray.Text) = "" Then
                MsgBox("托盘编号不能为空", , "提示")
                Exit Sub
            End If
            LoadTrayBatteryCodes(Trim(txtTray.Text), True)
        End If
    End Sub
    Private Sub SearchFromDB()
        LoadTrayBatteryCodes(Trim(txtTray.Text), True)
        If Trim(txtFirst.Text) <> "" And Trim(txtLast.Text) <> "" Then
            CmdFresh_Click(Nothing, Nothing)
        End If
    End Sub

    Private Sub txtTray_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtTray.TextChanged
        If suppressTrayTextChanged OrElse isLoadingTrayCodes Then
            Return
        End If

        Dim trayCode As String = Trim(txtTray.Text)
        If trayCode = "" OrElse trayCode = lastLoadedTrayCode Then
            Return
        End If

        LoadTrayBatteryCodes(trayCode, False)
    End Sub

    Private Function LoadTrayBatteryCodes(trayCode As String, showMessageWhenEmpty As Boolean) As Boolean
        trayCode = Trim(trayCode)
        If trayCode = "" Then
            If showMessageWhenEmpty Then
                MsgBox("托盘号不能为空。", vbOKOnly, "提示")
            End If
            Return False
        End If

        If isLoadingTrayCodes Then
            Return False
        End If

        isLoadingTrayCodes = True
        Try
            Dim channelCodes As Dictionary(Of Integer, String) = QueryTrayCodesFromLocalDb(trayCode)
            If channelCodes Is Nothing OrElse channelCodes.Count = 0 Then
                If showMessageWhenEmpty Then
                    MsgBox("未查询到托盘明细，请人工录入或等待接口接入。", vbOKOnly, "提示")
                End If
                Return False
            End If

            For i As Integer = 0 To UBound(RecieveData)
                RecieveData(i).BatCode = ""
            Next

            Dim orderedChannels = channelCodes.Keys.OrderBy(Function(channel) channel).ToList()
            ReDim testArray(orderedChannels.Count - 1)
            For i As Integer = 0 To orderedChannels.Count - 1
                Dim channel As Integer = orderedChannels(i)
                RecieveData(channel - 1).BatCode = channelCodes(channel)
                testArray(i) = CShort(channel - 1)
            Next

            txtFirst.Text = "1"
            txtLast.Text = orderedChannels.Count.ToString()
            lastLoadedTrayCode = trayCode
            FreshBat(2)
            SaveBarcode()

            If showMessageWhenEmpty Then
                MsgBox("已加载托盘条码：" & orderedChannels.Count.ToString() & " 个通道。", vbOKOnly, "提示")
            End If
            Return True
        Catch ex As Exception
            If showMessageWhenEmpty Then
                MsgBox("加载托盘条码失败：" & ex.Message, vbOKOnly, "提示")
            End If
            Return False
        Finally
            isLoadingTrayCodes = False
        End Try
    End Function

    Private Function QueryTrayCodesFromLocalDb(trayCode As String) As Dictionary(Of Integer, String)
        Dim result As New Dictionary(Of Integer, String)

        Try
            Dim service As New OcvDataService()
            Dim trayCells As DataTable = service.GetTrayCellsByTrayCode(trayCode)

            For Each row As DataRow In trayCells.Rows
                If IsDBNull(row("ChannelNo")) OrElse IsDBNull(row("BatteryCode")) Then
                    Continue For
                End If

                Dim channel As Integer = Convert.ToInt32(row("ChannelNo"))
                Dim barcode As String = Convert.ToString(row("BatteryCode")).Trim()
                If barcode = "" Then
                    Continue For
                End If

                If channel < 1 OrElse channel > MAX_CHANNEL Then
                    Continue For
                End If

                If result.ContainsKey(channel) Then
                    Throw New InvalidOperationException("托盘明细存在重复通道：CH" & channel.ToString())
                End If

                result.Add(channel, barcode)
            Next
        Catch ex As SqlException
            Throw New InvalidOperationException("查询 OCV_DATA 托盘明细失败：" & ex.Message, ex)
        End Try

        Return result
    End Function

    Private Function MapTrayPositionToChannel(nRow As Integer, nCol As Integer) As Integer
        Return (nCol - 1) * intRow + nRow
    End Function

    Private Function SaveCurrentTrayCellsToOcvData() As Boolean
        Dim trayCode As String = Trim(txtTray.Text)
        If trayCode = "" Then
            MsgBox("托盘号不能为空。", vbOKOnly, "提示")
            Return False
        End If

        Dim cells As New List(Of OcvTrayCellInput)()
        For channel As Integer = 1 To MAX_CHANNEL
            Dim barcode As String = Trim(RecieveData(channel - 1).BatCode)
            If barcode <> "" Then
                cells.Add(New OcvTrayCellInput With {
                    .ChannelNo = channel,
                    .BatteryCode = barcode,
                    .SourceType = "UpperComputer",
                    .RowNo = CByte(((channel - 1) Mod intRow) + 1),
                    .ColNo = CByte(((channel - 1) \ intRow) + 1)
                })
            End If
        Next

        If cells.Count = 0 Then
            MsgBox("没有可保存的电芯条码。", vbOKOnly, "提示")
            Return False
        End If

        Try
            Dim service As New OcvDataService()
            service.SaveTrayCells(trayCode, cells, Initsetting.curOperator, Environment.MachineName)
            Return True
        Catch ex As Exception
            MsgBox("保存托盘明细到 OCV_DATA 失败：" & ex.Message, vbOKOnly, "提示")
            Return False
        End Try
    End Function

    Private Sub GroupBox1_Enter(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles GroupBox1.Enter

    End Sub

    Private Sub PictureBox1_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles PictureBox1.Paint

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bnt_close.Click
        'If frmTest.fixtureClose = False Then
        '    MsgBox("托盘没有闭合，请完成相应操作！", vbOKOnly, "提示")
        'End If
        Me.Close()
    End Sub

    Private Sub PictureBox1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox1.Click

    End Sub

    Private Sub Btn_scanCode_Click(sender As Object, e As EventArgs) Handles Btn_scanCode.Click
        'Dim msg = ScanBarCode()
        'If msg = "" Then
        '    txtTray.Text = "扫描失败！"
        'Else
        '    txtTray.Text = msg
        'End If
        '启动扫码程序
        LaunchCWtray()
    End Sub

    Private Sub txtLast_TextChanged(sender As Object, e As EventArgs) Handles txtLast.TextChanged
        If Val(txtLast.Text) > 256 Then
            MsgBox("请正确输入通道号！", vbOKOnly, "提示")
        End If
    End Sub

    Private Sub frmInput_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing

        ''托盘编码自动更新到联机界面
        'My.Forms.frmTest.txtTray.Text = My.Forms.frmInput.txtTray.Text
        ''显示首尾条形码
        'My.Forms.frmTest.txtFirstNum.Text = My.Forms.frmInput.txtFirst.Text
        'My.Forms.frmTest.txtLastNum.Text = My.Forms.frmInput.txtLast.Text
        IsExitFlag = True
        '等待保存条码的工作线程退出后再释放共享内存,避免线程访问已释放资源 (R2-07)
        Try
            If saveCodeTask IsNot Nothing Then
                saveCodeTask.Wait(3000)
            End If
        Catch
        End Try
        ReadBarCode.Dispose()
    End Sub
End Class
