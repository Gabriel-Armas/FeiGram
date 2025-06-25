using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FeigramClient.Models
{
    class Profile
    {
        public required string Id { get; set; }
        public string? Photo { get; set; }
        public string? Name { get; set; }
        public string? Sex { get; set; }
        [JsonPropertyName("enrollment")]
        public string? Tuition { get; set; }
        public int? FollowerCount { get; set; }
    }
}
