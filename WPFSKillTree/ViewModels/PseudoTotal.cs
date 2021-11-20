using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PoESkillTree.ViewModels
{
    /// <summary>
    /// Slightly altered Variant of Attribute with only single stored value
    /// </summary>
    public class PseudoTotal: INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion

        protected void OnNotifyPropertyChanged([CallerMemberName] string memberName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(memberName));
            }
        }
#if PoESkillTree_UseSwordfishDictionary==false && PoESkillTree_UseIXDictionary == false && PoeSkillTree_DontUseKeyedTrackedStats==false
        public string Key;
#endif
        //Stores the Display of the PseudoTotal
        private string text = "Not Calculated Yet";
        public string Text
        {
            get => text;
            set
            {
                if (text != value)
                {
                    text = value;
                    OnNotifyPropertyChanged();
                }
            }
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