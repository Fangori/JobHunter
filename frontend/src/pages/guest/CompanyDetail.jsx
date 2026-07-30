import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { api } from "../../api/client";

// Nut "Theo doi" (UC15) se them o Phase 8 khi co API that - khong lam
// nut gia khong goi duoc backend o day.
export default function CompanyDetail() {
  const { id } = useParams();
  const [company, setCompany] = useState(null);

  useEffect(() => {
    api.get(`/employers/${id}`).then(setCompany);
  }, [id]);

  if (!company) return <div className="page-container">Đang tải...</div>;

  return (
    <div className="page-container" style={{ maxWidth: 720 }}>
      <div className="card">
        {company.logo && <img src={company.logo} alt="Logo" style={{ width: 80, height: 80, objectFit: "contain", marginBottom: 12 }} />}
        <h1 style={{ fontSize: 26 }}>{company.tenCongTy}</h1>
        <p style={{ color: "var(--text-muted)" }}>
          {company.diaChi && <span>{company.diaChi} · </span>}
          {company.quyMo && <span>Quy mô {company.quyMo} · </span>}
          {company.website && <a href={company.website} target="_blank" rel="noreferrer">{company.website}</a>}
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
