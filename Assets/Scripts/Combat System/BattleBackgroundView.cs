using UnityEngine;

namespace DemonsLord.CombatSystem
{
    public class BattleBackgroundView : MonoBehaviour
    {
        [SerializeField] private BattleBackgroundCatalog catalog;
        [SerializeField] private PrimitiveType primitiveType = PrimitiveType.Quad;

        private GameObject _backgroundInstance;

        public void ShowForBiome(string biomeId)
        {
            if (catalog == null)
            {
                Debug.LogWarning("[BattleBackgroundView] Catalog is not assigned.", this);
                return;
            }

            Show(catalog.GetByBiomeId(biomeId));
        }

        public void Show(BattleBackgroundDefinition definition)
        {
            Clear();

            if (definition == null)
            {
                return;
            }

            _backgroundInstance = GameObject.CreatePrimitive(primitiveType);
            _backgroundInstance.name = "BattleBackground";
            _backgroundInstance.transform.SetParent(transform, false);
            _backgroundInstance.transform.localPosition = definition.LocalPosition;
            _backgroundInstance.transform.localRotation = Quaternion.identity;
            _backgroundInstance.transform.localScale = definition.Scale;

            var collider = _backgroundInstance.GetComponent<Collider>();
            if (collider != null)
            {
                DestroySafe(collider);
            }

            var renderer = _backgroundInstance.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (definition.Material != null)
                {
                    renderer.sharedMaterial = definition.Material;
                }
                else if (definition.Texture != null)
                {
                    var material = new Material(Shader.Find("Unlit/Texture"));
                    material.mainTexture = definition.Texture;
                    renderer.sharedMaterial = material;
                }
            }
        }

        public void Clear()
        {
            if (_backgroundInstance == null)
            {
                return;
            }

            DestroySafe(_backgroundInstance);
            _backgroundInstance = null;
        }

        private static void DestroySafe(Object target)
        {
            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
