-- Them 3 cot moi vao TIN_TUYEN_DUNG: SoLuongTuyen, LuongToiThieu, LuongToiDa
-- (don vi trieu VND, giong quy uoc hien co cua MucLuong dang text "18-25
-- trieu"). Muc dich: form "Dang tin" nhap so thay vi text tu do, va co du
-- lieu SO de loc/sap xep theo muc luong o trang tim kiem (gop y bao cao
-- 11-12/08 - "khoang Luong toi thieu/toi da" + bo loc "Chon muc luong").
--
-- MucLuong (cot text cu) VAN GIU NGUYEN, khong xoa - Service tu dong sinh
-- lai chuoi hien thi nay tu LuongToiThieu/LuongToiDa moi khi tao/sua tin,
-- de moi noi dang doc MucLuong (JobCard, JobDetail, seed data cu...) khong
-- phai sua gi ca.
--
-- Ap truc tiep len DB dang chay, KHONG drop/reseed. Idempotent - chay lai
-- nhieu lan khong loi (kiem tra cot da ton tai truoc khi ALTER).

USE JobHunterDB;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TIN_TUYEN_DUNG') AND name = 'SoLuongTuyen')
BEGIN
    ALTER TABLE TIN_TUYEN_DUNG ADD SoLuongTuyen INT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TIN_TUYEN_DUNG') AND name = 'LuongToiThieu')
BEGIN
    ALTER TABLE TIN_TUYEN_DUNG ADD LuongToiThieu INT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TIN_TUYEN_DUNG') AND name = 'LuongToiDa')
BEGIN
    ALTER TABLE TIN_TUYEN_DUNG ADD LuongToiDa INT NULL;
END
GO

-- Backfill du lieu demo/seed da co san: MucLuong dang dung 100% dinh dang
-- "<min>-<max> trieu" (vd "18-25 trieu") nen parse duoc bang string. Tin
-- nao khong khop dinh dang (vd "Thoa thuan") thi TRY_CAST tra NULL, giu
-- nguyen khong loi.
UPDATE TIN_TUYEN_DUNG
SET
    LuongToiThieu = TRY_CAST(LEFT(MucLuong, CHARINDEX('-', MucLuong) - 1) AS INT),
    LuongToiDa = TRY_CAST(
        SUBSTRING(
            MucLuong,
            CHARINDEX('-', MucLuong) + 1,
            CHARINDEX(' ', MucLuong, CHARINDEX('-', MucLuong)) - CHARINDEX('-', MucLuong) - 1
        ) AS INT
    ),
    SoLuongTuyen = ISNULL(SoLuongTuyen, 1)
WHERE MucLuong LIKE '%-%tri_u%' AND LuongToiThieu IS NULL;
GO

UPDATE TIN_TUYEN_DUNG SET SoLuongTuyen = 1 WHERE SoLuongTuyen IS NULL;
GO

SELECT MaTin, MucLuong, LuongToiThieu, LuongToiDa, SoLuongTuyen FROM TIN_TUYEN_DUNG;
GO
