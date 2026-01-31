using UnityEngine;
using UnityEngine.EventSystems; // Nécessaire pour détecter le clic

public class ButtonEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // On va réduire le bouton à 90% de sa taille (0.9)
    
    [SerializeField] private float reduction = 0.9f; 
    private Vector3 tailleOriginale;

    void Start()
    {
        // On mémorise la taille normale au début
        tailleOriginale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Quand le doigt appuie : on réduit
        transform.localScale = tailleOriginale * reduction;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Quand le doigt relâche : on remet normal
        transform.localScale = tailleOriginale;
    }
}