using System.Text.Json.Serialization;

public class FollowerCountResponse
{
    [JsonPropertyName("profileId")]
    public string ProfileId { get; set; } = default!;
    [JsonPropertyName("followerCount")]
    public int FollowerCount { get; set; }
}