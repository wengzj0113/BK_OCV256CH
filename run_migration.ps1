# OCV_DATA 数据库迁移执行脚本
# 在 SQL Server 环境的 PowerShell 中运行
#
# 用法:
#   1. 修改下方 $ServerName / $UserId / $Password 为实际值
#   2. 先 DryRun 模式确认数据条数
#   3. 确认无误后改为 -Mode Execute 正式执行
#
# 示例:
#   .\run_migration.ps1 -ServerName "localhost" -UserId "sa" -Password "xxx" -Mode DryRun
#   .\run_migration.ps1 -ServerName "localhost" -UserId "sa" -Password "xxx" -Mode Execute

param(
    [Parameter(Mandatory=$true)]
    [string]$ServerName,

    [Parameter(Mandatory=$true)]
    [string]$UserId,

    [Parameter(Mandatory=$true)]
    [string]$Password,

    [ValidateSet('DryRun','Execute','InitializeOnly')]
    [string]$Mode = 'DryRun'
)

$ErrorActionPreference = 'Stop'
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$SqlDir = Join-Path $ScriptDir 'docs\sql'

# 1. sqlcmd 路径探测
$sqlcmdPaths = @(
    'sqlcmd',
    'C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\180\Tools\Binn\sqlcmd.exe',
    'C:\Program Files\Microsoft SQL Server\150\Tools\Binn\sqlcmd.exe',
    'C:\Program Files\Microsoft SQL Server\140\Tools\Binn\sqlcmd.exe'
)
$sqlcmd = $null
foreach ($p in $sqlcmdPaths) {
    try {
        if (Get-Command $p -ErrorAction SilentlyContinue) { $sqlcmd = $p; break }
    } catch {}
}
if (-not $sqlcmd) {
    Write-Error 'sqlcmd 未找到,请确认已安装 SQL Server Command Line Tools (https://aka.ms/sqlcmd)'
    exit 1
}

# 2. 连接测试
Write-Host "=== OCV_DATA 数据库迁移 ===" -ForegroundColor Cyan
Write-Host "服务器: $ServerName"
Write-Host "用户:   $UserId"
Write-Host "模式:   $Mode"
Write-Host "脚本目录: $SqlDir"
Write-Host ""

Write-Host "[预检] 测试 SQL Server 连接 ..." -ForegroundColor Yellow
$testResult = & $sqlcmd -S $ServerName -U $UserId -P $Password -Q "SELECT @@VERSION" 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "SQL Server 连接失败: $testResult"
    exit 1
}
Write-Host "[预检] 连接成功。" -ForegroundColor Green
Write-Host ""

# 3. 执行脚本(根据 Mode 决定步进)
$DryRunFlag = if ($Mode -eq 'Execute') { 0 } else { 1 }

# Step 0: 初始化数据库
Write-Host "[0/6] 执行 000_init_database.sql ..." -ForegroundColor Yellow
& $sqlcmd -S $ServerName -U $UserId -P $Password -i (Join-Path $SqlDir '000_init_database.sql') 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "000_init_database.sql 执行失败,中止。"
    exit 1
}
Write-Host ""

# Step 1: Schema
Write-Host "[1/6] 执行 001_create_ocv_data_schema.sql ..." -ForegroundColor Yellow
& $sqlcmd -S $ServerName -U $UserId -P $Password -i (Join-Path $SqlDir '001_create_ocv_data_schema.sql') 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "001_create_ocv_data_schema.sql 执行失败,中止。"
    exit 1
}
Write-Host ""

# Step 2: 存储过程
Write-Host "[2/6] 执行 002_create_core_procedures.sql ..." -ForegroundColor Yellow
& $sqlcmd -S $ServerName -U $UserId -P $Password -i (Join-Path $SqlDir '002_create_core_procedures.sql') 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "002_create_core_procedures.sql 执行失败,中止。"
    exit 1
}
Write-Host ""

# InitializeOnly 模式到这里结束
if ($Mode -eq 'InitializeOnly') {
    Write-Host "=== 初始化完成 (InitializeOnly) ===" -ForegroundColor Green
    Write-Host "Schema/存储过程已部署,尚未执行数据迁移。" -ForegroundColor Cyan
    Write-Host "如需执行数据迁移,运行 -Mode DryRun 或 -Mode Execute"
    exit 0
}

# Step 3: 迁移 UnsentData + PendingUploads
Write-Host "[3/6] 迁移 CWSQL.UnsentData/CWSQL.PendingUploads -> OCV_DATA.UploadOutbox ($Mode) ..." -ForegroundColor Yellow
& $sqlcmd -S $ServerName -U $UserId -P $Password -d OCV_DATA -Q "
DECLARE @m int, @s int;
EXEC dbo.usp_MigrateLegacyUnsentDataToUploadOutbox
    @SourceDatabase = N'CWSQL',
    @DryRun = $DryRunFlag,
    @MigratedRows = @m OUTPUT,
    @SkippedRows = @s OUTPUT;
SELECT @m AS MigratedRows, @s AS SkippedRows;" 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "上传缓存迁移失败,中止。"
    exit 1
}
Write-Host ""

# Step 4: 迁移分档规则
Write-Host "[4/6] 迁移 CWSQL.CW_tblCondition/CW_tblSelFld -> OCV_DATA.GradeScheme/GradeRule ($Mode) ..." -ForegroundColor Yellow
& $sqlcmd -S $ServerName -U $UserId -P $Password -d OCV_DATA -i (Join-Path $SqlDir '007_migrate_grade_rules.sql') 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "007_migrate_grade_rules.sql 执行失败,中止。"
    exit 1
}
& $sqlcmd -S $ServerName -U $UserId -P $Password -d OCV_DATA -Q "
DECLARE @s int, @r int, @f int, @k int;
EXEC dbo.usp_MigrateLegacyGradeRules
    @SourceDatabase = N'CWSQL',
    @DryRun = $DryRunFlag,
    @MigratedSchemes = @s OUTPUT,
    @MigratedRules = @r OUTPUT,
    @MigratedFields = @f OUTPUT,
    @SkippedRows = @k OUTPUT;
SELECT @s AS Schemes, @r AS Rules, @f AS Fields, @k AS Skipped;" 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "分档规则迁移失败,中止。"
    exit 1
}
Write-Host ""

# Step 5: 健康检查
Write-Host "[5/6] 执行 008_post_migration_health_check.sql ..." -ForegroundColor Yellow
& $sqlcmd -S $ServerName -U $UserId -P $Password -i (Join-Path $SqlDir '008_post_migration_health_check.sql') 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Warning "健康检查脚本退出码非 0,请检查上方输出。"
}
Write-Host ""

# Step 6: 完成
Write-Host "=== 迁移完成 ($Mode) ===" -ForegroundColor Green
if ($Mode -eq 'DryRun') {
    Write-Host "请确认 [3/6] MigratedRows 和 [4/6] Schemes/Rules/Fields 条数符合预期" -ForegroundColor Cyan
    Write-Host "确认后用 -Mode Execute 重新运行正式执行" -ForegroundColor Cyan
} else {
    Write-Host "[8/6] 已正式迁移,可以切换应用连接到 OCV_DATA" -ForegroundColor Cyan
}