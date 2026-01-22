import { icons } from "./icons.js";
import { setupPills } from "./pills.js";
import { getMetadataTreeLevel } from "../services/metadataTreeApi.js";
import { getActiveFileContentTypes } from "../services/fileContentTypesApi.js";
import { getFileContentTypeFields } from "../services/expertSearchApi.js";
import { getFileLocation } from "../services/filesApi.js"; // fallback only
import { escapeHtml } from "../utils/dom.js";

export function initMetadataTreeUI() {
  const root = document.getElementById("metadata-tree-explorer");
  if (!root) return;

  const treeContainer = root.querySelector("#treeContainer");
  const detailsPanel = root.querySelector("#detailsPanel");
  const globalStatus = root.querySelector("#globalStatus");
  const btnLoad = root.querySelector("#btnLoad");
  const fileContentTypeInput = root.querySelector("#fileContentTypeId");
  const btnToggleConfig = document.getElementById("btnToggleConfig");
  const orderedSection = root.querySelector(".mtc-ordered");
  const configSection = root.querySelector("#mtcConfigWrapper");

  const pills = setupPills(root);

  // Cache children per path (includes folders + files returned by API)
  const childrenCache = new Map();
  const pathKey = (arr) => (arr && arr.length ? arr.join("/") : "__root__");

  let lastSelected = null;

  // -------------------------------
  // ✅ Standard product: click handler for goToLocation icon
  // $(document).on("click", ".goToLocationRecentFile", function(){ eval($(this).attr("clickattr")) })
  // -------------------------------
  ensureStandardGoToLocationWiring();

  if (btnToggleConfig && (configSection || orderedSection)) {
    btnToggleConfig.addEventListener("click", () => {
      const target = configSection || orderedSection;
      const isHidden = target.hidden;
      target.hidden = !isHidden;
      btnToggleConfig.textContent = isHidden
        ? "Masquer la configuration"
        : "Afficher la configuration";
    });
  }

  btnLoad.addEventListener("click", loadRoot);
  fileContentTypeInput.addEventListener("change", loadAvailableFields);

  loadFileContentTypes();

  function select(node) {
    if (lastSelected) lastSelected.classList.remove("selected-node");
    node.classList.add("selected-node");
    lastSelected = node;
  }

  // Used only as a fallback display if Folder().goToLocation isn't available
  function extractFirstPath(rootNode) {
    if (!rootNode) return "/";

    const parts = [];
    let n = rootNode;

    while (n) {
      const t = n.text || n.label;
      if (t && t !== "/") parts.push(t);

      const ch = n.children;
      if (!ch || ch === false || ch.length === 0) break;
      n = ch[0];
    }

    return "/" + parts.join("/");
  }

  function renderFilesTable(files, path) {
    const location = "/" + (path || []).join("/");

    detailsPanel.innerHTML = `
      <div class="small text-muted mb-2">
        Dossier: ${escapeHtml(location)}
      </div>
      <table id="filesTable" class="table table-striped table-bordered" style="width:100%">
        <thead>
          <tr>
            <th>Nom</th>
            <th>Emplacement physique</th>
          </tr>
        </thead>
        <tbody></tbody>
      </table>
    `;

    // ✅ IMPORTANT: fileId is the id used by /Files/GetFileLocation?id=...
    const rows = (files || []).map((f) => {
      const name = String(f?.label ?? "");
      return {
        fileId: f?.fileId, // ✅ you said it's fileId
        name
      };
    });

    const hasJq = typeof window.$ !== "undefined";
    const hasDT = hasJq && window.$.fn && typeof window.$.fn.DataTable === "function";

    // Fallback if DataTables isn't available
    if (!hasDT) {
      const tbody = detailsPanel.querySelector("#filesTable tbody");

      if (!rows.length) {
        tbody.innerHTML = `<tr><td colspan="2" class="text-muted small">Aucun fichier.</td></tr>`;
        return;
      }

      tbody.innerHTML = rows
        .map((r) => {
          const fid = r.fileId == null ? "" : String(r.fileId);
          return `
            <tr>
              <td>${escapeHtml(r.name)}</td>
              <td>
                <i clickattr="goToLocation(${escapeHtml(fid)})"
                   title="Go to location"
                   class="fa fa-location-dot warning fa-lg pointer goToLocationRecentFile"
                   style="line-height: 20px;"></i>
              </td>
            </tr>
          `;
        })
        .join("");

      return;
    }

    const $table = window.$("#filesTable");
    if (window.$.fn.DataTable.isDataTable($table)) {
      $table.DataTable().destroy();
    }

    $table.DataTable({
      data: rows,
      columns: [
        { data: "name", title: "Nom" },
        {
          data: null,
          title: "Emplacement physique",
          orderable: false,
          searchable: false,
          width: "40px",
          render: function (_data, _type, row) {
            const fid = row.fileId == null ? "" : String(row.fileId);

            // ✅ EXACT standard product style: clickattr + classes
            return `
              <i clickattr="goToLocation(${fid})"
                 title="Go to location"
                 class="fa fa-location-dot warning fa-lg pointer goToLocationRecentFile"
                 style="line-height: 20px;"></i>
            `;
          }
        }
      ],
      pageLength: 10,
      lengthMenu: [10, 25, 50, 100],
      order: [[0, "asc"]],
      autoWidth: false,
      responsive: true
    });
  }

  async function loadRoot() {
    treeContainer.innerHTML = "<span class='text-muted small'>Chargement…</span>";
    detailsPanel.innerHTML = "<p class='text-muted small'>Aucun nœud sélectionné.</p>";
    lastSelected = null;

    const id = Number(fileContentTypeInput.value);
    const orderedFields = pills.getOrderedFieldsArray();

    if (!id || !orderedFields.length) {
      alert("Veuillez renseigner FileContentTypeId et au moins un champ ordonné.");
      treeContainer.innerHTML = "<span class='text-muted small'>Aucun résultat.</span>";
      return;
    }

    globalStatus.textContent = "Chargement...";

    try {
      const data = await getMetadataTreeLevel(id, orderedFields, []);
      childrenCache.set("__root__", data);

      const foldersOnly = (data || []).filter((n) => n && n.nodeType === 0);

      treeContainer.innerHTML = "";
      if (!foldersOnly.length) {
        treeContainer.innerHTML = "<span class='text-muted small'>Aucun dossier.</span>";
        globalStatus.textContent = "OK";
        return;
      }

      const ul = document.createElement("ul");
      ul.className = "tree-list";

      foldersOnly.forEach((node) => ul.appendChild(createFolderNode(node, [])));

      treeContainer.appendChild(ul);
      globalStatus.textContent = "OK";
    } catch (e) {
      console.error(e);
      globalStatus.textContent = "Erreur";
      treeContainer.innerHTML = "<span class='text-muted small'>Erreur lors du chargement.</span>";
      alert(e?.message || "Erreur lors du chargement.");
    }
  }

  function createFolderNode(node, path) {
    const li = document.createElement("li");

    const label = document.createElement("div");
    label.className = "tree-node-label";

    const arrow = document.createElement("span");
    arrow.className = "tree-icon";

    const icon = document.createElement("span");
    icon.className = "tree-icon";

    const text = document.createElement("span");
    text.textContent = node.label;

    const badge = document.createElement("span");
    badge.className = "badge tree-badge";
    badge.textContent = node.itemsCount;

    label.appendChild(arrow);
    label.appendChild(icon);
    label.appendChild(text);
    label.appendChild(badge);

    li.appendChild(label);

    const currentPath = (path || []).concat([node.label]);

    // Default look (expandable until proven leaf folder)
    arrow.innerHTML = icons.arrowRightSvg;
    icon.innerHTML = icons.folderClosedSvg;

    label.addEventListener("click", async () => {
      select(label);

      const id = Number(fileContentTypeInput.value);
      const orderedFields = pills.getOrderedFieldsArray();

      // Load children once
      if (!childrenCache.has(pathKey(currentPath))) {
        try {
          const children = await getMetadataTreeLevel(id, orderedFields, currentPath);
          childrenCache.set(pathKey(currentPath), children);
        } catch (e) {
          console.error(e);
          return;
        }
      }

      const children = childrenCache.get(pathKey(currentPath)) || [];
      const folders = children.filter((c) => c && c.nodeType === 0);
      const files = children.filter((c) => c && c.nodeType === 1);

      // ✅ Leaf folder (no subfolders) -> no arrow, no UL, show datatable
      if (folders.length === 0) {
        arrow.innerHTML = "";
        icon.innerHTML = icons.folderOpenSvg;
        renderFilesTable(files, currentPath);
        return;
      }

      // Expandable folder -> toggle UL with folders only
      toggleFolder(li, arrow, icon, folders, currentPath);
    });

    return li;
  }

  function toggleFolder(li, arrow, icon, folders, path) {
    let ul = li.querySelector("ul");

    if (ul) {
      const open = ul.style.display === "block";
      ul.style.display = open ? "none" : "block";
      arrow.innerHTML = open ? icons.arrowRightSvg : icons.arrowDownSvg;
      icon.innerHTML = open ? icons.folderClosedSvg : icons.folderOpenSvg;
      return;
    }

    ul = document.createElement("ul");
    ul.className = "tree-list";

    // Render folders ONLY (files are hidden from tree)
    (folders || []).forEach((f) => ul.appendChild(createFolderNode(f, path)));

    li.appendChild(ul);
    ul.style.display = "block";
    arrow.innerHTML = icons.arrowDownSvg;
    icon.innerHTML = icons.folderOpenSvg;
  }

  async function loadFileContentTypes() {
    globalStatus.textContent = "Chargement des types de contenu...";

    try {
      const types = await getActiveFileContentTypes();
      fileContentTypeInput.innerHTML = "";

      (types || []).forEach((t) => {
        const o = document.createElement("option");
        o.value = t.id;
        o.textContent = t.text;
        fileContentTypeInput.appendChild(o);
      });

      globalStatus.textContent = "Types de contenu chargés.";
      await loadAvailableFields();
    } catch (e) {
      console.error(e);
      globalStatus.textContent = "Erreur lors du chargement des types.";
    }
  }

  async function loadAvailableFields() {
    const id = Number(fileContentTypeInput.value);
    if (!id) return;

    globalStatus.textContent = "Chargement des champs...";

    try {
      const data = await getFileContentTypeFields(id);
      const fields = Array.isArray(data?.fields) ? data.fields.map((f) => f.key) : [];
      pills.setAvailableFields(fields);
      globalStatus.textContent = "Champs disponibles chargés.";
    } catch (e) {
      console.error(e);
      globalStatus.textContent = "Erreur lors du chargement des champs.";
    }
  }

  // -------------------------------
  // ✅ Standard product implementation
  // -------------------------------
  function ensureStandardGoToLocationWiring() {
    // If jQuery isn't there, we cannot reproduce standard eval(clickattr) behavior
    if (typeof window.$ === "undefined") return;

    // 1) delegated click handler (same as standard product)
    if (!window.__goToLocationDelegationInstalled) {
      window.__goToLocationDelegationInstalled = true;

      window.$(document).on("click", ".goToLocationRecentFile", function () {
        const s = window.$(this).attr("clickattr");
        const id = parseGoToLocationId(s);
        if (id != null && typeof window.goToLocation === "function") {
          window.goToLocation(id);
        }
      });
    }

    // 2) global goToLocation(id) function
    if (!window.goToLocation) {
      window.goToLocation = async function (id) {
        // Standard product path if available
        if (window.Common && typeof window.Common.ajaxGet === "function") {
          window.Common.ajaxGet(
            "/Files/GetFileLocation",
            { id: id },
            function (resp) {
              // mimic standard guards
              if (resp == null) {
                if (window.Common.alertMsg) {
                  window.Common.alertMsg(window.Resources?.FileDeleted || "File deleted");
                } else {
                  alert("File deleted");
                }
                return;
              }
              if (resp.id == null) {
                if (window.Common.alertMsg) {
                  window.Common.alertMsg(window.Resources?.NoPermission || "No permission");
                } else {
                  alert("No permission");
                }
                return;
              }

              // Standard navigation: new Folder().goToLocation(resp)
              if (window.Folder) {
                try {
                  new window.Folder().goToLocation(resp);
                  return;
                } catch (e) {
                  console.error(e);
                }
              }

              // Fallback if Folder isn't present
              console.warn("Folder navigation unavailable.", extractFirstPath(resp));
            },
            null,
            true
          );
          return;
        }

        // Fallback if Common.ajaxGet isn't available
        try {
          const resp = await getFileLocation(id);
          if (resp && resp.id != null && window.Folder) {
            try {
              new window.Folder().goToLocation(resp);
              return;
            } catch (e) {
              console.error(e);
            }
          }
          console.warn("Folder navigation unavailable.", extractFirstPath(resp));
        } catch (e) {
          console.error(e);
          alert(e?.message || "Erreur Go To Location");
        }
      };
    }
  }

  function parseGoToLocationId(value) {
    if (!value) return null;
    const match = String(value).match(/^\s*goToLocation\(([^)]+)\)\s*$/);
    if (!match) return null;
    const id = Number(String(match[1]).trim());
    return Number.isFinite(id) ? id : null;
  }
}
