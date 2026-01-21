export function getBearerToken() {
  const value = (window.IdentityAccessToken || "").trim();
  return value || "";
}
