import { render } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { AuthProvider } from "../context/AuthContext";

// AuthProvider doc trang thai dang nhap tu localStorage luc mount, nen seed
// truoc khi render de cac form can auth.token (PostJob, ManageCv...) hoat dong.
export function seedAuth(auth = { token: "fake-token", vaiTro: "NhaTuyenDung", hoTenOrTenCongTy: "Test Co" }) {
  localStorage.setItem("jobhunter_auth", JSON.stringify(auth));
}

export function renderWithProviders(ui, { route = "/" } = {}) {
  return render(
    <MemoryRouter initialEntries={[route]}>
      <AuthProvider>{ui}</AuthProvider>
    </MemoryRouter>
  );
}
