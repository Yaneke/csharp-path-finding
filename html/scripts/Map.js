COLOURS = ["green", "red", "blue", "cyan", "yellow", "orange"];
const zoomMin = 1, zoomMax = 20; // 1 = show whole map in canvas ... 20 = show single cell

class GraphMap {
    constructor(canvas) {
        this.reset();
        this.zoomLevel = zoomMin;
        this.canvas = canvas;
        this.context = canvas[0].getContext("2d");
        trackTransforms(this.context);
    }

    setWidth(width) {
        this.width = parseInt(width);
    }

    setHeight(height) {
        this.height = parseInt(height);
    }

    setData(data) {
        this.data = data;
    }

    resetImage() {
        this.image = null;
    }

    isDrawn() {
        return this.image == null;
    }

    reset() {
        this.width = 0;
        this.height = 0;
        this.data = null;
        this.image = null;
        this.pathRequest = new PathRequest();
    }

    addArrow(start, end) {
        this.pathRequest.addPath(new Coordinate(start.x, start.y), new Coordinate(end.x, end.y));
        this.draw(true);
    }


    draw(force = false) {
        if (force || this.image == null) {
            // Save and restore context for custom transforms.
            this.context.save();
            this.context.setTransform(1, 0, 0, 1, 0, 0);
            this.context.clearRect(0, 0, this.canvas[0].width, this.canvas[0].height);
            this.context.restore();
            this.context.fillStyle = "black";
            for (var i = 0; i < this.height; i++) {
                for (var j = 0; j < this.width; j++) {
                    if (this.data[i][j] != ".") {
                        this.context.fillRect(j, i, 1, 1);
                    }
                }
            }
            for (var i = 0; i < this.pathRequest.length(); i++) {
                this.drawArrow(this.pathRequest.start[i], this.pathRequest.end[i], COLOURS[i]);
            }
            this.image = this.context.getImageData(0, 0, this.canvas[0].width, this.canvas[0].height);
            this.image.data.set(new Uint8ClampedArray(this.image.data));
        } else {
            this.context.putImageData(this.image, 0, 0);
        }
    }

    /**
     * 
     * @param {Coordinate} start 
     * @param {Coordinate} stop 
     */
    drawArrow(start, stop, colour = null) {
        var headlen = 2; // length of head in cells
        this.context.lineWidth = 0.5;
        if (colour == null) {
            this.context.strokeStyle = COLOURS[this.pathRequest.length()];
        } else {
            this.context.strokeStyle = colour;
        }
        this.context.lineCap = "round";
        this.context.lineJoin = "round";

        var fromX = start.x + 0.5;
        var fromY = start.y + 0.5;
        var toX = stop.x + 0.5;
        var toY = stop.y + 0.5;
        var dx = toX - fromX;
        var dy = toY - fromY;
        var angle = Math.atan2(dy, dx);
        this.context.beginPath();
        this.context.moveTo(fromX, fromY);
        this.context.lineTo(toX, toY);
        this.context.lineTo(toX - headlen * Math.cos(angle - Math.PI / 6), toY - headlen * Math.sin(angle - Math.PI / 6));
        this.context.moveTo(toX, toY);
        this.context.lineTo(toX - headlen * Math.cos(angle + Math.PI / 6), toY - headlen * Math.sin(angle + Math.PI / 6));
        this.context.closePath();
        this.context.stroke();
    }


    drawPathAnswer(pathAnswer) {
        var i = 0;
        var that = this;
        pathAnswer.paths.forEach(function (path) {
            that.context.fillStyle = COLOURS[i++ % COLOURS.length];
            path.coordinates.forEach(function (coord) {
                that.context.fillRect(coord.x, coord.y, 1, 1);
            });
        });
    }

    getPathRequests() {
        console.log(JSON.stringify(this.pathRequest));
        return this.pathRequest;
    }

    resetPaths() {
        console.log("Clicked");
        this.pathRequest = new PathRequest();
        this.draw(true);
    }

    // ----- MAP ZOOM & TRANSLATE -----

    translateMap(trans) {
        this.translateFromTo({ x: 0, y: 0 }, trans);
    }

    translateFromTo(from, to) {
        var diff = { x: to.x - from.x, y: to.y - from.y };
        var trans = this.translateBounded(diff);

        this.context.translate(trans.x, trans.y);

        // prevents scaling from putting map out of bouds
        var topLeft = this.context.transformedPoint(0, 0);
        this.context.translate(Math.min(topLeft.x, 0), Math.min(topLeft.y, 0));
        this.draw(true); // TODO use callback instead ?
    }

    translateBounded(trans) {
        var topLeftBound = this.context.transformedPoint(0, 0);
        var botRightBound = this.context.transformedPoint(this.canvas[0].width, this.canvas[0].height);
        var transBound = {
            xPos: topLeftBound.x,
            yPos: topLeftBound.y,
            xNeg: this.width - botRightBound.x,
            yNeg: this.height - botRightBound.y
        }
        var boundedX = trans.x < 0 ? - Math.min(transBound.xNeg, -trans.x) : Math.min(transBound.xPos, trans.x);
        var boundedY = trans.y < 0 ? - Math.min(transBound.yNeg, -trans.y) : Math.min(transBound.yPos, trans.y);
        return { x: boundedX, y: boundedY };
    }

    zoomTo(value, center = this.getCenterCoords()) {
        return this.zoomDelta(value - this.zoomLevel, center)
    }

    zoomDelta(diff, center) {
        var newLevel = (diff > 0) ? Math.min(this.zoomLevel + diff, zoomMax) : Math.max(this.zoomLevel + diff, zoomMin);
        var trueDiff = newLevel - this.zoomLevel;
        this.zoomLevel = newLevel;

        var scaleFactor = Math.pow(this.width, 1.0 / (zoomMax - zoomMin));
        var factor = Math.pow(scaleFactor, trueDiff);

        this.context.translate(center.x, center.y); // resets origin so that it scales from center
        this.context.scale(factor, factor);
        this.translateMap({ x: -center.x, y: -center.y }); // takes care of re-drawing map

        console.log("Zoomed " + trueDiff + " times, current zoom: " + this.zoomLevel);
        return this.zoomLevel;
    }

    getLocalCoords(canvasX, canvasY) {
        return this.context.transformedPoint(canvasX, canvasY);
    }

    getCenterCoords() {
        return this.getLocalCoords(this.canvas[0].width / 2, this.canvas[0].height / 2);
    }

    resizeCanvas(resetTransform = false) {
        var prevCenter, prevZoom;
        if (!resetTransform) {
            prevCenter = this.getCenterCoords();
            prevZoom = this.zoomLevel;
        }
    
        this.zoomLevel = zoomMin;
        this.context.setTransform(1, 0, 0, 1, 0, 0);
        this.canvas.prop("width", canvas.innerWidth()); // maximize width to use all available pixels
        var canvasBaseScale = this.canvas[0].width * 1.0 / this.width;
        this.canvas.prop("height", this.height * canvasBaseScale); // adjust height to maintain aspect ratio
        this.context.scale(canvasBaseScale, canvasBaseScale);
    
        if (!resetTransform) {
            this.zoomTo(prevZoom, prevCenter); // modifies this.zoomLevel accordingly
            var newCenter = this.getCenterCoords();
            var newTrans = {x: newCenter.x - prevCenter.x, y: newCenter.y - prevCenter.y};
            this.translateMap(newTrans); // TODO centering is a bit off
        }
        return this.zoomLevel;
    };

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