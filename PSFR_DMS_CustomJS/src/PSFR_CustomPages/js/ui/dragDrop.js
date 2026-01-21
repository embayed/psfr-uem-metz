export function enableVerticalReorder(listEl) {
  if (!listEl) return;

  listEl.addEventListener("dragover", function (e) {
    e.preventDefault();

    const dragging = listEl.querySelector(".ordered-field-pill.dragging");
    if (!dragging) return;

    const afterElement = getDragAfterElement(listEl, e.clientY);
    if (afterElement == null) listEl.appendChild(dragging);
    else listEl.insertBefore(dragging, afterElement);
  });
}

export function wireDragForPill(pill) {
  pill.addEventListener("dragstart", function (e) {
    pill.classList.add("dragging");
    e.dataTransfer.effectAllowed = "move";
    e.dataTransfer.setData("text/plain", pill.dataset.field || "");
  });

  pill.addEventListener("dragend", function () {
    pill.classList.remove("dragging");
  });
}

function getDragAfterElement(container, y) {
  const items = Array.prototype.slice.call(container.querySelectorAll(".ordered-field-pill:not(.dragging)"));
  let closest = { offset: Number.NEGATIVE_INFINITY, element: null };

  items.forEach(function (child) {
    const box = child.getBoundingClientRect();
    const offset = y - (box.top + box.height / 2);
    if (offset < 0 && offset > closest.offset) closest = { offset: offset, element: child };
  });

  return closest.element;
}
