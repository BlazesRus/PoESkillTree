using System;
using System.Collections.Generic;

namespace POESKillTree.SkillTreeFiles
{
    public enum NodeType
    {
        Normal,
        Notable,
        Keystone,
        Mastery,
        JewelSocket
    }

    public class SkillNode
    {
        public const string DefaultJewelImage = "Art/2DArt/SkillIcons/passives/MasteryBlank.png";
        public const string DefaultJewelKey = "normal_Art/2DArt/SkillIcons/passives/MasteryBlank.png";
        public string JewelKey { get => Icon==""? DefaultJewelKey : "normal_" + Icon; }
        public static float[] SkillsPerOrbit = {1, 6, 12, 12, 40};
        public static float[] OrbitRadii = {0, 81.5f, 163, 326, 489};
        public Dictionary<string, IReadOnlyList<float>> Attributes;
        public HashSet<int> Connections = new HashSet<int>();
        public List<SkillNode> Neighbor = new List<SkillNode>();
        // The subset of neighbors to which connections should be drawn.
        public readonly List<SkillNode> VisibleNeighbors = new List<SkillNode>();
        public SkillNodeGroup SkillNodeGroup;
        public int A; // "a": 3,
        public string[] attributes; // "sd": ["8% increased Block Recovery"],
        public int Da; // "da": 0,
        public int G; // "g": 1,
        public int Ia; //"ia": 0,
        public string Icon; // icon "icon": "Art/2DArt/SkillIcons/passives/tempint.png",
        public UInt16 Id; // "id": -28194677,
        public NodeType Type; // "ks", "not", "m", "isJewelSocket"
        public List<ushort> LinkId = new List<ushort>(); // "out": []
        public string Name; //"dn": "Block Recovery",
        public int Orbit; //  "o": 1,
        public int OrbitIndex; // "oidx": 3,
        public int Sa; //s "sa": 0,
        public bool IsSkilled = false;
        public int? Spc;
        public bool IsMultipleChoice; //"isMultipleChoice": false
        public bool IsMultipleChoiceOption; //"isMultipleChoiceOption": false
        public int passivePointsGranted; //"passivePointsGranted": 1
        public string ascendancyName; //"ascendancyName": "Raider"
        public bool IsAscendancyStart; //"isAscendancyStart": false
        public string[] reminderText;

        public Vector2D Position
        {
            get
            {
                if (SkillNodeGroup == null) return new Vector2D();
                double d = OrbitRadii[Orbit];
                return (SkillNodeGroup.Position - new Vector2D(d * Math.Sin(-Arc), d * Math.Cos(-Arc)));
            }
        }
        public double Arc => GetOrbitAngle(OrbitIndex, (int) SkillsPerOrbit[Orbit]);

        public string IconKey
        {
            get
            {
                string iconPrefix;
                switch (Type)
                {
                    case NodeType.JewelSocket:
                        if(Icon=="")//Set to use default key if icon is set to blank for whatever reason
                        {
                            return JewelKey;
                        }
                        else if(Icon!= DefaultJewelImage)
                        {
                            //Int Jewels
                            if (Id == POESKillTree.SkillTreeFiles.ConvertedJewelData.JSlot_Int_WitchID)//Jewel Slot directly north of Witch starting area
                            { return "JSlotIcon_Int_Witch"; }
                            else if (Id == POESKillTree.SkillTreeFiles.ConvertedJewelData.JSlot_Int_ScionID)//Jewel slot far NE of Scion Starting Area;} Nearest Jewel to CI area (Int Threshold Jewel Slot)
                            { return "JSlotIcon_Int_Scion"; }
                            else if (Id == POESKillTree.SkillTreeFiles.ConvertedJewelData.JSlot_Int_WitchShadowID)//NE from center jewel slot between Witch and shadow areas
                            { return "JSlotIcon_Int_WitchShadow"; }
                            else if (Id == POESKillTree.SkillTreeFiles.ConvertedJewelData.JSlot_Int_TemplarWitchID)//Jewel slot north-west of Scion area;} At road between Templar and Witch areas
                            { return "JSlotIcon_Int_TemplarWitch"; }
                            //Str Jewels
                            else if (Id == POESKillTree.SkillTreeFiles.ConvertedJewelData.JSlot_Str_WarriorDuelistID)//Jewel slot south-west of Scion area;} At road between Marauder and Duelist areas
                            { return "JSlotIcon_Str_WarriorDuelist"; }
                            else if (Id == POESKillTree.SkillTreeFiles.ConvertedJewelData.JSlot_Str_WarriorTemplarScionID)//Jewel slot west of Scion area;} At road between Marauder and Templar areas
                            { return "JSlotIcon_Str_WarriorTemplarScion"; }
                            else if (Id == POESKillTree.SkillTreeFiles.ConvertedJewelData.JSlot_Str_FarWarTempScionID)//Jewel slot far west of Scion area;} At road between Marauder and Templar areas;} Nearest jewel slot to Resolute Technique
                            { return "JSlotIcon_Str_FarWarTempScion"; }
                            else if (Id == POESKillTree.SkillTreeFiles.ConvertedJewelData.JSlot_Str_WarriorID)//Jewel slot west of Marauder area
                            { return "JSlotIcon_Str_Warrior"; }
                            //Dex Jewels
                            else if (Id == POESKillTree.SkillTreeFiles.ConvertedJewelData.JSlot_Dex_ShadowRangerID)//Jewel Slot east of Scion starting area between Shadow and Ranger areas(above Ranger area);} Nearest jewel slot to Charisma passive node
                            { return "JSlotIcon_Dex_ShadowRanger"; }
                            else if (Id == POESKillTree.SkillTreeFiles.ConvertedJewelData.JSlot_Dex_RangerID)//Jewel slot east of Ranger area(Jewel10)
                            { return "JSlotIcon_Dex_Ranger"; }
                            else if (Id == POESKillTree.SkillTreeFiles.ConvertedJewelData.JSlot_Dex_RangerDuelistID)//Jewel slot south-east of Scion area;} At road between Ranger and Duelist areas
                            { return "JSlotIcon_Dex_RangerDuelist"; }
                            //Hybrid Jewels
                            else if (Id == POESKillTree.SkillTreeFiles.ConvertedJewelData.JSlot_StrInt_TemplarID)//Jewel slot west of Templar starting area
                            { return "JSlotIcon_StrInt_Templar"; }
                            else if (Id == POESKillTree.SkillTreeFiles.ConvertedJewelData.JSlot_StrInt_ScionID)//Scion Jewel Slot west of starting area
                            { return "JSlotIcon_StrInt_Scion"; }
                            else if (Id == POESKillTree.SkillTreeFiles.ConvertedJewelData.JSlot_DexInt_ShadowID)//Jewel slot east of Shadow starting area
                            { return "JSlotIcon_DexInt_Shadow"; }
                            else if (Id == POESKillTree.SkillTreeFiles.ConvertedJewelData.JSlot_DexInt_ScionID)//Scion jewel slot east of starting area
                            { return "JSlotIcon_DexInt_Scion"; }
                            else if (Id == POESKillTree.SkillTreeFiles.ConvertedJewelData.JSlot_StrDex_ScionID)//Scion Jewel Slot south of starting area
                            { return "JSlotIcon_StrDex_Scion"; }
                            else if (Id == POESKillTree.SkillTreeFiles.ConvertedJewelData.JSlot_StrDex_DuelistID)//Jewel slot south of Duelist starting area
                            { return "JSlotIcon_StrDex_Duelist"; }
                            //Non-Threshold Jewel Slots
                            else if (Id == POESKillTree.SkillTreeFiles.ConvertedJewelData.JSlot_Neutral_AcrobaticsID)
                            { return "JSlotIcon_Neutral_Acrobatics"; }
                            else if (Id == POESKillTree.SkillTreeFiles.ConvertedJewelData.JSlot_Neutral_PointBlankID)
                            { return "JSlotIcon_Neutral_PointBlank"; }
                            else if (Id == POESKillTree.SkillTreeFiles.ConvertedJewelData.JSlot_Neutral_MinionInstabilityID)
                            { return "JSlotIcon_Neutral_MinionInstability"; }
                            else if (Id == POESKillTree.SkillTreeFiles.ConvertedJewelData.JSlot_Neutral_IronGripID)
                            { return "JSlotIcon_Neutral_IronGrip"; }
                            else
                            {
                                return JewelKey;
                            }
                        }
                        else
                        {
                            return "normal_" + Icon;
                        }
                    case NodeType.Normal:
                        iconPrefix = "normal";
                        break;
                    case NodeType.Notable:
                        iconPrefix = "notable";
                        break;
                    case NodeType.Keystone:
                        iconPrefix = "keystone";
                        break;
                    case NodeType.Mastery:
                        iconPrefix = "mastery";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return iconPrefix + "_" + Icon;
            }
        }

        private static double GetOrbitAngle(int orbitIndex, int maxNodePositions)
        {
            // An orbit with 40 node placements has specific angles for certain orbit indices.
            /*if( max_node_positions == 40 )
            {
                switch( orbit_index )
                {
                case  0: return GetOrbitAngle(  0, 12 );
                case  1: return GetOrbitAngle(  0, 12 ) + 1 * 10.0f;
                case  2: return GetOrbitAngle(  0, 12 ) + 2 * 10.0f;
                case  3: return GetOrbitAngle(  1, 12 );
                case  4: return GetOrbitAngle(  1, 12 ) + 1 * 10.0f;
                case  5: return GetOrbitAngle(  1, 12 ) + 1 * 15.0f;
                case  6: return GetOrbitAngle(  1, 12 ) + 2 * 10.0f;
                case  7: return GetOrbitAngle(  2, 12 );
                case  8: return GetOrbitAngle(  2, 12 ) + 1 * 10.0f;
                case  9: return GetOrbitAngle(  2, 12 ) + 2 * 10.0f;
                case 10: return GetOrbitAngle(  3, 12 );
                case 11: return GetOrbitAngle(  3, 12 ) + 1 * 10.0f;
                case 12: return GetOrbitAngle(  3, 12 ) + 2 * 10.0f;
                case 13: return GetOrbitAngle(  4, 12 );
                case 14: return GetOrbitAngle(  4, 12 ) + 1 * 10.0f;
                case 15: return GetOrbitAngle(  4, 12 ) + 1 * 15.0f;
                case 16: return GetOrbitAngle(  4, 12 ) + 2 * 10.0f;
                case 17: return GetOrbitAngle(  5, 12 );
                case 18: return GetOrbitAngle(  5, 12 ) + 1 * 10.0f;
                case 19: return GetOrbitAngle(  5, 12 ) + 2 * 10.0f;
                case 20: return GetOrbitAngle(  6, 12 );
                case 21: return GetOrbitAngle(  6, 12 ) + 1 * 10.0f;
                case 22: return GetOrbitAngle(  6, 12 ) + 2 * 10.0f;
                case 23: return GetOrbitAngle(  7, 12 );
                case 24: return GetOrbitAngle(  7, 12 ) + 1 * 10.0f;
                case 25: return GetOrbitAngle(  7, 12 ) + 1 * 15.0f;
                case 26: return GetOrbitAngle(  7, 12 ) + 2 * 10.0f;
                case 27: return GetOrbitAngle(  8, 12 );
                case 28: return GetOrbitAngle(  8, 12 ) + 1 * 10.0f;
                case 29: return GetOrbitAngle(  8, 12 ) + 2 * 10.0f;
                case 30: return GetOrbitAngle(  9, 12 );
                case 31: return GetOrbitAngle(  9, 12 ) + 1 * 10.0f;
                case 32: return GetOrbitAngle(  9, 12 ) + 2 * 10.0f;
                case 33: return GetOrbitAngle( 10, 12 );
                case 34: return GetOrbitAngle( 10, 12 ) + 1 * 10.0f;
                case 35: return GetOrbitAngle( 10, 12 ) + 1 * 15.0f;
                case 36: return GetOrbitAngle( 10, 12 ) + 2 * 10.0f;
                case 37: return GetOrbitAngle( 11, 12 );
                case 38: return GetOrbitAngle( 11, 12 ) + 1 * 10.0f;
                case 39: return GetOrbitAngle( 11, 12 ) + 2 * 10.0f;
                }
            }*/

            return 2 * Math.PI * orbitIndex / maxNodePositions;
        }
    }
}