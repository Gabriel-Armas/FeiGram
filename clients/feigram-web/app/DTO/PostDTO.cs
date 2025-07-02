using System.Text.Json.Serialization;


namespace app.DTO
{
    public class PostDTO
    {
        [JsonPropertyName("post_id")]
        public int PostId { get; set; }

        [JsonPropertyName("id_usuario")]
        public string IdUsuario { get; set; }

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }

        [JsonPropertyName("url_media")]
        public string UrlMedia { get; set; }

        [JsonPropertyName("fechaPublicacion")]
        public DateTime FechaPublicacion { get; set; }
        [JsonPropertyName("comentarios")]
        public int Comentarios { get; set; }
        [JsonPropertyName("likes")]
        public int Likes { get; set; }
    }
}
