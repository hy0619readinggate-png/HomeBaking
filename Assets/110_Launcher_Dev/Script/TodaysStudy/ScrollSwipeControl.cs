using DoDoEng.Common;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ScrollSwipeControl : MonoBehaviour
{
    // Properties
    public bool Interactable
    {
        get => isInteractable;
        set => isInteractable = value;
    }
    // Methods
    public void Activate(bool isActivate)
    {
        this.isActivate = isActivate;
    }
    public void SetIndex(int idx)
    {
        Debug.Log($"SetIndex: {idx}");
        pos = new float[transform.childCount];
        if (pos.Length > idx)
        {
            float distance = 1f / (pos.Length - 1f);

            for (int i = 0; i < pos.Length; i++)
            {
                pos[i] = distance * i;
            }

            scrollSpeed = 0;
            scrollPos = pos[idx];
            scrollbar.value = scrollPos;

            currentIdx = idx;
            jumpIdx = idx;
            OnChangeIndex?.Invoke(currentIdx);
        }
    }
    public void JumpIndex(int idx)
    {
        jumpIdx = idx;
    }

    // Events
    public event Action OnBeginDrag;
    public event Action OnEndDrag;
    public event Action<int> OnChangeIndex;



    // Fields
    private float scrollPosPrev = 0;
    private float scrollPos = 0;
    private float[] pos;
    private float scrollSpeed = 0;
    private bool isActivate = true;
    private int currentIdx = -1;
    private bool isDrag = false;
    private int jumpIdx = -1;
    private bool isDown = false;
    private bool isInteractable = true;



    // Unity Inspectors
    [Header("★ Bindings")]
    [SerializeField] Scrollbar scrollbar;
    [Header("★ Sound")]
    [SerializeField] private AudioClip scrollCLIP = null;
    [Header("★ Config")]
    [SerializeField] float scaleValue = 0.6f;

    // Unity Messages
    private void Update()
    {
        if (isActivate && transform.childCount > 0)
        {
            try
            {
                pos = new float[transform.childCount];
                float distance = 1f / (pos.Length - 1f);

                for (int i = 0; i < pos.Length; i++)
                {
                    pos[i] = distance * i;
                }

                if (currentIdx != jumpIdx)
                {
                    scrollSpeed = 0;
                    scrollPos = pos[jumpIdx];
                    scrollbar.value = Mathf.Lerp(scrollbar.value, pos[jumpIdx], 0.1f);
                    //if (scrollbar.value == pos[jumpIdx])
                    if (Math.Abs(scrollbar.value - pos[jumpIdx]) < 0.0001)
                    {
                        AudioMGR.One.PlayEffect(scrollCLIP);
                        currentIdx = jumpIdx;
                        OnChangeIndex?.Invoke(currentIdx);
                    }
                }
                else
                {
                    //스크롤 이동 처리
                    if (isInteractable && Input.GetMouseButton(0))
                    {
                        if (!isDown)
                        {
                            isDown = true;
                            scrollPosPrev = scrollbar.value;
                        }
                        scrollSpeed = scrollbar.value - scrollPos;
                        scrollPos = scrollbar.value;
                        if (!isDrag && Math.Abs(scrollbar.value - scrollPosPrev) > 0.001)
                        {
                            scrollPosPrev = scrollbar.value;
                            OnBeginDrag?.Invoke();
                            isDrag = true;
                        }
                    }

                    int targetIdx = 0;
                    if (scrollPos <= pos[0] - (distance / 2))
                    {
                        targetIdx = 0;
                    }
                    else if (scrollPos >= pos[pos.Length - 1] + (distance / 2))
                    {
                        targetIdx = pos.Length - 1;
                    }
                    else
                    {
                        for (targetIdx = 0; targetIdx < pos.Length; targetIdx++)
                        {
                            if (pos[targetIdx] - (distance / 2) < scrollPos && scrollPos < pos[targetIdx] + (distance / 2))
                                break;
                        }
                    }
                    //LOG.Warning($"targetIdx: {targetIdx}", this);
                    if (scrollSpeed > 0 && targetIdx < pos.Length - 1 && scrollPos > pos[targetIdx])
                        targetIdx++;
                    else if (scrollSpeed < 0 && targetIdx > 0 && scrollPos < pos[targetIdx])
                        targetIdx--;

                    if (!Input.GetMouseButton(0))
                    {
                        isDown = false;

                        scrollbar.value = Mathf.Lerp(scrollbar.value, pos[targetIdx], 0.1f);

                        if (isDrag)
                        {
                            OnEndDrag?.Invoke();
                            isDrag = false;
                        }

                        if (currentIdx != targetIdx)
                        {
                            LOG.Info($"currentIdx: {currentIdx}, targetIdx: {targetIdx}", this);
                            if (currentIdx != -1)
                                AudioMGR.One.PlayEffect(scrollCLIP);
                            currentIdx = targetIdx;
                            jumpIdx = targetIdx;
                            OnChangeIndex?.Invoke(currentIdx);
                        }
                    }
                }

                for (int i = 0; i < pos.Length; i++)
                {
                    var offset = (scrollbar.value - pos[i]) / distance;
                    var scale = 1f - Mathf.Abs(offset) * scaleValue;
                    var child = transform.GetChild(i);
                    child.localScale = new Vector2(scale, scale);
                    child.localRotation = Quaternion.Euler(0, 0, offset * 8f);
                }
        }
            catch (Exception ex)
            {
            LOG.Info(ex.Message, this);
        }
    }
    }
}
