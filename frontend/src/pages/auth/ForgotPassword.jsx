import { useState } from "react";
import { KeyRound, Mail } from "lucide-react";
import { api, ApiError } from "../../api/client";

// BM04 buoc 1
export default function ForgotPassword() {
  const [email, setEmail] = useState("");
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setMessage("");
    setLoading(true);
    try {
      const res = await api.post("/auth/forgot-password", { email });
      setMessage(res.message);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-split">
      <div className="auth-split-panel">
        <KeyRound size={40} style={{ marginBottom: 16 }} />
        <h2>Quên mật khẩu?</h2>
        <p>Đừng lo lắng! Nhập email liên kết với tài khoản của bạn và chúng tôi sẽ gửi liên kết để đặt lại mật khẩu.</p>
      </div>
      <div className="auth-split-form">
        <div className="card" style={{ textAlign: "center" }}>
          <div style={{
            width: 56, height: 56, borderRadius: "50%", background: "var(--info-bg)", color: "var(--indigo-dark)",
            display: "flex", alignItems: "center", justifyContent: "center", margin: "0 auto 12px",
          }}>
            <KeyRound size={26} />
          </div>
          <h2>Quên mật khẩu</h2>
          <form onSubmit={handleSubmit} style={{ textAlign: "left" }}>
            <div className="field">
              <label>Email</label>
              <div className="input-icon-wrap">
                <Mail size={18} />
                <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
              </div>
            </div>
            {error && <p className="error-text">{error}</p>}
            {message && <p className="success-text">{message}</p>}
            <button className="btn btn-primary" style={{ width: "100%" }} disabled={loading} type="submit">
              {loading ? "Đang gửi..." : "Gửi liên kết đặt lại"}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
