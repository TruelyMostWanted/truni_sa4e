using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XmasWishes.Models.account;

[Table("Accounts")]
public class Account
{
    [Key]
    public int Id { get; set; }
    [Required]
    [MaxLength(32)]
    public string Username { get; set; }
    [MaxLength(64)]
    public string Email { get; set; }
    [MaxLength(256)]
    public string Password { get; set; }
    [MaxLength(64)]
    public string Name { get; set; }

    public Account(int id, string username, string email, string password, string name)
    {
        Id = id;
        Username = username;
        Email = email;
        Password = password;
        Name = name;
    }
    public Account()
    {
        
    }
}