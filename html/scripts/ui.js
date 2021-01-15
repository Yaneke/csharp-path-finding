"use strict;"

function setWidthInPercent(element) {
    var percentageWidth = (element.width() / element.parent().width()) * 100;
    element.width(percentageWidth + '%');
    console.log("Width set for" + element);
    resizeCanvas();
    drawMap();
}
