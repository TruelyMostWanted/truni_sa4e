using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace XmasWishes.Models.wishes;

[Table("Wishes")]
public class Wish
{
    public enum StatusEnum
    {
        Formulated,
        InProgress,
        Delivering,
        UnderTree
    }
    
    [Key] public int Id { get; set; }
    [Required] [MaxLength(500)] public string Description { get; set; }
    [MaxLength(100)] public string FileName { get; set; }
    [Required] [MaxLength(32)] public string Status { get; set; }
}