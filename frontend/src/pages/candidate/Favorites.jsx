import { useEffect, useState } from "react";
import { api } from "../../api/client";
import { useAuth } from "../../context/AuthContext";
import JobCard from "../../components/JobCard";

export default function Favorites() {
  const { auth } = useAuth();
  const [jobs, setJobs] = useState(null);
  const [loadError, setLoadError] = useState(false);

  const load = () => {
    setLoadError(false);
    api.get("/favorites/mine", auth.token).then(setJobs).catch(() => setLoadError(true));
  };

  useEffect(() => {
    load();
  }, []);

  const toggleFavorite = async (maTin) => {
    await api.del(`/favorites/${maTin}`, auth.token);
    load();
  };

  if (loadError) {
    return (
      <div className="page-container">
        <h1>Tin đã lưu</h1>
        <p className="error-text">Không tải được dữ liệu.</p>
        <button type="button" className="btn btn-secondary" onClick={load}>Thử lại</button>
      </div>
    );
  }

  if (!jobs) return <div className="page-container">Đang tải...</div>;

  return (
    <div className="page-container">
      <div className="dashboard-header-band">
        <h1>Tin đã lưu</h1>
        <p>Các tin bạn đã lưu để xem lại sau.</p>
      </div>
      {jobs.length === 0 && <p>Bạn chưa lưu tin nào.</p>}
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(300px, 1fr))", gap: 16 }}>
        {jobs.map((job) => (
          <JobCard key={job.maTin} job={job} isFavorited onToggleFavorite={toggleFavorite} />
        ))}
      </div>
    </div>
  );
}
