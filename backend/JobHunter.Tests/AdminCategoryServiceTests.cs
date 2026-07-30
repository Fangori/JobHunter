using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Models;
using JobHunter.API.Services;
using Xunit;

namespace JobHunter.Tests;

public class AdminCategoryServiceTests
{
    [Fact]
    [Trait("Category", "BR19")]
    public async Task ThemKyNang_TrungTen_ThatBai()
    {
        var db = TestHelpers.NewInMemoryDb();
        var service = new AdminCategoryService(db);
        await service.ThemKyNangAsync(new DanhMucKyNangUpsertRequest { TenKyNang = "React" });

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.ThemKyNangAsync(new DanhMucKyNangUpsertRequest { TenKyNang = "react" })); // khong phan biet hoa/thuong
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    [Trait("Category", "BR20")]
    public async Task XoaKyNang_DangDuocDung_ChiVoHieuHoa()
    {
        var db = TestHelpers.NewInMemoryDb();
        var service = new AdminCategoryService(db);
        var skill = await service.ThemKyNangAsync(new DanhMucKyNangUpsertRequest { TenKyNang = "Java" });

        var taiKhoanUv = new TaiKhoan { Email = "uv@t.local", MatKhau = "x", VaiTro = "UngVien", DaXacThuc = true, TrangThai = "HoatDong", NgayTao = DateTime.UtcNow };
        db.TaiKhoans.Add(taiKhoanUv);
        await db.SaveChangesAsync();
        var cv = new Cv { MaTK = taiKhoanUv.MaTK, TenCV = "CV1", LoaiCV = "TrucTuyen", TrangThai = "HoatDong", NgayTao = DateTime.UtcNow };
        db.Cvs.Add(cv);
        await db.SaveChangesAsync();
        db.CvKyNangs.Add(new CvKyNang { MaCV = cv.MaCV, MaKyNang = skill.MaKyNang });
        await db.SaveChangesAsync();

        var message = await service.XoaKyNangAsync(skill.MaKyNang);

        Assert.Equal("Kỹ năng đang được sử dụng, không thể xóa. Đã chuyển sang trạng thái ngừng sử dụng.", message);
        var entity = await db.DanhMucKyNangs.FindAsync(skill.MaKyNang);
        Assert.Equal("NgungSuDung", entity!.TrangThai);
    }

    [Fact]
    [Trait("Category", "BR20")]
    public async Task XoaKyNang_KhongDuocDung_XoaVatLy()
    {
        var db = TestHelpers.NewInMemoryDb();
        var service = new AdminCategoryService(db);
        var skill = await service.ThemKyNangAsync(new DanhMucKyNangUpsertRequest { TenKyNang = "Python" });

        var message = await service.XoaKyNangAsync(skill.MaKyNang);

        Assert.Equal("Xóa kỹ năng thành công.", message);
        Assert.Null(await db.DanhMucKyNangs.FindAsync(skill.MaKyNang));
    }

    [Fact]
    [Trait("Category", "BR21")]
    public async Task ThemNganhNghe_TrungTen_ThatBai()
    {
        var db = TestHelpers.NewInMemoryDb();
        var service = new AdminCategoryService(db);
        await service.ThemNganhNgheAsync(new DanhMucNganhNgheUpsertRequest { TenNganhNghe = "Công nghệ thông tin" });

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.ThemNganhNgheAsync(new DanhMucNganhNgheUpsertRequest { TenNganhNghe = "Công nghệ thông tin" }));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    [Trait("Category", "BR22")]
    public async Task XoaNganhNghe_DangDuocDung_ChiVoHieuHoa()
    {
        var db = TestHelpers.NewInMemoryDb();
        var service = new AdminCategoryService(db);
        var nganh = await service.ThemNganhNgheAsync(new DanhMucNganhNgheUpsertRequest { TenNganhNghe = "Tài chính" });

        var taiKhoanNtd = new TaiKhoan { Email = "ntd@t.local", MatKhau = "x", VaiTro = "NhaTuyenDung", DaXacThuc = true, TrangThai = "HoatDong", NgayTao = DateTime.UtcNow };
        db.TaiKhoans.Add(taiKhoanNtd);
        await db.SaveChangesAsync();
        db.NhaTuyenDungs.Add(new NhaTuyenDung { MaTK = taiKhoanNtd.MaTK, TenCongTy = "Co", MaNganhNghe = nganh.MaNganhNghe, SoTinDangTuyen = 0 });
        await db.SaveChangesAsync();

        var message = await service.XoaNganhNgheAsync(nganh.MaNganhNghe);

        Assert.Equal("Ngành nghề đang được sử dụng, không thể xóa. Đã chuyển sang trạng thái ngừng sử dụng.", message);
        var entity = await db.DanhMucNganhNghes.FindAsync(nganh.MaNganhNghe);
        Assert.Equal("NgungSuDung", entity!.TrangThai);
    }
}
