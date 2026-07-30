import { useEffect, useState } from "react";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

export default function ApproveJobs() {
  const { auth } = useAuth();
  const [jobs, setJobs] = useState([]);
  const [stats, setStats] = useState({ soChoDuyet: 0, soDaDuyet: 0 });
  const [rejectingId, setRejectingId] = useState(null);
  const [lyDo, setLyDo] = useState("");
  const [error, setError] = useState("");

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
    <div className="page-container">
      <h2>Duyệt tin tuyển dụng</h2>

      <div style={{ display: "flex", gap: 16, marginBottom: 24 }}>
        <div className="card" style={{ flex: 1, textAlign: "center" }}>
          <div style={{ fontSize: 32, fontWeight: 700, color: "var(--indigo)" }}>{stats.soChoDuyet}</div>
          <div style={{ color: "var(--text-muted)" }}>Số tin chờ duyệt</div>
        </div>
        <div className="card" style={{ flex: 1, textAlign: "center" }}>
          <div style={{ fontSize: 32, fontWeight: 700, color: "var(--success)" }}>{stats.soDaDuyet}</div>
          <div style={{ color: "var(--text-muted)" }}>Số tin đã duyệt</div>
        </div>
      </div>

      {error && <p className="error-text">{error}</p>}

      <div className="card">
        <h3>Danh sách chờ duyệt</h3>
        {jobs.length === 0 && <p>Không có tin nào chờ duyệt.</p>}
        {jobs.map((job) => (
          <div key={job.maTin} style={{ borderBottom: "1px solid var(--border)", padding: "12px 0" }}>
            <strong>{job.tieuDe}</strong> — {job.tenCongTy}
            <div style={{ marginTop: 8, display: "flex", gap: 8 }}>
              <button className="btn btn-primary" style={{ height: 36 }} onClick={() => approve(job.maTin)}>Duyệt</button>
              <button className="btn btn-secondary" style={{ height: 36 }} onClick={() => setRejectingId(job.maTin)}>Từ chối</button>
            </div>
            {rejectingId === job.maTin && (
              <div style={{ marginTop: 8 }}>
                <input placeholder="Lý do từ chối (bắt buộc)" value={lyDo} onChange={(e) => setLyDo(e.target.value)} />
                <button className="btn btn-primary" style={{ height: 36, marginTop: 8 }} onClick={() => submitReject(job.maTin)}>
                  Xác nhận từ chối
                </button>
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
