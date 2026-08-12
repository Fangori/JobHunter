import { useEffect, useState } from "react";
import { X } from "lucide-react";
import { api } from "../api/client";

const TRANG_THAI_LABEL = {
  ChoDuyet: "Chờ duyệt",
  DaDuyet: "Đã duyệt",
  TuChoi: "Từ chối",
  DaGo: "Đã gỡ",
  DaDong: "Đã đóng",
};
const TRANG_THAI_BADGE = {
  ChoDuyet: "badge-warning",
  DaDuyet: "badge-success",
  TuChoi: "badge-danger",
  DaGo: "badge-danger",
  DaDong: "badge-neutral",
};

// Modal xem chi tiet 1 tin tuyen dung, dung chung cho Admin (Duyet tin/Go
// tin) - GET /jobs/{id} khong gioi han theo TrangThai nen xem duoc ca tin
// Cho duyet/Tu choi/Da go, khong chi tin Da duyet nhu trang JobDetail cong khai.
export default function JobDetailModal({ maTin, onClose }) {
  const [job, setJob] = useState(null);
  const [skillNames, setSkillNames] = useState({});
  const [error, setError] = useState("");

  useEffect(() => {
    api.get(`/jobs/${maTin}`).then(setJob).catch(() => setError("Không tải được chi tiết tin."));
    api.get("/skills").then((skills) => {
      const map = {};
      skills.forEach((s) => (map[s.maKyNang] = s.tenKyNang));
      setSkillNames(map);
    });
  }, [maTin]);

  return (
    <div
      style={{ position: "fixed", inset: 0, background: "rgba(0,0,0,0.4)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 100, padding: 24 }}
      onClick={onClose}
    >
      <div className="card" style={{ width: 640, maxHeight: "85vh", overflowY: "auto", background: "white", position: "relative" }} onClick={(e) => e.stopPropagation()}>
        <button
          type="button"
          onClick={onClose}
          aria-label="Đóng"
          style={{ position: "absolute", top: 16, right: 16, background: "none", border: "none", cursor: "pointer", color: "var(--text-muted)", display: "flex" }}
        >
          <X size={20} />
        </button>

        {error && <p className="error-text">{error}</p>}
        {!job && !error && <p>Đang tải...</p>}

        {job && (
          <>
            <span className={`badge ${TRANG_THAI_BADGE[job.trangThai] || "badge-neutral"}`}>
              {TRANG_THAI_LABEL[job.trangThai] || job.trangThai}
            </span>
            <h2 style={{ margin: "10px 0 4px" }}>{job.tieuDe}</h2>
            <p style={{ margin: "0 0 12px", color: "var(--text-muted)" }}>{job.tenCongTy}</p>

            <div style={{ display: "flex", gap: 16, flexWrap: "wrap", fontSize: 14, color: "var(--text-muted)", marginBottom: 16 }}>
              {job.diaDiem && <span>📍 {job.diaDiem}</span>}
              {job.hinhThucLamViec && <span>{job.hinhThucLamViec}</span>}
              {job.mucLuong && <span>💰 {job.mucLuong}</span>}
              {job.soLuongTuyen && <span>Tuyển {job.soLuongTuyen} người</span>}
              <span>Hạn nộp: {job.hanNopHoSo}</span>
            </div>

            <h3>Mô tả công việc</h3>
            <p style={{ whiteSpace: "pre-wrap" }}>{job.moTaCongViec}</p>

            {job.yeuCauUngVien && (
              <>
                <h3>Yêu cầu ứng viên</h3>
                <p style={{ whiteSpace: "pre-wrap" }}>{job.yeuCauUngVien}</p>
              </>
            )}
            {job.quyenLoi && (
              <>
                <h3>Quyền lợi</h3>
                <p style={{ whiteSpace: "pre-wrap" }}>{job.quyenLoi}</p>
              </>
            )}

            {job.kyNangYeuCau?.length > 0 && (
              <>
                <h3>Kỹ năng yêu cầu</h3>
                <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
                  {job.kyNangYeuCau.map((k) => (
                    <span key={k.maKyNang} className="badge badge-info">
                      {skillNames[k.maKyNang] || k.maKyNang}
                      {k.mucDoUuTien === "BatBuoc" ? " (Bắt buộc)" : k.mucDoUuTien === "UuTien" ? " (Ưu tiên)" : ""}
                    </span>
                  ))}
                </div>
              </>
            )}
          </>
        )}
      </div>
    </div>
  );
}
