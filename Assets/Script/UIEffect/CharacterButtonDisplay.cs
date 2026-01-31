using UnityEngine;
using UnityEngine.UI;

public class CharacterButtonDisplay : MonoBehaviour
{
    [Header("Glisse l'objet Image_Selection (cadre vert) ici")]
    public GameObject selectionBorder; 

    // C'est la fonction qui manquait et qui causait ton erreur !
    public void SetSelected(bool isSelected)
    {
        if (selectionBorder != null)
        {
            selectionBorder.SetActive(isSelected);
        }
    }
}