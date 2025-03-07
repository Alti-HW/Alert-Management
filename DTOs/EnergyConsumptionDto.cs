using System;
using System.Text.Json.Serialization;

namespace Alert_Management.DTOs
{
    public class EnergyConsumptionDto
    {
        [JsonPropertyName("consumption_id")]
        public int ConsumptionId { get; set; }

        [JsonPropertyName("floor_id")]
        public int FloorId { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("energy_consumed_kwh")]
        public decimal EnergyConsumedKwh { get; set; }

        [JsonPropertyName("peak_load_kw")]
        public decimal PeakLoadKw { get; set; }

        [JsonPropertyName("avg_temperature_c")]
        public decimal AvgTemperatureC { get; set; }

        [JsonPropertyName("co2_emissions_kg")]
        public decimal Co2EmissionsKg { get; set; }
    }
}
