using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuAccueil : MonoBehaviour
{
    public void ClickCommencer()
    {
        GameData.ResetDonnees(); // On nettoie les anciennes données
        SceneManager.LoadScene("SceneSelection"); // Nom exact de ta scène 2
    }
}