Imports System.Collections.Generic

Public Enum PlcAutomaticTestState
    Idle
    ReadingTrayCode
    WaitingManualTrayCode
    WaitingR150Reset
    ArmingR151
    WaitingFixturePressed
    PreparingTest
    InitialTesting
    AutoRetesting
    Finalizing
    WaitingNgSlotReady
    WaitingR161Ack
    WaitingCycleReset
    Fault
End Enum

Public Class AutomaticTestCycle
    Public Property TrayCode As String
    Public Property NeedNgRetest As Boolean
    Public Property RequiresNgSorting As Boolean
    Public Property MaxRetestRounds As Integer
    Public Property CurrentRetestRound As Integer
    Public Property CurrentNgChannels As New HashSet(Of Integer)()
    Public Property WrittenNgChannels As New HashSet(Of Integer)()
    Public Property FinalResultPublished As Boolean
    Public Property NgGradesWritten As Boolean
    Public Property R161Written As Boolean

    Public Sub Reset()
        TrayCode = String.Empty
        NeedNgRetest = False
        RequiresNgSorting = False
        MaxRetestRounds = 0
        CurrentRetestRound = 0
        CurrentNgChannels.Clear()
        WrittenNgChannels.Clear()
        FinalResultPublished = False
        NgGradesWritten = False
        R161Written = False
    End Sub
End Class

Public Class ChannelTestResult
    Public Property ChannelNumber As Integer
    Public Property Voltage As Single
    Public Property Resistance As Single
    Public Property IsNg As Boolean
End Class

Public Class TestPassResult
    Public Property Completed As Boolean
    Public Property Results As New List(Of ChannelTestResult)()
    Public Property ErrorMessage As String
End Class
