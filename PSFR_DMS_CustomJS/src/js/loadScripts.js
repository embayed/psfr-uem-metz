function loadScripts(sources) {
  sources.forEach((src) => {
    var script = document.createElement("script");
    script.src = "/js/Custom/" + src + "?" + Math.random();
    script.type = "text/javascript";
    script.async = false;
    document.body.appendChild(script);
  });
}
function loadStyles(sources) {
  sources.forEach((src) => {
    $("head").append(
      `<link href="${src + "?" + Math.random()}" rel="stylesheet">`
    );
  });
}
loadStyles([]);
loadScripts(["copyFileLink.js"]);
