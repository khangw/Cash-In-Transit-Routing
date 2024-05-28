using Google.OrTools.ConstraintSolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingApi.Model
{
    public class Routing
    {
        public DataModel data;
        public RoutingModel model;
        public RoutingIndexManager manager;
        public Assignment solution;

        public Routing(DataModel data, RoutingModel model, RoutingIndexManager manager, Assignment solution)
        {
            this.data = data;
            this.model = model;
            this.manager = manager;
            this.solution = solution;
        }
    }
}
