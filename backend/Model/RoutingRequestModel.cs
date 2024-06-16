namespace RoutingApi.Model
{
    public class RoutingRequestModel
    {
        public List<List<long>> Costs { get; set; }
        public long TimeLimit { get; set; }
        public int Depot { get; set; }
    }
}
