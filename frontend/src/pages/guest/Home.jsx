import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import {
  Search, MapPin, ShoppingBag, Cpu, GraduationCap, Megaphone,
  Factory, Landmark, ShoppingCart, HeartPulse, Briefcase, X,
} from "lucide-react";
import { api } from "../../api/client";
import { useAuth } from "../../context/AuthContext";
import JobCard from "../../components/JobCard";

// Icon trang tri theo ten nganh nghe (DANH_MUC_NGANH_NGHE khong co cot icon
// trong schema) - chi de card nhin sinh dong hon, khong anh huong nghiep vu.
// Luoi tin tuyen dung 3 cot tren desktop (tu dong xep lai it hon o man
// hinh hep), thay vi 1 tin/1 hang ngang toan chieu rong nhu truoc.
const JOB_GRID_STYLE = { display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(300px, 1fr))", gap: 16, marginBottom: 8 };

const NGANH_NGHE_ICON = {
  "Bán lẻ": ShoppingBag,
  "Công nghệ thông tin": Cpu,
  "Giáo dục": GraduationCap,
  "Marketing": Megaphone,
  "Sản xuất": Factory,
  "Tài chính - Ngân hàng": Landmark,
  "Thương mại điện tử": ShoppingCart,
  "Y tế": HeartPulse,
};

// Gop y bao cao (LAB4 - "Tim kiem va loc viec lam"): loai hinh cong viec
// (checkbox), muc luong (dropdown khoang), sap xep, phan trang theo so
// trang. "Cap bac" bi bo qua co y - khong co trong schema/Lab 3, da chot
// voi nguoi dung 12/08 la khong lam.
const HINH_THUC_OPTIONS = [
  { value: "FullTime", label: "Full-time" },
  { value: "PartTime", label: "Part-time" },
  { value: "Remote", label: "Remote" },
];
const LUONG_OPTIONS = [
  { value: "", label: "Tất cả mức lương" },
  { value: "duoi10", label: "Dưới 10 triệu", max: 10 },
  { value: "10-20", label: "10 - 20 triệu", min: 10, max: 20 },
  { value: "20-30", label: "20 - 30 triệu", min: 20, max: 30 },
  { value: "tren30", label: "Trên 30 triệu", min: 30 },
];
const SORT_OPTIONS = [
  { value: "moi_nhat", label: "Mới nhất" },
  { value: "luong_giam", label: "Lương cao - thấp" },
  { value: "luong_tang", label: "Lương thấp - cao" },
];
const PAGE_SIZE = 9;

export default function Home() {
  const { auth } = useAuth();
  const [keyword, setKeyword] = useState("");
  const [diaDiem, setDiaDiem] = useState("");
  const [maNganhNghe, setMaNganhNghe] = useState(null);
  const [hinhThucSelected, setHinhThucSelected] = useState([]);
  const [luongRange, setLuongRange] = useState("");
  const [sortBy, setSortBy] = useState("moi_nhat");
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [featured, setFeatured] = useState([]);
  const [industries, setIndustries] = useState([]);
  const [results, setResults] = useState([]);
  const [searched, setSearched] = useState(false);
  const [favoriteIds, setFavoriteIds] = useState(new Set());
  const [recommended, setRecommended] = useState([]);
  const [coCv, setCoCv] = useState(true);
  const [selectedIndustry, setSelectedIndustry] = useState(null);

  useEffect(() => {
    api.get("/jobs/featured?top=6").then(setFeatured).catch(() => {});
    api.get("/industries").then(setIndustries).catch(() => {});
  }, []);

  useEffect(() => {
    if (auth?.vaiTro === "UngVien") {
      api.get("/favorites/mine", auth.token)
        .then((list) => setFavoriteIds(new Set(list.map((j) => j.maTin))))
        .catch(() => {});
      api.get("/jobs/recommended", auth.token)
        .then((data) => {
          setCoCv(data.coCv !== false);
          setRecommended(data.goiY ?? []);
        })
        .catch(() => {});
    } else {
      setFavoriteIds(new Set());
      setRecommended([]);
      setCoCv(true);
    }
  }, [auth]);

  const toggleFavorite = async (maTin) => {
    const daLuu = favoriteIds.has(maTin);
    try {
      if (daLuu) {
        await api.del(`/favorites/${maTin}`, auth.token);
        setFavoriteIds((prev) => {
          const next = new Set(prev);
          next.delete(maTin);
          return next;
        });
      } else {
        await api.post(`/favorites/${maTin}`, undefined, auth.token);
        setFavoriteIds((prev) => new Set(prev).add(maTin));
      }
    } catch {
      // bo qua loi luu tin, khong chan luong duyet tin chinh
    }
  };

  // Tham so nhan overrides de doc gia tri MOI ngay lap tuc (setState la bat
  // dong bo - vd bam checkbox loai hinh phai loc bang gia tri VUA doi, khong
  // phai state cu con luu trong closure).
  const runSearch = async (overrides = {}) => {
    const kw = overrides.keyword ?? keyword;
    const dd = overrides.diaDiem ?? diaDiem;
    const nn = "maNganhNghe" in overrides ? overrides.maNganhNghe : maNganhNghe;
    const hinhThuc = overrides.hinhThucSelected ?? hinhThucSelected;
    const luong = overrides.luongRange ?? luongRange;
    const sort = overrides.sortBy ?? sortBy;
    const targetPage = overrides.page ?? 1;

    const params = new URLSearchParams();
    if (kw) params.set("keyword", kw);
    if (dd) params.set("diaDiem", dd);
    if (nn) params.set("maNganhNghe", nn);
    hinhThuc.forEach((h) => params.append("hinhThucLamViec", h));
    const luongOpt = LUONG_OPTIONS.find((o) => o.value === luong);
    if (luongOpt?.min !== undefined) params.set("luongMin", luongOpt.min);
    if (luongOpt?.max !== undefined) params.set("luongMax", luongOpt.max);
    params.set("sortBy", sort);
    params.set("page", targetPage);
    params.set("pageSize", PAGE_SIZE);

    const data = await api.get(`/jobs/search?${params.toString()}`);
    setResults(data.items);
    setTotalCount(data.totalCount);
    setPage(data.page);
    setSearched(true);
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    setSelectedIndustry(null);
    setMaNganhNghe(null);
    runSearch({ maNganhNghe: null });
  };

  // Loc theo nganh nghe THAT (qua MaNganhNghe cua cong ty dang tin), khong
  // phai nhet ten nganh vao o tu khoa - truoc do o keyword.Contains(TieuDe)
  // gan nhu luon ra 0 ket qua vi tieu de tin khong bao gio chua ten nganh.
  const handleIndustryClick = (nn) => {
    setKeyword("");
    setSelectedIndustry(nn.tenNganhNghe);
    setMaNganhNghe(nn.maNganhNghe);
    runSearch({ keyword: "", maNganhNghe: nn.maNganhNghe });
  };

  const toggleHinhThuc = (value) => {
    const next = hinhThucSelected.includes(value)
      ? hinhThucSelected.filter((v) => v !== value)
      : [...hinhThucSelected, value];
    setHinhThucSelected(next);
    runSearch({ hinhThucSelected: next });
  };

  const changeLuongRange = (value) => {
    setLuongRange(value);
    runSearch({ luongRange: value });
  };

  const changeSortBy = (value) => {
    setSortBy(value);
    runSearch({ sortBy: value });
  };

  const clearAllFilters = () => {
    setKeyword("");
    setDiaDiem("");
    setSelectedIndustry(null);
    setMaNganhNghe(null);
    setHinhThucSelected([]);
    setLuongRange("");
    setSortBy("moi_nhat");
    setSearched(false);
    setResults([]);
    setTotalCount(0);
    setPage(1);
  };

  const canFavorite = auth?.vaiTro === "UngVien";
  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE));

  // Tranh hien trung tin: neu da co muc "Goi y cho ban", loai nhung tin do
  // ra khoi "Viec Lam Noi Bat" thay vi lap lai y het danh sach ben tren.
  const recommendedShown = !searched && canFavorite && coCv ? recommended.slice(0, 6) : [];
  const recommendedIds = new Set(recommendedShown.map((j) => j.maTin));
  const featuredFiltered = recommendedIds.size > 0 ? featured.filter((j) => !recommendedIds.has(j.maTin)) : featured;

  return (
    <div className="page-container">
      <h1 style={{ textAlign: "center" }}>Khám phá Sự nghiệp tương lai của bạn</h1>
      <p style={{ textAlign: "center", color: "var(--text-muted)", maxWidth: 560, margin: "0 auto 24px" }}>
        Hàng ngàn cơ hội việc làm từ các công ty hàng đầu đang chờ đón bạn. Bắt đầu hành trình ngay hôm nay.
      </p>

      <form onSubmit={handleSubmit} className="card" style={{ display: "flex", gap: 8, marginBottom: 12 }}>
        <div className="input-icon-wrap" style={{ flex: 1 }}>
          <Search size={18} />
          <input placeholder="Từ khoá (vị trí, kỹ năng...)" value={keyword} onChange={(e) => setKeyword(e.target.value)} />
        </div>
        <div className="input-icon-wrap" style={{ flex: 1 }}>
          <MapPin size={18} />
          <input placeholder="Địa điểm" value={diaDiem} onChange={(e) => setDiaDiem(e.target.value)} />
        </div>
        <button className="btn btn-primary" type="submit">Tìm Việc Ngay</button>
      </form>

      <div className="card" style={{ display: "flex", flexWrap: "wrap", gap: 20, alignItems: "center", marginBottom: 32 }}>
        <div>
          <p style={{ margin: "0 0 6px", fontSize: 13, color: "var(--text-muted)", fontWeight: 600 }}>Loại hình công việc</p>
          <div style={{ display: "flex", gap: 14, flexWrap: "wrap" }}>
            {HINH_THUC_OPTIONS.map((o) => (
              <label key={o.value} style={{ display: "flex", alignItems: "center", gap: 6, fontWeight: 400, fontSize: 14 }}>
                <input
                  type="checkbox"
                  style={{ height: "auto", width: "auto" }}
                  checked={hinhThucSelected.includes(o.value)}
                  onChange={() => toggleHinhThuc(o.value)}
                />
                {o.label}
              </label>
            ))}
          </div>
        </div>
        <div className="field" style={{ margin: 0 }}>
          <label>Mức lương</label>
          <select value={luongRange} onChange={(e) => changeLuongRange(e.target.value)} style={{ width: 180 }}>
            {LUONG_OPTIONS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
          </select>
        </div>
        <div className="field" style={{ margin: 0 }}>
          <label>Sắp xếp</label>
          <select value={sortBy} onChange={(e) => changeSortBy(e.target.value)} style={{ width: 180 }}>
            {SORT_OPTIONS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
          </select>
        </div>
        {searched && (
          <button
            type="button"
            className="btn btn-secondary"
            style={{ height: 36, padding: "0 14px", display: "flex", alignItems: "center", gap: 6, marginLeft: "auto" }}
            onClick={clearAllFilters}
          >
            <X size={16} /> Xóa tất cả
          </button>
        )}
      </div>

      {industries.length > 0 && (
        <div style={{ marginBottom: 32 }}>
          <p style={{ color: "var(--text-muted)", fontSize: 14, marginBottom: 10 }}>Khám phá theo ngành nghề</p>
          <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(220px, 1fr))", gap: 12 }}>
            {industries.slice(0, 8).map((nn) => {
              const Icon = NGANH_NGHE_ICON[nn.tenNganhNghe] || Briefcase;
              return (
                <button
                  key={nn.maNganhNghe}
                  type="button"
                  className="card"
                  style={{
                    display: "flex", alignItems: "center", gap: 10, padding: "14px 16px",
                    textAlign: "left", cursor: "pointer", border: "1px solid var(--border)",
                  }}
                  onClick={() => handleIndustryClick(nn)}
                >
                  <span style={{
                    flexShrink: 0, width: 36, height: 36, borderRadius: "var(--radius)", background: "var(--info-bg)",
                    color: "var(--indigo-dark)", display: "flex", alignItems: "center", justifyContent: "center",
                  }}>
                    <Icon size={18} />
                  </span>
                  <span style={{ fontSize: 14, fontWeight: 600 }}>{nn.tenNganhNghe}</span>
                </button>
              );
            })}
          </div>
        </div>
      )}

      {!searched && canFavorite && !coCv && (
        <div className="card" style={{ marginBottom: 32 }}>
          <p style={{ margin: 0 }}>
            Bạn cần tạo CV trước để nhận gợi ý việc làm phù hợp.{" "}
            <Link to="/candidate/cvs">Tạo CV ngay</Link>
          </p>
        </div>
      )}

      {recommendedShown.length > 0 && (
        <>
          <h2>Gợi ý cho bạn</h2>
          <div style={JOB_GRID_STYLE}>
            {recommendedShown.map((job) => (
              <JobCard
                key={job.maTin}
                job={job}
                isFavorited={favoriteIds.has(job.maTin)}
                onToggleFavorite={toggleFavorite}
                matchPercent={job.phanTramPhuHop}
              />
            ))}
          </div>
        </>
      )}

      {searched ? (
        <>
          <h2>{selectedIndustry ? `Ngành ${selectedIndustry}` : "Kết quả tìm kiếm"} ({totalCount})</h2>
          {results.length === 0 && <p>Không tìm thấy việc làm phù hợp với điều kiện tìm kiếm.</p>}
          <div style={JOB_GRID_STYLE}>
            {results.map((job) => (
              <JobCard
                key={job.maTin}
                job={job}
                isFavorited={favoriteIds.has(job.maTin)}
                onToggleFavorite={canFavorite ? toggleFavorite : undefined}
              />
            ))}
          </div>
          {totalPages > 1 && (
            <div style={{ display: "flex", justifyContent: "center", gap: 6, marginTop: 20 }}>
              {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
                <button
                  key={p}
                  type="button"
                  className={p === page ? "btn btn-primary" : "btn btn-secondary"}
                  style={{ height: 36, width: 36, padding: 0 }}
                  onClick={() => runSearch({ page: p })}
                >
                  {p}
                </button>
              ))}
            </div>
          )}
        </>
      ) : (
        featuredFiltered.length > 0 && (
          <>
            <h2>Việc Làm Nổi Bật</h2>
            <div style={JOB_GRID_STYLE}>
              {featuredFiltered.map((job) => (
                <JobCard
                  key={job.maTin}
                  job={job}
                  isFavorited={favoriteIds.has(job.maTin)}
                  onToggleFavorite={canFavorite ? toggleFavorite : undefined}
                />
              ))}
            </div>
          </>
        )
      )}
    </div>
  );
}
