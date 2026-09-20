using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using LoreCompanion.Models;
using LoreCompanion.ViewModels;
using LoreCompanion.ViewModels.Dialogs;
using LoreCompanion.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LoreCompanion
{
    public class Bootstrapper : BootstrapperBase
    {
        private IServiceProvider _serviceProvider = null!;

        public Bootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            ServiceCollection services = new();

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

            services.AddSingleton<IWindowManager, WindowManager>();
            services.AddSingleton<IEventAggregator, EventAggregator>();
            services.AddSingleton<IDialogService, DialogConductor>();

            // Sets up SQLite with the file path of your choice
            var dbFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LoreCompanion");

            Directory.CreateDirectory(dbFolder);

            var dbPath = Path.Combine(dbFolder, "lorecompanion.db");

            services.AddDbContextFactory<LoreDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

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
            await using var context = await _serviceProvider.GetRequiredService<IDbContextFactory<LoreDbContext>>()
                                                            .CreateDbContextAsync();

            await context.Database.MigrateAsync();

            await DisplayRootViewForAsync<ShellViewModel>();
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            var shellViewModel = _serviceProvider.GetRequiredService<ShellViewModel>();
            shellViewModel.DeactivateAsync(true).GetAwaiter().GetResult();
        }
    }
}