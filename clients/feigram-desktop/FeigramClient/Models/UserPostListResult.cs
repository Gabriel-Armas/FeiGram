using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeigramClient.Models
{
    public class UserPostListResult
    {
        public List<UserPostDto> Items { get; set; } = new();
    }
}
