using System.ComponentModel.DataAnnotations;

namespace Csharp.Common.Db.Entities;

public class Car
{
    public int CarId { get; set; }
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public CarMake CarMake { get; set; }
}