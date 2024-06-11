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
        public IActionResult Solve([FromBody] List<List<long>> costs)
        {
            int depot = 0; // Assuming depot is the first point
            long timeLimit = 140;

            // Convert List<List<long>> to long[,]
            int size = costs.Count;
            long[,] costArray = new long[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    costArray[i, j] = costs[i][j] / 60; //Convert to minutes
                }
            }

            //string solution = StringSolution(costArray, depot, timeLimit);
            //return Ok(solution);
            List<List<int>> routeSolution = RouteSolution(costArray, depot, timeLimit);
            Console.WriteLine(routeSolution);
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

        private string StringSolution(long[,] costs, int depot, long timeLimit = 1000)
        {
            int maxVehicle = costs.GetLength(0);
            for (int vehicle = 1; vehicle <= maxVehicle; vehicle++)
            {
                Console.WriteLine(vehicle);
                DataModel tempData = new DataModel(costs, vehicle, depot);
                Routing routing = VRP.GetRouting(tempData);
                if (VRP.GetMaxRouteCost(routing) <= timeLimit)
                {
                    return VRP.GetSolutionString(routing);
                }
            }
            return "No solution, time limit not available";
        }

        //private long[,] ConvertSecondMatrixToMinuteMatrix(long[,] costMatrix)
        //{
        //    // Get the dimensions of the input matrix
        //    int rows = costMatrix.GetLength(0);
        //    int cols = costMatrix.GetLength(1);

        //    // Create a new matrix to store the results
        //    long[,] minuteMatrix = new long[rows, cols];

        //    // Convert each element from seconds to minutes
        //    for (int i = 0; i < rows; i++)
        //    {
        //        for (int j = 0; j < cols; j++)
        //        {
        //            minuteMatrix[i, j] = costMatrix[i, j] / 60;
        //        }
        //    }

        //    return minuteMatrix;
        //}

    }
}
