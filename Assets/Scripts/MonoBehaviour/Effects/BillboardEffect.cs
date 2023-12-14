using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardEffect : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    public bool applyOnlyAtStart = false;
    private Camera mainCam;
    private RectTransform rectTransform;
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        mainCam = Camera.main;
        if (applyOnlyAtStart)
        {
            ApplyBillBoardEffect();
            FollowTarget();
        }
    }
    private void LateUpdate()
    {
        if (!applyOnlyAtStart)
        {
            ApplyBillBoardEffect();
            FollowTarget();
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
    private void FollowTarget()
    {
        if (targetTransform != null)
        {
            rectTransform.position = targetTransform.position;
        }
    }
}