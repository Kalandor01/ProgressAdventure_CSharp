using NPrng;
using NPrng.Generators;

namespace ProjectAdventure
{
    public static class SaveData
    {
        #region Public fields
        public static IPseudoRandomGenerator mainSeed;
        #endregion

        #region Public functions
        public static void InitialiseVariables(IPseudoRandomGenerator? mainSeed = null)
        {
            SaveData.mainSeed = mainSeed is not null ? mainSeed : new SplittableRandom();
        }
        #endregion
    }
}
