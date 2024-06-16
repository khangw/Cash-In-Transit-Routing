using Microsoft.AspNetCore.Mvc;
using RoutingApi.Model;
using System.Collections.Generic;

namespace RoutingApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoutingController : ControllerBase
    {
        [HttpPost]
        public IActionResult Solve([FromBody] RoutingRequestModel request)
        {
            // Extract values from request
            List<List<long>> costs = request.Costs;
            long timeLimit = request.TimeLimit;
            int depot = request.Depot;

            // Convert List<List<long>> to long[,]
            int size = costs.Count;
            long[,] costArray = new long[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    costArray[i, j] = costs[i][j] / 60; // Convert to minutes
                }
            }

            List<List<int>> routeSolution = RouteSolution(costArray, depot, timeLimit);
            return Ok(routeSolution);
        }

        private List<List<int>> RouteSolution(long[,] costs, int depot, long timeLimit = 1000)
        {
            List < List<int> > res = new List <List <int>> ();

            int maxVehicle = costs.GetLength(0);
            for (int vehicle = 1; vehicle <= maxVehicle; vehicle++)
            {
                //Console.WriteLine(vehicle);
                DataModel tempData = new DataModel(costs, vehicle, depot);
                Routing routing = VRP.GetRouting(tempData);
                if (VRP.GetMaxRouteCost(routing) <= timeLimit)
                {
                    res = VRP.GetRoutingSolution(routing);
                    return res;
                }
            }
            return res;
        }
    }
}
