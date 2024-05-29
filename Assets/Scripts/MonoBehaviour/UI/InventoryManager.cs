using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class InventoryManager
{
    private List<InventoryCell> cells;
    public List<InventoryCell> Cells { get => cells; set => cells = value; }

    public EntityManager EntityManager;

    public void TryAddItem(IItem item)
    {
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
