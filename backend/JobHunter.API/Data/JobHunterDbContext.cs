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
        });

        modelBuilder.Entity<ThamSo>(e =>
        {
            e.ToTable("THAM_SO");
            e.HasKey(x => x.MaThamSo);
        });
    }
}
