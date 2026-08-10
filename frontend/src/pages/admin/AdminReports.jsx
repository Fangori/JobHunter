import { useState } from "react";
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from "recharts";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";
import { layDanhSach6ThangGanNhat } from "../../utils/reportMonths";

const now = new Date();

const MAU_CHI_TIEU = {
  "Tài khoản Ứng viên mới": "#3949c6",
  "Tài khoản NTD mới": "#1f9d55",
  "Tin tuyển dụng mới": "#b7791f",
  "Đơn ứng tuyển mới": "#d64545",
};

export default function AdminReports() {
  const { auth } = useAuth();
  const [thang, setThang] = useState(now.getMonth() + 1);
  const [nam, setNam] = useState(now.getFullYear());
  const [report, setReport] = useState(null);
  const [trendData, setTrendData] = useState([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const load = async () => {
    setError("");
    setLoading(true);
    try {
      const thangs = layDanhSach6ThangGanNhat(thang, nam);
      const ketQua = await Promise.all(
        thangs.map((t) => api.get(`/admin/reports?thang=${t.thang}&nam=${t.nam}`, auth.token))
      );
      setReport(ketQua[ketQua.length - 1]);
      setTrendData(
        ketQua.map((r) => ({
          nhan: `T${r.thang}/${r.nam}`,
          ...Object.fromEntries(r.chiTieu.map((c) => [c.ten, c.soLuong])),
        }))
      );
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
      setReport(null);
      setTrendData([]);
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
          <input type="number" value={nam} onChange={(e) => setNam(Number(e.target.value))} min={2000} max={2100} style={{ width: 100 }} />
        </div>
        <button className="btn btn-primary" style={{ height: 36, padding: "0 16px" }} disabled={loading} onClick={load}>
          {loading ? "Đang tải..." : "Xem báo cáo"}
        </button>
      </div>

      {error && <p className="error-text">{error}</p>}

      {report && (
        <div>
          <h3>Báo cáo tháng {report.thang}/{report.nam}</h3>
          <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(220px, 1fr))", gap: 12, marginBottom: 24 }}>
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

          <div className="card">
            <h3 style={{ marginTop: 0 }}>Xu hướng 6 tháng gần nhất</h3>
            <ResponsiveContainer width="100%" height={320}>
              <LineChart data={trendData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="nhan" />
                <YAxis allowDecimals={false} />
                <Tooltip />
                <Legend />
                {Object.entries(MAU_CHI_TIEU).map(([ten, mau]) => (
                  <Line key={ten} type="monotone" dataKey={ten} stroke={mau} strokeWidth={2} dot />
                ))}
              </LineChart>
            </ResponsiveContainer>
          </div>
        </div>
      )}
    </div>
  );
}
