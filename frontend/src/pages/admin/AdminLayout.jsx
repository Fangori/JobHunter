import { NavLink, Outlet } from "react-router-dom";

const NAV_ITEMS = [
  { to: "/admin/pending-jobs", label: "Duyệt tin" },
  { to: "/admin/removed-jobs", label: "Gỡ tin" },
  { to: "/admin/accounts/employers", label: "TK Nhà tuyển dụng" },
  { to: "/admin/accounts/candidates", label: "TK Ứng viên" },
  { to: "/admin/skills", label: "Danh mục Kỹ năng" },
  { to: "/admin/industries", label: "Danh mục Ngành nghề" },
  { to: "/admin/reports", label: "Báo cáo" },
];

export default function AdminLayout() {
  return (
    <div className="page-container" style={{ display: "flex", gap: 24, alignItems: "flex-start" }}>
      <aside className="card" style={{ width: 220, flexShrink: 0, padding: 12 }}>
        <nav style={{ display: "flex", flexDirection: "column", gap: 4 }}>
          {NAV_ITEMS.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              style={({ isActive }) => ({
                display: "block",
                padding: "8px 12px",
                borderRadius: "var(--radius)",
                textDecoration: "none",
                color: isActive ? "white" : "inherit",
                background: isActive ? "var(--indigo)" : "transparent",
                fontWeight: isActive ? 600 : 400,
              })}
            >
              {item.label}
            </NavLink>
          ))}
        </nav>
      </aside>
      <div style={{ flex: 1, minWidth: 0 }}>
        <Outlet />
      </div>
    </div>
  );
}
