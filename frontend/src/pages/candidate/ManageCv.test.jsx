import { fireEvent } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import ManageCv from "./ManageCv";
import { api } from "../../api/client";
import { renderWithProviders, seedAuth } from "../../test/renderWithProviders";

vi.mock("../../api/client", async (importOriginal) => {
  const actual = await importOriginal();
  return { ...actual, api: { ...actual.api, get: vi.fn() } };
});

function fieldInput(scope, labelText, tag = "input") {
  const label = Array.from(scope.querySelectorAll(".field label")).find((el) => el.textContent === labelText);
  return label.parentElement.querySelector(tag);
}

describe("ManageCv - tai len CV (dinh dang/dung luong file)", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
    seedAuth({ token: "fake-token", vaiTro: "UngVien", hoTenOrTenCongTy: "Test UV" });
    api.get.mockResolvedValue([]); // /skills, /cvs/mine, /cvs/mine?trangThai=DaAn deu tra rong
    global.fetch = vi.fn();
  });

  it("surfaces the server's file-validation error (MS36) when the upload is rejected", async () => {
    global.fetch.mockResolvedValueOnce({
      ok: false,
      status: 400,
      json: async () => ({ message: "File không đúng định dạng hoặc vượt quá 10MB." }),
    });
    const user = userEvent.setup();
    const { container, findByText } = renderWithProviders(<ManageCv />);

    const forms = container.querySelectorAll("form");
    const uploadForm = forms[forms.length - 1];
    await user.type(fieldInput(uploadForm, "Tên CV"), "CV cua toi");
    const fileInput = uploadForm.querySelector('input[type="file"]');
    await user.upload(fileInput, new File(["noi dung"], "cv.pdf", { type: "application/pdf" }));

    // jsdom khong tinh dung constraint-validation cho input[type=file] duoc
    // gan file qua userEvent.upload nen dung fireEvent.submit truc tiep thay
    // vi click nut (click se bi HTML5 required validation chan sai).
    fireEvent.submit(uploadForm);

    expect(await findByText("File không đúng định dạng hoặc vượt quá 10MB.")).toBeInTheDocument();
  });

  it("shows the server's success message (MS35) when the upload succeeds", async () => {
    global.fetch.mockResolvedValueOnce({
      ok: true,
      status: 201,
      json: async () => ({ maCV: 1, tenCV: "CV cua toi", loaiCV: "Upload", trangThai: "HoatDong" }),
    });
    const user = userEvent.setup();
    const { container, findByText } = renderWithProviders(<ManageCv />);

    const forms = container.querySelectorAll("form");
    const uploadForm = forms[forms.length - 1];
    await user.type(fieldInput(uploadForm, "Tên CV"), "CV cua toi");
    const fileInput = uploadForm.querySelector('input[type="file"]');
    await user.upload(fileInput, new File(["%PDF-1.4"], "cv.pdf", { type: "application/pdf" }));

    fireEvent.submit(uploadForm);

    expect(await findByText("Tải lên CV thành công.")).toBeInTheDocument();
  });
});
