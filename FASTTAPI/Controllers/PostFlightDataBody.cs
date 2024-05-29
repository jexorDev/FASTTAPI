namespace FASTTAPI.Controllers
{
    public class PostFlightDataBody
    {
        public string AdminPassword { get; set; }
        public bool Arrived { get; set; }
        public bool ScheduledArriving { get; set; }
        public bool Departed { get; set; }
        public bool ScheduledDeparting { get; set; }
        public DateTime FromDateTime { get; set; }
        public DateTime ToDateTime { get; set; }
    }
}
