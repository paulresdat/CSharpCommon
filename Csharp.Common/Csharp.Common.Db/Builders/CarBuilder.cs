using Csharp.Common.Db.Entities;
using Csharp.Common.EntityFramework.Builders;

namespace Csharp.Common.Db.Builders;

public class CarBuilder : BuilderDbContext<Car, CsharpCommonTestingDbContext, CarBuilder>
{
    public override CarBuilder WithDefaults()
    {
        return With(x =>
        {
            x.Name = "DEFAULT CAR NAME";
            x.CarMake = CarMake.Volkswagen;
        });
    }

    public CarBuilder WithName(string name)
    {
        return With(x => x.Name = name);
    }

    public CarBuilder With(CarMake carMake)
    {
        return With(x => x.CarMake = carMake);
    }
}

public class PersonBuilder : BuilderDbContext<Person, ICsharpCommonTestingDbContext, PersonBuilder>
{
    public override PersonBuilder WithDefaults()
    {
        return With(x => { });
    }

    public PersonBuilder WithName(string name)
    {
        return With(x => x.Name = name);
    }
} 