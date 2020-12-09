"use strict";

var canvas = null;
var select = null;
var scale = 1;
var xMousePosField = null;
var yMousePosField = null;
var mouseDownIntervalID = NaN;
var mousePos = {
    x: NaN,
    y: NaN
};
var pathStart = {
    x: NaN,
    y: NaN
};
var pathEnd = {
    x: NaN,
    y: NaN
};
var map = {
    width: 0,
    height: 0,
    data: null,
    image: null
};

$(document).ready(function () {
    canvas = $("#canvas");
    select = $("#mapselect");
    xMousePosField = $("#xMousePos");
    yMousePosField = $("#yMousePos");
    scale = $("#scaleField").val();
    $("#xPathStart").on("change", function () {
        pathStart.x = $(this).val();
    });
    $("yxPathStart").on("change", function () {
        pathStart.y = $(this).val();
    });
    $("#xPathEnd").on("change", function () {
        pathEnd.x = $(this).val();
    });
    $("yxPathEnd").on("change", function () {
        pathEnd.y = $(this).val();
    });
    $.get("maps", loadMapList); // Load the maps in the select

    $("#scaleField").on("change", function () {
        scale = $(this).val();
        map.imageData = null;
        drawMap();
    });

    select.on("change", function () {
        loadMap(drawMap);
    })

    $("#loadButton").on("click", function () {
        loadMap(drawMap);
    });

    $("#getPathButton").on("click", function () {
        $.get("/getPath", { pathStart: pathStart, pathEnd: pathEnd }, drawPath);
    });

    canvas.on("mousedown", function () {
        // Start drawing a line from start point to the current mouse position.
        pathStart.x = mousePos.x;
        pathStart.y = mousePos.y;
        $("#xPathStart").val(pathStart.x);
        $("#yPathStart").val(pathStart.y);
        mouseDownIntervalID = setInterval(whileMouseDown, 20);
    });

    canvas.on("mouseup", function () {
        // Draw one final line
        pathEnd.x = mousePos.x;
        pathEnd.y = mousePos.y;
        $("#xPathEnd").val(pathEnd.x);
        $("#yPathEnd").val(pathEnd.y);
        clearInterval(mouseDownIntervalID);
    });

    canvas.on("mouseout", function () {
        clearInterval(mouseDownIntervalID);
    });

    canvas.on("mousemove", function (event) {
        updateCursorPosition(event);
        xMousePosField.val(mousePos.x);
        yMousePosField.val(mousePos.y);
    });
});

function whileMouseDown() {
    drawMap();
    var context = canvas[0].getContext("2d");
    context.strokeStyle = "green";
    context.lineWidth = scale;
    drawArrow(context, pathStart.x * scale, pathStart.y * scale, mousePos.x * scale, mousePos.y * scale);
}


function drawPath(path) {
    var textPath = "" + path.length + "\n";
    var context = canvas[0].getContext("2d");
    context.fillStyle = "red";
    path.forEach(function (vertexString) {
        var [y, x] = vertexString.id.split(", ");
        textPath += "(" + x + ", " + y + ") ";
        context.fillRect(x * scale, y * scale, scale, scale);
    });
    $("#pathResult").text(textPath);
}

function drawMap() {
    var context = canvas[0].getContext("2d");
    if (map.imageData == null) {
        canvas.prop("width", map.width * scale);
        canvas.prop("height", map.height * scale);
        for (var i = 0; i < map.height; i++) {
            for (var j = 0; j < map.width; j++) {
                if (map.data[i][j] != ".") {
                    context.fillRect(j * scale, i * scale, scale, scale);
                }
            }
        }
        map.imageData = context.getImageData(0, 0, canvas[0].width, canvas[0].height);
        map.imageData.data.set(new Uint8ClampedArray(map.imageData.data));
    } else {
        context.clearRect(0, 0, canvas[0].width, canvas[0].height);
        canvas[0].getContext("2d").putImageData(map.imageData, 0, 0);
    }
}


function loadMapList(result) {
    result.forEach(element => {
        select.append(new Option(element, element));
    });
    loadMap(drawMap);
}

function loadMap(callback) {
    $.get("/getSelectedMap", {
        map: $("#mapselect option:selected").text()
    }, function (result) {
        var lines = result.split("\n");
        map.width = lines[2].split(" ")[1];
        map.height = lines[1].split(" ")[1];
        map.data = lines.slice(4, lines.length - 1);
        map.imageData = null;
        callback()
    });
}


function updateCursorPosition(event) {
    const rect = canvas[0].getBoundingClientRect();
    mousePos.x = Math.floor((event.clientX - rect.left) / scale);
    mousePos.y = Math.floor((event.clientY - rect.top) / scale);
}



function drawArrow(context, fromx, fromy, tox, toy) {
    var headlen = 10; // length of head in pixels
    var dx = tox - fromx;
    var dy = toy - fromy;
    var angle = Math.atan2(dy, dx);
    context.beginPath();
    context.moveTo(fromx, fromy);
    context.lineTo(tox, toy);
    context.lineTo(tox - headlen * Math.cos(angle - Math.PI / 6), toy - headlen * Math.sin(angle - Math.PI / 6));
    context.moveTo(tox, toy);
    context.lineTo(tox - headlen * Math.cos(angle + Math.PI / 6), toy - headlen * Math.sin(angle + Math.PI / 6));
    context.stroke();
}