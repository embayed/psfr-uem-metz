(function (global) {
  "use strict";

  function CtAccessRulesApi(constants, ajax) {
    this.constants = constants;
    this.ajax = ajax;
  }

  CtAccessRulesApi.prototype.loadContentTypes = function (onOk) {
    this.ajax.get(this.constants.API_CONTENT_TYPES, null, function (items) {
      onOk(Array.isArray(items) ? items : []);
    }, function () {
      onOk([]);
    });
  };

  CtAccessRulesApi.prototype.loadFields = function (contentTypeId, onOk, onFail) {
    this.ajax.get(this.constants.API_FIELDS, { contentTypeId: contentTypeId }, function (resp) {
      onOk(resp || {});
    }, onFail);
  };

  CtAccessRulesApi.prototype.listRules = function (contentTypeId, onOk, onFail) {
    this.ajax.get(this.constants.API_RULES_LIST, { contentTypeId: contentTypeId }, function (items) {
      onOk(Array.isArray(items) ? items : []);
    }, onFail);
  };

  CtAccessRulesApi.prototype.createRule = function (body, onOk, onFail) {
    this.ajax.json("POST", this.constants.API_RULES_CREATE, body, onOk, onFail);
  };

  CtAccessRulesApi.prototype.deleteRule = function (id, onOk, onFail) {
    var url = this.constants.API_RULES_DELETE + "?id=" + encodeURIComponent(id);
    this.ajax.delete(url, onOk, onFail);
  };

  global.CtAccessRulesApi = CtAccessRulesApi;
})(window);
