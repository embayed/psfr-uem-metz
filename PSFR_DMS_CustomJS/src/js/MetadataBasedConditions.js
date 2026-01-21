// (function () {
//   "use strict";

//   var HASH = "#ct-access-rules";

//   // =========================
//   // APIs you already have
//   // =========================
//   var API_CONTENT_TYPES = "/FileContentTypes/ListActiveBasic";
//   var API_FIELDS = "/ExpertSearch/GetFileContentTypeFields"; // ?contentTypeId=1
//   var API_IDENTITY_SEARCH = function () { return window.IdentityUrl + "/api/SearchRolesGroupsUsersWithAttributes"; };

//   // =========================
//   // APIs YOU WILL CREATE
//   // =========================
//   // You can change these URLs later, keep them consistent backend-side.
//   var API_RULES_LIST = window.CustomDmsUrl + "/api/accessRules/List";         // GET ?contentTypeId=1
//   var API_RULES_CREATE = window.CustomDmsUrl + "/api/accessRules/Create";     // POST JSON
//   var API_RULES_DELETE = window.CustomDmsUrl + "/api/accessRules/Delete";     // DELETE ?id=123  (or POST if you prefer)

//   // =========================
//   // Helpers
//   // =========================
//   function isRtl() {
//     return window.language === "ar";
//   }

//   function t(key, fallback) {
//     if (window.Resources && window.Resources[key]) return window.Resources[key];
//     return fallback || key;
//   }

//   function escapeHtml(value) {
//     if (value == null) return "";
//     return String(value)
//       .replace(/&/g, "&amp;")
//       .replace(/</g, "&lt;")
//       .replace(/>/g, "&gt;")
//       .replace(/"/g, "&quot;")
//       .replace(/'/g, "&#039;");
//   }

//   function showError(msg) {
//     if (window.Common && Common.alertMsg) {
//       Common.alertMsg(msg);
//       return;
//     }
//     alert(msg);
//   }

//   function showSuccess(msg) {
//     if (window.Common && Common.showScreenSuccessMsg) {
//       Common.showScreenSuccessMsg(msg);
//       return;
//     }
//     alert(msg);
//   }

//   function ajaxGet(url, data, onOk, onFail) {
//     if (window.Common && Common.ajaxGet) {
//       Common.ajaxGet(url, data, onOk, onFail || function () { Common.showScreenErrorMsg(); });
//       return;
//     }

//     $.ajax({
//       url: url,
//       method: "GET",
//       data: data || {},
//       success: onOk,
//       error: onFail || function () { showError("Request failed"); }
//     });
//   }

//   function ajaxJson(method, url, body, onOk, onFail) {
//     $.ajax({
//       url: url,
//       method: method,
//       contentType: "application/json",
//       data: body ? JSON.stringify(body) : null,
//       success: onOk,
//       error: onFail || function () { showError("Request failed"); }
//     });
//   }

//   function ajaxDelete(url, onOk, onFail) {
//     $.ajax({
//       url: url,
//       method: "DELETE",
//       success: onOk,
//       error: onFail || function () { showError("Delete failed"); }
//     });
//   }

//   function parseTranslationMap(formTranslationJson) {
//     // formTranslation is array of { Keyword, En, Ar, Fr, Type? }
//     var map = {};
//     try {
//       var arr = JSON.parse(formTranslationJson || "[]");
//       for (var i = 0; i < arr.length; i++) {
//         if (!arr[i] || !arr[i].Keyword) continue;
//         map[arr[i].Keyword] = arr[i];
//       }
//     } catch (e) {
//       // ignore
//     }
//     return map;
//   }

//   function findFormComponentByKey(components, key) {
//     // Deep scan Form.io components to find matching component.key
//     var stack = Array.isArray(components) ? components.slice() : [];
//     while (stack.length) {
//       var c = stack.shift();
//       if (!c) continue;
//       if (c.key === key) return c;
//       if (Array.isArray(c.components)) stack = stack.concat(c.components);
//       if (Array.isArray(c.columns)) {
//         for (var i = 0; i < c.columns.length; i++) {
//           if (c.columns[i] && Array.isArray(c.columns[i].components)) stack = stack.concat(c.columns[i].components);
//         }
//       }
//       if (Array.isArray(c.rows)) {
//         for (var r = 0; r < c.rows.length; r++) {
//           var row = c.rows[r];
//           if (!Array.isArray(row)) continue;
//           for (var j = 0; j < row.length; j++) {
//             if (row[j] && Array.isArray(row[j].components)) stack = stack.concat(row[j].components);
//           }
//         }
//       }
//       if (c.type === "tabs" && Array.isArray(c.components)) stack = stack.concat(c.components);
//     }
//     return null;
//   }

//   function buildFieldLabel(component, translationMap) {
//     var label = component && component.label ? component.label : (component && component.key ? component.key : "");
//     if (!label) return "";

//     var tr = translationMap[label];
//     if (!tr) return label;

//     if (window.language === "fr" && tr.Fr) return tr.Fr;
//     if (window.language === "ar" && tr.Ar) return tr.Ar;
//     if (window.language === "en" && tr.En) return tr.En;

//     return label;
//   }

//   function buildPageHtml() {
//     return ""
//       + "<div class='ct-access-rules-page'>"
//       + "  <div class='panel panel-default'>"
//       + "    <div class='panel-heading'>"
//       + "      <h3 class='panel-title'>" + escapeHtml(t("AccessRules", "Content Type Access Rules")) + "</h3>"
//       + "    </div>"
//       + "    <div class='panel-body'>"
//       + "      <div class='row'>"
//       + "        <div class='col-md-6 col-xs-12'>"
//       + "          <label>" + escapeHtml(t("ContentType", "Content Type")) + "</label>"
//       + "          <div id='ctContainer'>"
//       + "            <select id='cmbContentTypeRules' class='form-control'></select>"
//       + "          </div>"
//       + "        </div>"
//       + "        <div class='col-md-6 col-xs-12'>"
//       + "          <label>" + escapeHtml(t("Targets", "Users / Groups / Roles")) + "</label>"
//       + "          <div id='targetsContainer'>"
//       + "            <select id='cmbTargets' class='form-control' multiple='multiple'></select>"
//       + "          </div>"
//       + "        </div>"
//       + "      </div>"
//       + "      <hr/>"

//       + "      <div class='row'>"
//       + "        <div class='col-md-12'>"
//       + "          <div style='display:flex; justify-content:space-between; align-items:center;'>"
//       + "            <h4 style='margin:0;'>" + escapeHtml(t("Conditions", "Conditions")) + "</h4>"
//       + "            <button type='button' id='btnAddCondition' class='btn btn-xs btn-warning'>"
//       + "              <i class='fa fa-plus'></i> " + escapeHtml(t("AddCondition", "Add condition")) + ""
//       + "            </button>"
//       + "          </div>"
//       + "          <div id='conditionsContainer' style='margin-top:10px;'></div>"
//       + "        </div>"
//       + "      </div>"

//       + "      <hr/>"

//       + "      <div class='row'>"
//       + "        <div class='col-md-12'>"
//       + "          <button type='button' id='btnSaveRule' class='btn btn-primary'>"
//       + "            <i class='fa fa-save'></i> " + escapeHtml(t("Save", "Save")) + ""
//       + "          </button>"
//       + "          <button type='button' id='btnResetRule' class='btn btn-default'>"
//       + "            <i class='fa fa-undo'></i> " + escapeHtml(t("Clear", "Clear")) + ""
//       + "          </button>"
//       + "        </div>"
//       + "      </div>"

//       + "      <hr/>"

//       + "      <h4 style='margin-top:0;'>" + escapeHtml(t("SavedRules", "Saved rules")) + "</h4>"
//       + "      <div class='table-responsive'>"
//       + "        <table class='table table-bordered table-striped'>"
//       + "          <thead>"
//       + "            <tr>"
//       + "              <th style='width:120px;'>" + escapeHtml(t("ContentType", "Content Type")) + "</th>"
//       + "              <th>" + escapeHtml(t("Conditions", "Conditions")) + "</th>"
//       + "              <th style='width:260px;'>" + escapeHtml(t("Targets", "Targets")) + "</th>"
//       + "              <th style='width:90px;'>" + escapeHtml(t("Actions", "Actions")) + "</th>"
//       + "            </tr>"
//       + "          </thead>"
//       + "          <tbody id='rulesTableBody'>"
//       + "            <tr><td colspan='4'>" + escapeHtml(t("Loading", "Loading...")) + "</td></tr>"
//       + "          </tbody>"
//       + "        </table>"
//       + "      </div>"

//       + "    </div>"
//       + "  </div>"
//       + "</div>";
//   }

//   // =========================
//   // State
//   // =========================
//   var state = {
//     contentTypes: [],
//     fields: [],            // [{ key, label }]
//     contentTypeId: null,
//     contentTypeText: "",
//     translationMap: {},
//     formComponents: [],
//     conditionCounter: 0
//   };

//   // =========================
//   // UI Render
//   // =========================
//   function renderIntoContentWrapper() {
//     var container = document.querySelector(".content-wrapper");
//     if (!container) return;

//     container.innerHTML = "<div id='ctAccessRulesHost'></div>";
//     $("#ctAccessRulesHost").html(buildPageHtml());

//     initContentTypeDropdown();
//     initTargetsSelector();
//     wireButtons();

//     loadContentTypes();
//   }

//   function initContentTypeDropdown() {
//     if (!$.fn.select2) return;

//     $("#cmbContentTypeRules").select2({
//       dir: isRtl() ? "rtl" : "ltr",
//       language: window.language,
//       width: "100%",
//       allowClear: true,
//       placeholder: t("SelectContentType", "Select content type"),
//       dropdownParent: $("#ctContainer")
//     });

//     $("#cmbContentTypeRules").on("change", function () {
//       var selected = $("#cmbContentTypeRules").select2("data")[0];
//       state.contentTypeId = selected ? selected.id : null;
//       state.contentTypeText = selected ? selected.text : "";

//       resetConditions();
//       state.fields = [];

//       if (!state.contentTypeId) {
//         $("#conditionsContainer").html("");
//         renderRulesTable([]);
//         return;
//       }

//       loadFieldsByContentTypeId(state.contentTypeId);
//       loadSavedRules(state.contentTypeId);
//     });
//   }

//   function initTargetsSelector() {
//     if (!$.fn.select2) return;

//     $("#cmbTargets").select2({
//       dir: isRtl() ? "rtl" : "ltr",
//       language: window.language,
//       width: "100%",
//       multiple: true,
//       allowClear: true,
//       placeholder: t("SearchTargets", "Search users / groups / roles"),
//       dropdownParent: $("#targetsContainer"),
//       minimumInputLength: 0,
//       ajax: {
//         delay: 250,
//         url: API_IDENTITY_SEARCH(),
//         type: "POST",
//         dataType: "json",
//         headers: {
//           Authorization: "Bearer " + (window.IdentityAccessToken || "")
//         },
//         data: function (params) {
//           // form-urlencoded style parameters (select2 will encode object)
//           var attrs = [];
//           if (window.language === "fr") {
//             attrs.push("FirstNameFr");
//             attrs.push("LastNameFr");
//           } else if (window.language === "ar") {
//             attrs.push("FirstNameAr");
//             attrs.push("LastNameAr");
//           } else {
//             attrs.push("FirstNameFr");
//             attrs.push("LastNameFr");
//           }

//           return {
//             text: params.term || "",
//             language: window.language || "fr",
//             showOnlyActiveUsers: true,
//             "attributes[]": attrs
//           };
//         },
//         processResults: function (items) {
//           items = items || [];
//           return {
//             results: items.map(function (x) {
//               return {
//                 id: x.type + ":" + x.id,
//                 text: (x.name || "") + " (" + (x.type || "") + ")",
//                 raw: x
//               };
//             })
//           };
//         }
//       }
//     });
//   }

//   function wireButtons() {
//     $("#btnAddCondition").on("click", function () {
//       if (!state.contentTypeId) {
//         showError(t("SelectContentTypeFirst", "Please select a content type first."));
//         return;
//       }
//       addConditionRow();
//     });

//     $("#btnResetRule").on("click", function () {
//       resetConditions();
//       $("#cmbTargets").val(null).trigger("change");
//     });

//     $("#btnSaveRule").on("click", function () {
//       saveRule();
//     });

//     $("#conditionsContainer").on("click", ".btn-delete-condition", function () {
//       $(this).closest(".condition-row").remove();
//     });

//     $("#rulesTableBody").on("click", ".btn-delete-rule", function () {
//       var id = $(this).attr("data-id");
//       if (!id) return;
//       deleteRule(id);
//     });
//   }

//   // =========================
//   // Data Loading
//   // =========================
//   function loadContentTypes() {
//     ajaxGet(API_CONTENT_TYPES, null, function (items) {
//       state.contentTypes = Array.isArray(items) ? items : [];

//       var data = state.contentTypes.map(function (x) {
//         return { id: x.id, text: x.text };
//       });

//       $("#cmbContentTypeRules").empty();

//       $("#cmbContentTypeRules").select2({
//         data: data,
//         dir: isRtl() ? "rtl" : "ltr",
//         language: window.language,
//         width: "100%",
//         allowClear: true,
//         placeholder: t("SelectContentType", "Select content type"),
//         dropdownParent: $("#ctContainer")
//       });

//       $("#cmbContentTypeRules").val(null).trigger("change");
//       renderRulesTable([]);
//       $("#conditionsContainer").html("");
//     });
//   }

//   function loadFieldsByContentTypeId(contentTypeId) {
//     ajaxGet(API_FIELDS, { contentTypeId: contentTypeId }, function (resp) {
//       // resp: { fields:[{key,type}], form:"{...}", formTranslation:"[...]"}
//       var fields = Array.isArray(resp && resp.fields) ? resp.fields : [];
//       var formJson = resp && resp.form ? resp.form : null;
//       var formTranslation = resp && resp.formTranslation ? resp.formTranslation : "[]";

//       state.translationMap = parseTranslationMap(formTranslation);

//       state.formComponents = [];
//       try {
//         var parsedForm = JSON.parse(formJson || "{}");
//         state.formComponents = Array.isArray(parsedForm.components) ? parsedForm.components : [];
//       } catch (e) {
//         state.formComponents = [];
//       }

//       // Build display fields list using Form.io component label + translation.
//       state.fields = fields.map(function (f) {
//         var comp = findFormComponentByKey(state.formComponents, f.key);
//         var label = comp ? buildFieldLabel(comp, state.translationMap) : f.key;
//         return { key: f.key, label: label, type: f.type };
//       });

//       resetConditions();
//       addConditionRow(); // start with one row
//     });
//   }

//   // =========================
//   // Conditions Builder
//   // =========================
//   function resetConditions() {
//     state.conditionCounter = 0;
//     $("#conditionsContainer").html("");
//   }

//   function addConditionRow(existing) {
//     state.conditionCounter += 1;
//     var id = state.conditionCounter;

//     var html =
//       ""
//       + "<div class='condition-row row' data-row-id='" + id + "' style='margin-bottom:8px;'>"
//       + "  <div class='col-md-4 col-xs-12'>"
//       + "    <label style='display:none;'>" + escapeHtml(t("Field", "Field")) + "</label>"
//       + "    <div id='fieldCon_" + id + "'>"
//       + "      <select id='cmbField_" + id + "' class='form-control'></select>"
//       + "    </div>"
//       + "  </div>"
//       + "  <div class='col-md-2 col-xs-12'>"
//       + "    <div id='opCon_" + id + "'>"
//       + "      <select id='cmbOp_" + id + "' class='form-control'>"
//       + "        <option value='Equal'>==</option>"
//       + "        <option value='NotEqual'>!=</option>"
//       + "        <option value='Contains'>" + escapeHtml(t("Contains", "Contains")) + "</option>"
//       + "        <option value='StartWith'>" + escapeHtml(t("StartWith", "Starts with")) + "</option>"
//       + "        <option value='IsNull'>" + escapeHtml(t("IsEmpty", "Is empty")) + "</option>"
//       + "        <option value='IsNotNull'>" + escapeHtml(t("IsNotEmpty", "Is not empty")) + "</option>"
//       + "      </select>"
//       + "    </div>"
//       + "  </div>"
//       + "  <div class='col-md-5 col-xs-12'>"
//       + "    <input id='txtVal_" + id + "' type='text' class='form-control' placeholder='" + escapeHtml(t("Value", "Value")) + "'/>"
//       + "  </div>"
//       + "  <div class='col-md-1 col-xs-12' style='display:flex; align-items:center; gap:6px;'>"
//       + "    <button type='button' class='btn btn-xs btn-danger btn-delete-condition' title='Delete'>"
//       + "      <i class='fa fa-trash'></i>"
//       + "    </button>"
//       + "  </div>"
//       + "</div>";

//     $("#conditionsContainer").append(html);

//     var fieldData = state.fields.map(function (f) {
//       return { id: f.key, text: f.label };
//     });

//     if ($.fn.select2) {
//       $("#cmbField_" + id).select2({
//         dir: isRtl() ? "rtl" : "ltr",
//         language: window.language,
//         width: "100%",
//         allowClear: true,
//         placeholder: t("SelectField", "Select field"),
//         data: fieldData,
//         dropdownParent: $("#fieldCon_" + id)
//       });
//     } else {
//       var $cmb = $("#cmbField_" + id);
//       $cmb.append("<option value=''></option>");
//       for (var i = 0; i < fieldData.length; i++) {
//         $cmb.append("<option value='" + escapeHtml(fieldData[i].id) + "'>" + escapeHtml(fieldData[i].text) + "</option>");
//       }
//     }

//     if (existing) {
//       $("#cmbField_" + id).val(existing.fieldKey).trigger("change");
//       $("#cmbOp_" + id).val(existing.op);
//       $("#txtVal_" + id).val(existing.value);
//     }

//     $("#cmbOp_" + id).on("change", function () {
//       var v = $(this).val();
//       if (v === "IsNull" || v === "IsNotNull") {
//         $("#txtVal_" + id).val("").prop("disabled", true);
//       } else {
//         $("#txtVal_" + id).prop("disabled", false);
//       }
//     }).trigger("change");
//   }

//   function buildConditionsPayload() {
//     var conditions = [];
//     $("#conditionsContainer .condition-row").each(function () {
//       var rowId = $(this).attr("data-row-id");
//       var fieldKey = $("#cmbField_" + rowId).val();
//       var op = $("#cmbOp_" + rowId).val();
//       var value = $("#txtVal_" + rowId).val();

//       if (!fieldKey) return;

//       if ((op === "IsNull" || op === "IsNotNull")) {
//         conditions.push({ fieldKey: fieldKey, op: op, value: "" });
//         return;
//       }

//       if (!value || String(value).trim() === "") return;

//       conditions.push({ fieldKey: fieldKey, op: op, value: value });
//     });

//     return conditions;
//   }

//   function buildTargetsPayload() {
//     var selected = $("#cmbTargets").select2 ? $("#cmbTargets").select2("data") : [];
//     selected = Array.isArray(selected) ? selected : [];

//     // store as normalized objects: { type:'User'|'Group'|'Role', id: 1, name:'...' }
//     var targets = [];
//     for (var i = 0; i < selected.length; i++) {
//       var raw = selected[i].raw;
//       if (raw && raw.type && raw.id != null) {
//         targets.push({ type: raw.type, id: raw.id, name: raw.name || selected[i].text });
//       } else {
//         // fallback if raw missing
//         var parts = String(selected[i].id || "").split(":");
//         if (parts.length === 2) {
//           targets.push({ type: parts[0], id: Number(parts[1]), name: selected[i].text });
//         }
//       }
//     }
//     return targets;
//   }

//   // =========================
//   // Save / List / Delete Rules
//   // =========================
//   function saveRule() {
//     if (!state.contentTypeId) {
//       showError(t("SelectContentTypeFirst", "Please select a content type first."));
//       return;
//     }

//     var conditions = buildConditionsPayload();
//     if (!conditions.length) {
//       showError(t("AddAtLeastOneCondition", "Please add at least one valid condition."));
//       return;
//     }

//     var targets = buildTargetsPayload();
//     if (!targets.length) {
//       showError(t("SelectAtLeastOneTarget", "Please select at least one user/group/role."));
//       return;
//     }

//     var body = {
//       contentTypeId: Number(state.contentTypeId),
//       contentTypeText: state.contentTypeText,
//       conditions: conditions,
//       targets: targets
//     };

//     ajaxJson("POST", API_RULES_CREATE, body, function () {
//       showSuccess(t("Saved", "Saved successfully"));
//       resetConditions();
//       $("#cmbTargets").val(null).trigger("change");
//       loadSavedRules(state.contentTypeId);
//     });
//   }

//   function loadSavedRules(contentTypeId) {
//     $("#rulesTableBody").html("<tr><td colspan='4'>" + escapeHtml(t("Loading", "Loading...")) + "</td></tr>");

//     ajaxGet(API_RULES_LIST, { contentTypeId: contentTypeId }, function (items) {
//       items = Array.isArray(items) ? items : [];
//       renderRulesTable(items);
//     }, function () {
//       renderRulesTable([]);
//       showError("Failed to load saved rules.");
//     });
//   }

//   function deleteRule(id) {
//     // if you prefer POST delete, change this to ajaxJson("POST", ...)
//     ajaxDelete(API_RULES_DELETE + "?id=" + encodeURIComponent(id), function () {
//       showSuccess(t("Deleted", "Deleted"));
//       loadSavedRules(state.contentTypeId);
//     });
//   }

//   function renderRulesTable(items) {
//     if (!items || !items.length) {
//       $("#rulesTableBody").html("<tr><td colspan='4'>" + escapeHtml(t("NoData", "No data")) + "</td></tr>");
//       return;
//     }

//     var rows = "";
//     for (var i = 0; i < items.length; i++) {
//       var rule = items[i];

//       var ct = rule.contentTypeText || state.contentTypeText || "";
//       var conditionsText = formatConditionsText(rule.conditions);
//       var targetsText = formatTargetsText(rule.targets);

//       rows += ""
//         + "<tr>"
//         + "  <td>" + escapeHtml(ct) + "</td>"
//         + "  <td>" + escapeHtml(conditionsText) + "</td>"
//         + "  <td>" + escapeHtml(targetsText) + "</td>"
//         + "  <td>"
//         + "    <button type='button' class='btn btn-xs btn-danger btn-delete-rule' data-id='" + escapeHtml(rule.id) + "'>"
//         + "      <i class='fa fa-trash'></i>"
//         + "    </button>"
//         + "  </td>"
//         + "</tr>";
//     }

//     $("#rulesTableBody").html(rows);
//   }

//   function formatConditionsText(conditions) {
//     conditions = Array.isArray(conditions) ? conditions : [];
//     if (!conditions.length) return "";

//     var parts = [];
//     for (var i = 0; i < conditions.length; i++) {
//       var c = conditions[i];
//       var fieldLabel = getFieldLabel(c.fieldKey);
//       var op = c.op || "Equal";
//       var opText = op === "Equal" ? "==" :
//         op === "NotEqual" ? "!=" :
//           op === "Contains" ? "CONTAINS" :
//             op === "StartWith" ? "STARTS_WITH" :
//               op === "IsNull" ? "IS_EMPTY" :
//                 op === "IsNotNull" ? "IS_NOT_EMPTY" :
//                   op;

//       if (op === "IsNull" || op === "IsNotNull") {
//         parts.push(fieldLabel + " " + opText);
//       } else {
//         parts.push(fieldLabel + " " + opText + " " + (c.value != null ? c.value : ""));
//       }
//     }

//     // You said: metadata1 value1 AND metadata2 value2
//     return parts.join(" AND ");
//   }

//   function getFieldLabel(fieldKey) {
//     var f = state.fields.filter(function (x) { return x.key === fieldKey; })[0];
//     return f ? f.label : fieldKey;
//   }

//   function formatTargetsText(targets) {
//     targets = Array.isArray(targets) ? targets : [];
//     if (!targets.length) return "";

//     return targets.map(function (t) {
//       return (t.name || (t.type + ":" + t.id)) + " [" + (t.type || "") + "]";
//     }).join(", ");
//   }

//   // =========================
//   // Hash Routing
//   // =========================
//   function onHashChange() {
//     if ((window.location.hash || "") === HASH) {
//       renderIntoContentWrapper();
//     }
//   }

//   onHashChange();
//   window.addEventListener("hashchange", onHashChange);

// })();
