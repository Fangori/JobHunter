import { useEffect, useState } from "react";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

export default function AdminSkills() {
  const { auth } = useAuth();
  const [skills, setSkills] = useState([]);
  const [tenKyNang, setTenKyNang] = useState("");
  const [nhomNganh, setNhomNganh] = useState("");
  const [editingId, setEditingId] = useState(null);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const load = () => api.get("/admin/skills", auth.token).then(setSkills);

  useEffect(() => {
    load();
  }, []);

  const resetForm = () => {
    setEditingId(null);
    setTenKyNang("");
    setNhomNganh("");
  };

  const startEdit = (skill) => {
    setEditingId(skill.maKyNang);
    setTenKyNang(skill.tenKyNang);
    setNhomNganh(skill.nhomNganh || "");
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setSuccess("");
    try {
      const body = { tenKyNang, nhomNganh: nhomNganh || null };
      const result = editingId
        ? await api.put(`/admin/skills/${editingId}`, body, auth.token)
        : await api.post("/admin/skills", body, auth.token);
      setSuccess(result.message); // MS49
      resetForm();
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra."); // MS50
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm("Xóa kỹ năng này?")) return;
    setError("");
    setSuccess("");
    try {
      const result = await api.del(`/admin/skills/${id}`, auth.token);
      setSuccess(result.message); // MS51 hoac MS52
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    }
  };

  return (
    <div>
      <h2>Danh mục Kỹ năng</h2>

      <form onSubmit={handleSubmit} className="card" style={{ marginBottom: 24, display: "flex", gap: 8, alignItems: "flex-end" }}>
        <div className="field" style={{ flex: 1, margin: 0 }}>
          <label>Tên kỹ năng</label>
          <input value={tenKyNang} onChange={(e) => setTenKyNang(e.target.value)} required />
        </div>
        <div className="field" style={{ flex: 1, margin: 0 }}>
          <label>Nhóm ngành</label>
          <input value={nhomNganh} onChange={(e) => setNhomNganh(e.target.value)} />
        </div>
        <button className="btn btn-primary" style={{ height: 36, padding: "0 16px" }} type="submit">
          {editingId ? "Cập nhật" : "Thêm mới"}
        </button>
        {editingId && <button type="button" className="btn btn-secondary" style={{ height: 36, padding: "0 16px" }} onClick={resetForm}>Hủy</button>}
      </form>

      {error && <p className="error-text">{error}</p>}
      {success && <p className="success-text">{success}</p>}

      <div className="card">
        {skills.map((s) => (
          <div key={s.maKyNang} style={{ display: "flex", justifyContent: "space-between", alignItems: "center", borderBottom: "1px solid var(--border)", padding: "10px 0" }}>
            <div>
              <strong>{s.tenKyNang}</strong>
              {s.nhomNganh && <span style={{ color: "var(--text-muted)" }}> · {s.nhomNganh}</span>}
              {s.trangThai === "NgungSuDung" && <span className="error-text" style={{ marginLeft: 8 }}>(Ngừng sử dụng)</span>}
            </div>
            <div style={{ display: "flex", gap: 8 }}>
              <button className="btn btn-secondary" style={{ height: 32, padding: "0 12px" }} onClick={() => startEdit(s)}>Sửa</button>
              <button className="btn btn-secondary" style={{ height: 32, padding: "0 12px" }} onClick={() => handleDelete(s.maKyNang)}>Xóa</button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
