import { useState } from "react";
import { User, Lock, Mail, Phone, Building2, MapPin, Briefcase, CheckCircle2 } from "lucide-react";
import { api, ApiError } from "../../api/client";

const emptyCandidateForm = () => ({ hoTen: "", matKhau: "", email: "", xacNhanMatKhau: "", sdt: "" });

function DangKyThanhCongPopup({ message, onClose }) {
  return (
    <div style={{
      position: "fixed", inset: 0, background: "rgba(0,0,0,0.4)",
      display: "flex", alignItems: "center", justifyContent: "center", zIndex: 100,
    }}>
      <div className="card" style={{ width: 400, background: "white", textAlign: "center" }}>
        <CheckCircle2 size={40} color="var(--success)" style={{ marginBottom: 12 }} />
        <p className="success-text" style={{ fontSize: 15 }}>{message}</p>
        <button className="btn btn-primary" style={{ width: "100%" }} onClick={onClose}>Đóng</button>
      </div>
    </div>
  );
}

function CandidateForm() {
  const [form, setForm] = useState(emptyCandidateForm());
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
      setForm(emptyCandidateForm());
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
        <div className="input-icon-wrap">
          <User size={18} />
          <input value={form.hoTen} onChange={set("hoTen")} required />
        </div>
      </div>
      <div className="field">
        <label>Email</label>
        <div className="input-icon-wrap">
          <Mail size={18} />
          <input type="email" value={form.email} onChange={set("email")} required />
        </div>
      </div>
      <div className="field">
        <label>Mật khẩu</label>
        <div className="input-icon-wrap">
          <Lock size={18} />
          <input type="password" value={form.matKhau} onChange={set("matKhau")} required />
        </div>
      </div>
      <div className="field">
        <label>Xác nhận mật khẩu</label>
        <div className="input-icon-wrap">
          <Lock size={18} />
          <input type="password" value={form.xacNhanMatKhau} onChange={set("xacNhanMatKhau")} required />
        </div>
      </div>
      <div className="field">
        <label>Số điện thoại</label>
        <div className="input-icon-wrap">
          <Phone size={18} />
          <input value={form.sdt} onChange={set("sdt")} />
        </div>
      </div>
      {error && <p className="error-text">{error}</p>}
      <button className="btn btn-primary" style={{ width: "100%" }} disabled={loading} type="submit">
        {loading ? "Đang đăng ký..." : "Đăng ký ngay"}
      </button>
      {success && <DangKyThanhCongPopup message={success} onClose={() => setSuccess("")} />}
    </form>
  );
}

const emptyEmployerForm = () => ({ tenCongTy: "", diaChi: "", email: "", matKhau: "", sdt: "", xacNhanMatKhau: "" });

function EmployerForm() {
  const [form, setForm] = useState(emptyEmployerForm());
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
      setForm(emptyEmployerForm());
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
        <div className="input-icon-wrap">
          <Building2 size={18} />
          <input value={form.tenCongTy} onChange={set("tenCongTy")} required />
        </div>
      </div>
      <div className="field">
        <label>Địa chỉ công ty</label>
        <div className="input-icon-wrap">
          <MapPin size={18} />
          <input value={form.diaChi} onChange={set("diaChi")} required />
        </div>
      </div>
      <div className="field">
        <label>Email công ty</label>
        <div className="input-icon-wrap">
          <Mail size={18} />
          <input type="email" value={form.email} onChange={set("email")} required />
        </div>
      </div>
      <div className="field">
        <label>Mật khẩu</label>
        <div className="input-icon-wrap">
          <Lock size={18} />
          <input type="password" value={form.matKhau} onChange={set("matKhau")} required />
        </div>
      </div>
      <div className="field">
        <label>Xác nhận mật khẩu</label>
        <div className="input-icon-wrap">
          <Lock size={18} />
          <input type="password" value={form.xacNhanMatKhau} onChange={set("xacNhanMatKhau")} required />
        </div>
      </div>
      <div className="field">
        <label>Số điện thoại</label>
        <div className="input-icon-wrap">
          <Phone size={18} />
          <input value={form.sdt} onChange={set("sdt")} />
        </div>
      </div>
      {error && <p className="error-text">{error}</p>}
      <button className="btn btn-primary" style={{ width: "100%" }} disabled={loading} type="submit">
        {loading ? "Đang đăng ký..." : "Đăng ký ngay"}
      </button>
      {success && <DangKyThanhCongPopup message={success} onClose={() => setSuccess("")} />}
    </form>
  );
}

export default function Register() {
  const [tab, setTab] = useState("candidate");

  return (
    <div className="auth-split">
      <div className="auth-split-panel">
        <Briefcase size={40} style={{ marginBottom: 16 }} />
        <h2>Bắt đầu hành trình sự nghiệp</h2>
        <p>Tham gia nền tảng kết nối ứng viên và nhà tuyển dụng hàng đầu.</p>
      </div>
      <div className="auth-split-form">
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
    </div>
  );
}
