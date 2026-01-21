export function renderIframeView(url) {
  const container = document.querySelector(".content-wrapper");
  if (!container) return;

  container.innerHTML = `
    <div class="iframe-container">
      <iframe id="externalFrame" src="${url}" frameborder="0" loading="lazy"></iframe>
    </div>
  `;

  const iframe = document.getElementById("externalFrame");
  if (!iframe) return;

  iframe.addEventListener("load", () => {
    iframe.contentWindow.postMessage({
      type: "AUTH_TOKEN",
      token: window.IdentityAccessToken,
      lang: window.language
    }, "https://prjw-uemmetz.intalio-fr.com:3030");
  });
}