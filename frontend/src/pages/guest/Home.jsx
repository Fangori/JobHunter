import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../../api/client";

const TAG_GOI_Y = ["React", "Java", "Python", "SQL Server", "Docker", "Node.js"];

function JobCard({ job }) {
  return (
    <Link to={`/jobs/${job.maTin}`} className="card" style={{ display: "block", textDecoration: "none", color: "inherit", marginBottom: 12 }}>
      <h3 style={{ margin: "0 0 4px" }}>{job.tieuDe}</h3>
      <p style={{ color: "var(--text-muted)", margin: "0 0 8px" }}>{job.tenCongTy}</p>
      <p style={{ margin: 0, fontSize: 14 }}>
        {job.diaDiem && <span>{job.diaDiem} · </span>}
        {job.hinhThucLamViec && <span>{job.hinhThucLamViec} · </span>}
        {job.mucLuong && <span>{job.mucLuong}</span>}
      </p>
    </Link>
  );
}

export default function Home() {
  const [keyword, setKeyword] = useState("");
  const [diaDiem, setDiaDiem] = useState("");
  const [featured, setFeatured] = useState([]);
  const [results, setResults] = useState([]);
  const [searched, setSearched] = useState(false);

  useEffect(() => {
    api.get("/jobs/featured?top=6").then(setFeatured).catch(() => {});
  }, []);

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
          {results.map((job) => <JobCard key={job.maTin} job={job} />)}
        </>
      ) : (
        <>
          <h2>Việc Làm Nổi Bật</h2>
          {featured.map((job) => <JobCard key={job.maTin} job={job} />)}
        </>
      )}
    </div>
  );
}
