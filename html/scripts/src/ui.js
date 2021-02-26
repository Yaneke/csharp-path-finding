"use strict";

function setWidthInPercent(element) {
    var percentageWidth = (element.width() / element.parent().width()) * 100;
    element.width(percentageWidth + '%');
    console.log("Width set for" + element);
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


