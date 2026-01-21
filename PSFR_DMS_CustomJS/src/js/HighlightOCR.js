$(document).ajaxSuccess(function (event, xhr, settings) {
  if (settings.url.startsWith("/Files/GetBasicDetails")) {
    const fullTextValue = $("#txtSimpleSearch").val() || ""; // Default to empty string if undefined/null
    const ocrContentValue = $('[ref="txtOcrContent"]').val() || ""; // Default to empty string if undefined/null
 
    // Use ocrContentValue if it's filled, otherwise use fullTextValue
    const searchValue = ocrContentValue.trim() !== "" ? ocrContentValue : fullTextValue.trim() !== "" ? fullTextValue : null;
 
    if (searchValue) {
      const viewerFrame = $('[ref="viewerFrame"]');
      if (viewerFrame.length > 0) {
        const handleLoad = function () {
          const url = new URL(viewerFrame.attr("src"));
          url.searchParams.set("highlightKeyword", searchValue);
          viewerFrame.off("load", handleLoad); // Unbind the event after it's triggered
          viewerFrame.attr("src", url.toString());
        };
        viewerFrame.on("load", handleLoad);
      }
    }
  }
});
