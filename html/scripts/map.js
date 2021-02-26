
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

    isDrawn() {
        return this.image == null;
    }

    reset() {
        this.width = 0;
        this.height = 0;
        this.data = null;
        this.image = null;
        this.pathRequest = new PathRequest();
        this.pathAnswers = new Array();
        this.pathColours = new Array();
    }

    addPath(index, start, end) {
        this.pathRequest.addPath(index, new Coordinate(start.x, start.y), new Coordinate(end.x, end.y));
        this.draw(true);
    }


    draw(force = false) {
        if (force || this.image == null) {
            // Save and reastore context for custom transforms.
            context.save();
            context.setTransform(1, 0, 0, 1, 0, 0);
            context.clearRect(0, 0, canvas[0].width, canvas[0].height);
            context.restore();
            // Map obstacles
            context.fillStyle = "black";
            for (let i = 0; i < map.height; i++) {
                for (var j = 0; j < map.width; j++) {
                    if (map.data[i][j] != ".") {
                        context.fillRect(j, i, 1, 1);
                    }
                }
            }
            // Path answers
            for (let i = 0; i < this.pathAnswers.length; i++) {
                context.fillStyle = this.pathColours[i];
                this.pathAnswers[i].coordinates.forEach(coord => {
                    context.fillRect(coord.x, coord.y, 1, 1);
                });
            }
            // Path requests
            for (let i = 0; i < this.pathRequest.length(); i++) {
                this.drawArrow(this.pathRequest.start[i], this.pathRequest.end[i], this.pathColours[i]);
            }
            this.image = context.getImageData(0, 0, canvas[0].width, canvas[0].height);
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
    drawArrow(start, stop, colour = null, width = 0.5) {
        let headlen = 2.5 * width; // length of head in cells
        context.lineWidth = 0.5;
        if (!colour) {
            context.strokeStyle = this.getAgentColour(this.pathRequest.length());
        } else {
            context.strokeStyle = colour;
        }
        context.lineCap = "round";
        context.lineJoin = "round";
        context.lineWidth = width;

        let fromX = start.x + 0.5;
        let fromY = start.y + 0.5;
        let toX = stop.x + 0.5;
        let toY = stop.y + 0.5;
        let dx = toX - fromX;
        let dy = toY - fromY;
        let angle = Math.atan2(dy, dx);
        context.beginPath();
        context.moveTo(fromX, fromY);
        context.lineTo(toX, toY);
        context.lineTo(toX - headlen * Math.cos(angle - Math.PI / 6), toY - headlen * Math.sin(angle - Math.PI / 6));
        context.moveTo(toX, toY);
        context.lineTo(toX - headlen * Math.cos(angle + Math.PI / 6), toY - headlen * Math.sin(angle + Math.PI / 6));
        context.closePath();
        context.stroke();
    }


    setPathAnswer(pathAnswer) {
        this.pathAnswers = pathAnswer.paths;
    }


    getPathRequests() {
        return this.pathRequest;
    }

    getAgentColour(agentNum) {
        while (this.pathColours.length <= agentNum) {
            this.pathColours.push(getRandomColour());
        }
        return this.pathColours[agentNum];
    }

    resetPaths() {
        this.pathRequest = new PathRequest();
        this.pathColours = new Array();
        this.pathAnswers = new Array();
        this.draw(true);
    }

}