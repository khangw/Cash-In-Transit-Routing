using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingApi.Model
{
    public class DataModel
    {
        public long[,] CostMatrix;
        public int VehicleNumber;
        public int Depot;

        public DataModel(long[,] costMatrix, int vehicleNumber, int depot)
        {
            CostMatrix = costMatrix;
            VehicleNumber = vehicleNumber;
            Depot = depot;
        }
    }
}
