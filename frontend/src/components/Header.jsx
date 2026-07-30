import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function Header() {
  const { auth, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/");
  };

  return (
    <header className="app-header">
      <Link to="/" className="brand">JobHunter</Link>
      <nav style={{ display: "flex", gap: 16, alignItems: "center" }}>
        {!auth && <Link to="/login">Đăng nhập</Link>}
        {!auth && <Link to="/register">Đăng ký</Link>}
        {auth?.vaiTro === "NhaTuyenDung" && <Link to="/employer/post-job">Đăng tin</Link>}
        {auth?.vaiTro === "NhaTuyenDung" && <Link to="/employer/profile">Hồ sơ công ty</Link>}
        {auth?.vaiTro === "Admin" && <Link to="/admin/pending-jobs">Duyệt tin</Link>}
        {auth?.vaiTro === "UngVien" && <Link to="/candidate/cvs">CV của tôi</Link>}
        {auth?.vaiTro === "UngVien" && <Link to="/candidate/profile">Hồ sơ cá nhân</Link>}
        {auth && (
          <>
            <span>{auth.hoTenOrTenCongTy} ({auth.vaiTro})</span>
            <button onClick={handleLogout} style={{ height: 36, padding: "0 16px", background: "transparent", color: "white", border: "1px solid white", borderRadius: "var(--radius)", cursor: "pointer", fontWeight: 600 }}>
              Đăng xuất
            </button>
          </>
        )}
      </nav>
    </header>
  );
}
