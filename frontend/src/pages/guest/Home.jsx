import { useEffect, useState } from "react";
import { api } from "../../api/client";
import { useAuth } from "../../context/AuthContext";
import JobCard from "../../components/JobCard";

const TAG_GOI_Y = ["React", "Java", "Python", "SQL Server", "Docker", "Node.js"];

export default function Home() {
  const { auth } = useAuth();
  const [keyword, setKeyword] = useState("");
  const [diaDiem, setDiaDiem] = useState("");
  const [featured, setFeatured] = useState([]);
  const [results, setResults] = useState([]);
  const [searched, setSearched] = useState(false);
  const [favoriteIds, setFavoriteIds] = useState(new Set());

  useEffect(() => {
    api.get("/jobs/featured?top=6").then(setFeatured).catch(() => {});
  }, []);

  useEffect(() => {
    if (auth?.vaiTro === "UngVien") {
      api.get("/favorites/mine", auth.token)
        .then((list) => setFavoriteIds(new Set(list.map((j) => j.maTin))))
        .catch(() => {});
    } else {
      setFavoriteIds(new Set());
    }
  }, [auth]);

  const toggleFavorite = async (maTin) => {
    const daLuu = favoriteIds.has(maTin);
    try {
      if (daLuu) {
        await api.del(`/favorites/${maTin}`, auth.token);
        setFavoriteIds((prev) => {
          const next = new Set(prev);
          next.delete(maTin);
          return next;
        });
      } else {
        await api.post(`/favorites/${maTin}`, undefined, auth.token);
        setFavoriteIds((prev) => new Set(prev).add(maTin));
      }
    } catch {
      // bo qua loi luu tin, khong chan luong duyet tin chinh
    }
  };

  const search = async (kw = keyword, dd = diaDiem) => {
    const params = new URLSearchParams();
    if (kw) params.set("keyword", kw);
    if (dd) params.set("diaDiem", dd);
    const data = await api.get(`/jobs?${params.toString()}`);
    setResults(data);
    setSearched(true);
  };

  const handleTagClick = (tag) => {
    setKeyword(tag);
    search(tag, diaDiem);
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    search();
  };

  const canFavorite = auth?.vaiTro === "UngVien";

  return (
    <div className="page-container">
      <h1 style={{ textAlign: "center" }}>Khám phá Sự nghiệp tương lai của bạn</h1>

      <form onSubmit={handleSubmit} className="card" style={{ display: "flex", gap: 8, marginBottom: 12 }}>
        <input placeholder="Từ khoá (vị trí, kỹ năng...)" value={keyword} onChange={(e) => setKeyword(e.target.value)} />
        <input placeholder="Địa điểm" value={diaDiem} onChange={(e) => setDiaDiem(e.target.value)} />
        <button className="btn btn-primary" type="submit">Tìm Việc Ngay</button>
      </form>

      <div style={{ display: "flex", gap: 8, flexWrap: "wrap", marginBottom: 32, justifyContent: "center" }}>
        {TAG_GOI_Y.map((tag) => (
          <button key={tag} type="button" className="btn btn-secondary" style={{ height: 32, padding: "0 12px" }} onClick={() => handleTagClick(tag)}>
            {tag}
          </button>
        ))}
      </div>

      {searched ? (
        <>
          <h2>Kết quả tìm kiếm ({results.length})</h2>
          {results.length === 0 && <p>Không tìm thấy việc làm phù hợp với điều kiện tìm kiếm.</p>}
          {results.map((job) => (
            <JobCard
              key={job.maTin}
              job={job}
              isFavorited={favoriteIds.has(job.maTin)}
              onToggleFavorite={canFavorite ? toggleFavorite : undefined}
            />
          ))}
        </>
      ) : (
        <>
          <h2>Việc Làm Nổi Bật</h2>
          {featured.map((job) => (
            <JobCard
              key={job.maTin}
              job={job}
              isFavorited={favoriteIds.has(job.maTin)}
              onToggleFavorite={canFavorite ? toggleFavorite : undefined}
            />
          ))}
        </>
      )}
    </div>
  );
}
