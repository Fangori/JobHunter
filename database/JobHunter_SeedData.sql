-- ====================================================================
-- JOBHUNTER - SEED DATA: THAM_SO (TS1-TS8)
-- Bat buoc chay sau CreateTables truoc khi test bat ky nghiep vu nao
-- co lien quan validation, vi Service layer doc gia tri tu bang nay
-- thay vi hardcode.
-- ====================================================================
USE JobHunterDB;
GO

INSERT INTO THAM_SO (MaThamSo, GiaTri, GhiChu) VALUES
('TS1', '8',  N'Do dai mat khau toi thieu (ky tu)'),
('TS2', '5',  N'So lan dang nhap sai toi da truoc khi khoa tam thoi'),
('TS3', '15', N'Thoi gian khoa tam thoi tai khoan (phut)'),
('TS4', '15', N'Thoi gian hieu luc lien ket dat lai MK / xac thuc email (phut)'),
('TS5', '10', N'Dung luong file CV toi da (MB)'),
('TS6', '3',  N'So dinh dang file CV duoc chap nhan (.pdf/.doc/.docx)'),
('TS7', '1',  N'So ngay toi thieu tu luc dang tin den han nop ho so'),
('TS8', '1',  N'So don ung tuyen toi da CUA 1 UNG VIEN cho 1 tin (dang hoat dong)');
GO

-- ====================================================================
-- SEED: Tai khoan Admin dau tien (43 UC khong co UC dang ky Admin,
-- phai co san tu dau). Hash BCrypt cua "Admin@123" (cost=11).
-- ====================================================================
INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao) VALUES
(N'admin@jobhunter.local', '$2a$11$Q.MmKNxfSUCOzm0PXVakDOJ05OjB3D/vFDHPpW81wczTVMIOCwDGW', 'Admin', 1, 'HoatDong', 0, SYSDATETIME());
GO

-- ====================================================================
-- SEED: DANH_MUC_KY_NANG (huong tech stack, dung de dang tin/tao CV/
-- loc ung vien demo hom nay)
-- ====================================================================
INSERT INTO DANH_MUC_KY_NANG (TenKyNang, NhomNganh, TrangThai) VALUES
(N'C#', N'Ngon ngu lap trinh', 'HoatDong'),
(N'Java', N'Ngon ngu lap trinh', 'HoatDong'),
(N'Python', N'Ngon ngu lap trinh', 'HoatDong'),
(N'JavaScript', N'Ngon ngu lap trinh', 'HoatDong'),
(N'TypeScript', N'Ngon ngu lap trinh', 'HoatDong'),
(N'React', N'Frontend', 'HoatDong'),
(N'Angular', N'Frontend', 'HoatDong'),
(N'Node.js', N'Backend', 'HoatDong'),
(N'ASP.NET Core', N'Backend', 'HoatDong'),
(N'SQL Server', N'Database', 'HoatDong'),
(N'MongoDB', N'Database', 'HoatDong'),
(N'Docker', N'DevOps', 'HoatDong'),
(N'Kubernetes', N'DevOps', 'HoatDong'),
(N'Git', N'Cong cu', 'HoatDong'),
(N'AWS', N'DevOps', 'HoatDong');
GO

-- ====================================================================
-- SEED: DANH_MUC_NGANH_NGHE (Phase 7 - can cho BM08 "Linh vuc hoat
-- dong" luc NTD cap nhat ho so cong ty)
-- ====================================================================
INSERT INTO DANH_MUC_NGANH_NGHE (TenNganhNghe, TrangThai) VALUES
(N'Công nghệ thông tin', 'HoatDong'),
(N'Tài chính - Ngân hàng', 'HoatDong'),
(N'Thương mại điện tử', 'HoatDong'),
(N'Giáo dục', 'HoatDong'),
(N'Y tế', 'HoatDong'),
(N'Sản xuất', 'HoatDong'),
(N'Bán lẻ', 'HoatDong'),
(N'Marketing', 'HoatDong');
GO

-- ====================================================================
-- SEED: Tai khoan Ung vien demo de test dang nhap/giao dien (khong
-- thuoc UC dang ky nao, chi de tien QA thu cong). Da xac thuc san
-- (DaXacThuc=1) nen dang nhap duoc ngay, khong can qua buoc xac thuc
-- email. Hash BCrypt cua "Test@1234" (cost=11, dat QD01/TS1: >=8 ky
-- tu + >=1 chu so).
-- ====================================================================
DECLARE @MaTkUngVienDemo INT;

INSERT INTO TAI_KHOAN (Email, MatKhau, VaiTro, DaXacThuc, TrangThai, SoLanDangNhapSai, NgayTao) VALUES
(N'ungvien.demo@jobhunter.local', '$2a$11$vYTeFhlGGpfPl6aPiELM1eqPelmlqJ3rxovEFJtDUYdVdex82b5JO', 'UngVien', 1, 'HoatDong', 0, SYSDATETIME());

SET @MaTkUngVienDemo = SCOPE_IDENTITY();

INSERT INTO UNG_VIEN (MaTK, HoTen, SDT, DiaChi, GioiThieuBanThan, SoCV) VALUES
(@MaTkUngVienDemo, N'Nguyễn Văn Test', N'0901234567', N'Quận 1, TP. Hồ Chí Minh', N'Tài khoản demo dùng để kiểm tra giao diện Ứng viên.', 0);
GO
