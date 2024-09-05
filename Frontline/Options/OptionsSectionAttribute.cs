namespace Frontline.Options;

[AttributeUsage(AttributeTargets.Class)]
public class OptionsSectionAttribute : Attribute
{
    public string SectionName { get; }
    
    public OptionsSectionAttribute(string sectionName)
    {
        SectionName = sectionName;
    }
}