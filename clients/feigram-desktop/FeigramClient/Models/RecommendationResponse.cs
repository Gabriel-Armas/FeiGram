using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FeigramClient.Models
{
    public class RecommendationResponse
    {
        [JsonPropertyName("posts")]
        public List<PostDto> Posts { get; set; }
    }
}
