(function (global) {
  "use strict";

  function CtAccessRulesRouter(constants, page) {
    this.constants = constants;
    this.page = page;
  }

  CtAccessRulesRouter.prototype.onHashChange = function () {
    if ((global.location.hash || "") === this.constants.HASH) {
      this.page.renderIntoContentWrapper();
    }
  };

  CtAccessRulesRouter.prototype.start = function () {
    this.onHashChange();
    global.addEventListener("hashchange", this.onHashChange.bind(this));
  };

  global.CtAccessRulesRouter = CtAccessRulesRouter;
})(window);
