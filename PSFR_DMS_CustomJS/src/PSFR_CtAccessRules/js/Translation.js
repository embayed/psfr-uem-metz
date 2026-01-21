(function (global) {
  "use strict";

  function CtAccessRulesTranslation() { }

  CtAccessRulesTranslation.prototype.parseTranslationMap = function (formTranslationJson) {
    var map = {};
    try {
      var arr = JSON.parse(formTranslationJson || "[]");
      for (var i = 0; i < arr.length; i++) {
        if (!arr[i] || !arr[i].Keyword) continue;
        map[arr[i].Keyword] = arr[i];
      }
    } catch (e) {
      // ignore
    }
    return map;
  };

  CtAccessRulesTranslation.prototype.buildFieldLabel = function (component, translationMap) {
    var label = component && component.label
      ? component.label
      : component && component.key
        ? component.key
        : "";

    if (!label) return "";

    var tr = translationMap[label];
    if (!tr) return label;

    if (global.language === "fr" && tr.Fr) return tr.Fr;
    if (global.language === "ar" && tr.Ar) return tr.Ar;
    if (global.language === "en" && tr.En) return tr.En;

    return label;
  };

  global.CtAccessRulesTranslation = CtAccessRulesTranslation;
})(window);
