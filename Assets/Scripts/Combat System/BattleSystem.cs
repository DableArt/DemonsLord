using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    public void OnPlayerAttack()
    {
        FindObjectOfType<BattleManager>()?.OnPlayerAttack();
    }

    public void OnPlayerDefend()
    {
        FindObjectOfType<BattleManager>()?.OnPlayerDefend();
    }

    public void OnPlayerWait()
    {
        FindObjectOfType<BattleManager>()?.OnPlayerWait();
    }

    public void OnPlayerUltimate()
    {
        FindObjectOfType<BattleManager>()?.OnPlayerUltimate();
    }
}
