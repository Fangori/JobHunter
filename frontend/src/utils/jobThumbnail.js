import cntt from "../assets/job-thumbs/cntt.jpg";
import taichinh from "../assets/job-thumbs/taichinh.jpg";
import tmdt from "../assets/job-thumbs/tmdt.jpg";
import giaoduc from "../assets/job-thumbs/giaoduc.jpg";
import yte from "../assets/job-thumbs/yte.jpg";
import sanxuat from "../assets/job-thumbs/sanxuat.jpg";
import banle from "../assets/job-thumbs/banle.jpg";
import marketing from "../assets/job-thumbs/marketing.jpg";

// TIN_TUYEN_DUNG khong co cot anh minh hoa trong schema da chot - chon anh
// theo MaNganhNghe CUA CONG TY dang tin (cot da co san tren NHA_TUYEN_DUNG,
// API da tra ve o TinTuyenDungSummaryDto.maNganhNghe). Khoa co dinh theo id
// that trong DANH_MUC_NGANH_NGHE (xac nhan qua truy van DB, khong doi):
// 1 CNTT, 2 Tai chinh-Ngan hang, 3 TMDT, 4 Giao duc, 5 Y te, 6 San xuat,
// 7 Ban le, 8 Marketing.
const THUMBNAIL_BY_NGANH = {
  1: { src: cntt, credit: "Joonspoon / Wikimedia Commons (CC BY-SA 4.0)" },
  2: { src: taichinh, credit: "Diliff / Wikimedia Commons (CC BY-SA 3.0)" },
  3: { src: tmdt, credit: "Kabugenyo / Wikimedia Commons (CC BY-SA 3.0)" },
  4: { src: giaoduc, credit: "Jeff chenqinyi / Wikimedia Commons (CC BY-SA 3.0)" },
  5: { src: yte, credit: "Bruno ashimwe winks / Wikimedia Commons (CC BY-SA 4.0)" },
  6: { src: sanxuat, credit: "Steve Jurvetson, Mike Bird / Wikimedia Commons (CC BY-SA 4.0)" },
  7: { src: banle, credit: "Frankie Fouganthin / Wikimedia Commons (CC BY-SA 4.0)" },
  8: { src: marketing, credit: "Today Testing / Wikimedia Commons (CC BY-SA 4.0)" },
};

export function getJobThumbnail(maNganhNghe) {
  return THUMBNAIL_BY_NGANH[maNganhNghe] ?? null;
}
