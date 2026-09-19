using Caliburn.Micro;

namespace LoreCompanion.ViewModels
{
    public abstract class SectionScreen : Screen
    {
        public string Section { get; }

        protected SectionScreen(string section)
        {
            Section = section;
        }
    }
}