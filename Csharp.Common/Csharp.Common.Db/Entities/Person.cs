using System.ComponentModel.DataAnnotations;

namespace Csharp.Common.Db.Entities;

public class Person
{
    public int PersonId { get; set; }
    [Required]
    public string Name { get; set; } = null!;
    public DateTime Entered { get; set; }
}