

namespace visualisation.data_objects {
    class PathRequest {
        public Coordinate start { get; set; }
        public Coordinate end { get; set; }

        public override string ToString() {
            return "start: " + this.start.ToString() + ", end = " + this.end.ToString();
        }
    }
}