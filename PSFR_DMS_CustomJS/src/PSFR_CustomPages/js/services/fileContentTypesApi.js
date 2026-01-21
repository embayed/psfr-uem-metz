import { normalizeBaseUrl } from "../utils/url.js";
import { getBearerToken } from "../utils/security.js";

export async function getActiveFileContentTypes() {

  const token = getBearerToken();
  const headers = { "Content-Type": "application/json" };
  if (token) headers["Authorization"] = "Bearer " + token;

  const res = await fetch( "/FileContentTypes/ListActiveBasic", {
    method: "GET",
    headers: headers
  });

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
