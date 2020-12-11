using graph;
using game;
using System.Collections.Generic;

namespace search {
    public class VertexConflict {
        public Vertex vertex { get; }
        public int timestep { get; }
        public Agent agent1 { get; }
        public Agent agent2 { get; }

        public VertexConflict(Vertex vertex, int timestep, Agent agent1, Agent agent2) {
            this.vertex = vertex;
            this.timestep = timestep;
            this.agent1 = agent1;
            this.agent2 = agent2;
        }
    }

    public class EdgeConflict {
        public Edge edge { get; }
        public int timestep { get; }
        public Agent agent1 { get; }
        public Agent agent2 { get; }

        public EdgeConflict(Edge edge, int timestep, Agent agent1, Agent agent2) {
            this.edge = edge;
            this.timestep = timestep;
            this.agent1 = agent1;
            this.agent2 = agent2;
        }
    }
}