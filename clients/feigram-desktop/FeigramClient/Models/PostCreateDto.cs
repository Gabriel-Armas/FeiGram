using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeigramClient.Models
{
    class PostCreateDto
    {
        public string descripcion { get; set; } = "";
        public string url_media { get; set; } = "";
        public DateTime fechaPublicacion { get; set; }
    }
}
