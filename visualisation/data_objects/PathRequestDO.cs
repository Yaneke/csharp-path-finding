

namespace visualisation.data_objects {
    class PathRequestDO {
        public CoordinateDO start { get; set; }
        public CoordinateDO end { get; set; }

        public override string ToString() {
            return "start: " + this.start.ToString() + ", end = " + this.end.ToString();
        }
    }
}