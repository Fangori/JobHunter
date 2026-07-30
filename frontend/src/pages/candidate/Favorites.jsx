import { useEffect, useState } from "react";
import { api } from "../../api/client";
import { useAuth } from "../../context/AuthContext";
import JobCard from "../../components/JobCard";

export default function Favorites() {
  const { auth } = useAuth();
  const [jobs, setJobs] = useState(null);

  const load = () => api.get("/favorites/mine", auth.token).then(setJobs);

  useEffect(() => {
    load();
  }, []);

  const toggleFavorite = async (maTin) => {
    await api.del(`/favorites/${maTin}`, auth.token);
    load();
  };

  if (!jobs) return <div className="page-container">Đang tải...</div>;

  return (
    <div className="page-container" style={{ maxWidth: 720 }}>
      <h1>Tin đã lưu</h1>
      {jobs.length === 0 && <p>Bạn chưa lưu tin nào.</p>}
      {jobs.map((job) => (
        <JobCard key={job.maTin} job={job} isFavorited onToggleFavorite={toggleFavorite} />
      ))}
    </div>
  );
}
