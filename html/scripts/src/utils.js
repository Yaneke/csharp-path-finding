
function getRandomColour() {
    // TODO: Prevent colours too light for readability
    // TODO: Prevent colours too dark for confusion with borders
    let letters = '0123456789ABCDEF';
    let color = '#';
    for (var i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
    }
    return color;
}

function getComplementaryColour(colour) {
    let hex = parseInt(colour.substring(1), 16);
    let max = parseInt("FFFFFF", 16);
    return "#"+(max-hex).toString(16);
}