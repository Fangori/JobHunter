import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

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

const FILTER_TABS = [
  { key: "all", label: "Tất cả", match: () => true },
  { key: "ChoDuyet", label: "Chờ duyệt", match: (s) => s === "ChoDuyet" },
  { key: "DaDuyet", label: "Đã duyệt", match: (s) => s === "DaDuyet" },
  { key: "TuChoi", label: "Từ chối", match: (s) => s === "TuChoi" },
  { key: "DaGo", label: "Đã gỡ", match: (s) => s === "DaGo" },
  { key: "DaDong", label: "Đã đóng", match: (s) => s === "DaDong" },
];

// Gia dinh trinh duyet chay o timezone UTC+ (VN); UTC am co the lech 1 ngay do parse UTC + setHours local.
function soNgayConLai(hanNopHoSo) {
  const han = new Date(hanNopHoSo);
  const homNay = new Date();
  han.setHours(0, 0, 0, 0);
  homNay.setHours(0, 0, 0, 0);
  return Math.round((han - homNay) / (1000 * 60 * 60 * 24));
}

export default function MyJobs() {
  const { auth } = useAuth();
  const [jobs, setJobs] = useState(null);
  const [filter, setFilter] = useState("all");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [busyId, setBusyId] = useState(null);
  const [extendingId, setExtendingId] = useState(null);
  const [hanNopMoi, setHanNopMoi] = useState("");

  const load = () => api.get("/jobs/mine", auth.token).then(setJobs);

  useEffect(() => {
    load();
  }, []);

  const clearMsg = () => {
    setError("");
    setSuccess("");
  };

  const handleClose = async (maTin) => {
    if (!window.confirm("Đóng tin này? Ứng viên sẽ không thể ứng tuyển thêm.")) return;
    clearMsg();
    setBusyId(maTin);
    try {
      const result = await api.post(`/jobs/${maTin}/close`, undefined, auth.token);
      setSuccess(result.message);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    } finally {
      setBusyId(null);
    }
  };

  const startExtend = (maTin) => {
    clearMsg();
    setExtendingId(maTin);
    setHanNopMoi("");
  };

  const confirmExtend = async (maTin) => {
    clearMsg();
    setBusyId(maTin);
    try {
      const result = await api.post(`/jobs/${maTin}/extend`, { hanNopMoi }, auth.token);
      setSuccess(result.message); // MS42
      setExtendingId(null);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    } finally {
      setBusyId(null);
    }
  };

  if (!jobs) return <div className="page-container">Đang tải...</div>;

  const soChoDuyet = jobs.filter((j) => j.trangThai === "ChoDuyet").length;
  const soDaDuyet = jobs.filter((j) => j.trangThai === "DaDuyet").length;
  const soDaDong = jobs.filter((j) => j.trangThai === "DaDong").length;
  const visible = jobs.filter((j) => FILTER_TABS.find((t) => t.key === filter).match(j.trangThai));

  return (
    <div className="page-container">
      <div className="dashboard-header-band">
        <h1>Tin tuyển dụng của tôi</h1>
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "repeat(4, 1fr)", gap: 12, marginBottom: 20 }}>
        <div className="card" style={{ textAlign: "center" }}>
          <p style={{ fontSize: 28, fontWeight: 700, margin: 0, color: "var(--navy)" }}>{jobs.length}</p>
          <p style={{ margin: 0, color: "var(--text-muted)", fontSize: 14 }}>Tổng tin</p>
        </div>
        <div className="card" style={{ textAlign: "center" }}>
          <p style={{ fontSize: 28, fontWeight: 700, margin: 0, color: "var(--warning)" }}>{soChoDuyet}</p>
          <p style={{ margin: 0, color: "var(--text-muted)", fontSize: 14 }}>Chờ duyệt</p>
        </div>
        <div className="card" style={{ textAlign: "center" }}>
          <p style={{ fontSize: 28, fontWeight: 700, margin: 0, color: "var(--success)" }}>{soDaDuyet}</p>
          <p style={{ margin: 0, color: "var(--text-muted)", fontSize: 14 }}>Đã duyệt</p>
        </div>
        <div className="card" style={{ textAlign: "center" }}>
          <p style={{ fontSize: 28, fontWeight: 700, margin: 0, color: "var(--text-muted)" }}>{soDaDong}</p>
          <p style={{ margin: 0, color: "var(--text-muted)", fontSize: 14 }}>Đã đóng</p>
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
      {success && <p className="success-text">{success}</p>}
      {visible.length === 0 && <p>{jobs.length === 0 ? "Bạn chưa đăng tin nào." : "Không có tin nào ở trạng thái này."}</p>}
      {visible.map((job) => {
        const ngayConLai = job.trangThai === "DaDuyet" ? soNgayConLai(job.hanNopHoSo) : null;
        return (
          <div key={job.maTin} className="card" style={{ marginBottom: 12 }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", flexWrap: "wrap", gap: 8 }}>
              <div>
                <strong>{job.tieuDe}</strong>
                <p style={{ margin: "4px 0", fontSize: 14 }}>
                  Trạng thái: <span className={`badge ${TRANG_THAI_BADGE[job.trangThai] || "badge-neutral"}`}>{TRANG_THAI_LABEL[job.trangThai] || job.trangThai}</span>
                  {" · "}Hạn nộp: {job.hanNopHoSo}
                  {ngayConLai !== null && (
                    <span style={{ color: ngayConLai <= 3 ? "var(--warning)" : "var(--text-muted)", fontWeight: ngayConLai <= 3 ? 600 : 400 }}>
                      {" "}({ngayConLai >= 0 ? `còn ${ngayConLai} ngày` : "đã quá hạn"})
                    </span>
                  )}
                </p>
              </div>
              <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
                <Link to={`/employer/jobs/${job.maTin}/applicants`} className="btn btn-secondary" style={{ height: 32, padding: "0 12px" }}>
                  Xem ứng viên
                </Link>
                {job.trangThai !== "DaGo" && job.trangThai !== "DaDong" && (
                  <Link to={`/employer/jobs/${job.maTin}/edit`} className="btn btn-secondary" style={{ height: 32, padding: "0 12px" }}>
                    Sửa
                  </Link>
                )}
                {job.trangThai === "DaDuyet" && (
                  <>
                    <button
                      type="button"
                      className="btn btn-secondary"
                      style={{ height: 32, padding: "0 12px" }}
                      disabled={busyId === job.maTin}
                      onClick={() => startExtend(job.maTin)}
                    >
                      Gia hạn
                    </button>
                    <button
                      type="button"
                      className="btn btn-secondary"
                      style={{ height: 32, padding: "0 12px" }}
                      disabled={busyId === job.maTin}
                      onClick={() => handleClose(job.maTin)}
                    >
                      Đóng tin
                    </button>
                  </>
                )}
              </div>
            </div>

            {extendingId === job.maTin && (
              <div style={{ marginTop: 12, display: "flex", gap: 8, alignItems: "center" }}>
                <input type="date" value={hanNopMoi} onChange={(e) => setHanNopMoi(e.target.value)} />
                <button
                  type="button"
                  className="btn btn-primary"
                  style={{ height: 36, padding: "0 16px" }}
                  disabled={!hanNopMoi || busyId === job.maTin}
                  onClick={() => confirmExtend(job.maTin)}
                >
                  Xác nhận
                </button>
                <button type="button" className="btn btn-secondary" style={{ height: 36, padding: "0 16px" }} onClick={() => setExtendingId(null)}>
                  Hủy
                </button>
              </div>
            )}
          </div>
        );
      })}
    </div>
  );
}
