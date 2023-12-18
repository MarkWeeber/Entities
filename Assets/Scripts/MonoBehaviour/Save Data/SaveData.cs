using System;
using UnityEngine;

[System.Serializable]
public class SaveData : IEquatable<SaveData>
{
    public string DateTime;
    public uint CoinsCollected;
    public float CurrentHealth;

    public bool Equals(SaveData other)
    {
        if (other == null)
        {
            return false;
        }
        else if (String.Equals(DateTime, other.DateTime) && CoinsCollected == other.CoinsCollected && CurrentHealth == other.CurrentHealth)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

public interface SaveDataSender
{

}