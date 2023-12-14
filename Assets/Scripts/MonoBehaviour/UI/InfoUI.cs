using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InfoUI : SingletonBehaviour<InfoUI>
{
    [Header("References")]
    [SerializeField] private Transform informationContainer;
    [Header("Settings")]
    [SerializeField] private bool showConsoleMessages = false;
    [SerializeField] private float fadeOutTime = 3f;
    [SerializeField] private Color noteColor;
    [SerializeField] private Color warningColor;
    [SerializeField] private Color errorColor;
    [SerializeField] private Color successColor;

    private List<TMP_Text> textList;
    private int index = -1;
    private int count = 0;
    

    protected override void Awake()
    {
        base.Awake();
        textList = informationContainer.GetComponentsInChildren<TMP_Text>().ToList();
        count = textList.Count;
        if (showConsoleMessages)
        {
            Application.logMessageReceived += HandleConsoleMessages;
        }
    }

    private void OnDestroy()
    {
        if (showConsoleMessages)
        {
            Application.logMessageReceived -= HandleConsoleMessages;
        }
    }

    private void Update()
    {
        HandleInformationMessageShow();
    }

    private void HandleInformationMessageShow()
    {
        TMP_Text[] sortedArray = textList.OrderByDescending(t => t.color.a).ToArray(); // sorting
        for (int i = 0; i < sortedArray.Length; i++)
        {
            sortedArray[i].transform.SetSiblingIndex(i);
        }
        foreach (TMP_Text _text in textList)
        {
            float alpha = _text.color.a;
            float timer = alpha * fadeOutTime;
            if (alpha > 0.005f)
            {
                timer -= Time.deltaTime;
                _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, Mathf.Lerp(0f, 1f, Mathf.Abs(timer / fadeOutTime)));
            }
        }
    }

    public void SendInformation(string message, MessageType infoMessageType = MessageType.NOTE)
    {
        index++;
        if (index >= count)
        {
            index = 0;
        }
        textList[index].text = message;
        switch (infoMessageType)
        {
            case MessageType.NOTE:
                textList[index].color = noteColor;
                break;
            case MessageType.WARNING:
                textList[index].color = warningColor;
                break;
            case MessageType.ERROR:
                textList[index].color = errorColor;
                break;
            case MessageType.SUCCESS:
                textList[index].color = successColor;
                break;
            default:
                break;
        }
    }

    private void HandleConsoleMessages(string condition, string stackTrace, LogType type)
    {
        MessageType messageType = MessageType.NOTE;
        switch (type)
        {
            case LogType.Error:
                messageType = MessageType.ERROR;
                break;
            case LogType.Assert:
                messageType = MessageType.NOTE;
                break;
            case LogType.Warning:
                messageType = MessageType.WARNING;
                break;
            case LogType.Log:
                messageType = MessageType.NOTE;
                break;
            case LogType.Exception:
                messageType = MessageType.ERROR;
                break;
            default:
                break;
        }
        SendInformation(condition + " + " + stackTrace, messageType);
    }
}

public enum MessageType
{
    NOTE = 1,
    WARNING = 2,
    ERROR = 3,
    SUCCESS = 4,
}