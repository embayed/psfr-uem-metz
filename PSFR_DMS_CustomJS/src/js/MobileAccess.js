function handleMenuChange() {
  const currentMenu = window.location.hash || "#";
  $('body').removeClass('aside-toggled');
  if (currentMenu == "#advancedsearch")
    $('[ref="extensionContainer"]').addClass("col-xs-12");
}

$(function () {
  $(window).on("hashchange", handleMenuChange);
  setTimeout(handleMenuChange, 0);
});