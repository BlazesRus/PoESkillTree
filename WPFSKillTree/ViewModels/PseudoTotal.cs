using System;

namespace PoESkillTree.ViewModels
{
    /// <summary>
    /// Slightly altered Variant of Attribute with only single stored value
    /// </summary>
    public class PseudoTotal
    {
#if PoESkillTree_UseSwordfishDictionary==false && PoESkillTree_UseIXDictionary == false && PoeSkillTree_DontUseKeyedTrackedStats==false
        public string Key;
#endif
        //Stores the Display of the PseudoTotal
        private string text = "Not Calculated Yet";
        public string Text
        {
            get => text;
            set => text = value;
        }
        public float Total { get; set; }

#if PoESkillTree_UseSwordfishDictionary==false && PoESkillTree_UseIXDictionary == false && PoeSkillTree_DontUseKeyedTrackedStats==false
        public PseudoTotal(string text, float total, string keyVal)
        {
            Key = keyVal;
#else
        public PseudoTotal(string text, float total)
        {
#endif
            Text = text;
            Total = total;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}