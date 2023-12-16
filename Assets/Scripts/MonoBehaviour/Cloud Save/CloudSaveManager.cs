using System.Collections.Generic;
using System;
using Unity.Services.CloudSave;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class CloudSaveManager : SingletonBehaviour<CloudSaveManager>
{
    public bool IsSignedIn;
    protected override void Awake()
    {
        dontDestroyOnload = true;
        base.Awake();
    }

    private async void Start()
    {

        await InitialyzeUnityServicesAsync();
        if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn)
        {
            IsSignedIn = true;
        }
        else
        {
            IsSignedIn = false;
        }
        await ForceSaveSingleData("PlayerHealth", LocalSaveManager.Instance.SaveData.CurrentHealth.ToString());
        await ListKeys();
    }

    private async Task ForceSaveSingleData(string key, string value)
    {
        try
        {
            Dictionary<string, object> oneElement = new Dictionary<string, object>();

            // It's a text input field, but let's see if you actually entered a number.
            if (Int32.TryParse(value, out int wholeNumber))
            {
                oneElement.Add(key, wholeNumber);
            }
            else if (Single.TryParse(value, out float fractionalNumber))
            {
                oneElement.Add(key, fractionalNumber);
            }
            else
            {
                oneElement.Add(key, value);
            }

            // Saving the data without write lock validation by passing the data as an object instead of a SaveItem
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
}

