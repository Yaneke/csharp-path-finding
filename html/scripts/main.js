"use strict";

var canvas = null;
var mapselect = null;
var map = null;

// MAP INTERACTION METADATA

var mousePos, mousePosRaw, pathStart, pathEnd, dragStart;
var cntrlIsPressed = false;
var dragZoomMode, addPathMode, mouseMoved, mapWasDragged;
var dragMapIntervalID, drawPathIntervalID;


window.onload = function () {
    $("#zoomSlider").slider({
        min: zoomMin,
        max: zoomMax,
        step: 1,
        value: zoomMin,
        animate: "fast",
        slide: function (_, ui) {
            document.getElementById("zoomLabel").innerHTML = ("Scale: " + ui.value);
            map.zoomTo(ui.value);
        },
        change: function (_, ui) {
            document.getElementById("zoomLabel").innerHTML = ("Scale: " + ui.value);
            //map.zoomTo(ui.value) // TODO ?
        }
    });

    canvas = $("#canvas");
    map = new GraphMap(canvas);

    mapselect = $("#mapselect");

    resetMeta();
    resetMode();

    $("input[type=checkbox].constraint").on("click", updateConflicts);
    updateConflicts(null);


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
        loadMap();
    })

    $.get("maps", loadMapList); // Load the maps in the select, forces redraw (see function above)

    $("#getPathButton").on("click", function () {
        $.post("/getPath", JSON.stringify(map.getPathRequests()), function (res) {
            map.drawPathAnswer(res);
            console.log(res);
            $("#computationTime").val(res.duration);
            $("#solutionCost").val(res.cost);
            let tbody = $("#result_tbody");
            tbody.html("");
            let i = 0;
            res.paths.forEach(path => {
                let coordPath = "";
                path.coordinates.forEach(coord => {
                    coordPath += "(" + coord.x + ", " + coord.y + ")-> ";
                });
                let row = "<tr> <td> " + i++ + "</td> <td> " + path.coordinates.length + "</td> <td>" + coordPath + "</td> </tr>";
                tbody.append(row);
            });
        });
    });

    $("#resetPathButton").on("click", function () { map.resetPaths(); });

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
                $("#zoomSlider").slider("value", map.zoomDelta(event.shiftKey ? -1 : 1, mousePosRaw));
            }
            console.log("Cntrl+mouseup: exit dragZoomMode");
        }
        if (addPathMode) {
            clearInterval(drawPathIntervalID);
            mouseMoved = true; // forces to draw last line (in case mouse moved but interval did not trigger)
            whileAddingPath();
            pathEnd = new Coordinate(mousePos.x, mousePos.y);
            map.addArrow(pathStart, pathEnd);
            $("#xPathEnd").val(mousePos.x);
            $("#yPathEnd").val(mousePos.y);
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
            setZoom(map.resizeCanvas());
            map.draw();
        }

    });
    $(".path.pane").resizable({
        handles: "e, w",
        resize: function (event, ui) {
            ui.position.left = 0;
        },
        stop: function (event, ui) {
            setWidthInPercent(ui.element);
            setZoom(map.resizeCanvas());
            map.draw();
        }
    });

    window.addEventListener("resize", function () {
        setZoom(map.resizeCanvas());
    });
};


function updateConflicts(_event) {
    let data = {};
    $("input[type=checkbox].constraint").each(function () {
        data[this.name] = this.checked;
    });
    $.post("/constraints", JSON.stringify(data), function () { });
}

function setZoom(value = zoomMin) {
    $("#zoomSlider").slider("value", value);
}

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
    setZoom();
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
    // Alternative for computing position
    // const rect = canvas[0].getBoundingClientRect();
    // var lastX = event.clientX - rect.left;
    // var lastY = event.clientY - rect.top;
    var lastX = (event.offsetX || (event.pageX - canvas[0].offsetLeft));
    var lastY = (event.offsetY || (event.pageY - canvas[0].offsetTop));

    // Inverts the current context transform to account for scale, translation, rotation
    var trans = map.getLocalCoords(lastX, lastY);

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
        map.draw();
        map.drawArrow(pathStart, pathEnd);
    }
}

function whileDraggingMap() {
    if (mouseMoved) {
        mouseMoved = false;
        mapWasDragged = true;
        map.translateFromTo(dragStart, mousePosRaw);
    }
}

// ----- MAP LOADING + CANVAS RESIZING -----

function loadMapList(result) {
    result.forEach(element => {
        mapselect.append(new Option(element, element));
    });
    loadMap();
}

function loadMap() {
    $.get("/getSelectedMap", {
        map: $("#mapselect option:selected").text()
    }, function (result) {
        resetMeta();
        map.reset();
        var lines = result.split("\n");
        map.setWidth(lines[2].split(" ")[1]);
        map.setHeight(lines[1].split(" ")[1]);
        map.setData(lines.slice(4, lines.length));

        setZoom(map.resizeCanvas(true));
        map.draw();
    });
}
