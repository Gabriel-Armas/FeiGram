using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeigramClient.Models
{
    public class ProfileSingleton
    {
        public string Id { get; set; }
        public string? Photo { get; set; }
        public string? Name { get; set; }
        public string? Sex { get; set; }
        public string? Email { get; set; }
        public int? FollowerCount { get; set; }
        public string? Token { get; set; }
        public string? Role { get; set; }


    }
}
