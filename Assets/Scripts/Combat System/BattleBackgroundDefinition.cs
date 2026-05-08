using System.Collections.Generic;
using UnityEngine;

namespace DemonsLord.CombatSystem
{
    [CreateAssetMenu(fileName = "BattleBackgroundDefinition", menuName = "Combat/Battle Background Definition")]
    public class BattleBackgroundDefinition : ScriptableObject
    {
        public string BiomeId = "plains";
        public Material Material;
        public Texture Texture;
        public Vector3 Scale = new Vector3(16f, 9f, 1f);
        public Vector3 LocalPosition = new Vector3(0f, 0f, 10f);
    }

    [CreateAssetMenu(fileName = "BattleBackgroundCatalog", menuName = "Combat/Battle Background Catalog")]
    public class BattleBackgroundCatalog : ScriptableObject
    {
        public List<BattleBackgroundDefinition> Backgrounds = new List<BattleBackgroundDefinition>();

        public BattleBackgroundDefinition GetByBiomeId(string biomeId)
        {
            for (int i = 0; i < Backgrounds.Count; i++)
            {
                var definition = Backgrounds[i];
                if (definition != null && string.Equals(definition.BiomeId, biomeId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return definition;
                }
            }

            for (int i = 0; i < Backgrounds.Count; i++)
            {
                if (Backgrounds[i] != null)
                {
                    return Backgrounds[i];
                }
            }

            return null;
        }
    }
}
