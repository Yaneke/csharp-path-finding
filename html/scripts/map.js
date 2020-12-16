"use strict";

var canvas = null;
var context = null;
var mapselect = null;

var canvasBaseScale = null // canvas pixels per map cell (at map scale 1)
var mapScale = null; // 1 = show whole map in canvas ... 10 = show 1 cell in canvas ?

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
var cntrlIsPressed, dragZoomMode, addPathMode;

$(document).ready(function() {
    $("canvas").on("resize", function() {
        console.log("Canvas resized");
    });
});

window.onload = function () {
    canvas = $("#canvas");
    context = canvas[0].getContext("2d");
    trackTransforms(context);

    mapselect = $("#mapselect");
    xMousePosField = $("#xMousePos");
    yMousePosField = $("#yMousePos");
    mapScale = 1;

    cntrlIsPressed = false; // determines if mousedown enters dragZoom or addPath mode
    dragZoomMode = false;
    var dragStart, wasDragged;

    addPathMode = false;
    var lastX = canvas.width / 2, lastY = canvas.height / 2;

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

    // $("#scaleField").on("change", function () {
    //     scale = canvas[0].width / (map.width * 1.0 / $(this).val())
    //     console.log(scale);
    //     map.imageData = null; // forces re-draw
    //     reDrawMap();
    // });

    mapselect.on("change", function () {
        loadMap(reDrawMap);
    })

    $.get("maps", loadMapList); // Load the maps in the select, forces redraw (see function above)

    $("#loadButton").on("click", function () {
        loadMap(reDrawMap);
    });

    $("#getPathButton").on("click", function () {
        $.get("/getPath", { pathStart: pathStart, pathEnd: pathEnd }, drawPath);
    });

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
        if (cntrlIsPressed) {
            // Moving the mouse will drag, releasing it without moving will zoom (dezoom if shift is pressed)
            dragZoomMode = true;
            wasDragged = false; // can become true one mousemove
            console.log("Cntrl+mousedown: start dragging or zooming map");
            document.body.style.mozUserSelect = document.body.style.webkitUserSelect = document.body.style.userSelect = 'none';
            lastX = event.offsetX || (event.pageX - canvas.offsetLeft);
            lastY = event.offsetY || (event.pageY - canvas.offsetTop);
            dragStart = context.transformedPoint(lastX, lastY);
            // TODO use setInterval(whileDragging, 20) instead
        } else {
            // Start drawing a line from start point to the current mouse position.
            addPathMode = true;
            console.log("mousedown: retained path start, start moving to path end")
            pathStart.x = mousePos.x;
            pathStart.y = mousePos.y;
            $("#xPathStart").val(pathStart.x);
            $("#yPathStart").val(pathStart.y);
            mouseDownIntervalID = setInterval(whileAddingPath, 20);
        }
    });

    canvas.on("mousemove", function (event) {
        if (dragZoomMode) {
            lastX = event.offsetX || (event.pageX - canvas.offsetLeft);
            lastY = event.offsetY || (event.pageY - canvas.offsetTop);
            wasDragged = true;
            if (dragStart) {
                console.log("dragging... from" + dragStart.x + ", " + dragStart.y + " to " + lastX + ", " + lastY);
                var pt = context.transformedPoint(lastX, lastY);
                context.translate(pt.x - dragStart.x, pt.y - dragStart.y);
                reDrawMap();
            }
        } else {
            updateMousePos(event);
            xMousePosField.val(mousePos.x);
            yMousePosField.val(mousePos.y);
        }
    });

    canvas.on("mouseup", function (event) {
        if (dragZoomMode) {
            if (wasDragged) {
                console.log("Cntrl+mouseup: stop dragging map")// End dragging
                dragZoomMode = false;
                dragStart = null;
                wasDragged = false;
            } else {
                console.log("Cntrl+mouseup (no drag): adjust zoom")
                zoom(event.shiftKey ? -1 : 1 );
            }
        } else {
            // Draw one final line
            console.log("mouseup: finish adding agent")
            pathEnd.x = mousePos.x;
            pathEnd.y = mousePos.y;
            $("#xPathEnd").val(pathEnd.x);
            $("#yPathEnd").val(pathEnd.y);
            clearInterval(mouseDownIntervalID);
        }
    });

    canvas.on("mouseout", function () {
        if (dragZoomMode) {
            console.log("mouseout: stop dragging map")// End dragging
            dragZoomMode = false;
            dragStart = null;
        }
        clearInterval(mouseDownIntervalID);
    });

    var scaleFactor = 1.1;
    var zoom = function (clicks) {
        console.log("Zoomed " + clicks + " times");
        var pt = context.transformedPoint(lastX, lastY);
        context.translate(pt.x, pt.y);
        var factor = Math.pow(scaleFactor, clicks);
        context.scale(factor, factor);
        context.translate(-pt.x, -pt.y);
        reDrawMap();
    }

    // canvas.on("wheel",  function (event) {
    //     var delta = event.wheelDelta ? event.wheelDelta / 40 : event.detail ? -event.detail : 0;
    //     console.log("scroll with delta: " + delta);
    //     if (delta) zoom(delta);
    //     return event.preventDefault() && false;
    // });
    // canvas.on('DOMMouseScroll', handleScroll); // CONNECT WITH SLIDER
    // canvas.on('mousewheel', handleScroll);
};

function whileAddingPath() {
    console.log("drawing arrow...");
    reDrawMap(); // TODO: really useful ?
    context.strokeStyle = "green";
    context.lineWidth = canvasBaseScale;
    drawArrow(context, pathStart.x * mapScale, pathStart.y * mapScale, mousePos.x * mapScale, mousePos.y * mapScale);
}


function drawPath(path) {
    var textPath = "" + path.length + "\n";
    context.fillStyle = "red";
    path.forEach(function (vertexString) {
        var [y, x] = vertexString.id.split(", ");
        textPath += "(" + x + ", " + y + ") ";
        context.fillRect(x * mapScale, y * mapScale, mapScale, mapScale);
    });
    $("#pathResult").text(textPath);
}

function reDrawMap() {
    context.save();
    context.setTransform(1, 0, 0, 1, 0, 0);
    context.clearRect(0, 0, canvas[0].width, canvas[0].height);
    context.restore();

    for (var i = 0; i < map.height; i++) {
        for (var j = 0; j < map.width; j++) {
            if (map.data[i][j] != ".") {
                context.fillRect(j, i, 1, 1);
            }
        }
    }
    context.stroke();
    // var pt1 = context.transformedPoint(0, 0);
    // var pt2 = context.transformedPoint(canvas[0].width, canvas[0].height); // TODO actual bounds (translate/scale)
    // map.imageData = context.getImageData(pt1.x, pt1.y, pt2.x - pt1.x, pt2.y - pt1.y);
    // map.imageData.data.set(new Uint8ClampedArray(map.imageData.data));
    // // } else {
    // //     context.clearRect(0, 0, canvas[0].width, canvas[0].height);
    // //     canvas[0].getContext("2d").putImageData(map.imageData, 0, 0);
    // // }
}


function loadMapList(result) {
    result.forEach(element => {
        mapselect.append(new Option(element, element));
    });
    loadMap(reDrawMap);
}

function loadMap(callback) {
    $.get("/getSelectedMap", {
        map: $("#mapselect option:selected").text()
    }, function (result) {
        var lines = result.split("\n");
        map.width = lines[2].split(" ")[1];
        map.height = lines[1].split(" ")[1];
        map.data = lines.slice(4, lines.length);
        map.imageData = null;
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


function updateMousePos(event) {
    // Alternative
    // const rect = canvas[0].getBoundingClientRect();
    // var lastX = event.clientX - rect.left;
    // var lastY = event.clientY - rect.top;
    var lastX = (event.offsetX || (event.pageX - canvas[0].offsetLeft));
    var lastY = (event.offsetY || (event.pageY - canvas[0].offsetTop));
    
    var trans = context.transformedPoint(lastX , lastY);
    mousePos.x = Math.min(Math.max(0, Math.floor(trans.x)), map.width - 1);
    mousePos.y = Math.min(Math.max(0, Math.floor(trans.y)), map.height - 1);
}



function drawArrow(context, fromX, fromY, toX, toY) {
    function toScale(x) {return x;}
    var headlen = toScale(10); // length of head in pixels
    var fromX = toScale(fromX), fromY = toScale(fromY), toX = toScale(toX), toY = toScale(toY);
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
}

// Taken from http://phrogz.net/tmp/canvas_zoom_to_cursor.html
// Copyright © 2011 <a href="mailto:!@phrogz.net">Gavin Kistner</a>. 
// Written to support <a href="http://stackoverflow.com/questions/5189968/zoom-to-cursor-calculations/5526721#5526721">this Stack Overflow answer</a>.</p>
function trackTransforms(ctx){
    var svg = document.createElementNS("http://www.w3.org/2000/svg",'svg');
    var xform = svg.createSVGMatrix();
    ctx.getTransform = function(){ return xform; };
    
    var savedTransforms = [];
    var save = ctx.save;
    ctx.save = function(){
        savedTransforms.push(xform.translate(0,0));
        return save.call(ctx);
    };
    var restore = ctx.restore;
    ctx.restore = function(){
        xform = savedTransforms.pop();
        return restore.call(ctx);
    };

    var scale = ctx.scale;
    ctx.scale = function(sx,sy){
        xform = xform.scaleNonUniform(sx,sy);
        return scale.call(ctx,sx,sy);
    };
    var rotate = ctx.rotate;
    ctx.rotate = function(radians){
        xform = xform.rotate(radians*180/Math.PI);
        return rotate.call(ctx,radians);
    };
    var translate = ctx.translate;
    ctx.translate = function(dx,dy){
        xform = xform.translate(dx,dy);
        return translate.call(ctx,dx,dy);
    };
    var transform = ctx.transform;
    ctx.transform = function(a,b,c,d,e,f){
        var m2 = svg.createSVGMatrix();
        m2.a=a; m2.b=b; m2.c=c; m2.d=d; m2.e=e; m2.f=f;
        xform = xform.multiply(m2);
        return transform.call(ctx,a,b,c,d,e,f);
    };
    var setTransform = ctx.setTransform;
    ctx.setTransform = function(a,b,c,d,e,f){
        xform.a = a;
        xform.b = b;
        xform.c = c;
        xform.d = d;
        xform.e = e;
        xform.f = f;
        return setTransform.call(ctx,a,b,c,d,e,f);
    };
    var pt  = svg.createSVGPoint();
    ctx.transformedPoint = function(x,y){
        pt.x=x; pt.y=y;
        return pt.matrixTransform(xform.inverse());
    }
}