import { useEffect, useState } from "react";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";
import JobDetailModal from "../../components/JobDetailModal";

export default function RemovedJobs() {
  const { auth } = useAuth();
  const [activeJobs, setActiveJobs] = useState([]);
  const [removedJobs, setRemovedJobs] = useState([]);
  const [removingId, setRemovingId] = useState(null);
  const [lyDo, setLyDo] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [viewingId, setViewingId] = useState(null); // ma tin dang xem chi tiet

  const load = async () => {
    setActiveJobs(await api.get("/jobs", auth.token));
    setRemovedJobs(await api.get("/jobs/removed", auth.token));
  };

  useEffect(() => {
    load();
  }, []);

  const clearMsg = () => {
    setError("");
    setSuccess("");
  };

  const submitRemove = async (id) => {
    clearMsg();
    try {
      const result = await api.post(`/jobs/${id}/remove`, { lyDo }, auth.token);
      setSuccess(result.message); // MS45
      setRemovingId(null);
      setLyDo("");
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra."); // MS54
    }
  };

  const restore = async (id) => {
    clearMsg();
    try {
      const result = await api.post(`/jobs/${id}/restore-removed`, undefined, auth.token);
      setSuccess(result.message); // MS46
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    }
  };

  return (
    <div>
      <div className="dashboard-header-band">
        <h2>Gỡ tin vi phạm</h2>
      </div>
      {error && <p className="error-text">{error}</p>}
      {success && <p className="success-text">{success}</p>}

      <h3>Tin đang hiển thị công khai</h3>
      {activeJobs.length === 0 && <p>Không có tin nào.</p>}
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(280px, 1fr))", gap: 12, marginBottom: 24 }}>
        {activeJobs.map((job) => (
          <div key={job.maTin} className="card" style={{ cursor: "pointer" }} onClick={() => setViewingId(job.maTin)} title="Bấm để xem chi tiết tin">
            <span className="badge badge-success">Đang hiển thị</span>
            <p style={{ fontWeight: 600, margin: "10px 0 2px" }}>{job.tieuDe}</p>
            <p style={{ margin: 0, color: "var(--text-muted)", fontSize: 14 }}>{job.tenCongTy}</p>
            <div style={{ marginTop: 12 }} onClick={(e) => e.stopPropagation()}>
              {removingId === job.maTin ? (
                <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
                  <input placeholder="Lý do gỡ tin (bắt buộc)" value={lyDo} onChange={(e) => setLyDo(e.target.value)} />
                  <div style={{ display: "flex", gap: 8 }}>
                    <button className="btn btn-primary" style={{ height: 36 }} onClick={() => submitRemove(job.maTin)}>Xác nhận gỡ</button>
                    <button className="btn btn-secondary" style={{ height: 36 }} onClick={() => { setRemovingId(null); setLyDo(""); }}>Hủy</button>
                  </div>
                </div>
              ) : (
                <button className="btn btn-secondary" style={{ height: 36 }} onClick={() => { setRemovingId(job.maTin); setLyDo(""); }}>Gỡ tin</button>
              )}
            </div>
          </div>
        ))}
      </div>

      <h3>Tin đã gỡ</h3>
      {removedJobs.length === 0 && <p>Không có tin nào.</p>}
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(280px, 1fr))", gap: 12 }}>
        {removedJobs.map((job) => (
          <div key={job.maTin} className="card" style={{ cursor: "pointer" }} onClick={() => setViewingId(job.maTin)} title="Bấm để xem chi tiết tin">
            <span className="badge badge-danger">Đã gỡ</span>
            <p style={{ fontWeight: 600, margin: "10px 0 2px" }}>{job.tieuDe}</p>
            <p style={{ margin: 0, color: "var(--text-muted)", fontSize: 14 }}>{job.tenCongTy}</p>
            <button className="btn btn-primary" style={{ height: 36, marginTop: 12 }} onClick={(e) => { e.stopPropagation(); restore(job.maTin); }}>Phục hồi</button>
          </div>
        ))}
      </div>

      {viewingId && <JobDetailModal maTin={viewingId} onClose={() => setViewingId(null)} />}
    </div>
  );
}
