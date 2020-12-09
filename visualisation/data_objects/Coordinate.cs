

namespace visualisation.data_objects {
    public class Coordinate {
        public int x { get; set; }
        public int y { get; set; }

        public Coordinate(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public override string ToString() {
            return "(" + x + ", " + y + ")";
        }
    }
}