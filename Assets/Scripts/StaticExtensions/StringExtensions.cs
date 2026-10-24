
public static class StringExtensions 
{
    public static bool HasContent(this string value)
    {
        return !string.IsNullOrEmpty(value);
    }
}
