import { useEffect, useState } from "react";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

const BASE_URL = "http://localhost:5147/api";

// BM08
export default function CompanyProfile() {
  const { auth } = useAuth();
  const [industries, setIndustries] = useState([]);
  const [form, setForm] = useState({ tenCongTy: "", quyMo: "", maNganhNghe: "", diaChi: "", website: "", gioiThieuCongTy: "" });
  const [logo, setLogo] = useState(null);
  const [logoHienTai, setLogoHienTai] = useState(null);
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
      const res = await fetch(`${BASE_URL}/employers/me`, {
        method: "PUT",
        headers: { Authorization: `Bearer ${auth.token}` },
        body: fd,
      });
      const data = await res.json();
      if (!res.ok) throw new ApiError(data.message, res.status);
      setLogoHienTai(data.logo);
      setSuccess("Cập nhật hồ sơ công ty thành công."); // MS25
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra."); // MS60
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page-container" style={{ maxWidth: 520 }}>
      <div className="card">
        <h2>Hồ sơ công ty</h2>
        {dangTai && <p>Đang tải...</p>}
        {!dangTai && (
        <>
        {logoHienTai && <img src={logoHienTai} alt="Logo" style={{ width: 96, height: 96, objectFit: "contain", marginBottom: 16 }} />}
        <form onSubmit={handleSubmit}>
          <div className="field">
            <label>Tên công ty</label>
            <input value={form.tenCongTy} onChange={set("tenCongTy")} required />
          </div>
          <div className="field">
            <label>Logo</label>
            <input type="file" accept=".jpg,.jpeg,.png" onChange={(e) => setLogo(e.target.files[0])} />
          </div>
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
          <div className="field">
            <label>Giới thiệu công ty</label>
            <textarea rows={4} value={form.gioiThieuCongTy} onChange={set("gioiThieuCongTy")} />
          </div>
          {error && <p className="error-text">{error}</p>}
          {success && <p className="success-text">{success}</p>}
          <button className="btn btn-primary" style={{ width: "100%" }} disabled={loading} type="submit">
            {loading ? "Đang lưu..." : "Lưu"}
          </button>
        </form>
        </>
        )}
      </div>
    </div>
  );
}
