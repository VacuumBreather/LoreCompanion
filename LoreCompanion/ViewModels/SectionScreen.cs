using Caliburn.Micro;

namespace LoreCompanion.ViewModels
{
    public abstract class SectionScreen : Screen
    {
        protected SectionScreen(string section)
        {
            DisplayName = GetType().Name.Replace("ViewModel", "");
            Section = section;
        }

        /// <inheritdoc/>
        public sealed override string DisplayName
        {
            get => base.DisplayName;
            set => base.DisplayName = value;
        }

        public string Section { get; }
    }
}