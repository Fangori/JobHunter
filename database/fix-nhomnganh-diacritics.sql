-- One-off fix: DANH_MUC_KY_NANG.NhomNganh bi thieu dau tieng Viet trong
-- seed goc (khong phai loi encoding sqlcmd - chinh file seed go thieu dau
-- tu dau). Ap truc tiep len DB dang chay, khong reseed toan bo de giu
-- nguyen du lieu demo/QA da tao.
UPDATE DANH_MUC_KY_NANG SET NhomNganh = N'Ngôn ngữ lập trình' WHERE NhomNganh = N'Ngon ngu lap trinh';
UPDATE DANH_MUC_KY_NANG SET NhomNganh = N'Công cụ' WHERE NhomNganh = N'Cong cu';
GO
SELECT TenKyNang, NhomNganh FROM DANH_MUC_KY_NANG ORDER BY TenKyNang;
GO
