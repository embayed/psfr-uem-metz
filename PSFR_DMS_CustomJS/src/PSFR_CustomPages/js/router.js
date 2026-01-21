import { MetadataTreeController } from "./Routes/MetadataTreeController.js";
import { ExternalIframeController } from "./Routes/ExternalIframeController.js";

export function registerRoutes() {
  $(document).ready(function () {
    const controllers = [
      new MetadataTreeController(),
      new ExternalIframeController()
    ];

    if (window.DMSComponents && DMSComponents.Router && window.Backbone && Backbone.history) {
      controllers.forEach((c) => c.register(DMSComponents.Router));

      Backbone.history.stop();
      Backbone.history.start();
    } else {
      const renderByHash = () => {
        const hash = window.location.hash || "";
        const controller = controllers.find((c) => c.matchesHash(hash));
        if (controller) controller.render();
      };

      renderByHash();
      window.addEventListener("hashchange", renderByHash);
    }

    $(window).on("hashchange", handleMenuChange);
    setTimeout(handleMenuChange, 0);
  });
}

function handleMenuChange() {
  const currentMenu = window.location.hash || "#";
  $(".sidebar li.active").removeClass("active");
  $(`a[href='${currentMenu}']`).parent().addClass("active");
}
