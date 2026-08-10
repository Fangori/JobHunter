// Tra ve mang 6 cap {thang, nam} lien tiep, ket thuc dung o (thang, nam)
// dau vao - dung de goi lai API /admin/reports nhieu lan cho bieu do
// xu huong, khong doi API/backend.
export function layDanhSach6ThangGanNhat(thang, nam) {
  const ketQua = [];
  let t = thang;
  let n = nam;
  for (let i = 0; i < 6; i++) {
    ketQua.unshift({ thang: t, nam: n });
    t -= 1;
    if (t < 1) {
      t = 12;
      n -= 1;
    }
  }
  return ketQua;
}
