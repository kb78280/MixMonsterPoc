using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // Si tu utilises TextMeshPro

public class SelecteurManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI titrePhase; // "Joueur 1 : Choisissez 2 persos"
    public Button boutonValider;

    private bool tourJoueur1 = true; // true = J1 choisit, false = J2 choisit
    private int compteurSelection = 0; // Il en faut 2

    void Start()
    {
        boutonValider.interactable = false; // Désactivé tant qu'on n'a pas choisi 2 persos
        MettreAJourTexte();
    }

    // Cette fonction sera appelée par les boutons de la grille
    // Tu devras créer un petit script sur tes boutons : public void Choisir(int id) { manager.SelectionnerPerso(id); }
    public void SelectionnerPerso(int idPerso)
    {
        if (compteurSelection >= 2) return; // On ne peut pas en prendre plus de 2

        if (tourJoueur1)
        {
            GameData.choixTeamA.Add(idPerso);
            Debug.Log("J1 a choisi perso " + idPerso);
        }
        else
        {
            GameData.choixTeamB.Add(idPerso);
            Debug.Log("J2 a choisi perso " + idPerso);
        }

        compteurSelection++;

        // Si on a choisi 2 persos, on allume le bouton valider
        if (compteurSelection == 2)
        {
            boutonValider.interactable = true;
        }
    }

    public void ValiderChoix()
    {
        if (tourJoueur1)
        {
            // Fin du tour J1, on passe au J2
            tourJoueur1 = false;
            compteurSelection = 0;
            boutonValider.interactable = false;
            MettreAJourTexte();
        }
        else
        {
            // Fin du tour J2, on lance la suite
            SceneManager.LoadScene("SceneInitiative");
        }
    }

    void MettreAJourTexte()
    {
        if (tourJoueur1) titrePhase.text = "JOUEUR 1 : Choisissez 2 champions";
        else titrePhase.text = "ADVERSAIRE : Choisissez 2 champions";
    }
}