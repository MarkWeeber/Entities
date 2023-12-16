using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationController : SingletonBehaviour<ApplicationController>
{
    public const string MainSceneName = "MainScene";


    protected override void Awake()
    {
        dontDestroyOnload = true;
        base.Awake();
    }

    private void Start()
    {
        LoadMainScene();
    }

    private void LoadMainScene()
    {
        if (SceneManager.GetActiveScene().name != MainSceneName)
        {
            SceneManager.LoadScene(MainSceneName);
        }
    }
}
