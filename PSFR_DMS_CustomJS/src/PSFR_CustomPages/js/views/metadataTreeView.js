import { initMetadataTreeUI } from "../ui/tree.js";
import { ensureCssOnce } from "../utils/dom.js";

export function renderMetadataTreeView() {
  const container = document.querySelector(".content-wrapper");
  if (!container) return;

  // ensureCssOnce("/custom/metadata-tree.css", "metadata-tree-css");

  container.innerHTML = `
  <button id="btnToggleConfig" type="button" class="btn btn-primary pull-right">
  Masquer la configuration
</button>

   <h3 class="content-heading mtc-title">Metadata Tree Explorer</h3>



<div id="metadata-tree-explorer" class="mtc-root">
  <!-- Collapsable configuration zone (top part only) -->
  <div id="mtcConfigWrapper">
    <div class="panel panel-default">
      <div class="panel-body">
        <div class="mtc-top">
          <div class="mtc-ordered">
            <label class="control-label mtc-label"> Champs ordonnés </label>

            <div class="mtc-help">
              <div class="mtc-help-line">
                <span class="mtc-help-dot">•</span>
                Glissez-déposez pour réordonner
              </div>
              <div class="mtc-help-line">
                <span class="mtc-help-dot">•</span>
                Cliquez sur <strong>×</strong> pour retirer (puis ré-ajouter
                depuis “Champs disponibles”)
              </div>
            </div>

            <div class="ordered-fields-wrapper">
              <ul id="orderedFieldsList" class="mtc-ordered-list">
              </ul>

              <div class="mtc-available">
                <small class="text-muted">Champs disponibles (cliquez pour ré-ajouter)</small>
                <ul id="availableFieldsList"></ul>
              </div>
            </div>
          </div>

          <div class="mtc-controls">
            <label class="control-label">Type de contenu des fichiers</label>
            <select
              id="fileContentTypeId"
              class="form-control input-sm">
              <option value="">Chargement...</option>
            </select>

            <button id="btnLoad" class="btn btn-primary pull-right btnMargin" type="button">
              Charger l'arborescence
            </button>

            <div class="text-right mtc-status">
              <span id="globalStatus" class="text-muted small"></span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>

  <!-- Always visible -->
  <div class="mtc-main">
    <div class="mtc-col">
      <div class="panel panel-default">
        <div class="panel-heading"><strong>Arborescence</strong></div>
        <div class="panel-body tree-container" id="treeContainer">
          <span class="text-muted small">Cliquez sur "Charger l'arborescence"</span>
        </div>
      </div>
    </div>

    <div class="mtc-col">
      <div class="panel panel-default">
        <div class="panel-heading">
          <strong>Détails du nœud sélectionné</strong>
        </div>
        <div class="panel-body" id="detailsPanel">
          <p class="text-muted small">Aucun nœud sélectionné.</p>
        </div>
      </div>
    </div>
  </div>
</div>

  `;

  initMetadataTreeUI();
}
