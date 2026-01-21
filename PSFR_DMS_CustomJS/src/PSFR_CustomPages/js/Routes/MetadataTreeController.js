import { renderMetadataTreeView } from "../views/metadataTreeView.js";

export class MetadataTreeController {
  constructor() {
    this.routeName = "metadata-tree";
    this.hash = "#metadata-tree";
  }

  register(router) {
    router.route(this.routeName, "metadataTreeRoute", () => {
      this.render();
    });
  }

  render() {
    renderMetadataTreeView();
  }

  matchesHash(hash) {
    return (hash || "") === this.hash;
  }
}
