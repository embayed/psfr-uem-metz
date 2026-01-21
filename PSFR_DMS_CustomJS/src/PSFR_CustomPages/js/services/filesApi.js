import { getBearerToken } from "../utils/security.js";
import { normalizeBaseUrl } from "../utils/url.js";

const BASE_URL = normalizeBaseUrl(window.BASE_URL || "");

export async function getFileLocation(fileId) {
  const token = getBearerToken();
  const headers = { "Content-Type": "application/json" };
  if (token) headers["Authorization"] = "Bearer " + token;

  const res = await fetch(
    BASE_URL + "/Files/GetFileLocation?id=" + encodeURIComponent(String(fileId)),
    { method: "GET", headers }
  );

  if (!res.ok) {
    const text = await safeReadText(res);
    throw new Error("API error " + res.status + (text ? " - " + text : ""));
  }

  return await res.json();
}

async function safeReadText(res) {
  try {
    return await res.text();
  } catch {
    return "";
  }
}
