
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

class Adjacency {
    public Top: boolean;
    public Right: boolean;
    public Bottom: boolean;
    public Left: boolean;
    public Any: boolean;

    constructor(adj: string) {
        if (adj === "." || adj === 'T') {
            this.Any = true;
            adj = 'F'; // max hex value = fully connected (to any existing neighbours)
        } else {
            this.Any = false;
        };
        let adjBits = (parseInt(adj, 16) + 16).toString(2); // add 16 in lieu of padding
        this.Top = adjBits[1] == '1';
        this.Right = adjBits[2] == '1';
        this.Bottom = adjBits[3] == '1';
        this.Left = adjBits[4] == '1';
    }

    public toHex() {
        let res = 0;
        const bits = [this.Top, this.Right, this.Bottom, this.Left]; // order matters !
        bits.forEach(b => {
            if (b) res += 1;
            res *= 2;
        });
        return res.toString(16);
    }
};