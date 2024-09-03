using UnityEngine;
using MoreMountains;
using MoreMountains.Feedbacks;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace UI
{
    public class PointerEnter : MonoBehaviour, IPointerEnterHandler
    {
        public UnityEvent OnHoverOver;

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHoverOver.Invoke();
        }
    }
}