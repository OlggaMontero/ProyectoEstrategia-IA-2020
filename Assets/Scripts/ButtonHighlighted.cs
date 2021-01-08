using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonHighlighted : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler
{
    public bool IsHighlighted;

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsHighlighted = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsHighlighted = false;
    }

    public void OnSelect(BaseEventData eventData)
    {
    }
}