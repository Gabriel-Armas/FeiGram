using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeigramClient.Models
{
    public class ProfileWithFollowerCount
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Photo { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
        public string Enrollment { get; set; } = string.Empty;
        public int FollowerCount { get; set; }
    }
}
