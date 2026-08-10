using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JobHunter.API.Data;
using JobHunter.API.DTOs;
using JobHunter.API.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JobHunter.Tests;

// Test qua HTTP that (TestServer) + SQL Server that (JobHunterDB_Test,
// xem CustomWebApplicationFactory + scripts/setup-test-db.ps1).
// Muc dich rieng cua tang nay (khac unit test EF InMemory o cac file
// khac): xac nhan dung CHECK constraint / unique index cua chinh
// SQL Server, thu duoc bang unit test InMemory vi provider do KHONG
// enforce CHECK constraint hay index that.
public class IntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public IntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullFlow_DangKy_DangNhap_DangTin_Duyet_UngTuyen_HoatDongDungQuaHttpThat()
    {
        var client = _factory.CreateClient();
        var rand = Guid.NewGuid().ToString("N")[..8];

        var regNtd = await client.PostAsJsonAsync("/api/auth/register/employer", new
        {
            tenCongTy = $"IT Corp {rand}", diaChi = "1 Test St", email = $"it_ntd_{rand}@test.local",
            matKhau = "Test1234", sdt = "0900000000", xacNhanMatKhau = "Test1234",
        });
        Assert.Equal(HttpStatusCode.Created, regNtd.StatusCode);

        // Phase 7: dang ky xong chua xac thuc email (DaXacThuc=false), khong
        // dang nhap duoc. Danh dau da xac thuc thang qua DB o day vi muc dich
        // cua test nay la kiem tra toan bo pipeline dang tin/duyet/ung tuyen,
        // KHONG phai re-test luong UC03 (da co rieng o AuthServiceTests +
        // test_phase7.py qua console mock).
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JobHunterDbContext>();
            var taiKhoan = await db.TaiKhoans.FirstAsync(x => x.Email == $"it_ntd_{rand}@test.local");
            taiKhoan.DaXacThuc = true;
            await db.SaveChangesAsync();
        }

        var loginNtd = await client.PostAsJsonAsync("/api/auth/login", new { email = $"it_ntd_{rand}@test.local", matKhau = "Test1234" });
        Assert.Equal(HttpStatusCode.OK, loginNtd.StatusCode);
        var ntdToken = (await loginNtd.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOpts))!.Token;

        var skillsRes = await (await client.GetAsync("/api/skills")).Content.ReadFromJsonAsync<List<SkillDto>>(JsonOpts);
        var maKyNang = skillsRes!.First(s => s.TenKyNang == "Git").MaKyNang;

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ntdToken);
        var postJob = await client.PostAsJsonAsync("/api/jobs", new
        {
            tieuDe = $"IT Job {rand}", moTaCongViec = "mo ta", hanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            kyNangYeuCau = new[] { new { maKyNang, mucDoUuTien = "BatBuoc" } },
        });
        Assert.Equal(HttpStatusCode.Created, postJob.StatusCode);
        var job = await postJob.Content.ReadFromJsonAsync<JobDto>(JsonOpts);

        var loginAdmin = await client.PostAsJsonAsync("/api/auth/login", new { email = "admin@jobhunter.local", matKhau = "Admin@123" });
        var adminToken = (await loginAdmin.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOpts))!.Token;
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        var approve = await client.PostAsync($"/api/jobs/{job!.MaTin}/approve", null);
        Assert.Equal(HttpStatusCode.OK, approve.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        var publicList = await (await client.GetAsync("/api/jobs")).Content.ReadFromJsonAsync<List<JobDto>>(JsonOpts);
        Assert.Contains(publicList!, j => j.MaTin == job.MaTin);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Database_TaiKhoan_TrangThai_CheckConstraint_TuChoiGiaTriSai()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<JobHunterDbContext>();

        var ex = await Assert.ThrowsAsync<SqlException>(() => db.Database.ExecuteSqlRawAsync(
            "INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao) " +
            "VALUES ('checkconstraint_test@x.com', 'x', 'UngVien', 1, 'KhongPhaiEnumHopLe', 0, SYSDATETIME())"));

        Assert.Contains("CHECK constraint", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task Register_HaiNhaTuyenDungKhongCoMaSoThue_CaHaiThanhCong()
    {
        // Hoi quy cho bug that da bat duoc: SQL Server chi cho 1 dong NULL trong
        // cot UNIQUE thuong. MaSoThue luon NULL vi BM02 khong thu luc dang ky ->
        // phai la filtered unique index. Test nay se do do neu ai lo sua nham
        // ve lai UNIQUE thuong.
        var client = _factory.CreateClient();
        var rand = Guid.NewGuid().ToString("N")[..8];

        var res1 = await client.PostAsJsonAsync("/api/auth/register/employer", new
        {
            tenCongTy = $"Corp A {rand}", diaChi = "A", email = $"nomst_a_{rand}@test.local",
            matKhau = "Test1234", sdt = "0900000000", xacNhanMatKhau = "Test1234",
        });
        var res2 = await client.PostAsJsonAsync("/api/auth/register/employer", new
        {
            tenCongTy = $"Corp B {rand}", diaChi = "B", email = $"nomst_b_{rand}@test.local",
            matKhau = "Test1234", sdt = "0900000000", xacNhanMatKhau = "Test1234",
        });

        Assert.Equal(HttpStatusCode.Created, res1.StatusCode);
        Assert.Equal(HttpStatusCode.Created, res2.StatusCode);
    }

    private record LoginResponseDto(string Token, string VaiTro, string HoTenOrTenCongTy);
    private record SkillDto(int MaKyNang, string TenKyNang);
    private record JobDto(int MaTin, string TieuDe, string TrangThai);

    [Fact]
    [Trait("Category", "Integration")]
    public async Task MuaGoiDichVu_VuotGioiHanMienPhi_BiChanChoDenKhiMuaGoi()
    {
        var client = _factory.CreateClient();
        var rand = Guid.NewGuid().ToString("N")[..8];

        var regNtd = await client.PostAsJsonAsync("/api/auth/register/employer", new
        {
            tenCongTy = $"Goi Corp {rand}", diaChi = "1 Test St", email = $"goi_ntd_{rand}@test.local",
            matKhau = "Test1234", sdt = "0900000000", xacNhanMatKhau = "Test1234",
        });
        Assert.Equal(HttpStatusCode.Created, regNtd.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JobHunterDbContext>();
            var taiKhoan = await db.TaiKhoans.FirstAsync(x => x.Email == $"goi_ntd_{rand}@test.local");
            taiKhoan.DaXacThuc = true;
            await db.SaveChangesAsync();
        }

        var login = await client.PostAsJsonAsync("/api/auth/login", new { email = $"goi_ntd_{rand}@test.local", matKhau = "Test1234" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var token = (await login.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOpts))!.Token;
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Dang du 3 tin (dung dung gioi han Mien phi QD18)
        for (int i = 0; i < 3; i++)
        {
            var post = await client.PostAsJsonAsync("/api/jobs", new
            {
                tieuDe = $"Goi Job {rand} {i}", moTaCongViec = "mo ta",
                hanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                kyNangYeuCau = Array.Empty<object>(),
            });
            Assert.Equal(HttpStatusCode.Created, post.StatusCode);
        }

        // Tin thu 4 phai bi chan (QD18)
        var postThu4 = await client.PostAsJsonAsync("/api/jobs", new
        {
            tieuDe = $"Goi Job {rand} thu 4", moTaCongViec = "mo ta",
            hanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            kyNangYeuCau = Array.Empty<object>(),
        });
        Assert.Equal(HttpStatusCode.BadRequest, postThu4.StatusCode);

        // Mua goi Standard (gioi han 10) -> tin thu 4 phai dang duoc
        var danhSachGoi = await (await client.GetAsync("/api/packages")).Content.ReadFromJsonAsync<DanhSachGoiResponse>(JsonOpts);
        var goiStandard = danhSachGoi!.DanhSachGoi.First(g => g.TenGoi == "Standard");
        var mua = await client.PostAsJsonAsync($"/api/packages/{goiStandard.MaGoi}/mua", new { phuongThucThanhToan = "ChuyenKhoan", thongTinThanhToan = "STK 123456" });
        Assert.Equal(HttpStatusCode.OK, mua.StatusCode);

        var postSauKhiMua = await client.PostAsJsonAsync("/api/jobs", new
        {
            tieuDe = $"Goi Job {rand} sau khi mua", moTaCongViec = "mo ta",
            hanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            kyNangYeuCau = Array.Empty<object>(),
        });
        Assert.Equal(HttpStatusCode.Created, postSauKhiMua.StatusCode);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task XoaGoiDichVu_GoiChiCoGiaoDichDaHetHan_XoaVatLyKhongLoi500()
    {
        var client = _factory.CreateClient();
        var rand = Guid.NewGuid().ToString("N")[..8];

        var loginAdmin = await client.PostAsJsonAsync("/api/auth/login", new { email = "admin@jobhunter.local", matKhau = "Admin@123" });
        Assert.Equal(HttpStatusCode.OK, loginAdmin.StatusCode);
        var adminToken = (await loginAdmin.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOpts))!.Token;
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var them = await client.PostAsJsonAsync("/api/admin/packages", new
        {
            tenGoi = $"Goi Test Xoa {rand}", gioiHanTin = 5, coNoiBat = false, giaTien = 99000,
        });
        Assert.Equal(HttpStatusCode.Created, them.StatusCode);
        var themBody = await them.Content.ReadFromJsonAsync<JsonElement>();
        var maGoi = themBody.GetProperty("goi").GetProperty("maGoi").GetInt32();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JobHunterDbContext>();
            var ntd = await db.NhaTuyenDungs.FirstAsync();
            db.GiaoDichMuaGois.Add(new GiaoDichMuaGoi
            {
                MaTK = ntd.MaTK,
                MaGoi = maGoi,
                NgayMua = DateTime.UtcNow.AddDays(-60),
                NgayHetHan = DateTime.UtcNow.AddDays(-30),
                SoTien = 99000,
                PhuongThucThanhToan = "ChuyenKhoan",
                TrangThai = "ThanhCong",
            });
            await db.SaveChangesAsync();
        }

        var xoa = await client.DeleteAsync($"/api/admin/packages/{maGoi}");
        Assert.Equal(HttpStatusCode.OK, xoa.StatusCode);
    }
}
