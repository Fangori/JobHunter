import { Link } from "react-router-dom";
import { Heart, MapPin, Wallet, Clock } from "lucide-react";

function thoiGianTruoc(ngayDang) {
  const giay = Math.floor((Date.now() - new Date(ngayDang).getTime()) / 1000);
  if (giay < 3600) return `${Math.max(1, Math.floor(giay / 60))} phút trước`;
  if (giay < 86400) return `${Math.floor(giay / 3600)} giờ trước`;
  return `${Math.floor(giay / 86400)} ngày trước`;
}

export default function JobCard({ job, isFavorited, onToggleFavorite, matchPercent }) {
  return (
    <div className="card" style={{ marginBottom: 12, position: "relative" }}>
      {matchPercent != null && (
        <span
          title="Mức độ phù hợp với CV của bạn"
          style={{
            position: "absolute", top: 12, right: onToggleFavorite ? 44 : 12,
            background: "var(--indigo)", color: "white", borderRadius: 999,
            fontSize: 12, fontWeight: 700, padding: "2px 8px",
          }}
        >
          {matchPercent}% phù hợp
        </span>
      )}
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
            cursor: "pointer", display: "flex", color: isFavorited ? "#e0245e" : "var(--text-muted)",
          }}
        >
          <Heart size={20} fill={isFavorited ? "#e0245e" : "none"} />
        </button>
      )}
      <Link to={`/jobs/${job.maTin}`} style={{ display: "flex", gap: 14, textDecoration: "none", color: "inherit" }}>
        {job.logo ? (
          <img
            src={job.logo}
            alt={job.tenCongTy}
            style={{ flexShrink: 0, width: 48, height: 48, borderRadius: "var(--radius)", objectFit: "contain", background: "var(--info-bg)" }}
          />
        ) : (
          <div style={{
            flexShrink: 0, width: 48, height: 48, borderRadius: "var(--radius)", background: "var(--info-bg)",
            color: "var(--indigo-dark)", display: "flex", alignItems: "center", justifyContent: "center",
            fontWeight: 700, fontSize: 18,
          }}>
            {job.tenCongTy?.[0]?.toUpperCase() || "?"}
          </div>
        )}
        <div style={{ flex: 1, minWidth: 0 }}>
          <h3 style={{ margin: "0 0 4px", paddingRight: (onToggleFavorite ? 28 : 0) + (matchPercent != null ? 90 : 0) }}>{job.tieuDe}</h3>
          <p style={{ color: "var(--text-muted)", margin: "0 0 8px" }}>{job.tenCongTy}</p>
          <p style={{ margin: 0, fontSize: 14, color: "var(--text-muted)", display: "flex", flexWrap: "wrap", gap: 12, alignItems: "center" }}>
            {job.diaDiem && <span style={{ display: "inline-flex", alignItems: "center", gap: 4 }}><MapPin size={14} /> {job.diaDiem}</span>}
            {job.mucLuong && <span style={{ display: "inline-flex", alignItems: "center", gap: 4 }}><Wallet size={14} /> {job.mucLuong}</span>}
            {job.hinhThucLamViec && <span>{job.hinhThucLamViec}</span>}
            {job.ngayDang && <span style={{ display: "inline-flex", alignItems: "center", gap: 4 }}><Clock size={14} /> {thoiGianTruoc(job.ngayDang)}</span>}
          </p>
        </div>
      </Link>
    </div>
  );
}
