import { createContext, useContext, useState } from "react";

const AuthContext = createContext(null);

function readStored() {
  const raw = localStorage.getItem("jobhunter_auth");
  return raw ? JSON.parse(raw) : null;
}

export function AuthProvider({ children }) {
  const [auth, setAuth] = useState(readStored());

  const login = (loginResponse) => {
    localStorage.setItem("jobhunter_auth", JSON.stringify(loginResponse));
    setAuth(loginResponse);
  };

  const logout = () => {
    localStorage.removeItem("jobhunter_auth");
    setAuth(null);
  };

  return (
    <AuthContext.Provider value={{ auth, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}
