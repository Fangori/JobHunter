-- One-off demo-data script: them 7 cong ty moi (1 cong ty/1 nganh nghe con
-- lai chua co du lieu) + 1 tin da duyet moi cong ty, de bo loc "Kham pha
-- theo nganh nghe" o trang chu co ket qua that cho ca 8 nganh thay vi chi
-- "Cong nghe thong tin" (5 cong ty seed truoc chi thuoc 1 nganh nay).
-- KHONG phai mot phan cua seed goc - chi chay 1 lan. Neu chay lai se loi
-- do UNIQUE email - vay la an toan (khong tao trung).
--
-- Mat khau tat ca tai khoan demo trong file nay: Test@1234
-- (dung lai bcrypt hash da xac nhan tuong thich BCrypt.Net-Next).

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
DECLARE @hash NVARCHAR(255) = N'$2a$11$vYTeFhlGGpfPl6aPiELM1eqPelmlqJ3rxovEFJtDUYdVdex82b5JO';

DECLARE @nnTaiChinh INT = (SELECT MaNganhNghe FROM DANH_MUC_NGANH_NGHE WHERE TenNganhNghe = N'Tài chính - Ngân hàng');
DECLARE @nnTMDT INT = (SELECT MaNganhNghe FROM DANH_MUC_NGANH_NGHE WHERE TenNganhNghe = N'Thương mại điện tử');
DECLARE @nnGiaoDuc INT = (SELECT MaNganhNghe FROM DANH_MUC_NGANH_NGHE WHERE TenNganhNghe = N'Giáo dục');
DECLARE @nnYTe INT = (SELECT MaNganhNghe FROM DANH_MUC_NGANH_NGHE WHERE TenNganhNghe = N'Y tế');
DECLARE @nnSanXuat INT = (SELECT MaNganhNghe FROM DANH_MUC_NGANH_NGHE WHERE TenNganhNghe = N'Sản xuất');
DECLARE @nnBanLe INT = (SELECT MaNganhNghe FROM DANH_MUC_NGANH_NGHE WHERE TenNganhNghe = N'Bán lẻ');
DECLARE @nnMarketing INT = (SELECT MaNganhNghe FROM DANH_MUC_NGANH_NGHE WHERE TenNganhNghe = N'Marketing');

DECLARE @ntd INT;

-- Tai chinh - Ngan hang -----------------------------------------------
INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao)
VALUES (N'demo-ntd4@jobhunter.local', @hash, 'NhaTuyenDung', 1, 'HoatDong', 0, '2026-07-20T09:00:00');
SET @ntd = SCOPE_IDENTITY();
INSERT INTO NHA_TUYEN_DUNG (MaTK, TenCongTy, MaNganhNghe, DiaChi, QuyMo, SoTinDangTuyen)
VALUES (@ntd, N'Ngân hàng TMCP Đông Á Số', @nnTaiChinh, N'Quận 1, TP.HCM', '>500', 1);
INSERT INTO TIN_TUYEN_DUNG (MaTK, TieuDe, MoTaCongViec, YeuCauUngVien, QuyenLoi, MucLuong, DiaDiem, HinhThucLamViec, SoNamKinhNghiemYeuCau, NgayDang, HanNopHoSo, TrangThai)
VALUES (@ntd, N'Chuyên viên Tín dụng Doanh nghiệp', N'Thẩm định hồ sơ vay, tư vấn giải pháp tài chính cho khách hàng doanh nghiệp.', N'Tốt nghiệp Tài chính/Ngân hàng, có kinh nghiệm tín dụng.', N'Thưởng KPI, bảo hiểm sức khỏe cao cấp.', N'18-25 triệu', N'Quận 1, TP.HCM', 'FullTime', 2, '2026-07-25T09:00:00', '2026-10-31', 'DaDuyet');

-- Thuong mai dien tu -----------------------------------------------
INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao)
VALUES (N'demo-ntd5@jobhunter.local', @hash, 'NhaTuyenDung', 1, 'HoatDong', 0, '2026-07-22T09:00:00');
SET @ntd = SCOPE_IDENTITY();
INSERT INTO NHA_TUYEN_DUNG (MaTK, TenCongTy, MaNganhNghe, DiaChi, QuyMo, SoTinDangTuyen)
VALUES (@ntd, N'Công ty CP Thương mại Điện tử VietMart', @nnTMDT, N'Quận 7, TP.HCM', '50-200', 1);
INSERT INTO TIN_TUYEN_DUNG (MaTK, TieuDe, MoTaCongViec, YeuCauUngVien, QuyenLoi, MucLuong, DiaDiem, HinhThucLamViec, SoNamKinhNghiemYeuCau, NgayDang, HanNopHoSo, TrangThai)
VALUES (@ntd, N'Nhân viên Vận hành Sàn TMĐT', N'Quản lý gian hàng, xử lý đơn hàng và chăm sóc khách hàng trên các sàn Shopee/Lazada.', N'Thành thạo tin học văn phòng, nhanh nhẹn.', N'Thưởng doanh số, đào tạo nội bộ.', N'10-14 triệu', N'Quận 7, TP.HCM', 'FullTime', 0, '2026-07-28T09:00:00', '2026-10-31', 'DaDuyet');

-- Giao duc -----------------------------------------------
INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao)
VALUES (N'demo-ntd6@jobhunter.local', @hash, 'NhaTuyenDung', 1, 'HoatDong', 0, '2026-07-24T09:00:00');
SET @ntd = SCOPE_IDENTITY();
INSERT INTO NHA_TUYEN_DUNG (MaTK, TenCongTy, MaNganhNghe, DiaChi, QuyMo, SoTinDangTuyen)
VALUES (@ntd, N'Trung tâm Anh ngữ Học Viện Toàn Cầu', @nnGiaoDuc, N'Quận Phú Nhuận, TP.HCM', '<50', 1);
INSERT INTO TIN_TUYEN_DUNG (MaTK, TieuDe, MoTaCongViec, YeuCauUngVien, QuyenLoi, MucLuong, DiaDiem, HinhThucLamViec, SoNamKinhNghiemYeuCau, NgayDang, HanNopHoSo, TrangThai)
VALUES (@ntd, N'Giáo viên Tiếng Anh', N'Giảng dạy tiếng Anh giao tiếp cho học viên độ tuổi 10-18.', N'IELTS 7.0+ hoặc tương đương, yêu thích giảng dạy.', N'Lương theo giờ, thưởng học viên đạt mục tiêu.', N'150.000đ/giờ', N'Quận Phú Nhuận, TP.HCM', 'PartTime', 1, '2026-07-30T09:00:00', '2026-10-31', 'DaDuyet');

-- Y te -----------------------------------------------
INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao)
VALUES (N'demo-ntd7@jobhunter.local', @hash, 'NhaTuyenDung', 1, 'HoatDong', 0, '2026-07-26T09:00:00');
SET @ntd = SCOPE_IDENTITY();
INSERT INTO NHA_TUYEN_DUNG (MaTK, TenCongTy, MaNganhNghe, DiaChi, QuyMo, SoTinDangTuyen)
VALUES (@ntd, N'Phòng khám Đa khoa An Tâm', @nnYTe, N'Quận 10, TP.HCM', '50-200', 1);
INSERT INTO TIN_TUYEN_DUNG (MaTK, TieuDe, MoTaCongViec, YeuCauUngVien, QuyenLoi, MucLuong, DiaDiem, HinhThucLamViec, SoNamKinhNghiemYeuCau, NgayDang, HanNopHoSo, TrangThai)
VALUES (@ntd, N'Điều dưỡng viên', N'Chăm sóc và theo dõi tình trạng bệnh nhân theo chỉ định bác sĩ.', N'Tốt nghiệp Cao đẳng/Đại học Điều dưỡng, có chứng chỉ hành nghề.', N'Bảo hiểm đầy đủ, trực thưởng.', N'9-13 triệu', N'Quận 10, TP.HCM', 'FullTime', 1, '2026-08-01T09:00:00', '2026-10-31', 'DaDuyet');

-- San xuat -----------------------------------------------
INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao)
VALUES (N'demo-ntd8@jobhunter.local', @hash, 'NhaTuyenDung', 1, 'HoatDong', 0, '2026-07-28T09:00:00');
SET @ntd = SCOPE_IDENTITY();
INSERT INTO NHA_TUYEN_DUNG (MaTK, TenCongTy, MaNganhNghe, DiaChi, QuyMo, SoTinDangTuyen)
VALUES (@ntd, N'Công ty TNHH Sản xuất Cơ khí Phương Nam', @nnSanXuat, N'KCN Tân Bình, TP.HCM', '200-500', 1);
INSERT INTO TIN_TUYEN_DUNG (MaTK, TieuDe, MoTaCongViec, YeuCauUngVien, QuyenLoi, MucLuong, DiaDiem, HinhThucLamViec, SoNamKinhNghiemYeuCau, NgayDang, HanNopHoSo, TrangThai)
VALUES (@ntd, N'Kỹ sư Cơ khí', N'Thiết kế, giám sát vận hành và bảo trì dây chuyền sản xuất.', N'Tốt nghiệp Cơ khí/Cơ điện tử, đọc hiểu bản vẽ kỹ thuật.', N'Phụ cấp ăn ở, xe đưa đón.', N'13-18 triệu', N'KCN Tân Bình, TP.HCM', 'FullTime', 2, '2026-08-02T09:00:00', '2026-10-31', 'DaDuyet');

-- Ban le -----------------------------------------------
INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao)
VALUES (N'demo-ntd9@jobhunter.local', @hash, 'NhaTuyenDung', 1, 'HoatDong', 0, '2026-07-30T09:00:00');
SET @ntd = SCOPE_IDENTITY();
INSERT INTO NHA_TUYEN_DUNG (MaTK, TenCongTy, MaNganhNghe, DiaChi, QuyMo, SoTinDangTuyen)
VALUES (@ntd, N'Chuỗi Siêu thị Mini Bốn Mùa', @nnBanLe, N'Quận Gò Vấp, TP.HCM', '200-500', 1);
INSERT INTO TIN_TUYEN_DUNG (MaTK, TieuDe, MoTaCongViec, YeuCauUngVien, QuyenLoi, MucLuong, DiaDiem, HinhThucLamViec, SoNamKinhNghiemYeuCau, NgayDang, HanNopHoSo, TrangThai)
VALUES (@ntd, N'Quản lý Cửa hàng', N'Điều phối nhân sự, quản lý hàng hóa và doanh thu cửa hàng.', N'Kinh nghiệm quản lý bán lẻ tối thiểu 1 năm.', N'Thưởng doanh số, tăng lương định kỳ.', N'10-15 triệu', N'Quận Gò Vấp, TP.HCM', 'FullTime', 1, '2026-08-04T09:00:00', '2026-10-31', 'DaDuyet');

-- Marketing -----------------------------------------------
INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao)
VALUES (N'demo-ntd10@jobhunter.local', @hash, 'NhaTuyenDung', 1, 'HoatDong', 0, '2026-08-01T09:00:00');
SET @ntd = SCOPE_IDENTITY();
INSERT INTO NHA_TUYEN_DUNG (MaTK, TenCongTy, MaNganhNghe, DiaChi, QuyMo, SoTinDangTuyen)
VALUES (@ntd, N'Công ty TNHH Truyền thông Sáng Tạo Việt', @nnMarketing, N'Quận 3, TP.HCM', '<50', 1);
INSERT INTO TIN_TUYEN_DUNG (MaTK, TieuDe, MoTaCongViec, YeuCauUngVien, QuyenLoi, MucLuong, DiaDiem, HinhThucLamViec, SoNamKinhNghiemYeuCau, NgayDang, HanNopHoSo, TrangThai)
VALUES (@ntd, N'Chuyên viên Marketing Digital', N'Lên kế hoạch và triển khai chiến dịch quảng cáo trên Facebook/TikTok/Google Ads.', N'Có kinh nghiệm chạy quảng cáo, tư duy sáng tạo.', N'Thưởng theo hiệu quả chiến dịch.', N'12-17 triệu', N'Quận 3, TP.HCM', 'Remote', 1, '2026-08-06T09:00:00', '2026-10-31', 'DaDuyet');
