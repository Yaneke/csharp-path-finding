class Coordinate {
    /**
     * @param {Number} x 
     * @param {Number} y 
     */
    constructor(x = NaN, y = NaN) {
        this.x = typeof(x) === "string" ? parseInt(x) : x;
        this.y = typeof(y) === "string" ? parseInt(y) : y;
    }

    to_ij() {
        return new Coordinate(this.y, this.x);
    }
}

/**
 * Request for a number paths.
 */
class PathRequest {
    constructor() {
        this.start = new Array();
        this.end = new Array();
    }

    /**
     * @param {Coordinate} start
     * @param {Coordinate} end
     */
    addRequest(start, end) {
        this.start.push(start);
        this.end.push(end);
    }

    length() {
        return this.start.length;
    }
}


class PathAnswer {
    constructor() {
        this.paths = new Array();
        this.cost = 0;
    }

    static fromJson(json) {
        return Object.assign(new PathAnswer(), json);
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
