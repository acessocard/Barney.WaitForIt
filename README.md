# Barney.WaitForIt

A simple helper to perform async application initialization in .NET Core 2.0 or higher.

## Usage

1. Install the [Barney.WaitForIt](https://www.nuget.org/packages/Barney.WaitForIt/) NuGet package:

    Command line:

    ```PowerShell
    dotnet add package Barney.WaitForIt
    ```

    Package manager console:
    ```PowerShell
    Install-Package Barney.WaitForIt
    ```


1. Create a class (or several) that implements `IAsyncInitializer`. This class can depend on any registered service.

    ```csharp
    public class MyAppInitializer : IAsyncInitializer
    {
        public MyAppInitializer(IFoo foo, IBar bar)
        {
            ...
        }

        public async Task InitializeAsync()
        {
            // Initialization code here
        }
    }
    ```

1. Register your initializer(s) in the `Startup.ConfigureServices` method:

    ```csharp
        services.AddAsyncInitializer<MyAppInitializer>();
    ```

1. In the `Program` class, make the `Main` method async and change its code to initialize the host before running it:

    ```csharp
    public static async Task Main(string[] args)
    {
        var host = new HostBuilder()
                        .ConfigureServices(configureServices)
                        .UseConsoleLifetime()
                        .Build();
        
        await host.WaitForItAsync();
        await host.StartAsync();
    }
    ```

(Note that you need to [set the C# language version to 7.1 or higher in your project](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version#edit-the-csproj-file) to enable the "async Main" feature.)

This will run each initializer, in the order in which they were registered.