import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
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

const TRANG_THAI_BADGE = {
  DaNop: "badge-info",
  DangXemXet: "badge-warning",
  PhongVan: "badge-warning",
  TuChoi: "badge-danger",
  Nhan: "badge-success",
  DaHuy: "badge-neutral",
};

const HUY_DUOC = ["DaNop", "DangXemXet"]; // BR10

const FILTER_TABS = [
  { key: "all", label: "Tất cả", match: () => true },
  { key: "cho_xu_ly", label: "Chờ xử lý", match: (s) => s === "DaNop" || s === "DangXemXet" },
  { key: "phong_van", label: "Phỏng vấn", match: (s) => s === "PhongVan" },
  { key: "da_nhan", label: "Đã nhận", match: (s) => s === "Nhan" },
  { key: "tu_choi", label: "Từ chối", match: (s) => s === "TuChoi" },
];

export default function MyApplications() {
  const { auth } = useAuth();
  const [applications, setApplications] = useState(null);
  const [error, setError] = useState("");
  const [loadError, setLoadError] = useState(false);
  const [busyId, setBusyId] = useState(null);
  const [filter, setFilter] = useState("all");

  const load = () => {
    setLoadError(false);
    api.get("/applications/mine", auth.token).then(setApplications).catch(() => setLoadError(true));
  };

  useEffect(() => {
    load();
  }, []);

  const handleCancel = async (maDon) => {
    setError("");
    setBusyId(maDon);
    try {
      await api.post(`/applications/${maDon}/cancel`, undefined, auth.token);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra."); // MS34
    } finally {
      setBusyId(null);
    }
  };

  if (loadError) {
    return (
      <div className="page-container" style={{ maxWidth: 720 }}>
        <h1>Đơn ứng tuyển của tôi</h1>
        <p className="error-text">Không tải được dữ liệu.</p>
        <button type="button" className="btn btn-secondary" onClick={load}>Thử lại</button>
      </div>
    );
  }

  if (!applications) return <div className="page-container">Đang tải...</div>;

  return (
    <div className="page-container" style={{ maxWidth: 720 }}>
      <div className="dashboard-header-band">
        <h1>Đơn ứng tuyển của tôi</h1>
      </div>
      <div style={{ display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: 12, marginBottom: 20 }}>
        <div className="card" style={{ textAlign: "center" }}>
          <p style={{ fontSize: 28, fontWeight: 700, margin: 0, color: "var(--navy)" }}>{applications.length}</p>
          <p style={{ margin: 0, color: "var(--text-muted)", fontSize: 14 }}>Tổng số</p>
        </div>
        <div className="card" style={{ textAlign: "center" }}>
          <p style={{ fontSize: 28, fontWeight: 700, margin: 0, color: "var(--navy)" }}>{applications.filter((d) => d.trangThai === "PhongVan").length}</p>
          <p style={{ margin: 0, color: "var(--text-muted)", fontSize: 14 }}>Đang phỏng vấn</p>
        </div>
        <div className="card" style={{ textAlign: "center" }}>
          <p style={{ fontSize: 28, fontWeight: 700, margin: 0, color: "var(--navy)" }}>{applications.filter((d) => d.trangThai === "DaNop" || d.trangThai === "DangXemXet").length}</p>
          <p style={{ margin: 0, color: "var(--text-muted)", fontSize: 14 }}>Chờ xử lý</p>
        </div>
      </div>
      <div className="tabs">
        {FILTER_TABS.map((t) => (
          <button key={t.key} type="button" className={filter === t.key ? "active" : ""} onClick={() => setFilter(t.key)}>
            {t.label}
          </button>
        ))}
      </div>
      {error && <p className="error-text">{error}</p>}
      {applications.length === 0 && <p>Bạn chưa ứng tuyển vào tin nào.</p>}
      {applications
        .filter((don) => FILTER_TABS.find((t) => t.key === filter).match(don.trangThai))
        .map((don) => (
        <div key={don.maDon} className="card" style={{ marginBottom: 12 }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start" }}>
            <div>
              <Link to={`/jobs/${don.maTin}`}><strong>{don.tieuDe}</strong></Link>
              <p style={{ color: "var(--text-muted)", margin: "4px 0" }}>{don.tenCongTy}</p>
              <p style={{ margin: "4px 0" }}>
                <span className={`badge ${TRANG_THAI_BADGE[don.trangThai] || "badge-neutral"}`}>
                  {TRANG_THAI_LABEL[don.trangThai] || don.trangThai}
                </span>
              </p>
              <p style={{ margin: 0, fontSize: 13, color: "var(--text-muted)" }}>
                Nộp lúc: {new Date(don.ngayNop).toLocaleString("vi-VN")}
              </p>
            </div>
            {HUY_DUOC.includes(don.trangThai) && (
              <button
                type="button"
                className="btn btn-secondary"
                style={{ height: 36, padding: "0 16px", whiteSpace: "nowrap" }}
                disabled={busyId === don.maDon}
                onClick={() => handleCancel(don.maDon)}
              >
                {busyId === don.maDon ? "Đang hủy..." : "Hủy đơn"}
              </button>
            )}
          </div>
        </div>
      ))}
    </div>
  );
}
