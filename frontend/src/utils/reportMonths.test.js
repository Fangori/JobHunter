import { describe, it, expect } from "vitest";
import { layDanhSach6ThangGanNhat } from "./reportMonths";

describe("layDanhSach6ThangGanNhat", () => {
  it("khong qua nam khi thang du lon", () => {
    expect(layDanhSach6ThangGanNhat(8, 2026)).toEqual([
      { thang: 3, nam: 2026 },
      { thang: 4, nam: 2026 },
      { thang: 5, nam: 2026 },
      { thang: 6, nam: 2026 },
      { thang: 7, nam: 2026 },
      { thang: 8, nam: 2026 },
    ]);
  });

  it("lui qua nam truoc khi thang dang chon la thang 3", () => {
    expect(layDanhSach6ThangGanNhat(3, 2026)).toEqual([
      { thang: 10, nam: 2025 },
      { thang: 11, nam: 2025 },
      { thang: 12, nam: 2025 },
      { thang: 1, nam: 2026 },
      { thang: 2, nam: 2026 },
      { thang: 3, nam: 2026 },
    ]);
  });

  it("truong hop bien - thang dang chon la thang 1", () => {
    expect(layDanhSach6ThangGanNhat(1, 2026)).toEqual([
      { thang: 8, nam: 2025 },
      { thang: 9, nam: 2025 },
      { thang: 10, nam: 2025 },
      { thang: 11, nam: 2025 },
      { thang: 12, nam: 2025 },
      { thang: 1, nam: 2026 },
    ]);
  });

  it("phan tu cuoi luon la thang/nam dau vao", () => {
    const ketQua = layDanhSach6ThangGanNhat(6, 2027);
    expect(ketQua).toHaveLength(6);
    expect(ketQua[5]).toEqual({ thang: 6, nam: 2027 });
  });
});
