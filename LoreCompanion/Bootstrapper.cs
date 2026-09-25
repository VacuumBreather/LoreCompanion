using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Caliburn.Micro;
using LoreCompanion.Models;
using LoreCompanion.Utilities;
using LoreCompanion.ViewModels;
using LoreCompanion.ViewModels.Dialogs;
using LoreCompanion.ViewModels.Notifications;
using LoreCompanion.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using LogManager = LoreCompanion.Utilities.LogManager;

namespace LoreCompanion
{
    public class Bootstrapper : BootstrapperBase
    {
        private ServiceProvider _serviceProvider = null!;

        public Bootstrapper()
        {
            Initialize();

            Application.DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        protected override void Configure()
        {
            ServiceCollection services = new();

            services.AddConfiguredSerilog(AppHelper.AppDataFolder);

            services.Scan(scan => scan.FromAssemblyOf<Bootstrapper>()
                                      .AddClasses(classes => classes.AssignableTo<SectionScreen>()
                                                                    .Where(t => !t.IsAbstract)
                                                                    .Where(t => !t.IsGenericTypeDefinition))
                                      .As<SectionScreen>()
                                      .WithTransientLifetime()
                                      .AddClasses(classes => classes.AssignableTo<UserControl>())
                                      .AsSelf()
                                      .WithTransientLifetime());

            services.AddSingleton<ShellViewModel>();
            services.AddSingleton<ShellView>();

            services.AddSingleton<CachedDataLoader>();
            services.AddSingleton<IWindowManager, WindowManager>();
            services.AddSingleton<IEventAggregator, EventAggregator>();
            services.AddSingleton<IDialogService, DialogConductor>();
            services.AddSingleton<INotificationService, NotificationConductor>();

            services.AddDbContextFactory<LoreDbContext>(options => options.UseSqlite(AppHelper.ConnectionString));

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override object? GetInstance(Type service, string key)
        {
            return _serviceProvider.GetKeyedService(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _serviceProvider.GetServices(service)!;
        }

        protected override void BuildUp(object instance)
        {
            var type = instance.GetType();

            foreach (var property in type.GetProperties(
                         BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!property.CanWrite)
                {
                    continue;
                }

                var value = _serviceProvider.GetService(property.PropertyType);

                if (value is null)
                {
                    continue;
                }

                property.SetValue(instance, value);
            }
        }

        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                await DisplayRootViewForAsync<ShellViewModel>();
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "An error occurred during startup");

                Application.Current.Shutdown();
            }
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            try
            {
                var logger = LogManager.GetLogger();
                logger.Information("Application is shutting down...");

                Task.Run(async () =>
                    {
                        if (_serviceProvider is IAsyncDisposable asyncDisposable)
                        {
                            await asyncDisposable.DisposeAsync();
                        }
                        else if (_serviceProvider is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    })
                    .GetAwaiter()
                    .GetResult();
            }
            finally
            {
                base.OnExit(sender, e);
            }
        }

        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Fatal(e.Exception, "Unhandled exception on the WPF dispatcher");
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                Log.Fatal(exception, "Unhandled exception on the AppDomain");
            }
            else
            {
                Log.Fatal("Unhandled exception on the AppDomain: {ExceptionObject}", e.ExceptionObject);
            }
        }

        private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.Error(e.Exception, "Unhandled exception on an unobserved task");
            e.SetObserved();
        }
    }
}