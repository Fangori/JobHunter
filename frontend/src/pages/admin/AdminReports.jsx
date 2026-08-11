import { useEffect, useState } from "react";
import { LineChart, Line, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from "recharts";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";
import { layDanhSach6ThangGanNhat } from "../../utils/reportMonths";

const now = new Date();
const TEN_DOANH_THU = "Doanh thu gói dịch vụ";

const MAU_CHI_TIEU = {
  "Tài khoản Ứng viên mới": "#3949c6",
  "Tài khoản NTD mới": "#1f9d55",
  "Tin tuyển dụng mới": "#b7791f",
  "Đơn ứng tuyển mới": "#d64545",
};

// Dung lai dung 3 mau da co san (khop voi MAU_CHI_TIEU: indigo cho Ung
// vien, success cho NTD) - them 1 mau moi (warning) cho Admin, thay vi
// bay ngau nhien mau khac vao he mau da chot cua trang.
const MAU_VAI_TRO = { UngVien: "#3949c6", NhaTuyenDung: "#1f9d55", Admin: "#b7791f" };

export default function AdminReports() {
  const { auth } = useAuth();
  const [thang, setThang] = useState(now.getMonth() + 1);
  const [nam, setNam] = useState(now.getFullYear());
  const [report, setReport] = useState(null);
  const [trendData, setTrendData] = useState([]);
  const [phanBoVaiTro, setPhanBoVaiTro] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  // Snapshot tong so tai khoan theo vai tro tinh den hien tai - khac voi
  // bao cao theo thang/nam ben duoi, nen nap 1 lan khi vao trang.
  useEffect(() => {
    api.get("/admin/reports/phan-bo-vai-tro", auth.token).catch(() => null).then(setPhanBoVaiTro);
  }, [auth.token]);

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

  const chiTieuDem = report ? report.chiTieu.filter((c) => c.ten !== TEN_DOANH_THU) : [];
  const doanhThu = report ? report.chiTieu.find((c) => c.ten === TEN_DOANH_THU)?.soLuong ?? 0 : 0;
  const maxSoLuong = Math.max(1, ...chiTieuDem.map((c) => c.soLuong));

  const tongTaiKhoan = phanBoVaiTro ? phanBoVaiTro.soUngVien + phanBoVaiTro.soNhaTuyenDung + phanBoVaiTro.soAdmin : 0;
  const phanBoData = phanBoVaiTro
    ? [{ ten: "Tài khoản", ungVien: phanBoVaiTro.soUngVien, nhaTuyenDung: phanBoVaiTro.soNhaTuyenDung, admin: phanBoVaiTro.soAdmin }]
    : [];

  return (
    <div>
      <div className="dashboard-header-band">
        <h2>Báo cáo thống kê</h2>
      </div>

      {tongTaiKhoan > 0 && (
        <div className="card" style={{ marginBottom: 24 }}>
          <h3 style={{ marginTop: 0 }}>Phân bố vai trò người dùng ({tongTaiKhoan} tài khoản)</h3>
          <ResponsiveContainer width="100%" height={90}>
            <BarChart data={phanBoData} layout="vertical" margin={{ top: 0, right: 0, bottom: 0, left: 0 }}>
              <XAxis type="number" hide domain={[0, tongTaiKhoan]} />
              <YAxis type="category" dataKey="ten" hide />
              <Tooltip />
              {/* Luu y: Recharts tu dat thu tu Legend cua stacked BarChart
                  theo internal state, khong theo thu tu payload truyen vao
                  (da thu doi ca 2 chieu, khong doi) - chap nhan thu tu mac
                  dinh, mau/nhan/so lieu van dung 1-1 voi thanh. */}
              <Legend />
              {/* stroke mau nen the (surface gap) de tach cac doan xep chong -
                  khong thi 3 mau dinh lien nhau, kho phan biet ranh gioi. */}
              <Bar dataKey="ungVien" stackId="vaiTro" fill={MAU_VAI_TRO.UngVien} stroke="#ffffff" strokeWidth={2} name={`Ứng viên (${phanBoVaiTro.soUngVien})`} barSize={24} radius={[4, 0, 0, 4]} />
              <Bar dataKey="nhaTuyenDung" stackId="vaiTro" fill={MAU_VAI_TRO.NhaTuyenDung} stroke="#ffffff" strokeWidth={2} name={`Nhà tuyển dụng (${phanBoVaiTro.soNhaTuyenDung})`} barSize={24} />
              <Bar dataKey="admin" stackId="vaiTro" fill={MAU_VAI_TRO.Admin} stroke="#ffffff" strokeWidth={2} name={`Admin (${phanBoVaiTro.soAdmin})`} barSize={24} radius={[0, 4, 4, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}

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
          <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(220px, 1fr))", gap: 12, marginBottom: 16 }}>
            {chiTieuDem.map((c) => (
              <div key={c.ten} className="card">
                <p style={{ margin: 0, color: "var(--text-muted)", fontSize: 14 }}>{c.ten}</p>
                <p style={{ fontSize: 28, fontWeight: 700, margin: "4px 0 8px", color: "var(--indigo)" }}>{c.soLuong}</p>
                <div style={{ height: 6, borderRadius: 3, background: "var(--bg)", overflow: "hidden" }}>
                  <div style={{ height: "100%", width: `${(c.soLuong / maxSoLuong) * 100}%`, background: "var(--indigo)" }} />
                </div>
              </div>
            ))}
          </div>

          <div className="card" style={{ marginBottom: 24, background: "linear-gradient(135deg, var(--navy), var(--indigo))" }}>
            <p style={{ margin: 0, color: "rgba(255,255,255,0.85)", fontSize: 14 }}>Doanh thu gói dịch vụ</p>
            <p style={{ fontSize: 32, fontWeight: 700, margin: "4px 0 0", color: "white" }}>{doanhThu.toLocaleString("vi-VN")}đ</p>
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
