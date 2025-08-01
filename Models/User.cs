using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFaturaApp.Models
{
    [Table("USERS")] // Tablo adı
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Column("USERNAME")]
        public string Username { get; set; }

        [Column("PASSWORD_")] // SQL'deki adı PASSWORD_
        public string Password { get; set; }

        [Column("FULLNAME")] // SQL'de FULLNAME varsa
        public string Fullname { get; set; }

        [Column("EMAIL")] // SQL'deki adı EMAIL
        public string Email { get; set; }

        [Column("TELNO")] // SQL'deki adı TELNO
        public string Telno { get; set; }

        [Column("CREATEDATE")]
        public DateTime CreateDate { get; set; }
    }
}
