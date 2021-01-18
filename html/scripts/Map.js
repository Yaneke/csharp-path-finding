
COLOURS = ["green", "red", "blue"];

class GraphMap {
    constructor(context) {
        this.reset();
        this.context = context;
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
            // Save and reastore context for custom transforms.
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
            for (var i = 0; i < this.pathRequest.length(); i++) {
                this.drawArrow(this.pathRequest.start[i], this.pathRequest.end[i], COLOURS[i]);
            }
            this.image = context.getImageData(0, 0, canvas[0].width, canvas[0].height);
            this.image.data.set(new Uint8ClampedArray(graphMap.imageData.data));
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
        context.lineWidth = 0.5;
        if (colour == null) {
            context.strokeStyle = COLOURS[this.pathRequest.length()];
        } else {
            context.strokeStyle = colour;
        }
        context.lineCap = "round";
        context.lineJoin = "round";

        var fromX = start.x + 0.5;
        var fromY = start.y + 0.5;
        var toX = stop.x + 0.5;
        var toY = stop.y + 0.5;
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


    drawPathAnswer(pathAnswer) {
        var i = 0;
        pathAnswer.paths.forEach(function (path) {
            context.fillStyle = COLOURS[i++ % COLOURS.length];
            path.coordinates.forEach(function (coord) {
                context.fillRect(coord.x, coord.y, 1, 1);
            });
        });
    }


    getPathRequests() {
        console.log(JSON.stringify(this.pathRequest));
        return this.pathRequest;
    }

}