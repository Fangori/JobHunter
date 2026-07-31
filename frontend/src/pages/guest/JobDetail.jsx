import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { MapPin, Briefcase, Wallet, CalendarDays, Heart } from "lucide-react";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";
import ApplyModal from "../candidate/ApplyModal";

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

  return (
    <div className="page-container" style={{ maxWidth: 760 }}>
      <div className="card">
        <div style={{ display: "flex", gap: 16, justifyContent: "space-between", alignItems: "flex-start", flexWrap: "wrap" }}>
          <div style={{ display: "flex", gap: 14 }}>
            <div style={{
              flexShrink: 0, width: 56, height: 56, borderRadius: "var(--radius)", background: "var(--info-bg)",
              color: "var(--indigo-dark)", display: "flex", alignItems: "center", justifyContent: "center",
              fontWeight: 700, fontSize: 22,
            }}>
              {job.tenCongTy?.[0]?.toUpperCase() || "?"}
            </div>
            <div>
              <h1 style={{ fontSize: 26, margin: "0 0 4px" }}>{job.tieuDe}</h1>
              <Link to={`/companies/${job.maTkNtd}`} style={{ color: "var(--text-muted)" }}>{job.tenCongTy}</Link>
            </div>
          </div>
          <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
            {job.mucLuong && <span style={{ fontWeight: 700, color: "var(--indigo)", fontSize: 18 }}>{job.mucLuong}</span>}
            {auth?.vaiTro === "UngVien" && (
              <button
                type="button"
                onClick={toggleFavorite}
                title={isFavorited ? "Bỏ lưu tin" : "Lưu tin"}
                style={{ border: "1px solid var(--border)", borderRadius: "var(--radius)", background: "transparent", cursor: "pointer", display: "flex", padding: 8, color: isFavorited ? "#e0245e" : "var(--text-muted)" }}
              >
                <Heart size={20} fill={isFavorited ? "#e0245e" : "none"} />
              </button>
            )}
          </div>
        </div>
        {favMsg && <p className={favMsg === "Đã lưu tin vào danh sách yêu thích." ? "success-text" : "error-text"}>{favMsg}</p>}

        <div style={{ display: "flex", gap: 20, flexWrap: "wrap", margin: "16px 0", fontSize: 14, color: "var(--text-muted)" }}>
          {job.diaDiem && <span style={{ display: "inline-flex", alignItems: "center", gap: 6 }}><MapPin size={16} /> {job.diaDiem}</span>}
          {job.hinhThucLamViec && <span style={{ display: "inline-flex", alignItems: "center", gap: 6 }}><Briefcase size={16} /> {job.hinhThucLamViec}</span>}
          {job.mucLuong && <span style={{ display: "inline-flex", alignItems: "center", gap: 6 }}><Wallet size={16} /> {job.mucLuong}</span>}
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

        {auth?.vaiTro === "UngVien" && (
          <button className="btn btn-primary" style={{ marginTop: 24 }} onClick={() => setShowApply(true)}>
            Ứng tuyển ngay
          </button>
        )}
        {auth?.vaiTro === "NhaTuyenDung" && (
          <Link to={`/employer/jobs/${job.maTin}/applicants`} className="btn btn-primary" style={{ marginTop: 24 }}>
            Xem danh sách ứng viên
          </Link>
        )}
      </div>

      {showApply && <ApplyModal maTin={job.maTin} onClose={() => setShowApply(false)} />}
    </div>
  );
}
