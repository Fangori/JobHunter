import { useEffect, useState } from "react";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";
import JobDetailModal from "../../components/JobDetailModal";

export default function ApproveJobs() {
  const { auth } = useAuth();
  const [jobs, setJobs] = useState([]);
  const [stats, setStats] = useState({ soChoDuyet: 0, soDaDuyet: 0 });
  const [rejectingId, setRejectingId] = useState(null);
  const [lyDo, setLyDo] = useState("");
  const [error, setError] = useState("");
  const [viewingId, setViewingId] = useState(null); // ma tin dang xem chi tiet

  const load = async () => {
    setJobs(await api.get("/jobs/pending", auth.token));
    setStats(await api.get("/jobs/pending/stats", auth.token));
  };

  useEffect(() => { load(); }, []);

  const approve = async (id) => {
    setError("");
    try {
      await api.post(`/jobs/${id}/approve`, {}, auth.token);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    }
  };

  const submitReject = async (id) => {
    setError("");
    try {
      await api.post(`/jobs/${id}/reject`, { lyDo }, auth.token);
      setRejectingId(null);
      setLyDo("");
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    }
  };

  return (
    <div>
      <div className="dashboard-header-band">
        <h2>Duyệt tin tuyển dụng</h2>
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "repeat(2, 1fr)", gap: 16, marginBottom: 24 }}>
        <div className="card" style={{ textAlign: "center" }}>
          <p style={{ fontSize: 32, fontWeight: 700, margin: 0, color: "var(--indigo)" }}>{stats.soChoDuyet}</p>
          <p style={{ margin: 0, color: "var(--text-muted)" }}>Số tin chờ duyệt</p>
        </div>
        <div className="card" style={{ textAlign: "center" }}>
          <p style={{ fontSize: 32, fontWeight: 700, margin: 0, color: "var(--success)" }}>{stats.soDaDuyet}</p>
          <p style={{ margin: 0, color: "var(--text-muted)" }}>Số tin đã duyệt</p>
        </div>
      </div>

      {error && <p className="error-text">{error}</p>}

      <h3>Danh sách chờ duyệt</h3>
      {jobs.length === 0 && <p>Không có tin nào chờ duyệt.</p>}
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(280px, 1fr))", gap: 12 }}>
        {jobs.map((job) => (
          <div
            key={job.maTin}
            className="card"
            style={{ cursor: "pointer" }}
            onClick={() => setViewingId(job.maTin)}
            title="Bấm để xem chi tiết tin"
          >
            <span className="badge badge-warning">Chờ duyệt</span>
            <p style={{ fontWeight: 600, margin: "10px 0 2px" }}>{job.tieuDe}</p>
            <p style={{ margin: 0, color: "var(--text-muted)", fontSize: 14 }}>{job.tenCongTy}</p>
            <div style={{ marginTop: 12, display: "flex", gap: 8 }} onClick={(e) => e.stopPropagation()}>
              <button className="btn btn-primary" style={{ height: 36 }} onClick={() => approve(job.maTin)}>Duyệt</button>
              <button className="btn btn-secondary" style={{ height: 36 }} onClick={() => setRejectingId(job.maTin)}>Từ chối</button>
            </div>
            {rejectingId === job.maTin && (
              <div style={{ marginTop: 8 }} onClick={(e) => e.stopPropagation()}>
                <input placeholder="Lý do từ chối (bắt buộc)" value={lyDo} onChange={(e) => setLyDo(e.target.value)} />
                <button className="btn btn-primary" style={{ height: 36, marginTop: 8 }} onClick={() => submitReject(job.maTin)}>
                  Xác nhận từ chối
                </button>
              </div>
            )}
          </div>
        ))}
      </div>

      {viewingId && <JobDetailModal maTin={viewingId} onClose={() => setViewingId(null)} />}
    </div>
  );
}
