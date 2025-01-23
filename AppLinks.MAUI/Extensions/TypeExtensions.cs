namespace AppLinks.MAUI.Extensions
{
    internal static class TypeExtensions
    {
        internal static string GetFormattedName(this Type type)
        {
            if (type.IsGenericType)
            {
                var genericArguments = type.GetGenericArguments()
                    .Select(x => x.Name)
                    .Aggregate((x1, x2) => $"{x1}, {x2}");
                return $"{type.Name[..type.Name.IndexOf("`")]}"
                       + $"<{genericArguments}>";
            }

            return type.Name;
        }
    }
}