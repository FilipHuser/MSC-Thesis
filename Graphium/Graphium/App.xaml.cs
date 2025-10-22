using System.Windows;
using Graphium.Interfaces;
using Graphium.Services;
using Graphium.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using static Graphium.ViewModels.ViewModelBase;

namespace Graphium
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            IServiceCollection services = new ServiceCollection();

            //SERVICES
            services.AddSingleton<IViewManager, ViewManagerService>();
            services.AddSingleton<IViewFactory , ViewFactoryService>();
            services.AddSingleton<IViewModelFactory, ViewModelFactoryService>();

            services.AddSingleton<ISignalService, SignalService>();
            services.AddSingleton<IDataHubService, DataHubService>();

            //VIEWMODELS
            services.AddScoped<MainViewModel>();

            services.AddSingleton<Create<MeasurementViewModel>>(x =>
            {
                return () => new MeasurementViewModel(
                    x.GetRequiredService<ISignalService>(),
                    x.GetRequiredService<IDataHubService>(),
                    x.GetRequiredService<IViewModelFactory>());
            });

            services.AddSingleton<Create<DataPlotterViewModel>>(x => 
            {
                return () => new DataPlotterViewModel(
                    x.GetRequiredService<ISignalService>());
            });

            services.AddSingleton<Create<DataAcquisitionViewModel>>(x =>
            {
                return () => new DataAcquisitionViewModel(
                    x.GetRequiredService<ISignalService>(),
                    x.GetRequiredService<IViewModelFactory>());
            });

            services.AddSingleton<Create<ChannelsConfigViewModel>>(x => 
            {
                return () => new ChannelsConfigViewModel(x.GetRequiredService<ISignalService>());
            });

            services.AddScoped<MainWindow>(x => new MainWindow(x.GetRequiredService<MainViewModel>()));

            _serviceProvider = services.BuildServiceProvider();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var win = _serviceProvider.GetRequiredService<MainWindow>();
            win.Show();
        }
    }
}
