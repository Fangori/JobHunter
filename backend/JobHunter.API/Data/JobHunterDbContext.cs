using JobHunter.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.API.Data;

public class JobHunterDbContext : DbContext
{
    public JobHunterDbContext(DbContextOptions<JobHunterDbContext> options) : base(options)
    {
    }

    public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
    public DbSet<UngVien> UngViens => Set<UngVien>();
    public DbSet<NhaTuyenDung> NhaTuyenDungs => Set<NhaTuyenDung>();
    public DbSet<ThamSo> ThamSos => Set<ThamSo>();
    public DbSet<DanhMucKyNang> DanhMucKyNangs => Set<DanhMucKyNang>();
    public DbSet<TinTuyenDung> TinTuyenDungs => Set<TinTuyenDung>();
    public DbSet<TinKyNang> TinKyNangs => Set<TinKyNang>();
    public DbSet<Cv> Cvs => Set<Cv>();
    public DbSet<CvKyNang> CvKyNangs => Set<CvKyNang>();
    public DbSet<CvKinhNghiem> CvKinhNghiems => Set<CvKinhNghiem>();
    public DbSet<CvHocVan> CvHocVans => Set<CvHocVan>();
    public DbSet<DonUngTuyen> DonUngTuyens => Set<DonUngTuyen>();
    public DbSet<TokenXacThuc> TokenXacThucs => Set<TokenXacThuc>();
    public DbSet<DanhMucNganhNghe> DanhMucNganhNghes => Set<DanhMucNganhNghe>();
    public DbSet<ThongBao> ThongBaos => Set<ThongBao>();
    public DbSet<TinYeuThich> TinYeuThichs => Set<TinYeuThich>();
    public DbSet<TheoDoiCongTy> TheoDoiCongTys => Set<TheoDoiCongTy>();
    public DbSet<GoiDichVu> GoiDichVus => Set<GoiDichVu>();
    public DbSet<GiaoDichMuaGoi> GiaoDichMuaGois => Set<GiaoDichMuaGoi>();

    // DbSet khac duoc them dan theo tung Phase khi entity tuong ung
    // duoc tao trong Models/, khop dung database/JobHunter_CreateTables.sql

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaiKhoan>(e =>
        {
            e.ToTable("TAI_KHOAN");
            e.HasKey(x => x.MaTK);
        });

        modelBuilder.Entity<UngVien>(e =>
        {
            e.ToTable("UNG_VIEN");
            e.HasKey(x => x.MaTK);
            e.HasOne(x => x.TaiKhoan)
                .WithOne(x => x.UngVien)
                .HasForeignKey<UngVien>(x => x.MaTK);
        });

        modelBuilder.Entity<NhaTuyenDung>(e =>
        {
            e.ToTable("NHA_TUYEN_DUNG");
            e.HasKey(x => x.MaTK);
            e.HasOne(x => x.TaiKhoan)
                .WithOne(x => x.NhaTuyenDung)
                .HasForeignKey<NhaTuyenDung>(x => x.MaTK);
            e.HasOne<DanhMucNganhNghe>()
                .WithMany()
                .HasForeignKey(x => x.MaNganhNghe);
        });

        modelBuilder.Entity<DanhMucNganhNghe>(e =>
        {
            e.ToTable("DANH_MUC_NGANH_NGHE");
            e.HasKey(x => x.MaNganhNghe);
        });

        modelBuilder.Entity<TokenXacThuc>(e =>
        {
            e.ToTable("TOKEN_XAC_THUC");
            e.HasKey(x => x.MaToken);
            e.HasOne<TaiKhoan>().WithMany().HasForeignKey(x => x.MaTK);
        });

        modelBuilder.Entity<ThamSo>(e =>
        {
            e.ToTable("THAM_SO");
            e.HasKey(x => x.MaThamSo);
        });

        modelBuilder.Entity<DanhMucKyNang>(e =>
        {
            e.ToTable("DANH_MUC_KY_NANG");
            e.HasKey(x => x.MaKyNang);
        });

        modelBuilder.Entity<TinTuyenDung>(e =>
        {
            e.ToTable("TIN_TUYEN_DUNG");
            e.HasKey(x => x.MaTin);
            e.HasOne(x => x.NhaTuyenDung)
                .WithMany()
                .HasForeignKey(x => x.MaTK);
        });

        modelBuilder.Entity<TinKyNang>(e =>
        {
            e.ToTable("TIN_KY_NANG");
            e.HasKey(x => new { x.MaTin, x.MaKyNang });
            e.HasOne(x => x.TinTuyenDung)
                .WithMany(x => x.TinKyNangs)
                .HasForeignKey(x => x.MaTin);
            e.HasOne(x => x.DanhMucKyNang)
                .WithMany()
                .HasForeignKey(x => x.MaKyNang);
        });

        modelBuilder.Entity<Cv>(e =>
        {
            e.ToTable("CV");
            e.HasKey(x => x.MaCV);
            e.HasOne(x => x.UngVien).WithMany().HasForeignKey(x => x.MaTK);
            e.HasMany(x => x.CvKinhNghiems).WithOne().HasForeignKey(x => x.MaCV);
            e.HasMany(x => x.CvHocVans).WithOne().HasForeignKey(x => x.MaCV);
        });

        modelBuilder.Entity<CvKyNang>(e =>
        {
            e.ToTable("CV_KY_NANG");
            e.HasKey(x => new { x.MaCV, x.MaKyNang });
            e.HasOne(x => x.Cv).WithMany(x => x.CvKyNangs).HasForeignKey(x => x.MaCV);
            e.HasOne(x => x.DanhMucKyNang).WithMany().HasForeignKey(x => x.MaKyNang);
        });

        modelBuilder.Entity<CvKinhNghiem>(e =>
        {
            e.ToTable("CV_KINH_NGHIEM");
            e.HasKey(x => x.MaKinhNghiem);
        });

        modelBuilder.Entity<CvHocVan>(e =>
        {
            e.ToTable("CV_HOC_VAN");
            e.HasKey(x => x.MaHocVan);
        });

        modelBuilder.Entity<DonUngTuyen>(e =>
        {
            e.ToTable("DON_UNG_TUYEN");
            e.HasKey(x => x.MaDon);
            e.HasOne(x => x.TinTuyenDung).WithMany().HasForeignKey(x => x.MaTin);
            e.HasOne(x => x.Cv).WithMany().HasForeignKey(x => x.MaCV);
        });

        modelBuilder.Entity<ThongBao>(e =>
        {
            e.ToTable("THONG_BAO");
            e.HasKey(x => x.MaThongBao);
            e.HasOne<TaiKhoan>().WithMany().HasForeignKey(x => x.MaTK);
        });

        modelBuilder.Entity<TinYeuThich>(e =>
        {
            e.ToTable("TIN_YEU_THICH");
            e.HasKey(x => new { x.MaTK, x.MaTin });
            e.HasOne<UngVien>().WithMany().HasForeignKey(x => x.MaTK);
            e.HasOne<TinTuyenDung>().WithMany().HasForeignKey(x => x.MaTin);
        });

        modelBuilder.Entity<TheoDoiCongTy>(e =>
        {
            e.ToTable("THEO_DOI_CONG_TY");
            e.HasKey(x => new { x.MaTK_UngVien, x.MaTK_NTD });
            e.HasOne<UngVien>().WithMany().HasForeignKey(x => x.MaTK_UngVien);
            e.HasOne<NhaTuyenDung>().WithMany().HasForeignKey(x => x.MaTK_NTD);
        });

        modelBuilder.Entity<GoiDichVu>(e =>
        {
            e.ToTable("GOI_DICH_VU");
            e.HasKey(x => x.MaGoi);
            e.Property(x => x.GiaTien).HasColumnType("decimal(12,2)");
        });

        modelBuilder.Entity<GiaoDichMuaGoi>(e =>
        {
            e.ToTable("GIAO_DICH_MUA_GOI");
            e.HasKey(x => x.MaGiaoDich);
            e.Property(x => x.SoTien).HasColumnType("decimal(12,2)");
            e.HasOne<NhaTuyenDung>().WithMany().HasForeignKey(x => x.MaTK);
            e.HasOne(x => x.GoiDichVu).WithMany().HasForeignKey(x => x.MaGoi);
        });
    }
}
