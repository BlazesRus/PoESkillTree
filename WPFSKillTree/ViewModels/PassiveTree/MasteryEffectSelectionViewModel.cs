using PoESkillTree.Common.ViewModels;
using PoESkillTree.Utils.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace PoESkillTree.ViewModels.PassiveTree
{
    public class MasteryEffectSelectionViewModelProxy : BindingProxy<MasteryEffectSelectionViewModel>
    {
    }

    /// <summary>
    /// View model for selecting a mastery effect.
    /// </summary>
    public class MasteryEffectSelectionViewModel : CloseableViewModel<bool>
    {
        public PassiveNodeViewModel Node { get; }
        public IEnumerable<PassiveNodeViewModel> Masteries { get; }

        private ushort InitialNodeSkill;

        public MasteryEffectSelectionViewModel(PassiveNodeViewModel node, IEnumerable<PassiveNodeViewModel> masteries)
        {
            Node = node;
            Masteries = masteries;
            InitialNodeSkill = node.Skill;
        }

        public bool IsEffectEnabled(ushort effect) => !Masteries.Any(x => x.Skill == effect);
        public bool IsEffectChecked(ushort effect) => Node.Skill == effect || Masteries.Any(x => x.Skill == effect);

        public ICommand SetEffect => new RelayCommand<ushort>((effect) =>
        {
            if (effect != InitialNodeSkill)
            {
                if (effect == Node.Id)
                    MasteryDefinitions.SetMasteryLabel(Node);
                else
                {
                    for (int EffectIndex = 0; EffectIndex < Node.MasterEffects.Length; ++EffectIndex)//Find Matching 
                    {
                        if (effect == Node.MasterEffects[EffectIndex].Effect)
                        {
                            MasteryDefinitions.ApplyMasterySelectionStats(Node, Node.MasterEffects[EffectIndex].StatDescriptions, effect);
                            break;
                        }
                    }
                }
                InitialNodeSkill = effect;//Just in case switch between different effects
            }
        });

    }
}
