using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XmasWishes.Models.persons;

[Table("Persons")]
public class Person
{
    [Key]
    public int Id { get; set; } // Primärschlüssel
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Surname { get; set; }
    
    [Required]
    public int Age { get; set; }

    public Person(int id, string name, string surname, int age)
    {
        Id = id;
        Name = name;
        Surname = surname;
        Age = age;
    }

    public Person()
    {
        
    }
}