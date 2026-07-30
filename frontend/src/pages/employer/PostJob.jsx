import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

export default function PostJob() {
  const { auth } = useAuth();
  const navigate = useNavigate();
  const [skills, setSkills] = useState([]);
  const [selectedSkills, setSelectedSkills] = useState({}); // { maKyNang: mucDoUuTien }
  const [form, setForm] = useState({
    tieuDe: "", moTaCongViec: "", yeuCauUngVien: "", quyenLoi: "", mucLuong: "",
    diaDiem: "", hinhThucLamViec: "FullTime", soNamKinhNghiemYeuCau: "", hanNopHoSo: "",
  });
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    api.get("/skills").then(setSkills);
  }, []);

  const set = (key) => (e) => setForm({ ...form, [key]: e.target.value });

  const toggleSkill = (maKyNang) => {
    setSelectedSkills((prev) => {
      const next = { ...prev };
      if (next[maKyNang]) delete next[maKyNang];
      else next[maKyNang] = "BatBuoc";
      return next;
    });
  };

  const setMucDoUuTien = (maKyNang, value) => {
    setSelectedSkills((prev) => ({ ...prev, [maKyNang]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setSuccess("");
    setLoading(true);
    try {
      const body = {
        ...form,
        soNamKinhNghiemYeuCau: form.soNamKinhNghiemYeuCau ? Number(form.soNamKinhNghiemYeuCau) : null,
        kyNangYeuCau: Object.entries(selectedSkills).map(([maKyNang, mucDoUuTien]) => ({
          maKyNang: Number(maKyNang), mucDoUuTien,
        })),
      };
      await api.post("/jobs", body, auth.token);
      setSuccess("Đăng tin thành công, tin đang chờ Admin duyệt."); // MS05
      setTimeout(() => navigate("/"), 1500);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page-container" style={{ maxWidth: 720 }}>
      <div className="card">
        <h2>Đăng tin tuyển dụng</h2>
        <form onSubmit={handleSubmit}>
          <div className="field">
            <label>Tiêu đề tin</label>
            <input value={form.tieuDe} onChange={set("tieuDe")} required />
          </div>
          <div className="field">
            <label>Mô tả công việc</label>
            <textarea rows={4} value={form.moTaCongViec} onChange={set("moTaCongViec")} required />
          </div>
          <div className="field">
            <label>Yêu cầu ứng viên</label>
            <textarea rows={3} value={form.yeuCauUngVien} onChange={set("yeuCauUngVien")} />
          </div>
          <div className="field">
            <label>Quyền lợi</label>
            <textarea rows={3} value={form.quyenLoi} onChange={set("quyenLoi")} />
          </div>
          <div className="field">
            <label>Mức lương</label>
            <input value={form.mucLuong} onChange={set("mucLuong")} placeholder="VD: 15-20 triệu" />
          </div>
          <div className="field">
            <label>Địa điểm làm việc</label>
            <input value={form.diaDiem} onChange={set("diaDiem")} />
          </div>
          <div className="field">
            <label>Hình thức làm việc</label>
            <select value={form.hinhThucLamViec} onChange={set("hinhThucLamViec")}>
              <option value="FullTime">Full-time</option>
              <option value="PartTime">Part-time</option>
              <option value="Remote">Remote</option>
            </select>
          </div>
          <div className="field">
            <label>Số năm kinh nghiệm yêu cầu</label>
            <input type="number" min="0" value={form.soNamKinhNghiemYeuCau} onChange={set("soNamKinhNghiemYeuCau")} />
          </div>
          <div className="field">
            <label>Hạn nộp hồ sơ</label>
            <input type="date" value={form.hanNopHoSo} onChange={set("hanNopHoSo")} required />
          </div>
          <div className="field">
            <label>Danh sách kỹ năng yêu cầu</label>
            <div className="card" style={{ maxHeight: 220, overflowY: "auto" }}>
              {skills.map((s) => (
                <div key={s.maKyNang} style={{ display: "flex", alignItems: "center", gap: 8, marginBottom: 6 }}>
                  <input
                    type="checkbox"
                    style={{ height: "auto", width: "auto" }}
                    checked={!!selectedSkills[s.maKyNang]}
                    onChange={() => toggleSkill(s.maKyNang)}
                  />
                  <span style={{ flex: 1 }}>{s.tenKyNang}</span>
                  {selectedSkills[s.maKyNang] && (
                    <select
                      style={{ height: 32, width: 120 }}
                      value={selectedSkills[s.maKyNang]}
                      onChange={(e) => setMucDoUuTien(s.maKyNang, e.target.value)}
                    >
                      <option value="BatBuoc">Bắt buộc</option>
                      <option value="UuTien">Ưu tiên</option>
                    </select>
                  )}
                </div>
              ))}
            </div>
          </div>
          {error && <p className="error-text">{error}</p>}
          {success && <p className="success-text">{success}</p>}
          <button className="btn btn-primary" style={{ width: "100%" }} disabled={loading} type="submit">
            {loading ? "Đang đăng..." : "Đăng tin"}
          </button>
        </form>
      </div>
    </div>
  );
}
