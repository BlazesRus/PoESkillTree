using System;
using PoESkillTree.Utils;

namespace PoESkillTree.TreeGenerator.Model
{
    /// <summary>
    /// Data class for Constraints with a data object, a target value and a weight.
    /// </summary>
    /// <typeparam name="T">Type of the stored data object.</typeparam>
    public class PseudoCalcConstraint<T> : Notifier, ICloneable
    {
        /// <summary>
        /// Minimum allowed weight (inclusive).
        /// </summary>
        public static int MinWeight
        {
            get { return 1; }
        }

        /// <summary>
        /// Maximim allowed weight (inclusive).
        /// </summary>
        public static int MaxWeight
        {
            get { return 100; }
        }

        private const int DefaultWeight = 100;
        private const float DefaultTargetValue = 1;

        private T _data;

        public T Data
        {
            get { return _data; }
            set { SetProperty(ref _data, value); }
        }

        private float _targetValue = DefaultTargetValue;

        public float TargetValue
        {
            get { return _targetValue; }
            set { SetProperty(ref _targetValue, value); }
        }

        private int _weight = DefaultWeight;

        public int Weight
        {
            get { return _weight; }
            set
            {
                if (value < MinWeight || value > MaxWeight)
                    throw new ArgumentOutOfRangeException("value", value, "must be between MinWeight and MaxWeight");
                SetProperty(ref _weight, value);
            }
        }
        
        private bool _required = false;

        public bool Required
        {
            get { return _required; }
            set
            {
                SetProperty(ref _required, value);
            }
        }
		
        public PseudoCalcConstraint(T data = default(T))
        {
            Data = data;
        }

        private PseudoCalcConstraint(PseudoCalcConstraint<T> toClone)
            : this(toClone.Data)
        {
            TargetValue = toClone.TargetValue;
            Weight = toClone.Weight;
			Required = toClone.Required;
        }

        public object Clone()
        {
            return new PseudoCalcConstraint<T>(this);
        }
    }

    // Just here to specify Constraints in Xaml as DataContext with
    // d:DataContext="{d:DesignInstance model:NonGenericPseudoCalcConstraint}"
    // because Xaml doesn't support generic classes for that.
    public abstract class NonGenericPseudoCalcConstraint : PseudoCalcConstraint<object>
    { }
}