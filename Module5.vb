Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Runtime.CompilerServices.RuntimeHelpers
Imports System.Text
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json.Serialization

Module Module5

    Public Class EQEquipmentInfo
        Public Property EquipmentNo As String
        Public Property EqState As String
        Public Property StateDateTime As String
        ' Default constructor
        Public Sub New()
            ' Initialize properties with default values
            EquipmentNo = ""
            EqState = ""
            StateDateTime = ""
        End Sub

    End Class
    ''' <summary>
    ''' 用这个模块的程序20230815 weng
    ''' </summary>
    Public Class BatteryInfo
        ''' <summary>
        ''' 电池条码
        ''' </summary>
        ''' <returns></returns>
        Public Property BatteryNo As String
        ''' <summary>
        ''' 设备编号
        ''' </summary>
        ''' <returns></returns>
        Public Property EquipmentNo As String
        ''' <summary>
        ''' 托盘条码
        ''' </summary>
        ''' <returns></returns>
        Public Property TrayNo As String
        ''' <summary>
        ''' 托盘格子
        ''' </summary>
        ''' <returns></returns>
        Public Property TrayGridNo As String
        ''' <summary>
        ''' 是否合格( 1：合格 ，0：不合格)
        ''' </summary>
        ''' <returns></returns>
        Public Property IsPass As String
        ''' <summary>
        ''' 不良编码
        ''' </summary>
        ''' <returns></returns>
        Public Property NgCode As String
        ''' <summary>
        ''' 产品状态 1:'OK' ,2:NG ,3:复测 1 ,4:复测 2
        ''' </summary>
        ''' <returns></returns>
        Public Property ProductType As String
        ''' <summary>
        ''' 测试电压 mv
        ''' </summary>
        ''' <returns></returns>
        Public Property Voltage As String
        ''' <summary>
        ''' 内阻 mΩ
        ''' </summary>
        ''' <returns></returns>
        Public Property InternalResistance As String
        ''' <summary>
        ''' K 值(电压/小时)
        ''' </summary>
        ''' <returns></returns>
        Public Property KValue As String
        ''' <summary>
        ''' 壳体电压(mv)
        ''' </summary>
        ''' <returns></returns>
        Public Property CaseVoltage As String
        ''' <summary>
        ''' OCV 序号（OCV1、OCV2、OCV3）
        ''' </summary>
        ''' <returns></returns>
        Public Property OCVNo As String
        ''' <summary>
        ''' 测试人员
        ''' </summary>
        ''' <returns></returns>
        Public Property TestName As String
        ''' <summary>
        ''' 测试次数
        ''' </summary>
        ''' <returns></returns>
        Public Property TestNum As String
        ''' <summary>
        ''' 测试时间
        ''' </summary>
        ''' <returns></returns>
        Public Property TestTime As String
        ''' <summary>
        ''' 测试通道
        ''' </summary>
        ''' <returns></returns>
        Public Property TestChanner As String
        ''' <summary>
        ''' 入库人
        ''' </summary>
        ''' <returns></returns>
        Public Property InStockName As String
        ''' <summary>
        ''' 入库时间
        ''' </summary>
        ''' <returns></returns>
        Public Property InStockTime As String
        ''' <summary>
        ''' 出库人
        ''' </summary>
        ''' <returns></returns>
        Public Property OutStockName As String
        ''' <summary>
        ''' 出库时间
        ''' </summary>
        ''' <returns></returns>
        Public Property OutStockTime As String
        Public Property Remark As String
        Public Property ACR As String
    End Class

    Public Class AddProductOcvList

        Public Property AddProductOcvList As List(Of BatteryInfo)
        Public Sub New()
            AddProductOcvList = New List(Of BatteryInfo)()
        End Sub

    End Class

    Public Class MesResponse
        Public Property Code As Integer
        Public Property Msg As String
        Public Property Data As Object
    End Class

    Public Class MesApi
        Private Shared ReadOnly MesUrl As String = "http://192.168.19.58:8128/api/IOT/ProductOcv/Save" ' Replace with your actual MES API URL
        Private Shared ReadOnly MesUrlEQ As String = "http://192.168.19.58:8128/api/IOT/EQEquipmentInfo/UpdateActualState" ' Replace with your actual MES API URL

        Public Shared Function HttpPost(jsonData As String) As MesResponse

            Dim client As New HttpClient()
            Try
                client.Timeout = TimeSpan.FromSeconds(60)

                Dim content = New StringContent(jsonData, Encoding.UTF8, "application/json")

                Dim response = client.PostAsync(MesUrl, content).Result

                If response.IsSuccessStatusCode Then

                    Dim resultString = response.Content.ReadAsStringAsync().Result
                    Dim result = JsonConvert.DeserializeObject(Of MesResponse)(resultString)
                    Return result

                Else
                    Return New MesResponse With {.Code = 500, .Msg = "Error"}
                End If
            Catch ex As Exception
                Return New MesResponse With {.Code = 500, .Msg = ex.Message}
            Finally
                client.Dispose()
            End Try

        End Function

        Public Shared Function HttpPostEQStatus(jsonData As String) As MesResponse

            Dim client As New HttpClient()
            Try
                client.Timeout = TimeSpan.FromSeconds(60)

                Dim content = New StringContent(jsonData, Encoding.UTF8, "application/json")

                Dim response = client.PostAsync(MesUrlEQ, content).Result

                If response.IsSuccessStatusCode Then

                    Dim resultString = response.Content.ReadAsStringAsync().Result
                    Dim result = JsonConvert.DeserializeObject(Of MesResponse)(resultString)
                    Return result

                Else
                    Return New MesResponse With {.Code = 500, .Msg = "Error"}
                End If
            Catch ex As Exception
                Return New MesResponse With {.Code = 500, .Msg = ex.Message}
            Finally
                client.Dispose()
            End Try

        End Function
        Shared ReadOnly settings = New JsonSerializerSettings With {
                    .ContractResolver = New CamelCasePropertyNamesContractResolver,
                    .DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    .Formatting = Newtonsoft.Json.Formatting.None
                }
        Public Shared Function UploadBatteryData(batteryList As AddProductOcvList) As MesResponse
            ' Convert batteryList to JSON
            '**************************************
            Dim UpNum As Integer = 0
            Dim Datapath As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
            Dim ret As New MesResponse With {.Code = -1, .Msg = "未上传", .Data = Nothing}

            'Dim TmpText = Cls_HttpReport.JsonWithoutQuote(Cls_HttpReport.JsonForJava(JsonConvert.SerializeObject(batteryList)))
            Dim TmpText = JsonConvert.SerializeObject(batteryList, settings)
            IO.File.AppendAllLines(Datapath & "\UpLoadMes.xml", {Date.Now.ToString, "Send:", TmpText})
            Try
                ret = HttpPost(TmpText)

                TmpText = JsonConvert.SerializeObject(ret)
                IO.File.AppendAllLines(Datapath & "\UpLoadMes.xml", {"Receive:", TmpText, ""})
                If ret.Code = 200 Then
                    UpNum = 1
                End If
            Catch ex As Exception
                IO.File.AppendAllLines(Datapath & "\UpLoadMes.xml", {"Error:", ex.ToString, ""})
                ret.Msg = ex.Message
            End Try
            Return ret
            '********************************
            'Dim jsonData As String = Newtonsoft.Json.JsonConvert.SerializeObject(New With {.AddProductOcvList = batteryList})
            'Try
            '    Dim request As WebRequest = WebRequest.Create(MesUrl)
            '    request.Method = "POST"
            '    request.ContentType = "application/json"

            '    ' Create a WebRequest to the MES API
            '    ' Write the JSON data to the request stream
            '    Using streamWriter As New StreamWriter(request.GetRequestStream())
            '        streamWriter.Write(jsonData)
            '    End Using

            '    ' Get the response from the MES API
            '    Dim response As WebResponse = request.GetResponse()

            '    ' Parse the response JSON
            '    Dim responseBody As String
            '    Using streamReader As New StreamReader(response.GetResponseStream())
            '        responseBody = streamReader.ReadToEnd()
            '    End Using

            '    Dim mesResponse As MesResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(Of MesResponse)(responseBody)

            '    ' Return the parsed response
            '    Return mesResponse
            'Catch ex As Exception

            'End Try
        End Function
        ''' <summary>
        ''' 上传设备状态信息
        ''' </summary>
        ''' <param name="EQStatusData"></param>
        ''' <returns></returns>
        Public Shared Function UploadEQStatusData(EQStatusData As EQEquipmentInfo) As MesResponse
            ' Convert batteryList to JSON
            '**************************************
            Dim UpNum As Integer = 0
            Dim Datapath As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
            Dim ret As New MesResponse With {.Code = -1, .Msg = "未上传", .Data = Nothing}

            ' Dim TmpText = Cls_HttpReport.JsonWithoutQuote(Cls_HttpReport.JsonForJava(JsonConvert.SerializeObject(batteryList)))
            Dim TmpText = JsonConvert.SerializeObject(EQStatusData, settings)

            IO.File.AppendAllLines(Datapath & "\UpLoadMes.xml", {Date.Now.ToString, "Send:", TmpText})
            Try
                ret = HttpPostEQStatus(TmpText)
                TmpText = JsonConvert.SerializeObject(ret)
                IO.File.AppendAllLines(Datapath & "\UpLoadMes.xml", {"Receive:", TmpText, ""})
                If ret.Code = 200 Then
                    UpNum = 1
                End If
            Catch ex As Exception
                IO.File.AppendAllLines(Datapath & "\UpLoadMes.xml", {"Error:", ex.ToString, ""})
                ret.Msg = ex.Message
            End Try
            Return ret
        End Function
    End Class

    Public Class MainApplication
        Public Shared Sub startUpMes(mesdata As BatteryInfo)
            ' Read battery information from CSV file
            Dim batteryList As New AddProductOcvList

            Try
                'batteryList.AddProductOcvList.Add(mesdata)
                If batteryList.AddProductOcvList IsNot Nothing Then
                    batteryList.AddProductOcvList.Add(mesdata)
                Else
                    ' 处理未初始化的情况，可能初始化它或引发异常
                End If
            Catch ex As Exception

            End Try
            ' Upload battery data to MES system
            Dim mesResponse As MesResponse = MesApi.UploadBatteryData(batteryList)
            'Console.WriteLine("Response Message: " & mesResponse.Msg)
        End Sub
        Public Shared Sub startUpEQStatusMes(mesdata As EQEquipmentInfo)
            ' Upload battery data to MES system
            Dim mesResponse As MesResponse = MesApi.UploadEQStatusData(mesdata)
            'Console.WriteLine("Response Message: " & mesResponse.Msg)
        End Sub
    End Class

End Module
