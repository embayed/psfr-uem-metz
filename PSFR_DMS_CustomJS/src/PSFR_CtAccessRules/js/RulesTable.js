(function (global) {
  "use strict";

  function CtAccessRulesRulesTable(helpers, state) {
    this.helpers = helpers;
    this.state = state;
  }

  CtAccessRulesRulesTable.prototype.render = function (items) {
    var escapeHtml = this.helpers.escapeHtml.bind(this.helpers);
    var t = this.helpers.t.bind(this.helpers);

    if (!items || !items.length) {
      $("#rulesTableBody").html("<tr><td colspan='4'>" + escapeHtml(t("NoData", "No data")) + "</td></tr>");
      return;
    }

    var rows = "";
    for (var i = 0; i < items.length; i++) {
      var rule = items[i];

      var ct = rule.contentTypeText || this.state.contentTypeText || "";
      var conditionsHtml = this.formatConditionsText(rule.conditions);
      var targetsText = this.formatTargetsText(rule.targets);

      rows += ""
        + "<tr>"
        + " <td>" + escapeHtml(ct) + "</td>"
        + " <td>" + conditionsHtml + "</td>"
        + " <td>" + escapeHtml(targetsText) + "</td>"
        + " <td>"
        + " <button type='button' class='btn btn-xs btn-danger btn-delete-rule' data-id='" + escapeHtml(rule.id) + "'>"
        + " <i class='fa fa-trash'></i>"
        + " </button>"
        + " </td>"
        + "</tr>";
    }

    $("#rulesTableBody").html(rows);
  };

  CtAccessRulesRulesTable.prototype.getFieldLabel = function (fieldKey) {
    var f = this.state.fields.filter(function (x) { return x.key === fieldKey; })[0];
    return f ? f.label : fieldKey;
  };

  CtAccessRulesRulesTable.prototype.formatConditionsText = function (conditions) {
    var escapeHtml = this.helpers.escapeHtml.bind(this.helpers);

    conditions = Array.isArray(conditions) ? conditions : [];
    if (!conditions.length) return "";

    var parts = [];
    for (var i = 0; i < conditions.length; i++) {
      var c = conditions[i];

      var fieldLabel = escapeHtml(this.getFieldLabel(c.fieldKey));
      var op = c.op || "Equal";

      var opText =
        op === "Equal" ? "<span class='op-eq'>==</span>" :
        op === "NotEqual" ? "<span class='op-neq'>!=</span>" :
        op === "Contains" ? "<span class='op-contains'>CONTAINS</span>" :
        op === "StartWith" ? "<span class='op-starts'>STARTS_WITH</span>" :
        op === "IsNull" ? "<span class='op-null'>IS_EMPTY</span>" :
        op === "IsNotNull" ? "<span class='op-not-null'>IS_NOT_EMPTY</span>" :
        "<span class='op-custom'>" + escapeHtml(op) + "</span>";

      if (op === "IsNull" || op === "IsNotNull") {
        parts.push("<span class='field'>" + fieldLabel + "</span> " + opText);
      } else {
        var valueText = c.value != null ? escapeHtml(String(c.value)) : "";
        parts.push("<span class='field'>" + fieldLabel + "</span> " + Common.translate( opText )+ " <span class='value'>" + valueText + "</span>");
      }
    }

    return parts.join(" <span class='logical-and'>AND</span> ");
  };

  CtAccessRulesRulesTable.prototype.formatTargetsText = function (targets) {
    targets = Array.isArray(targets) ? targets : [];
    if (!targets.length) return "";

    return targets
      .map(function (t) {
        return (t.name || (t.type + ":" + t.id)) + " [" + (t.type || "") + "]";
      })
      .join(", ");
  };

  global.CtAccessRulesRulesTable = CtAccessRulesRulesTable;
})(window);
