using DoDoEng.Common;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class OnOffToggle : Graphic, IPointerDownHandler
{
    // Properties
    public bool IsActivated => isActivated;

    // Methods
    public void ActivateImmediatly(bool active)
    {
        LOG.Function(this, $"{active}");

        isActivated = active;

        animator.SetTrigger(isActivated ? "onIdle" : "offIdle");

        OnActivated?.Invoke(isActivated);
    }

    // Events
    public Action<bool> OnActivated;



    // Fields : caching
    private Button btn_ = null;
    private Button btn => btn_ ??= GetComponent<Button>();
    private Animator animator_ = null;
    private Animator animator => animator_ ??= GetComponent<Animator>();

    // Fields
    private bool isActivated = false;

    // Overrides
    public override bool Raycast(Vector2 sp, Camera eventCamera)
    {
        // https://younitystudy.tistory.com/m/75
        return true;
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
    }



    // Unity Messages
    protected override void Awake()
    {
        base.Awake();

        raycastTarget = false;
    }
    protected override void Start()
    {
        base.Start();
    }

    // Interface : IPointerDownHandler
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        LOG.Info($"OnPointerDown() | {eventData.position}", this);

        isActivated = !isActivated;

        animator.SetTrigger(isActivated ? "on" : "off");

        OnActivated?.Invoke(isActivated);
    }
}
