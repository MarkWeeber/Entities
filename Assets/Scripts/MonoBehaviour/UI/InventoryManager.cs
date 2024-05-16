using System.Collections.Generic;
using UnityEngine;

public class InventoryManager
{
    private List<InventoryCell> cells;
    public List<InventoryCell> Cells { get => cells; set => cells = value; }

    public void TryAddItem(IItem item)
    {
        Debug.Log("Trying to add item");
        foreach (var cell in cells)
        {
            if (!cell.Contained)
            {
                cell.ContainItem(item);
                break;
            }
        }
    }


}
