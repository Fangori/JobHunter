import { useEffect, useRef, useState } from "react";
import { FileText, Upload } from "lucide-react";

export default function FileUpload({ label, accept, value, onChange, variant = "document", existingUrl }) {
  const inputRef = useRef(null);
  const [previewUrl, setPreviewUrl] = useState(null);

  useEffect(() => {
    if ((variant !== "avatar" && variant !== "banner") || !value) {
      setPreviewUrl(null);
      return;
    }
    const url = URL.createObjectURL(value);
    setPreviewUrl(url);
    return () => URL.revokeObjectURL(url);
  }, [value, variant]);

  const handleChange = (e) => {
    const file = e.target.files[0];
    onChange(file);
  };

  const displayUrl = previewUrl || existingUrl;

  if (variant === "banner") {
    return (
      <div className="field">
        {label && <label>{label}</label>}
        <div
          style={{
            position: "relative", height: 180, borderRadius: "var(--radius-lg)", overflow: "hidden",
            background: displayUrl ? undefined : "var(--info-bg)",
          }}
        >
          {displayUrl && (
            <img src={displayUrl} alt="" style={{ width: "100%", height: "100%", objectFit: "cover", display: "block" }} />
          )}
          <button
            type="button"
            className="btn btn-secondary"
            style={{ position: "absolute", right: 12, bottom: 12, height: 36, background: "rgba(255,255,255,0.92)" }}
            onClick={() => inputRef.current.click()}
          >
            {displayUrl ? "Đổi ảnh bìa" : "Tải ảnh bìa lên"}
          </button>
        </div>
        <input ref={inputRef} type="file" accept={accept} onChange={handleChange} style={{ display: "none" }} />
      </div>
    );
  }

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
