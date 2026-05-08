using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FixedScreen : MonoBehaviour
{
    CanvasScaler canvasScaler;


    void Start()
    {
        canvasScaler = GetComponent<CanvasScaler>();

        //화면 꺼짐 방지
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        //SetFixedScreen();
    }


    void Update()
    {
        SetFixedScreen();
    }

    void SetFixedScreen()
    {
        //Default 해상도 비율
        float fixedAspectRatio = 16f / 9f;

        //현재 해상도의 비율
        float currentAspectRatio = (float)Screen.width / (float)Screen.height;

        //현재 해상도 가로 비율이 더 길 경우
        if (currentAspectRatio > fixedAspectRatio) canvasScaler.matchWidthOrHeight = 1;
        //현재 해상도의 세로 비율이 더 길 경우
        else if (currentAspectRatio < fixedAspectRatio) canvasScaler.matchWidthOrHeight = 0;

        //Debug.Log("fixedAspectRatio : " + fixedAspectRatio);
        //Debug.Log("currentAspectRatio : " + currentAspectRatio);
    }
}
