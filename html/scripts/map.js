"use strict";

var canvas = null;
var context = null;
var mapselect = null;

var map = {
    width: 0,
    height: 0,
    data: null
};
map = new GraphMap();

// MAP METADATA
var zoomLevel = null; // 1 = show whole map in canvas ... 20 = show single cell
const zoomMin = 1, zoomMax = 20;

function resetZoom() {
    zoomLevel = zoomMin;
    $("#zoomSlider").slider("value", zoomMin);
}


var mousePos, mousePosRaw, pathStart, pathEnd, dragStart;
function resetMeta() {
    $("#xPathStart").val("");
    $("#yPathStart").val("");
    $("#xPathEnd").val("");
    $("#yPathEnd").val("");
    mousePos = new Coordinate();
    mousePosRaw = new Coordinate();
    pathStart = new Coordinate();
    pathEnd = new Coordinate();
    dragStart = new Coordinate();
    resetZoom();
}

// MAP MODES
var cntrlIsPressed = false;
var dragZoomMode, addPathMode, mouseMoved, mapWasDragged;
var dragMapIntervalID, drawPathIntervalID;
function resetMode() {
    dragZoomMode = false;
    addPathMode = false;
    mouseMoved = false;
    mapWasDragged = false;
    clearInterval(dragMapIntervalID);
    clearInterval(drawPathIntervalID);
}

window.onload = function () {
    $(".slider").slider({
        min: zoomMin,
        max: zoomMax,
        step: 1,
        value: zoomMin,
        animate: "fast",
        slide: function (event, ui) {
            document.getElementById("zoomLabel").innerHTML = ("Scale: " + ui.value);
            zoomMapTo(ui.value);
        },
        change: function (event, ui) {
            document.getElementById("zoomLabel").innerHTML = ("Scale: " + ui.value);
        }
    });

    canvas = $("#canvas");
    context = canvas[0].getContext("2d");
    trackTransforms(context);

    mapselect = $("#mapselect");

    resetMeta();
    resetMode();



    $("#xPathStart").on("change", function () {
        pathStart.x = $(this).val();
    });
    $("#yPathStart").on("change", function () {
        pathStart.y = $(this).val();
    });
    $("#xPathEnd").on("change", function () {
        pathEnd.x = $(this).val();
    });
    $("#yPathEnd").on("change", function () {
        pathEnd.y = $(this).val();
    });

    mapselect.on("change", function () {
        loadMap(drawMap);
    })

    $.get("maps", loadMapList); // Load the maps in the select, forces redraw (see function above)

    $("#loadButton").on("click", function () {
        loadMap(drawMap);
    });

    $("#getPathButton").on("click", function () {
        var pathRequest = new PathRequest(pathStart, pathEnd)
        console.log(JSON.stringify([pathRequest]));
        $.post("/getPath", JSON.stringify([pathRequest]), drawPath);
    });

    // --- KEY & MOUSE BINDINGS ---

    // Cntrl key down: mousedown will enter drag/zoom mode 
    $(document).on("keydown", function (event) {
        switch (event.which) {
            case 17: // Cntrl
                cntrlIsPressed = true;
                break;
        }
    });

    // Cntrl key up: mouseup/mouseout will exit drag/zoom mode
    $(document).on("keyup", function (event) {
        switch (event.which) {
            case 17:
                cntrlIsPressed = false;
                break;
        }
    });

    canvas.on("mousedown", function (event) {
        mouseMoved = false;
        if (cntrlIsPressed) {
            // Moving the mouse will drag, releasing it without moving will zoom (dezoom if shift is pressed)
            console.log("Cntrl+mousedown: enter dragZoomMode");
            dragZoomMode = true;
            dragStart = { x: mousePosRaw.x, y: mousePosRaw.y };
            dragMapIntervalID = setInterval(whileDraggingMap, 50);
        } else {
            // Start drawing a line from start point to the current mouse position.
            console.log("mousedown: enter addPathMode")
            addPathMode = true;
            mouseMoved = true; // forces arrow to be drawn at beginning
            pathStart = new Coordinate(mousePos.x, mousePos.y);
            $("#xPathStart").val(pathStart.x);
            $("#yPathStart").val(pathStart.y);
            drawPathIntervalID = setInterval(whileAddingPath, 50);
        }
    });

    canvas.on("mousemove", function (event) {
        updateMousePos(event);
        mouseMoved = true;
    });

    canvas.on("mouseup", function (event) {
        if (dragZoomMode) {
            clearInterval(dragMapIntervalID);
            if (mapWasDragged || mouseMoved) {
                whileDraggingMap();
            } else {
                zoomMap(event.shiftKey ? -1 : 1);
            }
            console.log("Cntrl+mouseup: exit dragZoomMode");
        }
        if (addPathMode) {
            clearInterval(drawPathIntervalID);
            mouseMoved = true; // forces to draw last line (in case mouse moved but interval did not trigger)
            whileAddingPath();
            pathEnd.x = mousePos.x;
            pathEnd.y = mousePos.y;
            $("#xPathEnd").val(pathEnd.x);
            $("#yPathEnd").val(pathEnd.y);
            console.log("mouseup: exit addPathMode")
        }
        resetMode();
    });

    canvas.on("mouseout", function () {
        resetMode();
    });

    // Make sure mouseup event ends dragging/zooming/adding path even if it happens out of canvas
    $(document).on("mouseup"), function () {
        resetMode();
    }


    $(".map.pane").resizable({
        handles: "e, w",
        stop: function (event, ui) {
            setWidthInPercent(ui.element);
        }

    });
    $(".path.pane").resizable({
        handles: "e, w",
        resize: function (event, ui) {
            ui.position.left = 0;
        },
        stop: function (event, ui) {
            setWidthInPercent(ui.element);
        }
    });

    window.addEventListener("resize", function () {
        resizeCanvas();
    });
};

function updateMousePos(event) {
    // Alternative for computing position
    // const rect = canvas[0].getBoundingClientRect();
    // var lastX = event.clientX - rect.left;
    // var lastY = event.clientY - rect.top;
    var lastX = (event.offsetX || (event.pageX - canvas[0].offsetLeft));
    var lastY = (event.offsetY || (event.pageY - canvas[0].offsetTop));

    // Inverts the current context transform to account for scale, translation, rotation
    var trans = context.transformedPoint(lastX, lastY);

    mousePosRaw.x = trans.x;
    mousePosRaw.y = trans.y;

    mousePos.x = Math.min(Math.max(0, Math.floor(trans.x)), map.width - 1);
    mousePos.y = Math.min(Math.max(0, Math.floor(trans.y)), map.height - 1);
    $("#xMousePos").val(mousePos.x);
    $("#yMousePos").val(mousePos.y);
}

function whileAddingPath() {
    if (mouseMoved) {
        mouseMoved = false;
        console.log("Moved path arrow.");
        pathEnd.x = mousePos.x;
        pathEnd.y = mousePos.y
        drawMap();
    }
}

function whileDraggingMap() {
    if (mouseMoved) {
        mouseMoved = false;
        mapWasDragged = true;
        console.log("Dragged map.");
        translateMapFromTo();
    }
}

// ----- MAP LOADING + CANVAS RESIZING -----

function loadMapList(result) {
    result.forEach(element => {
        mapselect.append(new Option(element, element));
    });
    loadMap(drawMap);
}

function loadMap(callback) {
    $.get("/getSelectedMap", {
        map: $("#mapselect option:selected").text()
    }, function (result) {
        resetMeta();
        var lines = result.split("\n");
        map.width = lines[2].split(" ")[1];
        map.height = lines[1].split(" ")[1];
        map.data = lines.slice(4, lines.length);

        resizeCanvas(true);
        callback()
    });
}

function resizeCanvas(resetTransform = false) {
    var prevCenter, prevZoom;
    if (!resetTransform) {
        prevCenter = context.transformedPoint(canvas[0].width / 2, canvas[0].height / 2);
        prevZoom = zoomLevel;
    }

    resetZoom();

    context.setTransform(1, 0, 0, 1, 0, 0);
    canvas.prop("width", canvas.innerWidth()); // maximize width to use all available pixels
    var canvasBaseScale = canvas[0].width * 1.0 / map.width;
    canvas.prop("height", map.height * canvasBaseScale); // adjust height to maintain aspect ratio
    context.scale(canvasBaseScale, canvasBaseScale);

    if (!resetTransform) {
        // translateMap({x: 0, y: 0}, prevCenter); // TODO centering is a bit off
        zoomMapTo(prevZoom, prevCenter);
    }
};

// ----- MAP ZOOM & TRANSLATE -----

function translateBounded(trans) {
    var topLeftBound = context.transformedPoint(0, 0);
    var botRightBound = context.transformedPoint(canvas[0].width, canvas[0].height);
    var transBound = {
        xPos: topLeftBound.x,
        yPos: topLeftBound.y,
        xNeg: map.width - botRightBound.x,
        yNeg: map.height - botRightBound.y
    }
    var boundedX = trans.x < 0 ? - Math.min(transBound.xNeg, -trans.x) : Math.min(transBound.xPos, trans.x);
    var boundedY = trans.y < 0 ? - Math.min(transBound.yNeg, -trans.y) : Math.min(transBound.yPos, trans.y);
    return { x: boundedX, y: boundedY };
}

function translateMap(trans) {
    translateMapFromTo({ x: 0, y: 0 }, trans);
}

function translateMapFromTo(from = dragStart, to = mousePosRaw) {
    var diff = { x: to.x - from.x, y: to.y - from.y };
    var trans = translateBounded(diff);

    context.translate(trans.x, trans.y);

    // prevents scaling from putting map out of bouds
    var topLeft = context.transformedPoint(0, 0);
    context.translate(Math.min(topLeft.x, 0), Math.min(topLeft.y, 0));
    drawMap(); // TODO use callback instead ?
}

function zoomMapTo(value, center = context.transformedPoint(canvas[0].width / 2, canvas[0].height / 2)) {
    zoomMap(value - zoomLevel, center)
}

function zoomMap(diff, center = mousePosRaw) {
    var newLevel = (diff > 0) ? Math.min(zoomLevel + diff, zoomMax) : Math.max(zoomLevel + diff, zoomMin);
    var trueDiff = newLevel - zoomLevel;

    zoomLevel = newLevel;
    $("#zoomSlider").slider("value", zoomLevel);

    var scaleFactor = Math.pow(map.width, 1.0 / (zoomMax - zoomMin));
    var factor = Math.pow(scaleFactor, trueDiff);

    context.translate(center.x, center.y); // resets origin so that it scales from center
    context.scale(factor, factor);
    translateMap({ x: -center.x, y: -center.y }); // takes care of re-drawing map

    console.log("Zoomed " + trueDiff + " times, current zoom: " + zoomLevel);
}

// ----- MAP DRAWING -----

function drawMap() {
    context.save();
    context.setTransform(1, 0, 0, 1, 0, 0);
    context.clearRect(0, 0, canvas[0].width, canvas[0].height);
    context.restore();

    context.fillStyle = "black";
    for (var i = 0; i < map.height; i++) {
        for (var j = 0; j < map.width; j++) {
            if (map.data[i][j] != ".") {
                context.fillRect(j, i, 1, 1);
            }
        }
    }
    context.stroke();
    if (pathStart.x && pathStart.y && pathEnd.x && pathEnd.y) drawArrow();
}

function drawArrow() {
    var headlen = 2; // length of head in cells
    context.lineWidth = 0.5;
    context.strokeStyle = "green";
    context.lineCap = "round";
    context.lineJoin = "round";

    var fromX = pathStart.x + 0.5, fromY = pathStart.y + 0.5;
    var toX = pathEnd.x + 0.5, toY = pathEnd.y + 0.5;
    var dx = toX - fromX;
    var dy = toY - fromY;
    var angle = Math.atan2(dy, dx);
    context.beginPath();
    context.moveTo(fromX, fromY);
    context.lineTo(toX, toY);
    context.lineTo(toX - headlen * Math.cos(angle - Math.PI / 6), toY - headlen * Math.sin(angle - Math.PI / 6));
    context.moveTo(toX, toY);
    context.lineTo(toX - headlen * Math.cos(angle + Math.PI / 6), toY - headlen * Math.sin(angle + Math.PI / 6));
    context.closePath();
    context.stroke();
}

function drawPath(path) {
    console.log(path);
    var textPath = "" + path.length + "\n";
    context.fillStyle = "red";
    path.forEach(function (vertexString) {
        var [y, x] = vertexString.id.split(", ");
        textPath += "(" + x + ", " + y + ") ";
        context.fillRect(x, y, 1, 1);
    });
    $("#pathResult").text(textPath);
}

// ----- TRANSFORMS TRACKING -----

// Taken from http://phrogz.net/tmp/canvas_zoom_to_cursor.html
// Copyright © 2011 <a href="mailto:!@phrogz.net">Gavin Kistner</a>. 
// Written to support <a href="http://stackoverflow.com/questions/5189968/zoom-to-cursor-calculations/5526721#5526721">this Stack Overflow answer</a>.</p>
function trackTransforms(ctx) {
    var svg = document.createElementNS("http://www.w3.org/2000/svg", 'svg');
    var xform = svg.createSVGMatrix();
    ctx.getTransform = function () { return xform; };

    var savedTransforms = [];
    var save = ctx.save;
    ctx.save = function () {
        savedTransforms.push(xform.translate(0, 0));
        return save.call(ctx);
    };
    var restore = ctx.restore;
    ctx.restore = function () {
        xform = savedTransforms.pop();
        return restore.call(ctx);
    };

    var scale = ctx.scale;
    ctx.scale = function (sx, sy) {
        xform = xform.scaleNonUniform(sx, sy);
        return scale.call(ctx, sx, sy);
    };
    var rotate = ctx.rotate;
    ctx.rotate = function (radians) {
        xform = xform.rotate(radians * 180 / Math.PI);
        return rotate.call(ctx, radians);
    };
    var translate = ctx.translate;
    ctx.translate = function (dx, dy) {
        xform = xform.translate(dx, dy);
        return translate.call(ctx, dx, dy);
    };
    var transform = ctx.transform;
    ctx.transform = function (a, b, c, d, e, f) {
        var m2 = svg.createSVGMatrix();
        m2.a = a; m2.b = b; m2.c = c; m2.d = d; m2.e = e; m2.f = f;
        xform = xform.multiply(m2);
        return transform.call(ctx, a, b, c, d, e, f);
    };
    var setTransform = ctx.setTransform;
    ctx.setTransform = function (a, b, c, d, e, f) {
        xform.a = a;
        xform.b = b;
        xform.c = c;
        xform.d = d;
        xform.e = e;
        xform.f = f;
        return setTransform.call(ctx, a, b, c, d, e, f);
    };
    var pt = svg.createSVGPoint();
    ctx.transformedPoint = function (x, y) {
        pt.x = x; pt.y = y;
        return pt.matrixTransform(xform.inverse());
    }
    ctx.inverseTransform = function (x, y) {
        pt.x = x; pt.y = y;
        return pt.matrixTransform(xform);
    }
}