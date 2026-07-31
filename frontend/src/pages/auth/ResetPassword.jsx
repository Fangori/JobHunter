import { useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { Lock, CheckCircle2, ShieldCheck } from "lucide-react";
import { api, ApiError } from "../../api/client";

// BM04 buoc 2
export default function ResetPassword() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [matKhauMoi, setMatKhauMoi] = useState("");
  const [xacNhanMatKhauMoi, setXacNhanMatKhauMoi] = useState("");
  const [error, setError] = useState("");
  const [ok, setOk] = useState(false);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      const token = searchParams.get("token");
      const res = await api.post("/auth/reset-password", { token, matKhauMoi, xacNhanMatKhauMoi });
      setOk(true);
      setTimeout(() => navigate("/login"), 1500);
      return res;
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-split">
      <div className="auth-split-panel">
        <ShieldCheck size={40} style={{ marginBottom: 16 }} />
        <h2>Đặt lại mật khẩu</h2>
        <p>Tạo mật khẩu mới để tiếp tục sử dụng tài khoản JobHunter của bạn.</p>
      </div>
      <div className="auth-split-form">
        <div className="card" style={{ textAlign: "center" }}>
          {ok ? (
            <>
              <div style={{
                width: 56, height: 56, borderRadius: "50%", background: "var(--success-bg)", color: "var(--success)",
                display: "flex", alignItems: "center", justifyContent: "center", margin: "0 auto 12px",
              }}>
                <CheckCircle2 size={28} />
              </div>
              <h2>Thành công</h2>
              <p className="success-text">Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại.</p>
              <Link to="/login" className="btn btn-primary" style={{ width: "100%" }}>Đăng nhập ngay</Link>
            </>
          ) : (
            <>
              <div style={{
                width: 56, height: 56, borderRadius: "50%", background: "var(--info-bg)", color: "var(--indigo-dark)",
                display: "flex", alignItems: "center", justifyContent: "center", margin: "0 auto 12px",
              }}>
                <Lock size={26} />
              </div>
              <h2>Đặt lại mật khẩu</h2>
              <form onSubmit={handleSubmit} style={{ textAlign: "left" }}>
                <div className="field">
                  <label>Mật khẩu mới</label>
                  <div className="input-icon-wrap">
                    <Lock size={18} />
                    <input type="password" value={matKhauMoi} onChange={(e) => setMatKhauMoi(e.target.value)} required />
                  </div>
                </div>
                <div className="field">
                  <label>Xác nhận mật khẩu mới</label>
                  <div className="input-icon-wrap">
                    <Lock size={18} />
                    <input type="password" value={xacNhanMatKhauMoi} onChange={(e) => setXacNhanMatKhauMoi(e.target.value)} required />
                  </div>
                </div>
                {error && <p className="error-text">{error}</p>}
                <button className="btn btn-primary" style={{ width: "100%" }} disabled={loading} type="submit">
                  {loading ? "Đang xử lý..." : "Xác nhận"}
                </button>
              </form>
            </>
          )}
        </div>
      </div>
    </div>
  );
}
