import { Link } from "react-router-dom";
import { Heart, MapPin, Wallet, Clock } from "lucide-react";

function thoiGianTruoc(ngayDang) {
  const giay = Math.floor((Date.now() - new Date(ngayDang).getTime()) / 1000);
  if (giay < 3600) return `${Math.max(1, Math.floor(giay / 60))} phút trước`;
  if (giay < 86400) return `${Math.floor(giay / 3600)} giờ trước`;
  return `${Math.floor(giay / 86400)} ngày trước`;
}

// The bo doc, dat trong luoi nhieu cot (xem Home.jsx/Favorites.jsx) thay vi
// 1 hang ngang toan chieu rong nhu truoc - do trong 1 the it thong tin, xep
// hang ngang de lai nhieu khoang trong ben phai tren man hinh rong.
export default function JobCard({ job, isFavorited, onToggleFavorite, matchPercent }) {
  return (
    <div className="card" style={{ position: "relative", height: "100%", display: "flex", flexDirection: "column", paddingTop: 40 }}>
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
      <Link to={`/jobs/${job.maTin}`} style={{ display: "flex", flexDirection: "column", gap: 10, flex: 1, textDecoration: "none", color: "inherit" }}>
        <div style={{ display: "flex", alignItems: "center", gap: 10, minWidth: 0 }}>
          {job.logo ? (
            <img
              src={job.logo}
              alt={job.tenCongTy}
              style={{ flexShrink: 0, width: 40, height: 40, borderRadius: "var(--radius)", objectFit: "contain", background: "var(--info-bg)" }}
            />
          ) : (
            <div style={{
              flexShrink: 0, width: 40, height: 40, borderRadius: "var(--radius)", background: "var(--info-bg)",
              color: "var(--indigo-dark)", display: "flex", alignItems: "center", justifyContent: "center",
              fontWeight: 700, fontSize: 16,
            }}>
              {job.tenCongTy?.[0]?.toUpperCase() || "?"}
            </div>
          )}
          <p style={{ color: "var(--text-muted)", margin: 0, fontSize: 13, overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>{job.tenCongTy}</p>
        </div>

        <h3 style={{ margin: 0, fontSize: 16, lineHeight: 1.35 }}>{job.tieuDe}</h3>

        <div style={{ marginTop: "auto", display: "flex", flexDirection: "column", gap: 6, fontSize: 13, color: "var(--text-muted)", paddingTop: 8 }}>
          {job.diaDiem && <span style={{ display: "inline-flex", alignItems: "center", gap: 6 }}><MapPin size={14} /> {job.diaDiem}</span>}
          {job.mucLuong && <span style={{ display: "inline-flex", alignItems: "center", gap: 6 }}><Wallet size={14} /> {job.mucLuong}</span>}
          <span style={{ display: "flex", alignItems: "center", gap: 10, flexWrap: "wrap" }}>
            {job.hinhThucLamViec && <span className="badge badge-info">{job.hinhThucLamViec}</span>}
            {job.ngayDang && <span style={{ display: "inline-flex", alignItems: "center", gap: 4 }}><Clock size={14} /> {thoiGianTruoc(job.ngayDang)}</span>}
          </span>
        </div>
      </Link>
    </div>
  );
}
