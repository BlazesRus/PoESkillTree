using POESKillTree.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using POESKillTree.TreeGenerator.Model.PseudoAttributes;
using POESKillTree.TreeGenerator.Settings;

namespace POESKillTree.Model
{
    /// <summary>
    /// Data class for settings for TrackedStatsMenuModel
    /// </summary>
    /// <seealso cref="POESKillTree.Utils.Notifier" />
    public class TrackedStatOptions : Notifier
    {
        private string _StatTrackingSavePath;

        /// <summary>
        /// Gets or sets the stat tracking save path.
        /// </summary>
        /// <value>
        /// The stat tracking save path.
        /// </value>
        public string StatTrackingSavePath
        {
            get { return _StatTrackingSavePath; }
            set { SetProperty(ref _StatTrackingSavePath, value); }
        }

        private string[] _TrackingFileList = { "" };

        /// <summary>
        /// Gets or sets the tracking file list. (Holds the names of TrackingStat loading files)
        /// </summary>
        /// <value>
        /// The tracking file list.
        /// </value>
        public string[] TrackingFileList
        {
            get { return _TrackingFileList; }
            set { SetProperty(ref _TrackingFileList, value); }
        }
    }
    ///// <summary>
    ///// Data class for settings for TrackedStatsMenuModelSolver.
    ///// </summary>
    ///// <seealso cref="POESKillTree.TreeGenerator.Settings.SolverSettings" />
    //public class TrackedStatSettings : SolverSettings
    //{
    //    /// <summary>
    //    /// The attribute constraints the solver should try to fulfill.
    //    /// The key is the name of the attribute.
    //    /// Value is a tuple of target value (float) and weight (double).
    //    /// Weight must be between 0 and 1 (both inclusive).
    //    /// </summary>
    //    public readonly Dictionary<string, Tuple<float, double>> AttributeConstraints;

    //    /// <summary>
    //    /// The pseudo attribute constraints the solver should try to fulfill.
    //    /// The key is the name of the attribute.
    //    /// Value is a tuple of target value (float) and weight (double).
    //    /// Weight must be between 0 and 1 (both inclusive).
    //    /// </summary>
    //    public readonly Dictionary<PseudoAttribute, Tuple<float, double>> PseudoAttributeConstraints;

    //    /// <summary>
    //    /// Starting attributes of stats that calculations are based on.
    //    /// (e.g. base attributes, attributes from items)
    //    /// </summary>
    //    public readonly Dictionary<string, float> InitialAttributes;

    //    /// <summary>
    //    /// Creates new AdvancesSolverSettings.
    //    /// </summary>
    //    /// <param name="baseSettings">Base settings to copy.</param>
    //    /// <param name="totalPoints">Maximum for points spent in the result tree. (>= 0)</param>
    //    /// <param name="initialAttributes">Starting attributes of stats that calculations are based on.</param>
    //    /// <param name="attributeConstraints">The attribute constraints the solver should try to fullfill.</param>
    //    /// <param name="pseudoAttributeConstraints">The pseudo attribute constraints the solver should try to fullfill.</param>
    //    /// <param name="weaponClass">WeaponClass used for pseudo attribute calculation.</param>
    //    /// <param name="tags">Tags used for pseudo attribute calculation.</param>
    //    /// <param name="offHand">OffHand used for pseudo attribute calculation.</param>
    //    public TrackedStatSettings(SolverSettings baseSettings,
    //        int totalPoints,
    //        Dictionary<string, float> initialAttributes,
    //        Dictionary<string, Tuple<float, double>> attributeConstraints,
    //        Dictionary<PseudoAttribute, Tuple<float, double>> pseudoAttributeConstraints,
    //        WeaponClass weaponClass, Tags tags, OffHand offHand)
    //        : base(baseSettings)
    //    {
    //        if (totalPoints < 0) throw new ArgumentOutOfRangeException(nameof(totalPoints), totalPoints, "must be >= 0");

    //        AttributeConstraints = attributeConstraints ?? new Dictionary<string, Tuple<float, double>>();
    //        PseudoAttributeConstraints = pseudoAttributeConstraints ?? new Dictionary<PseudoAttribute, Tuple<float, double>>();
    //        InitialAttributes = initialAttributes ?? new Dictionary<string, float>();

    //        if (AttributeConstraints.Values.Any(tuple => tuple.Item2 < 0 || tuple.Item2 > 1))
    //            throw new ArgumentException("Weights need to be between 0 and 1", "attributeConstraints");
    //        if (AttributeConstraints.Values.Any(t => t.Item1 <= 0))
    //            throw new ArgumentException("Target values need to be greater zero", "attributeConstraints");
    //        if (PseudoAttributeConstraints.Values.Any(tuple => tuple.Item2 < 0 || tuple.Item2 > 1))
    //            throw new ArgumentException("Weights need to be between 0 and 1", "pseudoAttributeConstraints");
    //        if (PseudoAttributeConstraints.Values.Any(t => t.Item1 <= 0))
    //            throw new ArgumentException("Target values need to be greater zero", "pseudoAttributeConstraints");
    //    }
    //}
}
