-- Them cot AnhBia (URL Cloudinary, giong het co che cua cot Logo da co san)
-- vao NHA_TUYEN_DUNG - cho phep NTD tu UPLOAD anh bia rieng (UC08) thay vi
-- luon bi gan co dinh theo MaTK.
--
-- NULL = chua upload, tiep tuc dung mac dinh xoay vong theo MaTK nhu code
-- cu (tuong thich nguoc, khong can backfill du lieu cu).
--
-- Ap truc tiep len DB dang chay, KHONG drop/reseed. Idempotent.

USE JobHunterDB;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('NHA_TUYEN_DUNG') AND name = 'AnhBia')
BEGIN
    ALTER TABLE NHA_TUYEN_DUNG ADD AnhBia NVARCHAR(255) NULL;
END
ELSE
BEGIN
    -- Da tung tao voi NVARCHAR(30) (ban dau dinh chi luu 1 key nho, sau doi
    -- huong sang upload URL that) - mo rong lai cho vua URL Cloudinary.
    ALTER TABLE NHA_TUYEN_DUNG ALTER COLUMN AnhBia NVARCHAR(255) NULL;
END
GO
