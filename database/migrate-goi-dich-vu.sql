-- Ap truc tiep 2 bang moi (GOI_DICH_VU, GIAO_DICH_MUA_GOI) + seed len DB
-- dang chay, KHONG drop/reseed toan bo de giu nguyen du lieu demo/QA da
-- co. Noi dung CREATE TABLE/INSERT giong het JobHunter_CreateTables.sql
-- va JobHunter_SeedData.sql (2 file do la nguon su that cho lan reseed
-- day du sau nay).

CREATE TABLE GOI_DICH_VU (
    MaGoi           INT IDENTITY(1,1) PRIMARY KEY,
    TenGoi          NVARCHAR(50) NOT NULL UNIQUE,
    GioiHanTin      INT NOT NULL CHECK (GioiHanTin > 0),
    CoNoiBat        BIT NOT NULL DEFAULT 0,
    GiaTien         DECIMAL(12,2) NOT NULL CHECK (GiaTien >= 0),
    ThoiHan         INT NOT NULL DEFAULT 30 CHECK (ThoiHan > 0),
    TrangThai       NVARCHAR(20) NOT NULL DEFAULT 'DangBan'
                        CHECK (TrangThai IN ('DangBan','NgungBan'))
);
GO

CREATE TABLE GIAO_DICH_MUA_GOI (
    MaGiaoDich              INT IDENTITY(1,1) PRIMARY KEY,
    MaTK                    INT NOT NULL,
    MaGoi                   INT NOT NULL,
    NgayMua                 DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    NgayHetHan              DATETIME2 NOT NULL,
    SoTien                  DECIMAL(12,2) NOT NULL,
    PhuongThucThanhToan     NVARCHAR(20) NOT NULL
                                CHECK (PhuongThucThanhToan IN ('TheNganHang','ChuyenKhoan')),
    TrangThai               NVARCHAR(20) NOT NULL DEFAULT 'ThanhCong'
                                CHECK (TrangThai IN ('ThanhCong','ThatBai')),
    CONSTRAINT FK_GiaoDich_NTD FOREIGN KEY (MaTK) REFERENCES NHA_TUYEN_DUNG(MaTK),
    CONSTRAINT FK_GiaoDich_Goi FOREIGN KEY (MaGoi) REFERENCES GOI_DICH_VU(MaGoi)
);
GO

INSERT INTO GOI_DICH_VU (TenGoi, GioiHanTin, CoNoiBat, GiaTien, ThoiHan, TrangThai) VALUES
(N'Standard', 10, 0, 299000, 30, 'DangBan'),
(N'Gold', 20, 1, 599000, 30, 'DangBan');
GO

SELECT MaGoi, TenGoi, GioiHanTin, CoNoiBat, GiaTien, ThoiHan, TrangThai FROM GOI_DICH_VU;
GO
