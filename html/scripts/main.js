"use strict";

var canvas = null;
var context = null;

var map = null;

// MAP METADATA
var zoomLevel = null; // 1 = show whole map in canvas ... 20 = show single cell
const zoomMin = 1, zoomMax = 20;
var mousePos = new Coordinate();
var mousePosRaw = new Coordinate();
var pathStart = new Coordinate();
var pathEnd = new Coordinate();
var dragStart = new Coordinate();
var cntrlIsPressed = false;
var dragZoomMode, addPathMode, mouseMoved, mapWasDragged;
var dragMapIntervalID, drawPathIntervalID;


window.onload = function () {
    $(".slider").slider({
        min: zoomMin,
        max: zoomMax,
        step: 1,
        value: zoomMin,
        animate: "fast",
        slide: function (_event, ui) {
            document.getElementById("zoomLabel").innerHTML = ("Scale: " + ui.value);
            zoomMapTo(ui.value);
        }
    });

    canvas = $("#canvas");
    context = canvas[0].getContext("2d");
    map = new GraphMap(context);
    trackTransforms(context);

    resetMode();

    zoomLevel = zoomMin;
    $("#zoomSlider").slider("value", zoomMin);

    $("input[type=checkbox].constraint").on("click", updateConflicts);
    updateConflicts(null);

    $("#selfLoop, #selfLoopCost").on("change", function () {
        if (this.type == "checkbox") {
            $("#selfLoopCost").prop("disabled", !this.checked);
        }
        $.post("/updateMapSettings", JSON.stringify(buildMapSettings()));
    });


    $("#addPathBtn").on("click", addPathColumn);
    $("#cbsStep").on("click", function () {
        this.disabled = true;
        $(this).after('<i id="spinner" class="fa fa-spinner fa-spin" aria-hidden="true"></i>');
        $.post("/getPathStep", JSON.stringify(map.getPathRequests()), displayCBSStep).fail(resp => {
            $("#cbsStep").prop("disabled", false);
            alert(resp.responseText);
        });
    });

    $("#mapselect").on("change", function () {
        loadMap();
    })

    $.get("maps", loadMapList); // Load the maps in the select, forces redraw (see function above)

    $("#getPathButton").on("click", function () {
        this.disabled = true;
        $(this).after('<i id="spinner" class="fa fa-spinner fa-spin" aria-hidden="true"></i>');
        $.post("/getPath", JSON.stringify(map.getPathRequests()), displayPathAnswer).fail(resp => {
            $("#getPathButton").prop("disabled", false);
            alert(resp.responseText);
        });
    });

    $("#resetPathButton").on("click", function () {
        map.resetPaths();
        resetPathTable();
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

    canvas.on("mousedown", function (_event) {
        mouseMoved = false;
        if (cntrlIsPressed) {
            // Moving the mouse will drag, releasing it without moving will zoom (dezoom if shift is pressed)
            dragZoomMode = true;
            dragStart = { x: mousePosRaw.x, y: mousePosRaw.y };
            dragMapIntervalID = setInterval(whileDraggingMap, 50);
        } else {
            // Start drawing a line from start point to the current mouse position.
            addPathMode = true;
            mouseMoved = true; // forces arrow to be drawn at beginning
            pathStart = new Coordinate(mousePos.x, mousePos.y);
            let [srcx, srcy] = $("#sources td:last").children();
            srcx.value = mousePos.x;
            srcy.value = mousePos.y;
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
        }
        if (addPathMode) {
            clearInterval(drawPathIntervalID);
            mouseMoved = true; // forces to draw last line (in case mouse moved but interval did not trigger)
            whileAddingPath();
            pathEnd = new Coordinate(mousePos.x, mousePos.y);
            let [dstx, dsty] = $("#destinations td:last").children();
            dstx.value = mousePos.x;
            dsty.value = mousePos.y;
            $(dsty).trigger("change");
            addPathColumn();
        }
        resetMode();
    });

    $("#sources, #destinations").on("change", "input", function () {
        let agentNum = parseInt(this.id.substring(4));
        let srcx = $("#srcx" + agentNum).val() || 0;
        let srcy = $("#srcy" + agentNum).val() || 0;
        let dstx = $("#dstx" + agentNum).val() || 0;
        let dsty = $("#dsty" + agentNum).val() || 0;
        map.addPath(agentNum, new Coordinate(srcx, srcy), new Coordinate(dstx, dsty));
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
        stop: function (_event, ui) {
            setWidthInPercent(ui.element);
        }

    });
    $(".path.pane").resizable({
        handles: "e, w",
        resize: function (_event, ui) {
            ui.position.left = 0;
        },
        stop: function (_event, ui) {
            setWidthInPercent(ui.element);
        }
    });

    window.addEventListener("resize", function () {
        resizeCanvas();
    });
};


function updateConflicts(_event) {
    let data = {};
    $("input[type=checkbox].constraint").each(function () {
        data[this.name] = this.checked;
    });
    console.log(data);
    $.post("/constraints", JSON.stringify(data), function () { });
}


// MAP MODES

function resetMode() {
    dragZoomMode = false;
    addPathMode = false;
    mouseMoved = false;
    mapWasDragged = false;
    clearInterval(dragMapIntervalID);
    clearInterval(drawPathIntervalID);
}

function updateMousePos(event) {
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
        pathEnd.x = mousePos.x;
        pathEnd.y = mousePos.y
        map.draw();
        map.drawArrow(pathStart, pathEnd);
    }
}

function whileDraggingMap() {
    if (mouseMoved) {
        mouseMoved = false;
        mapWasDragged = true;
        translateMapFromTo();
    }
}

// ----- MAP LOADING + CANVAS RESIZING -----

function loadMapList(result) {
    result.forEach(element => {
        $("#mapselect").append(new Option(element, element));
    });
    loadMap();
}

function buildMapSettings() {
    return {
        cost: parseInt($("#selfLoopCost").val()),
        map: $("#mapselect option:selected").text(),
        selfLoop: $("#selfLoop").prop("checked")
    }
}

function loadMap() {
    $.post("/updateMapSettings", JSON.stringify(buildMapSettings()), function (result) {
        map.reset();
        resetPathTable();
        var lines = result.split("\n");
        map.setWidth(lines[2].split(" ")[1]);
        map.setHeight(lines[1].split(" ")[1]);
        map.setData(lines.slice(4, lines.length));
        resizeCanvas(true);
        map.draw();
    }).fail(err => {
        alert("Could not load the map!");
        console.log(err);
    });
}

function resizeCanvas(resetTransform = false) {
    var prevCenter, prevZoom;
    if (!resetTransform) {
        prevCenter = context.transformedPoint(canvas[0].width / 2, canvas[0].height / 2);
        prevZoom = zoomLevel;
    }
    zoomLevel = zoomMin;
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
    map.draw(true); // TODO use callback instead ?
}

function zoomMapTo(value, center = context.transformedPoint(canvas[0].width / 2, canvas[0].height / 2)) {
    zoomMap(value - zoomLevel, center)
}

function zoomMap(diff, center = mousePosRaw) {
    var newLevel = (diff > 0) ? Math.min(zoomLevel + diff, zoomMax) : Math.max(zoomLevel + diff, zoomMin);
    var trueDiff = newLevel - zoomLevel;
    zoomLevel = newLevel;
    var scaleFactor = Math.pow(map.width, 1.0 / (zoomMax - zoomMin));
    var factor = Math.pow(scaleFactor, trueDiff);

    context.translate(center.x, center.y); // resets origin so that it scales from center
    context.scale(factor, factor);
    translateMap({ x: -center.x, y: -center.y }); // takes care of re-drawing map
    //console.log("Zoomed " + trueDiff + " times, current zoom: " + zoomLevel);
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