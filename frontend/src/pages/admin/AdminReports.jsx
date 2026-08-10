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

  const maxSoLuong = report ? Math.max(1, ...report.chiTieu.map((c) => c.soLuong)) : 1;

  return (
    <div>
      <div className="dashboard-header-band">
        <h2>Báo cáo thống kê</h2>
      </div>

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
        <div>
          <h3>Báo cáo tháng {report.thang}/{report.nam}</h3>
          <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(220px, 1fr))", gap: 12 }}>
            {report.chiTieu.map((c) => (
              <div key={c.ten} className="card">
                <p style={{ margin: 0, color: "var(--text-muted)", fontSize: 14 }}>{c.ten}</p>
                <p style={{ fontSize: 28, fontWeight: 700, margin: "4px 0 8px", color: "var(--indigo)" }}>{c.soLuong}</p>
                <div style={{ height: 6, borderRadius: 3, background: "var(--bg)", overflow: "hidden" }}>
                  <div style={{ height: "100%", width: `${(c.soLuong / maxSoLuong) * 100}%`, background: "var(--indigo)" }} />
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
