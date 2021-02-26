const COLOURS = ["green", "red", "blue", "cyan", "yellow", "orange"];
const zoomMin = 1, zoomMax = 20; // 1 = show whole map in canvas ... 20 = show single cell

class GridMap {
    canvas: any;
    context: CanvasRenderingContext2D;
    data: string[][];
    image: any;

    width: number;
    height: number;
    zoomLevel: number;
    agentColours: Array<string>;

    pathRequest: PathRequest;
    pathSolution: PathAnswer;

    // CONSTRUCTOR, GETTERS, SETTERS, RESETTERS

    constructor(canvas) {
        this.reset();
        this.zoomLevel = zoomMin;
        this.canvas = canvas;
        this.context = canvas[0].getContext("2d");
        this.agentColours = new Array();
        trackTransforms(this.context);
    }

    public reset() {
        this.width = 0;
        this.height = 0;
        this.data = null;
        this.image = null;
        this.pathRequest = new PathRequest();
        this.pathSolution = new PathAnswer();
    }

    public resetPaths() {
        this.pathRequest = new PathRequest();
        this.pathSolution = new PathAnswer();
        this.draw(true); // forces re-drawing so as to remove previous paths
    }

    public setWidth(width: string) {
        this.width = parseInt(width);
    }

    public setHeight(height: string) {
        this.height = parseInt(height);
    }

    public setData(data: string[][]) {
        this.data = data;
    }

    public getPathRequests() {
        return this.pathRequest;
    }

    public addArrow(start: Coordinate, end: Coordinate) {
        this.pathRequest.addRequest(new Coordinate(start.x, start.y), new Coordinate(end.x, end.y));
        this.draw(true);
    }

    public setPathAnswer(pathAnswer: PathAnswer) {
        this.pathSolution = pathAnswer;
        this.draw(true);
    }
    public getAgentColour(agentNum: number) {
        while (this.agentColours.length <= agentNum) {
            this.agentColours.push(getRandomColour());
        }
        return this.agentColours[agentNum];
    }

    // DRAWING FUNCTIONALITIES

    private isDrawn() {
        return this.image != null;
    }

    /**
     * Ensures all current displayable data is drawn on the canvas.
     * This includes the map grid (obstacles and free space), arrows and paths.
     * @param force True if forced re-drawing from this.data, false otherwise
     */
    public draw(force = false) {
        // For first drawings and forced re-drawings, parse the map again
        if (force || !this.isDrawn()) {
            // Save context, clear canvas, restore context (keeps transforms).
            this.context.save();
            this.context.setTransform(1, 0, 0, 1, 0, 0);
            this.context.clearRect(0, 0, this.canvas[0].width, this.canvas[0].height);
            this.context.restore();

            // Draws map boundaries and obstacles
            this.context.fillStyle = "black";
            for (let i = 0; i < this.height; i++) {
                for (let j = 0; j < this.width; j++) {
                    if (this.data[i][j] != ".") {
                        this.context.fillRect(j, i, 1, 1);
                    }
                }
            }

            // Draws the answer paths, if any
            let i = 0;
            this.pathSolution.paths.forEach(path => {
                console.log("Colour for agent " + i + ": " + this.agentColours[i]);
                this.drawPath(path, this.agentColours[i], i);
                i++;
            });

            // Draws (source -> goal) arrows for each path that doesn't have an answer
            let colourIndex = 0;
            for (let j = i; j < this.pathRequest.length(); j++) {
                this.drawArrow(this.pathRequest.start[j], this.pathRequest.end[j], this.agentColours[colourIndex]);
                colourIndex++;
            }

            // Draw map grid
            this.context.fillStyle = "black";
            this.context.globalCompositeOperation = "source-over";
            var gridWidth = 1 / 100;
            for (var x = 1; x < this.width; x++) {
                this.context.fillRect(x - gridWidth / 2, 0, gridWidth, this.height);
            }
            for (var y = 1; y < this.height; y++) {
                this.context.fillRect(0, y - gridWidth / 2, this.width, gridWidth);
            }

            // Stores the image data for faster re-drawing
            this.image = this.context.getImageData(0, 0, this.canvas[0].width, this.canvas[0].height);
            this.image.data.set(new Uint8ClampedArray(this.image.data));
        } else {
            this.context.putImageData(this.image, 0, 0);
        }
    }

    /**
     * Draws a temporary arrow from one grid coordinate to another.
     * The arrow disappears on next re-draw.
     * @param {Coordinate} from Coordinates of the arrow base (source)
     * @param {Coordinate} to Coordinates of the arrow head (goal)
     */
    public drawArrow(from: Coordinate, to: Coordinate, colour: string = null, width: number = 0.5) {
        let headlen = 2.5 * width; // length of head in cells
        this.context.lineWidth = width; // width of arrow (in cells)
        if (!colour) {
            this.context.strokeStyle = this.getAgentColour(this.pathRequest.length());
        } else {
            this.context.strokeStyle = colour;
        }
        this.context.lineCap = "round";
        this.context.lineJoin = "round";
        this.context.globalCompositeOperation = "hue";

        var fromX = from.x + 0.5;
        var fromY = from.y + 0.5;
        var toX = to.x + 0.5;
        var toY = to.y + 0.5;
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


    /**
     * Draws a sequence of grid coordinates in a given color, forming a path.
     * The path disappears on the next re-draw.
     * @param {Path} path The sequence of grid coordinates to draw.
     */
    public drawPath(path: Path, colour: string, offset: number, offsetMax = this.pathSolution.paths.length) {
        if (path.coordinates.length == 0) {
            return;
        };
        this.context.lineWidth = 1 / offsetMax;
        this.context.strokeStyle = colour;
        this.context.lineCap = "round";
        // this.context.lineJoin = "round";
        this.context.globalCompositeOperation = "hue";
        var off = offset / offsetMax + 0.5 * 1 / offsetMax;
        let coord = path.coordinates[0];
        this.context.beginPath()
        this.context.moveTo(coord.x + off, coord.y + off);
        for (var i = 1; i < path.coordinates.length; i++) {
            coord = path.coordinates[i];
            this.context.lineTo(coord.x + off, coord.y + off);
        }
        this.context.stroke();
        this.context.closePath();
    }

    // ----- MAP ZOOM & TRANSLATE -----

    private translateMap(trans: Coordinate) {
        this.translateFromTo(new Coordinate(0, 0), trans);
    }

    /**
     * Performs vector translation on the map, without moving its outer bounds into the canvas
     * (the translation is bounded according to its current zoom level and position in canvas).
     * @param from The initial position of the translation, in (canvas) pixels
     * @param to The final position of the translation, in (canvas) pixels
     */
    public translateFromTo(from: Coordinate, to: Coordinate) {
        var diff = new Coordinate(to.x - from.x, to.y - from.y);
        var trans = this.boundTranslation(diff); // prevents scaling from putting map out of bouds

        this.context.translate(trans.x, trans.y);

        var topLeft = this.toGridCoords(0, 0);
        this.context.translate(Math.min(topLeft.x, 0), Math.min(topLeft.y, 0));
        this.draw(true); // TODO use callback instead ?
    }

    private boundTranslation(trans: Coordinate): Coordinate {
        var topLeftBound = this.toGridCoords(0, 0);
        var botRightBound = this.toGridCoords(this.canvas[0].width, this.canvas[0].height);
        var transBound = {
            xPos: topLeftBound.x,
            yPos: topLeftBound.y,
            xNeg: this.width - botRightBound.x,
            yNeg: this.height - botRightBound.y
        }
        var boundedX = trans.x < 0 ? - Math.min(transBound.xNeg, -trans.x) : Math.min(transBound.xPos, trans.x);
        var boundedY = trans.y < 0 ? - Math.min(transBound.yNeg, -trans.y) : Math.min(transBound.yPos, trans.y);
        return new Coordinate(boundedX, boundedY);
    }

    /**
     * Sets the zoom level while maintaining a given point fixed in the canvas.
     * @param value Desired zoom level 
     * @param fixed Fixed point to keep in place relative to canvas
     * @returns The actual new zoom level (in range zoomMin, zoomMax)
     */
    public zoomTo(value: number, fixed: Coordinate = this.getCenterCoords()) {
        return this.zoomDelta(value - this.zoomLevel, fixed)
    }

    /**
     * Modifies the zoom by some delta while maintaining a given point fixed in the canvas.
     * @param diff Desired zoom difference with previous level
     * @param fixed Fixed point to keep in place relative to canvas
     * @returns The actual new zoom level (in range zoomMin, zoomMax)
     */
    public zoomDelta(diff: number, fixed: Coordinate) {
        var newLevel = (diff > 0) ? Math.min(this.zoomLevel + diff, zoomMax) : Math.max(this.zoomLevel + diff, zoomMin);
        var trueDiff = newLevel - this.zoomLevel;
        this.zoomLevel = newLevel;

        var scaleFactor = Math.pow(this.width, 1.0 / (zoomMax - zoomMin));
        var factor = Math.pow(scaleFactor, trueDiff);

        this.context.translate(fixed.x, fixed.y); // resets origin so that it scales from center
        this.context.scale(factor, factor);
        this.translateMap(new Coordinate(-fixed.x, -fixed.y)); // takes care of re-drawing map

        console.log("Zoomed " + trueDiff + " times, current zoom: " + this.zoomLevel);
        return this.zoomLevel;
    }

    /**
     * Transforms canvas coordinates (in pixels from the top left) to grid coordinates 
     * (in cells from the top left), accounting for all current transformations.
     * For smoothness of map manipulations, floating point cell coordinates are used
     * (for the cell's coordinates, truncate x and y values).
     * @param canvasX The horizontal offset in pixels from the left of the canvas
     * @param canvasY The vertical offset in pixels from the top of the canvas
     */
    public toGridCoords(canvasX: number, canvasY: number): Coordinate {
        // Use the string notation to access the function because TypeScript
        // does not recognize "transformedPoint" as a property of "CanvasRenderingContext2D"
        return this.context["transformedPoint"](canvasX, canvasY);
    }

    /**
     * Provides the grid coordinates of the point currently at the center of the canvas
     * (using cells as a unit but with floating point values to maintain precision).
     */
    public getCenterCoords(): Coordinate {
        return this.toGridCoords(this.canvas[0].width / 2, this.canvas[0].height / 2);
    }

    /**
     * Resizes canvas to use all available screen pixels. 
     * @param resetTransform If true, zoom will be reset to 1 (displaying the complete map)
     * @returns The new zoom level (in range zoomMin, zoomMax)
     */
    public resizeCanvas(resetTransform = false): number {
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
            var newTrans = new Coordinate(newCenter.x - prevCenter.x, newCenter.y - prevCenter.y);
            this.translateMap(newTrans);
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