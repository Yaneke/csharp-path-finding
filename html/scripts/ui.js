"use strict";

function setWidthInPercent(element) {
    var percentageWidth = (element.width() / element.parent().width()) * 100;
    element.width(percentageWidth + '%');
    resizeCanvas();
    drawMap();
}


function resetPathTable() {
    // Remove all table headers except the plus button of the first row
    $("#agents th").next().remove();
    // Remove the corresponding sources and destinations
    $("#sources td").remove();
    $("#destinations td").remove();
    addPathColumn();
}

function addPathColumn() {
    let thead = $("#agents");
    let nagents = thead.children().length - 1;
    let colour = map.getAgentColour(nagents);
    console.log("Agent colour=", colour);
    let tdsrc = '<td> (<input class="pathRequest" id="srcx' + nagents + '" type="text" size="3">, <input class="pathRequest" id="srcy' + nagents + '" type="text" size="3">)</td >';
    let tddst = '<td> (<input class="pathRequest" id="dstx' + nagents + '" type="text" size="3">, <input class="pathRequest" id="dsty' + nagents + '" type="text" size="3">)</td >';
    thead.append('<th style="color:' + colour + '"> Agent ' + nagents + " </th>");
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
    if (res) {
        displayPathAnswer(res.pathAnswer);
        // Show the eventual conflict
        if (res.conflict) {
            let constrainedObject = res.conflict.constrainedObject;
            if (constrainedObject.i && constrainedObject.j) { // Vertex conflict
                let [x, y] = [constrainedObject.j, constrainedObject.i];
                $("#conflict").html("Vertex at t=" + res.conflict.timestep + " and coordinates (" + x + ", " + y + ")");
                context.fillStyle = "red";
                context.fillRect(x, y, 1, 1);
            } else { // Cardinal conflict
                let conflictData = getCardinalDirectionAndDelta(constrainedObject);
                $("#conflict").html("Cardinal conflict (" + conflictData.direction + ") at t=" + res.conflict.timestep);
                let v1 = res.pathAnswer.paths[0].coordinates[res.conflict.timestep];
                let v2 = res.pathAnswer.paths[1].coordinates[res.conflict.timestep];
                map.drawArrow(v1, new Coordinate(v1.x + conflictData.deltax, v1.y + conflictData.deltay), "red", 0.2);
                map.drawArrow(v2, new Coordinate(v2.x + conflictData.deltax, v2.y + conflictData.deltay), "red", 0.2);
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
            console.log(constraints[agent]);
            // let agentConstraints = constraints[agent].sort(cons => { cons.timestep });
            constraints[agent].forEach(constraint => {
                let strConstraint = "";
                if (constraint.constrainedObject == parseInt(constraint.constrainedObject)) { // Cardinal constraint
                    strConstraint = getCardinalDirectionAndDelta(constraint.constrainedObject).direction;
                }
                let row = "<tr><td>" + agent + "</td> <td>" + constraint.timestep + "</td> <td>" + strConstraint + "</td></tr>";
                table.append(row);
            });
            table.append('<tr><td></td><td></td><td></td></tr>');
        }
        table.children("tr:last").remove();
        // Display the constraints as arrows
        console.log(res.constraints);
        for (let i in res.constraints) {
            drawCardinalConstraintsAsArrows(res.pathAnswer.paths[i], res.constraints[i]);
        };
        //drawCardinalConstraintsAsArrows()
    } else {
        $("#getPathButton").prop("disabled", false);
        $("#cbsStep").prop("disabled", false);
        $("#spinner").remove();
    }
}

function drawCardinalConstraintsAsArrows(agentPath, agentConstraints) {
    console.log(agentPath);
    console.log(agentConstraints);
    agentConstraints.forEach(constraint => {
        let coord = agentPath.coordinates[constraint.timestep];
        let delta = getCardinalDirectionAndDelta(constraint.constrainedObject);
        map.drawArrow(coord, new Coordinate(coord.x + delta.deltax, coord.y + delta.deltay), "blue", 0.1);
        console.log("Constraint at t=" + constraint.timestep);
        console.log("Vertex at time t=" + agentPath.coordinates[constraint.timestep]);
    });
}

function getCardinalDirectionAndDelta(cardinalInt) {
    let res = {
        direction: null,
        deltax: 0,
        deltay: 0
    };
    switch (cardinalInt) {
        case 0:
            res.direction = "North";
            res.deltay = -1;
            break;
        case 1:
            res.direction = "South";
            res.deltay = 1;
            break;
        case 2:
            res.direction = "East";
            res.deltax = 1;
            break;
        case 3:
            res.direction = "West";
            res.deltax = -1;
            break;
    }
    return res;
}

