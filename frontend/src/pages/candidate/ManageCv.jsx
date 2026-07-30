import { useEffect, useState } from "react";
import { api, ApiError } from "../../api/client";
import { useAuth } from "../../context/AuthContext";

const TRINH_DO_OPTIONS = ["TrungCap", "CaoDang", "DaiHoc", "SauDaiHoc"];
const TRINH_DO_LABEL = { TrungCap: "Trung cấp", CaoDang: "Cao đẳng", DaiHoc: "Đại học", SauDaiHoc: "Sau đại học" };

function emptyKinhNghiem() {
  return { congTy: "", viTri: "", tuNgay: "", denNgay: "", moTaCongViec: "" };
}
function emptyHocVan() {
  return { truong: "", chuyenNganh: "", tuNam: "", denNam: "" };
}

export default function ManageCv() {
  const { auth } = useAuth();
  const [skills, setSkills] = useState([]);
  const [myCvs, setMyCvs] = useState([]);
  const [selectedSkills, setSelectedSkills] = useState({}); // { maKyNang: mucDoThanhThao }
  const [kinhNghiem, setKinhNghiem] = useState([emptyKinhNghiem()]);
  const [hocVan, setHocVan] = useState([emptyHocVan()]);
  const [form, setForm] = useState({ tenCv: "", viTriMongMuon: "", mucLuongMongMuon: "", trinhDoHocVan: "DaiHoc" });
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);

  const [uploadTenCv, setUploadTenCv] = useState("");
  const [uploadFile, setUploadFile] = useState(null);
  const [uploadError, setUploadError] = useState("");
  const [uploadSuccess, setUploadSuccess] = useState("");

  const loadCvs = () => api.get("/cvs/mine", auth.token).then(setMyCvs);

  useEffect(() => {
    api.get("/skills").then(setSkills);
    loadCvs();
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
      await api.post("/cvs/online", body, auth.token);
      setSuccess("Tạo CV thành công."); // MS03
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
    if (!uploadFile) return;
    try {
      const fd = new FormData();
      fd.append("tenCv", uploadTenCv);
      fd.append("file", uploadFile);
      const res = await fetch("http://localhost:5147/api/cvs/upload", {
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
      <h2>Quản lý CV</h2>

      {myCvs.length > 0 && (
        <div className="card" style={{ marginBottom: 24 }}>
          <h3>CV của tôi</h3>
          {myCvs.map((cv) => (
            <div key={cv.maCV} style={{ padding: "8px 0", borderBottom: "1px solid var(--border)" }}>
              <strong>{cv.tenCV}</strong> — {cv.loaiCV === "TrucTuyen" ? "Trực tuyến" : "Upload"}
              {cv.viTriMongMuon && <span> · {cv.viTriMongMuon}</span>}
            </div>
          ))}
        </div>
      )}

      <div style={{ display: "flex", gap: 24 }}>
        <div className="card" style={{ flex: 1 }}>
          <h3>Tạo CV trực tuyến</h3>
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
            <button className="btn btn-primary" style={{ width: "100%" }} disabled={loading} type="submit">
              {loading ? "Đang lưu..." : "Lưu"}
            </button>
          </form>
        </div>

        <div className="card" style={{ flex: 1, alignSelf: "flex-start" }}>
          <h3>Xem trước</h3>
          <h2 style={{ marginBottom: 0 }}>{form.tenCv || "(Tên CV)"}</h2>
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
          <div className="field">
            <label>File CV (.pdf/.doc/.docx, tối đa 10MB)</label>
            <input type="file" accept=".pdf,.doc,.docx" onChange={(e) => setUploadFile(e.target.files[0])} required />
          </div>
          {uploadError && <p className="error-text">{uploadError}</p>}
          {uploadSuccess && <p className="success-text">{uploadSuccess}</p>}
          <button className="btn btn-primary" type="submit">Tải lên CV mới</button>
        </form>
      </div>
    </div>
  );
}
