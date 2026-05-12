namespace Csharp.Common.Db.Entities;

public class PersonCarXref
{
    public int PersonCarXrefId { get; set; }
    public int PersonId { get; set; }
    public int CarId { get; set; }

    public virtual Person Person { get; set; } = null!;
    public virtual Car Car { get; set; } = null!;
}