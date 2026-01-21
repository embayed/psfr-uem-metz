import { normalizeBaseUrl } from "../utils/url.js";
import { getBearerToken } from "../utils/security.js";

export async function getMetadataTreeLevel(fileContentTypeId, orderedFields, path) {
  const baseUrl = normalizeBaseUrl(window.CustomDmsUrl);
  if (!baseUrl) throw new Error("window.CustomDmsUrl is empty.");

  const token = getBearerToken();
  const headers = { "Content-Type": "application/json" };
  if (token) headers["Authorization"] = "Bearer " + token;

  const res = await fetch(baseUrl + "/api/metadata-tree/level", {
    method: "POST",
    headers: headers,
    body: JSON.stringify({ fileContentTypeId: fileContentTypeId, orderedFields: orderedFields, path: path })
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
