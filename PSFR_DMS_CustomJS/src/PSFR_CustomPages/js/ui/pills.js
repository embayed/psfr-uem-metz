import { wireDragForPill, enableVerticalReorder } from "./dragDrop.js";
import { escapeHtml } from "../utils/dom.js";

export function setupPills(root) {
  const orderedFieldsList = root.querySelector("#orderedFieldsList");
  const availableFieldsList = root.querySelector("#availableFieldsList");
  const newFieldInput = root.querySelector("#newFieldInput");
  const btnAddField = root.querySelector("#btnAddField");

  enableVerticalReorder(orderedFieldsList);

  wireExisting(orderedFieldsList);
  attachRemoveHandlers(orderedFieldsList, availableFieldsList);

  if (btnAddField && newFieldInput) {
    btnAddField.addEventListener("click", function () {
      const value = (newFieldInput.value || "").trim();
      if (!value) return;

      const li = document.createElement("li");
      li.className = "ordered-field-pill";
      li.setAttribute("draggable", "true");
      li.setAttribute("data-field", value);
      li.innerHTML = "<span class='pill-handle'>⋮⋮</span><span>" + escapeHtml(value) + "</span><span class='pill-remove'>×</span>";

      orderedFieldsList.appendChild(li);
      wireDragForPill(li);
      attachRemoveHandlers(orderedFieldsList, availableFieldsList);
      newFieldInput.value = "";
    });
  }

  return {
    getOrderedFieldsArray: function () {
      const nodes = orderedFieldsList.querySelectorAll(".ordered-field-pill");
      return Array.prototype.map.call(nodes, function (p) { return p.getAttribute("data-field"); });
    },
    setAvailableFields: function (fields) {
      availableFieldsList.innerHTML = "";
      const orderedFields = new Set(this.getOrderedFieldsArray());

      fields.forEach(function (field) {
        if (!field || orderedFields.has(field)) return;
        const li = buildAvailablePill(field, orderedFieldsList, availableFieldsList);
        availableFieldsList.appendChild(li);
      });
    },
    clearOrderedFields: function () {
      orderedFieldsList.innerHTML = "";
    }
  };
}

function wireExisting(orderedFieldsList) {
  const pills = orderedFieldsList.querySelectorAll(".ordered-field-pill");
  pills.forEach(function (p) { wireDragForPill(p); });
}

function attachRemoveHandlers(orderedFieldsList, availableFieldsList) {
  const pills = orderedFieldsList.querySelectorAll(".ordered-field-pill");
  pills.forEach(function (p) {
    const removeBtn = p.querySelector(".pill-remove");
    if (!removeBtn) return;

    removeBtn.onclick = function (e) {
      e.stopPropagation();
      moveToAvailable(p, orderedFieldsList, availableFieldsList);
    };
  });
}

function moveToAvailable(pill, orderedFieldsList, availableFieldsList) {
  pill.remove();
  pill.classList.remove("ordered-field-pill", "dragging");
  pill.removeAttribute("draggable");
  pill.classList.add("available-pill");

  const removeBtn = pill.querySelector(".pill-remove");
  if (removeBtn) removeBtn.textContent = "+";

  pill.onclick = function () {
    moveToOrdered(pill, orderedFieldsList, availableFieldsList);
  };

  availableFieldsList.appendChild(pill);
}

function moveToOrdered(pill, orderedFieldsList, availableFieldsList) {
  pill.remove();
  pill.classList.remove("available-pill");
  pill.classList.add("ordered-field-pill");
  pill.onclick = null;

  const removeBtn = pill.querySelector(".pill-remove");
  if (removeBtn) removeBtn.textContent = "×";

  pill.setAttribute("draggable", "true");
  orderedFieldsList.appendChild(pill);
  wireDragForPill(pill);
  attachRemoveHandlers(orderedFieldsList, availableFieldsList);
}

function buildAvailablePill(value, orderedFieldsList, availableFieldsList) {
  const li = document.createElement("li");
  li.className = "available-pill";
  li.setAttribute("data-field", value);
  li.innerHTML = "<span class='pill-handle'>⋮⋮</span><span>" + escapeHtml(value) + "</span><span class='pill-remove'>+</span>";
  li.onclick = function () {
    moveToOrdered(li, orderedFieldsList, availableFieldsList);
  };
  return li;
}
