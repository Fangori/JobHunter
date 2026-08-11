import { useState } from "react";
import { Lock, Eye, EyeOff } from "lucide-react";

// Dung chung cho moi o nhap mat khau (Dang nhap, Dang ky UV/NTD) - gop y
// "them nut hien mat khau" tu file gop y.docx (2026-08-11).
export default function PasswordInput({ value, onChange, required, autoComplete }) {
  const [show, setShow] = useState(false);

  return (
    <div className="input-icon-wrap has-toggle">
      <Lock size={18} />
      <input
        type={show ? "text" : "password"}
        value={value}
        onChange={onChange}
        required={required}
        autoComplete={autoComplete}
      />
      <button
        type="button"
        className="input-icon-toggle"
        onClick={() => setShow((s) => !s)}
        tabIndex={-1}
        aria-label={show ? "Ẩn mật khẩu" : "Hiện mật khẩu"}
      >
        {show ? <EyeOff size={18} /> : <Eye size={18} />}
      </button>
    </div>
  );
}
