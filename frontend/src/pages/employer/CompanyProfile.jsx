import { useEffect, useState } from "react";
import { api, ApiError, BASE_URL } from "../../api/client";
import { useAuth } from "../../context/AuthContext";
import FileUpload from "../../components/FileUpload";
import { getCompanyBanner } from "../../utils/companyBanner";
import { decodeJwtMaTk } from "../../utils/jwt";

// BM08
export default function CompanyProfile() {
  const { auth } = useAuth();
  const [industries, setIndustries] = useState([]);
  const [form, setForm] = useState({ tenCongTy: "", quyMo: "", maNganhNghe: "", diaChi: "", website: "", gioiThieuCongTy: "" });
  const [logo, setLogo] = useState(null);
  const [logoHienTai, setLogoHienTai] = useState(null);
  const [anhBia, setAnhBia] = useState(null);
  const [anhBiaHienTai, setAnhBiaHienTai] = useState(null);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);
  const [dangTai, setDangTai] = useState(true);

  useEffect(() => {
    Promise.all([
      api.get("/industries").then(setIndustries),
      fetch(`${BASE_URL}/employers/me`, { headers: { Authorization: `Bearer ${auth.token}` } })
        .then((r) => r.json())
        .then((data) => {
          setForm({
            tenCongTy: data.tenCongTy || "", quyMo: data.quyMo || "", maNganhNghe: data.maNganhNghe || "",
            diaChi: data.diaChi || "", website: data.website || "", gioiThieuCongTy: data.gioiThieuCongTy || "",
          });
          setLogoHienTai(data.logo);
          setAnhBiaHienTai(data.anhBia);
        }),
    ]).finally(() => setDangTai(false)); // chi hien form sau khi ca 2 fetch xong, tranh
    // rang buoc du lieu vua go bi de len (cung 1 bug da bat duoc o Profile.jsx)
  }, []);

  const set = (key) => (e) => setForm({ ...form, [key]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setSuccess("");
    setLoading(true);
    try {
      const fd = new FormData();
      Object.entries(form).forEach(([k, v]) => fd.append(k, v ?? ""));
      if (logo) fd.append("logo", logo);
      if (anhBia) fd.append("anhBia", anhBia);
      const res = await fetch(`${BASE_URL}/employers/me`, {
        method: "PUT",
        headers: { Authorization: `Bearer ${auth.token}` },
        body: fd,
      });
      const data = await res.json();
      if (!res.ok) throw new ApiError(data.message, res.status);
      setLogoHienTai(data.logo);
      setAnhBiaHienTai(data.anhBia);
      setAnhBia(null);
      setSuccess("Cập nhật hồ sơ công ty thành công."); // MS25
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra."); // MS60
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page-container">
      <div className="dashboard-header-band">
        <h2>Hồ sơ công ty</h2>
      </div>

      {/* Anh bia - NTD tu upload qua Cloudinary (UC08, cot AnhBia moi them
          qua migration, cung co che voi Logo). Chua upload thi hien banner
          mac dinh xoay vong theo MaTK nhu truoc (tuong thich nguoc). */}
      {!dangTai && (
        <div style={{ marginBottom: 24 }}>
          <FileUpload
            label="Ảnh bìa công ty"
            accept=".jpg,.jpeg,.png"
            variant="banner"
            value={anhBia}
            existingUrl={anhBiaHienTai || getCompanyBanner(decodeJwtMaTk(auth.token)).src}
            onChange={setAnhBia}
          />
          {!anhBia && !anhBiaHienTai && (
            <p style={{ fontSize: 12, color: "var(--text-muted)", margin: "4px 0 0" }}>
              Ảnh: {getCompanyBanner(decodeJwtMaTk(auth.token)).credit} (ảnh mặc định - chưa tải ảnh bìa riêng)
            </p>
          )}
        </div>
      )}

      {dangTai && <div className="card"><p>Đang tải...</p></div>}

      {!dangTai && (
        <div style={{ display: "grid", gridTemplateColumns: "280px 1fr", gap: 24, alignItems: "start" }}>
          <div className="card" style={{ textAlign: "center" }}>
            {logoHienTai ? (
              <img src={logoHienTai} alt="Logo công ty" style={{ width: 120, height: 120, borderRadius: "var(--radius)", objectFit: "contain", margin: "0 auto 16px" }} />
            ) : (
              <div style={{
                width: 120, height: 120, borderRadius: "var(--radius)", background: "var(--info-bg)", color: "var(--indigo-dark)",
                display: "flex", alignItems: "center", justifyContent: "center", fontWeight: 700, fontSize: 40, margin: "0 auto 16px",
              }}>
                {form.tenCongTy?.[0]?.toUpperCase() || "?"}
              </div>
            )}
            <p style={{ fontWeight: 700, fontSize: 18, margin: "0 0 4px" }}>{form.tenCongTy || "Chưa đặt tên"}</p>
            <p style={{ color: "var(--text-muted)", margin: 0 }}>Nhà tuyển dụng</p>
          </div>

          <div className="card">
            <form onSubmit={handleSubmit}>
              <FileUpload
                label="Logo công ty"
                accept=".jpg,.jpeg,.png"
                variant="avatar"
                value={logo}
                existingUrl={logoHienTai}
                onChange={setLogo}
              />
              <div className="field">
                <label>Tên công ty</label>
                <input value={form.tenCongTy} onChange={set("tenCongTy")} required />
              </div>
              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 16 }}>
                <div className="field">
                  <label>Quy mô</label>
                  <select value={form.quyMo} onChange={set("quyMo")}>
                    <option value="">-- Chọn --</option>
                    <option value="<50">&lt;50</option>
                    <option value="50-200">50-200</option>
                    <option value="200-500">200-500</option>
                    <option value=">500">&gt;500</option>
                  </select>
                </div>
                <div className="field">
                  <label>Lĩnh vực hoạt động</label>
                  <select value={form.maNganhNghe} onChange={set("maNganhNghe")}>
                    <option value="">-- Chọn --</option>
                    {industries.map((i) => <option key={i.maNganhNghe} value={i.maNganhNghe}>{i.tenNganhNghe}</option>)}
                  </select>
                </div>
                <div className="field">
                  <label>Địa chỉ</label>
                  <input value={form.diaChi} onChange={set("diaChi")} />
                </div>
                <div className="field">
                  <label>Website</label>
                  <input value={form.website} onChange={set("website")} />
                </div>
              </div>
              <div className="field">
                <label>Giới thiệu công ty</label>
                <textarea rows={6} value={form.gioiThieuCongTy} onChange={set("gioiThieuCongTy")} />
              </div>
              {error && <p className="error-text">{error}</p>}
              {success && <p className="success-text">{success}</p>}
              <button className="btn btn-primary" style={{ width: "100%" }} disabled={loading} type="submit">
                {loading ? "Đang lưu..." : "Lưu"}
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
