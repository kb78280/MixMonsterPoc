using UnityEngine;

public class LanceurDeDe : MonoBehaviour
{
    [Header("Références")]

    public Transform personnage; // Glisse ta capsule "Joueur" ici

    [Header("Réglages Position")]
    public float distanceDevant = 0f; // 0 pour être dans le perso
    public float hauteurOffset = 0f; // Pour le lever 

    [Header("Réglages Tir")]
    private bool aEteTire = false; // Pour savoir si on doit bloquer sa position ou le laisser voler
    public float multiplicateurForce = 2f; 
    public float forceMax = 25f;
    public float forceHauteurAuto = 5f; // La petite "cloche" automatique (fixe)

    private Rigidbody rb;
    private LineRenderer ligneVisee;
    private Vector3 positionSourisDebut;
    private bool estEnVisee = false;

    // Référence à la caméra pour savoir où est "l'avant"
    private Camera cam;


    void Update()
    {
        // TANT QU'ON A PAS TIRÉ : On force le dé à rester devant le joueur
        if (!aEteTire && personnage != null)
        {
            PositionnerSurLeJoueur();
        }
    }

    void Start()
    {
        
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        ligneVisee = GetComponent<LineRenderer>();
        ligneVisee.enabled = false;
        cam = Camera.main;

        // GESTION COLLISIONS (Dé vs Perso)
       if (personnage != null)
        {
            Collider colPerso = personnage.GetComponent<Collider>();
            Collider colDe = GetComponent<Collider>();
            if (colPerso && colDe) Physics.IgnoreCollision(colPerso, colDe);
        }
    }

    void OnMouseDown()
    {
        // Si aucun perso n'est assigné (phase de sélection), on interdit le tir
    if (personnage == null) return;
        // On commence à viser quand on clique SUR le dé
        estEnVisee = true;
        positionSourisDebut = Input.mousePosition;

        // Préparation de la ligne (Invisible au départ)
        // On la place au sol (0.2f) pour éviter le flash visuel d'une frame
        Vector3 pointSol = transform.position;
        pointSol.y = 0.2f;

        // On met le départ et l'arrivée au même endroit = point invisible
        ligneVisee.SetPosition(0, pointSol);
        ligneVisee.SetPosition(1, pointSol);

        ligneVisee.enabled = true;
        
        // On bloque la caméra
        cam.GetComponent<CameraOrbitale>().enabled = false;
    }

    void OnMouseDrag()
    {
        if (estEnVisee)
        {
            Vector3 forceVecteur = CalculerVecteurForce();
            
            // --- NOUVELLE LOGIQUE D'AFFICHAGE ---
            
            float hauteurFleche = 0.2f; // Hauteur au sol
            float rayonCapsule = 0.6f;  // Décalage pour sortir du personnage

            // 1. Calcul du point de DÉPART (Décalé du centre)
            // On prend la direction du tir et on avance de 60cm
            Vector3 directionTir = forceVecteur.normalized;
            Vector3 pointDepart = transform.position + (directionTir * rayonCapsule);
            pointDepart.y = hauteurFleche;

            // 2. Calcul du point d'ARRIVÉE (Bout de la flèche)
            // On prend le centre et on ajoute la force totale
            Vector3 pointArrivee = transform.position + (forceVecteur * 0.5f);
            pointArrivee.y = hauteurFleche;

            // 3. SÉCURITÉ (Anti-Reverse)
            // Si on tire tout doucement, l'arrivée pourrait être derrière le départ.
            // Si la flèche est plus courte que le rayon du perso, on la cache (taille 0).
            if (Vector3.Distance(transform.position, pointArrivee) < rayonCapsule)
            {
                pointArrivee = pointDepart;
            }

            // 4. Application
            ligneVisee.SetPosition(0, pointDepart);
            ligneVisee.SetPosition(1, pointArrivee);
        }
    }

    void OnMouseUp()
    {
        if (estEnVisee)
        {
            estEnVisee = false;
            aEteTire = true;
            
            ligneVisee.enabled = false;

            // On réactive la caméra
            cam.GetComponent<CameraOrbitale>().enabled = true;
            rb.isKinematic = false;

            // TIRER !
            Vector3 forceFinale = CalculerVecteurForce();
            
            // On ajoute la petite cloche vers le haut (fixe)
            forceFinale.y = forceHauteurAuto;

            rb.AddForce(forceFinale, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 200f); // Rotation pour le style
        }
    }

   void PositionnerSurLeJoueur()
    {
        // On place le dé exactement sur le joueur (avec tes offsets si besoin)
        // Comme tu veux tirer "depuis l'intérieur", distanceDevant sera surement à 0
        Vector3 posCible = personnage.position + (personnage.forward * distanceDevant);
        posCible.y = personnage.position.y + hauteurOffset;
        
        transform.position = posCible;
       // rb.velocity = Vector3.zero;
       // rb.angularVelocity = Vector3.zero;
    }

    public void ResetDe()
    {
        aEteTire = false; // On le remet en mode "suivi"
        if (!rb.isKinematic) 
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        rb.isKinematic = true;
    }

    // Fonction mathématique pour calculer la force selon la caméra
    Vector3 CalculerVecteurForce()
    {
        // 1. Calculer la distance de glissement du doigt
        Vector3 differenceEcran = positionSourisDebut - Input.mousePosition;
        
       Vector3 camAvant = cam.transform.forward;
        camAvant.y = 0; // On aplatit pour rester au sol
        camAvant.Normalize();

        Vector3 camDroite = cam.transform.right;
        camDroite.y = 0;
        camDroite.Normalize();

        // 3. LE MIXAGE (Magie)
        // Si je tire la souris vers le bas (Y positif), je veux aller "Devant la caméra"
        // Si je tire la souris vers la gauche (X positif), je veux aller "A droite de la caméra" (Effet lance-pierre inversé)
        
        Vector3 force = (camAvant * differenceEcran.y) + (camDroite * differenceEcran.x);

        // 4. On applique le multiplicateur de puissance
        force *= (multiplicateurForce * 0.01f);

        // 5. On limite la force max
        if (force.magnitude > forceMax)
        {
            force = force.normalized * forceMax;
        }

        return force;
    }
}
