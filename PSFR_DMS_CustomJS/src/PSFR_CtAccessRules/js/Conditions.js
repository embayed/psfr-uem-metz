(function (global) {
  "use strict";

  function CtAccessRulesConditions(helpers, state) {
    this.helpers = helpers;
    this.state = state;
  }

  CtAccessRulesConditions.prototype.reset = function () {
    this.state.resetConditions();
    $("#conditionsContainer").html("");
  };

  CtAccessRulesConditions.prototype.addRow = function (existing) {
    this.state.conditionCounter += 1;

    var id = this.state.conditionCounter;
    var escapeHtml = this.helpers.escapeHtml.bind(this.helpers);
    var t = this.helpers.t.bind(this.helpers);
    var isRtl = this.helpers.isRtl.bind(this.helpers);

    var html = ""
      + "<div class='condition-row row' data-row-id='" + id + "' style='margin-bottom:8px;'>"
      + " <div class='col-md-4 col-xs-12'>"
      + " <label style='display:none;'>" + escapeHtml(t("Field", "Field")) + "</label>"
      + " <div id='fieldCon_" + id + "'>"
      + " <select id='cmbField_" + id + "' class='form-control'></select>"
      + " </div>"
      + " </div>"
      + " <div class='col-md-2 col-xs-12'>"
      + " <div id='opCon_" + id + "'>"
      + " <select id='cmbOp_" + id + "' class='form-control'>"
      + " <option value='Equal'>==</option>"
      + " <option value='NotEqual'>!=</option>"
      + " <option value='Contains'>" + escapeHtml(t("Contains", "Contains")) + "</option>"
      + " <option value='StartWith'>" + escapeHtml(t("StartWith", "Starts with")) + "</option>"
      + " <option value='IsNull'>" + escapeHtml(t("IsEmpty", "Is empty")) + "</option>"
      + " <option value='IsNotNull'>" + escapeHtml(t("IsNotEmpty", "Is not empty")) + "</option>"
      + " </select>"
      + " </div>"
      + " </div>"
      + " <div class='col-md-5 col-xs-12'>"
      + " <input id='txtVal_" + id + "' type='text' class='form-control' placeholder='" + escapeHtml(t("Value", "Value")) + "'/>"
      + " </div>"
      + " <div class='col-md-1 col-xs-12' style='display:flex; align-items:center; gap:6px;'>"
      + " <button type='button' class='btn btn-xs btn-danger btn-delete-condition' title='Delete'>"
      + " <i class='fa fa-trash'></i>"
      + " </button>"
      + " </div>"
      + "</div>";

    $("#conditionsContainer").append(html);

    var fieldData = this.state.fields.map(function (f) {
      return { id: f.key, text: f.label };
    });

    if ($.fn.select2) {
  $("#cmbField_" + id).select2({
    dir: isRtl() ? "rtl" : "ltr",
    language: global.language,
    width: "100%",
    allowClear: true,
    placeholder: t("SelectField", "Select field"),
    data: fieldData,
    dropdownParent: $("#fieldCon_" + id)
  });

  $("#cmbOp_" + id).select2({
    dir: isRtl() ? "rtl" : "ltr",
    language: global.language,
    width: "100%",
    minimumResultsForSearch: Infinity,
    dropdownParent: $("#opCon_" + id)
  });
} else {

      var $cmb = $("#cmbField_" + id);
      $cmb.append("<option value=''></option>");
      for (var i = 0; i < fieldData.length; i++) {
        $cmb.append("<option value='" + escapeHtml(fieldData[i].id) + "'>" + escapeHtml(fieldData[i].text) + "</option>");
      }
    }

    if (existing) {
      $("#cmbField_" + id).val(existing.fieldKey).trigger("change");
      $("#cmbOp_" + id).val(existing.op);
      $("#txtVal_" + id).val(existing.value);
    }

    $("#cmbOp_" + id).on("change", function () {
      var v = $(this).val();
      if (v === "IsNull" || v === "IsNotNull") {
        $("#txtVal_" + id).val("").prop("disabled", true);
      } else {
        $("#txtVal_" + id).prop("disabled", false);
      }
    }).trigger("change");
  };

  CtAccessRulesConditions.prototype.buildPayload = function () {
    var conditions = [];
    $("#conditionsContainer .condition-row").each(function () {
      var rowId = $(this).attr("data-row-id");

      var fieldKey = $("#cmbField_" + rowId).val();
      var op = $("#cmbOp_" + rowId).val();
      var value = $("#txtVal_" + rowId).val();

      if (!fieldKey) return;

      if (op === "IsNull" || op === "IsNotNull") {
        conditions.push({ fieldKey: fieldKey, op: op, value: "" });
        return;
      }

      if (!value || String(value).trim() === "") return;

      conditions.push({ fieldKey: fieldKey, op: op, value: value });
    });

    return conditions;
  };

  global.CtAccessRulesConditions = CtAccessRulesConditions;
})(window);
