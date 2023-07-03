using System.Reflection;

namespace Csharp.Common.Builders;

/// <summary>
/// This interface allows for some self awareness and reflection when fetching builders using a service scope and
/// for programmable building of any kind of builder object in the lower layers of the abstract class.
/// </summary>
public interface IBuilder
{
    object BlindBuild();
    void SetDefaults();
}

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

/// <summary>
/// This layer holds the base foundation of functionality that builders that are not tied to a database will have.  For
/// more details please consult the comments of each method.
/// </summary>
/// <typeparam name="TModel"></typeparam>
/// <typeparam name="TParentBuilder"></typeparam>
public abstract class BuilderAbstract<TModel, TParentBuilder>
    where TModel : class, new() 
    where TParentBuilder : BuilderAbstract<TModel, TParentBuilder>, new()
{
    private List<BuildAction> BuildActions { get; } = new();

    private Func<TModel>? ExistingFunction { get; set; }

    /// <summary>
    /// TODO - add documentation
    /// This is a lesser used method and needs to be documented for specific use cases
    /// </summary>
    /// <returns>The TModel concrete of the builder</returns>
    public object BlindBuild()
    {
        return Build();
    }

    /// <summary>
    /// TODO - add documentation
    /// This is a lesser used method and needs to be documented for specific use cases
    /// </summary>
    public void SetDefaults()
    {
        WithDefaults();
    }

    /// <summary>
    /// The Build() method is the point at which the model is actually created by use of a special cast.
    /// Refer to the implicit operator TModel for implementation details.
    /// </summary>
    /// <returns></returns>
    public TModel Build()
    {
        return (TModel) this;
    }

    /// <summary>
    /// This is a very important abstract function that all builders must override and implement.  It allows you to
    /// setup a default construction of an object before populating it with values that are required to change. Used
    /// extensively in unit testing.
    /// </summary>
    /// <returns></returns>
    public abstract TParentBuilder WithDefaults();

    /// <summary>
    /// <para>
    /// This is the method that allows you to populate data using explicit method calls on your parent builder.  You use
    /// this when defining new methods that describe the action on the model in the context of the builder.
    /// </para>
    /// <para>
    /// A way to think about this is that this method is the magic behind the builder besides the implicit static cast
    /// operator.  This is the heart of the builder that allows you to describe the actions upon the data object the
    /// builder is responsible for.  Ie: <c>WithFirstName(string firstName)</c> is the method that describes the action and then
    /// <c>With(x => x.FirstName = firstName)</c> is the implementation that performs an action on the model.
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// public MyBuilder WithFirstName(string firstName)
    /// {
    ///   With(x => x.FirstName = firstName);
    /// }
    /// </code>
    /// </example>
    /// <param name="action"></param>
    /// <returns></returns>
    public TParentBuilder With(Action<TModel> action)
    {
        BuildActions.Add(new BuildAction {
            Action = action
        });

        return (TParentBuilder) this;
    }

    /// <summary>
    /// <para>
    /// This is the part of the builder that allows for an implicit cast operation using generic types that are
    /// defined when creating the builder.  When you create a new builder of type `TModel`, this cast allows for the
    /// `TModel` type to be instantiated and then to roll through each action placed on the object (in turn of invocation)
    /// such that the returned object after cast is the concrete type with all the actions defined in the fluency
    /// pattern applied to the object before returning.  In other words: this gets you the object that you defined
    /// in the code.
    /// </para>
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public static implicit operator TModel(BuilderAbstract<TModel, TParentBuilder> b)
    {
        // invoke the creation of the object
        TModel built = b.ExistingFunction?.Invoke() ?? b.Convert();

        // roll through the actions that were applied to the object
        foreach (var buildAction in b.BuildActions)
        {
            buildAction.Action?.Invoke(built);
        }

        // clear out the actions
        b.BuildActions.Clear();

        // return the concrete type
        return built;
    }

    /// <summary>
    /// This returns an actual instance of the object
    ///
    /// TODO - analyze the new() type as a constraint
    /// </summary>
    /// <returns></returns>
    private TModel Convert()
    {
        return Activator.CreateInstance<TModel>();
    }

    /// <summary>
    /// This defines the private data type of actions the builder is aware of
    /// </summary>
    private class BuildAction
    {
        // public string? BuildTitle { get; set; }
        public Action<TModel>? Action { get; set; }
    }
    
    #region custom attribute value functionality
    /// <summary>
    /// TODO - add documentation
    /// This is a lesser used method and needs to be documented for specific use cases
    /// </summary>
    /// <param name="customAttributeName"></param>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    protected CustomAttributeData? GetCustomAttribute(string customAttributeName, Type type, string propertyName)
    {
        return type.GetProperty(propertyName)?
            .GetCustomAttributesData()
            .FirstOrDefault(x => x.AttributeType.Name == customAttributeName);
    }

    /// <summary>
    /// TODO - add documentation
    /// This is a lesser used method and needs to be documented for specific use cases
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    protected int? GetStringLengthAttributeValue(Type type, string propertyName)
    {
        var attribute = GetCustomAttribute("StringLengthAttribute", type, propertyName);
        if (attribute is null)
        {
            return null;
        }
        return (int?)attribute.ConstructorArguments[0].Value;
    }

    /// <summary>
    /// TODO - add documentation
    /// This is a lesser used method and needs to be documented for specific use cases
    /// </summary>
    /// <param name="value"></param>
    /// <param name="stringLength"></param>
    /// <param name="exceptionMessage"></param>
    /// <exception cref="BuilderException"></exception>
    protected void ValidateStringLength(string value, int? stringLength, string exceptionMessage)
    {
        if (stringLength is null or 0)
        {
            throw new BuilderException("A null or 0 string length is not accepted as a string length value for validating.");
        }

        if (value.Length > stringLength)
        {
            throw new BuilderException(exceptionMessage + " :: " + value.Length + " > " + stringLength);
        }
    }
    #endregion
}
