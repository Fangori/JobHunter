import { useEffect, useState } from "react";
import { Search } from "lucide-react";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

export default function AdminAccounts({ vaiTro }) {
  const { auth } = useAuth();
  const [accounts, setAccounts] = useState([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [lockingId, setLockingId] = useState(null);
  const [lyDo, setLyDo] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const load = () => api.get(`/admin/accounts?vaiTro=${vaiTro}`, auth.token).then(setAccounts);

  useEffect(() => {
    load();
  }, [vaiTro]);

  const clearMsg = () => {
    setError("");
    setSuccess("");
  };

  const submitLock = async (id) => {
    clearMsg();
    try {
      const result = await api.post(`/admin/accounts/${id}/lock`, { lyDo }, auth.token);
      setSuccess(result.message);
      setLockingId(null);
      setLyDo("");
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra."); // MS55
    }
  };

  const unlock = async (id) => {
    clearMsg();
    try {
      const result = await api.post(`/admin/accounts/${id}/unlock`, undefined, auth.token);
      setSuccess(result.message);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    }
  };

  const title = vaiTro === "NhaTuyenDung" ? "Quản lý tài khoản Nhà tuyển dụng" : "Quản lý tài khoản Ứng viên";
  const soHoatDong = accounts.filter((a) => a.trangThai !== "BiKhoa").length;
  const soBiKhoa = accounts.filter((a) => a.trangThai === "BiKhoa").length;
  const filtered = accounts.filter((a) => {
    const term = searchTerm.trim().toLowerCase();
    if (!term) return true;
    return (a.hoTenOrTenCongTy || "").toLowerCase().includes(term) || (a.email || "").toLowerCase().includes(term);
  });

  return (
    <div>
      <div className="dashboard-header-band">
        <h2>{title}</h2>
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: 16, marginBottom: 24 }}>
        <div className="card" style={{ textAlign: "center" }}>
          <p style={{ fontSize: 28, fontWeight: 700, margin: 0, color: "var(--navy)" }}>{accounts.length}</p>
          <p style={{ margin: 0, color: "var(--text-muted)", fontSize: 14 }}>Tổng số</p>
        </div>
        <div className="card" style={{ textAlign: "center" }}>
          <p style={{ fontSize: 28, fontWeight: 700, margin: 0, color: "var(--success)" }}>{soHoatDong}</p>
          <p style={{ margin: 0, color: "var(--text-muted)", fontSize: 14 }}>Đang hoạt động</p>
        </div>
        <div className="card" style={{ textAlign: "center" }}>
          <p style={{ fontSize: 28, fontWeight: 700, margin: 0, color: "var(--danger)" }}>{soBiKhoa}</p>
          <p style={{ margin: 0, color: "var(--text-muted)", fontSize: 14 }}>Bị khóa</p>
        </div>
      </div>

      <div className="input-icon-wrap" style={{ marginBottom: 16, maxWidth: 360 }}>
        <Search size={18} />
        <input
          placeholder="Tìm theo tên hoặc email..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
      </div>

      {error && <p className="error-text">{error}</p>}
      {success && <p className="success-text">{success}</p>}

      {filtered.length === 0 && (
        <p>{accounts.length === 0 ? "Không có tài khoản nào." : "Không tìm thấy tài khoản khớp từ khóa."}</p>
      )}
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(280px, 1fr))", gap: 12 }}>
        {filtered.map((acc) => (
          <div key={acc.maTk} className="card">
            <span className={`badge ${acc.trangThai === "BiKhoa" ? "badge-danger" : "badge-success"}`}>
              {acc.trangThai === "BiKhoa" ? "Bị khóa" : "Hoạt động"}
            </span>
            <p style={{ fontWeight: 600, margin: "10px 0 2px" }}>{acc.hoTenOrTenCongTy}</p>
            <p style={{ margin: 0, color: "var(--text-muted)", fontSize: 14 }}>{acc.email}</p>
            {acc.trangThai === "BiKhoa" && acc.lyDoKhoa && (
              <p style={{ margin: "4px 0 0", fontSize: 13 }}>Lý do: {acc.lyDoKhoa}</p>
            )}
            <div style={{ marginTop: 12 }}>
              {acc.trangThai === "BiKhoa" ? (
                <button className="btn btn-primary" style={{ height: 36, padding: "0 16px" }} onClick={() => unlock(acc.maTk)}>Mở khóa</button>
              ) : (
                <button className="btn btn-secondary" style={{ height: 36, padding: "0 16px" }} onClick={() => { setLockingId(acc.maTk); setLyDo(""); }}>Khóa</button>
              )}
            </div>
            {lockingId === acc.maTk && (
              <div style={{ marginTop: 8, display: "flex", flexDirection: "column", gap: 8 }}>
                <input placeholder="Lý do khóa (bắt buộc)" value={lyDo} onChange={(e) => setLyDo(e.target.value)} />
                <div style={{ display: "flex", gap: 8 }}>
                  <button className="btn btn-primary" style={{ height: 36 }} onClick={() => submitLock(acc.maTk)}>Xác nhận khóa</button>
                  <button className="btn btn-secondary" style={{ height: 36 }} onClick={() => setLockingId(null)}>Hủy</button>
                </div>
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
