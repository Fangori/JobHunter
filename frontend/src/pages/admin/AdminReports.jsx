import { useState } from "react";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

const now = new Date();

export default function AdminReports() {
  const { auth } = useAuth();
  const [thang, setThang] = useState(now.getMonth() + 1);
  const [nam, setNam] = useState(now.getFullYear());
  const [report, setReport] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const load = async () => {
    setError("");
    setLoading(true);
    try {
      const data = await api.get(`/admin/reports?thang=${thang}&nam=${nam}`, auth.token);
      setReport(data);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h2>Báo cáo thống kê</h2>

      <div className="card" style={{ marginBottom: 24, display: "flex", gap: 8, alignItems: "flex-end" }}>
        <div className="field" style={{ margin: 0 }}>
          <label>Tháng</label>
          <select value={thang} onChange={(e) => setThang(Number(e.target.value))}>
            {Array.from({ length: 12 }, (_, i) => i + 1).map((m) => <option key={m} value={m}>{m}</option>)}
          </select>
        </div>
        <div className="field" style={{ margin: 0 }}>
          <label>Năm</label>
          <input type="number" value={nam} onChange={(e) => setNam(Number(e.target.value))} style={{ width: 100 }} />
        </div>
        <button className="btn btn-primary" style={{ height: 36, padding: "0 16px" }} disabled={loading} onClick={load}>
          {loading ? "Đang tải..." : "Xem báo cáo"}
        </button>
      </div>

      {error && <p className="error-text">{error}</p>}

      {report && (
        <div className="card">
          <h3>Báo cáo tháng {report.thang}/{report.nam}</h3>
          <table style={{ width: "100%", borderCollapse: "collapse" }}>
            <thead>
              <tr style={{ textAlign: "left", borderBottom: "2px solid var(--border)" }}>
                <th style={{ padding: 8 }}>Chỉ tiêu</th>
                <th style={{ padding: 8 }}>Số lượng</th>
              </tr>
            </thead>
            <tbody>
              {report.chiTieu.map((c) => (
                <tr key={c.ten} style={{ borderBottom: "1px solid var(--border)" }}>
                  <td style={{ padding: 8 }}>{c.ten}</td>
                  <td style={{ padding: 8, fontWeight: 700, color: "var(--indigo)" }}>{c.soLuong}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
