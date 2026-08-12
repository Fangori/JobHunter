import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { api } from "../../api/client";
import { useAuth } from "../../context/AuthContext";
import ApplicantDetail from "./ApplicantDetail";

const TRINH_DO_OPTIONS = ["TrungCap", "CaoDang", "DaiHoc", "SauDaiHoc"];
const TRINH_DO_LABEL = { TrungCap: "Trung cấp", CaoDang: "Cao đẳng", DaiHoc: "Đại học", SauDaiHoc: "Sau đại học" };
const TRANG_THAI_LABEL = { DaNop: "Đã nộp", DangXemXet: "Đang xem xét", PhongVan: "Phỏng vấn", TuChoi: "Từ chối", Nhan: "Nhận", DaHuy: "Đã hủy" };
const TRANG_THAI_BADGE = { DaNop: "badge-info", DangXemXet: "badge-warning", PhongVan: "badge-warning", TuChoi: "badge-danger", Nhan: "badge-success", DaHuy: "badge-neutral" };

export default function Applicants() {
  const { id } = useParams();
  const { auth } = useAuth();
  const [skills, setSkills] = useState([]);
  const [list, setList] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedMaDon, setSelectedMaDon] = useState(null);
  const [sortKey, setSortKey] = useState(null);
  const [sortDir, setSortDir] = useState("desc");

  // 3 tieu chi loc dung BM14
  const [selectedSkills, setSelectedSkills] = useState([]);
  const [minNamKinhNghiem, setMinNamKinhNghiem] = useState("");
  const [selectedTrinhDo, setSelectedTrinhDo] = useState([]);
  const [filtering, setFiltering] = useState(false);

  useEffect(() => {
    api.get("/skills").then(setSkills);
    loadDefault();
  }, [id]);

  const loadDefault = async () => {
    setLoading(true);
    const data = await api.get(`/jobs/${id}/applicants`, auth.token);
    setList(data);
    setFiltering(false);
    setLoading(false);
  };

  const toggleSkill = (maKyNang) => {
    setSelectedSkills((prev) => prev.includes(maKyNang) ? prev.filter((x) => x !== maKyNang) : [...prev, maKyNang]);
  };
  const toggleTrinhDo = (t) => {
    setSelectedTrinhDo((prev) => prev.includes(t) ? prev.filter((x) => x !== t) : [...prev, t]);
  };

  const applyFilter = async () => {
    setLoading(true);
    const params = new URLSearchParams();
    selectedSkills.forEach((id) => params.append("maKyNang", id));
    selectedTrinhDo.forEach((t) => params.append("trinhDoHocVan", t));
    if (minNamKinhNghiem) params.set("minNamKinhNghiem", minNamKinhNghiem);
    const data = await api.get(`/jobs/${id}/applicants/filter?${params.toString()}`, auth.token);
    setList(data);
    setFiltering(true);
    setLoading(false);
  };

  const clearFilter = () => {
    setSelectedSkills([]);
    setSelectedTrinhDo([]);
    setMinNamKinhNghiem("");
    loadDefault();
  };

  const toggleSort = (key) => {
    if (sortKey === key) {
      setSortDir((d) => (d === "desc" ? "asc" : "desc"));
    } else {
      setSortKey(key);
      setSortDir("desc");
    }
  };

  const skillNames = Object.fromEntries(skills.map((s) => [s.maKyNang, s.tenKyNang]));

  const sortedList = sortKey
    ? [...list].sort((a, b) => (sortDir === "desc" ? b[sortKey] - a[sortKey] : a[sortKey] - b[sortKey]))
    : list;

  return (
    <div className="page-container">
      <div className="dashboard-header-band">
        <h2>Danh sách ứng viên</h2>
        {!loading && <p>{list.length} ứng viên phù hợp</p>}
      </div>
      <div style={{ display: "flex", gap: 24 }}>
        <aside className="card" style={{ width: 260, flexShrink: 0, alignSelf: "flex-start" }}>
          <h3>Bộ lọc</h3>

          <div className="field">
            <label>Kỹ năng</label>
            <div style={{ maxHeight: 160, overflowY: "auto" }}>
              {skills.map((s) => (
                <label key={s.maKyNang} style={{ display: "flex", alignItems: "center", gap: 8, marginBottom: 6, fontWeight: 400 }}>
                  <input type="checkbox"
                    checked={selectedSkills.includes(s.maKyNang)} onChange={() => toggleSkill(s.maKyNang)} />
                  {s.tenKyNang}
                </label>
              ))}
            </div>
          </div>

          <div className="field">
            <label>Số năm kinh nghiệm tối thiểu</label>
            <input type="number" min="0" value={minNamKinhNghiem} onChange={(e) => setMinNamKinhNghiem(e.target.value)} />
          </div>

          <div className="field">
            <label>Học vấn</label>
            {TRINH_DO_OPTIONS.map((t) => (
              <label key={t} style={{ display: "flex", alignItems: "center", gap: 8, marginBottom: 6, fontWeight: 400 }}>
                <input type="checkbox"
                  checked={selectedTrinhDo.includes(t)} onChange={() => toggleTrinhDo(t)} />
                {TRINH_DO_LABEL[t]}
              </label>
            ))}
          </div>

          <button className="btn btn-primary" style={{ width: "100%", marginBottom: 8 }} onClick={applyFilter}>Lọc</button>
          {filtering && <button className="btn btn-secondary" style={{ width: "100%" }} onClick={clearFilter}>Bỏ lọc</button>}
        </aside>

        <div style={{ flex: 1 }}>
          {loading && <p>Đang tải...</p>}
          {!loading && list.length === 0 && (
            <p className="error-text">Không tìm thấy ứng viên phù hợp với tiêu chí đã chọn.</p>
          )}
          {!loading && list.length > 0 && (
            <table style={{ width: "100%", borderCollapse: "collapse" }}>
              <thead>
                <tr style={{ textAlign: "left", borderBottom: "2px solid var(--border)" }}>
                  <th style={{ padding: 8 }}>Ứng viên</th>
                  <th style={{ padding: 8 }}>Kỹ năng khớp</th>
                  <th style={{ padding: 8, cursor: "pointer" }} onClick={() => toggleSort("phanTramPhuHop")}>
                    Tỉ lệ phù hợp {sortKey === "phanTramPhuHop" ? (sortDir === "desc" ? "▼" : "▲") : ""}
                  </th>
                  <th style={{ padding: 8, cursor: "pointer" }} onClick={() => toggleSort("soNamKinhNghiem")}>
                    Kinh nghiệm {sortKey === "soNamKinhNghiem" ? (sortDir === "desc" ? "▼" : "▲") : ""}
                  </th>
                  <th style={{ padding: 8 }}>Trạng thái</th>
                </tr>
              </thead>
              <tbody>
                {sortedList.map((a) => (
                  <tr
                    key={a.maDon}
                    className="hover-row"
                    style={{ borderBottom: "1px solid var(--border)", cursor: "pointer" }}
                    onClick={() => setSelectedMaDon(a.maDon)}
                  >
                    <td style={{ padding: 8 }}>{a.hoTen}<br /><span style={{ color: "var(--text-muted)", fontSize: 13 }}>{a.tenCV}</span></td>
                    <td style={{ padding: 8 }}>{a.kyNangKhop.join(", ") || "—"}</td>
                    <td style={{ padding: 8, fontWeight: 700, color: "var(--indigo)" }}>{a.phanTramPhuHop}%</td>
                    <td style={{ padding: 8 }}>{a.soNamKinhNghiem} năm</td>
                    <td style={{ padding: 8 }}>
                      <span className={`badge ${TRANG_THAI_BADGE[a.trangThai] || "badge-neutral"}`}>
                        {TRANG_THAI_LABEL[a.trangThai] || a.trangThai}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>

      {selectedMaDon && (
        <ApplicantDetail
          maDon={selectedMaDon}
          skillNames={skillNames}
          onClose={() => setSelectedMaDon(null)}
          onUpdated={filtering ? applyFilter : loadDefault}
        />
      )}
    </div>
  );
}
