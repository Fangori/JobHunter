export const ROLE_LABELS = {
  UngVien: "Ứng viên",
  NhaTuyenDung: "Nhà tuyển dụng",
  Admin: "Quản trị viên",
};

export function roleLabel(vaiTro) {
  return ROLE_LABELS[vaiTro] || vaiTro;
}
