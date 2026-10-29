using System;

[AttributeUsage(AttributeTargets.Field)]
public class RequiredAttribute : Attribute
{
    public Search Search { get; }
    public bool IncludeInactive { get; set; } = true;
    public bool Optional { get; set; }

    public RequiredAttribute(Search search = Search.Self) 
    {
        Search = search;
    }
}
