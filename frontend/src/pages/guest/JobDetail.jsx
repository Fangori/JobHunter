import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { api } from "../../api/client";
import { useAuth } from "../../context/AuthContext";
import ApplyModal from "../candidate/ApplyModal";

export default function JobDetail() {
  const { id } = useParams();
  const [job, setJob] = useState(null);
  const [skillNames, setSkillNames] = useState({});
  const [showApply, setShowApply] = useState(false);
  const { auth } = useAuth();

  useEffect(() => {
    api.get(`/jobs/${id}`).then(setJob);
    api.get("/skills").then((skills) => {
      const map = {};
      skills.forEach((s) => (map[s.maKyNang] = s.tenKyNang));
      setSkillNames(map);
    });
  }, [id]);

  if (!job) return <div className="page-container">Đang tải...</div>;

  return (
    <div className="page-container" style={{ maxWidth: 720 }}>
      <div className="card">
        <h1 style={{ fontSize: 28 }}>{job.tieuDe}</h1>
        <p style={{ color: "var(--text-muted)" }}>{job.tenCongTy}</p>
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
      </div>

      {showApply && <ApplyModal maTin={job.maTin} onClose={() => setShowApply(false)} />}
    </div>
  );
}
