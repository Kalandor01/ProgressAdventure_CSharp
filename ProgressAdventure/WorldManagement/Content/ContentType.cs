using PACommon.Enums;
using ProgressAdventure.ConfigManagement;

namespace ProgressAdventure.WorldManagement.Content
{
    public class ContentType : AdvancedEnumTree<ContentType>
    {
        protected static readonly bool isClearable = UpdateIsClearable(true);
        protected static readonly bool isRemovale = UpdateIsRemovable(true);

        /// <summary>
        /// Loads the default values of the enum.
        /// </summary>
        static ContentType()
        {
            var t = Terrain.FIELD;
            var s = Structure.NONE;
            var p = Population.NONE;
        }

        public static readonly EnumTreeValue<ContentType> _TERRAIN = AddValue(null, ConfigUtils.MakeNamespacedString(nameof(Terrain).ToLower()));
        public static class Terrain
        {
            public static readonly EnumTreeValue<ContentType> FIELD = AddValue(_TERRAIN, nameof(FIELD).ToLower());
            public static readonly EnumTreeValue<ContentType> MOUNTAIN = AddValue(_TERRAIN, nameof(MOUNTAIN).ToLower());
            public static readonly EnumTreeValue<ContentType> OCEAN = AddValue(_TERRAIN, nameof(OCEAN).ToLower());
            public static readonly EnumTreeValue<ContentType> SHORE = AddValue(_TERRAIN, nameof(SHORE).ToLower());
        }

        public static readonly EnumTreeValue<ContentType> _STRUCTURE = AddValue(null, ConfigUtils.MakeNamespacedString(nameof(Structure).ToLower()));
        public static class Structure
        {
            public static readonly EnumTreeValue<ContentType> NONE = AddValue(_STRUCTURE, nameof(NONE).ToLower());
            public static readonly EnumTreeValue<ContentType> VILLAGE = AddValue(_STRUCTURE, nameof(VILLAGE).ToLower());
            public static readonly EnumTreeValue<ContentType> KINGDOM = AddValue(_STRUCTURE, nameof(KINGDOM).ToLower());
            public static readonly EnumTreeValue<ContentType> BANDIT_CAMP = AddValue(_STRUCTURE, nameof(BANDIT_CAMP).ToLower());
        }
        
        public static readonly EnumTreeValue<ContentType> _POPULATION = AddValue(null, ConfigUtils.MakeNamespacedString(nameof(Population).ToLower()));
        public static class Population
        {
            public static readonly EnumTreeValue<ContentType> NONE = AddValue(_POPULATION, nameof(NONE).ToLower());
            public static readonly EnumTreeValue<ContentType> HUMAN = AddValue(_POPULATION, nameof(HUMAN).ToLower());
            public static readonly EnumTreeValue<ContentType> DWARF = AddValue(_POPULATION, nameof(DWARF).ToLower());
            public static readonly EnumTreeValue<ContentType> ELF = AddValue(_POPULATION, nameof(ELF).ToLower());
            public static readonly EnumTreeValue<ContentType> DEMON = AddValue(_POPULATION, nameof(DEMON).ToLower());
        }
    }
}
