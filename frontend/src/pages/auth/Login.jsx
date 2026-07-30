import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

export default function Login() {
  const [email, setEmail] = useState("");
  const [matKhau, setMatKhau] = useState("");
  const [error, setError] = useState("");
  const [chuaXacThuc, setChuaXacThuc] = useState(false);
  const [resendMsg, setResendMsg] = useState("");
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setChuaXacThuc(false);
    setResendMsg("");
    setLoading(true);
    try {
      const result = await api.post("/auth/login", { email, matKhau });
      login(result);
      navigate("/");
    } catch (err) {
      const msg = err instanceof ApiError ? err.message : "Có lỗi xảy ra.";
      setError(msg);
      if (msg.includes("chưa xác thực")) setChuaXacThuc(true);
    } finally {
      setLoading(false);
    }
  };

  const handleResend = async () => {
    setResendMsg("");
    try {
      const result = await api.post("/auth/resend-verification", { email });
      setResendMsg(result.message);
    } catch (err) {
      setResendMsg(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    }
  };

  return (
    <div className="page-container auth-card-wrapper">
      <div className="card">
        <h2>Đăng nhập</h2>
        <form onSubmit={handleSubmit}>
          <div className="field">
            <label>Email</label>
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
          </div>
          <div className="field">
            <label>Mật khẩu</label>
            <input type="password" value={matKhau} onChange={(e) => setMatKhau(e.target.value)} required />
          </div>
          {error && <p className="error-text">{error}</p>}
          {chuaXacThuc && (
            <button type="button" className="btn btn-secondary" style={{ width: "100%", marginBottom: 12 }} onClick={handleResend}>
              Gửi lại liên kết xác thực
            </button>
          )}
          {resendMsg && <p className="success-text">{resendMsg}</p>}
          <button className="btn btn-primary" style={{ width: "100%" }} disabled={loading} type="submit">
            {loading ? "Đang đăng nhập..." : "Đăng Nhập"}
          </button>
        </form>
        <p style={{ marginTop: 12, textAlign: "center" }}>
          <Link to="/forgot-password">Quên mật khẩu?</Link>
        </p>
      </div>
    </div>
  );
}
