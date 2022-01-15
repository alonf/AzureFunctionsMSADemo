using AutoMapper;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using QueueTriggerCosmosOutput;

[assembly: FunctionsStartup(typeof(Startup))]

namespace QueueTriggerCosmosOutput;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var mapperConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new DataItemAutoMappingProfile());
        });

        IMapper mapper = mapperConfig.CreateMapper();
        mapperConfig.AssertConfigurationIsValid();
        builder.Services.AddSingleton(mapper);
    }
}
