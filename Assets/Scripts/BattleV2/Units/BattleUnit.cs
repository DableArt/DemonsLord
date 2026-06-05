using UnityEngine;

namespace BattleV2.Units
{
    public enum BattleUnitSide
    {
        Player = 0,
        Enemy = 1
    }

    public class BattleUnit : MonoBehaviour
    {
        [SerializeField] private BattleUnitSide side;
        [SerializeField] private string unitId = string.Empty;

        public BattleUnitSide Side => side;
        public string UnitId => unitId;

        public void Initialize(BattleUnitSide newSide, string newUnitId)
        {
            side = newSide;
            unitId = newUnitId ?? string.Empty;
        }
    }
}
