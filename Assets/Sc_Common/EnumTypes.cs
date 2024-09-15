using UnityEngine;

public class EnumTypes : MonoBehaviour
{
    public enum AttackType
    {
        DevAttack,
        DevAction1,
        DevAction2,
        DevAction3
    }

    public struct GameStateInfo
    {
        public int turnCounter;
        public int selfIndex;
        public int aiBots;
        public int pcBots;
        public float[] aiHPArray;
        public float[] pcHPArray;
        public float[] aiHPArrayPct;
        public float[] pcHPArrayPct;
        public float[] aiEnergyArrayPct;
        public float[] pcEnergyArrayPct;
    }

    public enum AdaptationType
    {
        None,
        IncreaseRAM,
        BiggerHardDrives,
        HotSpares,
        UpgradedFirewall,
        EfficientRouting
    }
}
