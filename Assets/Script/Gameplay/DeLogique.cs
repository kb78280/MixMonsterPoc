using UnityEngine;
using System.Collections;

public class DeLogique : MonoBehaviour
{
    [Header("Références")]
    public Transform pionQuiJoue; 
    public GameManager manager;

    [Header("Réglages Mouvement")]
    public float vitesseDeplacement = 5f;
    private Rigidbody rb;
    private LanceurDeDe lanceurScript;
    private bool aToucheAdversaire = false;
    private bool estLance = false;
    private bool enTraitement = false; // Flag pour empêcher les appels multiples

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lanceurScript = GetComponent<LanceurDeDe>();
    }

    void Update()
    {
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
        // Si on touche un PION (qui a le script PionStats)
        PionStats ennemi = collision.gameObject.GetComponent<PionStats>();

        if (ennemi != null && collision.gameObject.transform != pionQuiJoue)
        {
            // Vérification : On ne tape pas ses amis !
            PionStats moi = pionQuiJoue.GetComponent<PionStats>();
            if (moi.equipe != ennemi.equipe)
            {
                aToucheAdversaire = true;
                
                // --- INFLIGER DÉGÂTS ---
                ennemi.RecevoirDegats();
            }
        }
    }

    IEnumerator GererFinDuTour()
    {
        // On attend un peu pour être sûr que le dé est vraiment arrêté
        yield return new WaitForSeconds(0.3f);

        if (!aToucheAdversaire)
        {
            // Le personnage avance vers le dé
            Rigidbody rbPion = pionQuiJoue.GetComponent<Rigidbody>();
            Vector3 destination = new Vector3(transform.position.x, pionQuiJoue.position.y, transform.position.z);
            float distancePrecedente = 9999f;
            float timerImmobile = 0f;

            while (Vector3.Distance(pionQuiJoue.position, destination) > 0.1f)
            {
                float distanceActuelle = Vector3.Distance(pionQuiJoue.position, destination);
                if (Mathf.Abs(distanceActuelle - distancePrecedente) < 0.001f)
                {
                    timerImmobile += Time.fixedDeltaTime;
                    if (timerImmobile > 0.2f) break;
                }
                else
                {
                    timerImmobile = 0f;
                    distancePrecedente = distanceActuelle;
                }
                Vector3 prochainePosition = Vector3.MoveTowards(pionQuiJoue.position, destination, vitesseDeplacement * Time.fixedDeltaTime);
                rbPion.MovePosition(prochainePosition);
                pionQuiJoue.LookAt(destination);
                yield return new WaitForFixedUpdate(); 
            }
        }

        yield return new WaitForSeconds(1f);

        // --- RAPPORT AU CHEF ---
        // On dit au Manager si on a touché ou pas pour qu'il gère les tours restants
        if (manager != null)
        {
            manager.AnalyserFinDeTir(aToucheAdversaire);
        }
        
        // Réinitialisation des variables
        aToucheAdversaire = false;
        enTraitement = false;
    }

    // Méthode publique pour réinitialiser les variables quand le dé est réinitialisé
    public void ReinitialiserVariables()
    {
        aToucheAdversaire = false;
        estLance = false;
        enTraitement = false;
    }

    
}