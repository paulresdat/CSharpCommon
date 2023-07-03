namespace Csharp.Common.EntityFramework.Domain.Entities.Interfaces;

/// <summary>
/// <para>This interface helps enforce a standard for audit fields.  Feel free to extend into your own project by using this
/// as a baseline for all audit data that should exist in every table created in the database.</para>
///
/// <example>
/// <code>
/// // all db objects must extend this interface
/// public interface IMyProjectAudit : IAudit
/// {
///   string AnotherAuditField { get; set; }
/// }
/// </code>
/// </example>
/// </summary>
public interface IAudit
{
    DateTime CreatedDate { get; set; }
    string CreatedBy { get; set; }
    DateTime UpdatedDate { get; set; }
    string UpdatedBy { get; set; }
}