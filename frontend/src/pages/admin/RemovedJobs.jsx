import { useEffect, useState } from "react";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

export default function RemovedJobs() {
  const { auth } = useAuth();
  const [activeJobs, setActiveJobs] = useState([]);
  const [removedJobs, setRemovedJobs] = useState([]);
  const [removingId, setRemovingId] = useState(null);
  const [lyDo, setLyDo] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

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
      <h2>Gỡ tin vi phạm</h2>
      {error && <p className="error-text">{error}</p>}
      {success && <p className="success-text">{success}</p>}

      <div className="card" style={{ marginBottom: 24 }}>
        <h3>Tin đang hiển thị công khai</h3>
        {activeJobs.length === 0 && <p>Không có tin nào.</p>}
        {activeJobs.map((job) => (
          <div key={job.maTin} style={{ borderBottom: "1px solid var(--border)", padding: "12px 0" }}>
            <strong>{job.tieuDe}</strong> — {job.tenCongTy}
            <div style={{ marginTop: 8 }}>
              {removingId === job.maTin ? (
                <div style={{ display: "flex", gap: 8 }}>
                  <input placeholder="Lý do gỡ tin (bắt buộc)" value={lyDo} onChange={(e) => setLyDo(e.target.value)} style={{ flex: 1 }} />
                  <button className="btn btn-primary" style={{ height: 36 }} onClick={() => submitRemove(job.maTin)}>Xác nhận gỡ</button>
                  <button className="btn btn-secondary" style={{ height: 36 }} onClick={() => { setRemovingId(null); setLyDo(""); }}>Hủy</button>
                </div>
              ) : (
                <button className="btn btn-secondary" style={{ height: 36 }} onClick={() => { setRemovingId(job.maTin); setLyDo(""); }}>Gỡ tin</button>
              )}
            </div>
          </div>
        ))}
      </div>

      <div className="card">
        <h3>Tin đã gỡ</h3>
        {removedJobs.length === 0 && <p>Không có tin nào.</p>}
        {removedJobs.map((job) => (
          <div key={job.maTin} style={{ borderBottom: "1px solid var(--border)", padding: "12px 0", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
            <span><strong>{job.tieuDe}</strong> — {job.tenCongTy}</span>
            <button className="btn btn-primary" style={{ height: 36 }} onClick={() => restore(job.maTin)}>Phục hồi</button>
          </div>
        ))}
      </div>
    </div>
  );
}
