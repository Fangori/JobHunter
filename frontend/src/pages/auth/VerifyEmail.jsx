import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { MailCheck, MailWarning } from "lucide-react";
import { api, ApiError } from "../../api/client";

export default function VerifyEmail() {
  const [searchParams] = useSearchParams();
  const [message, setMessage] = useState("");
  const [ok, setOk] = useState(false);

  useEffect(() => {
    const token = searchParams.get("token");
    if (!token) {
      setMessage("Thiếu token xác thực.");
      return;
    }
    api.post("/auth/verify-email", { token })
      .then((res) => {
        setOk(true);
        setMessage(res.message);
      })
      .catch((err) => setMessage(err instanceof ApiError ? err.message : "Có lỗi xảy ra."));
  }, [searchParams]);

  return (
    <div className="auth-split">
      <div className="auth-split-panel">
        <MailCheck size={40} style={{ marginBottom: 16 }} />
        <h2>Xác thực email</h2>
        <p>Xác thực email giúp bảo vệ tài khoản và mở khoá đầy đủ tính năng của JobHunter.</p>
      </div>
      <div className="auth-split-form">
        <div className="card" style={{ textAlign: "center" }}>
          <div style={{
            width: 56, height: 56, borderRadius: "50%", margin: "0 auto 12px",
            background: ok ? "var(--success-bg)" : "var(--danger-bg)", color: ok ? "var(--success)" : "var(--danger)",
            display: "flex", alignItems: "center", justifyContent: "center",
          }}>
            {ok ? <MailCheck size={28} /> : <MailWarning size={28} />}
          </div>
          <h2>Xác thực email</h2>
          <p className={ok ? "success-text" : "error-text"}>{message || "Đang xác thực..."}</p>
          {ok && <Link to="/login" className="btn btn-primary" style={{ width: "100%", marginTop: 12 }}>Đăng nhập ngay</Link>}
        </div>
      </div>
    </div>
  );
}
