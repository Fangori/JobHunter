import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

export default function PostJob() {
  const { auth } = useAuth();
  const navigate = useNavigate();
  const { id } = useParams();
  const isEdit = !!id;
  const [skills, setSkills] = useState([]);
  const [selectedSkills, setSelectedSkills] = useState({}); // { maKyNang: mucDoUuTien }
  const [form, setForm] = useState({
    tieuDe: "", moTaCongViec: "", yeuCauUngVien: "", quyenLoi: "", mucLuong: "",
    diaDiem: "", hinhThucLamViec: "FullTime", soNamKinhNghiemYeuCau: "", hanNopHoSo: "",
  });
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);
  const [dangTai, setDangTai] = useState(isEdit);

  useEffect(() => {
    api.get("/skills").then(setSkills);
    if (isEdit) {
      api.get(`/jobs/${id}`).then((job) => {
        setForm({
          tieuDe: job.tieuDe,
          moTaCongViec: job.moTaCongViec,
          yeuCauUngVien: job.yeuCauUngVien || "",
          quyenLoi: job.quyenLoi || "",
          mucLuong: job.mucLuong || "",
          diaDiem: job.diaDiem || "",
          hinhThucLamViec: job.hinhThucLamViec || "FullTime",
          soNamKinhNghiemYeuCau: job.soNamKinhNghiemYeuCau ?? "",
          hanNopHoSo: job.hanNopHoSo,
        });
        const skillMap = {};
        job.kyNangYeuCau.forEach((k) => (skillMap[k.maKyNang] = k.mucDoUuTien || "BatBuoc"));
        setSelectedSkills(skillMap);
      }).finally(() => setDangTai(false));
    }
  }, [id]);

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
      if (isEdit) {
        const result = await api.put(`/jobs/${id}`, body, auth.token);
        setSuccess(result.message); // MS41 hoac thong bao chung
      } else {
        await api.post("/jobs", body, auth.token);
        setSuccess("Đăng tin thành công, tin đang chờ Admin duyệt."); // MS05
      }
      setTimeout(() => navigate("/employer/my-jobs"), 1500);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    } finally {
      setLoading(false);
    }
  };

  if (dangTai) return <div className="page-container">Đang tải...</div>;

  return (
    <div className="page-container" style={{ maxWidth: 760 }}>
      <div className="dashboard-header-band">
        <h2>{isEdit ? "Sửa tin tuyển dụng" : "Đăng tin tuyển dụng"}</h2>
      </div>
      <div className="card">
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
          <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 16 }}>
            <div className="field">
              <label>Mức lương</label>
              <input value={form.mucLuong} onChange={set("mucLuong")} placeholder="VD: 15-20 triệu" />
            </div>
            <div className="field">
              <label>Số năm kinh nghiệm yêu cầu</label>
              <input type="number" min="0" value={form.soNamKinhNghiemYeuCau} onChange={set("soNamKinhNghiemYeuCau")} />
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
              <label>Hạn nộp hồ sơ</label>
              <input type="date" value={form.hanNopHoSo} onChange={set("hanNopHoSo")} required />
            </div>
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
            {loading ? "Đang lưu..." : isEdit ? "Cập nhật tin" : "Đăng tin"}
          </button>
        </form>
      </div>
    </div>
  );
}
