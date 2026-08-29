$ErrorActionPreference = 'Stop'

function Assert-Contains([string]$text, [string]$pattern, [string]$message) {
    if ($text -notmatch $pattern) {
        throw $message
    }
}

function Assert-NotContains([string]$text, [string]$pattern, [string]$message) {
    if ($text -match $pattern) {
        throw $message
    }
}

$client = Get-Content -LiteralPath 'SerialMiddleMachineClient.vb' -Raw
$form = Get-Content -LiteralPath 'frmInit.vb' -Raw
$designer = Get-Content -LiteralPath 'frmInit.Designer.vb' -Raw
$module = Get-Content -LiteralPath 'Module1.vb' -Raw

@(
    @{ p = 'RegRelaySettleMs\s+As\s+UShort\s*=\s*&H33'; m = 'missing relay register' },
    @{ p = 'RegChannelSettleMs\s+As\s+UShort\s*=\s*&H34'; m = 'missing channel register' },
    @{ p = 'RegAutoStepMs\s+As\s+UShort\s*=\s*&H36'; m = 'missing instrument register' },
    @{ p = 'RegTriggerPulseMs\s+As\s+UShort\s*=\s*&H37'; m = 'missing trigger register' },
    @{ p = 'RegPhaseGuardMs\s+As\s+UShort\s*=\s*&H6C'; m = 'missing phase guard register' },
    @{ p = 'ReadScmTimingSettings'; m = 'missing SCM read method' },
    @{ p = 'WriteScmTimingSettings'; m = 'missing SCM write method' },
    @{ p = 'WriteRegisterAndVerifyStatus\(item\.Address'; m = 'SCM writes are not verified' }
) | ForEach-Object {
    Assert-Contains $client $_.p $_.m
}

$writeBlock = [regex]::Match($client, 'Public Sub WriteScmTimingSettings[\s\S]*?End Sub').Value
$lastIndex = -1
@(
    'New ScmTimingRegister("继电器稳定时间", RegRelaySettleMs',
    'New ScmTimingRegister("通道稳定时间", RegChannelSettleMs',
    'New ScmTimingRegister("仪表等待时间", RegAutoStepMs',
    'New ScmTimingRegister("触发脉宽时间", RegTriggerPulseMs',
    'New ScmTimingRegister("遍末保护时间", RegPhaseGuardMs'
) | ForEach-Object {
    $index = $writeBlock.IndexOf($_)
    if ($index -lt 0 -or $index -le $lastIndex) {
        throw "SCM timing register order is incomplete: $_"
    }
    $lastIndex = $index
}

Assert-Contains $form 'Handles\s+btnScmRead\.Click' 'read event is not wired'
Assert-Contains $form 'Handles\s+btnScmApply\.Click' 'apply event is not wired'
Assert-Contains $designer 'TabControl1\.Controls\.Add\(Me\.TabScmSet\)' 'SCM tab is not in TabControl'
Assert-Contains $module 'tempConfigPath' 'SCM persistence does not use a temporary file'
Assert-Contains $module 'File\.Replace\(tempConfigPath, configPath' 'SCM persistence does not replace the config atomically'
Assert-Contains $form 'AssignScmTimingSettings' 'SCM config assignment helper is missing'
$normalizeBlock = [regex]::Match($form, 'Private Function NormalizeScmTimingSettings[\s\S]*?End Function').Value
Assert-NotContains $normalizeBlock 'Initsetting\.Scm\w+\s*=' 'normalizing SCM settings mutates global config before apply'

@(
    @{ p = 'txtScmRelaySettleMs'; m = 'missing relay textbox' },
    @{ p = 'txtScmChannelSettleMs'; m = 'missing channel textbox' },
    @{ p = 'txtScmAutoStepMs'; m = 'missing instrument textbox' },
    @{ p = 'txtScmTriggerPulseMs'; m = 'missing trigger textbox' },
    @{ p = 'txtScmPhaseGuardMs'; m = 'missing phase guard textbox' },
    @{ p = 'lblScmRelaySettleMs'; m = 'missing relay label' },
    @{ p = 'lblScmChannelSettleMs'; m = 'missing channel label' },
    @{ p = 'lblScmAutoStepMs'; m = 'missing instrument label' },
    @{ p = 'lblScmTriggerPulseMs'; m = 'missing trigger label' },
    @{ p = 'lblScmPhaseGuardMs'; m = 'missing phase guard label' },
    @{ p = 'lblScmRelayUnit'; m = 'missing relay unit label' },
    @{ p = 'lblScmChannelUnit'; m = 'missing channel unit label' },
    @{ p = 'lblScmAutoStepUnit'; m = 'missing instrument unit label' },
    @{ p = 'lblScmTriggerUnit'; m = 'missing trigger unit label' },
    @{ p = 'lblScmPhaseGuardUnit'; m = 'missing phase guard unit label' },
    @{ p = 'btnScmRead'; m = 'missing read button' },
    @{ p = 'btnScmApply'; m = 'missing apply button' },
    @{ p = 'lblScmStatus'; m = 'missing status label' }
) | ForEach-Object {
    Assert-Contains $designer $_.p $_.m
}

Assert-Contains $designer 'TabScmSet\.Controls\.Add\(Me\.txtScmRelaySettleMs\)' 'relay textbox is not static Designer control'
Assert-Contains $designer 'TabScmSet\.Controls\.Add\(Me\.btnScmApply\)' 'apply button is not static Designer control'
Assert-NotContains $form 'New\s+(TextBox|Label|Button)[^\r\n]*Scm' 'SCM controls are created dynamically'
Assert-NotContains $form 'Scm[^\r\n]*Controls\.Add\(' 'SCM controls are added dynamically'

@(
    @{ p = 'Dim ScmRelaySettleMs\s+As\s+Integer'; m = 'missing relay settings field' },
    @{ p = 'Dim ScmChannelSettleMs\s+As\s+Integer'; m = 'missing channel settings field' },
    @{ p = 'Dim ScmAutoStepMs\s+As\s+Integer'; m = 'missing instrument settings field' },
    @{ p = 'Dim ScmTriggerPulseMs\s+As\s+Integer'; m = 'missing trigger settings field' },
    @{ p = 'Dim ScmPhaseGuardMs\s+As\s+Integer'; m = 'missing phase guard settings field' },
    @{ p = 'SaveInitFile\(\)[\s\S]*ScmRelaySettleMs'; m = 'SCM fields are not persisted' },
    @{ p = 'ReadInitFile\(\)[\s\S]*ScmRelaySettleMs'; m = 'SCM fields are not loaded' }
) | ForEach-Object {
    Assert-Contains $module $_.p $_.m
}

Write-Host 'SCM settings static verification passed.'
