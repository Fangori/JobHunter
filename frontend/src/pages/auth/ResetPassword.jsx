import { useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
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
    <div className="page-container auth-card-wrapper">
      <div className="card">
        <h2>Đặt lại mật khẩu</h2>
        {ok ? (
          <>
            <p className="success-text">Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại.</p>
            <Link to="/login" className="btn btn-primary" style={{ width: "100%" }}>Đăng nhập ngay</Link>
          </>
        ) : (
          <form onSubmit={handleSubmit}>
            <div className="field">
              <label>Mật khẩu mới</label>
              <input type="password" value={matKhauMoi} onChange={(e) => setMatKhauMoi(e.target.value)} required />
            </div>
            <div className="field">
              <label>Xác nhận mật khẩu mới</label>
              <input type="password" value={xacNhanMatKhauMoi} onChange={(e) => setXacNhanMatKhauMoi(e.target.value)} required />
            </div>
            {error && <p className="error-text">{error}</p>}
            <button className="btn btn-primary" style={{ width: "100%" }} disabled={loading} type="submit">
              {loading ? "Đang xử lý..." : "Xác nhận"}
            </button>
          </form>
        )}
      </div>
    </div>
  );
}
