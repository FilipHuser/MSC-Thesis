using System.Diagnostics.CodeAnalysis;
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
            services.AddSingleton<ILoggingService, LoggingService>();
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<ISignalService, SignalService>();
            services.AddSingleton<IDataHubService, DataHubService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IAppConfigurationService, AppConfigurationService>();
            services.AddSingleton<IFileExportService, FileExportService>();

            //VIEWMODELS
            services.AddScoped<MainViewModel>();

            services.AddSingleton<Create<MeasurementViewModel>>(x =>
            {
                return () => new MeasurementViewModel(
                    x.GetRequiredService<ISignalService>(),
                    x.GetRequiredService<IDataHubService>(),
                    x.GetRequiredService<IViewModelFactory>(),
                    x.GetRequiredService<IDialogService>(),
                    x.GetRequiredService<IFileExportService>());
            });

            services.AddSingleton<Create<DataPlotterViewModel>>(x => 
            {
                return () => new DataPlotterViewModel(
                    x.GetRequiredService<ISignalService>(),
                    x.GetRequiredService<ILoggingService>());
            });

            services.AddSingleton<Create<DataAcquisitionViewModel>>(x =>
            {
                return () => new DataAcquisitionViewModel(
                    x.GetRequiredService<ISignalService>(),
                    x.GetRequiredService<IViewModelFactory>());
            });

            services.AddSingleton<Create<ChannelsConfigViewModel>>(x => 
            {
                return () => new ChannelsConfigViewModel(
                    x.GetRequiredService<ISignalService>(),
                    x.GetRequiredService<ILoggingService>(),
                    x.GetRequiredService<IConfigurationService>(),
                    x.GetRequiredService<IViewManager>());
            });

            services.AddSingleton<Create<SignalManagerViewModel>>(x =>
            {
                return () => new SignalManagerViewModel(
                    x.GetRequiredService<IViewManager>(),
                    x.GetRequiredService<IConfigurationService>());
            });

            services.AddSingleton<Create<SignalCreatorViewModel>>(x => {
                return () => new SignalCreatorViewModel(
                    x.GetRequiredService<IViewManager>());
            });

            services.AddSingleton<Create<PreferencesViewModel>>(x => {
                return () => new PreferencesViewModel(
                    x.GetRequiredService<IAppConfigurationService>(),
                    x.GetRequiredService<IViewManager>());
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
