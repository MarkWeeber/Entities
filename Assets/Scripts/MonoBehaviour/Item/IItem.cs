public interface IItem
{
    IItemAction[] ItemActions { get; set; }
    void InitializeActions();
}
