using GenHTTP.Engine;

using GenHTTP.Modules.Practices;
using System;
using WebSimulateSNP42X;
using Microsoft.Extensions.Configuration;

IWorkLogger logger = new WorkLogger();

IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

try
{
    var settings = config.GetRequiredSection("Settings").Get<Settings>();
    if(settings != null ) Settings.INSTANCE = settings;
}
catch (Exception ex)
{
    logger.Error("Fail to get settings from appsettings.json.", ex);
}

var project = Project.Setup();

return Host.Create()
           .Handler(project)
           .Defaults()
           .Console()
#if DEBUG
           .Development()
#endif
           .Run();