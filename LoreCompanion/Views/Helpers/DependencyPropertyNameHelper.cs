namespace LoreCompanion.Views.Helpers
{
    public static class DependencyPropertyNameHelper
    {
        public static string GetName(string str)
        {
            return str.Replace("Property", "");
        }

        /// <summary>Gets the name of a routed event minus the "Event" part.</summary>
        /// <param name="routedEventName">The full name of the outed event.</param>
        /// <returns>The name of a outed event minus the "Event" part.</returns>
        public static string GetRoutedEventName(string routedEventName)
        {
            return routedEventName.Replace("Event", "");
        }
    }
}