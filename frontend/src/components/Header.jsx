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
        {auth?.vaiTro === "Admin" && <Link to="/admin/pending-jobs">Duyệt tin</Link>}
        {auth && (
          <>
            <span>{auth.hoTenOrTenCongTy} ({auth.vaiTro})</span>
            <button className="btn btn-secondary" onClick={handleLogout} style={{ height: 36, color: "white", borderColor: "white" }}>
              Đăng xuất
            </button>
          </>
        )}
      </nav>
    </header>
  );
}
