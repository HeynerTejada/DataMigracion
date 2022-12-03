using AppDesktop.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace AppDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();

            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();

            mainWindow.Show();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IMainViewModel, MainViewModel>();

            services.AddTransient(typeof(MainWindow));
        }
    }
}