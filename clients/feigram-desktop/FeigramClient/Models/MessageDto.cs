using System;
using System.Text.Json.Serialization;

namespace FeigramClient.Models
{
    public class MessageDto
    {
        [JsonPropertyName("message_id")]
        public int MessageId { get; set; }

        [JsonPropertyName("from")]
        public string FromUserId { get; set; } = string.Empty;

        [JsonPropertyName("to")]
        public string ToUserId { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("sent_at")]
        public DateTime SentAt { get; set; }
    }
}
