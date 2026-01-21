(function (global) {
  "use strict";

  function CtAccessRulesHtml(helpers) {
    this.helpers = helpers;
  }

  CtAccessRulesHtml.prototype.buildPageHtml = function () {
    var escapeHtml = this.helpers.escapeHtml.bind(this.helpers);
    var t = this.helpers.t.bind(this.helpers);

    return ""
      + "<div class='ct-access-rules-page'>"
      + " <div class='panel panel-default'>"
      + " <div class='panel-heading'>"
      + " <h3 class='panel-title'>" + escapeHtml(t("AccessRules", "Content Type Access Rules")) + "</h3>"
      + " </div>"
      + " <div class='panel-body'>"

      + " <div class='row'>"
      + " <div class='col-md-6 col-xs-12'>"
      + " <label>" + escapeHtml(t("ContentType", "Content Type")) + "</label>"
      + " <div id='ctContainer'>"
      + " <select id='cmbContentTypeRules' class='form-control'></select>"
      + " </div>"
      + " </div>"
      + " <div class='col-md-6 col-xs-12'>"
      + " <label>" + escapeHtml(t("Targets", "Users / Groups / Roles")) + "</label>"
      + " <div id='targetsContainer'>"
      + " <select id='cmbTargets' class='form-control' multiple='multiple'></select>"
      + " </div>"
      + " </div>"
      + " </div>"

      + " <hr/>"

      + " <div class='row'>"
      + " <div class='col-md-12'>"
      + " <div style='display:flex; justify-content:space-between; align-items:center;'>"
      + " <h4 style='margin:0;'>" + escapeHtml(t("Conditions", "Conditions")) + "</h4>"
      + " <button type='button' id='btnAddCondition' class='btn btn-xs btn-warning'>"
      + " <i class='fa fa-plus'></i> " + escapeHtml(t("AddCondition", "Add condition"))
      + " </button>"
      + " </div>"
      + " <div id='conditionsContainer' style='margin-top:10px;'></div>"
      + " </div>"
      + " </div>"

      + " <hr/>"

      + " <div class='row'>"
      + " <div class='col-md-12'>"
      + " <button type='button' id='btnSaveRule' class='btn btn-primary'>"
      + " <i class='fa fa-save'></i> " + escapeHtml(t("Save", "Save"))
      + " </button>"
      + " <button type='button' id='btnResetRule' class='btn btn-default'>"
      + " <i class='fa fa-undo'></i> " + escapeHtml(t("Clear", "Clear"))
      + " </button>"
      + " </div>"
      + " </div>"

      + " <hr/>"

      + " <h4 style='margin-top:0;'>" + escapeHtml(t("SavedRules", "Saved rules")) + "</h4>"
      + " <div class='table-responsive'>"
      + " <table class='table table-bordered table-striped'>"
      + " <thead>"
      + " <tr>"
      + " <th style='width:120px;'>" + escapeHtml(t("ContentType", "Content Type")) + "</th>"
      + " <th>" + escapeHtml(t("Conditions", "Conditions")) + "</th>"
      + " <th style='width:260px;'>" + escapeHtml(t("Targets", "Targets")) + "</th>"
      + " <th style='width:90px;'>" + escapeHtml(t("Actions", "Actions")) + "</th>"
      + " </tr>"
      + " </thead>"
      + " <tbody id='rulesTableBody'>"
      + " <tr><td colspan='4'>" + escapeHtml(t("Loading", "Loading...")) + "</td></tr>"
      + " </tbody>"
      + " </table>"
      + " </div>"

      + " </div>"
      + " </div>"
      + "</div>";
  };

  global.CtAccessRulesHtml = CtAccessRulesHtml;
})(window);
