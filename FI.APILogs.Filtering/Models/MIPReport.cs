using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FI.APILogs.Filtering.Models
{
    internal class MIPReport
    {
        [JsonPropertyName("Date")]
        public DateTime? Date { get; set; }

        [JsonPropertyName("Sender")]
        public string? Sender { get; set; }

        [JsonPropertyName("Receivers")]
        public List<string>? Receivers { get; set; }

        [JsonPropertyName("Subject")]
        public string? Subject { get; set; }

        [JsonPropertyName("Label Name")]
        public string? LabelName { get; set; }

        [JsonPropertyName("Job Title")]
        public string? JobTitle { get; set; }

        [JsonPropertyName("LOB")]
        public string? LOB { get; set; }

        [JsonPropertyName("Sender Manager")]
        public string? SenderManager { get; set; }

        [JsonPropertyName("Company")]
        public string? Company { get; set; }
    }
}
