using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationController : MonoBehaviour
{
    private static ApplicationController instance;
    public static ApplicationController Instance { get => instance; }

    public const string MainSceneName = "MainScene";

    private async void Start()
    {
        DontDestroyOnLoad(gameObject);
        await InitialyzeUnityServices();
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Success");
        }
        else
        {
            Debug.Log("Failed");
        }
        //var data = new Dictionary<string, object> { { "MySaveKey", "HelloWorld" } };
        //await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        //SceneManager.LoadScene(MainSceneName);
    }

    private async Task InitialyzeUnityServices()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void LoadSomeData()
    {
        //Task<Dictionary<string, Item>> savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "key" });

        //Debug.Log("Done: " + savedData["key"]);
    }
}
