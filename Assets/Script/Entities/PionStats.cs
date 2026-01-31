using UnityEngine;

public class PionStats : MonoBehaviour
{
    [Header("Infos")]
    public string equipe = "TeamA"; 
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

    // --- CORRECTION IMPORTANTE : void devient string ---
    public string RecevoirDegats()
    {
        if (estMort) return "Déjà Mort";

        pointsDeVie--;
        Debug.Log(name + " a reçu un coup. PV restants : " + pointsDeVie);

        if (pointsDeVie == 1)
        {
            // Blessé
            if(rend != null) rend.material.color = couleurBlessure;
            return "Blesse";
        }
        else if (pointsDeVie <= 0)
        {
            // Mort
            Mourir();
            return "Mort";
        }

        return "Rien";
    }

    void Mourir()
    {
        estMort = true;
        pointsDeVie = 0; // Sécurité
        
        if(rend != null) rend.material.color = couleurMort;
        
        rb.constraints = RigidbodyConstraints.None; 
        rb.AddForce(Random.onUnitSphere * 5f, ForceMode.Impulse); 
        
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        gameObject.tag = "Untagged"; 
    }
}