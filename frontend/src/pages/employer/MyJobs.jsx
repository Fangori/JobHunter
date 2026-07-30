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

export default function MyJobs() {
  const { auth } = useAuth();
  const [jobs, setJobs] = useState(null);
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

  return (
    <div className="page-container">
      <h1>Tin tuyển dụng của tôi</h1>
      {error && <p className="error-text">{error}</p>}
      {success && <p className="success-text">{success}</p>}
      {jobs.length === 0 && <p>Bạn chưa đăng tin nào.</p>}
      {jobs.map((job) => (
        <div key={job.maTin} className="card" style={{ marginBottom: 12 }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", flexWrap: "wrap", gap: 8 }}>
            <div>
              <strong>{job.tieuDe}</strong>
              <p style={{ margin: "4px 0", fontSize: 14 }}>
                Trạng thái: <strong>{TRANG_THAI_LABEL[job.trangThai] || job.trangThai}</strong>
                {" · "}Hạn nộp: {job.hanNopHoSo}
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
      ))}
    </div>
  );
}
