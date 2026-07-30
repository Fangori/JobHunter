import { useEffect, useState } from "react";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

export default function AdminAccounts({ vaiTro }) {
  const { auth } = useAuth();
  const [accounts, setAccounts] = useState([]);
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

  return (
    <div>
      <h2>{title}</h2>
      {error && <p className="error-text">{error}</p>}
      {success && <p className="success-text">{success}</p>}

      <div className="card">
        {accounts.length === 0 && <p>Không có tài khoản nào.</p>}
        {accounts.map((acc) => (
          <div key={acc.maTk} style={{ borderBottom: "1px solid var(--border)", padding: "12px 0" }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start" }}>
              <div>
                <strong>{acc.hoTenOrTenCongTy}</strong>
                <p style={{ margin: "4px 0", color: "var(--text-muted)", fontSize: 14 }}>{acc.email}</p>
                <p style={{ margin: 0, fontSize: 14 }}>
                  Trạng thái: <strong>{acc.trangThai === "BiKhoa" ? "Bị khóa" : "Hoạt động"}</strong>
                  {acc.trangThai === "BiKhoa" && acc.lyDoKhoa && <span> — Lý do: {acc.lyDoKhoa}</span>}
                </p>
              </div>
              {acc.trangThai === "BiKhoa" ? (
                <button className="btn btn-primary" style={{ height: 36, padding: "0 16px" }} onClick={() => unlock(acc.maTk)}>Mở khóa</button>
              ) : (
                <button className="btn btn-secondary" style={{ height: 36, padding: "0 16px" }} onClick={() => { setLockingId(acc.maTk); setLyDo(""); }}>Khóa</button>
              )}
            </div>
            {lockingId === acc.maTk && (
              <div style={{ marginTop: 8, display: "flex", gap: 8 }}>
                <input placeholder="Lý do khóa (bắt buộc)" value={lyDo} onChange={(e) => setLyDo(e.target.value)} style={{ flex: 1 }} />
                <button className="btn btn-primary" style={{ height: 36 }} onClick={() => submitLock(acc.maTk)}>Xác nhận khóa</button>
                <button className="btn btn-secondary" style={{ height: 36 }} onClick={() => setLockingId(null)}>Hủy</button>
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
