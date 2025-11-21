using System.ComponentModel.DataAnnotations;

namespace MysqlMongodbMigrator.Models
{
    public class Users
    {
        [Key]
        public int UserID { get; set; }
        
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Roles { get; set; }
    }
}