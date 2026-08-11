import houston from "../assets/company-skyline.jpg";
import nyc from "../assets/banners/nyc.jpg";
import vancouver from "../assets/banners/vancouver.jpg";
import seattle from "../assets/banners/seattle.jpg";
import london from "../assets/banners/london.jpg";

// NHA_TUYEN_DUNG (schema da chot trong bao cao, Lab 3) khong co cot anh
// bia/banner rieng cho tung cong ty - chi co Logo. De moi cong ty van co
// banner nhin rieng biet ma khong doi DB, gan CO DINH 1 trong 5 anh nay
// theo MaTK (xoay vong neu nhieu hon 5 cong ty). Thuan tuy trang tri o
// frontend, khong luu vao CSDL.
const BANNERS = [
  { src: houston, credit: "Jason Villanueva / Wikimedia Commons (CC BY-SA 4.0)" },
  { src: nyc, credit: "William Warby / Wikimedia Commons (CC BY 2.0)" },
  { src: vancouver, credit: "Kyle Pearce / Wikimedia Commons (CC BY-SA 2.0)" },
  { src: seattle, credit: "Doug Brown / Wikimedia Commons (CC BY-SA 2.0)" },
  { src: london, credit: "Marcus news, IgnisFatuus / Wikimedia Commons (CC BY-SA 3.0)" },
];

export function getCompanyBanner(maTk) {
  const n = Math.abs(Number(maTk));
  const idx = Number.isFinite(n) ? n % BANNERS.length : 0;
  return BANNERS[idx];
}
