using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Nécessaire pour les Listes

public class DeLogique : MonoBehaviour
{
    [Header("Références")]
    public Transform pionQuiJoue; 
    public GameManager manager;

    [Header("Réglages Mouvement")]
    public float vitesseDeplacement = 5f;
    
    private Rigidbody rb;
    private bool estLance = false;
    private bool enTraitement = false; // Flag pour empêcher les appels multiples

    // --- LISTES POUR LE SCORE ET L'HISTORIQUE ---
    private List<PionStats> victimesCeTour = new List<PionStats>(); 
    private List<string> effetsCeTour = new List<string>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Détection du mouvement du dé
        if (rb.velocity.magnitude > 0.5f) estLance = true;

        // On attend un peu avant de vérifier que le dé est vraiment arrêté
        if (estLance && !enTraitement && rb.velocity.magnitude < 0.1f && rb.angularVelocity.magnitude < 0.1f)
        {
            // On met les flags AVANT de lancer la coroutine pour éviter les appels multiples
            estLance = false;
            enTraitement = true;
            StartCoroutine(GererFinDuTour());
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // On vérifie si on touche un PION
        PionStats ennemi = collision.gameObject.GetComponent<PionStats>();

        // Si c'est un pion valide et que ce n'est pas moi-même
        if (ennemi != null && collision.gameObject.transform != pionQuiJoue)
        {
            PionStats moi = pionQuiJoue.GetComponent<PionStats>();
            
            // Vérification : On ne tape pas ses amis !
            if (moi.equipe != ennemi.equipe)
            {
                // ANTI-REBOND : Si cet ennemi est DÉJÀ dans la liste de ce tour, on l'ignore
                if (victimesCeTour.Contains(ennemi)) return;

                // --- APPLICATION DES DÉGÂTS ---
                // La fonction renvoie "Mort", "Blesse" ou "Rien"
                string resultat = ennemi.RecevoirDegats();

                // On stocke les infos pour le rapport final
                victimesCeTour.Add(ennemi);
                effetsCeTour.Add(resultat);
                
                Debug.Log("Touché : " + ennemi.name + " -> " + resultat);
            }
        }
    }

    // Méthode publique appelée par le GameManager avant un nouveau tir
    public void ReinitialiserVariables()
    {
        estLance = false;
        enTraitement = false;
        
        // IMPORTANT : On vide l'historique du tir précédent
        victimesCeTour.Clear();
        effetsCeTour.Clear();
    }

    IEnumerator GererFinDuTour()
    {
        // On attend un peu pour être sûr que la physique est stabilisée
        yield return new WaitForSeconds(0.3f);

        // LOGIQUE DE DÉPLACEMENT :
        // Si la liste des victimes est vide (Count == 0), alors on a raté -> Le pion avance vers le dé.
        // Si on a touché quelqu'un, le pion reste à sa place d'origine.
        if (victimesCeTour.Count == 0)
        {
            Rigidbody rbPion = pionQuiJoue.GetComponent<Rigidbody>();
            Vector3 destination = new Vector3(transform.position.x, pionQuiJoue.position.y, transform.position.z);
            
            float distancePrecedente = 9999f;
            float timerImmobile = 0f;

            // Boucle de déplacement du pion
            while (Vector3.Distance(pionQuiJoue.position, destination) > 0.1f)
            {
                float distanceActuelle = Vector3.Distance(pionQuiJoue.position, destination);
                
                // Sécurité : Si le pion est bloqué contre un mur
                if (Mathf.Abs(distanceActuelle - distancePrecedente) < 0.001f)
                {
                    timerImmobile += Time.fixedDeltaTime;
                    if (timerImmobile > 0.2f) break; // On force l'arrêt s'il est coincé
                }
                else
                {
                    timerImmobile = 0f;
                    distancePrecedente = distanceActuelle;
                }

                // Mouvement fluide
                Vector3 prochainePosition = Vector3.MoveTowards(pionQuiJoue.position, destination, vitesseDeplacement * Time.fixedDeltaTime);
                rbPion.MovePosition(prochainePosition);
                pionQuiJoue.LookAt(destination);
                
                yield return new WaitForFixedUpdate(); 
            }
        }

        // Petite pause dramatique avant de redonner la main au Manager
        yield return new WaitForSeconds(1f);

        // --- RAPPORT AU CHEF ---
        if (manager != null)
        {
            // On envoie les listes complètes (Qui et Quoi) au GameManager
            manager.AnalyserFinDeTir(victimesCeTour, effetsCeTour);
        }
        
        // On reset le flag de traitement pour le prochain tour
        enTraitement = false;
    }
}