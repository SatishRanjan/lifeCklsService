namespace LifeCicklsWebApi
{
    public static class EnvironmentHelper
    {
        public static string GetEnvironmentValueOrDefault(string variableName, string defaultValue)
        {
            string? value = Environment.GetEnvironmentVariable(variableName);
            return value ?? defaultValue;
        }
    }
}
