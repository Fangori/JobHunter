import { useEffect, useState } from "react";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

export default function AdminIndustries() {
  const { auth } = useAuth();
  const [industries, setIndustries] = useState([]);
  const [tenNganhNghe, setTenNganhNghe] = useState("");
  const [editingId, setEditingId] = useState(null);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const load = () => api.get("/admin/industries", auth.token).then(setIndustries);

  useEffect(() => {
    load();
  }, []);

  const resetForm = () => {
    setEditingId(null);
    setTenNganhNghe("");
  };

  const startEdit = (nganh) => {
    setEditingId(nganh.maNganhNghe);
    setTenNganhNghe(nganh.tenNganhNghe);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setSuccess("");
    try {
      const body = { tenNganhNghe };
      const result = editingId
        ? await api.put(`/admin/industries/${editingId}`, body, auth.token)
        : await api.post("/admin/industries", body, auth.token);
      setSuccess(result.message); // MS56
      resetForm();
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra."); // MS57
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm("Xóa ngành nghề này?")) return;
    setError("");
    setSuccess("");
    try {
      const result = await api.del(`/admin/industries/${id}`, auth.token);
      setSuccess(result.message); // MS58 hoac MS59
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    }
  };

  return (
    <div>
      <h2>Danh mục Ngành nghề</h2>

      <form onSubmit={handleSubmit} className="card" style={{ marginBottom: 24, display: "flex", gap: 8, alignItems: "flex-end" }}>
        <div className="field" style={{ flex: 1, margin: 0 }}>
          <label>Tên ngành nghề</label>
          <input value={tenNganhNghe} onChange={(e) => setTenNganhNghe(e.target.value)} required />
        </div>
        <button className="btn btn-primary" style={{ height: 36, padding: "0 16px" }} type="submit">
          {editingId ? "Cập nhật" : "Thêm mới"}
        </button>
        {editingId && <button type="button" className="btn btn-secondary" style={{ height: 36, padding: "0 16px" }} onClick={resetForm}>Hủy</button>}
      </form>

      {error && <p className="error-text">{error}</p>}
      {success && <p className="success-text">{success}</p>}

      <div className="card">
        {industries.map((n) => (
          <div key={n.maNganhNghe} style={{ display: "flex", justifyContent: "space-between", alignItems: "center", borderBottom: "1px solid var(--border)", padding: "10px 0" }}>
            <div>
              <strong>{n.tenNganhNghe}</strong>
              {n.trangThai === "NgungSuDung" && <span className="error-text" style={{ marginLeft: 8 }}>(Ngừng sử dụng)</span>}
            </div>
            <div style={{ display: "flex", gap: 8 }}>
              <button className="btn btn-secondary" style={{ height: 32, padding: "0 12px" }} onClick={() => startEdit(n)}>Sửa</button>
              <button className="btn btn-secondary" style={{ height: 32, padding: "0 12px" }} onClick={() => handleDelete(n.maNganhNghe)}>Xóa</button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
