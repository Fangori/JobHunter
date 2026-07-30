import { Link } from "react-router-dom";

export default function JobCard({ job, isFavorited, onToggleFavorite }) {
  return (
    <div className="card" style={{ marginBottom: 12, position: "relative" }}>
      {onToggleFavorite && (
        <button
          type="button"
          onClick={(e) => {
            e.preventDefault();
            onToggleFavorite(job.maTin);
          }}
          title={isFavorited ? "Bỏ lưu tin" : "Lưu tin"}
          style={{
            position: "absolute", top: 12, right: 12, border: "none", background: "transparent",
            cursor: "pointer", fontSize: 22, lineHeight: 1, color: isFavorited ? "#e0245e" : "var(--text-muted)",
          }}
        >
          {isFavorited ? "♥" : "♡"}
        </button>
      )}
      <Link to={`/jobs/${job.maTin}`} style={{ display: "block", textDecoration: "none", color: "inherit" }}>
        <h3 style={{ margin: "0 0 4px", paddingRight: onToggleFavorite ? 28 : 0 }}>{job.tieuDe}</h3>
        <p style={{ color: "var(--text-muted)", margin: "0 0 8px" }}>{job.tenCongTy}</p>
        <p style={{ margin: 0, fontSize: 14 }}>
          {job.diaDiem && <span>{job.diaDiem} · </span>}
          {job.hinhThucLamViec && <span>{job.hinhThucLamViec} · </span>}
          {job.mucLuong && <span>{job.mucLuong}</span>}
        </p>
      </Link>
    </div>
  );
}
