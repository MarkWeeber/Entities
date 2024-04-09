using UnityEngine;
using Zenject;

public class GameSettings : MonoBehaviour
{
    public PlayerConfig playerConfig;
    //public GameSettings(PlayerConfig playerConfig)
    //{
    //    this.playerConfig = playerConfig;
    //}

    [Inject]
    private void Init(PlayerConfig playerConfig)
    {
        this.playerConfig = playerConfig;
    }

    private void Awake()
    {
    }
}
