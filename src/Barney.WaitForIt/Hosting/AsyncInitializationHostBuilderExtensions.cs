using Microsoft.Extensions.DependencyInjection;
using Barney.WaitForIt;
using System;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Provides extension methods to perform async initialization of an application.
    /// </summary>
    public static class AsyncInitializationHostBuilderExtensions
    {

        /// <summary>
        /// Initializes the application, by calling all registered async initializers.
        /// </summary>
        /// <param name="host">The web host.</param>
        /// <returns>A task that represents the initialization completion.</returns>
        public static async Task<IHost> WaitForItAsync(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var rootInitializer = scope.ServiceProvider.GetService<RootInitializer>();
                if (rootInitializer == null)
                {
                    throw new InvalidOperationException("The async initialization service isn't registered, register it by calling AddAsyncInitialization() on the service collection or by adding an async initializer.");
                }

                await rootInitializer.InitializeAsync();

                return host;
            }
        }
    }
}
