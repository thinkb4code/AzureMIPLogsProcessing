using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FI.APILogs.Filtering.Models
{
    internal class ClaimPeopleRecord
    {
        [JsonPropertyName("Email")]
        public string? Email { get; set; }

        [JsonPropertyName("Job Title")]
        public string? JobTitle { get; set; }

        [JsonPropertyName("LOB")]
        public string? LOB { get; set; }

        [JsonPropertyName("N-3")]
        public string? N3 { get; set; }

        [JsonPropertyName("Company")]
        public string? Company { get; set; }

        public ClaimPeopleRecord(string csvLine, long index)
        {
            var data = csvLine.Split(',');
            Email = data[0];
            JobTitle = data[1];
            LOB = data[2];

            // Fix used for reading CSV file, as the manager name is in format LastName, First Name causing index out of bound error. 
            // Hence, If statement is used to skip first like i.e. Email, LOB... Company and check if manager is blank
            if (string.IsNullOrEmpty(data[3]) || index == 0)
            {
                N3 = data[3];
                Company = data[4];
            }else
            {
                N3 = string.Concat(data[3], ",", data[4]);
                Company = data[5];
            }
        }
    }
}
