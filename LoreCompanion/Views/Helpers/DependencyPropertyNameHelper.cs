namespace LoreCompanion.Views.Helpers
{
    public static class DependencyPropertyNameHelper
    {
        public static string GetName(string str)
        {
            return str.Replace("Property", "");
        }
    }
}