using UnityEngine;
using UnityEngine.EventSystems;

public class HoverScaler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float scaleFactor = 1.1f;  // 10% Vergrößerung (1.1 = +10%)
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;  // Ursprungsgröße merken
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * scaleFactor;  // 10% größer
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;  // Zurück zur Originalgröße
    }
}
