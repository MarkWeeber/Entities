using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIFPSCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private float updateRateInSeconds = 0.2f;

    private float updateTimer = 0;

    private void Update()
    {
        if (updateTimer <= 0f)
        {
            int fps = (int)Mathf.RoundToInt(1f / Time.deltaTime);
            text.text = fps.ToString();
            updateTimer = updateRateInSeconds;
        }
        else
        {
            updateTimer -= Time.deltaTime;
        }
        
    }
}
