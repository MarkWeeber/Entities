using System.Collections.Generic;
using Unity.Services.CloudSave;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System;

public class CloudSaveManager : SingletonBehaviour<CloudSaveManager>
{
    private const string key = "SaveData";

    public bool IsSignedIn;
    public bool CloudDataFetched;
    private LocalSaveManager localSaveManager;
    private SaveData saveData;
    protected override void Awake()
    {
        dontDestroyOnload = true;
        base.Awake();
        localSaveManager = LocalSaveManager.Instance;
    }

    private async void Start()
    {
        await InitialyzeUnityServicesAsync();
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn)
        {
            IsSignedIn = true;
            saveData = await RetrieveSpecificData<SaveData>(key);
            if (saveData != null)
            {
                CloudDataFetched = true;
            }
            if (localSaveManager.Active)
            {
                if (!saveData.Equals(localSaveManager.SaveData))
                {
                    DateTime cloudTime = DateTime.Parse(saveData.DateTime);
                    DateTime localTime = DateTime.Parse(localSaveManager.SaveData.DateTime);
                    if (cloudTime > localTime)
                    {
                        localSaveManager.SetSaveData(saveData.CoinsCollected, saveData.CurrentHealth);
                    }
                    else if (cloudTime < localTime)
                    {
                        await ForceSaveSingleData(key, LocalSaveManager.Instance.SaveData);
                    }
                }
            }

        }
        else
        {
            IsSignedIn = false;
        }
    }

    private async Task ForceSaveSingleData(string key, object value)
    {
        try
        {
            Dictionary<string, object> oneElement = new Dictionary<string, object>();
            oneElement.Add(key, value);
            Dictionary<string, string> result =
                await CloudSaveService.Instance.Data.Player.SaveAsync(oneElement);
            Debug.Log(
                $"Successfully saved {key}:{value} with updated write lock {result[key]}"
            );
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }
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

    private async Task ListKeys()
    {
        var keys = await CloudSaveService.Instance.Data.Player.ListAllKeysAsync();
        for (int i = 0; i < keys.Count; i++)
        {
            Debug.Log(keys[i].Key);
        }
        var keysCustom = await CloudSaveService.Instance.Data.Custom.ListAllKeysAsync("Logins_1");
        for (int i = 0; i < keysCustom.Count; i++)
        {
            Debug.Log(keysCustom[i].Key);
        }
    }

    private async Task<T> RetrieveSpecificData<T>(string key)
    {
        try
        {
            var results = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { key }
            );

            if (results.TryGetValue(key, out var item))
            {
                return item.Value.GetAs<T>();
            }
            else
            {
                Debug.Log($"There is no such key as {key}!");
            }
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }
        return default;
    }
}

