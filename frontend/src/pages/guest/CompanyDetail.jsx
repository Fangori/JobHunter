import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { MapPin, Globe, Users, Plus, Check } from "lucide-react";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";
import { getCompanyBanner } from "../../utils/companyBanner";

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

  const banner = getCompanyBanner(id, company.anhBia);

  return (
    <div className="page-container">
      <div
        style={{
          height: 220,
          borderRadius: "var(--radius-lg)",
          marginBottom: 24,
          backgroundImage: `linear-gradient(to bottom, rgba(26,33,64,0.15), var(--navy)), url(${banner.src})`,
          backgroundSize: "cover",
          backgroundPosition: "center",
          position: "relative",
        }}
      >
        {banner.credit && (
          <span style={{ position: "absolute", right: 12, bottom: 8, fontSize: 11, color: "rgba(255,255,255,0.7)" }}>
            Ảnh: {banner.credit}
          </span>
        )}
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "320px 1fr", gap: 24, alignItems: "start" }}>
        <div className="card" style={{ position: "sticky", top: 24 }}>
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
          <h1 style={{ fontSize: 22, margin: "0 0 8px" }}>{company.tenCongTy}</h1>
          {auth?.vaiTro === "UngVien" && (
            <button
              type="button"
              className={isFollowing ? "btn btn-secondary" : "btn btn-primary"}
              style={{ width: "100%", height: 36, display: "flex", alignItems: "center", justifyContent: "center", gap: 6, marginBottom: 12 }}
              onClick={toggleFollow}
            >
              {isFollowing ? <Check size={16} /> : <Plus size={16} />}
              {isFollowing ? "Đang theo dõi" : "Theo dõi"}
            </button>
          )}
          {followMsg && <p className={followMsg.startsWith("Đã theo dõi") ? "success-text" : "error-text"}>{followMsg}</p>}
          <div style={{ display: "flex", flexDirection: "column", gap: 8, color: "var(--text-muted)", fontSize: 14 }}>
            {company.diaChi && <span style={{ display: "inline-flex", alignItems: "center", gap: 6 }}><MapPin size={16} /> {company.diaChi}</span>}
            {company.quyMo && <span style={{ display: "inline-flex", alignItems: "center", gap: 6 }}><Users size={16} /> Quy mô {company.quyMo}</span>}
            {company.website && <a href={company.website} target="_blank" rel="noreferrer" style={{ display: "inline-flex", alignItems: "center", gap: 6 }}><Globe size={16} /> {company.website}</a>}
          </div>
          {company.gioiThieuCongTy && (
            <p style={{ whiteSpace: "pre-wrap", marginTop: 16, paddingTop: 16, borderTop: "1px solid var(--border)" }}>{company.gioiThieuCongTy}</p>
          )}
        </div>

        <div className="card">
          <h3 style={{ marginTop: 0 }}>Tin tuyển dụng đang mở</h3>
          {company.tinDangTuyen.length === 0 && <p>Chưa có tin nào.</p>}
          <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(260px, 1fr))", gap: 12 }}>
            {company.tinDangTuyen.map((job) => (
              <Link key={job.maTin} to={`/jobs/${job.maTin}`} className="card" style={{ display: "block", textDecoration: "none", color: "inherit" }}>
                <strong>{job.tieuDe}</strong>
                {job.diaDiem && <p style={{ margin: "4px 0 0", color: "var(--text-muted)", fontSize: 14 }}>{job.diaDiem}</p>}
              </Link>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
