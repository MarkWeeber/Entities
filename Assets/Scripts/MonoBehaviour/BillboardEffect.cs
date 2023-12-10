using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardEffect : MonoBehaviour
{
    private Camera mainCam;
    public bool applyOnlyAtStart = true;
    private RectTransform rectTransform;
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCam = Camera.main;
        if (applyOnlyAtStart)
        {
            ApplyBillBoardEffect();
        }
    }
    private void LateUpdate()
    {
        if (!applyOnlyAtStart)
        {
            ApplyBillBoardEffect();
        }
    }

    private void ApplyBillBoardEffect()
    {
        if (rectTransform != null)
        {
            rectTransform.rotation = mainCam.transform.rotation;
            rectTransform.rotation = Quaternion.Euler(0f, rectTransform.rotation.eulerAngles.y, 0f);
        }
        else
        {
            transform.rotation = mainCam.transform.rotation;
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        }
    }
}