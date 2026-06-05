using BattleV2.Data;
using UnityEngine;

namespace BattleV2.Scene
{
    public class BattleSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private BattleSceneController battleSceneController;

        private void Awake()
        {
            if (battleSceneController == null)
            {
                battleSceneController = GetComponent<BattleSceneController>();
            }
        }

        private void Start()
        {
            if (battleSceneController == null)
            {
                Debug.LogError("[BattleV2] BattleSceneController reference is missing.");
                return;
            }

            var entry = BattleEntryStorage.ConsumeOrDefault();
            battleSceneController.Initialize(entry);
        }
    }
}
