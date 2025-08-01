using System;

namespace EFaturaApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }  // ileride hash yapılacak
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Telno { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
