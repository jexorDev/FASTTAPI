using Newtonsoft.Json;

namespace FASTTAPI.Models
{
    public class Airline
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "iata_code")]
        public string IataCode { get; set; }
        [JsonProperty(PropertyName = "icao_code")]
        public string IcaoCode { get; set; }
        [JsonProperty(PropertyName = "ticket_counter_opens_hours_prior_departure")]
        public decimal TicketCounterOpensHourPriorDeparture { get; set; }
        [JsonProperty(PropertyName = "ticket_counter_closes_hours_prior_departure")]
        public decimal TicketCounterClosesHourPriorDeparture { get; set; }
        [JsonProperty(PropertyName = "common_problems")]
        public List<AirlineProblem> CommonProblems { get; set; }

    }

    public class AirlineProblem
    {
        public string Description { get; set; }
        public string Solution { get; set; }
    }
}
