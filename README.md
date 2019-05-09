# Azure Functions Dependency Injection example

This repo is an Azure Functions dependency injection example for .NET. Implementation is forked from: https://github.com/Azure/azure-functions-dotnet-extensions, but refactored to use the Nuget package `Microsoft.Azure.Functions.Extensions`

Implementation is according to the [Microsoft Docs](https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection).

## Step by step
1. Install `Microsoft.Azure.Functions.Extensions` to your Azure Functions V2 .NET project.
2. Add a `Startup.cs` file, example:
```csharp
[assembly: FunctionsStartup(typeof(Microsoft.Azure.Functions.Samples.DependencyInjectionScopes.Startup))]
namespace Microsoft.Azure.Functions.Samples.DependencyInjectionScopes
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Register MyServiceA as transient.
            // A new instance will be returned every
            // time a service request is made
            builder.Services.AddTransient<MyServiceA>();
        }
    }
}
```
3. Convert your Functions to not use static classes and methods anymore.
```csharp
public class SampleFunction
{
    private readonly IGreeter _greeter;

    public SampleFunction(IGreeter greeter)
    {
        _greeter = greeter;
    }

    [FunctionName("SampleFunction")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        string name = req.Query["name"];

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        name = name ?? data?.name;

        return name != null
            ? (ActionResult)new OkObjectResult(_greeter.CreateGreeting(name))
            : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
    }
}
```