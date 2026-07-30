import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import Register from "./Register";
import { api, ApiError } from "../../api/client";

vi.mock("../../api/client", async (importOriginal) => {
  const actual = await importOriginal();
  return { ...actual, api: { ...actual.api, post: vi.fn() } };
});

// Cac <label> trong app hien khong gan htmlFor/id voi <input> (khoang trong
// accessibility da biet, ghi trong docs/IMPLEMENTATION_PLAN.md) nen phai tim
// qua cau truc DOM ".field" thay vi getByLabelText.
function fieldInput(container, labelText) {
  const label = Array.from(container.querySelectorAll(".field label")).find((el) => el.textContent === labelText);
  return label.parentElement.querySelector("input");
}

describe("Register - CandidateForm (QD01)", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("shows a client-side error and never calls the API when passwords do not match", async () => {
    const user = userEvent.setup();
    const { container } = render(<Register />);

    await user.type(fieldInput(container, "Họ và Tên"), "Nguyen Van A");
    await user.type(fieldInput(container, "Email"), "a@test.local");
    await user.type(fieldInput(container, "Mật khẩu"), "Test1234");
    await user.type(fieldInput(container, "Xác nhận mật khẩu"), "KhacMatKhau1");

    await user.click(screen.getByRole("button", { name: "Đăng ký ngay" }));

    expect(await screen.findByText("Mật khẩu xác nhận không khớp.")).toBeInTheDocument();
    expect(api.post).not.toHaveBeenCalled();
  });

  it("surfaces the server's duplicate-email error message without inventing its own wording", async () => {
    api.post.mockRejectedValueOnce(new ApiError("Email đã được sử dụng.", 409));
    const user = userEvent.setup();
    const { container } = render(<Register />);

    await user.type(fieldInput(container, "Họ và Tên"), "Nguyen Van B");
    await user.type(fieldInput(container, "Email"), "trung@test.local");
    await user.type(fieldInput(container, "Mật khẩu"), "Test1234");
    await user.type(fieldInput(container, "Xác nhận mật khẩu"), "Test1234");

    await user.click(screen.getByRole("button", { name: "Đăng ký ngay" }));

    expect(await screen.findByText("Email đã được sử dụng.")).toBeInTheDocument();
    expect(api.post).toHaveBeenCalledWith("/auth/register/candidate", expect.objectContaining({ email: "trung@test.local" }));
  });

  it("shows the success message returned by the API on successful registration", async () => {
    api.post.mockResolvedValueOnce({ message: "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản." });
    const user = userEvent.setup();
    const { container } = render(<Register />);

    await user.type(fieldInput(container, "Họ và Tên"), "Nguyen Van C");
    await user.type(fieldInput(container, "Email"), "moi@test.local");
    await user.type(fieldInput(container, "Mật khẩu"), "Test1234");
    await user.type(fieldInput(container, "Xác nhận mật khẩu"), "Test1234");
    await user.click(screen.getByRole("button", { name: "Đăng ký ngay" }));

    expect(await screen.findByText("Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản.")).toBeInTheDocument();
  });
});
