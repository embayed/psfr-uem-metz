$(document).on('draw.dt', '[ref="grdItems"]', function () {
    // Iterate through each table row within the grid
    $(this).find('tr').each(function () {
        const $ellipsis = $(this).find('.ellipsis');
        const title = $ellipsis.attr('title');

        // If a title exists, set it as the visible text
        if (title) {
            $ellipsis.text(title);
        }
    });
});