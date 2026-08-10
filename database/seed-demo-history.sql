-- One-off demo-data script: bom them du lieu lich su (Thang 2-7/2026) de
-- bieu do "Xu huong 6 thang gan nhat" o AdminReports.jsx co duong xu
-- huong that thay vi phang li roi vot len o thang hien tai. KHONG phai
-- mot phan cua seed goc (JobHunter_SeedData.sql) - chi chay 1 lan, sau
-- khi da co JobHunter_SeedData.sql. Neu chay lai se loi do UNIQUE email -
-- vay la an toan (khong tao trung).
--
-- Mat khau tat ca tai khoan demo trong file nay: Test@1234
-- (dung lai bcrypt hash $2a$ da xac nhan tuong thich BCrypt.Net-Next
-- tu dot seed candidate demo truoc do trong session).

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
DECLARE @hash NVARCHAR(255) = N'$2a$11$vYTeFhlGGpfPl6aPiELM1eqPelmlqJ3rxovEFJtDUYdVdex82b5JO';
DECLARE @maNganhCNTT INT = (SELECT MaNganhNghe FROM DANH_MUC_NGANH_NGHE WHERE TenNganhNghe = N'Công nghệ thông tin');

-- ====================================================================
-- 4 tai khoan NTD lich su (ntd0 truoc cua so 6 thang de co the dang tin
-- thang 3, ntd1/ntd2/ntd3 moi vao dung thang de tinh "TK NTD moi")
-- ====================================================================
DECLARE @ntd0 INT, @ntd1 INT, @ntd2 INT, @ntd3 INT;

INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao)
VALUES (N'demo-ntd0@jobhunter.local', @hash, 'NhaTuyenDung', 1, 'HoatDong', 0, '2026-02-20T09:00:00');
SET @ntd0 = SCOPE_IDENTITY();
INSERT INTO NHA_TUYEN_DUNG (MaTK, TenCongTy, MaNganhNghe, DiaChi, QuyMo, SoTinDangTuyen)
VALUES (@ntd0, N'Công ty TNHH Alpha Tech', @maNganhCNTT, N'Quận 1, TP.HCM', '50-200', 3);

INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao)
VALUES (N'demo-ntd1@jobhunter.local', @hash, 'NhaTuyenDung', 1, 'HoatDong', 0, '2026-04-12T09:00:00');
SET @ntd1 = SCOPE_IDENTITY();
INSERT INTO NHA_TUYEN_DUNG (MaTK, TenCongTy, MaNganhNghe, DiaChi, QuyMo, SoTinDangTuyen)
VALUES (@ntd1, N'Công ty TNHH ABC Solutions', @maNganhCNTT, N'Quận 3, TP.HCM', '<50', 1);

INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao)
VALUES (N'demo-ntd2@jobhunter.local', @hash, 'NhaTuyenDung', 1, 'HoatDong', 0, '2026-06-08T09:00:00');
SET @ntd2 = SCOPE_IDENTITY();
INSERT INTO NHA_TUYEN_DUNG (MaTK, TenCongTy, MaNganhNghe, DiaChi, QuyMo, SoTinDangTuyen)
VALUES (@ntd2, N'Công ty Cổ phần XYZ Tech', @maNganhCNTT, N'Quận Bình Thạnh, TP.HCM', '200-500', 1);

INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao)
VALUES (N'demo-ntd3@jobhunter.local', @hash, 'NhaTuyenDung', 1, 'HoatDong', 0, '2026-07-15T09:00:00');
SET @ntd3 = SCOPE_IDENTITY();
INSERT INTO NHA_TUYEN_DUNG (MaTK, TenCongTy, MaNganhNghe, DiaChi, QuyMo, SoTinDangTuyen)
VALUES (@ntd3, N'Công ty TNHH Green Software', @maNganhCNTT, N'Quận 7, TP.HCM', '<50', 2);

-- ====================================================================
-- 9 tai khoan Ung vien lich su (1 CV moi nguoi) - Thang 3(1) 4(1) 5(2)
-- 6(2) 7(3), tang dan de bieu do co xu huong di len
-- ====================================================================
DECLARE @uv1 INT, @uv2 INT, @uv3 INT, @uv4 INT, @uv5 INT, @uv6 INT, @uv7 INT, @uv8 INT, @uv9 INT;
DECLARE @cv1 INT, @cv2 INT, @cv3 INT, @cv4 INT, @cv5 INT, @cv6 INT, @cv7 INT, @cv8 INT, @cv9 INT;

INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao)
VALUES (N'demo-uv1@jobhunter.local', @hash, 'UngVien', 1, 'HoatDong', 0, '2026-03-10T10:00:00');
SET @uv1 = SCOPE_IDENTITY();
INSERT INTO UNG_VIEN (MaTK, HoTen, SoCV) VALUES (@uv1, N'Trần Thị Mai', 1);
INSERT INTO CV (MaTK, TenCV, LoaiCV, TrinhDoHocVan, ViTriMongMuon, TrangThai, NgayTao)
VALUES (@uv1, N'CV xin việc - Trần Thị Mai', 'TrucTuyen', 'DaiHoc', N'Frontend Developer', 'HoatDong', '2026-03-10T10:30:00');
SET @cv1 = SCOPE_IDENTITY();

INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao)
VALUES (N'demo-uv2@jobhunter.local', @hash, 'UngVien', 1, 'HoatDong', 0, '2026-04-08T10:00:00');
SET @uv2 = SCOPE_IDENTITY();
INSERT INTO UNG_VIEN (MaTK, HoTen, SoCV) VALUES (@uv2, N'Nguyễn Văn Bình', 1);
INSERT INTO CV (MaTK, TenCV, LoaiCV, TrinhDoHocVan, ViTriMongMuon, TrangThai, NgayTao)
VALUES (@uv2, N'CV xin việc - Nguyễn Văn Bình', 'TrucTuyen', 'DaiHoc', N'Backend Developer', 'HoatDong', '2026-04-08T10:30:00');
SET @cv2 = SCOPE_IDENTITY();

INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao)
VALUES (N'demo-uv3@jobhunter.local', @hash, 'UngVien', 1, 'HoatDong', 0, '2026-05-05T10:00:00');
SET @uv3 = SCOPE_IDENTITY();
INSERT INTO UNG_VIEN (MaTK, HoTen, SoCV) VALUES (@uv3, N'Lê Thị Hồng', 1);
INSERT INTO CV (MaTK, TenCV, LoaiCV, TrinhDoHocVan, ViTriMongMuon, TrangThai, NgayTao)
VALUES (@uv3, N'CV xin việc - Lê Thị Hồng', 'TrucTuyen', 'CaoDang', N'Tester', 'HoatDong', '2026-05-05T10:30:00');
SET @cv3 = SCOPE_IDENTITY();

INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao)
VALUES (N'demo-uv4@jobhunter.local', @hash, 'UngVien', 1, 'HoatDong', 0, '2026-05-20T10:00:00');
SET @uv4 = SCOPE_IDENTITY();
INSERT INTO UNG_VIEN (MaTK, HoTen, SoCV) VALUES (@uv4, N'Phạm Văn Đức', 1);
INSERT INTO CV (MaTK, TenCV, LoaiCV, TrinhDoHocVan, ViTriMongMuon, TrangThai, NgayTao)
VALUES (@uv4, N'CV xin việc - Phạm Văn Đức', 'TrucTuyen', 'DaiHoc', N'DevOps Engineer', 'HoatDong', '2026-05-20T10:30:00');
SET @cv4 = SCOPE_IDENTITY();

INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao)
VALUES (N'demo-uv5@jobhunter.local', @hash, 'UngVien', 1, 'HoatDong', 0, '2026-06-03T10:00:00');
SET @uv5 = SCOPE_IDENTITY();
INSERT INTO UNG_VIEN (MaTK, HoTen, SoCV) VALUES (@uv5, N'Hoàng Thị Lan', 1);
INSERT INTO CV (MaTK, TenCV, LoaiCV, TrinhDoHocVan, ViTriMongMuon, TrangThai, NgayTao)
VALUES (@uv5, N'CV xin việc - Hoàng Thị Lan', 'TrucTuyen', 'DaiHoc', N'UI/UX Designer', 'HoatDong', '2026-06-03T10:30:00');
SET @cv5 = SCOPE_IDENTITY();

INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao)
VALUES (N'demo-uv6@jobhunter.local', @hash, 'UngVien', 1, 'HoatDong', 0, '2026-06-18T10:00:00');
SET @uv6 = SCOPE_IDENTITY();
INSERT INTO UNG_VIEN (MaTK, HoTen, SoCV) VALUES (@uv6, N'Vũ Văn Nam', 1);
INSERT INTO CV (MaTK, TenCV, LoaiCV, TrinhDoHocVan, ViTriMongMuon, TrangThai, NgayTao)
VALUES (@uv6, N'CV xin việc - Vũ Văn Nam', 'TrucTuyen', 'DaiHoc', N'Fullstack Developer', 'HoatDong', '2026-06-18T10:30:00');
SET @cv6 = SCOPE_IDENTITY();

INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao)
VALUES (N'demo-uv7@jobhunter.local', @hash, 'UngVien', 1, 'HoatDong', 0, '2026-07-02T10:00:00');
SET @uv7 = SCOPE_IDENTITY();
INSERT INTO UNG_VIEN (MaTK, HoTen, SoCV) VALUES (@uv7, N'Đặng Thị Thu', 1);
INSERT INTO CV (MaTK, TenCV, LoaiCV, TrinhDoHocVan, ViTriMongMuon, TrangThai, NgayTao)
VALUES (@uv7, N'CV xin việc - Đặng Thị Thu', 'TrucTuyen', 'SauDaiHoc', N'Data Analyst', 'HoatDong', '2026-07-02T10:30:00');
SET @cv7 = SCOPE_IDENTITY();

INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao)
VALUES (N'demo-uv8@jobhunter.local', @hash, 'UngVien', 1, 'HoatDong', 0, '2026-07-12T10:00:00');
SET @uv8 = SCOPE_IDENTITY();
INSERT INTO UNG_VIEN (MaTK, HoTen, SoCV) VALUES (@uv8, N'Bùi Văn Hải', 1);
INSERT INTO CV (MaTK, TenCV, LoaiCV, TrinhDoHocVan, ViTriMongMuon, TrangThai, NgayTao)
VALUES (@uv8, N'CV xin việc - Bùi Văn Hải', 'TrucTuyen', 'DaiHoc', N'Mobile Developer', 'HoatDong', '2026-07-12T10:30:00');
SET @cv8 = SCOPE_IDENTITY();

INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao)
VALUES (N'demo-uv9@jobhunter.local', @hash, 'UngVien', 1, 'HoatDong', 0, '2026-07-25T10:00:00');
SET @uv9 = SCOPE_IDENTITY();
INSERT INTO UNG_VIEN (MaTK, HoTen, SoCV) VALUES (@uv9, N'Ngô Thị Yến', 1);
INSERT INTO CV (MaTK, TenCV, LoaiCV, TrinhDoHocVan, ViTriMongMuon, TrangThai, NgayTao)
VALUES (@uv9, N'CV xin việc - Ngô Thị Yến', 'TrucTuyen', 'DaiHoc', N'Business Analyst', 'HoatDong', '2026-07-25T10:30:00');
SET @cv9 = SCOPE_IDENTITY();

-- ====================================================================
-- 7 tin tuyen dung lich su - Thang 3(1) 4(1) 5(1) 6(2) 7(2), da duyet
-- ====================================================================
DECLARE @tin1 INT, @tin2 INT, @tin3 INT, @tin4 INT, @tin5 INT, @tin6 INT, @tin7 INT;

INSERT INTO TIN_TUYEN_DUNG (MaTK, TieuDe, MoTaCongViec, HinhThucLamViec, NgayDang, HanNopHoSo, TrangThai)
VALUES (@ntd0, N'Frontend Developer (ReactJS)', N'Phát triển giao diện web bằng ReactJS.', 'FullTime', '2026-03-01T09:00:00', '2026-09-01', 'DaDuyet');
SET @tin1 = SCOPE_IDENTITY();

INSERT INTO TIN_TUYEN_DUNG (MaTK, TieuDe, MoTaCongViec, HinhThucLamViec, NgayDang, HanNopHoSo, TrangThai)
VALUES (@ntd0, N'Backend Developer (.NET)', N'Xây dựng API bằng ASP.NET Core.', 'FullTime', '2026-04-05T09:00:00', '2026-09-05', 'DaDuyet');
SET @tin2 = SCOPE_IDENTITY();

INSERT INTO TIN_TUYEN_DUNG (MaTK, TieuDe, MoTaCongViec, HinhThucLamViec, NgayDang, HanNopHoSo, TrangThai)
VALUES (@ntd0, N'QA/Tester', N'Kiểm thử phần mềm, viết test case.', 'FullTime', '2026-05-10T09:00:00', '2026-09-10', 'DaDuyet');
SET @tin3 = SCOPE_IDENTITY();

INSERT INTO TIN_TUYEN_DUNG (MaTK, TieuDe, MoTaCongViec, HinhThucLamViec, NgayDang, HanNopHoSo, TrangThai)
VALUES (@ntd1, N'DevOps Engineer', N'Vận hành hạ tầng CI/CD, Docker, Kubernetes.', 'FullTime', '2026-06-01T09:00:00', '2026-09-15', 'DaDuyet');
SET @tin4 = SCOPE_IDENTITY();

INSERT INTO TIN_TUYEN_DUNG (MaTK, TieuDe, MoTaCongViec, HinhThucLamViec, NgayDang, HanNopHoSo, TrangThai)
VALUES (@ntd2, N'UI/UX Designer', N'Thiết kế giao diện và trải nghiệm người dùng.', 'FullTime', '2026-06-20T09:00:00', '2026-09-20', 'DaDuyet');
SET @tin5 = SCOPE_IDENTITY();

INSERT INTO TIN_TUYEN_DUNG (MaTK, TieuDe, MoTaCongViec, HinhThucLamViec, NgayDang, HanNopHoSo, TrangThai)
VALUES (@ntd3, N'Data Analyst', N'Phân tích dữ liệu kinh doanh, làm báo cáo.', 'FullTime', '2026-07-08T09:00:00', '2026-10-08', 'DaDuyet');
SET @tin6 = SCOPE_IDENTITY();

INSERT INTO TIN_TUYEN_DUNG (MaTK, TieuDe, MoTaCongViec, HinhThucLamViec, NgayDang, HanNopHoSo, TrangThai)
VALUES (@ntd3, N'Mobile Developer (Flutter)', N'Phát triển ứng dụng di động đa nền tảng.', 'FullTime', '2026-07-22T09:00:00', '2026-10-22', 'DaDuyet');
SET @tin7 = SCOPE_IDENTITY();

-- ====================================================================
-- 15 don ung tuyen lich su - Thang 3(1) 4(2) 5(3) 6(4) 7(5)
-- ====================================================================
INSERT INTO DON_UNG_TUYEN (MaTin, MaCV, NgayNop) VALUES (@tin1, @cv1, '2026-03-28T14:00:00');

INSERT INTO DON_UNG_TUYEN (MaTin, MaCV, NgayNop) VALUES (@tin1, @cv2, '2026-04-05T14:00:00');
INSERT INTO DON_UNG_TUYEN (MaTin, MaCV, NgayNop) VALUES (@tin2, @cv1, '2026-04-20T14:00:00');

INSERT INTO DON_UNG_TUYEN (MaTin, MaCV, NgayNop) VALUES (@tin2, @cv3, '2026-05-08T14:00:00');
INSERT INTO DON_UNG_TUYEN (MaTin, MaCV, NgayNop) VALUES (@tin3, @cv4, '2026-05-15T14:00:00');
INSERT INTO DON_UNG_TUYEN (MaTin, MaCV, NgayNop) VALUES (@tin1, @cv3, '2026-05-25T14:00:00');

INSERT INTO DON_UNG_TUYEN (MaTin, MaCV, NgayNop) VALUES (@tin4, @cv5, '2026-06-05T14:00:00');
INSERT INTO DON_UNG_TUYEN (MaTin, MaCV, NgayNop) VALUES (@tin3, @cv6, '2026-06-12T14:00:00');
INSERT INTO DON_UNG_TUYEN (MaTin, MaCV, NgayNop) VALUES (@tin2, @cv5, '2026-06-20T14:00:00');
INSERT INTO DON_UNG_TUYEN (MaTin, MaCV, NgayNop) VALUES (@tin4, @cv6, '2026-06-27T14:00:00');

INSERT INTO DON_UNG_TUYEN (MaTin, MaCV, NgayNop) VALUES (@tin6, @cv7, '2026-07-09T14:00:00');
INSERT INTO DON_UNG_TUYEN (MaTin, MaCV, NgayNop) VALUES (@tin7, @cv8, '2026-07-14T14:00:00');
INSERT INTO DON_UNG_TUYEN (MaTin, MaCV, NgayNop) VALUES (@tin5, @cv9, '2026-07-18T14:00:00');
INSERT INTO DON_UNG_TUYEN (MaTin, MaCV, NgayNop) VALUES (@tin6, @cv8, '2026-07-24T14:00:00');
INSERT INTO DON_UNG_TUYEN (MaTin, MaCV, NgayNop) VALUES (@tin7, @cv9, '2026-07-29T14:00:00');

-- Cap nhat lai cot dem denormalized (SoDonUngTuyen) cho khop du lieu vua them
UPDATE TIN_TUYEN_DUNG SET SoDonUngTuyen = (SELECT COUNT(*) FROM DON_UNG_TUYEN WHERE MaTin = TIN_TUYEN_DUNG.MaTin)
WHERE MaTin IN (@tin1, @tin2, @tin3, @tin4, @tin5, @tin6, @tin7);

GO
SELECT N'Da them:' AS KetQua,
  (SELECT COUNT(*) FROM TAI_KHOAN WHERE Email LIKE N'demo-uv%') AS SoUngVien,
  (SELECT COUNT(*) FROM TAI_KHOAN WHERE Email LIKE N'demo-ntd%') AS SoNTD,
  (SELECT COUNT(*) FROM TIN_TUYEN_DUNG WHERE TieuDe IN (N'Frontend Developer (ReactJS)', N'Backend Developer (.NET)', N'QA/Tester', N'DevOps Engineer', N'UI/UX Designer', N'Data Analyst', N'Mobile Developer (Flutter)')) AS SoTin,
  (SELECT COUNT(*) FROM DON_UNG_TUYEN WHERE NgayNop < '2026-08-01') AS SoDon;
GO
