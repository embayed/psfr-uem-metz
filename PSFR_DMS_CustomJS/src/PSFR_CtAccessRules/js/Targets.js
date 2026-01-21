(function (global) {
  "use strict";

  function CtAccessRulesTargets(helpers, constants) {
    this.helpers = helpers;
    this.constants = constants;
  }

  CtAccessRulesTargets.prototype.initSelect2 = function () {
    if (!$.fn.select2) return;

    var isRtl = this.helpers.isRtl.bind(this.helpers);
    var t = this.helpers.t.bind(this.helpers);
    $("#cmbTargets").select2({
      theme: "bootstrap",
      dir: isRtl() ? "rtl" : "ltr",
      language: global.language,
      width: "100%",
      multiple: true,
      allowClear: true,
      placeholder: t("SearchTargets", "Search users / groups / roles"),
      dropdownParent: $("#targetsContainer"),
      minimumInputLength: 0,

      templateResult: function (item) {
        if (!item || !item.id) return item.text;

        var raw = item.raw || {};
        var type = (raw.type || "").toLowerCase();

        var icon = "fa-user";
        if (type === "group") icon = "fa-users";
        if (type === "role") icon = "fa-id-badge";

        return $(
          "<span><i class='fa " +
            icon +
            "' style='margin-right:6px;'></i>" +
            item.text +
            "</span>",
        );
      },

      templateSelection: function (item) {
        if (!item || !item.id) return item.text;

        var raw = item.raw || {};
        var type = (raw.type || "").toLowerCase();

        var icon = "fa-user";
        if (type === "group") icon = "fa-users";
        if (type === "role") icon = "fa-id-badge";

        return $(
          "<span><i class='fa " +
            icon +
            "' style='margin-right:6px;'></i>" +
            item.text +
            "</span>",
        );
      },

      ajax: {
        delay: 250,
        url: this.constants.API_IDENTITY_SEARCH(),
        type: "POST",
        dataType: "json",
        headers: {
          Authorization: "Bearer " + (global.IdentityAccessToken || ""),
        },
        data: function (params) {
          var attrs = [];

          if (global.language === "fr") {
            attrs.push("FirstNameFr");
            attrs.push("LastNameFr");
          } else if (global.language === "ar") {
            attrs.push("FirstNameAr");
            attrs.push("LastNameAr");
          } else {
            attrs.push("FirstNameFr");
            attrs.push("LastNameFr");
          }

          return {
            text: params.term || "",
            language: global.language || "fr",
            showOnlyActiveUsers: true,
            "attributes[]": attrs,
          };
        },
        processResults: function (items) {
          items = items || [];
          return {
            results: items.map(function (x) {
              return {
                id: x.type + ":" + x.id,
                text: x.name || "",
                raw: x,
              };
            }),
          };
        },
      },
    });
  };

  CtAccessRulesTargets.prototype.buildPayload = function () {
    var selected = $("#cmbTargets").select2
      ? $("#cmbTargets").select2("data")
      : [];
    selected = Array.isArray(selected) ? selected : [];

    var targets = [];
    for (var i = 0; i < selected.length; i++) {
      var raw = selected[i].raw;

      if (raw && raw.type && raw.id != null) {
        targets.push({
          type: raw.type,
          id: raw.id,
          name: raw.name || selected[i].text,
        });
        continue;
      }

      var parts = String(selected[i].id || "").split(":");
      if (parts.length === 2) {
        targets.push({
          type: parts[0],
          id: Number(parts[1]),
          name: selected[i].text,
        });
      }
    }

    return targets;
  };

  global.CtAccessRulesTargets = CtAccessRulesTargets;
})(window);
