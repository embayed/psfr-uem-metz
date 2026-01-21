function downloadSearchResultExcel(SelectedFiles) {

  // --- Pre-checks
  if (!window.IdentityAccessToken) {
    Common.showScreenErrorMsg(Common.translate("AccessTokenMissing"));
    return;
  }

  if (!window.currentSearchPayload) {
    Common.showScreenErrorMsg(Common.translate("SearchBodyMissing"));
    return;
  }

  // --- Build form data matching backend params
  var formData = new FormData();
  formData.append("searchBody", window.currentSearchPayload);
  formData.append("searchType", window.currentSearchType);
  formData.append("language", window.language);
  formData.append("selectedFiles", SelectedFiles || "");

  var url =
    window.CustomDmsUrl.replace(/\/$/, "") +
    "/api/Export/ExportSearchResult";

  Common.mask(document.body);

  // =============================
  // FETCH PATH
  // =============================
  if (window.fetch) {
    var errorHandled = false;

    fetch(url, {
      method: "POST",
      headers: {
        Authorization: "Bearer " + window.IdentityAccessToken
      },
      body: formData
    })
      .then(function (response) {

        // --- 200 OK => XLSX file
        if (response.ok) {
          return response.blob();
        }

        // --- 401 Unauthorized
        if (response.status === 401) {
          Common.showScreenErrorMsg(
            Common.translate("ExportUnauthorized") ||
            Common.translate("SessionExpired") ||
            "Your session has expired. Please sign in again."
          );
          errorHandled = true;
          throw new Error("Unauthorized");
        }

        // --- JSON error from backend HttpAjaxError
        var ct = response.headers.get("Content-Type") || "";

        if (ct.includes("application/json")) {
          return response.json().then(function (err) {
            var backendMsg = err && (err.message || err.Message);

            Common.showScreenErrorMsg(
              backendMsg ||
              Common.translate("ExportSearchResultsFailed") ||
              "Failed to export search results."
            );
            errorHandled = true;

            throw new Error(backendMsg || "Export failed");
          });
        }

        // --- Plain-text / unknown error
        return response.text().then(function (text) {
          Common.showScreenErrorMsg(
            Common.translate("ExportSearchResultsFailed") ||
            "Failed to export search results."
          );
          errorHandled = true;

          throw new Error(text || "Export failed");
        });
      })
      .then(function (blob) {
        // --- SUCCESS
        triggerExcelDownload(blob, null);
      })
      .catch(function (err) {
        console.error("Excel export error:", err);

        // Detect "server down / no response" cases (ERR_CONNECTION_REFUSED, etc.)
        var isNetworkOrNoResponse =
          err &&
          (
            err.name === "TypeError" ||              // typical for fetch network errors
            err.message === "Failed to fetch" ||     // Chrome text
            err.message === "NetworkError when attempting to fetch resource." // Firefox
          );

        // If we didn't already show an error OR we have a pure network/no-response error,
        // show a dedicated message.
        if (!errorHandled || isNetworkOrNoResponse) {
          Common.showScreenErrorMsg(
            Common.translate("ExportNoResponse") ||
            Common.translate("ExportNetworkError") ||
            "The server did not respond. Please try again later."
          );
        }
      })
      .finally(function () {
        Common.unmask();
      });

    return;
  }

  // =============================
  // XHR FALLBACK PATH
  // =============================
  var xhr = new XMLHttpRequest();
  xhr.open("POST", url, true);
  xhr.responseType = "blob";
  xhr.setRequestHeader("Authorization", "Bearer " + window.IdentityAccessToken);

  xhr.onload = function () {

    if (xhr.status === 200) {
      triggerExcelDownload(
        xhr.response,
        xhr.getResponseHeader("Content-Disposition")
      );
      Common.unmask();
      return;
    }

    if (xhr.status === 401) {
      Common.showScreenErrorMsg(
        Common.translate("ExportUnauthorized") ||
        Common.translate("SessionExpired") ||
        "Your session has expired. Please sign in again."
      );

      Common.unmask();
      return;
    }

    // status === 0 is usually "server down / CORS / connection refused"
    if (xhr.status === 0) {
      Common.showScreenErrorMsg(
        Common.translate("ExportNoResponse") ||
        Common.translate("ExportNetworkError") ||
        "The server did not respond. Please try again later."
      );
      Common.unmask();
      return;
    }

    var ct = xhr.getResponseHeader("Content-Type") || "";
    var backendMsg = null;

    if (ct.includes("application/json") && xhr.responseText) {
      try {
        var err = JSON.parse(xhr.responseText);
        backendMsg = err && (err.message || err.Message);
      } catch (_) { }
    }

    Common.showScreenErrorMsg(
      backendMsg ||
      Common.translate("ExportSearchResultsFailed") ||
      "Failed to export search results."
    );

    Common.unmask();
  };

  xhr.onerror = function () {
    // Clearly a network / no-response case
    Common.showScreenErrorMsg(
      Common.translate("ExportNoResponse") ||
      Common.translate("ExportNetworkError") ||
      "The server did not respond. Please try again later."
    );

    Common.unmask();
  };

  xhr.send(formData);
}

function triggerExcelDownload(blob, contentDisposition) {
  var fileName = "SearchResult.xlsx";

  if (contentDisposition && contentDisposition.indexOf("filename=") !== -1) {
    fileName = contentDisposition
      .split("filename=")[1]
      .replace(/["']/g, "")
      .trim();
  }

  var url = window.URL.createObjectURL(blob);
  var a = document.createElement("a");
  a.href = url;
  a.download = fileName;

  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  window.URL.revokeObjectURL(url);
}

$.ajaxSetup({
  beforeSend: function (jqXHR, settings) {
    if (settings.url.includes("/AdvancedSearch/List")) {
      window.currentSearchPayload = settings.data;
      window.currentSearchType = "AdvancedSearch";
    }

    if (settings.url.includes("/ExpertSearch/List")) {
      window.currentSearchPayload = settings.data;
      window.currentSearchType = "ExpertSearch";
    }
    
    if (settings.url.includes("/AdvancedSearch/ListAdvance")) {
      window.currentSearchPayload = settings.data;
      window.currentSearchType = "AdvanceSearch";
    }
  }
});
