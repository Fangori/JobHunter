import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
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
    <div className="page-container" style={{ maxWidth: 720 }}>
      <div className="card">
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start" }}>
          <h1 style={{ fontSize: 28 }}>{job.tieuDe}</h1>
          {auth?.vaiTro === "UngVien" && (
            <button
              type="button"
              onClick={toggleFavorite}
              title={isFavorited ? "Bỏ lưu tin" : "Lưu tin"}
              style={{ border: "none", background: "transparent", cursor: "pointer", fontSize: 26, color: isFavorited ? "#e0245e" : "var(--text-muted)" }}
            >
              {isFavorited ? "♥" : "♡"}
            </button>
          )}
        </div>
        {favMsg && <p className={favMsg === "Đã lưu tin vào danh sách yêu thích." ? "success-text" : "error-text"}>{favMsg}</p>}
        <p style={{ color: "var(--text-muted)" }}>
          <Link to={`/companies/${job.maTkNtd}`}>{job.tenCongTy}</Link>
        </p>
        <p>
          {job.diaDiem && <span>{job.diaDiem} · </span>}
          {job.hinhThucLamViec && <span>{job.hinhThucLamViec} · </span>}
          {job.mucLuong && <span>{job.mucLuong}</span>}
        </p>
        <p>Hạn nộp hồ sơ: {job.hanNopHoSo}</p>

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
            <span key={k.maKyNang} className="btn btn-secondary" style={{ height: 32, padding: "0 12px", cursor: "default" }}>
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
