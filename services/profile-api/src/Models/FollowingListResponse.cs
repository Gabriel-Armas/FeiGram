using System.Text.Json.Serialization;


public class FollowingListResponse
{
    [JsonPropertyName("followingIds")]
    public string[] FollowingIds { get; set; } = [];
}