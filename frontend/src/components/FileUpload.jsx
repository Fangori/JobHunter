import { useRef, useState } from "react";
import { FileText, Upload } from "lucide-react";

export default function FileUpload({ label, accept, value, onChange, variant = "document", existingUrl }) {
  const inputRef = useRef(null);
  const [previewUrl, setPreviewUrl] = useState(null);

  const handleChange = (e) => {
    const file = e.target.files[0];
    if (file && variant === "avatar") {
      setPreviewUrl(URL.createObjectURL(file));
    }
    onChange(file);
  };

  const displayUrl = previewUrl || existingUrl;

  return (
    <div className="field">
      {label && <label>{label}</label>}
      <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
        {variant === "avatar" && (
          displayUrl ? (
            <img src={displayUrl} alt="Ảnh xem trước" style={{ width: 64, height: 64, borderRadius: "50%", objectFit: "cover", flexShrink: 0 }} />
          ) : (
            <div style={{ width: 64, height: 64, borderRadius: "50%", background: "var(--info-bg)", color: "var(--indigo-dark)", display: "flex", alignItems: "center", justifyContent: "center", flexShrink: 0 }}>
              <Upload size={22} />
            </div>
          )
        )}
        <button type="button" className="btn btn-secondary" style={{ height: 40 }} onClick={() => inputRef.current.click()}>
          {variant === "avatar" ? "Chọn ảnh" : "Chọn file"}
        </button>
        {variant === "document" && (
          <span style={{ display: "flex", alignItems: "center", gap: 6, color: "var(--text-muted)", fontSize: 14 }}>
            <FileText size={16} />
            {value ? value.name : "Chưa chọn file"}
          </span>
        )}
      </div>
      <input ref={inputRef} type="file" accept={accept} onChange={handleChange} style={{ display: "none" }} />
    </div>
  );
}
