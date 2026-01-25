using UnityEngine;

public class PionStats : MonoBehaviour
{
    [Header("Infos")]
    public string equipe = "TeamA"; // ou "TeamB"
    public int pointsDeVie = 2;
    public bool estMort = false;

    [Header("Couleurs")]
    public Color couleurBlessure = new Color(1f, 0.5f, 0f); // Orange
    public Color couleurMort = Color.black;

    private Rigidbody rb;
    private Renderer rend;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
    }

    public void RecevoirDegats()
    {
        if (estMort) return;

        pointsDeVie--;

        if (pointsDeVie == 1)
        {
            // Blessé
            Debug.Log(name + " est blessé !");
            rend.material.color = couleurBlessure;
        }
        else if (pointsDeVie <= 0)
        {
            Mourir();
        }
    }

    void Mourir()
    {
        estMort = true;
        Debug.Log(name + " est MORT !");
        
        // Changement visuel
        rend.material.color = couleurMort;
        
        // Physique de la mort (il tombe)
        rb.constraints = RigidbodyConstraints.None; // On débloque tout pour qu'il roule
        rb.AddForce(Random.onUnitSphere * 5f, ForceMode.Impulse); // Petit choc
        
        // On change le layer pour qu'on ne puisse plus le viser ou le toucher
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        gameObject.tag = "Untagged"; // Il ne compte plus comme un ennemi
    }
}