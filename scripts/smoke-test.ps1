<#
.SYNOPSIS
  Smoke test JobHunter API theo dung hop dong o docs/IMPLEMENTATION_PLAN.md muc 5 (API) + muc 6 (business rule).
  Output CHI in 1 dong [PASS]/[FAIL] moi buoc + 1 dong tong ket cuoi cung.
  Chi dump chi tiet response khi buoc do FAIL (de debug), giu output ngan
  gon toi da cho Claude Code doc lai it token nhat co the.

.PARAMETER Phase
  0-4: chi chay den het phase do (phase sau phu thuoc du lieu phase truoc
  nen luon chay tuan tu tu 0). Bo qua = chay full toi Phase 4.

.PARAMETER BaseUrl
  Goc API, mac dinh http://localhost:5147/api (dung port thuc te trong
  backend/JobHunter.API/Properties/launchSettings.json sau Phase 0).

.EXAMPLE
  .\scripts\smoke-test.ps1 -Phase 1
#>
param(
    [int]$Phase = 4,
    [string]$BaseUrl = "http://localhost:5147/api"
)

$ErrorActionPreference = "Stop"
$script:pass = 0
$script:fail = 0
$script:failDetails = @()

function Check {
    param(
        [string]$Name,
        [scriptblock]$Action,
        [scriptblock]$Assert
    )
    try {
        $result = & $Action
        $ok = & $Assert $result
        if ($ok) {
            Write-Host "[PASS] $Name"
            $script:pass++
        } else {
            Write-Host "[FAIL] $Name (assert khong khop)"
            $script:fail++
            $script:failDetails += @{ Name = $Name; Detail = ($result | ConvertTo-Json -Depth 5 -Compress) }
        }
        return $result
    } catch {
        $statusCode = $null
        if ($_.Exception.Response) { $statusCode = [int]$_.Exception.Response.StatusCode }
        Write-Host "[FAIL] $Name (loi: $($_.Exception.Message), status=$statusCode)"
        $script:fail++
        $script:failDetails += @{ Name = $Name; Detail = "$($_.Exception.Message) status=$statusCode" }
        return $null
    }
}

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Path,
        $Body = $null,
        [string]$Token = $null,
        [switch]$ExpectError
    )
    $headers = @{}
    if ($Token) { $headers["Authorization"] = "Bearer $Token" }
    $params = @{
        Method  = $Method
        Uri     = "$BaseUrl$Path"
        Headers = $headers
    }
    if ($Body -ne $null) {
        $params["Body"] = ($Body | ConvertTo-Json -Depth 5)
        $params["ContentType"] = "application/json"
    }
    if ($ExpectError) {
        try {
            Invoke-RestMethod @params
            throw "Ky vong loi nhung request thanh cong"
        } catch [System.Net.WebException] {
            return [int]$_.Exception.Response.StatusCode
        } catch {
            if ($_.Exception.Response) { return [int]$_.Exception.Response.StatusCode }
            throw
        }
    }
    return Invoke-RestMethod @params
}

$rand = Get-Random
$ntdEmail = "ntd_smoketest_$rand@test.local"
$uvEmail  = "uv_smoketest_$rand@test.local"
$password = "Test1234"

Write-Host "=== Phase 0: Setup / seed data ==="
$skills = Check "GET /api/skills tra ve >= 15 ky nang seed" `
    { Invoke-Api GET "/skills" } `
    { param($r) $r.Count -ge 15 }

if ($Phase -eq 0) {
    Write-Host "`n=== TOM TAT: $pass PASS / $fail FAIL ==="
    exit
}

Write-Host "`n=== Phase 1: Auth ==="
$reg1 = Check "Dang ky NTD thanh cong (dung field BM02)" `
    { Invoke-Api POST "/auth/register/employer" @{ tenCongTy="Cong Ty Test $rand"; diaChi="123 Duong Test"; email=$ntdEmail; matKhau=$password; sdt="0900000000"; xacNhanMatKhau=$password } } `
    { param($r) $r.maTK -gt 0 }

Check "Dang ky NTD trung email -> loi (QD02, MS13)" `
    { Invoke-Api POST "/auth/register/employer" -ExpectError @{ tenCongTy="Trung email"; diaChi="X"; email=$ntdEmail; matKhau=$password; sdt="0900000000"; xacNhanMatKhau=$password } } `
    { param($status) $status -eq 400 }

Check "Dang ky mat khau qua ngan -> loi (QD01/TS1, MS14)" `
    { Invoke-Api POST "/auth/register/candidate" -ExpectError @{ hoTen="A"; matKhau="a1"; email="ngan_$rand@test.local"; xacNhanMatKhau="a1"; sdt="0900000000" } } `
    { param($status) $status -eq 400 }

$loginNtd = Check "Dang nhap NTD thanh cong, co token" `
    { Invoke-Api POST "/auth/login" @{ email=$ntdEmail; matKhau=$password } } `
    { param($r) $r.token.Length -gt 0 }
$ntdToken = $loginNtd.token

$loginAdmin = Check "Dang nhap Admin seed (admin@jobhunter.local) thanh cong" `
    { Invoke-Api POST "/auth/login" @{ email="admin@jobhunter.local"; matKhau="Admin@123" } } `
    { param($r) $r.vaiTro -eq "Admin" }
$adminToken = $loginAdmin.token

$reg2 = Check "Dang ky Ung vien thanh cong (dung field BM01)" `
    { Invoke-Api POST "/auth/register/candidate" @{ hoTen="Ung Vien Test"; matKhau=$password; email=$uvEmail; xacNhanMatKhau=$password; sdt="0911111111" } } `
    { param($r) $r.maTK -gt 0 }

$loginUv = Check "Dang nhap Ung vien thanh cong" `
    { Invoke-Api POST "/auth/login" @{ email=$uvEmail; matKhau=$password } } `
    { param($r) $r.token.Length -gt 0 }
$uvToken = $loginUv.token

Check "Dang nhap sai mat khau 5 lan lien tiep -> khoa tam (QD03/TS2/TS3)" {
    for ($i = 0; $i -lt 5; $i++) {
        Invoke-Api POST "/auth/login" -ExpectError @{ email=$uvEmail; matKhau="saimatkhau" } | Out-Null
    }
    Invoke-Api POST "/auth/login" -ExpectError @{ email=$uvEmail; matKhau=$password }
} { param($status) $status -eq 403 }

if ($Phase -eq 1) {
    Write-Host "`n=== TOM TAT: $pass PASS / $fail FAIL ==="
    if ($fail -gt 0) { $failDetails | ForEach-Object { Write-Host "--- $($_.Name) ---`n$($_.Detail)" } }
    exit
}

Write-Host "`n=== Phase 2: Dang tin + Duyet + Xem cong khai ==="
$maKyNangCs = ($skills | Where-Object { $_.tenKyNang -eq "C#" } | Select-Object -First 1).maKyNang

Check "Dang tin voi HanNopHoSo = hom nay -> loi (QD09/TS7)" `
    { Invoke-Api POST "/jobs" -Token $ntdToken -ExpectError @{
        tieuDe="Vi tri test"; moTaCongViec="mo ta"; hanNopHoSo=(Get-Date).ToString("yyyy-MM-dd")
        kyNangYeuCau=@(@{ maKyNang=$maKyNangCs; mucDoUuTien="BatBuoc" })
      } } `
    { param($status) $status -eq 400 }

$job = Check "Dang tin hop le -> 201, TrangThai=ChoDuyet" `
    { Invoke-Api POST "/jobs" -Token $ntdToken @{
        tieuDe="Backend Developer Test $rand"; moTaCongViec="mo ta cong viec test"
        hanNopHoSo=(Get-Date).AddDays(7).ToString("yyyy-MM-dd")
        kyNangYeuCau=@(@{ maKyNang=$maKyNangCs; mucDoUuTien="BatBuoc" })
      } } `
    { param($r) $r.trangThai -eq "ChoDuyet" }
$maTin = $job.maTin

$publicListBefore = Check "Tin ChoDuyet KHONG xuat hien trong /api/jobs cong khai" `
    { Invoke-Api GET "/jobs" } `
    { param($r) -not ($r | Where-Object { $_.maTin -eq $maTin }) }

Check "Admin duyet tin thanh cong" `
    { Invoke-Api POST "/jobs/$maTin/approve" -Token $adminToken } `
    { param($r) $true }

$publicListAfter = Check "Tin DaDuyet XUAT HIEN trong /api/jobs cong khai" `
    { Invoke-Api GET "/jobs" } `
    { param($r) $r | Where-Object { $_.maTin -eq $maTin } }

if ($Phase -eq 2) {
    Write-Host "`n=== TOM TAT: $pass PASS / $fail FAIL ==="
    if ($fail -gt 0) { $failDetails | ForEach-Object { Write-Host "--- $($_.Name) ---`n$($_.Detail)" } }
    exit
}

Write-Host "`n=== Phase 3: CV + Ung tuyen ==="
$cv = Check "Tao CV truc tuyen thanh cong" `
    { Invoke-Api POST "/cvs/online" -Token $uvToken @{
        tenCv="CV Test $rand"; trinhDoHocVan="DaiHoc"
        kyNang=@(@{ maKyNang=$maKyNangCs; mucDoThanhThao="ThanhThao" })
      } } `
    { param($r) $r.maCV -gt 0 }
$maCv = $cv.maCV

$app1 = Check "Ung tuyen lan 1 -> 201" `
    { Invoke-Api POST "/applications" -Token $uvToken @{ maCv=$maCv; maTin=$maTin } } `
    { param($r) $r.maDon -gt 0 }

Check "Ung tuyen lan 2 cung tin, cung ung vien -> loi (QD10/TS8, MS31)" `
    { Invoke-Api POST "/applications" -Token $uvToken -ExpectError @{ maCv=$maCv; maTin=$maTin; thuGioiThieu="lan 2" } } `
    { param($status) $status -eq 409 }

if ($Phase -eq 3) {
    Write-Host "`n=== TOM TAT: $pass PASS / $fail FAIL ==="
    if ($fail -gt 0) { $failDetails | ForEach-Object { Write-Host "--- $($_.Name) ---`n$($_.Detail)" } }
    exit
}

Write-Host "`n=== Phase 4: Loc ung vien (3 tieu chi dung BM14) + diem phu hop ==="
$applicants = Check "NTD xem DS ung vien (khong loc), thay % phu hop = 100 (CV khop 1/1 ky nang yeu cau)" `
    { Invoke-Api GET "/jobs/$maTin/applicants" -Token $ntdToken } `
    { param($r) ($r | Where-Object { $_.maDon -eq $app1.maDon }).phanTramPhuHop -eq 100 }

Check "Loc theo dung ky nang yeu cau -> van thay ung vien" `
    { Invoke-Api GET "/jobs/$maTin/applicants/filter?maKyNang=$maKyNangCs" -Token $ntdToken } `
    { param($r) $r | Where-Object { $_.maDon -eq $app1.maDon } }

Check "Loc theo hoc van khac (CaoDang) khi CV la DaiHoc -> danh sach rong + MS07" `
    { Invoke-Api GET "/jobs/$maTin/applicants/filter?trinhDoHocVan=CaoDang" -Token $ntdToken } `
    { param($r) $r.Count -eq 0 }

Check "Loc theo minNamKinhNghiem=99 (khong ai du) -> danh sach rong" `
    { Invoke-Api GET "/jobs/$maTin/applicants/filter?minNamKinhNghiem=99" -Token $ntdToken } `
    { param($r) $r.Count -eq 0 }

Write-Host "`n=== TOM TAT: $pass PASS / $fail FAIL ==="
if ($fail -gt 0) {
    Write-Host "`n--- Chi tiet cac buoc FAIL ---"
    $failDetails | ForEach-Object { Write-Host "* $($_.Name): $($_.Detail)" }
    exit 1
}
exit 0
