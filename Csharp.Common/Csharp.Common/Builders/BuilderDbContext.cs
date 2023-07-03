using Microsoft.Extensions.DependencyInjection;

namespace Csharp.Common.Builders;

public interface IBuilderServiceScope
{
    void RegisterServiceScope(IServiceScopeFactory serviceScopeFactory);
}

public interface IBuilderDbContext<in TDbContext>
{
    void RegisterDbContext(TDbContext dbContext);
}

/// <summary>
/// <para>
/// This abstract class defines the ability to dynamically create data that populates a database for integration testing
/// scenarios.  You can also use this in production code, however this is a helper class that's most beneficial for
/// integration tests on the database.
/// </para>
///
/// <para>
/// This is an extremely helpful class for setting up scenarios in your integration tests by describing the scenario
/// or data that you want to create in the context of the test. For instance, you need to bootstrap an integration test
/// by first setting up data in the database that needs to exist for a specific integration test to pass.  Instead of
/// working with the db context itself and writing all the queries by hand, you create new builders and setup scenarios
/// that drastically shorthand the amount of code you write.
/// </para>
///
/// <para>
/// It's important to note how your unit testing classes are setup.  These builders work great in conjunction with the
/// unit testing framework classes also found within this library.  More documentation on how to setup an integration
/// testing class, setting up your objects properly, defining a rollback transaction strategy and using a singleton
/// approach for your DB context object is found in their respective classes in this project.
/// </para>
/// </summary>
/// <example>
/// <code>
/// // db class
/// class Person
/// {
///   public string FirstName { get; set; }
///   public string LastName { get; set; }
/// }
/// 
/// // db context
/// DbSet{Person} People => Set{Person}();
/// 
/// // builder
/// class PersonBuilder : BuilderDbContext{Person, YourDbContext, PersonBuilder}
/// {
///    public override PersonBuilder WithDefaults()
///    {
///      With(x => {
///        x.FirstName = "Test";
///        x.LastName = "LTest";
///      });
///    }
/// }
///
/// // in practice, db object is an EF database object
/// my dbObject = PersonBuilder.Create(dbContext)
///   .WithDefaults()
///   // saves to the database
///   .BuildAndSave();
/// </code>
/// </example>
/// <typeparam name="TModel">The DBSet Entity (your model)</typeparam>
/// <typeparam name="TDbContext">Your DbContext type (can be an interface)</typeparam>
/// <typeparam name="TParentBuilder">The parent builder for self awareness</typeparam>
public abstract class BuilderDbContext<TModel, TDbContext, TParentBuilder> : 
    BuilderDbConnector<TDbContext, TModel, TParentBuilder>,
    IBuilderDbContext<TDbContext>, IBuilderServiceScope
    where TDbContext : class 
    where TModel : class, new()
    where TParentBuilder: BuilderAbstract<TModel, TParentBuilder>, new()
{
    protected BuilderDbContext()
    {
    }

    /// <summary>
    /// This static builder acts as a wrapper to the `new` constructor.  It helps consistency by using a static method to
    /// create a new type.  You don't need it necessarily, but here for code consistency.  Using <c>Create</c> while
    /// injecting the dbContext in your integration tests is more fluent and readable in but neglecting Create in the
    /// case where you don't need to inject a dbContext will show 2 different patterns in your code and it
    /// thereby breaks consistency.  You can use <b>Create()</b> without injecting the db context if you just want to
    /// create the object itself without being tied to EF.  You're code will look more consistent if you use
    /// <c>Create</c> for all builder instantiations.  <c>BuildAndSave()</c> will throw an exception, you can only
    /// <c>Build</c>.
    /// </summary>
    /// <returns></returns>
    public static TParentBuilder Create()
    {
        var builder = new TParentBuilder();
        return builder;
    }

    /// <summary>
    /// This is the primary method used in integration tests to register the singleton of the dbContext, so that
    /// all builders will remain within the database transaction in the integration test.  This can also be used
    /// to push the scoped db context within a repository if you use a builder in production code.
    /// </summary>
    ///
    /// <example>
    /// <code>
    /// // unit test constructor (readonly properties assigned in the constructor)
    /// ServiceCollection.AddSingleton{YourDbContext}();
    /// _serviceProvider = ServiceCollection.BuildServiceProvider()
    /// _dbContext = _serviceProvider.GetRequiredService{YourDbContext}();
    ///
    /// 
    /// // in your unit test
    /// var myObj = MyObjectBuilder.Create(_dbContext)
    ///   .WithDefaults()
    ///   .AsASpecificScenario()
    ///   .BuildAndSave();
    /// </code>
    /// </example>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    public static TParentBuilder Create(TDbContext dbContext)
    {
        var builder = new TParentBuilder();
        ((IBuilderDbContext<TDbContext>)builder).RegisterDbContext(dbContext);
        return builder;
    }

    /// <summary>
    /// We can register a scoped service factory rather than a dbContext.  This allows the builder pattern to be used
    /// within production code.  We actually don't really use this in production so you won't see this pattern in use.
    /// </summary>
    /// <param name="serviceScopeFactory"></param>
    /// <returns></returns>
    public static TParentBuilder Create(IServiceScopeFactory serviceScopeFactory)
    {
        var builder = new TParentBuilder();
        ((IBuilderServiceScope)builder).RegisterServiceScope(serviceScopeFactory);
        return builder;
    }

    /// <summary>
    /// This method registers a service scope factory for the dbContext object to operate within a scoped, multithreaded
    /// environment.  When fetching the DbContext in production, you need it to be scoped to the service scope factory
    /// for multi-threaded use.  This is what makes the dbContext thread safe.
    /// </summary>
    /// <param name="serviceScopeFactory"></param>
    public void RegisterServiceScope(IServiceScopeFactory serviceScopeFactory)
    {
        RegisterBuilderDbConnectorServiceScopeFactory(serviceScopeFactory);
    }

    /// <summary>
    /// This method is a wrapper to the protected function of the DbConnector builder abstract class that handles
    /// the db context handling of a EF tied builder.
    /// </summary>
    /// <param name="dbContext"></param>
    public void RegisterDbContext(TDbContext dbContext)
    {
        RegisterTheDbContextExplicitlyRatherThanScopeFactory(dbContext);
    }

    /// <summary>
    /// <para>
    /// BuildAndSave is an abstract method that must be implemented in your builder by overriding the function.  The
    /// implementation is important to the context of your application and has not been automated at the time of this
    /// writing.  When you save an object, you must say how that object is to be saved within the context of EF and
    /// what DbSet that object belongs to.  Further information is below as well as an example.
    /// </para>
    ///
    /// <para>
    /// The DbContext holds the DbSets, and those are not available to the builder's abstract layer.  You must define
    /// how the object is being saved within the context of EF.  The example below is the boiler plate code that
    /// will work for a basic save.  It should at its bare minimum attach the object to the db context, save the data
    /// and return the EF object.  Any automapping, any DTO or abstracted object mapping should be done OUTSIDE of the
    /// builder.  The builder should only care about the object and its attachment to your database context with EF.
    /// </para>
    ///
    /// <para>
    /// One important note is that you'll find in the code that there is an object `DbContext`.  This is in the abstract
    /// layer, so you don't need to define it.  When you create a new builder context to save data using the
    /// <c>Create(_dbContext)</c> approach, the <c>_dbContext</c> object will be assigned to the internal <c>DbContext</c>
    /// object, so it's all available for you.  You need minimal custom code to get this to work when you bootstrap
    /// it appropriately.
    /// </para>
    /// </summary>
    ///
    /// <example>
    /// <code>
    /// // BOILER PLATE CODE TO WORK IN YOUR BUILDER
    /// protected override YourModel BuildAndSave()
    /// {
    ///   var built = Built();
    ///   DbContext.YourModels.Add(built);
    ///   DbContext.SaveChanges();
    ///   return built;
    /// }
    /// </code>
    /// </example>
    /// <returns></returns>
    public abstract TModel BuildAndSave();
}
