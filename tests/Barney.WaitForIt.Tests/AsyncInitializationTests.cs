using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Barney.WaitForIt.Tests
{
    public class AsyncInitializationTests
    {

        [Fact]
        public async Task Single_initializer_is_called()
        {
            var initializer = Substitute.For<IAsyncInitializer>();

            var host = CreateHost(p => p.AddAsyncInitializer(initializer));

            await host.WaitForItAsync();

            _ = initializer.Received().InitializeAsync();
        }

        [Fact]
        public async Task Delegate_initializer_is_called()
        {
            var initializer = Substitute.For<Func<Task>>();

            var host = CreateHost(services => services.AddAsyncInitializer(initializer));

            await host.WaitForItAsync();

            _ = initializer.Received()();
        }

        [Fact]
        public async Task Multiple_initializers_are_called_in_order()
        {
            var initializer1 = Substitute.For<IAsyncInitializer>();
            var initializer2 = Substitute.For<IAsyncInitializer>();
            var initializer3 = Substitute.For<IAsyncInitializer>();

            var host = CreateHost(services =>
            {
                services.AddAsyncInitializer(initializer1);
                services.AddAsyncInitializer(initializer2);
                services.AddAsyncInitializer(initializer3);
            });

            await host.WaitForItAsync();

            Received.InOrder(() =>
            {
                _ = initializer1.Received().InitializeAsync();
                _ = initializer2.Received().InitializeAsync();
                _ = initializer3.Received().InitializeAsync();
            });
        }

        [Fact]
        public async Task Failing_initializer_makes_initialization_fail()
        {
            var initializer1 = Substitute.For<IAsyncInitializer>();
            var initializer2 = Substitute.For<IAsyncInitializer>();
            var initializer3 = Substitute.For<IAsyncInitializer>();

            initializer2.When(x => x.InitializeAsync()).Do(x => throw new Exception("oops"));

            var host = CreateHost(services =>
            {
                services.AddAsyncInitializer(initializer1);
                services.AddAsyncInitializer(initializer2);
                services.AddAsyncInitializer(initializer3);
            });

            var exception = await Record.ExceptionAsync(() => host.WaitForItAsync());
            Assert.IsType<Exception>(exception);
            Assert.Equal("oops", exception.Message);

            _ = initializer1.Received().InitializeAsync();
            _ = initializer3.DidNotReceive().InitializeAsync();
        }

        [Fact]
        public async Task InitAsync_throws_InvalidOperationException_when_services_are_not_registered()
        {
            var host = CreateHost(services => { });
            var exception = await Record.ExceptionAsync(() => host.WaitForItAsync());
            Assert.IsType<InvalidOperationException>(exception);
            Assert.Equal("The async initialization service isn't registered, register it by calling AddAsyncInitialization() on the service collection or by adding an async initializer.", exception.Message);
        }

        private static IHost CreateHost(Action<IServiceCollection> configureServices) =>
                new HostBuilder()
                    .ConfigureServices(configureServices)
                    .Build();
    }
}
