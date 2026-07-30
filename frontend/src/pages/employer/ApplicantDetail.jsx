import { useEffect, useState } from "react";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

const TRANG_THAI_LABEL = {
  DaNop: "Đã nộp",
  DangXemXet: "Đang xem xét",
  PhongVan: "Phỏng vấn",
  TuChoi: "Từ chối",
  Nhan: "Nhận",
  DaHuy: "Đã hủy",
};

const TRINH_DO_LABEL = { TrungCap: "Trung cấp", CaoDang: "Cao đẳng", DaiHoc: "Đại học", SauDaiHoc: "Sau đại học" };

// Phai khop dung ChuyenTiepHopLe o backend (ApplicationService.cs) - BR05/QD11
const CHUYEN_TIEP_HOP_LE = {
  DaNop: ["DangXemXet", "TuChoi"],
  DangXemXet: ["PhongVan", "TuChoi"],
  PhongVan: ["Nhan", "TuChoi"],
};

export default function ApplicantDetail({ maDon, skillNames, onClose, onUpdated }) {
  const { auth } = useAuth();
  const [detail, setDetail] = useState(null);
  const [trangThaiMoi, setTrangThaiMoi] = useState("");
  const [ghiChuNoiBo, setGhiChuNoiBo] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);

  const load = () => api.get(`/applications/${maDon}/detail`, auth.token).then((d) => {
    setDetail(d);
    setGhiChuNoiBo(d.ghiChuNoiBo || "");
    setTrangThaiMoi("");
  });

  useEffect(() => {
    load();
  }, [maDon]);

  const handleUpdate = async () => {
    if (!trangThaiMoi) return;
    setError("");
    setSuccess("");
    setLoading(true);
    try {
      await api.put(`/applications/${maDon}/status`, { trangThaiMoi, ghiChuNoiBo: ghiChuNoiBo || null }, auth.token);
      setSuccess("Cập nhật trạng thái thành công."); // MS08
      await load();
      onUpdated?.();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra."); // MS09
    } finally {
      setLoading(false);
    }
  };

  if (!detail) {
    return (
      <div style={{ position: "fixed", inset: 0, background: "rgba(0,0,0,0.4)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 100 }}>
        <div className="card" style={{ background: "white" }}>Đang tải...</div>
      </div>
    );
  }

  const cacBuocKeTiep = CHUYEN_TIEP_HOP_LE[detail.trangThai] || [];

  return (
    <div style={{ position: "fixed", inset: 0, background: "rgba(0,0,0,0.4)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 100 }}>
      <div className="card" style={{ width: 560, maxHeight: "85vh", overflowY: "auto", background: "white" }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start" }}>
          <h3 style={{ margin: 0 }}>{detail.hoTenUngVien}</h3>
          <button type="button" className="btn btn-secondary" style={{ height: 32, padding: "0 12px" }} onClick={onClose}>Đóng</button>
        </div>
        <p style={{ color: "var(--text-muted)" }}>
          Trạng thái hiện tại: <strong>{TRANG_THAI_LABEL[detail.trangThai] || detail.trangThai}</strong>
        </p>

        <h4 style={{ marginBottom: 4 }}>{detail.cv.tenCV}</h4>
        {detail.cv.viTriMongMuon && <p style={{ margin: "2px 0" }}>Vị trí mong muốn: {detail.cv.viTriMongMuon}</p>}
        {detail.cv.mucLuongMongMuon && <p style={{ margin: "2px 0" }}>Mức lương mong muốn: {detail.cv.mucLuongMongMuon}</p>}
        {detail.cv.trinhDoHocVan && <p style={{ margin: "2px 0" }}>Trình độ: {TRINH_DO_LABEL[detail.cv.trinhDoHocVan] || detail.cv.trinhDoHocVan}</p>}

        {detail.cv.duongDanFile ? (
          <p><a href={detail.cv.duongDanFile} target="_blank" rel="noreferrer">Xem file CV đã tải lên</a></p>
        ) : (
          <>
            <h4>Kỹ năng</h4>
            <p>{detail.cv.kyNang.map((k) => skillNames?.[k.maKyNang] || k.maKyNang).join(", ") || "(không có)"}</p>

            <h4>Kinh nghiệm</h4>
            {detail.cv.kinhNghiem.length === 0 && <p>(không có)</p>}
            {detail.cv.kinhNghiem.map((k, i) => <p key={i} style={{ margin: "2px 0" }}>{k.viTri} tại {k.congTy} ({k.tuNgay} — {k.denNgay || "hiện tại"})</p>)}

            <h4>Học vấn</h4>
            {detail.cv.hocVan.length === 0 && <p>(không có)</p>}
            {detail.cv.hocVan.map((h, i) => <p key={i} style={{ margin: "2px 0" }}>{h.chuyenNganh} — {h.truong}</p>)}
          </>
        )}

        {detail.thuGioiThieu && (
          <>
            <h4>Thư giới thiệu</h4>
            <p style={{ whiteSpace: "pre-wrap" }}>{detail.thuGioiThieu}</p>
          </>
        )}

        <div className="field">
          <label>Ghi chú nội bộ</label>
          <textarea rows={2} value={ghiChuNoiBo} onChange={(e) => setGhiChuNoiBo(e.target.value)} disabled={cacBuocKeTiep.length === 0} />
        </div>

        {cacBuocKeTiep.length > 0 ? (
          <div className="field">
            <label>Chuyển trạng thái</label>
            <div style={{ display: "flex", gap: 8 }}>
              <select value={trangThaiMoi} onChange={(e) => setTrangThaiMoi(e.target.value)} style={{ flex: 1 }}>
                <option value="">-- Chọn trạng thái mới --</option>
                {cacBuocKeTiep.map((t) => <option key={t} value={t}>{TRANG_THAI_LABEL[t]}</option>)}
              </select>
              <button className="btn btn-primary" style={{ height: 36, padding: "0 16px" }} disabled={!trangThaiMoi || loading} onClick={handleUpdate}>
                {loading ? "Đang lưu..." : "Cập nhật"}
              </button>
            </div>
          </div>
        ) : (
          <p style={{ color: "var(--text-muted)" }}>Đơn đã ở trạng thái cuối cùng, không thể thay đổi thêm.</p>
        )}

        {error && <p className="error-text">{error}</p>}
        {success && <p className="success-text">{success}</p>}
      </div>
    </div>
  );
}
