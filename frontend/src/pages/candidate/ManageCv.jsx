import { useEffect, useState } from "react";
import { api, ApiError, BASE_URL } from "../../api/client";
import { useAuth } from "../../context/AuthContext";
import FileUpload from "../../components/FileUpload";

const TRINH_DO_OPTIONS = ["TrungCap", "CaoDang", "DaiHoc", "SauDaiHoc"];
const TRINH_DO_LABEL = { TrungCap: "Trung cấp", CaoDang: "Cao đẳng", DaiHoc: "Đại học", SauDaiHoc: "Sau đại học" };

function emptyKinhNghiem() {
  return { congTy: "", viTri: "", tuNgay: "", denNgay: "", moTaCongViec: "" };
}
function emptyHocVan() {
  return { truong: "", chuyenNganh: "", tuNam: "", denNam: "" };
}
function emptyForm() {
  return { tenCv: "", viTriMongMuon: "", mucLuongMongMuon: "", trinhDoHocVan: "DaiHoc" };
}

export default function ManageCv() {
  const { auth } = useAuth();
  const [skills, setSkills] = useState([]);
  const [myCvs, setMyCvs] = useState([]);
  const [trashCvs, setTrashCvs] = useState([]);
  const [editingCvId, setEditingCvId] = useState(null); // null = dang tao moi
  const [selectedSkills, setSelectedSkills] = useState({}); // { maKyNang: mucDoThanhThao }
  const [kinhNghiem, setKinhNghiem] = useState([emptyKinhNghiem()]);
  const [hocVan, setHocVan] = useState([emptyHocVan()]);
  const [form, setForm] = useState(emptyForm());
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);
  const [trashMsg, setTrashMsg] = useState("");

  const [uploadTenCv, setUploadTenCv] = useState("");
  const [uploadFile, setUploadFile] = useState(null);
  const [uploadError, setUploadError] = useState("");
  const [uploadSuccess, setUploadSuccess] = useState("");

  const loadCvs = () => api.get("/cvs/mine", auth.token).then(setMyCvs);
  const loadTrash = () => api.get("/cvs/mine?trangThai=DaAn", auth.token).then(setTrashCvs);

  useEffect(() => {
    api.get("/skills").then(setSkills);
    loadCvs();
    loadTrash();
  }, []);

  const set = (key) => (e) => setForm({ ...form, [key]: e.target.value });

  const toggleSkill = (maKyNang) => {
    setSelectedSkills((prev) => {
      const next = { ...prev };
      if (next[maKyNang]) delete next[maKyNang];
      else next[maKyNang] = "ThanhThao";
      return next;
    });
  };

  const resetForm = () => {
    setEditingCvId(null);
    setForm(emptyForm());
    setSelectedSkills({});
    setKinhNghiem([emptyKinhNghiem()]);
    setHocVan([emptyHocVan()]);
  };

  const startEdit = async (maCv) => {
    setError("");
    setSuccess("");
    try {
      const detail = await api.get(`/cvs/${maCv}`, auth.token);
      setEditingCvId(maCv);
      setForm({
        tenCv: detail.tenCV,
        viTriMongMuon: detail.viTriMongMuon || "",
        mucLuongMongMuon: detail.mucLuongMongMuon || "",
        trinhDoHocVan: detail.trinhDoHocVan || "DaiHoc",
      });
      const skillMap = {};
      detail.kyNang.forEach((k) => (skillMap[k.maKyNang] = k.mucDoThanhThao || "ThanhThao"));
      setSelectedSkills(skillMap);
      setKinhNghiem(detail.kinhNghiem.length > 0 ? detail.kinhNghiem : [emptyKinhNghiem()]);
      setHocVan(detail.hocVan.length > 0 ? detail.hocVan : [emptyHocVan()]);
      window.scrollTo({ top: 0, behavior: "smooth" });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    }
  };

  const handleDelete = async (maCv) => {
    if (!window.confirm("Bạn có chắc muốn xóa CV này?")) return;
    setError("");
    setSuccess("");
    try {
      const result = await api.del(`/cvs/${maCv}`, auth.token);
      setSuccess(result.message); // MS38 hoac MS39 (BR13)
      if (editingCvId === maCv) resetForm();
      await Promise.all([loadCvs(), loadTrash()]);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    }
  };

  const handleRestore = async (maCv) => {
    setTrashMsg("");
    try {
      const result = await api.post(`/cvs/${maCv}/restore`, undefined, auth.token);
      setTrashMsg(result.message); // MS40
      await Promise.all([loadCvs(), loadTrash()]);
    } catch (err) {
      setTrashMsg(err instanceof ApiError ? err.message : "Có lỗi xảy ra.");
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setSuccess("");
    setLoading(true);
    try {
      const body = {
        ...form,
        kyNang: Object.entries(selectedSkills).map(([maKyNang, mucDoThanhThao]) => ({ maKyNang: Number(maKyNang), mucDoThanhThao })),
        kinhNghiem: kinhNghiem.filter((k) => k.congTy && k.tuNgay),
        hocVan: hocVan.filter((h) => h.truong),
      };
      if (editingCvId) {
        const result = await api.put(`/cvs/${editingCvId}`, body, auth.token);
        setSuccess(result.message); // MS37
      } else {
        await api.post("/cvs/online", body, auth.token);
        setSuccess("Tạo CV thành công."); // MS03
      }
      resetForm();
      await loadCvs();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Có lỗi xảy ra."); // MS04 tra ve tu server neu thieu field
    } finally {
      setLoading(false);
    }
  };

  const handleUpload = async (e) => {
    e.preventDefault();
    setUploadError("");
    setUploadSuccess("");
    if (!uploadFile) { setUploadError("Vui lòng chọn file CV."); return; }
    try {
      const fd = new FormData();
      fd.append("tenCv", uploadTenCv);
      fd.append("file", uploadFile);
      const res = await fetch(`${BASE_URL}/cvs/upload`, {
        method: "POST",
        headers: { Authorization: `Bearer ${auth.token}` },
        body: fd,
      });
      const data = await res.json();
      if (!res.ok) throw new ApiError(data.message, res.status);
      setUploadSuccess("Tải lên CV thành công."); // MS35
      await loadCvs();
    } catch (err) {
      setUploadError(err instanceof ApiError ? err.message : "File không đúng định dạng hoặc vượt quá 10MB."); // MS36
    }
  };

  const skillName = (id) => skills.find((s) => s.maKyNang === id)?.tenKyNang || id;

  return (
    <div className="page-container">
      <div className="dashboard-header-band">
        <h2>Quản lý CV</h2>
      </div>

      {myCvs.length > 0 && (
        <div style={{ marginBottom: 24 }}>
          <h3>CV của tôi</h3>
          <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(240px, 1fr))", gap: 12 }}>
            {myCvs.map((cv) => (
              <div key={cv.maCV} className="card">
                <span className={`badge ${cv.loaiCV === "TrucTuyen" ? "badge-info" : "badge-neutral"}`}>
                  {cv.loaiCV === "TrucTuyen" ? "Trực tuyến" : "Tải lên"}
                </span>
                <p style={{ fontWeight: 600, margin: "10px 0 4px" }}>{cv.tenCV}</p>
                {cv.viTriMongMuon && <p style={{ margin: 0, color: "var(--text-muted)", fontSize: 14 }}>{cv.viTriMongMuon}</p>}
                <div style={{ display: "flex", gap: 8, marginTop: 12 }}>
                  {cv.loaiCV === "TrucTuyen" && (
                    <button type="button" className="btn btn-secondary" style={{ height: 32, padding: "0 12px" }} onClick={() => startEdit(cv.maCV)}>
                      Sửa
                    </button>
                  )}
                  <button type="button" className="btn btn-secondary" style={{ height: 32, padding: "0 12px" }} onClick={() => handleDelete(cv.maCV)}>
                    Xóa
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {(trashCvs.length > 0 || trashMsg) && (
        <div style={{ marginBottom: 24 }}>
          <h3>Thùng rác CV</h3>
          {trashMsg && <p className={trashMsg.includes("thành công") ? "success-text" : "error-text"}>{trashMsg}</p>}
          {trashCvs.length === 0 && <p style={{ color: "var(--text-muted)" }}>Thùng rác trống.</p>}
          <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(240px, 1fr))", gap: 12 }}>
            {trashCvs.map((cv) => (
              <div key={cv.maCV} className="card">
                <span className="badge badge-neutral">Đã lưu trữ</span>
                <p style={{ fontWeight: 600, margin: "10px 0 12px" }}>{cv.tenCV}</p>
                <button type="button" className="btn btn-primary" style={{ height: 32, padding: "0 12px" }} onClick={() => handleRestore(cv.maCV)}>
                  Phục hồi
                </button>
              </div>
            ))}
          </div>
        </div>
      )}

      <div style={{ display: "flex", gap: 24 }}>
        <div className="card" style={{ flex: 1 }}>
          <h3>{editingCvId ? "Sửa CV" : "Tạo CV trực tuyến"}</h3>
          <form onSubmit={handleSubmit}>
            <div className="field">
              <label>Tên CV</label>
              <input value={form.tenCv} onChange={set("tenCv")} required />
            </div>
            <div className="field">
              <label>Vị trí mong muốn</label>
              <input value={form.viTriMongMuon} onChange={set("viTriMongMuon")} />
            </div>
            <div className="field">
              <label>Mức lương mong muốn</label>
              <input value={form.mucLuongMongMuon} onChange={set("mucLuongMongMuon")} />
            </div>
            <div className="field">
              <label>Trình độ học vấn</label>
              <select value={form.trinhDoHocVan} onChange={set("trinhDoHocVan")}>
                {TRINH_DO_OPTIONS.map((t) => <option key={t} value={t}>{TRINH_DO_LABEL[t]}</option>)}
              </select>
            </div>

            <div className="field">
              <label>Danh sách kỹ năng</label>
              <div className="card" style={{ maxHeight: 180, overflowY: "auto" }}>
                {skills.map((s) => (
                  <label key={s.maKyNang} style={{ display: "flex", alignItems: "center", gap: 8, marginBottom: 6, fontWeight: 400 }}>
                    <input type="checkbox" style={{ height: "auto", width: "auto" }} checked={!!selectedSkills[s.maKyNang]} onChange={() => toggleSkill(s.maKyNang)} />
                    {s.tenKyNang}
                  </label>
                ))}
              </div>
            </div>

            <div className="field">
              <label>Kinh nghiệm làm việc</label>
              {kinhNghiem.map((k, i) => (
                <div key={i} className="card" style={{ marginBottom: 8 }}>
                  <input placeholder="Công ty" value={k.congTy} onChange={(e) => {
                    const next = [...kinhNghiem]; next[i] = { ...k, congTy: e.target.value }; setKinhNghiem(next);
                  }} style={{ marginBottom: 6 }} />
                  <input placeholder="Vị trí" value={k.viTri} onChange={(e) => {
                    const next = [...kinhNghiem]; next[i] = { ...k, viTri: e.target.value }; setKinhNghiem(next);
                  }} style={{ marginBottom: 6 }} />
                  <input type="date" value={k.tuNgay} onChange={(e) => {
                    const next = [...kinhNghiem]; next[i] = { ...k, tuNgay: e.target.value }; setKinhNghiem(next);
                  }} />
                </div>
              ))}
              <button type="button" className="btn btn-secondary" style={{ height: 32 }} onClick={() => setKinhNghiem([...kinhNghiem, emptyKinhNghiem()])}>
                + Thêm kinh nghiệm
              </button>
            </div>

            <div className="field">
              <label>Học vấn</label>
              {hocVan.map((h, i) => (
                <div key={i} className="card" style={{ marginBottom: 8 }}>
                  <input placeholder="Trường" value={h.truong} onChange={(e) => {
                    const next = [...hocVan]; next[i] = { ...h, truong: e.target.value }; setHocVan(next);
                  }} style={{ marginBottom: 6 }} />
                  <input placeholder="Chuyên ngành" value={h.chuyenNganh} onChange={(e) => {
                    const next = [...hocVan]; next[i] = { ...h, chuyenNganh: e.target.value }; setHocVan(next);
                  }} />
                </div>
              ))}
              <button type="button" className="btn btn-secondary" style={{ height: 32 }} onClick={() => setHocVan([...hocVan, emptyHocVan()])}>
                + Thêm học vấn
              </button>
            </div>

            {error && <p className="error-text">{error}</p>}
            {success && <p className="success-text">{success}</p>}
            <div style={{ display: "flex", gap: 8 }}>
              {editingCvId && (
                <button type="button" className="btn btn-secondary" style={{ flex: 1 }} onClick={resetForm}>
                  Hủy sửa
                </button>
              )}
              <button className="btn btn-primary" style={{ flex: 1 }} disabled={loading} type="submit">
                {loading ? "Đang lưu..." : editingCvId ? "Cập nhật" : "Lưu"}
              </button>
            </div>
          </form>
        </div>

        <div className="card" style={{ flex: 1, alignSelf: "flex-start", border: "1px solid var(--border)" }}>
          <h3 style={{ color: "var(--text-muted)", fontSize: 13, textTransform: "uppercase", letterSpacing: 0.5 }}>Xem trước</h3>
          <h2 style={{ marginBottom: 0, fontSize: 24 }}>{(form.tenCv || "(Tên CV)").toUpperCase()}</h2>
          <p style={{ color: "var(--text-muted)" }}>{form.viTriMongMuon || "Vị trí mong muốn"}</p>
          <p>{form.mucLuongMongMuon}</p>
          <p>Trình độ: {TRINH_DO_LABEL[form.trinhDoHocVan]}</p>
          <h3>Kỹ năng</h3>
          <p>{Object.keys(selectedSkills).map((id) => skillName(Number(id))).join(", ") || "(chưa chọn)"}</p>
          <h3>Kinh nghiệm</h3>
          {kinhNghiem.filter((k) => k.congTy).map((k, i) => <p key={i}>{k.viTri} tại {k.congTy}</p>)}
          <h3>Học vấn</h3>
          {hocVan.filter((h) => h.truong).map((h, i) => <p key={i}>{h.chuyenNganh} — {h.truong}</p>)}
        </div>
      </div>

      <div className="card" style={{ marginTop: 24 }}>
        <h3>Tải lên CV mới</h3>
        <form onSubmit={handleUpload}>
          <div className="field">
            <label>Tên CV</label>
            <input value={uploadTenCv} onChange={(e) => setUploadTenCv(e.target.value)} required />
          </div>
          <FileUpload
            label="File CV (.pdf/.doc/.docx, tối đa 10MB)"
            accept=".pdf,.doc,.docx"
            variant="document"
            value={uploadFile}
            onChange={setUploadFile}
          />
          {uploadError && <p className="error-text">{uploadError}</p>}
          {uploadSuccess && <p className="success-text">{uploadSuccess}</p>}
          <button className="btn btn-primary" type="submit">Tải lên CV mới</button>
        </form>
      </div>
    </div>
  );
}
