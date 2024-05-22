namespace FASTTAPI.Models.FlightAware
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    using System;
    using System.Collections.Generic;

    public class Arrival
    {
        public string ident { get; set; }
        public string ident_icao { get; set; }
        public string ident_iata { get; set; }
        public string actual_runway_off { get; set; }
        public string actual_runway_on { get; set; }
        public string fa_flight_id { get; set; }
        public string @operator { get; set; }
        public string operator_icao { get; set; }
        public string operator_iata { get; set; }
        public string flight_number { get; set; }
        public string registration { get; set; }
        public string atc_ident { get; set; }
        public string inbound_fa_flight_id { get; set; }
        public List<string> codeshares { get; set; }
        public List<string> codeshares_iata { get; set; }
        public bool blocked { get; set; }
        public bool diverted { get; set; }
        public bool cancelled { get; set; }
        public bool position_only { get; set; }
        public Origin origin { get; set; }
        public Destination destination { get; set; }
        public int? departure_delay { get; set; }
        public int? arrival_delay { get; set; }
        public int? filed_ete { get; set; }
        public int? progress_percent { get; set; }
        public string status { get; set; }
        public string aircraft_type { get; set; }
        public int? route_distance { get; set; }
        public int? filed_airspeed { get; set; }
        public int? filed_altitude { get; set; }
        public string route { get; set; }
        public string baggage_claim { get; set; }
        public int? seats_cabin_business { get; set; }
        public int? seats_cabin_coach { get; set; }
        public int? seats_cabin_first { get; set; }
        public string gate_origin { get; set; }
        public string gate_destination { get; set; }
        public string terminal_origin { get; set; }
        public string terminal_destination { get; set; }
        public string type { get; set; }
        public DateTime? scheduled_out { get; set; }
        public DateTime? estimated_out { get; set; }
        public DateTime? actual_out { get; set; }
        public DateTime? scheduled_off { get; set; }
        public DateTime? estimated_off { get; set; }
        public DateTime? actual_off { get; set; }
        public DateTime? scheduled_on { get; set; }
        public DateTime? estimated_on { get; set; }
        public DateTime? actual_on { get; set; }
        public DateTime? scheduled_in { get; set; }
        public DateTime? estimated_in { get; set; }
        public DateTime? actual_in { get; set; }
    }

    public class Departure
    {
        public string ident { get; set; }
        public string ident_icao { get; set; }
        public string ident_iata { get; set; }
        public string actual_runway_off { get; set; }
        public string actual_runway_on { get; set; }
        public string fa_flight_id { get; set; }
        public string @operator { get; set; }
        public string operator_icao { get; set; }
        public string operator_iata { get; set; }
        public string flight_number { get; set; }
        public string registration { get; set; }
        public string atc_ident { get; set; }
        public string inbound_fa_flight_id { get; set; }
        public List<string> codeshares { get; set; }
        public List<string> codeshares_iata { get; set; }
        public bool blocked { get; set; }
        public bool diverted { get; set; }
        public bool cancelled { get; set; }
        public bool position_only { get; set; }
        public Origin origin { get; set; }
        public Destination destination { get; set; }
        public int? departure_delay { get; set; }
        public int? arrival_delay { get; set; }
        public int? filed_ete { get; set; }
        public int? progress_percent { get; set; }
        public string status { get; set; }
        public string aircraft_type { get; set; }
        public int? route_distance { get; set; }
        public int? filed_airspeed { get; set; }
        public int? filed_altitude { get; set; }
        public string route { get; set; }
        public string baggage_claim { get; set; }
        public int? seats_cabin_business { get; set; }
        public int? seats_cabin_coach { get; set; }
        public int? seats_cabin_first { get; set; }
        public string gate_origin { get; set; }
        public string gate_destination { get; set; }
        public string terminal_origin { get; set; }
        public string terminal_destination { get; set; }
        public string type { get; set; }
        public DateTime? scheduled_out { get; set; }
        public DateTime? estimated_out { get; set; }
        public DateTime? actual_out { get; set; }
        public DateTime? scheduled_off { get; set; }
        public DateTime? estimated_off { get; set; }
        public DateTime? actual_off { get; set; }
        public DateTime? scheduled_on { get; set; }
        public DateTime? estimated_on { get; set; }
        public DateTime? actual_on { get; set; }
        public DateTime? scheduled_in { get; set; }
        public DateTime? estimated_in { get; set; }
        public DateTime? actual_in { get; set; }
    }

    public class Destination
    {
        public string code { get; set; }
        public string code_icao { get; set; }
        public string code_iata { get; set; }
        public string code_lid { get; set; }
        public string timezone { get; set; }
        public string name { get; set; }
        public string city { get; set; }
        public string airport_info_url { get; set; }
    }

    public class Links
    {
        public string next { get; set; }
    }

    public class Origin
    {
        public string code { get; set; }
        public string code_icao { get; set; }
        public string code_iata { get; set; }
        public string code_lid { get; set; }
        public string timezone { get; set; }
        public string name { get; set; }
        public string city { get; set; }
        public string airport_info_url { get; set; }
    }

    public class FlightAwareAirportFlightsResponseObject
    {
        public Links links { get; set; }
        public int? num_pages { get; set; }
        public List<ScheduledArrival> scheduled_arrivals { get; set; }
        public List<ScheduledDeparture> scheduled_departures { get; set; }
        public List<Arrival> arrivals { get; set; }
        public List<Departure> departures { get; set; }
    }

    public class ScheduledArrival
    {
        public string ident { get; set; }
        public string ident_icao { get; set; }
        public string ident_iata { get; set; }
        public string actual_runway_off { get; set; }
        public string actual_runway_on { get; set; }
        public string fa_flight_id { get; set; }
        public string @operator { get; set; }
        public string operator_icao { get; set; }
        public string operator_iata { get; set; }
        public string flight_number { get; set; }
        public string registration { get; set; }
        public string atc_ident { get; set; }
        public string inbound_fa_flight_id { get; set; }
        public List<string> codeshares { get; set; }
        public List<string> codeshares_iata { get; set; }
        public bool blocked { get; set; }
        public bool diverted { get; set; }
        public bool cancelled { get; set; }
        public bool position_only { get; set; }
        public Origin origin { get; set; }
        public Destination destination { get; set; }
        public int? departure_delay { get; set; }
        public int? arrival_delay { get; set; }
        public int? filed_ete { get; set; }
        public int? progress_percent { get; set; }
        public string status { get; set; }
        public string aircraft_type { get; set; }
        public int? route_distance { get; set; }
        public int? filed_airspeed { get; set; }
        public int? filed_altitude { get; set; }
        public string route { get; set; }
        public string baggage_claim { get; set; }
        public int? seats_cabin_business { get; set; }
        public int? seats_cabin_coach { get; set; }
        public int? seats_cabin_first { get; set; }
        public string gate_origin { get; set; }
        public string gate_destination { get; set; }
        public string terminal_origin { get; set; }
        public string terminal_destination { get; set; }
        public string type { get; set; }
        public DateTime? scheduled_out { get; set; }
        public DateTime? estimated_out { get; set; }
        public DateTime? actual_out { get; set; }
        public DateTime? scheduled_off { get; set; }
        public DateTime? estimated_off { get; set; }
        public DateTime? actual_off { get; set; }
        public DateTime? scheduled_on { get; set; }
        public DateTime? estimated_on { get; set; }
        public DateTime? actual_on { get; set; }
        public DateTime? scheduled_in { get; set; }
        public DateTime? estimated_in { get; set; }
        public DateTime? actual_in { get; set; }
    }

    public class ScheduledDeparture
    {
        public string ident { get; set; }
        public string ident_icao { get; set; }
        public string ident_iata { get; set; }
        public string actual_runway_off { get; set; }
        public string actual_runway_on { get; set; }
        public string fa_flight_id { get; set; }
        public string @operator { get; set; }
        public string operator_icao { get; set; }
        public string operator_iata { get; set; }
        public string flight_number { get; set; }
        public string registration { get; set; }
        public string atc_ident { get; set; }
        public string inbound_fa_flight_id { get; set; }
        public List<string> codeshares { get; set; }
        public List<string> codeshares_iata { get; set; }
        public bool blocked { get; set; }
        public bool diverted { get; set; }
        public bool cancelled { get; set; }
        public bool position_only { get; set; }
        public Origin origin { get; set; }
        public Destination destination { get; set; }
        public int? departure_delay { get; set; }
        public int? arrival_delay { get; set; }
        public int? filed_ete { get; set; }
        public int? progress_percent { get; set; }
        public string status { get; set; }
        public string aircraft_type { get; set; }
        public int? route_distance { get; set; }
        public int? filed_airspeed { get; set; }
        public int? filed_altitude { get; set; }
        public string route { get; set; }
        public string baggage_claim { get; set; }
        public int? seats_cabin_business { get; set; }
        public int? seats_cabin_coach { get; set; }
        public int? seats_cabin_first { get; set; }
        public string gate_origin { get; set; }
        public string gate_destination { get; set; }
        public string terminal_origin { get; set; }
        public string terminal_destination { get; set; }
        public string type { get; set; }
        public DateTime? scheduled_out { get; set; }
        public DateTime? estimated_out { get; set; }
        public DateTime? actual_out { get; set; }
        public DateTime? scheduled_off { get; set; }
        public DateTime? estimated_off { get; set; }
        public DateTime? actual_off { get; set; }
        public DateTime? scheduled_on { get; set; }
        public DateTime? estimated_on { get; set; }
        public DateTime? actual_on { get; set; }
        public DateTime? scheduled_in { get; set; }
        public DateTime? estimated_in { get; set; }
        public DateTime? actual_in { get; set; }
    }


}
