$('#btn_1').click(function () {
    var $list = $(".product_card");

    $list.sort(function (a, b) {
        var sort1 = $(a).find("p:first").html();
        var sort2 = $(b).find("p:first").html();

        return parseInt(sort1) - parseInt(sort2)    ;
    });

    $list.detach().appendTo("#category_item_list");
})

$('#btn_2').click(function () {
    var $list = $(".product_card");

    $list.sort(function (a, b) {
        var sort1 = $(a).find("p:first").html();
        var sort2 = $(b).find("p:first").html();

        return parseInt(sort2) - parseInt(sort1)  ;
    });

    $list.detach().appendTo("#category_item_list");
})

$('#btn_3').click(function () {
    var $list = $(".product_card");

    $list.sort(function (a, b) {
        var sort1 = $(a).find("p:last").html();
        var sort2 = $(b).find("p:last").html();

        var start = new Date(sort1.replace("-", "/").replace("-", "/"));
        var end   = new Date(sort2.replace("-", "/").replace("-", "/"));

        return end - start;
    });

    $list.detach().appendTo("#category_item_list");
})


$('#btn_4').click(function () {
    var $list = $(".product_card");

    $list.sort(function (a, b) {
        var sort1 = $(a).find("p:last").html();
        var sort2 = $(b).find("p:last").html();

        var start = new Date(sort1.replace("-", "/").replace("-", "/"));
        var end = new Date(sort2.replace("-", "/").replace("-", "/"));

        return start - end ;
    });

    $list.detach().appendTo("#category_item_list");
})