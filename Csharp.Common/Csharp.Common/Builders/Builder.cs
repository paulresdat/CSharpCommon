namespace Csharp.Common.Builders;

/// <summary>
/// <para>
/// The mother object of all shared models (data transfer objects) generated with a fluency pattern.  I highly recommend
/// using this pattern in your unit testing.  It's also handy in some cases using builders in your production code.
/// </para>
///
/// <para>Why is this handy?</para>
///
/// <list type="number">
/// <item><description>
///     It allows you to be more explicit about the scenarios or data types you want to use.
/// </description></item>
/// <item><description>
///     You can write data modeling scenarios into more complex unit tests.
/// </description></item>
/// <item><description>
///     Your data modeling in code can be much more readable and faster to program.
/// </description></item>
/// </list>
///
/// <para>
/// What makes this quite nice is the ability to fluently describe the data you want to populate and you can use
/// this to write more explicit scenario types with defaulted values to an important data object.
/// The example below illustrates how you can define default values, scenarios and methods around populating
/// the shared model without using the initialization syntax.  This makes model populating and also
/// use of DTO's a bit more readable and intuitive in code, especially for Unit Testing.
/// </para>
///
/// <para>
/// <code>
/// class Dto1
/// {
///   public string FirstName { get; set; }
///   public string LastName { get; set; }
/// }
///
/// class Dto1Builder : Builder{Dto1, Dto1Builder}
/// {
///   public override Dto1Builder WithDefaults()
///   {
///     With(x => {
///       x.FirstName = "Test";
///       x.LastName = "Test";
///     });
///   }
/// }
/// // and in practice
/// var myDto = Dto1Builder.Create()
///   .WithDefaults();
/// </code>
/// </para>
/// </summary>
/// <typeparam name="TModel">The Model type it will generate</typeparam>
/// <typeparam name="TParentBuilder">The parent builder for self awareness</typeparam>
public abstract class Builder<TModel, TParentBuilder> : BuilderAbstract<TModel, TParentBuilder>, IBuilder
    where TModel : class, new()
    where TParentBuilder : BuilderAbstract<TModel, TParentBuilder>, new()
{
    public static TParentBuilder Create()
    {
        return new TParentBuilder();
    }
}