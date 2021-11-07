using System;

namespace PoESkillTree.ViewModels
{
    /// <summary>
    /// Slightly altered Variant of Attribute with only single stored value
    /// </summary>
    public class PseudoTotal
    {
        public string Text { get; }
        public float Total { get; set; }
        public bool Missing { get; set; }

        public PseudoTotal(string text)
        {
            Text = text;
            Total = 0.0f;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}