import { useEffect, useState } from "react";
import { ApiError, BASE_URL } from "../../api/client";
import { useAuth } from "../../context/AuthContext";
import FileUpload from "../../components/FileUpload";

// BM05
export default function Profile() {
  const { auth } = useAuth();
  const [form, setForm] = useState({ hoTen: "", ngaySinh: "", sdt: "", diaChi: "", gioiThieuBanThan: "" });
  const [anhDaiDien, setAnhDaiDien] = useState(null);
  const [anhHienTai, setAnhHienTai] = useState(null);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);
  const [dangTai, setDangTai] = useState(true);

  useEffect(() => {
    fetch(`${BASE_URL}/candidates/me`, { headers: { Authorization: `Bearer ${auth.token}` } })
      .then((r) => r.json())
      .then((data) => {
        setForm({
          hoTen: data.hoTen || "", ngaySinh: data.ngaySinh || "", sdt: data.sdt || "",
          diaChi: data.diaChi || "", gioiThieuBanThan: data.gioiThieuBanThan || "",
        });
        setAnhHienTai(data.anhDaiDien);
      })
      // Chi hien form SAU KHI fetch xong - tranh rang buoc du lieu vua go
      // (rieng cua nguoi dung) bi setForm ben tren de len neu nguoi dung
      // go qua nhanh truoc khi fetch tra ve (bug that bat duoc luc test
      // Playwright: go form ngay sau khi trang mount, fetch tra ve sau
      // va xoa mat du lieu vua go).
      .finally(() => setDangTai(false));
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
      if (anhDaiDien) fd.append("anhDaiDien", anhDaiDien);
      const res = await fetch(`${BASE_URL}/candidates/me`, {
        method: "PUT",
        headers: { Authorization: `Bearer ${auth.token}` },
        body: fd,
      });
      const data = await res.json();
      if (!res.ok) throw new ApiError(data.message, res.status);
      setAnhHienTai(data.anhDaiDien);
      setSuccess("Cập nhật thông tin cá nhân thành công."); // MS24
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra."); // MS53
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page-container">
      <div className="dashboard-header-band">
        <h2>Thông tin cá nhân</h2>
      </div>
      {dangTai && <div className="card"><p>Đang tải...</p></div>}
      {!dangTai && (
        <div style={{ display: "grid", gridTemplateColumns: "280px 1fr", gap: 24, alignItems: "start" }}>
          <div className="card" style={{ textAlign: "center" }}>
            {anhHienTai ? (
              <img src={anhHienTai} alt="Ảnh đại diện" style={{ width: 120, height: 120, borderRadius: "50%", objectFit: "cover", margin: "0 auto 16px" }} />
            ) : (
              <div style={{
                width: 120, height: 120, borderRadius: "50%", background: "var(--info-bg)", color: "var(--indigo-dark)",
                display: "flex", alignItems: "center", justifyContent: "center", fontWeight: 700, fontSize: 40, margin: "0 auto 16px",
              }}>
                {form.hoTen?.[0]?.toUpperCase() || "?"}
              </div>
            )}
            <p style={{ fontWeight: 700, fontSize: 18, margin: "0 0 4px" }}>{form.hoTen || "Chưa đặt tên"}</p>
            <p style={{ color: "var(--text-muted)", margin: 0 }}>Ứng viên</p>
          </div>

          <div className="card">
            <form onSubmit={handleSubmit}>
              <FileUpload
                label="Ảnh đại diện"
                accept=".jpg,.jpeg,.png"
                variant="avatar"
                value={anhDaiDien}
                existingUrl={anhHienTai}
                onChange={setAnhDaiDien}
              />
              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 16 }}>
                <div className="field">
                  <label>Họ và tên</label>
                  <input value={form.hoTen} onChange={set("hoTen")} required />
                </div>
                <div className="field">
                  <label>Số điện thoại</label>
                  <input value={form.sdt} onChange={set("sdt")} />
                </div>
                <div className="field">
                  <label>Ngày sinh</label>
                  <input type="date" value={form.ngaySinh} onChange={set("ngaySinh")} />
                </div>
                <div className="field">
                  <label>Địa chỉ</label>
                  <input value={form.diaChi} onChange={set("diaChi")} />
                </div>
              </div>
              <div className="field">
                <label>Giới thiệu bản thân</label>
                <textarea rows={4} value={form.gioiThieuBanThan} onChange={set("gioiThieuBanThan")} />
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
