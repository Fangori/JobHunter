import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
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
    <div className="page-container auth-card-wrapper">
      <div className="card" style={{ textAlign: "center" }}>
        <h2>Xác thực email</h2>
        <p className={ok ? "success-text" : "error-text"}>{message || "Đang xác thực..."}</p>
        {ok && <Link to="/login" className="btn btn-primary" style={{ width: "100%", marginTop: 12 }}>Đăng nhập ngay</Link>}
      </div>
    </div>
  );
}
