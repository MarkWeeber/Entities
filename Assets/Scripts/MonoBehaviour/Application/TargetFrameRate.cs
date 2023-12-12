using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFrameRate : MonoBehaviour
{
    public int _TargetFrameRate = -1;
    private void Start()
    {
        Application.targetFrameRate = _TargetFrameRate;
    }
}
