var reload = function (x, interval) {
    setInterval(function () {
        $(x).load(location.href + " " + x + ">*", "");
    }, interval * 1000)
};