(function (global) {
  "use strict";

  function CtAccessRulesFormio() { }

  CtAccessRulesFormio.prototype.findComponentByKey = function (components, key) {
    var stack = Array.isArray(components) ? components.slice() : [];

    while (stack.length) {
      var c = stack.shift();
      if (!c) continue;

      if (c.key === key) return c;

      if (Array.isArray(c.components)) stack = stack.concat(c.components);

      if (Array.isArray(c.columns)) {
        for (var i = 0; i < c.columns.length; i++) {
          if (c.columns[i] && Array.isArray(c.columns[i].components)) {
            stack = stack.concat(c.columns[i].components);
          }
        }
      }

      if (Array.isArray(c.rows)) {
        for (var r = 0; r < c.rows.length; r++) {
          var row = c.rows[r];
          if (!Array.isArray(row)) continue;
          for (var j = 0; j < row.length; j++) {
            if (row[j] && Array.isArray(row[j].components)) {
              stack = stack.concat(row[j].components);
            }
          }
        }
      }

      if (c.type === "tabs" && Array.isArray(c.components)) {
        stack = stack.concat(c.components);
      }
    }

    return null;
  };

  CtAccessRulesFormio.prototype.tryParseComponents = function (formJson) {
    try {
      var parsed = JSON.parse(formJson || "{}");
      return Array.isArray(parsed.components) ? parsed.components : [];
    } catch (e) {
      return [];
    }
  };

  global.CtAccessRulesFormio = CtAccessRulesFormio;
})(window);

