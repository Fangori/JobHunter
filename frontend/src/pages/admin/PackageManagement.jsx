import { useEffect, useState } from "react";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

export default function PackageManagement() {
  const { auth } = useAuth();
  const [danhSach, setDanhSach] = useState([]);
  const [tenGoi, setTenGoi] = useState("");
  const [gioiHanTin, setGioiHanTin] = useState("");
  const [coNoiBat, setCoNoiBat] = useState(false);
  const [giaTien, setGiaTien] = useState("");
  const [editingId, setEditingId] = useState(null);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const load = () => api.get("/admin/packages", auth.token).then(setDanhSach);

  useEffect(() => {
    load();
  }, []);

  const resetForm = () => {
    setEditingId(null);
    setTenGoi("");
    setGioiHanTin("");
    setCoNoiBat(false);
    setGiaTien("");
  };

  const startEdit = (goi) => {
    setEditingId(goi.maGoi);
    setTenGoi(goi.tenGoi);
    setGioiHanTin(String(goi.gioiHanTin));
    setCoNoiBat(goi.coNoiBat);
    setGiaTien(String(goi.giaTien));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setSuccess("");
    try {
      const body = { tenGoi, gioiHanTin: Number(gioiHanTin), coNoiBat, giaTien: Number(giaTien) };
      const result = editingId
        ? await api.put(`/admin/packages/${editingId}`, body, auth.token)
        : await api.post("/admin/packages", body, auth.token);
      setSuccess(result.message); // MS62
      resetForm();
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra."); // MS63
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm("Xóa gói dịch vụ này?")) return;
    setError("");
    setSuccess("");
    try {
      const result = await api.del(`/admin/packages/${id}`, auth.token);
      setSuccess(result.message); // MS64 hoac MS65
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    }
  };

  return (
    <div>
      <div className="dashboard-header-band">
        <h2>Danh mục Gói dịch vụ</h2>
      </div>

      <form onSubmit={handleSubmit} className="card" style={{ marginBottom: 24, display: "flex", gap: 8, alignItems: "flex-end", flexWrap: "wrap" }}>
        <div className="field" style={{ flex: 1, margin: 0, minWidth: 140 }}>
          <label>Tên gói</label>
          <input value={tenGoi} onChange={(e) => setTenGoi(e.target.value)} required />
        </div>
        <div className="field" style={{ flex: 1, margin: 0, minWidth: 140 }}>
          <label>Giới hạn tin đăng</label>
          <input type="number" min="1" value={gioiHanTin} onChange={(e) => setGioiHanTin(e.target.value)} required />
        </div>
        <div className="field" style={{ flex: 1, margin: 0, minWidth: 140 }}>
          <label>Giá tiền (đ)</label>
          <input type="number" min="0" value={giaTien} onChange={(e) => setGiaTien(e.target.value)} required />
        </div>
        <label style={{ display: "flex", alignItems: "center", gap: 6, fontWeight: 400, marginBottom: 8 }}>
          <input type="checkbox" checked={coNoiBat} onChange={(e) => setCoNoiBat(e.target.checked)} />
          Nổi bật
        </label>
        <button className="btn btn-primary" style={{ height: 36, padding: "0 16px" }} type="submit">
          {editingId ? "Cập nhật" : "Thêm mới"}
        </button>
        {editingId && <button type="button" className="btn btn-secondary" style={{ height: 36, padding: "0 16px" }} onClick={resetForm}>Hủy</button>}
      </form>

      {error && <p className="error-text">{error}</p>}
      {success && <p className="success-text">{success}</p>}

      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(240px, 1fr))", gap: 12 }}>
        {danhSach.map((goi) => (
          <div key={goi.maGoi} className="card">
            <div style={{ display: "flex", gap: 6, marginBottom: 10 }}>
              {goi.coNoiBat && <span className="badge badge-warning">Nổi bật</span>}
              {goi.trangThai === "NgungBan" && <span className="badge badge-neutral">Ngừng bán</span>}
            </div>
            <p style={{ fontWeight: 600, fontSize: 18, margin: "0 0 4px" }}>{goi.tenGoi}</p>
            <p style={{ margin: "0 0 4px", color: "var(--text-muted)" }}>Giới hạn {goi.gioiHanTin} tin đăng đồng thời</p>
            <p style={{ margin: "0 0 12px", fontWeight: 600, color: "var(--indigo)" }}>{goi.giaTien.toLocaleString("vi-VN")}đ / {goi.thoiHan} ngày</p>
            <div style={{ display: "flex", gap: 8 }}>
              <button className="btn btn-secondary" style={{ height: 32, padding: "0 12px" }} onClick={() => startEdit(goi)}>Sửa</button>
              <button className="btn btn-secondary" style={{ height: 32, padding: "0 12px" }} onClick={() => handleDelete(goi.maGoi)}>Xóa</button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
