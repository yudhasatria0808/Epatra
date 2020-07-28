function stripTrailingSlash(str) {
    if (str.substr(-1) == '/') {
        return str.substr(0, str.length - 1);
    }
    return str;
}
var url = window.location.pathname;
var activePage = stripTrailingSlash(url);

$(".page-sidebar-menu > li > a").each(function () {
    var currentPage = stripTrailingSlash($(this).attr('href'));
    if (activePage == currentPage || (activePage.split('/')[1] == currentPage.split('/')[1]) && activePage !== "") {
        $(this).append("<span class='selected'></span>");
        $(this).parent().addClass('active');
    }
});

$('.page-sidebar-menu > li').find('li > a').each(function () {
    var currentPage = stripTrailingSlash($(this).attr('href'));
    if (activePage == currentPage || (activePage.split('/')[1] == currentPage.split('/')[1]) && activePage !== "") {
        $(this).parents('li').addClass('active');
        $(this).parents('ul').css("display", "block");
        $(this).parents('li').find('a').append("<span class='selected'></span>");
    }

});

if (activePage == "" || activePage.indexOf("Home") > -1)
{
    $('.start').addClass('active');
    $('.start').find('a').append("<span class='selected'></span>");
    
}

$(function () {
    $(".datepicker").datepicker({
        dateFormat: 'dd/mm/yy',
        changeYear: true,
        changeMonth: true,
        showAnim: 'slideDown'
    });
    $.validator.addMethod("date",
        function (value, element) {
            return this.optional(element) !== null;
            // || parseDate(value, "dd/mm/yy") !== null;
        });
});

$(function () {
    $(".yearpicker").datepicker({
        dateFormat: 'yy',
        changeYear: true,
        changeMonth: false,
        showAnim: 'slideDown'
    });
    $.validator.addMethod("date",
        function (value, element) {
            return this.optional(element) !== null;
            // || parseDate(value, "dd/mm/yy") !== null;
        });
});


$(function () {
    $('.datetimepicker').datetimepicker({
        format: 'DD/MM/YYYY HH:mm'
    });
});