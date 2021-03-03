"use strict";

var mapselect = null;
var canvas = null;
var map = null;

// MAP METADATA
var zoomLevel = null; // 1 = show whole map in canvas ... 20 = show single cell
var mousePos = new Coordinate();
var mousePosRaw = new Coordinate();
var pathStart = new Coordinate();
var pathEnd = new Coordinate();
var dragStart = new Coordinate();
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
        slide: function (_event, ui) {
            document.getElementById("zoomLabel").innerHTML = ("Scale: " + ui.value);
            map.zoomTo(ui.value);
        },
        change: function (_, ui) {
            document.getElementById("zoomLabel").innerHTML = ("Scale: " + ui.value);
            //map.zoomTo(ui.value) // TODO ?
        }
    });

    mapselect = $("#mapselect");
    canvas = $("#canvas");
    map = new GridMap(canvas);

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

    console.log("Getting maps list");
    $.get("maps", loadMapList); // Load the maps in the select, forces redraw (see function above)

    $("#getPathButton").on("click", function () {
        $(this).after('<i id="spinner" class="fa fa-spinner fa-spin" aria-hidden="true"></i>');
        $.post("/getPath", JSON.stringify(map.getPathRequests()), displayPathAnswer).fail(resp => {
            $(this).prop("disabled", false);
            $("#spinner").remove();
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
                console.log("control pressed");
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
        _event.preventDefault(); // avoids double-click text selection when zooming fast
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
                $("#zoomSlider").slider("value", map.zoomDelta(event.shiftKey ? -1 : 1, mousePosRaw));
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
        map.addArrow(new Coordinate(srcx, srcy), new Coordinate(dstx, dsty));
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
            setZoom(map.resizeCanvas());
            map.draw();
        }

    });
    $(".path.pane").resizable({
        handles: "e, w",
        resize: function (_event, ui) {
            ui.position.left = 0;
        },
        stop: function (_event, ui) {
            setWidthInPercent(ui.element);
            setZoom(map.resizeCanvas());
            map.draw();
        }
    });

    window.addEventListener("resize", function () {
        setZoom(map.resizeCanvas());
    });
};

function setZoom(value = zoomMin) {
    $("#zoomSlider").slider("value", value);
}


function updateConflicts(_event) {
    let data = {};
    $("input[type=checkbox].constraint").each(function () {
        data[this.name] = this.checked;
    });
    console.log(data);
    $.post("/constraints", JSON.stringify(data), function () { });
}


// ----- MAP MODES -----

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

    mousePosRaw = map.toGridCoords(lastX, lastY); // converts canvas coordinates to map coordinates

    mousePos.x = Math.min(Math.max(0, Math.floor(mousePosRaw.x)), map.width - 1);
    mousePos.y = Math.min(Math.max(0, Math.floor(mousePosRaw.y)), map.height - 1);
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
        map.translateFromTo(dragStart, mousePosRaw);
    }
}

// ----- MAP LOADING -----

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
        var lines = result.split(/\r?\n/);
        map.setType(lines[0].split(" ")[1]);
        map.setHeight(lines[1].split(" ")[1]);
        map.setWidth(lines[2].split(" ")[1]);
        map.setData(lines.slice(4, lines.length));

        setZoom(map.resizeCanvas(true));
        map.draw();
    }).fail(err => {
        alert("Could not load the map!");
        console.log(err);
    });
}
