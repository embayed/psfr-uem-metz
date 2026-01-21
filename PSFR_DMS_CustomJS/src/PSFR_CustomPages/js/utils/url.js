export function normalizeBaseUrl(value) {
  const url = (value || "").trim();
  if (!url) return "";
  return url.endsWith("/") ? url.slice(0, -1) : url;
}
