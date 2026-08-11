using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Models;
using JobHunter.API.Services;
using Xunit;

namespace JobHunter.Tests;

public class JobServiceTests
{
    private static async Task<int> TaoNtdAsync(JobHunter.API.Data.JobHunterDbContext db, string email = "ntd@test.local")
    {
        var taiKhoan = new TaiKhoan
        {
            Email = email, MatKhau = "x", VaiTro = "NhaTuyenDung",
            DaXacThuc = true, TrangThai = "HoatDong", NgayTao = DateTime.UtcNow,
        };
        db.TaiKhoans.Add(taiKhoan);
        await db.SaveChangesAsync();
        db.NhaTuyenDungs.Add(new NhaTuyenDung { MaTK = taiKhoan.MaTK, TenCongTy = "Test Co " + email, SoTinDangTuyen = 0 });
        await db.SaveChangesAsync();
        return taiKhoan.MaTK;
    }

    private static async Task<(JobService service, int maTkNtd)> NewServiceWithNtdAsync()
    {
        var (service, maTkNtd, _) = await NewServiceWithNtdAndDbAsync();
        return (service, maTkNtd);
    }

    private static async Task<(JobService service, int maTkNtd, JobHunter.API.Data.JobHunterDbContext db)> NewServiceWithNtdAndDbAsync()
    {
        var db = TestHelpers.NewInMemoryDb();
        var maTkNtd = await TaoNtdAsync(db);
        var service = new JobService(db, new ThamSoService(db), new NotificationService(db), new GoiDichVuService(db));
        return (service, maTkNtd, db);
    }

    [Fact]
    [Trait("Category", "QD09")]
    public async Task DangTin_HanNopKhongDuTS7Ngay_ThatBai()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var request = new DangTinRequest
        {
            TieuDe = "Test",
            MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow), // = hom nay, khong du TS7=1 ngay
        };

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.DangTinAsync(maTkNtd, request));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    [Trait("Category", "QD09")]
    public async Task DangTin_HanNopHopLe_ThanhCong()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var request = new DangTinRequest
        {
            TieuDe = "Test",
            MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        };

        var result = await service.DangTinAsync(maTkNtd, request);
        Assert.Equal("ChoDuyet", result.TrangThai);
    }

    [Fact]
    [Trait("Category", "BR16")]
    public async Task TuChoiTin_KhongCoLyDo_ThatBai()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.TuChoiTinAsync(job.MaTin, ""));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    [Trait("Category", "BR16")]
    public async Task TuChoiTin_CoLyDo_ThanhCongVaLuuLyDo()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });

        await service.TuChoiTinAsync(job.MaTin, "Khong phu hop tieu chuan dang tin");

        var chiTiet = await service.XemChiTietAsync(job.MaTin);
        Assert.Equal("TuChoi", chiTiet.TrangThai);
    }

    [Fact]
    [Trait("Category", "BR15")]
    public async Task SuaTin_DaDuyet_TuDongVeChoDuyet()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });
        await service.DuyetTinAsync(job.MaTin);

        var result = await service.SuaTinAsync(maTkNtd, job.MaTin, new DangTinRequest
        {
            TieuDe = "Test Da Sua", MoTaCongViec = "mo ta moi",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
        });

        Assert.Equal("ChoDuyet", result.Tin.TrangThai);
        Assert.Equal("Cập nhật tin thành công. Tin sẽ được duyệt lại trước khi hiển thị công khai.", result.Message);
    }

    [Fact]
    [Trait("Category", "BR15")]
    public async Task SuaTin_DangChoDuyet_KhongDoiTrangThaiVaThongBaoKhacMS41()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });

        var result = await service.SuaTinAsync(maTkNtd, job.MaTin, new DangTinRequest
        {
            TieuDe = "Test Da Sua", MoTaCongViec = "mo ta moi",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
        });

        Assert.Equal("ChoDuyet", result.Tin.TrangThai);
        Assert.Equal("Cập nhật tin thành công.", result.Message);
    }

    [Fact]
    [Trait("Category", "BR24")]
    public async Task GiaHan_HanMoiKhongHopLe_ThatBai()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });
        await service.DuyetTinAsync(job.MaTin);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.GiaHanAsync(maTkNtd, job.MaTin, DateOnly.FromDateTime(DateTime.UtcNow)));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    [Trait("Category", "BR24")]
    public async Task GiaHan_HanMoiHopLe_ThanhCong()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });
        await service.DuyetTinAsync(job.MaTin);

        var hanMoi = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
        var result = await service.GiaHanAsync(maTkNtd, job.MaTin, hanMoi);
        Assert.Equal(hanMoi, result.HanNopHoSo);
    }

    [Fact]
    [Trait("Category", "UC27")]
    public async Task DongTin_DangDaDuyet_ThanhCong()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });
        await service.DuyetTinAsync(job.MaTin);

        var result = await service.DongTinAsync(maTkNtd, job.MaTin);
        Assert.Equal("DaDong", result.TrangThai);
    }

    [Fact]
    [Trait("Category", "UC27")]
    public async Task DongTin_ChuaDuocDuyet_ThatBai()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.DongTinAsync(maTkNtd, job.MaTin));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    [Trait("Category", "BR17")]
    public async Task GoTin_KhongCoLyDo_ThatBai()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });
        await service.DuyetTinAsync(job.MaTin);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.GoTinAsync(job.MaTin, ""));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    [Trait("Category", "UC35")]
    public async Task GoTin_CoLyDo_ThanhCongVaLuuLyDo()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });
        await service.DuyetTinAsync(job.MaTin);

        await service.GoTinAsync(job.MaTin, "Vi pham chinh sach");

        var chiTiet = await service.XemChiTietAsync(job.MaTin);
        Assert.Equal("DaGo", chiTiet.TrangThai);
    }

    [Fact]
    [Trait("Category", "UC36")]
    public async Task PhucHoiTinDaGo_ThanhCong()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });
        await service.DuyetTinAsync(job.MaTin);
        await service.GoTinAsync(job.MaTin, "Vi pham chinh sach");

        await service.PhucHoiTinDaGoAsync(job.MaTin);

        var chiTiet = await service.XemChiTietAsync(job.MaTin);
        Assert.Equal("DaDuyet", chiTiet.TrangThai);
    }

    [Fact]
    [Trait("Category", "QD18")]
    public async Task DangTin_VuotGioiHanMienPhi3Tin_ThatBai()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        for (int i = 0; i < 3; i++)
        {
            await service.DangTinAsync(maTkNtd, new DangTinRequest
            {
                TieuDe = $"Job {i}", MoTaCongViec = "mo ta",
                HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            });
        }

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Job thu 4", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        }));
        Assert.Equal(400, ex.StatusCode);
        Assert.Equal("Bạn đã đạt giới hạn số tin đăng tuyển đồng thời của gói dịch vụ hiện tại. Vui lòng mua thêm gói dịch vụ để đăng thêm tin.", ex.Message);
    }

    [Fact]
    [Trait("Category", "QD18")]
    public async Task DangTin_TinBiTuChoiKhongTinhVaoGioiHan_VanDangDuocTiepTuc()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        for (int i = 0; i < 3; i++)
        {
            var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
            {
                TieuDe = $"Job {i}", MoTaCongViec = "mo ta",
                HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            });
            if (i == 0)
                await service.TuChoiTinAsync(job.MaTin, "Ly do tu choi test");
        }

        // Tin 0 da bi TuChoi (khong tinh vao gioi han) -> van con "cho" de dang tin thu 4
        var job4 = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Job thu 4", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });
        Assert.Equal("ChoDuyet", job4.TrangThai);
    }

    // UC10 (LAB4) - loc nang cao: khoang luong, loai hinh, sap xep, phan trang

    [Fact]
    [Trait("Category", "UC10")]
    public async Task DangTin_LuongToiThieuLonHonToiDa_ThatBai()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var request = new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            LuongToiThieu = 20, LuongToiDa = 10,
        };

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.DangTinAsync(maTkNtd, request));
        Assert.Equal(400, ex.StatusCode);
    }

    [Theory]
    [Trait("Category", "UC10")]
    [InlineData(15, 20, "15-20 triệu")]
    [InlineData(15, null, "Từ 15 triệu")]
    [InlineData(null, 20, "Đến 20 triệu")]
    [InlineData(null, null, "Thỏa thuận")]
    public async Task DangTin_SinhChuoiMucLuongTuKhoangSo(int? min, int? max, string mucLuongMongDoi)
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            LuongToiThieu = min, LuongToiDa = max,
        });

        var chiTiet = await service.XemChiTietAsync(job.MaTin);
        Assert.Equal(mucLuongMongDoi, chiTiet.MucLuong);
        Assert.Equal(min, chiTiet.LuongToiThieu);
        Assert.Equal(max, chiTiet.LuongToiDa);
    }

    private static async Task TaoVaDuyetJobAsync(JobService service, int maTkNtd, string tieuDe, string? hinhThuc, int? luongMin, int? luongMax)
    {
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = tieuDe, MoTaCongViec = "mo ta", HinhThucLamViec = hinhThuc,
            LuongToiThieu = luongMin, LuongToiDa = luongMax,
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });
        await service.DuyetTinAsync(job.MaTin);
    }

    [Fact]
    [Trait("Category", "UC10")]
    public async Task TimKiemVaLoc_LocTheoLoaiHinhLamViec_ChiTraDungLoaiDaChon()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        await TaoVaDuyetJobAsync(service, maTkNtd, "Full-time job", "FullTime", null, null);
        await TaoVaDuyetJobAsync(service, maTkNtd, "Remote job", "Remote", null, null);

        var result = await service.TimKiemVaLocAsync(new TimKiemVaLocRequest { HinhThucLamViec = new List<string> { "Remote" } });

        Assert.Single(result.Items);
        Assert.Equal("Remote job", result.Items[0].TieuDe);
    }

    [Fact]
    [Trait("Category", "UC10")]
    public async Task TimKiemVaLoc_LocTheoKhoangLuong_ChiTraTinCoLuongKhopKhoang()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        await TaoVaDuyetJobAsync(service, maTkNtd, "Luong thap", null, 5, 8);
        await TaoVaDuyetJobAsync(service, maTkNtd, "Luong trung binh", null, 15, 20);
        await TaoVaDuyetJobAsync(service, maTkNtd, "Thoa thuan", null, null, null); // khong khop khoang loc nao

        var result = await service.TimKiemVaLocAsync(new TimKiemVaLocRequest { LuongMin = 10, LuongMax = 25 });

        Assert.Single(result.Items);
        Assert.Equal("Luong trung binh", result.Items[0].TieuDe);
    }

    [Fact]
    [Trait("Category", "UC10")]
    public async Task TimKiemVaLoc_LocTheoKhoangLuong_TinLuongMoMotPhiaVanKhopKhoangLoc()
    {
        // Bug that gap 12/08: tin chi nhap "Tu 15 trieu" (LuongToiDa=NULL)
        // hoac "Den 8 trieu" (LuongToiThieu=NULL) bi loai oan khoi MOI khoang
        // loc vi dieu kien cu doi hoi ca 2 dau phai co gia tri. NULL o 1 dau
        // nghia la "khong gioi han phia do", khong phai "khong khop".
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        await TaoVaDuyetJobAsync(service, maTkNtd, "Tu 15 trieu (mo tren)", null, 15, null);
        await TaoVaDuyetJobAsync(service, maTkNtd, "Den 8 trieu (mo duoi)", null, null, 8);
        await TaoVaDuyetJobAsync(service, maTkNtd, "Thoa thuan (khong loc duoc)", null, null, null);

        var ketQua10Den20 = await service.TimKiemVaLocAsync(new TimKiemVaLocRequest { LuongMin = 10, LuongMax = 20 });
        Assert.Single(ketQua10Den20.Items);
        Assert.Equal("Tu 15 trieu (mo tren)", ketQua10Den20.Items[0].TieuDe);

        var ketQuaDuoi10 = await service.TimKiemVaLocAsync(new TimKiemVaLocRequest { LuongMax = 10 });
        Assert.Single(ketQuaDuoi10.Items);
        Assert.Equal("Den 8 trieu (mo duoi)", ketQuaDuoi10.Items[0].TieuDe);
    }

    [Fact]
    [Trait("Category", "UC10")]
    public async Task TimKiemVaLoc_SapXepLuongGiamDan()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        await TaoVaDuyetJobAsync(service, maTkNtd, "Luong thap", null, 5, 8);
        await TaoVaDuyetJobAsync(service, maTkNtd, "Luong cao", null, 30, 40);

        var result = await service.TimKiemVaLocAsync(new TimKiemVaLocRequest { SortBy = "luong_giam" });

        Assert.Equal("Luong cao", result.Items[0].TieuDe);
        Assert.Equal("Luong thap", result.Items[1].TieuDe);
    }

    [Fact]
    [Trait("Category", "UC10")]
    public async Task TimKiemVaLoc_PhanTrang_TraDungSoLuongVaTongSo()
    {
        // 5 tin DaDuyet vuot gioi han goi Mien phi (3 tin dong thoi/1 NTD -
        // QD18) neu dung chung 1 NTD, nen moi tin tao qua 1 NTD rieng, chi
        // de kiem tra phan trang cua ket qua tim kiem cong khai (khong lien
        // quan QD18).
        var (service, maTkNtd, db) = await NewServiceWithNtdAndDbAsync();
        await TaoVaDuyetJobAsync(service, maTkNtd, "Job 0", null, null, null);
        for (int i = 1; i < 5; i++)
        {
            var maTkNtdKhac = await TaoNtdAsync(db, $"ntd{i}@test.local");
            await TaoVaDuyetJobAsync(service, maTkNtdKhac, $"Job {i}", null, null, null);
        }

        var result = await service.TimKiemVaLocAsync(new TimKiemVaLocRequest { Page = 1, PageSize = 2 });

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
    }
}
