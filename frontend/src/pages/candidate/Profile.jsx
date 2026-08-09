import { useEffect, useState } from "react";
import { ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

const BASE_URL = "http://localhost:5147/api";

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
    <div className="page-container" style={{ maxWidth: 520 }}>
      <div className="dashboard-header-band">
        <h2>Thông tin cá nhân</h2>
      </div>
      <div className="card">
        {dangTai && <p>Đang tải...</p>}
        {!dangTai && (
        <>
        {anhHienTai && <img src={anhHienTai} alt="Ảnh đại diện" style={{ width: 96, height: 96, borderRadius: "50%", objectFit: "cover", marginBottom: 16 }} />}
        <form onSubmit={handleSubmit}>
          <div className="field">
            <label>Họ và tên</label>
            <input value={form.hoTen} onChange={set("hoTen")} required />
          </div>
          <div className="field">
            <label>Ngày sinh</label>
            <input type="date" value={form.ngaySinh} onChange={set("ngaySinh")} />
          </div>
          <div className="field">
            <label>Số điện thoại</label>
            <input value={form.sdt} onChange={set("sdt")} />
          </div>
          <div className="field">
            <label>Ảnh đại diện</label>
            <input type="file" accept=".jpg,.jpeg,.png" onChange={(e) => setAnhDaiDien(e.target.files[0])} />
          </div>
          <div className="field">
            <label>Địa chỉ</label>
            <input value={form.diaChi} onChange={set("diaChi")} />
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
        </>
        )}
      </div>
    </div>
  );
}
