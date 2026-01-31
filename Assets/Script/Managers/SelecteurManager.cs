using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SelecteurManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI titrePhase;
    public Button boutonValider;

    // IMPORTANT : Tu devras glisser tes boutons ici dans l'inspecteur
    [Header("Référence à tous les boutons")]
    public BoutonPerso[] tousLesBoutons; 

    private bool tourJoueur1 = true;

    void Start()
    {
        // Au démarrage, on s'assure que tout est propre
        ActualiserVisuelGlobal();
    }

    public void ClickSurPerso(int idPerso)
{
    Debug.Log($"========== DÉBUT ClickSurPerso({idPerso}) ==========");
    Debug.Log($"Frame: {Time.frameCount} | Time: {Time.time}");
    Debug.Log($"Stack Trace: {System.Environment.StackTrace}");
    
    List<int> listeActuelle = tourJoueur1 ? GameData.choixTeamA : GameData.choixTeamB;
    
    Debug.Log($"AVANT - Liste : [{string.Join(", ", listeActuelle)}]");
    Debug.Log($"Contains({idPerso})? {listeActuelle.Contains(idPerso)}");
    Debug.Log($"Count: {listeActuelle.Count}");

    if (listeActuelle.Contains(idPerso))
    {
        listeActuelle.Remove(idPerso);
        Debug.Log($"→ RETRAIT du perso {idPerso}");
    }
    else
    {
        if (listeActuelle.Count < 2)
        {
            listeActuelle.Add(idPerso);
            Debug.Log($"→ AJOUT du perso {idPerso}");
        }
        else
        {
            Debug.Log($"→ RIEN (liste pleine)");
        }
    }

    Debug.Log($"APRES - Liste : [{string.Join(", ", listeActuelle)}]");
    Debug.Log("========== FIN ClickSurPerso ==========\n");
    
    ActualiserVisuelGlobal();
}

    // Cette fonction ne gère QUE les données (Ta logique exacte)
   /* public void ClickSurPerso(int idPerso)
    {
        // 1. On récupère la bonne liste (Team A ou Team B)
        List<int> listeActuelle = tourJoueur1 ? GameData.choixTeamA : GameData.choixTeamB;

           Debug.Log($"========== CLICK sur perso {idPerso} ==========");
        Debug.Log($"AVANT - Liste actuelle : [{string.Join(", ", listeActuelle)}]");
        Debug.Log($"tourJoueur1 = {tourJoueur1}");

        // 2. TA LOGIQUE :
        if (listeActuelle.Contains(idPerso))
        {
            // "si l'id est dans la liste , sa le retire de la liste"
            listeActuelle.Remove(idPerso);
            Debug.Log($"→ RETRAIT du perso {idPerso}");
        }
        else
        {
            // "si l'id n 'est pas dans la liste..."
            if (listeActuelle.Count < 2)
            {
                // "...et que ma liste n'a pas deux ID alors je l'ajoute"
                listeActuelle.Add(idPerso);
                 Debug.Log($"→ AJOUT du perso {idPerso}");
            }
             else
            {
                Debug.Log($"→ RIEN (liste pleine avec {listeActuelle.Count} persos)");
            }
            // "Si l'id n est pas dans la liste et que ma liste a deux ID , alors je ne fait rien"
            // (Ici on ne fait rien, donc le code s'arrête là, c'est parfait)
        }

        // 3. LA MAGIE : On demande à l'écran de se mettre à jour par rapport aux données
        Debug.Log($"APRES - Liste actuelle : [{string.Join(", ", listeActuelle)}]");
        Debug.Log("========================================");
        ActualiserVisuelGlobal();
    }
*/
    // Cette fonction parcourt tous les boutons et décide s'ils doivent être verts
    void ActualiserVisuelGlobal()
    {
        List<int> listeActuelle = tourJoueur1 ? GameData.choixTeamA : GameData.choixTeamB;
 Debug.Log($">>> ACTUALISATION VISUELLE - Liste actuelle : [{string.Join(", ", listeActuelle)}]");
        // A. On boucle sur CHAQUE bouton de la grille
        foreach (var bouton in tousLesBoutons)
        {
            // Est-ce que ce bouton fait partie de la liste choisie ?
            bool doitEtreAllume = listeActuelle.Contains(bouton.idPersonnage);
            Debug.Log($"    Perso {bouton.idPersonnage} → {(doitEtreAllume ? "ALLUMÉ ✓" : "ÉTEINT ✗")}");
            
            // On applique le visuel (Allumé ou Éteint)
            if (bouton.displayScript != null)
            {
                bouton.displayScript.SetSelected(doitEtreAllume);
            }
        }

        // B. Gestion du bouton Valider (Activé seulement si on a 2 persos)
        boutonValider.interactable = (listeActuelle.Count == 2);

        // C. Gestion du Texte
        titrePhase.text = tourJoueur1 ? "JOUEUR 1 : Choisissez 2 champions" : "JOUEUR 2 : Choisissez 2 champions";
    }

    public void ValiderChoix()
    {
        if (tourJoueur1)
        {
            tourJoueur1 = false;
            // On rappelle la mise à jour : comme la liste du J2 est vide, tout va s'éteindre tout seul !
            ActualiserVisuelGlobal(); 
        }
        else
        {
            SceneManager.LoadScene("SceneInitiative");
        }
    }
}