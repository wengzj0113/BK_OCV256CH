Imports System.Drawing
Imports System.Windows.Forms

Public Structure ChannelStatistics
    Public Total As Integer
    Public PassCount As Integer
    Public NgCount As Integer
    Public TestingCount As Integer
    Public IdleCount As Integer
    Public ReadOnly Property PassRate As Double
        Get
            Return RateFor(PassCount)
        End Get
    End Property
    Public ReadOnly Property NgRate As Double
        Get
            Return RateFor(NgCount)
        End Get
    End Property
    Public ReadOnly Property TestingRate As Double
        Get
            Return RateFor(TestingCount)
        End Get
    End Property
    Public ReadOnly Property IdleRate As Double
        Get
            Return RateFor(IdleCount)
        End Get
    End Property
    Private Function RateFor(ByVal count As Integer) As Double
        If Total <= 0 Then Return 0.0R
        Return count * 100.0R / Total
    End Function
End Structure

Public Class StatisticsControl
    Inherits UserControl
    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub UpdateStatistics(ByVal statistics As ChannelStatistics)
        totalValue.Text = statistics.Total.ToString()
        passValue.Text = FormatMetric(statistics.PassCount, statistics.PassRate)
        ngValue.Text = FormatMetric(statistics.NgCount, statistics.NgRate)
        testingValue.Text = FormatMetric(statistics.TestingCount, statistics.TestingRate)
        idleValue.Text = FormatMetric(statistics.IdleCount, statistics.IdleRate)
    End Sub

    Private Shared Function FormatMetric(ByVal count As Integer, ByVal rate As Double) As String
        Return String.Format("{0}  ({1:0.0}%)", count, rate)
    End Function
End Class
