import { useEffect, useRef, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { api } from "../api/client";

function NotificationBell() {
  const { auth } = useAuth();
  const navigate = useNavigate();
  const [notifications, setNotifications] = useState([]);
  const [open, setOpen] = useState(false);
  const boxRef = useRef(null);

  const load = () => api.get("/notifications/mine", auth.token).then(setNotifications).catch(() => {});

  useEffect(() => {
    load();
    const interval = setInterval(load, 30000);
    return () => clearInterval(interval);
  }, []);

  useEffect(() => {
    const onClickOutside = (e) => {
      if (boxRef.current && !boxRef.current.contains(e.target)) setOpen(false);
    };
    document.addEventListener("mousedown", onClickOutside);
    return () => document.removeEventListener("mousedown", onClickOutside);
  }, []);

  const unreadCount = notifications.filter((n) => !n.daDoc).length;

  const handleClick = async (n) => {
    if (!n.daDoc) {
      await api.post(`/notifications/${n.maThongBao}/mark-read`, undefined, auth.token);
      load();
    }
    setOpen(false);
    if (n.lienKet) navigate(n.lienKet);
  };

  return (
    <div ref={boxRef} style={{ position: "relative" }}>
      <button
        type="button"
        onClick={() => setOpen((o) => !o)}
        style={{ position: "relative", border: "none", background: "transparent", color: "white", cursor: "pointer", fontSize: 20, padding: "0 4px" }}
        title="Thông báo"
      >
        🔔
        {unreadCount > 0 && (
          <span style={{
            position: "absolute", top: -4, right: -6, background: "#e0245e", color: "white",
            borderRadius: "999px", fontSize: 11, minWidth: 16, height: 16, display: "flex",
            alignItems: "center", justifyContent: "center", padding: "0 3px", fontWeight: 700,
          }}>
            {unreadCount}
          </span>
        )}
      </button>
      {open && (
        <div className="card" style={{
          position: "absolute", right: 0, top: 32, width: 320, maxHeight: 400, overflowY: "auto",
          background: "white", color: "var(--text)", zIndex: 200, padding: 8,
        }}>
          {notifications.length === 0 && <p style={{ margin: 8, color: "var(--text-muted)" }}>Chưa có thông báo nào.</p>}
          {notifications.map((n) => (
            <div
              key={n.maThongBao}
              onClick={() => handleClick(n)}
              style={{
                padding: 8, borderRadius: 8, cursor: "pointer",
                background: n.daDoc ? "transparent" : "rgba(90,120,255,0.08)",
                marginBottom: 4,
              }}
            >
              <p style={{ margin: 0, fontSize: 14 }}>{n.noiDung}</p>
              <p style={{ margin: "4px 0 0", fontSize: 12, color: "var(--text-muted)" }}>
                {new Date(n.ngayTao).toLocaleString("vi-VN")}
              </p>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

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
        {auth?.vaiTro === "NhaTuyenDung" && <Link to="/employer/my-jobs">Tin của tôi</Link>}
        {auth?.vaiTro === "NhaTuyenDung" && <Link to="/employer/profile">Hồ sơ công ty</Link>}
        {auth?.vaiTro === "Admin" && <Link to="/admin/pending-jobs">Duyệt tin</Link>}
        {auth?.vaiTro === "UngVien" && <Link to="/candidate/cvs">CV của tôi</Link>}
        {auth?.vaiTro === "UngVien" && <Link to="/candidate/applications">Đơn ứng tuyển</Link>}
        {auth?.vaiTro === "UngVien" && <Link to="/candidate/favorites">Tin đã lưu</Link>}
        {auth?.vaiTro === "UngVien" && <Link to="/candidate/profile">Hồ sơ cá nhân</Link>}
        {auth && <NotificationBell />}
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
