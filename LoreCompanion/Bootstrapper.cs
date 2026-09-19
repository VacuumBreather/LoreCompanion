using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using Grace.DependencyInjection;
using LoreCompanion.ViewModels;
using LoreCompanion.Views;

namespace LoreCompanion
{
    public class Bootstrapper : BootstrapperBase
    {
        private readonly DependencyInjectionContainer _container = new();

        public Bootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            _container.Configure(c =>
            {
                foreach (var type in typeof(Bootstrapper).Assembly.GetTypes()
                                                         .Where(t => !t.IsAbstract)
                                                         .Where(t => !t.IsGenericTypeDefinition)
                                                         .Where(t => t.IsAssignableTo(typeof(SectionScreen))))
                {
                    c.Export(type).As(typeof(SectionScreen)).Lifestyle.SingletonPerRequest();
                }
            });

            _container.Configure(c => c.ExportAssembly(typeof(Bootstrapper).Assembly)
                                       .Where(t => t.IsAssignableTo(typeof(UserControl)))
                                       .ByType()
                                       .Lifestyle.SingletonPerRequest()
                                 );

            _container.Configure(c => c.Export<ShellViewModel>().Lifestyle.Singleton());
            _container.Configure(c => c.Export<ShellView>().Lifestyle.Singleton());

            _container.Configure(c => c.Export<WindowManager>().ByInterfaces().Lifestyle.Singleton());
            _container.Configure(c => c.Export<EventAggregator>().ByInterfaces().Lifestyle.Singleton());
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.Locate(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.LocateAll(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.Inject(instance);
        }

        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            await DisplayRootViewForAsync<ShellViewModel>();
        }
    }
}