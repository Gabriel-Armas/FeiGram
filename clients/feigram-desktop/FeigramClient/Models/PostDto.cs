using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FeigramClient.Models
{
    public class PostDto
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
        public string Comentarios { get; set; }
        [JsonPropertyName("likes")]
        public string Likes { get; set; }
    }
}
