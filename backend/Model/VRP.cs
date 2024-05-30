using Google.OrTools.ConstraintSolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingApi.Model
{
    public class VRP
    {

        public static List<List<int>> GetRoutingSolution(Routing routing)
        {
            List<List<int>> res = new List<List<int>>();
            if (routing.solution == null)
            {
                return res;
            }
            for (int i = 0; i < routing.data.VehicleNumber; ++i)
            {
                List<int> routeIndex = new List<int>();

                long routeCost = 0;
                var index = routing.model.Start(i);
                while (routing.model.IsEnd(index) == false)
                {
                    routeIndex.Add(routing.manager.IndexToNode((int)index));
                    
                    var previousIndex = index;
                    index = routing.solution.Value(routing.model.NextVar(index));
                    routeCost += routing.model.GetArcCostForVehicle(previousIndex, index, 0);
                }

                routeIndex.Add(routing.manager.IndexToNode((int)index));

                if (routeIndex.Count > 2)
                    res.Add(routeIndex);

            }
            return res;
        }

        public static string GetSolutionString(Routing routing)
        {
            if (routing.solution == null)
            {
                return "No solution";
            }

            StringBuilder result = new StringBuilder();
            //result.AppendLine($"Objective {routing.solution.ObjectiveValue()}:");

            long maxRouteCost = 0;
            for (int i = 0; i < routing.data.VehicleNumber; ++i)
            {
                result.AppendLine($"Route for Vehicle {i}:");
                long routeCost = 0;
                var index = routing.model.Start(i);
                while (!routing.model.IsEnd(index))
                {
                    result.Append($"{routing.manager.IndexToNode((int)index)} -> ");
                    var previousIndex = index;
                    index = routing.solution.Value(routing.model.NextVar(index));
                    routeCost += routing.model.GetArcCostForVehicle(previousIndex, index, 0);
                }
                result.AppendLine($"{routing.manager.IndexToNode((int)index)}");
                result.AppendLine($"Time of the route: {routeCost}min");
                maxRouteCost = Math.Max(routeCost, maxRouteCost);
            }
            result.AppendLine($"Longest time of the routes: {maxRouteCost}min");

            return result.ToString();
        }
        public static void PrintSolution(Routing routing)
        {
            if (routing.solution == null)
            {
                Console.WriteLine("No solution");
                return;
            }
            Console.WriteLine($"Objective {routing.solution.ObjectiveValue()}:");

            // Inspect solution.
            long maxRouteCost = 0;
            for (int i = 0; i < routing.data.VehicleNumber; ++i)
            {
                Console.WriteLine("Route for Vehicle {0}:", i);
                long routeCost = 0;
                var index = routing.model.Start(i);
                while (routing.model.IsEnd(index) == false)
                {
                    Console.Write("{0} -> ", routing.manager.IndexToNode((int)index));
                    var previousIndex = index;
                    index = routing.solution.Value(routing.model.NextVar(index));
                    routeCost += routing.model.GetArcCostForVehicle(previousIndex, index, 0);
                }
                Console.WriteLine("{0}", routing.manager.IndexToNode((int)index));
                Console.WriteLine("Time of the route: {0}min", routeCost);
                maxRouteCost = Math.Max(routeCost, maxRouteCost);
            }
            Console.WriteLine("Longest time of the routes: {0}min", maxRouteCost);
        }
        public static long GetMaxRouteCost(Routing routing)
        {
            if (routing.solution == null)
            {
                //Console.WriteLine("No solution");
                return 0;
            }
            long maxRouteCost = 0;
            for (int i = 0; i < routing.data.VehicleNumber; ++i)
            {
                long routeCost = 0;
                var index = routing.model.Start(i);
                while (routing.model.IsEnd(index) == false)
                {
                    var previousIndex = index;
                    index = routing.solution.Value(routing.model.NextVar(index));
                    routeCost += routing.model.GetArcCostForVehicle(previousIndex, index, 0);
                }
                maxRouteCost = Math.Max(routeCost, maxRouteCost);
            }
            return maxRouteCost;
        }
        public static Routing GetRouting(DataModel data)
        {

            // Create Routing Index Manager
            RoutingIndexManager manager =
                new RoutingIndexManager(data.CostMatrix.GetLength(0), data.VehicleNumber, data.Depot);


            // Create Routing Model.
            RoutingModel routing = new RoutingModel(manager);

            // Create and register a transit callback.
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

            // Add Distance constraint.
            routing.AddDimension(transitCallbackIndex, 0, 3000,
                                 true, // start cumul to zero
                                 "Distance");
            RoutingDimension distanceDimension = routing.GetMutableDimension("Distance");
            distanceDimension.SetGlobalSpanCostCoefficient(100);

            // Setting first solution heuristic.
            RoutingSearchParameters searchParameters =
                operations_research_constraint_solver.DefaultRoutingSearchParameters();
            searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;

            // Solve the problem.
            Assignment solution = routing.SolveWithParameters(searchParameters);

            // Print solution on console.
            return new Routing(data, routing, manager, solution);
        }
    }
}
