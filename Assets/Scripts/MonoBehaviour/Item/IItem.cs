using UnityEngine;

public interface IItem
{
    string ItemName { get; }
    IItemAction[] ItemActions { get; }
    void InitializeActions();
    Sprite Image { get; }
}
