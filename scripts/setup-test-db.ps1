<#
.SYNOPSIS
  Tao/lam moi database JobHunterDB_Test dung cho integration test
  (WebApplicationFactory + SQL Server that, KHONG dung SQLite - xem
  CLAUDE.md muc Testing strategy). Chay lai script nay bat cu luc nao
  can reset sach du lieu test ve trang thai ban dau (chi co seed,
  khong co du lieu rac tu cac lan chay test truoc).

.PARAMETER Server
  SQL Server instance, mac dinh localhost (dung Docker container o
  Phase 0).
#>
param(
    [string]$Server = "localhost",
    [string]$SaPassword = "DevDocker_2026!Sql"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$createSql = Get-Content "$root\database\JobHunter_CreateTables.sql" -Raw
$seedSql = Get-Content "$root\database\JobHunter_SeedData.sql" -Raw

$testCreateSql = $createSql -replace "JobHunterDB", "JobHunterDB_Test"
$testSeedSql = $seedSql -replace "JobHunterDB", "JobHunterDB_Test"

$dropTestDb = "IF DB_ID('JobHunterDB_Test') IS NOT NULL BEGIN ALTER DATABASE JobHunterDB_Test SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE JobHunterDB_Test; END"

$tmpDrop = New-TemporaryFile
$tmpCreate = New-TemporaryFile
$tmpSeed = New-TemporaryFile
Set-Content -Path $tmpDrop -Value $dropTestDb -Encoding utf8
Set-Content -Path $tmpCreate -Value $testCreateSql -Encoding utf8
Set-Content -Path $tmpSeed -Value $testSeedSql -Encoding utf8

Write-Host "Dropping old JobHunterDB_Test (neu co)..."
sqlcmd -S $Server -U sa -P $SaPassword -C -f i:65001 -i $tmpDrop.FullName

Write-Host "Creating JobHunterDB_Test tu JobHunter_CreateTables.sql..."
# -f i:65001: doc file input dung UTF-8, tranh hong du lieu tieng Viet
# co dau (N'...') - da gap loi that 2026-08-10, khong phai loi hien
# thi ma la du lieu bi ghi sai vao DB neu thieu co flag nay.
sqlcmd -S $Server -U sa -P $SaPassword -C -f i:65001 -i $tmpCreate.FullName

Write-Host "Seeding JobHunterDB_Test tu JobHunter_SeedData.sql..."
sqlcmd -S $Server -U sa -P $SaPassword -C -f i:65001 -i $tmpSeed.FullName

Remove-Item $tmpDrop, $tmpCreate, $tmpSeed -Force

Write-Host "`nJobHunterDB_Test san sang."
