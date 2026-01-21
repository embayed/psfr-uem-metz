(function (global) {
  "use strict";

  function CtAccessRulesPage(constants, helpers, api, state, html, translation, formio, conditions, rulesTable, targets) {
    this.constants = constants;
    this.helpers = helpers;
    this.api = api;
    this.state = state;
    this.html = html;
    this.translation = translation;
    this.formio = formio;
    this.conditions = conditions;
    this.rulesTable = rulesTable;
    this.targets = targets;
  }

  CtAccessRulesPage.prototype.renderIntoContentWrapper = function () {
    var container = document.querySelector(".content-wrapper");
    if (!container) return;

    container.innerHTML = "<div id='ctAccessRulesHost'></div>";
    $("#ctAccessRulesHost").html(this.html.buildPageHtml());

    this.initContentTypeDropdown();
    this.targets.initSelect2();
    this.wireButtons();
    this.loadContentTypes();
  };

  CtAccessRulesPage.prototype.initContentTypeDropdown = function () {
    if (!$.fn.select2) return;

    var isRtl = this.helpers.isRtl.bind(this.helpers);
    var t = this.helpers.t.bind(this.helpers);
    var self = this;

    $("#cmbContentTypeRules").select2({
      dir: isRtl() ? "rtl" : "ltr",
      language: global.language,
      width: "100%",
      allowClear: true,
      placeholder: t("SelectContentType", "Select content type"),
      dropdownParent: $("#ctContainer")
    });

    $("#cmbContentTypeRules").on("change", function () {
      var selected = $("#cmbContentTypeRules").select2("data")[0];

      self.state.contentTypeId = selected ? selected.id : null;
      self.state.contentTypeText = selected ? selected.text : "";

      self.conditions.reset();
      self.state.fields = [];

      if (!self.state.contentTypeId) {
        $("#conditionsContainer").html("");
        self.rulesTable.render([]);
        return;
      }

      self.loadFieldsByContentTypeId(self.state.contentTypeId);
      self.loadSavedRules(self.state.contentTypeId);
    });
  };

  CtAccessRulesPage.prototype.wireButtons = function () {
    var self = this;
    var t = this.helpers.t.bind(this.helpers);

    $("#btnAddCondition").on("click", function () {
      if (!self.state.contentTypeId) {
        self.helpers.showError(t("SelectContentTypeFirst", "Please select a content type first."));
        return;
      }
      self.conditions.addRow();
    });

    $("#btnResetRule").on("click", function () {
      self.conditions.reset();
      $("#cmbTargets").val(null).trigger("change");
    });

    $("#btnSaveRule").on("click", function () {
      self.saveRule();
    });

    $("#conditionsContainer").on("click", ".btn-delete-condition", function () {
      $(this).closest(".condition-row").remove();
    });

    $("#rulesTableBody").on("click", ".btn-delete-rule", function () {
      var id = $(this).attr("data-id");
      if (!id) return;
      self.deleteRule(id);
    });
  };

  CtAccessRulesPage.prototype.loadContentTypes = function () {
    var self = this;

    this.api.loadContentTypes(function (items) {
      self.state.contentTypes = items;

      var data = self.state.contentTypes.map(function (x) { return { id: x.id, text: x.text }; });

      $("#cmbContentTypeRules").empty();
      $("#cmbContentTypeRules").select2({
        data: data,
        dir: self.helpers.isRtl() ? "rtl" : "ltr",
        language: global.language,
        width: "100%",
        allowClear: true,
        placeholder: self.helpers.t("SelectContentType", "Select content type"),
        dropdownParent: $("#ctContainer")
      });

      $("#cmbContentTypeRules").val(null).trigger("change");
      self.rulesTable.render([]);
      $("#conditionsContainer").html("");
    });
  };

  CtAccessRulesPage.prototype.loadFieldsByContentTypeId = function (contentTypeId) {
    var self = this;

    this.api.loadFields(
      contentTypeId,
      function (resp) {
        var fields = Array.isArray(resp && resp.fields) ? resp.fields : [];
        var formJson = resp && resp.form ? resp.form : null;
        var formTranslation = resp && resp.formTranslation ? resp.formTranslation : "[]";

        self.state.translationMap = self.translation.parseTranslationMap(formTranslation);
        self.state.formComponents = self.formio.tryParseComponents(formJson);

        self.state.fields = fields.map(function (f) {
          var comp = self.formio.findComponentByKey(self.state.formComponents, f.key);
          var label = comp ? self.translation.buildFieldLabel(comp, self.state.translationMap) : f.key;
          return { key: f.key, label: label, type: f.type };
        });

        self.conditions.reset();
        self.conditions.addRow();
      },
      function () {
        self.state.fields = [];
        self.conditions.reset();
      }
    );
  };

  CtAccessRulesPage.prototype.loadSavedRules = function (contentTypeId) {
    var self = this;
    var escapeHtml = this.helpers.escapeHtml.bind(this.helpers);
    var t = this.helpers.t.bind(this.helpers);

    $("#rulesTableBody").html("<tr><td colspan='4'>" + escapeHtml(t("Loading", "Loading...")) + "</td></tr>");

    this.api.listRules(
      contentTypeId,
      function (items) {
        self.rulesTable.render(items);
      },
      function () {
        self.rulesTable.render([]);
        self.helpers.showError("Failed to load saved rules.");
      }
    );
  };

  CtAccessRulesPage.prototype.saveRule = function () {
    var t = this.helpers.t.bind(this.helpers);

    if (!this.state.contentTypeId) {
      this.helpers.showError(t("SelectContentTypeFirst", "Please select a content type first."));
      return;
    }

    var conditions = this.conditions.buildPayload();
    if (!conditions.length) {
      this.helpers.showError(t("AddAtLeastOneCondition", "Please add at least one valid condition."));
      return;
    }

    var targets = this.targets.buildPayload();
    if (!targets.length) {
      this.helpers.showError(t("SelectAtLeastOneTarget", "Please select at least one user/group/role."));
      return;
    }

    var body = {
      contentTypeId: Number(this.state.contentTypeId),
      contentTypeText: this.state.contentTypeText,
      conditions: conditions,
      targets: targets
    };

    var self = this;
    this.api.createRule(body, function () {
      self.helpers.showSuccess(t("Saved", "Saved successfully"));
      self.conditions.reset();
      $("#cmbTargets").val(null).trigger("change");
      self.loadSavedRules(self.state.contentTypeId);
    });
  };

CtAccessRulesPage.prototype.deleteRule = function (id) {
  var self = this;
  var t = this.helpers.t.bind(this.helpers);

  Common.showConfirmMsg( t("ConfirmDeleteRule", Common.translate("Are you sure you want to delete this rule?")),
    function () {
      self.api.deleteRule(id, function () {
        self.helpers.showSuccess(t("Deleted", "Deleted"));
        self.loadSavedRules(self.state.contentTypeId);
      });
    }
  );
};


  global.CtAccessRulesPage = CtAccessRulesPage;
})(window);
