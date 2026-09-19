using Caliburn.Micro;

namespace LoreCompanion.ViewModels
{
    public class ShellViewModel : Conductor<SectionScreen>.Collection.OneActive
    {
        public ShellViewModel(IEnumerable<SectionScreen> sections)
        {
            Items.AddRange(sections);
        }

        protected override Task OnInitializedAsync(CancellationToken cancellationToken)
        {
            return ActivateItemAsync(Items.First(), cancellationToken);
        }
    }
}