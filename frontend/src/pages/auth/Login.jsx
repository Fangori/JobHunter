import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

export default function Login() {
  const [email, setEmail] = useState("");
  const [matKhau, setMatKhau] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      const result = await api.post("/auth/login", { email, matKhau });
      login(result);
      navigate("/");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    } finally {
      setLoading(false);
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
          <button className="btn btn-primary" style={{ width: "100%" }} disabled={loading} type="submit">
            {loading ? "Đang đăng nhập..." : "Đăng Nhập"}
          </button>
        </form>
      </div>
    </div>
  );
}
