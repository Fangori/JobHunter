import { useEffect, useState } from "react";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

export default function ApplyModal({ maTin, onClose }) {
  const { auth } = useAuth();
  const [cvs, setCvs] = useState([]);
  const [maCv, setMaCv] = useState("");
  const [thuGioiThieu, setThuGioiThieu] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    api.get("/cvs/mine", auth.token).then((data) => {
      setCvs(data);
      if (data.length > 0) setMaCv(data[0].maCV);
    });
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      await api.post("/applications", { maCv: Number(maCv), maTin, thuGioiThieu: thuGioiThieu || undefined }, auth.token);
      setSuccess("Ứng tuyển thành công."); // MS30
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra."); // MS31/MS32
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{
      position: "fixed", inset: 0, background: "rgba(0,0,0,0.4)",
      display: "flex", alignItems: "center", justifyContent: "center", zIndex: 100,
    }}>
      <div className="card" style={{ width: 420, background: "white" }}>
        <h3>Đơn ứng tuyển</h3>
        {success ? (
          <>
            <p className="success-text">{success}</p>
            <button className="btn btn-primary" style={{ width: "100%" }} onClick={onClose}>Đóng</button>
          </>
        ) : (
          <form onSubmit={handleSubmit}>
            <div className="field">
              <label>Chọn CV</label>
              {cvs.length === 0 ? (
                <p className="error-text">Bạn chưa có CV nào. Vui lòng tạo CV trước khi ứng tuyển.</p>
              ) : (
                <select value={maCv} onChange={(e) => setMaCv(e.target.value)} required>
                  {cvs.map((cv) => <option key={cv.maCV} value={cv.maCV}>{cv.tenCV}</option>)}
                </select>
              )}
            </div>
            <div className="field">
              <label>Thư giới thiệu (không bắt buộc)</label>
              <textarea rows={3} value={thuGioiThieu} onChange={(e) => setThuGioiThieu(e.target.value)} />
            </div>
            {error && <p className="error-text">{error}</p>}
            <div style={{ display: "flex", gap: 8 }}>
              <button type="button" className="btn btn-secondary" style={{ flex: 1 }} onClick={onClose}>Hủy</button>
              <button className="btn btn-primary" style={{ flex: 1 }} disabled={loading || cvs.length === 0} type="submit">
                {loading ? "Đang gửi..." : "Nộp đơn"}
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
}
