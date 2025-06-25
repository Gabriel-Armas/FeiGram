using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeigramClient.Models
{
    public class Comment
    {
        public int CommentId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string PostId { get; set; } = string.Empty;

        public string TextComment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();
    }
}
