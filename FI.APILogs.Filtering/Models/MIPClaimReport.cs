using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FI.APILogs.Filtering.Models
{
    internal class MIPClaimReport
    {
        [JsonPropertyName("CreationTime")]
        public DateTime? CreationTime { get; set; }

        [JsonPropertyName("ItemName")]
        public string? ItemName { get; set; }

        [JsonPropertyName("LabelName")]
        public string? LabelName { get; set; }

        [JsonPropertyName("Receivers")]
        public List<string>? Receivers { get; set; }

        [JsonPropertyName("Sender")]
        public string? Sender { get; set; }

        [JsonPropertyName("Job Title")]
        public string? JobTitle { get; set; }

        [JsonPropertyName("LOB")]
        public string? LOB { get; set; }

        [JsonPropertyName("N-3")]
        public string? N3 { get; set; }

        [JsonPropertyName("Company")]
        public string? Company { get; set; }
    }
}
