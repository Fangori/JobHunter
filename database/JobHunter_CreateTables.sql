-- ====================================================================
-- JOBHUNTER - CREATE DATABASE + 17 TABLES (SQL SERVER)
-- Nguon: JobHunter_Context_ChoClaudeCode.txt, muc 5 (Lab 3 schema)
-- Thu tu tao bang tuan theo FK cha->con. Chay thang trong SSMS.
-- ====================================================================

IF DB_ID('JobHunterDB') IS NULL
    CREATE DATABASE JobHunterDB;
GO

USE JobHunterDB;
GO

-- --------------------------------------------------------------------
-- 1. TAI_KHOAN
-- --------------------------------------------------------------------
CREATE TABLE TAI_KHOAN (
    MaTK                INT IDENTITY(1,1) PRIMARY KEY,
    Email               NVARCHAR(100) NOT NULL UNIQUE,
    MatKhau             NVARCHAR(255) NOT NULL,          -- toi thieu 8 ky tu, co it nhat 1 chu so (QD01/TS1) -> validate o Service layer
    VaiTro              NVARCHAR(20) NOT NULL
                            CHECK (VaiTro IN ('UngVien','NhaTuyenDung','Admin')),
    DaXacThuc           BIT NOT NULL DEFAULT 0,
    TrangThai           NVARCHAR(20) NOT NULL DEFAULT 'HoatDong'
                            CHECK (TrangThai IN ('HoatDong','BiKhoa')),
    SoLanDangNhapSai    INT NOT NULL DEFAULT 0,           -- QD03/TS2
    KhoaTamThoiDenLuc   DATETIME2 NULL,                   -- QD03/TS3, NULL neu khong bi khoa tam
    LyDoKhoa            NVARCHAR(255) NULL,                -- bat buoc khi TrangThai=BiKhoa (BR18)
    NgayTao             DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT CK_TaiKhoan_LyDoKhoa CHECK (TrangThai <> 'BiKhoa' OR LyDoKhoa IS NOT NULL)
);
GO

-- --------------------------------------------------------------------
-- 2. THAM_SO
-- --------------------------------------------------------------------
CREATE TABLE THAM_SO (
    MaThamSo    NVARCHAR(10) PRIMARY KEY,        -- VD: TS1, TS2...
    GiaTri      NVARCHAR(50) NOT NULL,
    GhiChu      NVARCHAR(255) NULL
);
GO

-- --------------------------------------------------------------------
-- 3. DANH_MUC_KY_NANG
-- --------------------------------------------------------------------
CREATE TABLE DANH_MUC_KY_NANG (
    MaKyNang    INT IDENTITY(1,1) PRIMARY KEY,
    TenKyNang   NVARCHAR(50) NOT NULL UNIQUE,     -- QD12/BR19: khong trung lap
    NhomNganh   NVARCHAR(50) NULL,
    TrangThai   NVARCHAR(20) NOT NULL DEFAULT 'HoatDong'
                    CHECK (TrangThai IN ('HoatDong','NgungSuDung'))  -- BR20
);
GO

-- --------------------------------------------------------------------
-- 4. DANH_MUC_NGANH_NGHE
-- --------------------------------------------------------------------
CREATE TABLE DANH_MUC_NGANH_NGHE (
    MaNganhNghe     INT IDENTITY(1,1) PRIMARY KEY,
    TenNganhNghe    NVARCHAR(100) NOT NULL UNIQUE,   -- BR21
    TrangThai       NVARCHAR(20) NOT NULL DEFAULT 'HoatDong'
                        CHECK (TrangThai IN ('HoatDong','NgungSuDung'))  -- BR22
);
GO

-- --------------------------------------------------------------------
-- 5. TOKEN_XAC_THUC
-- --------------------------------------------------------------------
CREATE TABLE TOKEN_XAC_THUC (
    MaToken         INT IDENTITY(1,1) PRIMARY KEY,
    MaTK            INT NOT NULL,
    LoaiToken       NVARCHAR(20) NOT NULL
                        CHECK (LoaiToken IN ('XacThucEmail','DatLaiMatKhau')),
    ThoiHanHetHan   DATETIME2 NOT NULL,   -- 15 phut ke tu luc tao (QD04/TS4)
    DaSuDung        BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Token_TaiKhoan FOREIGN KEY (MaTK) REFERENCES TAI_KHOAN(MaTK)
);
GO

-- --------------------------------------------------------------------
-- 6. THONG_BAO
-- --------------------------------------------------------------------
CREATE TABLE THONG_BAO (
    MaThongBao      INT IDENTITY(1,1) PRIMARY KEY,
    MaTK            INT NOT NULL,
    NoiDung         NVARCHAR(255) NOT NULL,
    LoaiThongBao    NVARCHAR(30) NOT NULL,   -- TrangThaiDon / TinDaDuyet / TinBiTuChoi / TinBiGo...
    DaDoc           BIT NOT NULL DEFAULT 0,
    NgayTao         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    LienKet         NVARCHAR(255) NULL,
    CONSTRAINT FK_ThongBao_TaiKhoan FOREIGN KEY (MaTK) REFERENCES TAI_KHOAN(MaTK)
);
GO

-- --------------------------------------------------------------------
-- 7. UNG_VIEN (1-1 voi TAI_KHOAN)
-- --------------------------------------------------------------------
CREATE TABLE UNG_VIEN (
    MaTK                INT PRIMARY KEY,
    HoTen               NVARCHAR(100) NOT NULL,
    NgaySinh            DATE NULL,
    SDT                 NVARCHAR(15) NULL,
    DiaChi              NVARCHAR(255) NULL,
    AnhDaiDien          NVARCHAR(255) NULL,      -- URL Cloudinary
    GioiThieuBanThan    NVARCHAR(MAX) NULL,
    SoCV                INT NOT NULL DEFAULT 0,   -- cot toi uu toc do
    CONSTRAINT FK_UngVien_TaiKhoan FOREIGN KEY (MaTK) REFERENCES TAI_KHOAN(MaTK)
);
GO

-- --------------------------------------------------------------------
-- 8. NHA_TUYEN_DUNG (1-1 voi TAI_KHOAN)
-- --------------------------------------------------------------------
CREATE TABLE NHA_TUYEN_DUNG (
    MaTK                INT PRIMARY KEY,
    TenCongTy           NVARCHAR(150) NOT NULL,
    MaSoThue            NVARCHAR(15) NULL,             -- 10 hoac 13 chu so; UNIQUE khai bao rieng ben duoi
    Logo                NVARCHAR(255) NULL,           -- URL Cloudinary
    AnhBia              NVARCHAR(255) NULL,            -- URL Cloudinary (giong co che cua Logo) - NULL = chua upload, dung banner mac dinh
    MaNganhNghe         INT NULL,
    DiaChi              NVARCHAR(255) NULL,
    Website             NVARCHAR(150) NULL,
    GioiThieuCongTy     NVARCHAR(MAX) NULL,
    SDT                 NVARCHAR(15) NULL,
    QuyMo               NVARCHAR(20) NULL
                            CHECK (QuyMo IN ('<50','50-200','200-500','>500')),
    SoTinDangTuyen      INT NOT NULL DEFAULT 0,        -- cot toi uu toc do
    CONSTRAINT FK_NTD_TaiKhoan FOREIGN KEY (MaTK) REFERENCES TAI_KHOAN(MaTK),
    CONSTRAINT FK_NTD_NganhNghe FOREIGN KEY (MaNganhNghe) REFERENCES DANH_MUC_NGANH_NGHE(MaNganhNghe)
);
GO

-- MaSoThue de trong (NULL) o hau het NTD vi BM02 (dang ky) KHONG thu
-- thap truong nay - chi dien sau o UC08 (ngoai pham vi hom nay). SQL
-- Server, KHAC voi nhieu RDBMS khac, chi cho phep DUY NHAT 1 dong NULL
-- trong 1 cot co UNIQUE constraint thuong -> phai dung filtered unique
-- index (WHERE MaSoThue IS NOT NULL) de cho phep nhieu NTD cung de
-- trong MaSoThue, chi ep duy nhat khi DA co gia tri.
SET QUOTED_IDENTIFIER ON;
CREATE UNIQUE INDEX UQ_NhaTuyenDung_MaSoThue ON NHA_TUYEN_DUNG(MaSoThue) WHERE MaSoThue IS NOT NULL;
GO

-- --------------------------------------------------------------------
-- 9. CV
-- --------------------------------------------------------------------
CREATE TABLE CV (
    MaCV                INT IDENTITY(1,1) PRIMARY KEY,
    MaTK                INT NOT NULL,
    TenCV               NVARCHAR(100) NOT NULL,
    LoaiCV              NVARCHAR(20) NOT NULL
                            CHECK (LoaiCV IN ('TrucTuyen','Upload')),
    DuongDanFile        NVARCHAR(255) NULL,     -- chi co gia tri khi LoaiCV=Upload; URL Cloudinary (QD07/TS5,TS6)
    TrinhDoHocVan       NVARCHAR(30) NULL
                            CHECK (TrinhDoHocVan IN ('TrungCap','CaoDang','DaiHoc','SauDaiHoc')),
    ViTriMongMuon       NVARCHAR(100) NULL,
    MucLuongMongMuon    NVARCHAR(50) NULL,
    TrangThai           NVARCHAR(20) NOT NULL DEFAULT 'HoatDong'
                            CHECK (TrangThai IN ('HoatDong','DaAn')),   -- BR13: xoa logic
    NgayTao             DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_CV_UngVien FOREIGN KEY (MaTK) REFERENCES UNG_VIEN(MaTK),
    CONSTRAINT CK_CV_DuongDanFile CHECK (LoaiCV <> 'Upload' OR DuongDanFile IS NOT NULL)
);
GO

-- --------------------------------------------------------------------
-- 10. CV_KINH_NGHIEM
-- --------------------------------------------------------------------
CREATE TABLE CV_KINH_NGHIEM (
    MaKinhNghiem    INT IDENTITY(1,1) PRIMARY KEY,
    MaCV            INT NOT NULL,
    CongTy          NVARCHAR(150) NOT NULL,
    ViTri           NVARCHAR(100) NULL,
    TuNgay          DATE NOT NULL,
    DenNgay         DATE NULL,        -- NULL = dang lam viec tai day
    MoTaCongViec    NVARCHAR(MAX) NULL,
    CONSTRAINT FK_KinhNghiem_CV FOREIGN KEY (MaCV) REFERENCES CV(MaCV)
);
GO

-- --------------------------------------------------------------------
-- 11. CV_HOC_VAN
-- --------------------------------------------------------------------
CREATE TABLE CV_HOC_VAN (
    MaHocVan        INT IDENTITY(1,1) PRIMARY KEY,
    MaCV            INT NOT NULL,
    Truong          NVARCHAR(150) NOT NULL,
    ChuyenNganh     NVARCHAR(100) NULL,
    TuNam           INT NULL,
    DenNam          INT NULL,
    CONSTRAINT FK_HocVan_CV FOREIGN KEY (MaCV) REFERENCES CV(MaCV)
);
GO

-- --------------------------------------------------------------------
-- 12. CV_KY_NANG (association class: CV <-> DANH_MUC_KY_NANG)
-- --------------------------------------------------------------------
CREATE TABLE CV_KY_NANG (
    MaCV                INT NOT NULL,
    MaKyNang            INT NOT NULL,
    MucDoThanhThao      NVARCHAR(20) NULL
                            CHECK (MucDoThanhThao IN ('CoBan','Kha','ThanhThao')),
    CONSTRAINT PK_CV_KyNang PRIMARY KEY (MaCV, MaKyNang),
    CONSTRAINT FK_CVKyNang_CV FOREIGN KEY (MaCV) REFERENCES CV(MaCV),
    CONSTRAINT FK_CVKyNang_KyNang FOREIGN KEY (MaKyNang) REFERENCES DANH_MUC_KY_NANG(MaKyNang)
);
GO

-- --------------------------------------------------------------------
-- 13. TIN_TUYEN_DUNG
-- --------------------------------------------------------------------
CREATE TABLE TIN_TUYEN_DUNG (
    MaTin                       INT IDENTITY(1,1) PRIMARY KEY,
    MaTK                        INT NOT NULL,
    TieuDe                      NVARCHAR(150) NOT NULL,
    MoTaCongViec                NVARCHAR(MAX) NOT NULL,
    YeuCauUngVien               NVARCHAR(MAX) NULL,
    QuyenLoi                    NVARCHAR(MAX) NULL,
    MucLuong                    NVARCHAR(50) NULL,   -- chuoi hien thi, Service tu sinh tu LuongToiThieu/LuongToiDa
    LuongToiThieu                INT NULL,             -- trieu VND, NULL = khong nhap (Thoa thuan)
    LuongToiDa                   INT NULL,             -- trieu VND
    SoLuongTuyen                 INT NULL,             -- so luong ung vien can tuyen
    DiaDiem                     NVARCHAR(150) NULL,
    HinhThucLamViec             NVARCHAR(30) NULL
                                    CHECK (HinhThucLamViec IN ('FullTime','PartTime','Remote')),
    SoNamKinhNghiemYeuCau       INT NULL,
    NgayDang                    DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    HanNopHoSo                  DATE NOT NULL,     -- phai sau NgayDang toi thieu 1 ngay (QD09/TS7)
    TrangThai                   NVARCHAR(20) NOT NULL DEFAULT 'ChoDuyet'
                                    CHECK (TrangThai IN ('ChoDuyet','DaDuyet','TuChoi','DaGo','DaDong')),
    LyDoTuChoi                  NVARCHAR(255) NULL,   -- bat buoc khi TrangThai=TuChoi (BR16)
    LyDoGo                      NVARCHAR(255) NULL,   -- bat buoc khi TrangThai=DaGo (BR17)
    SoDonUngTuyen                INT NOT NULL DEFAULT 0,   -- cot toi uu toc do
    CONSTRAINT FK_Tin_NTD FOREIGN KEY (MaTK) REFERENCES NHA_TUYEN_DUNG(MaTK),
    CONSTRAINT CK_Tin_HanNop CHECK (HanNopHoSo > CAST(NgayDang AS DATE)),
    CONSTRAINT CK_Tin_LyDoTuChoi CHECK (TrangThai <> 'TuChoi' OR LyDoTuChoi IS NOT NULL),
    CONSTRAINT CK_Tin_LyDoGo CHECK (TrangThai <> 'DaGo' OR LyDoGo IS NOT NULL)
);
GO

-- --------------------------------------------------------------------
-- 14. TIN_KY_NANG (association class: TIN_TUYEN_DUNG <-> DANH_MUC_KY_NANG)
-- --------------------------------------------------------------------
CREATE TABLE TIN_KY_NANG (
    MaTin           INT NOT NULL,
    MaKyNang        INT NOT NULL,
    MucDoUuTien     NVARCHAR(20) NULL
                        CHECK (MucDoUuTien IN ('BatBuoc','UuTien')),
    CONSTRAINT PK_Tin_KyNang PRIMARY KEY (MaTin, MaKyNang),
    CONSTRAINT FK_TinKyNang_Tin FOREIGN KEY (MaTin) REFERENCES TIN_TUYEN_DUNG(MaTin),
    CONSTRAINT FK_TinKyNang_KyNang FOREIGN KEY (MaKyNang) REFERENCES DANH_MUC_KY_NANG(MaKyNang)
);
GO

-- --------------------------------------------------------------------
-- 15. DON_UNG_TUYEN
-- NOTE: QD10/TS8 (toi da 1 don DANG HOAT DONG cho cung 1 tin CUA 1 UNG
-- VIEN) khong the ep bang UNIQUE constraint thuan vi "ung vien" duoc
-- suy ra qua CV.MaTK chu khong nam truc tiep tren bang nay -> PHAI
-- kiem tra o Service layer truoc khi INSERT (xem CLAUDE.md).
-- --------------------------------------------------------------------
CREATE TABLE DON_UNG_TUYEN (
    MaDon           INT IDENTITY(1,1) PRIMARY KEY,
    MaTin           INT NOT NULL,
    MaCV            INT NOT NULL,
    ThuGioiThieu    NVARCHAR(MAX) NULL,
    NgayNop         DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    TrangThai       NVARCHAR(20) NOT NULL DEFAULT 'DaNop'
                        CHECK (TrangThai IN ('DaNop','DangXemXet','PhongVan','TuChoi','Nhan','DaHuy')),  -- QD11
    GhiChuNoiBo     NVARCHAR(MAX) NULL,
    DaXem           BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Don_Tin FOREIGN KEY (MaTin) REFERENCES TIN_TUYEN_DUNG(MaTin),
    CONSTRAINT FK_Don_CV FOREIGN KEY (MaCV) REFERENCES CV(MaCV)
);
GO

-- --------------------------------------------------------------------
-- 16. TIN_YEU_THICH (association class: UNG_VIEN <-> TIN_TUYEN_DUNG)
-- --------------------------------------------------------------------
CREATE TABLE TIN_YEU_THICH (
    MaTK        INT NOT NULL,
    MaTin       INT NOT NULL,
    NgayLuu     DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT PK_TinYeuThich PRIMARY KEY (MaTK, MaTin),
    CONSTRAINT FK_YeuThich_UngVien FOREIGN KEY (MaTK) REFERENCES UNG_VIEN(MaTK),
    CONSTRAINT FK_YeuThich_Tin FOREIGN KEY (MaTin) REFERENCES TIN_TUYEN_DUNG(MaTin)
);
GO

-- --------------------------------------------------------------------
-- 17. THEO_DOI_CONG_TY (association class: UNG_VIEN <-> NHA_TUYEN_DUNG)
-- --------------------------------------------------------------------
CREATE TABLE THEO_DOI_CONG_TY (
    MaTK_UngVien    INT NOT NULL,
    MaTK_NTD        INT NOT NULL,
    NgayTheoDoi     DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT PK_TheoDoiCongTy PRIMARY KEY (MaTK_UngVien, MaTK_NTD),
    CONSTRAINT FK_TheoDoi_UngVien FOREIGN KEY (MaTK_UngVien) REFERENCES UNG_VIEN(MaTK),
    CONSTRAINT FK_TheoDoi_NTD FOREIGN KEY (MaTK_NTD) REFERENCES NHA_TUYEN_DUNG(MaTK)
);
GO

-- --------------------------------------------------------------------
-- 18. GOI_DICH_VU
-- --------------------------------------------------------------------
CREATE TABLE GOI_DICH_VU (
    MaGoi           INT IDENTITY(1,1) PRIMARY KEY,
    TenGoi          NVARCHAR(50) NOT NULL UNIQUE,        -- BR26: khong trung lap
    GioiHanTin      INT NOT NULL CHECK (GioiHanTin > 0),  -- QD18
    CoNoiBat        BIT NOT NULL DEFAULT 0,
    GiaTien         DECIMAL(12,2) NOT NULL CHECK (GiaTien >= 0),
    ThoiHan         INT NOT NULL DEFAULT 30 CHECK (ThoiHan > 0),
    TrangThai       NVARCHAR(20) NOT NULL DEFAULT 'DangBan'
                        CHECK (TrangThai IN ('DangBan','NgungBan'))  -- BR27
);
GO

-- --------------------------------------------------------------------
-- 19. GIAO_DICH_MUA_GOI
-- --------------------------------------------------------------------
CREATE TABLE GIAO_DICH_MUA_GOI (
    MaGiaoDich              INT IDENTITY(1,1) PRIMARY KEY,
    MaTK                    INT NOT NULL,             -- FK -> NHA_TUYEN_DUNG
    MaGoi                   INT NOT NULL,             -- FK -> GOI_DICH_VU
    NgayMua                 DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    NgayHetHan              DATETIME2 NOT NULL,        -- = NgayMua + ThoiHan goi (BR25)
    SoTien                  DECIMAL(12,2) NOT NULL,    -- = GiaTien goi tai thoi diem mua
    PhuongThucThanhToan     NVARCHAR(20) NOT NULL
                                CHECK (PhuongThucThanhToan IN ('TheNganHang','ChuyenKhoan')),
    TrangThai               NVARCHAR(20) NOT NULL DEFAULT 'ThanhCong'
                                CHECK (TrangThai IN ('ThanhCong','ThatBai')),
    CONSTRAINT FK_GiaoDich_NTD FOREIGN KEY (MaTK) REFERENCES NHA_TUYEN_DUNG(MaTK),
    CONSTRAINT FK_GiaoDich_Goi FOREIGN KEY (MaGoi) REFERENCES GOI_DICH_VU(MaGoi)
);
GO
