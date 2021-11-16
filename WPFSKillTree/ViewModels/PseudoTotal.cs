using System;

namespace PoESkillTree.ViewModels
{
    /// <summary>
    /// Slightly altered Variant of Attribute with only single stored value
    /// </summary>
    public class PseudoTotal
    {
        //Stores the Display of the PseudoTotal

        private string text = "Not Calculated Yet";
        public string Text
        {
            get => text;
            set => text = value;
        }
        public float Total { get; set; }

        public PseudoTotal(string text, float total)
        {
            Text = text;
            Total = total;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}