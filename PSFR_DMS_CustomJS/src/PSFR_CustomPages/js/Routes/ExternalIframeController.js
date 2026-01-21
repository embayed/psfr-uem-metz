import { renderIframeView } from "../views/iframeView.js";

export class ExternalIframeController {
  constructor() {
    this.routeName = "external-view";
    this.hash = "#external-view";
    this.iframeUrl = `https://prjw-uemmetz.intalio-fr.com:3030?lang=${window.language}`;
  }

  register(router) {
    router.route(this.routeName, "externalViewRoute", () => {
      this.render();
    });
  }

  render() {
    renderIframeView(this.iframeUrl);
  }

  matchesHash(hash) {
    return (hash || "") === this.hash;
  }
}
