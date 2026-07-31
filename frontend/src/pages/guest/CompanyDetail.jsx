import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { MapPin, Globe, Users, Plus, Check } from "lucide-react";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

export default function CompanyDetail() {
  const { id } = useParams();
  const { auth } = useAuth();
  const [company, setCompany] = useState(null);
  const [isFollowing, setIsFollowing] = useState(false);
  const [followMsg, setFollowMsg] = useState("");

  useEffect(() => {
    api.get(`/employers/${id}`).then(setCompany);
  }, [id]);

  useEffect(() => {
    if (auth?.vaiTro === "UngVien") {
      api.get("/follows/mine", auth.token)
        .then((list) => setIsFollowing(list.some((c) => c.maTk === Number(id))))
        .catch(() => {});
    }
  }, [id, auth]);

  const toggleFollow = async () => {
    setFollowMsg("");
    try {
      if (isFollowing) {
        await api.del(`/follows/${id}`, auth.token);
        setIsFollowing(false);
      } else {
        await api.post(`/follows/${id}`, undefined, auth.token);
        setIsFollowing(true);
        setFollowMsg("Đã theo dõi công ty. Bạn sẽ nhận thông báo khi có tin tuyển dụng mới."); // MS29
      }
    } catch (err) {
      setFollowMsg(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    }
  };

  if (!company) return <div className="page-container">Đang tải...</div>;

  return (
    <div className="page-container" style={{ maxWidth: 720 }}>
      <div className="card">
        {company.logo ? (
          <img src={company.logo} alt="Logo" style={{ width: 80, height: 80, objectFit: "contain", borderRadius: "var(--radius)", marginBottom: 12 }} />
        ) : (
          <div style={{
            width: 80, height: 80, borderRadius: "var(--radius)", background: "var(--info-bg)", color: "var(--indigo-dark)",
            display: "flex", alignItems: "center", justifyContent: "center", fontWeight: 700, fontSize: 28, marginBottom: 12,
          }}>
            {company.tenCongTy?.[0]?.toUpperCase() || "?"}
          </div>
        )}
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start" }}>
          <h1 style={{ fontSize: 26 }}>{company.tenCongTy}</h1>
          {auth?.vaiTro === "UngVien" && (
            <button
              type="button"
              className={isFollowing ? "btn btn-secondary" : "btn btn-primary"}
              style={{ height: 36, padding: "0 16px", whiteSpace: "nowrap", display: "flex", alignItems: "center", gap: 6 }}
              onClick={toggleFollow}
            >
              {isFollowing ? <Check size={16} /> : <Plus size={16} />}
              {isFollowing ? "Đang theo dõi" : "Theo dõi"}
            </button>
          )}
        </div>
        {followMsg && <p className={followMsg.startsWith("Đã theo dõi") ? "success-text" : "error-text"}>{followMsg}</p>}
        <p style={{ color: "var(--text-muted)", display: "flex", flexWrap: "wrap", gap: 16, alignItems: "center" }}>
          {company.diaChi && <span style={{ display: "inline-flex", alignItems: "center", gap: 6 }}><MapPin size={16} /> {company.diaChi}</span>}
          {company.quyMo && <span style={{ display: "inline-flex", alignItems: "center", gap: 6 }}><Users size={16} /> Quy mô {company.quyMo}</span>}
          {company.website && <a href={company.website} target="_blank" rel="noreferrer" style={{ display: "inline-flex", alignItems: "center", gap: 6 }}><Globe size={16} /> {company.website}</a>}
        </p>
        {company.gioiThieuCongTy && <p style={{ whiteSpace: "pre-wrap" }}>{company.gioiThieuCongTy}</p>}

        <h3 style={{ marginTop: 24 }}>Tin tuyển dụng đang mở</h3>
        {company.tinDangTuyen.length === 0 && <p>Chưa có tin nào.</p>}
        {company.tinDangTuyen.map((job) => (
          <Link key={job.maTin} to={`/jobs/${job.maTin}`} className="card" style={{ display: "block", marginBottom: 8, textDecoration: "none", color: "inherit" }}>
            <strong>{job.tieuDe}</strong>
            {job.diaDiem && <span> — {job.diaDiem}</span>}
          </Link>
        ))}
      </div>
    </div>
  );
}
