// Doc MaTK cua tai khoan dang dang nhap tu payload JWT, khong goi them API.
// Chi dung cho muc dich trang tri (vd chon banner cong ty co dinh theo
// MaTK) - KHONG dung claim nay de tu quyet dinh quyen truy cap, quyen van
// do backend kiem tra rieng qua [Authorize].
export function decodeJwtMaTk(token) {
  try {
    const payload = token.split(".")[1];
    const json = atob(payload.replace(/-/g, "+").replace(/_/g, "/"));
    const claims = JSON.parse(json);
    const maTk = claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
    return maTk ? Number(maTk) : null;
  } catch {
    return null;
  }
}
