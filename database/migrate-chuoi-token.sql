-- Them cot ChuoiToken (chuoi ngau nhien an toan, base64url 32 byte) vao
-- TOKEN_XAC_THUC. Truoc do link xac thuc/dat lai mat khau chi dung thang
-- MaToken (INT IDENTITY tu tang) lam token trong URL - chap nhan duoc khi
-- con mock ra console (khong ai thay ma doan), nhung gui EMAIL THAT thi
-- day la lo hong chiem tai khoan nghiem trong (thu tuan tu 1,2,3... la
-- xac thuc/dat lai mat khau duoc BAT KY tai khoan nao). Sua khi lam SMTP
-- that (12/08/2026).
--
-- Ap truc tiep len DB dang chay, KHONG drop/reseed. Idempotent.

USE JobHunterDB;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('TOKEN_XAC_THUC') AND name = 'ChuoiToken')
BEGIN
    ALTER TABLE TOKEN_XAC_THUC ADD ChuoiToken NVARCHAR(64) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('TOKEN_XAC_THUC') AND name = 'UQ_Token_ChuoiToken')
BEGIN
    CREATE UNIQUE INDEX UQ_Token_ChuoiToken ON TOKEN_XAC_THUC(ChuoiToken) WHERE ChuoiToken IS NOT NULL;
END
GO
