"use strict";

var canvas = null;
var context = null;
var mapselect = null;

var canvasBaseScale = null // canvas pixels per map cell (at map scale 1)
var mapScale = null; // 1 = show whole map in canvas ... 10 = show min(2 cells, map.width) in canvas ?

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
var dragStart = {
    x: NaN,
    y: NaN
};
var map = {
    width: 0,
    height: 0,
    data: null
};

var cntrlIsPressed, dragZoomMode, addPathMode, mouseMoved, mapWasDragged;
var mouseDownIntervalID = NaN;

$(document).ready(function () {
    $("canvas").on("resize", function () {
        console.log("Canvas resized");
    });
});

window.onload = function () {
    canvas = $("#canvas");
    context = canvas[0].getContext("2d");
    trackTransforms(context);

    mapselect = $("#mapselect");
    mapScale = 1;

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

    // TODO use zoom function
    // $("#scaleField").on("change", function () {
    //     scale = canvas[0].width / (map.width * 1.0 / $(this).val())
    //     console.log(scale);
    //     map.imageData = null; // forces re-draw
    //     reDrawMap();
    // });

    mapselect.on("change", function () {
        loadMap(drawMap);
    })

    $.get("maps", loadMapList); // Load the maps in the select, forces redraw (see function above)

    $("#loadButton").on("click", function () {
        loadMap(drawMap);
    });

    $("#getPathButton").on("click", function () {
        $.get("/getPath", { pathStart: pathStart, pathEnd: pathEnd }, drawPath);
    });

    // --- KEY & MOUSE BINDINGS ---

    cntrlIsPressed = false; // determines if mousedown enters dragZoom or addPath mode
    function resetMode() {
        dragZoomMode = false;
        addPathMode = false;
        mouseMoved = false;
        mapWasDragged = false;
    }
    resetMode();

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
            dragStart = { x: mousePos.x, y: mousePos.y };
            mouseDownIntervalID = setInterval(whileDraggingMap, 50);
        } else {
            // Start drawing a line from start point to the current mouse position.
            console.log("mousedown: enter addPathMode")
            addPathMode = true;
            mouseMoved = true; // forces arrow to be drawn at beginning
            pathStart = {x: mousePos.x, y: mousePos.y};
            $("#xPathStart").val(pathStart.x);
            $("#yPathStart").val(pathStart.y);
            mouseDownIntervalID = setInterval(whileAddingPath, 100);
        }
    });

    canvas.on("mousemove", function (event) {
        updateMousePos(event);
        mouseMoved = true;
    });

    canvas.on("mouseup", function (event) {
        clearInterval(mouseDownIntervalID);
        if (dragZoomMode) {
            if (mapWasDragged || mouseMoved) {
                whileDraggingMap();
            } else {
                zoomMap(event.shiftKey ? -1 : 1);
            }
            console.log("Cntrl+mouseup: exit dragZoomMode");
        }
        if (addPathMode) {
            // Draw one final line
            mouseMoved = true; // forces to draw last line
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
        clearInterval(mouseDownIntervalID);
        resetMode();
    });

    // Make sure mouseup event ends dragging/zooming/adding path even if it happens out of canvas
    $(document).on("mouseup"), function () {
        clearInterval(mouseDownIntervalID);
        resetMode();
    }
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

    mousePos.x = Math.min(Math.max(0, Math.floor(trans.x)), map.width - 1);
    mousePos.y = Math.min(Math.max(0, Math.floor(trans.y)), map.height - 1);
    $("#xMousePos").val(mousePos.x);
    $("#yMousePos").val(mousePos.y);
}

function whileAddingPath() {
    if (mouseMoved) {
        mouseMoved = false;
        console.log("Moved path arrow.");
        pathEnd = {x: mousePos.x, y: mousePos.y};
        drawMap();
    }
}

function whileDraggingMap() {
    if (mouseMoved) {
        mouseMoved = false;
        mapWasDragged = true;
        console.log("Dragged map.");
        translateMap();
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
        var lines = result.split("\n");
        map.width = lines[2].split(" ")[1];
        map.height = lines[1].split(" ")[1];
        map.data = lines.slice(4, lines.length);
        
        pathStart = {x: NaN, y: NaN};
        pathEnd = {x: NaN, y: NaN};

        resizeCanvas(true);
        callback()
    });
}

function resizeCanvas(resetTransform = true) {
    var transform;
    if (!resetTransform) {
        // TODO fix so that current transform is only adjusted by newScale
        // once fixed change default to resetTransform = false
        const newScale = canvas.innerWidth() * 1.0 / canvas[0].width;
        context.scale(newScale, newScale);
        transform = context.getTransform(); // this gives only NaN for some reason
    }
    context.setTransform(1, 0, 0, 1, 0, 0);
    canvas.prop("width", canvas.innerWidth()); // maximize width to use all available pixels
    canvasBaseScale = canvas[0].width * 1.0 / map.width;
    canvas.prop("height", map.height * canvasBaseScale); // adjust height to maintain aspect ratio
    context.scale(canvasBaseScale, canvasBaseScale);

    if (!resetTransform) {
        context.transform(transform);
    }
};

// ----- MAP ZOOM & TRANSLATE -----

function translateMap() {
    var topLeftBound = context.transformedPoint(0, 0);
    var botRightBound = context.transformedPoint(canvas[0].width, canvas[0].height);
    var diff = {x: mousePos.x - dragStart.x, y: mousePos.y - dragStart.y};
    var diffBound = {
        xPos: topLeftBound.x, 
        yPos: topLeftBound.y, 
        xNeg: map.width - botRightBound.x,
        yNeg: map.height - botRightBound.y
    }
    var transX = diff.x < 0 ? - Math.min(diffBound.xNeg, -diff.x) : Math.min(diffBound.xPos, diff.x);
    var transY = diff.y < 0 ? - Math.min(diffBound.yNeg, -diff.y) : Math.min(diffBound.yPos, diff.y);
    context.translate(transX, transY);
    drawMap();
}

function zoomMap(clicks) {
    var scaleFactor = 1.1;
    console.log("Zoomed " + clicks + " times");
    context.translate(mousePos.x, mousePos.y);
    var factor = Math.pow(scaleFactor, clicks);
    context.scale(factor, factor);
    context.translate(-mousePos.x, -mousePos.y);
    drawMap();
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
    context.stroke();
    context.closePath();
}

function drawPath(path) {
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