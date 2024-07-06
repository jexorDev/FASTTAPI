
namespace FASTTAPI.DataLayer.DataTransferObjects
{
    public class PaxVolumeHour 
    {
        public int Hour { get; set; }
        public int ArrivingPassengers { get; set; } 
        public int ArrivingFlights { get; set; } 
        public int DepartingPassengers { get; set; } 
        public int DepartingFlights { get; set; } 

    }
}
