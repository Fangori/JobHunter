import houston from "../assets/company-skyline.jpg";
import nyc from "../assets/banners/nyc.jpg";
import vancouver from "../assets/banners/vancouver.jpg";
import seattle from "../assets/banners/seattle.jpg";
import london from "../assets/banners/london.jpg";

// NHA_TUYEN_DUNG (schema da chot trong bao cao, Lab 3) khong co cot anh
// bia rieng luc dau - da them cot AnhBia (URL Cloudinary that, giong co che
// cua Logo) qua migrate-anh-bia-cong-ty.sql de NTD tu UPLOAD anh bia rieng
// (UC08). Neu NTD chua upload (AnhBia null), fallback ve 1 trong 5 anh
// curated ben duoi, xoay vong co dinh theo MaTK - tuong thich nguoc, khong
// can migrate du lieu cu.
const BANNERS = [
  { src: houston, credit: "Jason Villanueva / Wikimedia Commons (CC BY-SA 4.0)" },
  { src: nyc, credit: "William Warby / Wikimedia Commons (CC BY 2.0)" },
  { src: vancouver, credit: "Kyle Pearce / Wikimedia Commons (CC BY-SA 2.0)" },
  { src: seattle, credit: "Doug Brown / Wikimedia Commons (CC BY-SA 2.0)" },
  { src: london, credit: "Marcus news, IgnisFatuus / Wikimedia Commons (CC BY-SA 3.0)" },
];

// anhBiaUrl (tuy chon): URL Cloudinary that NTD da upload va luu trong DB -
// uu tien dung neu co (credit null - khong can ghi nguon vi la anh cua
// chinh cong ty). maTk: dung lam fallback xoay vong khi chua upload.
export function getCompanyBanner(maTk, anhBiaUrl) {
  if (anhBiaUrl) return { src: anhBiaUrl, credit: null };
  const n = Math.abs(Number(maTk));
  const idx = Number.isFinite(n) ? n % BANNERS.length : 0;
  return BANNERS[idx];
}
