"use strict";

function setWidthInPercent(element) {
    var percentageWidth = (element.width() / element.parent().width()) * 100;
    element.width(percentageWidth + '%');
    resizeCanvas();
    drawMap();
}


function resetPathTable() {
    addPathColumn();
    // Remove all table headers except the plus button and the first agent
    $("#agents th").next().next().remove();
    // Remove the corresponding sources and destinations
    $("#sources td").next().remove();
    $("#destinations td").next().remove();
}

function addPathColumn() {
    let thead = $("#agents");
    let nagents = thead.children().length - 1;
    let tdsrc = '<td> (<input class="pathRequest" id="srcx' + nagents + '" type="text" size="3">, <input class="pathRequest" id="srcy' + nagents + '" type="text" size="3">)</td >';
    let tddst = '<td> (<input class="pathRequest" id="dstx' + nagents + '" type="text" size="3">, <input class="pathRequest" id="dsty' + nagents + '" type="text" size="3">)</td >';
    thead.append("<th> Agent " + nagents + " </th>");
    $("#sources").append(tdsrc);
    $("#destinations").append(tddst);
}


function displayPathAnswer(pathAnswer) {
    map.setPathAnswer(pathAnswer);
    map.draw(true);
    $("#computationTime").val(pathAnswer.duration);
    $("#solutionCost").val(pathAnswer.cost);
    let tbody = $("#result_tbody");
    tbody.html("");
    let i = 0;
    pathAnswer.paths.forEach(path => {
        let coordPath = "";
        path.coordinates.forEach(coord => {
            coordPath += "(" + coord.x + ", " + coord.y + ")-> ";
        });
        let row = "<tr> <td> " + i++ + "</td> <td> " + path.coordinates.length + "</td> <td>" + coordPath + "</td> </tr>";
        tbody.append(row);
    });
    $("#getPathButton").prop("disabled", false);
    $("#cbsStep").prop("disabled", false);
    $("#spinner").remove();
}

function displayCBSStep(res) {
    console.log(res);
    if (res) {
        displayPathAnswer(res.pathAnswer);
        // Show the eventual conflict
        if (res.conflict) {
            let constrainedObject = res.conflict.constrainedObject;
            if (constrainedObject.i && constrainedObject.j) { // Vertex
                let [x, y] = [constrainedObject.j, constrainedObject.i];
                $("#conflict").html("Vertex conflict at t=" + res.conflict.timestep + " and coordinates (" + x + ", " + y + ")");
                context.fillStyle = "red";
                context.fillRect(x, y, 1, 1);
            } else { // Cardinal direction
                let direction, xdiff = 0, ydiff = 0;
                switch (constrainedObject) {
                    case 0: direction = "North"; ydiff = -1;
                        break;
                    case 1: direction = "South"; ydiff = 1;
                        break;
                    case 2: direction = "East"; xdiff = 1;
                        break;
                    case 3: direction = "West"; xdiff = -1;
                        break;
                    default:
                        direction = "Unknown";
                }
                $("#conflict").html("Cardinal conflict (" + direction + ") at t=" + res.conflict.timestep);
                let v1 = res.pathAnswer.paths[0].coordinates[res.conflict.timestep];
                let v2 = res.pathAnswer.paths[1].coordinates[res.conflict.timestep];
                map.drawArrow(v1, new Coordinate(v1.x + xdiff, v1.y + ydiff), "red");
                map.drawArrow(v2, new Coordinate(v2.x + xdiff, v2.y + ydiff), "red");
            }
            $("#conflict").css("color", "red");
        } else {
            $("#conflict").html("None");
            $("#conflict").css("color", "black");
        }
        // show the current constraints
        let constraints = res.constraints;
        let table = $("#constraints_tbody");
        table.html("");
        for (let agent in constraints) {
            constraints[agent].forEach(constraint => {
                let row = "<td>" + agent + "</td> <td>" + constraint.timestep + "</td> <td>" + JSON.stringify(constraint.constrainedObject) + "</td>";
                table.append(row);
            });
        }
    } else {
        $("#getPathButton").prop("disabled", false);
        $("#cbsStep").prop("disabled", false);
        $("#spinner").remove();
    }
}