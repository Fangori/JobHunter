import { Link } from "react-router-dom";
import { Briefcase } from "lucide-react";

export default function Footer() {
  return (
    <footer style={{ background: "#eef0f5", borderTop: "1px solid var(--border)", marginTop: 48 }}>
      <div className="page-container" style={{ display: "flex", flexWrap: "wrap", gap: 32, justifyContent: "space-between" }}>
        <div style={{ maxWidth: 320 }}>
          <div style={{ display: "flex", alignItems: "center", gap: 8, fontSize: 18, fontWeight: 700, color: "var(--navy)", marginBottom: 8 }}>
            <Briefcase size={20} /> JobHunter
          </div>
          <p style={{ color: "var(--text-muted)", fontSize: 14, margin: 0 }}>
            Nền tảng kết nối ứng viên tài năng với nhà tuyển dụng hàng đầu.
          </p>
        </div>

        <div>
          <h4 style={{ margin: "0 0 12px", fontSize: 14, color: "var(--navy)" }}>Về JobHunter</h4>
          <p style={{ margin: "0 0 8px", fontSize: 14, color: "var(--text-muted)" }}>Về chúng tôi</p>
          <p style={{ margin: 0, fontSize: 14, color: "var(--text-muted)" }}>Điều khoản dịch vụ</p>
        </div>

        <div>
          <h4 style={{ margin: "0 0 12px", fontSize: 14, color: "var(--navy)" }}>Khám phá</h4>
          <Link to="/" style={{ display: "block", marginBottom: 8, fontSize: 14, color: "var(--text-muted)" }}>Việc làm</Link>
          <p style={{ margin: 0, fontSize: 14, color: "var(--text-muted)" }}>Chính sách bảo mật</p>
        </div>
      </div>

      <div style={{ borderTop: "1px solid var(--border)", marginTop: 24, padding: "16px 24px" }}>
        <p style={{ maxWidth: 1100, margin: "0 auto", fontSize: 13, color: "var(--text-muted)" }}>
          © 2026 JobHunter. Tất cả quyền được bảo lưu.
        </p>
      </div>
    </footer>
  );
}
