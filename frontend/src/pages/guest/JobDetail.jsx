import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { MapPin, Briefcase, Wallet, CalendarDays, Heart, Users } from "lucide-react";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";
import ApplyModal from "../candidate/ApplyModal";
import { getCompanyBanner } from "../../utils/companyBanner";

export default function JobDetail() {
  const { id } = useParams();
  const [job, setJob] = useState(null);
  const [skillNames, setSkillNames] = useState({});
  const [showApply, setShowApply] = useState(false);
  const [isFavorited, setIsFavorited] = useState(false);
  const [favMsg, setFavMsg] = useState("");
  const { auth } = useAuth();

  useEffect(() => {
    api.get(`/jobs/${id}`).then(setJob);
    api.get("/skills").then((skills) => {
      const map = {};
      skills.forEach((s) => (map[s.maKyNang] = s.tenKyNang));
      setSkillNames(map);
    });
  }, [id]);

  useEffect(() => {
    if (auth?.vaiTro === "UngVien") {
      api.get("/favorites/mine", auth.token)
        .then((list) => setIsFavorited(list.some((j) => j.maTin === Number(id))))
        .catch(() => {});
    }
  }, [id, auth]);

  const toggleFavorite = async () => {
    setFavMsg("");
    try {
      if (isFavorited) {
        await api.del(`/favorites/${id}`, auth.token);
        setIsFavorited(false);
      } else {
        await api.post(`/favorites/${id}`, undefined, auth.token);
        setIsFavorited(true);
        setFavMsg("Đã lưu tin vào danh sách yêu thích."); // MS27
      }
    } catch (err) {
      setFavMsg(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    }
  };

  if (!job) return <div className="page-container">Đang tải...</div>;

  const banner = getCompanyBanner(job.maTkNtd);

  return (
    <div className="page-container">
      <div
        style={{
          height: 160,
          borderRadius: "var(--radius-lg)",
          marginBottom: 20,
          backgroundImage: `linear-gradient(to bottom, rgba(26,33,64,0.15), var(--navy)), url(${banner.src})`,
          backgroundSize: "cover",
          backgroundPosition: "center",
          position: "relative",
        }}
      >
        <span style={{ position: "absolute", right: 12, bottom: 8, fontSize: 11, color: "rgba(255,255,255,0.7)" }}>
          Ảnh: {banner.credit}
        </span>
      </div>
      <div style={{ display: "grid", gridTemplateColumns: "1fr 340px", gap: 24, alignItems: "start" }}>
        <div className="card">
          <h1 style={{ fontSize: 26, margin: "0 0 8px" }}>{job.tieuDe}</h1>
          <Link to={`/companies/${job.maTkNtd}`} style={{ color: "var(--text-muted)" }}>{job.tenCongTy}</Link>
          {favMsg && <p className={favMsg === "Đã lưu tin vào danh sách yêu thích." ? "success-text" : "error-text"}>{favMsg}</p>}

          <div style={{ display: "flex", gap: 20, flexWrap: "wrap", margin: "16px 0", fontSize: 14, color: "var(--text-muted)" }}>
            {job.diaDiem && <span style={{ display: "inline-flex", alignItems: "center", gap: 6 }}><MapPin size={16} /> {job.diaDiem}</span>}
            {job.hinhThucLamViec && <span style={{ display: "inline-flex", alignItems: "center", gap: 6 }}><Briefcase size={16} /> {job.hinhThucLamViec}</span>}
            {job.mucLuong && <span style={{ display: "inline-flex", alignItems: "center", gap: 6 }}><Wallet size={16} /> {job.mucLuong}</span>}
            {job.soLuongTuyen && <span style={{ display: "inline-flex", alignItems: "center", gap: 6 }}><Users size={16} /> Tuyển {job.soLuongTuyen} người</span>}
            <span style={{ display: "inline-flex", alignItems: "center", gap: 6 }}><CalendarDays size={16} /> Hạn nộp: {job.hanNopHoSo}</span>
          </div>

          <h3>Mô tả công việc</h3>
          <p style={{ whiteSpace: "pre-wrap" }}>{job.moTaCongViec}</p>

          {job.yeuCauUngVien && (
            <>
              <h3>Yêu cầu ứng viên</h3>
              <p style={{ whiteSpace: "pre-wrap" }}>{job.yeuCauUngVien}</p>
            </>
          )}
          {job.quyenLoi && (
            <>
              <h3>Quyền lợi</h3>
              <p style={{ whiteSpace: "pre-wrap" }}>{job.quyenLoi}</p>
            </>
          )}

          <h3>Kỹ năng yêu cầu</h3>
          <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
            {job.kyNangYeuCau.map((k) => (
              <span key={k.maKyNang} className="badge badge-info">
                {skillNames[k.maKyNang] || k.maKyNang}
                {k.mucDoUuTien === "BatBuoc" ? " (Bắt buộc)" : k.mucDoUuTien === "UuTien" ? " (Ưu tiên)" : ""}
              </span>
            ))}
          </div>
        </div>

        <div className="card" style={{ position: "sticky", top: 24 }}>
          <div style={{ display: "flex", gap: 12, alignItems: "center", marginBottom: 16 }}>
            {job.logo ? (
              <img
                src={job.logo}
                alt={job.tenCongTy}
                style={{ flexShrink: 0, width: 56, height: 56, borderRadius: "var(--radius)", objectFit: "contain", background: "var(--info-bg)" }}
              />
            ) : (
              <div style={{
                flexShrink: 0, width: 56, height: 56, borderRadius: "var(--radius)", background: "var(--info-bg)",
                color: "var(--indigo-dark)", display: "flex", alignItems: "center", justifyContent: "center",
                fontWeight: 700, fontSize: 22,
              }}>
                {job.tenCongTy?.[0]?.toUpperCase() || "?"}
              </div>
            )}
            <div>
              <Link to={`/companies/${job.maTkNtd}`} style={{ fontWeight: 700, display: "block" }}>{job.tenCongTy}</Link>
              <span style={{ color: "var(--text-muted)", fontSize: 13 }}>Xem hồ sơ công ty</span>
            </div>
          </div>

          {job.mucLuong && (
            <p style={{ fontWeight: 700, color: "var(--indigo)", fontSize: 20, margin: "0 0 16px" }}>{job.mucLuong}</p>
          )}

          <div style={{ display: "flex", gap: 8 }}>
            {auth?.vaiTro === "UngVien" && (
              <>
                <button className="btn btn-primary" style={{ flex: 1 }} onClick={() => setShowApply(true)}>
                  Ứng tuyển ngay
                </button>
                <button
                  type="button"
                  onClick={toggleFavorite}
                  title={isFavorited ? "Bỏ lưu tin" : "Lưu tin"}
                  style={{ border: "1px solid var(--border)", borderRadius: "var(--radius)", background: "transparent", cursor: "pointer", display: "flex", alignItems: "center", justifyContent: "center", padding: "0 12px", color: isFavorited ? "#e0245e" : "var(--text-muted)" }}
                >
                  <Heart size={20} fill={isFavorited ? "#e0245e" : "none"} />
                </button>
              </>
            )}
            {auth?.vaiTro === "NhaTuyenDung" && (
              <Link to={`/employer/jobs/${job.maTin}/applicants`} className="btn btn-primary" style={{ flex: 1, textAlign: "center" }}>
                Xem danh sách ứng viên
              </Link>
            )}
            {!auth && (
              <Link to={`/login?redirect=/jobs/${job.maTin}`} className="btn btn-primary" style={{ flex: 1, textAlign: "center" }}>
                Đăng nhập để ứng tuyển
              </Link>
            )}
          </div>
          {!auth && (
            <p style={{ margin: "10px 0 0", fontSize: 13, color: "var(--text-muted)", textAlign: "center" }}>
              Chưa có tài khoản? <Link to="/register">Đăng ký ngay</Link>
            </p>
          )}
        </div>
      </div>

      {showApply && <ApplyModal maTin={job.maTin} onClose={() => setShowApply(false)} />}
    </div>
  );
}
