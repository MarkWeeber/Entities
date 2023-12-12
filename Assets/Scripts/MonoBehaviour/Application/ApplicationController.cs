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
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        
        await InitialyzeUnityServicesAsync();
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn)
        {
            //
        }
        else
        {
            //
        }
        LoadMainScene();
    }

    private async Task InitialyzeUnityServicesAsync()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private void LoadMainScene()
    {
        if (SceneManager.GetActiveScene().name != MainSceneName)
        {
            SceneManager.LoadScene(MainSceneName);
        }
    }
}
