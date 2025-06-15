using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeigramClient.Models
{
    public class Like
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string PostId { get; set; } = string.Empty;
    }
}
