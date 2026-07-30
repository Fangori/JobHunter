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
