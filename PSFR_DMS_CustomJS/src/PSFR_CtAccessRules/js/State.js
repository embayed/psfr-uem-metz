(function (global) {
  "use strict";

  function CtAccessRulesState() {
    this.contentTypes = [];
    this.fields = [];
    this.contentTypeId = null;
    this.contentTypeText = "";
    this.translationMap = {};
    this.formComponents = [];
    this.conditionCounter = 0;
  }

  CtAccessRulesState.prototype.resetConditions = function () {
    this.conditionCounter = 0;
  };

  global.CtAccessRulesState = CtAccessRulesState;
})(window);

