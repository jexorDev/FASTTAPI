namespace FASTTAPI.DataLayer.DataTransferObjects
{
    public class Airline
    {
        public string IataCode { get; set; }
        public string IcaoCode { get; set; }    
        public string Name { get; set; }    
        public bool Hide { get; set; }
    }
}
