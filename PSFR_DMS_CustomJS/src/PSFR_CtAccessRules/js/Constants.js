(function (global) {
  "use strict";

  global.CtAccessRulesConstants = {
    HASH: "#ct-access-rules",

    // APIs you already have
    API_CONTENT_TYPES: "/FileContentTypes/ListActiveBasic",
    API_FIELDS: "/ExpertSearch/GetFileContentTypeFields",

    API_IDENTITY_SEARCH: function () {
      return global.IdentityUrl + "/api/SearchRolesGroupsUsersWithAttributes";
    },

    // APIs YOU WILL CREATE
    API_RULES_LIST: global.CustomDmsUrl + "/api/accessRules/List",
    API_RULES_CREATE: global.CustomDmsUrl + "/api/accessRules/Create",
    API_RULES_DELETE: global.CustomDmsUrl + "/api/accessRules/Delete"
  };
})(window);
