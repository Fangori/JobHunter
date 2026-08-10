import { useEffect, useState } from "react";
import { CheckCircle2 } from "lucide-react";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

function ThanhToanModal({ goi, onClose, onSuccess }) {
  const { auth } = useAuth();
  const [phuongThucThanhToan, setPhuongThucThanhToan] = useState("TheNganHang");
  const [thongTinThanhToan, setThongTinThanhToan] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleConfirm = async () => {
    setError("");
    setLoading(true);
    try {
      const result = await api.post(`/packages/${goi.maGoi}/mua`, { phuongThucThanhToan, thongTinThanhToan }, auth.token);
      onSuccess(result.message);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ position: "fixed", inset: 0, background: "rgba(0,0,0,0.4)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 100 }}>
      <div className="card" style={{ width: 420, background: "white" }}>
        <h3 style={{ marginTop: 0 }}>Thanh toán mua gói {goi.tenGoi}</h3>
        <p style={{ color: "var(--text-muted)" }}>Số tiền: {goi.giaTien.toLocaleString("vi-VN")}đ</p>
        <div className="field">
          <label>Phương thức thanh toán</label>
          <select value={phuongThucThanhToan} onChange={(e) => setPhuongThucThanhToan(e.target.value)}>
            <option value="TheNganHang">Thẻ ngân hàng</option>
            <option value="ChuyenKhoan">Chuyển khoản</option>
          </select>
        </div>
        <div className="field">
          <label>Thông tin thanh toán</label>
          <input
            placeholder={phuongThucThanhToan === "TheNganHang" ? "Số thẻ (giả lập)" : "Số tài khoản (giả lập)"}
            value={thongTinThanhToan}
            onChange={(e) => setThongTinThanhToan(e.target.value)}
          />
        </div>
        {error && <p className="error-text">{error}</p>}
        <div style={{ display: "flex", gap: 8, marginTop: 12 }}>
          <button className="btn btn-primary" style={{ flex: 1 }} disabled={loading} onClick={handleConfirm}>
            {loading ? "Đang xử lý..." : "Xác nhận thanh toán"}
          </button>
          <button className="btn btn-secondary" style={{ flex: 1 }} onClick={onClose} disabled={loading}>Hủy</button>
        </div>
      </div>
    </div>
  );
}

function MuaGoiThanhCongPopup({ message, onClose }) {
  return (
    <div style={{ position: "fixed", inset: 0, background: "rgba(0,0,0,0.4)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 100 }}>
      <div className="card" style={{ width: 400, background: "white", textAlign: "center" }}>
        <CheckCircle2 size={40} color="var(--success)" style={{ marginBottom: 12 }} />
        <p className="success-text" style={{ fontSize: 15 }}>{message}</p>
        <button className="btn btn-primary" style={{ width: "100%" }} onClick={onClose}>Đóng</button>
      </div>
    </div>
  );
}

export default function PackagePlans() {
  const { auth } = useAuth();
  const [goiHienTai, setGoiHienTai] = useState(null);
  const [danhSachGoi, setDanhSachGoi] = useState([]);
  const [dangTai, setDangTai] = useState(true);
  const [loadError, setLoadError] = useState(false);
  const [muaGoi, setMuaGoi] = useState(null);
  const [thanhCongMessage, setThanhCongMessage] = useState("");

  const load = () => {
    setLoadError(false);
    api.get("/packages", auth.token)
      .then((data) => {
        setGoiHienTai(data.goiHienTai);
        setDanhSachGoi(data.danhSachGoi);
      })
      .catch(() => setLoadError(true))
      .finally(() => setDangTai(false));
  };

  useEffect(() => {
    load();
  }, []);

  const handleMuaThanhCong = (message) => {
    setMuaGoi(null);
    setThanhCongMessage(message);
    load();
  };

  return (
    <div className="page-container">
      <div className="dashboard-header-band">
        <h1>Mua gói dịch vụ</h1>
      </div>

      {loadError && (
        <>
          <p className="error-text">Không tải được dữ liệu.</p>
          <button className="btn btn-secondary" onClick={load}>Thử lại</button>
        </>
      )}

      {!loadError && dangTai && <p>Đang tải...</p>}

      {!loadError && !dangTai && (
        <>
          <div className="card" style={{ marginBottom: 24 }}>
            <h3 style={{ marginTop: 0 }}>Gói hiện tại</h3>
            <p style={{ fontSize: 22, fontWeight: 700, color: "var(--indigo)", margin: "4px 0" }}>{goiHienTai.tenGoi}</p>
            <p style={{ margin: 0, color: "var(--text-muted)" }}>
              Giới hạn {goiHienTai.gioiHanTin} tin đăng đồng thời
              {goiHienTai.ngayHetHan && ` · Hết hạn: ${new Date(goiHienTai.ngayHetHan).toLocaleDateString("vi-VN")}`}
            </p>
          </div>

          <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(260px, 1fr))", gap: 16 }}>
            {danhSachGoi.map((goi) => (
              <div key={goi.maGoi} className="card">
                {goi.coNoiBat && <span className="badge badge-warning">Nổi bật</span>}
                <p style={{ fontSize: 20, fontWeight: 700, margin: "10px 0 4px" }}>{goi.tenGoi}</p>
                <p style={{ fontSize: 26, fontWeight: 700, color: "var(--indigo)", margin: "0 0 8px" }}>
                  {goi.giaTien.toLocaleString("vi-VN")}đ <span style={{ fontSize: 14, fontWeight: 400, color: "var(--text-muted)" }}>/ {goi.thoiHan} ngày</span>
                </p>
                <p style={{ margin: "0 0 16px", color: "var(--text-muted)" }}>Giới hạn {goi.gioiHanTin} tin đăng đồng thời</p>
                <button className="btn btn-primary" style={{ width: "100%" }} onClick={() => setMuaGoi(goi)}>Mua gói</button>
              </div>
            ))}
          </div>
        </>
      )}

      {muaGoi && (
        <ThanhToanModal goi={muaGoi} onClose={() => setMuaGoi(null)} onSuccess={handleMuaThanhCong} />
      )}
      {thanhCongMessage && (
        <MuaGoiThanhCongPopup message={thanhCongMessage} onClose={() => setThanhCongMessage("")} />
      )}
    </div>
  );
}
