namespace POESKillTree.SkillTreeFiles
{
	public class UsedAttribute
	{
		string Name;
		double ConversionMultiplier=1.0;
	}
	public class TrackedAttribute
	{
        string Name;
        System.Collections.Generic.SortedSet<UsedAttribute> UsedAttributes;
		double TotalStat=0.0;
	}
	public class TrackedAttributes : System.Collections.Generic.SortedSet<TrackedAttribute>
	{
	
	}
}