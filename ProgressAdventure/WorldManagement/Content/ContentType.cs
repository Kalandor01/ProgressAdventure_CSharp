namespace ProgressAdventure.WorldManagement.Content
{
    public static class ContentType
    {
        #region Public properties
        public static ContentTypeID BaseContentType
        {
            get => baseType;
        }
        public static ContentTypeID TerrainContentType
        {
            get => terrain;
        }
        public static ContentTypeID StructureContentType
        {
            get => structure;
        }
        public static ContentTypeID PopulationContentType
        {
            get => population;
        }
        #endregion

        private static readonly ContentTypeID baseType = 1;


        private static readonly ContentTypeID terrain = baseType[0];
        public static class Terrain
        {
            public static readonly ContentTypeID FIELD = terrain[0];
            public static readonly ContentTypeID MOUNTAIN = terrain[1];
            public static readonly ContentTypeID OCEAN = terrain[2];
            public static readonly ContentTypeID SHORE = terrain[3];
        }

        private static readonly ContentTypeID structure = baseType[1];
        public static class Structure
        {
            public static readonly ContentTypeID NONE = structure[0];
            public static readonly ContentTypeID VILLAGE = structure[1];
            public static readonly ContentTypeID KINGDOM = structure[2];
            public static readonly ContentTypeID BANDIT_CAMP = structure[3];
        }

        private static readonly ContentTypeID population = baseType[2];
        public static class Population
        {
            public static readonly ContentTypeID NONE = population[0];
            public static readonly ContentTypeID HUMAN = population[1];
            public static readonly ContentTypeID DWARF = population[2];
            public static readonly ContentTypeID ELF = population[3];
            public static readonly ContentTypeID DEMON = population[4];
        }
    }
}
