Imports System

<Flags()>
Friend Enum InitSettingsChangeKind
    None = 0
    Display = 1
    TestRules = 2
    DeviceConnection = 4
    Instrument = 8
    ExternalServices = 16
    Station = 32
    AccessControl = 64
    ProbeManagement = 128
    Calibration = 256
End Enum

''' <summary>
''' Classifies which parts of the running application are affected by an Initset change.
''' </summary>
Friend NotInheritable Class InitSettingsApplyPolicy
    Private Sub New()
    End Sub

    Public Shared Function Classify(previousSettings As Initset,
                                    currentSettings As Initset) As InitSettingsChangeKind
        Dim changes As InitSettingsChangeKind = InitSettingsChangeKind.None

        If previousSettings.ChannelPassColorArgb <> currentSettings.ChannelPassColorArgb OrElse
           previousSettings.ChannelTestingColorArgb <> currentSettings.ChannelTestingColorArgb OrElse
           previousSettings.ChannelIdleColorArgb <> currentSettings.ChannelIdleColorArgb OrElse
           previousSettings.ChannelResistanceNgColorArgb <> currentSettings.ChannelResistanceNgColorArgb OrElse
           previousSettings.ChannelVoltageNgColorArgb <> currentSettings.ChannelVoltageNgColorArgb OrElse
           previousSettings.ChannelSelectedColorArgb <> currentSettings.ChannelSelectedColorArgb Then
            changes = changes Or InitSettingsChangeKind.Display
        End If

        If previousSettings.upResistance <> currentSettings.upResistance OrElse
           previousSettings.downResistance <> currentSettings.downResistance OrElse
           previousSettings.upVoltage <> currentSettings.upVoltage OrElse
           previousSettings.downVoltage <> currentSettings.downVoltage OrElse
           previousSettings.txtCodeLenth <> currentSettings.txtCodeLenth OrElse
           previousSettings.delayTime <> currentSettings.delayTime OrElse
           previousSettings.overTemperateValue <> currentSettings.overTemperateValue Then
            changes = changes Or InitSettingsChangeKind.TestRules
        End If

        If previousSettings.ScanerPort <> currentSettings.ScanerPort OrElse
           previousSettings.ScanerRate <> currentSettings.ScanerRate OrElse
           previousSettings.SCMIpAdd <> currentSettings.SCMIpAdd OrElse
           previousSettings.SCM_port <> currentSettings.SCM_port OrElse
           previousSettings.SCMComPort <> currentSettings.SCMComPort OrElse
           previousSettings.SCMComRate <> currentSettings.SCMComRate OrElse
           previousSettings.IOPort <> currentSettings.IOPort OrElse
           previousSettings.IORate <> currentSettings.IORate OrElse
           previousSettings.PLCMode <> currentSettings.PLCMode OrElse
           previousSettings.PLCIP <> currentSettings.PLCIP OrElse
           previousSettings.PLCTcpPort <> currentSettings.PLCTcpPort OrElse
           previousSettings.PLCComPort <> currentSettings.PLCComPort OrElse
           previousSettings.PLCComRate <> currentSettings.PLCComRate OrElse
           previousSettings.TemperatureComPort <> currentSettings.TemperatureComPort OrElse
           previousSettings.TemperatureComRate <> currentSettings.TemperatureComRate OrElse
           previousSettings.Retest3562Port <> currentSettings.Retest3562Port OrElse
           previousSettings.Retest3562Rate <> currentSettings.Retest3562Rate Then
            changes = changes Or InitSettingsChangeKind.DeviceConnection
        End If

        If previousSettings.IpAddOf34461 <> currentSettings.IpAddOf34461 OrElse
           previousSettings.ComRateMode <> currentSettings.ComRateMode OrElse
           previousSettings.Recom <> currentSettings.Recom OrElse
           previousSettings.ReRate <> currentSettings.ReRate OrElse
           previousSettings.Dmm34461VoltageRange <> currentSettings.Dmm34461VoltageRange OrElse
           previousSettings.Dmm34461Nplc <> currentSettings.Dmm34461Nplc OrElse
           previousSettings.Dmm34461AutoZero <> currentSettings.Dmm34461AutoZero OrElse
           previousSettings.Main3562ResistanceRange <> currentSettings.Main3562ResistanceRange OrElse
           previousSettings.Main3562VoltageRange <> currentSettings.Main3562VoltageRange OrElse
           previousSettings.Main3562SampleRate <> currentSettings.Main3562SampleRate OrElse
           previousSettings.Main3562Function <> currentSettings.Main3562Function OrElse
           previousSettings.Retest3562ResistanceRange <> currentSettings.Retest3562ResistanceRange OrElse
           previousSettings.Retest3562VoltageRange <> currentSettings.Retest3562VoltageRange OrElse
           previousSettings.Retest3562SampleRate <> currentSettings.Retest3562SampleRate OrElse
           previousSettings.Retest3562Function <> currentSettings.Retest3562Function Then
            changes = changes Or InitSettingsChangeKind.Instrument
        End If

        If previousSettings.chkDatabase <> currentSettings.chkDatabase OrElse
           previousSettings.strDatabase <> currentSettings.strDatabase OrElse
           previousSettings.sPath <> currentSettings.sPath OrElse
           previousSettings.autoUpdate <> currentSettings.autoUpdate OrElse
           previousSettings.strUserID <> currentSettings.strUserID OrElse
           previousSettings.strpwd <> currentSettings.strpwd OrElse
           previousSettings.MesApiAddress <> currentSettings.MesApiAddress OrElse
           previousSettings.EquipPC_ID <> currentSettings.EquipPC_ID OrElse
           previousSettings.EquipPC_Password <> currentSettings.EquipPC_Password Then
            changes = changes Or InitSettingsChangeKind.ExternalServices
        End If

        If previousSettings.stationSelect <> currentSettings.stationSelect Then
            changes = changes Or InitSettingsChangeKind.Station
        End If

        If previousSettings.PsdOfAdmin <> currentSettings.PsdOfAdmin OrElse
           previousSettings.PsdOfOper <> currentSettings.PsdOfOper OrElse
           previousSettings.PsdOfGuest <> currentSettings.PsdOfGuest Then
            changes = changes Or InitSettingsChangeKind.AccessControl
        End If

        If previousSettings.Probelifespan <> currentSettings.Probelifespan OrElse
           previousSettings.ProbeUsedCount <> currentSettings.ProbeUsedCount Then
            changes = changes Or InitSettingsChangeKind.ProbeManagement
        End If

        If previousSettings.sngV_B <> currentSettings.sngV_B OrElse
           previousSettings.sngR_B <> currentSettings.sngR_B OrElse
           previousSettings.cailibrateModelValue <> currentSettings.cailibrateModelValue Then
            changes = changes Or InitSettingsChangeKind.Calibration
        End If

        Return changes
    End Function

    Public Shared Function RequiresDeviceRecheck(changes As InitSettingsChangeKind) As Boolean
        Return (changes And (InitSettingsChangeKind.DeviceConnection Or InitSettingsChangeKind.Instrument)) <> InitSettingsChangeKind.None
    End Function

    Public Shared Function CanApplyNow(testRunning As Boolean,
                                       automaticCycleBusy As Boolean,
                                       deviceCheckRunning As Boolean,
                                       engineeringWindowOpen As Boolean) As Boolean
        Return Not testRunning AndAlso
               Not automaticCycleBusy AndAlso
               Not deviceCheckRunning AndAlso
               Not engineeringWindowOpen
    End Function

    Public Shared Function RequiresSafeBoundary(changes As InitSettingsChangeKind) As Boolean
        Const boundaryChanges As InitSettingsChangeKind = InitSettingsChangeKind.TestRules Or
                                                           InitSettingsChangeKind.DeviceConnection Or
                                                           InitSettingsChangeKind.Instrument Or
                                                           InitSettingsChangeKind.ExternalServices Or
                                                           InitSettingsChangeKind.Station Or
                                                           InitSettingsChangeKind.ProbeManagement Or
                                                           InitSettingsChangeKind.Calibration
        Return (changes And boundaryChanges) <> InitSettingsChangeKind.None
    End Function

    Public Shared Function RequiresScmRecheck(previousSettings As Initset,
                                              currentSettings As Initset) As Boolean
        Return previousSettings.SCMIpAdd <> currentSettings.SCMIpAdd OrElse
               previousSettings.SCM_port <> currentSettings.SCM_port OrElse
               previousSettings.SCMComPort <> currentSettings.SCMComPort OrElse
               previousSettings.SCMComRate <> currentSettings.SCMComRate
    End Function

    Public Shared Function RequiresDmmRecheck(previousSettings As Initset,
                                              currentSettings As Initset) As Boolean
        Return previousSettings.IpAddOf34461 <> currentSettings.IpAddOf34461 OrElse
               previousSettings.Dmm34461VoltageRange <> currentSettings.Dmm34461VoltageRange OrElse
               previousSettings.Dmm34461Nplc <> currentSettings.Dmm34461Nplc OrElse
               previousSettings.Dmm34461AutoZero <> currentSettings.Dmm34461AutoZero
    End Function

    Public Shared Function RequiresMain3562Recheck(previousSettings As Initset,
                                                   currentSettings As Initset) As Boolean
        Return previousSettings.Recom <> currentSettings.Recom OrElse
               previousSettings.ReRate <> currentSettings.ReRate OrElse
               previousSettings.Main3562ResistanceRange <> currentSettings.Main3562ResistanceRange OrElse
               previousSettings.Main3562VoltageRange <> currentSettings.Main3562VoltageRange OrElse
               previousSettings.Main3562SampleRate <> currentSettings.Main3562SampleRate OrElse
               previousSettings.Main3562Function <> currentSettings.Main3562Function
    End Function

    Public Shared Function RequiresRetest3562Recheck(previousSettings As Initset,
                                                     currentSettings As Initset) As Boolean
        Return previousSettings.Retest3562Port <> currentSettings.Retest3562Port OrElse
               previousSettings.Retest3562Rate <> currentSettings.Retest3562Rate OrElse
               previousSettings.Retest3562ResistanceRange <> currentSettings.Retest3562ResistanceRange OrElse
               previousSettings.Retest3562VoltageRange <> currentSettings.Retest3562VoltageRange OrElse
               previousSettings.Retest3562SampleRate <> currentSettings.Retest3562SampleRate OrElse
               previousSettings.Retest3562Function <> currentSettings.Retest3562Function
    End Function

    Public Shared Function RequiresPlcClientRestart(previousSettings As Initset,
                                                    currentSettings As Initset) As Boolean
        Return previousSettings.PLCMode <> currentSettings.PLCMode OrElse
               previousSettings.PLCIP <> currentSettings.PLCIP OrElse
               previousSettings.PLCTcpPort <> currentSettings.PLCTcpPort OrElse
               previousSettings.PLCComPort <> currentSettings.PLCComPort OrElse
               previousSettings.PLCComRate <> currentSettings.PLCComRate
    End Function

    Public Shared Function RequiresScannerClientRestart(previousSettings As Initset,
                                                        currentSettings As Initset) As Boolean
        Return previousSettings.ScanerPort <> currentSettings.ScanerPort OrElse
               previousSettings.ScanerRate <> currentSettings.ScanerRate
    End Function

    Public Shared Function RequiresTemperatureClientReset(previousSettings As Initset,
                                                          currentSettings As Initset) As Boolean
        Return previousSettings.TemperatureComPort <> currentSettings.TemperatureComPort OrElse
               previousSettings.TemperatureComRate <> currentSettings.TemperatureComRate
    End Function

    Public Shared Function RequiresDatabaseRecheck(previousSettings As Initset,
                                                   currentSettings As Initset) As Boolean
        Return previousSettings.chkDatabase <> currentSettings.chkDatabase OrElse
               previousSettings.strDatabase <> currentSettings.strDatabase OrElse
               previousSettings.sPath <> currentSettings.sPath OrElse
               previousSettings.strUserID <> currentSettings.strUserID OrElse
               previousSettings.strpwd <> currentSettings.strpwd
    End Function

    Public Shared Function RequiresMesRecheck(previousSettings As Initset,
                                              currentSettings As Initset) As Boolean
        Return previousSettings.MesApiAddress <> currentSettings.MesApiAddress OrElse
               previousSettings.EquipPC_ID <> currentSettings.EquipPC_ID OrElse
               previousSettings.EquipPC_Password <> currentSettings.EquipPC_Password
    End Function

End Class
