using UnityEngine;

namespace DemonsLord.CombatSystem
{
    public class SampleBattleStarter : MonoBehaviour
    {
        [SerializeField] private CombatTuning tuning;
        [SerializeField] private string tileId = "grass";
        [SerializeField] private string biomeId = "plains";

        [Header("Player")]
        [SerializeField] private UnitStatsDefinition playerStatsDefinition;
        [SerializeField] private UnitHighStatsData playerHighStats = new UnitHighStatsData();

        [Header("NPC")]
        [SerializeField] private UnitStatsDefinition npcStatsDefinition;
        [SerializeField] private UnitHighStatsData npcHighStats = new UnitHighStatsData();

        private CombatService _combatService;

        public BattleSession CurrentSession
        {
            get { return _combatService != null ? _combatService.CurrentSession : null; }
        }

        [ContextMenu("Start Sample Battle")]
        public void StartSampleBattle()
        {
            if (playerStatsDefinition == null || npcStatsDefinition == null)
            {
                Debug.LogError("[SampleBattleStarter] Assign both player and NPC UnitStatsDefinition assets.", this);
                return;
            }

            if (_combatService == null)
            {
                _combatService = new CombatService();
            }

            var context = new BattleWorldContext
            {
                TileId = tileId,
                BiomeId = biomeId,
                Player = new BattleParticipantSetup
                {
                    UnitId = "player",
                    DisplayName = "Player",
                    StatsDefinition = playerStatsDefinition,
                    HighStats = playerHighStats
                },
                Npc = new BattleParticipantSetup
                {
                    UnitId = "npc",
                    DisplayName = "NPC",
                    StatsDefinition = npcStatsDefinition,
                    HighStats = npcHighStats
                }
            };

            var session = _combatService.StartBattle(context, tuning);
            Debug.Log("[SampleBattleStarter] Battle started. Current side: " + session.CurrentSide.Value, this);
        }

        private void OnDestroy()
        {
            if (_combatService != null)
            {
                _combatService.Dispose();
                _combatService = null;
            }
        }
    }
}
