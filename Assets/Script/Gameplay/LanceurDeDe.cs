using UnityEngine;
using Cinemachine; // Nécessaire pour parler à la caméra

public class LanceurDeDe : MonoBehaviour
{
    [Header("Références")]
    public Transform personnage; // Glisse ton Player/Dé ici
    public CinemachineFreeLook camJeu; // Glisse ta CamJeu ici

    [Header("Réglages Position")]
    public float distanceDevant = 0f;
    public float hauteurOffset = 0f;

    [Header("Réglages Tir")]
    public float multiplicateurForce = 2f;
    public float forceMax = 25f;
    public float forceHauteurAuto = 5f;

    private Rigidbody rb;
    private LineRenderer ligneVisee;
    private Vector3 positionSourisDebut;
    
    // États du jeu
    private bool estEnVisee = false; // Vrai si on a cliqué sur le joueur
    private bool aEteTire = false;   // Vrai si le dé est en mouvement

    private Camera cam; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        ligneVisee = GetComponent<LineRenderer>();
        ligneVisee.enabled = false;
        
        cam = Camera.main; 

        // Gestion collisions pour ne pas se tirer dessus
        if (personnage != null)
        {
            Collider colPerso = personnage.GetComponent<Collider>();
            Collider colDe = GetComponent<Collider>();
            if (colPerso && colDe) Physics.IgnoreCollision(colPerso, colDe);
        }
    }

    void Update()
    {
        // 1. GESTION DE LA CAMÉRA (La partie importante !)
        GererCamera();

        // 2. SUIVI DU PERSONNAGE
        if (!aEteTire && personnage != null)
        {
            PositionnerSurLeJoueur();
        }
    }

    // --- NOUVELLE FONCTION DE GESTION CAMÉRA ---
    void GererCamera()
    {
        if (camJeu == null) return;

        // Cas 1 : On est en train de viser avec le joueur (Clic sur joueur maintenu)
        if (estEnVisee)
        {
            // On force l'input à 0 pour que la caméra ne bouge PAS pendant qu'on tire
            camJeu.m_XAxis.m_InputAxisValue = 0;
            camJeu.m_YAxis.m_InputAxisValue = 0;
        }
        // Cas 2 : On clique n'importe où ailleurs (Clic gauche maintenu)
        else if (Input.GetMouseButton(0))
        {
            // On envoie manuellement les mouvements de la souris à la caméra
            camJeu.m_XAxis.m_InputAxisValue = Input.GetAxis("Mouse X");
            camJeu.m_YAxis.m_InputAxisValue = Input.GetAxis("Mouse Y");
        }
        // Cas 3 : On ne touche à rien
        else
        {
            // On coupe tout pour éviter l'inertie bizarre
            camJeu.m_XAxis.m_InputAxisValue = 0;
            camJeu.m_YAxis.m_InputAxisValue = 0;
        }
    }

    // Appelé UNIQUEMENT quand on clique SUR LE DÉ (Collider)
    void OnMouseDown()
    {
        if (personnage == null) return;

        estEnVisee = true; // On passe en mode visée
        positionSourisDebut = Input.mousePosition;

        // Initialisation de la ligne de visée
        Vector3 pointSol = transform.position;
        pointSol.y = 0.2f;
        ligneVisee.SetPosition(0, pointSol);
        ligneVisee.SetPosition(1, pointSol);
        ligneVisee.enabled = true;
    }

    void OnMouseDrag()
    {
        if (estEnVisee)
        {
            Vector3 forceVecteur = CalculerVecteurForce();
            
            // Affichage de la flèche
            float hauteurFleche = 0.2f; 
            float rayonCapsule = 0.6f;  

            Vector3 directionTir = forceVecteur.normalized;
            Vector3 pointDepart = transform.position + (directionTir * rayonCapsule);
            pointDepart.y = hauteurFleche;

            Vector3 pointArrivee = transform.position + (forceVecteur * 0.5f);
            pointArrivee.y = hauteurFleche;

            if (Vector3.Distance(transform.position, pointArrivee) < rayonCapsule)
            {
                pointArrivee = pointDepart;
            }

            ligneVisee.SetPosition(0, pointDepart);
            ligneVisee.SetPosition(1, pointArrivee);
        }
    }

    void OnMouseUp()
    {
        if (estEnVisee)
        {
            estEnVisee = false; // On quitte le mode visée
            aEteTire = true;
            ligneVisee.enabled = false;

            // Physique et Tir
            rb.isKinematic = false;
            Vector3 forceFinale = CalculerVecteurForce();
            forceFinale.y = forceHauteurAuto;

            rb.AddForce(forceFinale, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 200f);
        }
    }

    void PositionnerSurLeJoueur()
    {
        Debug.Log("Position Y Joueur: " + personnage.position.y + " | Offset: " + hauteurOffset);
        Vector3 posCible = personnage.position + (personnage.forward * distanceDevant);
        posCible.y = personnage.position.y + hauteurOffset;
        transform.position = posCible;
    }

    public void ResetDe()
    {
        if (personnage != null)
        {
            Collider colPerso = personnage.GetComponent<Collider>();
            Collider colDe = GetComponent<Collider>();
            
            // Le 'true' signifie : "Ignorez-vous, s'il vous plaît"
            if (colPerso && colDe) Physics.IgnoreCollision(colPerso, colDe, true);
        }

        aEteTire = false; 
       if (rb != null && !rb.isKinematic) 
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        if (rb != null) rb.isKinematic = true;
    }

    Vector3 CalculerVecteurForce()
    {
        Vector3 differenceEcran = positionSourisDebut - Input.mousePosition;
        
        Vector3 camAvant = cam.transform.forward;
        camAvant.y = 0; 
        camAvant.Normalize();

        Vector3 camDroite = cam.transform.right;
        camDroite.y = 0;
        camDroite.Normalize();

        Vector3 force = (camAvant * differenceEcran.y) + (camDroite * differenceEcran.x);

        force *= (multiplicateurForce * 0.01f);

        if (force.magnitude > forceMax)
        {
            force = force.normalized * forceMax;
        }

        return force;
    }
}