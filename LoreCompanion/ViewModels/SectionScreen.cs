using Caliburn.Micro;

namespace LoreCompanion.ViewModels
{
    public abstract class SectionScreen : Screen
    {
        protected SectionScreen(string section)
        {
            Section = section;
        }

        public string Section { get; }
    }
}