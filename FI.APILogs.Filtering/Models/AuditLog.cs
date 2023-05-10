using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FI.APILogs.Filtering.Models
{
    internal class AuditLog
    {
        [JsonPropertyName("CreationTime")]
        public DateTime? CreationTime { get; set; }

        [JsonPropertyName("Id")]
        public string? Id { get; set; }

        [JsonPropertyName("Operation")]
        public string? Operation { get; set; }

        [JsonPropertyName("OrganizationId")]
        public string? OrganizationId { get; set; }

        [JsonPropertyName("RecordType")]
        public int? RecordType { get; set; }

        [JsonPropertyName("UserKey")]
        public string? UserKey { get; set; }

        [JsonPropertyName("UserType")]
        public int? UserType { get; set; }

        [JsonPropertyName("Version")]
        public int? Version { get; set; }

        [JsonPropertyName("Workload")]
        public string? Workload { get; set; }

        [JsonPropertyName("ObjectId")]
        public string? ObjectId { get; set; }

        [JsonPropertyName("UserId")]
        public string? UserId { get; set; }

        [JsonPropertyName("ApplicationMode")]
        public string? ApplicationMode { get; set; }

        [JsonPropertyName("ItemName")]
        public string? ItemName { get; set; }

        [JsonPropertyName("LabelAction")]
        public string? LabelAction { get; set; }

        [JsonPropertyName("LabelAppliedDateTime")]
        public DateTime? LabelAppliedDateTime { get; set; }

        [JsonPropertyName("LabelId")]
        public string? LabelId { get; set; }

        [JsonPropertyName("LabelName")]
        public string? LabelName { get; set; }

        [JsonPropertyName("Receivers")]
        public List<string>? Receivers { get; set; }

        [JsonPropertyName("Sender")]
        public string? Sender { get; set; }

        //[JsonPropertyName("Item")]
        //public AuditLogItem? Item { get; set; }
    }

    internal class AuditLogItem
    {
        [JsonPropertyName("Id")]
        public string? Id { get; set; }

        [JsonPropertyName("InternetMessageId")]
        public string? InternetMessageId { get; set; }

        [JsonPropertyName("IsRecord")]
        public string? IsRecord { get; set; }

        [JsonPropertyName("SizeInBytes")]
        public string? SizeInBytes { get; set; }

        [JsonPropertyName("Subject")]
        public string? Subject { get; set; }        
    }
}
