const BASE_URL = "http://localhost:5147/api";

async function request(method, path, body, token) {
  const headers = { "Content-Type": "application/json" };
  if (token) headers["Authorization"] = `Bearer ${token}`;

  const res = await fetch(`${BASE_URL}${path}`, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

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
