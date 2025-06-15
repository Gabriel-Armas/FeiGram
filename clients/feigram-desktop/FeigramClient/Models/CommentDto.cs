using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeigramClient.Models
{
    class CommentDto
    {
        public string post_id { get; set; } = string.Empty;
        public string user_id { get; set; } = string.Empty;
        public string text_comment { get; set; } = string.Empty;
        public DateTime created_at { get; set; }
    }
}
