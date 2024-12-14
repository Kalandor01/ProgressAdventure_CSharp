namespace ProgressAdventure.WorldManagement.Content
{
    public static class ContentType
    {
        public static ContentTypeID AllContentType
        {
            get => all;
        }

        private static readonly ContentTypeID all = 1;

        public static readonly ContentTypeID TERRAIN = all[0];
        public static class Terrain
        {
            public static readonly ContentTypeID FIELD = TERRAIN[0];
            public static readonly ContentTypeID MOUNTAIN = TERRAIN[1];
            public static readonly ContentTypeID OCEAN = TERRAIN[2];
            public static readonly ContentTypeID SHORE = TERRAIN[3];
        }

        public static readonly ContentTypeID STRUCTURE = all[1];
        public static class Structure
        {
            public static readonly ContentTypeID NONE = STRUCTURE[0];
            public static readonly ContentTypeID VILLAGE = STRUCTURE[1];
            public static readonly ContentTypeID KINGDOM = STRUCTURE[2];
            public static readonly ContentTypeID BANDIT_CAMP = STRUCTURE[3];
        }

        public static readonly ContentTypeID POPULATION = all[2];
        public static class Population
        {
            public static readonly ContentTypeID NONE = POPULATION[0];
            public static readonly ContentTypeID HUMAN = POPULATION[1];
            public static readonly ContentTypeID DWARF = POPULATION[2];
            public static readonly ContentTypeID ELF = POPULATION[3];
            public static readonly ContentTypeID DEMON = POPULATION[4];
        }
    }
}
