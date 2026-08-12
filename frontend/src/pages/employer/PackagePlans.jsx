import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { api } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

export default function PackagePlans() {
  const { auth } = useAuth();
  const [goiHienTai, setGoiHienTai] = useState(null);
  const [danhSachGoi, setDanhSachGoi] = useState([]);
  const [dangTai, setDangTai] = useState(true);
  const [loadError, setLoadError] = useState(false);

  const load = () => {
    setLoadError(false);
    setDangTai(true);
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
              <div
                key={goi.maGoi}
                className="card"
                style={goi.coNoiBat ? { border: "2px solid var(--warning)", position: "relative" } : undefined}
              >
                {goi.coNoiBat && (
                  <span className="badge badge-warning" style={{ position: "absolute", top: -12, left: 20 }}>
                    ★ Được chọn nhiều nhất
                  </span>
                )}
                <p style={{ fontSize: 20, fontWeight: 700, margin: "10px 0 4px" }}>{goi.tenGoi}</p>
                <p style={{ fontSize: 26, fontWeight: 700, color: "var(--indigo)", margin: "0 0 8px" }}>
                  {goi.giaTien.toLocaleString("vi-VN")}đ <span style={{ fontSize: 14, fontWeight: 400, color: "var(--text-muted)" }}>/ {goi.thoiHan} ngày</span>
                </p>
                <p style={{ margin: "0 0 16px", color: "var(--text-muted)" }}>Giới hạn {goi.gioiHanTin} tin đăng đồng thời</p>
                <Link to={`/employer/service-packages/${goi.maGoi}/checkout`} className="btn btn-primary" style={{ width: "100%", textAlign: "center", display: "block" }}>
                  Mua gói
                </Link>
              </div>
            ))}
          </div>
        </>
      )}
    </div>
  );
}
