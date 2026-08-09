import { useEffect, useRef, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Bell, Briefcase, CircleUserRound, LogOut, ChevronDown } from "lucide-react";
import { useAuth } from "../context/AuthContext";
import { api } from "../api/client";
import { roleLabel } from "../utils/roleLabel";

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
        style={{ position: "relative", display: "flex", alignItems: "center", border: "none", background: "transparent", color: "white", cursor: "pointer", padding: "0 4px" }}
        title="Thông báo"
      >
        <Bell size={20} />
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

const ROLE_MENU_LINKS = {
  UngVien: [
    { to: "/candidate/cvs", label: "CV của tôi" },
    { to: "/candidate/applications", label: "Đơn ứng tuyển" },
    { to: "/candidate/favorites", label: "Tin đã lưu" },
    { to: "/candidate/profile", label: "Hồ sơ cá nhân" },
  ],
  NhaTuyenDung: [
    { to: "/employer/my-jobs", label: "Tin của tôi" },
    { to: "/employer/profile", label: "Hồ sơ công ty" },
  ],
  Admin: [
    { to: "/admin/pending-jobs", label: "Quản trị" },
  ],
};

function AccountMenu() {
  const { auth, logout } = useAuth();
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const boxRef = useRef(null);

  useEffect(() => {
    const onClickOutside = (e) => {
      if (boxRef.current && !boxRef.current.contains(e.target)) setOpen(false);
    };
    document.addEventListener("mousedown", onClickOutside);
    return () => document.removeEventListener("mousedown", onClickOutside);
  }, []);

  const handleLogout = () => {
    logout();
    navigate("/");
  };

  const menuLinks = ROLE_MENU_LINKS[auth.vaiTro] || [];

  return (
    <div ref={boxRef} style={{ position: "relative" }}>
      <button
        type="button"
        onClick={() => setOpen((o) => !o)}
        style={{ display: "flex", alignItems: "center", gap: 6, border: "none", background: "transparent", color: "white", cursor: "pointer", padding: "4px 6px", fontSize: 15 }}
      >
        <CircleUserRound size={20} />
        {auth.hoTenOrTenCongTy} ({roleLabel(auth.vaiTro)})
        <ChevronDown size={16} />
      </button>
      {open && (
        <div className="card" style={{
          position: "absolute", right: 0, top: 32, width: 220, zIndex: 200,
          background: "white", color: "var(--text)", padding: 8,
        }}>
          {menuLinks.map((link) => (
            <Link
              key={link.to}
              to={link.to}
              onClick={() => setOpen(false)}
              style={{ display: "block", padding: "8px 10px", borderRadius: 8, color: "var(--text)", textDecoration: "none" }}
            >
              {link.label}
            </Link>
          ))}
          <div style={{ borderTop: "1px solid var(--border)", margin: "6px 0" }} />
          <button
            type="button"
            onClick={handleLogout}
            style={{ display: "flex", alignItems: "center", gap: 6, width: "100%", padding: "8px 10px", background: "transparent", border: "none", color: "var(--danger)", cursor: "pointer", fontWeight: 600, borderRadius: 8 }}
          >
            <LogOut size={16} /> Đăng xuất
          </button>
        </div>
      )}
    </div>
  );
}

export default function Header() {
  const { auth } = useAuth();

  return (
    <header className="app-header">
      <Link to="/" className="brand" style={{ display: "flex", alignItems: "center", gap: 8 }}>
        <Briefcase size={22} /> JobHunter
      </Link>
      <nav style={{ display: "flex", gap: 16, alignItems: "center" }}>
        {!auth && <Link to="/login">Đăng nhập</Link>}
        {!auth && <Link to="/register">Đăng ký</Link>}
        {auth?.vaiTro === "NhaTuyenDung" && <Link to="/employer/post-job">Đăng tin</Link>}
        {auth && <NotificationBell />}
        {auth && <AccountMenu />}
      </nav>
    </header>
  );
}
