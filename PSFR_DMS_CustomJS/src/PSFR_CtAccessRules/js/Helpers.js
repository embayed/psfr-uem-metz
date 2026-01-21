(function (global) {
  "use strict";

  function CtAccessRulesHelpers() { }

  CtAccessRulesHelpers.prototype.isRtl = function () {
    return global.language === "ar";
  };

  CtAccessRulesHelpers.prototype.t = function (key, fallback) {
    if (global.Resources && global.Resources[key]) return global.Resources[key];
    return fallback || key;
  };

  CtAccessRulesHelpers.prototype.escapeHtml = function (value) {
    if (value == null) return "";
    return String(value)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#039;");
  };

  CtAccessRulesHelpers.prototype.showError = function (msg) {
    if (global.Common && global.Common.alertMsg) {
      global.Common.alertMsg(msg);
      return;
    }
    alert(msg);
  };

  CtAccessRulesHelpers.prototype.showSuccess = function (msg) {
    if (global.Common && global.Common.showScreenSuccessMsg) {
      global.Common.showScreenSuccessMsg(msg);
      return;
    }
    alert(msg);
  };

  global.CtAccessRulesHelpers = CtAccessRulesHelpers;
})(window);
