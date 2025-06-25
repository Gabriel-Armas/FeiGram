using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeigramClient.Models
{
    class CommentListResult
    {
        public string post_id { get; set; } = string.Empty;
        public List<CommentDto> comments { get; set; } = new();
    }
}
