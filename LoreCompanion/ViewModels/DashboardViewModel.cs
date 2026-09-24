using JetBrains.Annotations;
using LoreCompanion.ViewModels.Attributes;

namespace LoreCompanion.ViewModels
{
    [Dashboard, UsedImplicitly]
    public class DashboardViewModel : SectionScreen
    {
        public DashboardViewModel()
            : base(NavigationSection.Overview)
        {
        }
    }
}