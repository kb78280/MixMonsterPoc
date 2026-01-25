using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class DuelDeManager : MonoBehaviour
{
    public TextMeshProUGUI infoTexte;
    public TextMeshProUGUI resultatTexte;
    public Button boutonDe; // Le bouton qui sert de dé

    private int scoreJ1 = 0;
    private int scoreJ2 = 0;
    private bool tourJ1 = true;

    void Start()
    {
        infoTexte.text = "Joueur 1 : Lancez le dé !";
        resultatTexte.text = "";
    }

    public void LancerLeDe()
    {
        StartCoroutine(AnimationDe());
    }

    IEnumerator AnimationDe()
    {
        boutonDe.interactable = false;
        resultatTexte.text = "...";
        
        // Simulation "Tourbillon" (Attente visuelle)
        yield return new WaitForSeconds(1f);

        int resultat = Random.Range(1, 7); // 1 à 6
        resultatTexte.text = resultat.ToString();

        yield return new WaitForSeconds(1f); // Temps de lecture

        if (tourJ1)
        {
            scoreJ1 = resultat;
            tourJ1 = false;
            infoTexte.text = "Adversaire : Lancez le dé !";
            resultatTexte.text = "";
            boutonDe.interactable = true;
        }
        else
        {
            scoreJ2 = resultat;
            CalculerVainqueur();
        }
    }

    void CalculerVainqueur()
    {
        if (scoreJ1 > scoreJ2)
        {
            infoTexte.text = "Le Joueur 1 commence !";
            GameData.equipeQuiCommence = "TeamA";
        }
        else if (scoreJ2 > scoreJ1)
        {
            infoTexte.text = "L'Adversaire commence !";
            GameData.equipeQuiCommence = "TeamB";
        }
        else
        {
            // Égalité : On relance tout (version simple)
            infoTexte.text = "Égalité ! On recommence.";
            tourJ1 = true;
            boutonDe.interactable = true;
            return; // On sort pour ne pas changer de scène tout de suite
        }

        StartCoroutine(AllerAuJeu());
    }

    IEnumerator AllerAuJeu()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("SceneArene"); // Ta scène de combat actuelle
    }
}