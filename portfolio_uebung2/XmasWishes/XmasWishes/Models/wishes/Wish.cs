using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    
    [Key]
    public int Id { get; set; }
    [Required]
    [MaxLength(256)]
    public string Description { get; set; }
    [Required]
    public StatusEnum Status { get; set; }
}

