using UnityEngine;

public class GridManagerBehaviour : MonoBehaviour
{
    public GridSO GridSO;
    public GridBehaviour GridBehaviour;

    public void Init()
    {
        foreach (var cell in GridSO.Cells)
        {
            GameObject obj = cell.Cell.Unit.gameObject;

            Instantiate(obj, GridBehaviour.GetCellWorldPosition(cell.point), Quaternion.identity);
        }
    }

    private void Start()
    {
        Init();
    }
}
