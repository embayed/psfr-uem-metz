(function (global) {
  "use strict";

  function CtAccessRulesAjax(helpers) {
    this.helpers = helpers;
  }

  CtAccessRulesAjax.prototype.get = function (url, data, onOk, onFail) {
    if (global.Common && global.Common.ajaxGet) {
      global.Common.ajaxGet(
        url,
        data,
        onOk,
        onFail || function () { global.Common.showScreenErrorMsg(); }
      );
      return;
    }

    $.ajax({
      url: url,
      method: "GET",
      data: data || {},
      success: onOk,
      error: onFail || function () { this.helpers.showError("Request failed"); }.bind(this)
    });
  };

  CtAccessRulesAjax.prototype.json = function (method, url, body, onOk, onFail) {
    $.ajax({
      url: url,
      method: method,
      contentType: "application/json",
      data: body ? JSON.stringify(body) : null,
      success: onOk,
      error: onFail || function () { this.helpers.showError("Request failed"); }.bind(this)
    });
  };

  CtAccessRulesAjax.prototype.delete = function (url, onOk, onFail) {
    $.ajax({
      url: url,
      method: "DELETE",
      success: onOk,
      error: onFail || function () { this.helpers.showError("Delete failed"); }.bind(this)
    });
  };

  global.CtAccessRulesAjax = CtAccessRulesAjax;
})(window);
