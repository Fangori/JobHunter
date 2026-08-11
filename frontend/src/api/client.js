// VITE_API_BASE_URL: dat khi build cho moi truong deploy (vd
// https://ten-api.azurewebsites.net/api). Khong dat -> mac dinh localhost
// cho dev. Bien Vite phai bat dau bang "VITE_" moi duoc expose ra client.
export const BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5147/api";

async function request(method, path, body, token) {
  const headers = { "Content-Type": "application/json" };
  if (token) headers["Authorization"] = `Bearer ${token}`;

  let res;
  try {
    res = await fetch(`${BASE_URL}${path}`, {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
  } catch {
    // fetch tu ban than that bai (server khong chay, mat mang, CORS...)
    // - khac voi server tra ve loi nghiep vu (vao nhanh !res.ok ben duoi).
    // Bao ro nguyen nhan thay vi de component roi vao fallback chung chung.
    throw new ApiError("Không kết nối được đến server. Vui lòng kiểm tra backend đã chạy chưa.", 0);
  }

  const isJson = res.headers.get("content-type")?.includes("application/json");
  const data = isJson ? await res.json() : null;

  if (!res.ok) {
    const message = data?.message || `Lỗi ${res.status}`;
    throw new ApiError(message, res.status);
  }
  return data;
}

export class ApiError extends Error {
  constructor(message, status) {
    super(message);
    this.status = status;
  }
}

export const api = {
  get: (path, token) => request("GET", path, undefined, token),
  post: (path, body, token) => request("POST", path, body, token),
  put: (path, body, token) => request("PUT", path, body, token),
  del: (path, token) => request("DELETE", path, undefined, token),
};
