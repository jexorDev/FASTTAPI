namespace FASTTAPI.DataLayer.DataTransferObjects
{
    public class FailedFlight
    {
        public int Pk { get; set; } 
        public DateTime Timestamp { get; set; } 
        public string SerializedFlightInfo { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsFixed { get; set; }
    }
}
