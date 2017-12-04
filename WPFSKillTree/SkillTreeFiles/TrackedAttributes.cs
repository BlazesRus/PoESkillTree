namespace POESKillTree.SkillTreeFiles
{
	public class UsedAttribute
	{
		string Name;
		double ConversionMultiplier=1.0;
	}
	public class TrackedAttribute
	{
		List <UsedAttribute> UsedAttributes;
		double TotalStat=0.0;
	}
	public class TrackedAttributes : List <TrackedAttribute>
	{
	
	}
}