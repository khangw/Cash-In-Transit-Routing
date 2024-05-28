using Google.OrTools.ConstraintSolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingApi.Model
{
    public class TSP
    {
        public static void PrintSolution(Routing routing)
        {
            Console.WriteLine("Objective: {0} min", routing.solution.ObjectiveValue());
            // Inspect solution.
            Console.WriteLine("Route:");
            long routeDistance = 0;
            var index = routing.model.Start(0);
            while (routing.model.IsEnd(index) == false)
            {
                Console.Write("{0} -> ", routing.manager.IndexToNode((int)index));
                var previousIndex = index;
                index = routing.solution.Value(routing.model.NextVar(index));
                routeDistance += routing.model.GetArcCostForVehicle(previousIndex, index, 0);
            }
            Console.WriteLine("{0}", routing.manager.IndexToNode((int)index));
            Console.WriteLine("Total time: {0}min", routeDistance);
        }

        public static Routing GetRouting(DataModel data)
        {
            // Create Routing Index Manager
            RoutingIndexManager manager =
                new RoutingIndexManager(data.CostMatrix.GetLength(0), data.VehicleNumber, data.Depot);

            // Create Routing Model.
            RoutingModel routing = new RoutingModel(manager);

            int transitCallbackIndex = routing.RegisterTransitCallback((long fromIndex, long toIndex) =>
            {
                // Convert from routing variable Index to
                // distance matrix NodeIndex.
                var fromNode = manager.IndexToNode(fromIndex);
                var toNode = manager.IndexToNode(toIndex);
                return data.CostMatrix[fromNode, toNode];
            });

            // Define cost of each arc.
            routing.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndex);

            // Setting first solution heuristic.
            RoutingSearchParameters searchParameters =
                operations_research_constraint_solver.DefaultRoutingSearchParameters();
            searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;

            // Solve the problem.
            Assignment solution = routing.SolveWithParameters(searchParameters);

            // Print solution on console.
            //PrintSolution(data, routing, manager, solution);
            return new Routing(data, routing, manager, solution);
        }
        public static long GetCostRouting(Routing routing)
        {
            long routeCost = 0;
            var index = routing.model.Start(0);
            while (routing.model.IsEnd(index) == false)
            {
                var previousIndex = index;
                index = routing.solution.Value(routing.model.NextVar(index));
                routeCost += routing.model.GetArcCostForVehicle(previousIndex, index, 0);
            }
            return routeCost;
        }
    }
}
