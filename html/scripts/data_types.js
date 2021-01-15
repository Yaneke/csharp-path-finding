class Coordinate {
    /**
     * @param {Number} x 
     * @param {Number} y 
     */
    constructor(x = NaN, y = NaN) {
        this.x = x;
        this.y = y;
    }

    to_ij() {
        return new Coordinate(this.y, this.x);
    }
}


class PathRequest {
    /**
     * @param {Coordinate} start 
     * @param {Coordinate} end 
     */
    constructor(start, end) {
        this.start = start;
        this.end = end;
    }
}


class Path {
    /**
     * @param {List<Coordinate>} coordinates 
     * @param {Number} cost 
     */
    constructor(coordinates, cost) {
        this.coordinates = coordinates;
        this.cost = cost;
    }
}

class GraphMap {
    constructor(width = 0, height = 0, data = null, image = null) {
        this.width = parseInt(width);
        this.height = parseInt(height)
        this.data = data;
        this.image = image;
    }

    setWidth(width) {
        this.width = parseInt(width);
    }

    setHeight(height) {
        this.height = parseInt(height);
    }

    resetImage() {
        this.image = null;
    }

    isDrawn() {
        return this.image == null;
    }

}