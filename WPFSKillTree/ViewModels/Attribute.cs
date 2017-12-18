namespace POESKillTree.ViewModels
{
    public class Attribute
    {
        public string Text { get; set; }
        public float[] Deltas { get; set; }
        public bool Missing { get; set; }

        public Attribute(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Perform Explicit conversion from TrackedStat into Attribute for textblock conversion 
        /// </summary>
        /// <param name="value"></param>
        public static explicit operator Attribute(POESKillTree.SkillTreeFiles.TrackedAttribute value)
        {
            Attribute newSelf = new Attribute(value.Name());
            float[] newDelta = { (float)value.TotalStat };
            return newSelf;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}