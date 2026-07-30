import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { api, ApiError } from "../../api/client";

function CandidateForm() {
  const [form, setForm] = useState({ hoTen: "", matKhau: "", email: "", xacNhanMatKhau: "", sdt: "" });
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);

  const set = (key) => (e) => setForm({ ...form, [key]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setSuccess("");
    if (form.matKhau !== form.xacNhanMatKhau) {
      setError("Mật khẩu xác nhận không khớp.");
      return;
    }
    setLoading(true);
    try {
      const result = await api.post("/auth/register/candidate", form);
      setSuccess(result.message || "Đăng ký thành công.");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <div className="field">
        <label>Họ và Tên</label>
        <input value={form.hoTen} onChange={set("hoTen")} required />
      </div>
      <div className="field">
        <label>Mật khẩu</label>
        <input type="password" value={form.matKhau} onChange={set("matKhau")} required />
      </div>
      <div className="field">
        <label>Email</label>
        <input type="email" value={form.email} onChange={set("email")} required />
      </div>
      <div className="field">
        <label>Xác nhận mật khẩu</label>
        <input type="password" value={form.xacNhanMatKhau} onChange={set("xacNhanMatKhau")} required />
      </div>
      <div className="field">
        <label>Số điện thoại</label>
        <input value={form.sdt} onChange={set("sdt")} />
      </div>
      {error && <p className="error-text">{error}</p>}
      {success && <p className="success-text">{success}</p>}
      <button className="btn btn-primary" style={{ width: "100%" }} disabled={loading} type="submit">
        {loading ? "Đang đăng ký..." : "Đăng ký ngay"}
      </button>
    </form>
  );
}

function EmployerForm() {
  const [form, setForm] = useState({ tenCongTy: "", diaChi: "", email: "", matKhau: "", sdt: "", xacNhanMatKhau: "" });
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);

  const set = (key) => (e) => setForm({ ...form, [key]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setSuccess("");
    if (form.matKhau !== form.xacNhanMatKhau) {
      setError("Mật khẩu xác nhận không khớp.");
      return;
    }
    setLoading(true);
    try {
      const result = await api.post("/auth/register/employer", form);
      setSuccess(result.message || "Đăng ký thành công.");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <div className="field">
        <label>Tên công ty</label>
        <input value={form.tenCongTy} onChange={set("tenCongTy")} required />
      </div>
      <div className="field">
        <label>Địa chỉ công ty</label>
        <input value={form.diaChi} onChange={set("diaChi")} required />
      </div>
      <div className="field">
        <label>Email công ty</label>
        <input type="email" value={form.email} onChange={set("email")} required />
      </div>
      <div className="field">
        <label>Mật khẩu</label>
        <input type="password" value={form.matKhau} onChange={set("matKhau")} required />
      </div>
      <div className="field">
        <label>Số điện thoại</label>
        <input value={form.sdt} onChange={set("sdt")} />
      </div>
      <div className="field">
        <label>Xác nhận mật khẩu</label>
        <input type="password" value={form.xacNhanMatKhau} onChange={set("xacNhanMatKhau")} required />
      </div>
      {error && <p className="error-text">{error}</p>}
      {success && <p className="success-text">{success}</p>}
      <button className="btn btn-primary" style={{ width: "100%" }} disabled={loading} type="submit">
        {loading ? "Đang đăng ký..." : "Đăng ký ngay"}
      </button>
    </form>
  );
}

export default function Register() {
  const [tab, setTab] = useState("candidate");

  return (
    <div className="page-container auth-card-wrapper">
      <div className="card">
        <h2>Đăng ký tài khoản</h2>
        <div className="tabs">
          <button className={tab === "candidate" ? "active" : ""} onClick={() => setTab("candidate")} type="button">
            Ứng viên
          </button>
          <button className={tab === "employer" ? "active" : ""} onClick={() => setTab("employer")} type="button">
            Nhà tuyển dụng
          </button>
        </div>
        {tab === "candidate" ? <CandidateForm /> : <EmployerForm />}
      </div>
    </div>
  );
}
