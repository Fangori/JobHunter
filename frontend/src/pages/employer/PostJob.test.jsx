import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import PostJob from "./PostJob";
import { api, ApiError } from "../../api/client";
import { renderWithProviders, seedAuth } from "../../test/renderWithProviders";

vi.mock("../../api/client", async (importOriginal) => {
  const actual = await importOriginal();
  return { ...actual, api: { ...actual.api, get: vi.fn(), post: vi.fn() } };
});

function fieldInput(container, labelText, tag = "input") {
  const label = Array.from(container.querySelectorAll(".field label")).find((el) => el.textContent === labelText);
  return label.parentElement.querySelector(tag);
}

describe("PostJob - dang tin (QD09 han nop ho so)", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
    seedAuth();
    api.get.mockResolvedValue([]); // GET /skills
  });

  it("surfaces the server's QD09 deadline error (MS06) instead of a generic message", async () => {
    api.post.mockRejectedValueOnce(new ApiError("Hạn nộp hồ sơ phải sau ngày đăng tin ít nhất 1 ngày.", 400));
    const user = userEvent.setup();
    const { container } = renderWithProviders(<PostJob />, { route: "/employer/post-job" });

    await user.type(fieldInput(container, "Tiêu đề tin"), "Backend Dev");
    await user.type(fieldInput(container, "Mô tả công việc", "textarea"), "Mo ta cong viec");
    const hanNopInput = fieldInput(container, "Hạn nộp hồ sơ");
    await user.clear(hanNopInput);
    await user.type(hanNopInput, "2020-01-01"); // ngay qua khu, vi pham QD09

    await user.click(screen.getByRole("button", { name: "Đăng tin" }));

    expect(await screen.findByText("Hạn nộp hồ sơ phải sau ngày đăng tin ít nhất 1 ngày.")).toBeInTheDocument();
  });

  it("shows the server's success message (MS05) on a valid submission", async () => {
    api.post.mockResolvedValueOnce({ maTin: 1, trangThai: "ChoDuyet" });
    const user = userEvent.setup();
    const { container } = renderWithProviders(<PostJob />, { route: "/employer/post-job" });

    await user.type(fieldInput(container, "Tiêu đề tin"), "Backend Dev");
    await user.type(fieldInput(container, "Mô tả công việc", "textarea"), "Mo ta cong viec");
    await user.type(fieldInput(container, "Hạn nộp hồ sơ"), "2027-01-01");

    await user.click(screen.getByRole("button", { name: "Đăng tin" }));

    expect(await screen.findByText("Đăng tin thành công, tin đang chờ Admin duyệt.")).toBeInTheDocument();
    expect(api.post).toHaveBeenCalledWith(
      "/jobs",
      expect.objectContaining({ tieuDe: "Backend Dev", hanNopHoSo: "2027-01-01" }),
      "fake-token"
    );
  });
});
