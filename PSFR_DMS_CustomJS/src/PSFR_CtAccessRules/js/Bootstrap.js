(function (global) {
  "use strict";

  function start() {
    var constants = global.CtAccessRulesConstants;
    var helpers = new global.CtAccessRulesHelpers();
    var ajax = new global.CtAccessRulesAjax(helpers);
    var api = new global.CtAccessRulesApi(constants, ajax);
    var state = new global.CtAccessRulesState();
    var html = new global.CtAccessRulesHtml(helpers);
    var translation = new global.CtAccessRulesTranslation();
    var formio = new global.CtAccessRulesFormio();
    var conditions = new global.CtAccessRulesConditions(helpers, state);
    var rulesTable = new global.CtAccessRulesRulesTable(helpers, state);
    var targets = new global.CtAccessRulesTargets(helpers, constants);

    var page = new global.CtAccessRulesPage(constants, helpers, api, state, html, translation, formio, conditions, rulesTable, targets);

    var hasProductRouter = global.DMSComponents && global.DMSComponents.Router && global.Backbone && global.Backbone.history;

    if (hasProductRouter) {
      // Register route in product router WITHOUT touching history
      if (typeof global.DMSComponents.Router.route === "function") {
        global.DMSComponents.Router.route("ct-access-rules", "ctAccessRulesRoute", function () {
          page.renderIntoContentWrapper();
        });
      }
      return;
    }

    // Fallback: pure hash handling
    var router = new global.CtAccessRulesRouter(constants, page);
    router.start();
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", start);
    return;
  }

  start();
})(window);
