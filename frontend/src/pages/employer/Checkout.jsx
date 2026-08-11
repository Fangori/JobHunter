import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { CheckCircle2, ShieldCheck } from "lucide-react";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

// UC43 - trang thanh toan rieng (khong con la modal) de co du khong gian
// hien thi tom tat don hang + form thanh toan tren man hinh may tinh.
export default function Checkout() {
  const { maGoi } = useParams();
  const { auth } = useAuth();

  const [goi, setGoi] = useState(null);
  const [dangTai, setDangTai] = useState(true);
  const [loadError, setLoadError] = useState(false);

  const [phuongThucThanhToan, setPhuongThucThanhToan] = useState("TheNganHang");
  // Thong tin the (giao dien day du hon cho thuc te, van la gia lap - khong
  // xu ly thanh toan that, chi ghep thanh 1 chuoi ThongTinThanhToan gui len API).
  const [soThe, setSoThe] = useState("");
  const [tenChuThe, setTenChuThe] = useState("");
  const [hanThe, setHanThe] = useState("");
  const [cvv, setCvv] = useState("");
  // Thong tin chuyen khoan
  const [nganHang, setNganHang] = useState("");
  const [soTaiKhoan, setSoTaiKhoan] = useState("");
  const [tenChuTk, setTenChuTk] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [thanhCongMessage, setThanhCongMessage] = useState("");

  const formatSoThe = (v) => v.replace(/\D/g, "").slice(0, 16).replace(/(.{4})/g, "$1 ").trim();
  const formatHanThe = (v) => {
    const digits = v.replace(/\D/g, "").slice(0, 4);
    return digits.length > 2 ? `${digits.slice(0, 2)}/${digits.slice(2)}` : digits;
  };

  useEffect(() => {
    setLoadError(false);
    setDangTai(true);
    api.get("/packages", auth.token)
      .then((data) => {
        const found = data.danhSachGoi.find((g) => String(g.maGoi) === maGoi);
        if (!found) throw new Error("not found");
        setGoi(found);
      })
      .catch(() => setLoadError(true))
      .finally(() => setDangTai(false));
  }, [maGoi, auth.token]);

  const handleConfirm = async (e) => {
    e.preventDefault();
    setError("");

    let thongTinThanhToan;
    if (phuongThucThanhToan === "TheNganHang") {
      if (!soThe.trim() || !tenChuThe.trim() || hanThe.length < 5 || cvv.trim().length < 3) {
        setError("Thanh toán thất bại, vui lòng thử lại."); // MS61
        return;
      }
      const soTheDigits = soThe.replace(/\D/g, "");
      thongTinThanhToan = `Thẻ **** **** **** ${soTheDigits.slice(-4)} - ${tenChuThe.trim().toUpperCase()} - Hết hạn ${hanThe}`;
    } else {
      if (!nganHang.trim() || !soTaiKhoan.trim() || !tenChuTk.trim()) {
        setError("Thanh toán thất bại, vui lòng thử lại."); // MS61
        return;
      }
      thongTinThanhToan = `${nganHang.trim()} - STK ${soTaiKhoan.trim()} - ${tenChuTk.trim().toUpperCase()}`;
    }

    setLoading(true);
    try {
      const result = await api.post(`/packages/${goi.maGoi}/mua`, { phuongThucThanhToan, thongTinThanhToan }, auth.token);
      setThanhCongMessage(result.message);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    } finally {
      setLoading(false);
    }
  };

  if (dangTai) {
    return (
      <div className="page-container">
        <div className="dashboard-header-band"><h1>Thanh toán</h1></div>
        <div className="card"><p>Đang tải...</p></div>
      </div>
    );
  }

  if (loadError || !goi) {
    return (
      <div className="page-container">
        <div className="dashboard-header-band"><h1>Thanh toán</h1></div>
        <div className="card">
          <p className="error-text">Không tìm thấy gói dịch vụ này.</p>
          <Link to="/employer/service-packages" className="btn btn-secondary">Quay lại danh sách gói</Link>
        </div>
      </div>
    );
  }

  if (thanhCongMessage) {
    return (
      <div className="page-container">
        <div className="dashboard-header-band"><h1>Thanh toán</h1></div>
        <div className="card" style={{ textAlign: "center", padding: "48px 24px" }}>
          <CheckCircle2 size={48} color="var(--success)" style={{ marginBottom: 16 }} />
          <p className="success-text" style={{ fontSize: 17, marginBottom: 24 }}>{thanhCongMessage}</p>
          <Link to="/employer/service-packages" className="btn btn-primary">Về trang gói dịch vụ</Link>
        </div>
      </div>
    );
  }

  return (
    <div className="page-container">
      <div className="dashboard-header-band">
        <h1>Thanh toán</h1>
        <p>Hoàn tất thanh toán để kích hoạt gói dịch vụ ngay lập tức.</p>
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "1fr 380px", gap: 24, alignItems: "start" }}>
        <div className="card">
          <h3 style={{ marginTop: 0 }}>Thông tin thanh toán</h3>
          <form onSubmit={handleConfirm}>
            <div className="field">
              <label>Phương thức thanh toán</label>
              <select value={phuongThucThanhToan} onChange={(e) => setPhuongThucThanhToan(e.target.value)}>
                <option value="TheNganHang">Thẻ ngân hàng</option>
                <option value="ChuyenKhoan">Chuyển khoản</option>
              </select>
            </div>

            {phuongThucThanhToan === "TheNganHang" ? (
              <>
                <div className="field">
                  <label>Số thẻ</label>
                  <input
                    placeholder="1234 5678 9012 3456"
                    value={soThe}
                    onChange={(e) => setSoThe(formatSoThe(e.target.value))}
                    inputMode="numeric"
                  />
                </div>
                <div className="field">
                  <label>Tên chủ thẻ</label>
                  <input
                    placeholder="NGUYEN VAN A"
                    value={tenChuThe}
                    onChange={(e) => setTenChuThe(e.target.value)}
                    style={{ textTransform: "uppercase" }}
                  />
                </div>
                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 16 }}>
                  <div className="field">
                    <label>Ngày hết hạn</label>
                    <input
                      placeholder="MM/YY"
                      value={hanThe}
                      onChange={(e) => setHanThe(formatHanThe(e.target.value))}
                      inputMode="numeric"
                    />
                  </div>
                  <div className="field">
                    <label>CVV</label>
                    <input
                      placeholder="123"
                      value={cvv}
                      onChange={(e) => setCvv(e.target.value.replace(/\D/g, "").slice(0, 4))}
                      inputMode="numeric"
                      maxLength={4}
                    />
                  </div>
                </div>
              </>
            ) : (
              <>
                <div className="field">
                  <label>Ngân hàng</label>
                  <input
                    placeholder="VD: Vietcombank"
                    value={nganHang}
                    onChange={(e) => setNganHang(e.target.value)}
                  />
                </div>
                <div className="field">
                  <label>Số tài khoản</label>
                  <input
                    placeholder="Số tài khoản (giả lập)"
                    value={soTaiKhoan}
                    onChange={(e) => setSoTaiKhoan(e.target.value.replace(/\D/g, ""))}
                    inputMode="numeric"
                  />
                </div>
                <div className="field">
                  <label>Tên chủ tài khoản</label>
                  <input
                    placeholder="NGUYEN VAN A"
                    value={tenChuTk}
                    onChange={(e) => setTenChuTk(e.target.value)}
                    style={{ textTransform: "uppercase" }}
                  />
                </div>
              </>
            )}

            <p style={{ display: "flex", alignItems: "center", gap: 8, color: "var(--text-muted)", fontSize: 13 }}>
              <ShieldCheck size={16} /> Đây là giao dịch giả lập cho mục đích demo, không có xử lý thanh toán thật.
            </p>
            {error && <p className="error-text">{error}</p>}
            <div style={{ display: "flex", gap: 8, marginTop: 12 }}>
              <button className="btn btn-primary" style={{ flex: 1 }} disabled={loading} type="submit">
                {loading ? "Đang xử lý..." : "Xác nhận thanh toán"}
              </button>
              <Link to="/employer/service-packages" className="btn btn-secondary" style={{ flex: 1, textAlign: "center" }}>Hủy</Link>
            </div>
          </form>
        </div>

        <div className="card" style={{ position: "sticky", top: 24 }}>
          <h3 style={{ marginTop: 0 }}>Tóm tắt đơn hàng</h3>
          {goi.coNoiBat && <span className="badge badge-warning" style={{ marginBottom: 8 }}>Nổi bật</span>}
          <p style={{ fontSize: 20, fontWeight: 700, margin: "8px 0 4px" }}>{goi.tenGoi}</p>
          <p style={{ color: "var(--text-muted)", margin: "0 0 16px" }}>Giới hạn {goi.gioiHanTin} tin đăng đồng thời · {goi.thoiHan} ngày</p>
          <div style={{ borderTop: "1px solid var(--border)", paddingTop: 16, display: "flex", justifyContent: "space-between", alignItems: "baseline" }}>
            <span style={{ color: "var(--text-muted)" }}>Tổng cộng</span>
            <span style={{ fontSize: 26, fontWeight: 700, color: "var(--indigo)" }}>{goi.giaTien.toLocaleString("vi-VN")}đ</span>
          </div>
        </div>
      </div>
    </div>
  );
}
