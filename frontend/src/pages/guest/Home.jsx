import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import {
  Search, MapPin, ShoppingBag, Cpu, GraduationCap, Megaphone,
  Factory, Landmark, ShoppingCart, HeartPulse, Briefcase,
} from "lucide-react";
import { api } from "../../api/client";
import { useAuth } from "../../context/AuthContext";
import JobCard from "../../components/JobCard";

// Icon trang tri theo ten nganh nghe (DANH_MUC_NGANH_NGHE khong co cot icon
// trong schema) - chi de card nhin sinh dong hon, khong anh huong nghiep vu.
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

export default function Home() {
  const { auth } = useAuth();
  const [keyword, setKeyword] = useState("");
  const [diaDiem, setDiaDiem] = useState("");
  const [featured, setFeatured] = useState([]);
  const [industries, setIndustries] = useState([]);
  const [results, setResults] = useState([]);
  const [searched, setSearched] = useState(false);
  const [favoriteIds, setFavoriteIds] = useState(new Set());
  const [recommended, setRecommended] = useState([]);
  const [coCv, setCoCv] = useState(true);

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

  const search = async (kw = keyword, dd = diaDiem) => {
    const params = new URLSearchParams();
    if (kw) params.set("keyword", kw);
    if (dd) params.set("diaDiem", dd);
    const data = await api.get(`/jobs?${params.toString()}`);
    setResults(data);
    setSearched(true);
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    search();
  };

  const handleIndustryClick = (tenNganhNghe) => {
    setKeyword(tenNganhNghe);
    search(tenNganhNghe, diaDiem);
  };

  const canFavorite = auth?.vaiTro === "UngVien";

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
                  onClick={() => handleIndustryClick(nn.tenNganhNghe)}
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
          {recommendedShown.map((job) => (
            <JobCard
              key={job.maTin}
              job={job}
              isFavorited={favoriteIds.has(job.maTin)}
              onToggleFavorite={toggleFavorite}
              matchPercent={job.phanTramPhuHop}
            />
          ))}
        </>
      )}

      {searched ? (
        <>
          <h2>Kết quả tìm kiếm ({results.length})</h2>
          {results.length === 0 && <p>Không tìm thấy việc làm phù hợp với điều kiện tìm kiếm.</p>}
          {results.map((job) => (
            <JobCard
              key={job.maTin}
              job={job}
              isFavorited={favoriteIds.has(job.maTin)}
              onToggleFavorite={canFavorite ? toggleFavorite : undefined}
            />
          ))}
        </>
      ) : (
        featuredFiltered.length > 0 && (
          <>
            <h2>Việc Làm Nổi Bật</h2>
            {featuredFiltered.map((job) => (
              <JobCard
                key={job.maTin}
                job={job}
                isFavorited={favoriteIds.has(job.maTin)}
                onToggleFavorite={canFavorite ? toggleFavorite : undefined}
              />
            ))}
          </>
        )
      )}
    </div>
  );
}
