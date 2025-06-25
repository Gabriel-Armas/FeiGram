using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FeigramClient.Models
{
    public class FullUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Lasname { get; set; }
        public string Tuition { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Photo { get; set; }
        public string? Role { get; set; }
    }
}
