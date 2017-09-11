function CutText(str, maxlength) {
    if (str.length <= maxlength) return str;
    var temp = str.substr(0, maxlength - 3) + "...";
    return temp;

}

function change(className,max) {
    for (var i = 0; i < document.getElementsByClassName(className).length; i++)
        document.getElementsByClassName(className)[i].innerHTML = CutText(document.getElementsByClassName(className)[i].innerHTML, max);
}
