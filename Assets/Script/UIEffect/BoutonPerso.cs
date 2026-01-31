using UnityEngine;
using UnityEngine.UI;

public class BoutonPerso : MonoBehaviour
{
    public int idPersonnage; 
    public SelecteurManager manager; 
    public CharacterButtonDisplay displayScript;
    
    private float dernierClic = -999f;
    private const float delaiAntiRebond = 0.3f;

    void Start()
    {
        // On attache automatiquement l'événement au bouton
        Button bouton = GetComponent<Button>();
        if (bouton != null)
        {
            bouton.onClick.RemoveAllListeners(); // On nettoie d'abord
            bouton.onClick.AddListener(OnClickBouton); // On ajoute notre fonction
        }
    }

    public void OnClickBouton()
    {
        // Anti-rebond
        if (Time.time - dernierClic < delaiAntiRebond)
        {
            Debug.Log($"⚠️ Clic ignoré (trop rapide) pour perso {idPersonnage}");
            return;
        }
        
        dernierClic = Time.time;
        
        Debug.Log($"🟢 OnClickBouton() ACCEPTÉ pour perso {idPersonnage} - GameObject: {gameObject.name}");
        manager.ClickSurPerso(idPersonnage);
    }
}