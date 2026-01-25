using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Les Équipes")]
    public List<PionStats> teamA; // Glisse les pions du Joueur 1
    public List<PionStats> teamB; // Glisse les pions du Joueur 2
    
    [Header("Références")]
    public LanceurDeDe scriptLanceur;
    public DeLogique scriptLogique;

    // --- ÉTAT DU JEU ---
    private string tourActuel = "TeamA"; // Valeur par défaut
    private int coupsRestants = 2;       // La règle des 2 coups
    private bool enAttenteDeSelection = true; // Est-ce qu'on doit choisir un pion ?
    private PionStats pionSelectionne;

    void Start()
    {
        // --- MISE A JOUR : GESTION DE L'INITIATIVE ---
        // On regarde si une équipe a gagné l'initiative dans la scène précédente via GameData
        if (GameData.equipeQuiCommence != "")
        {
            tourActuel = GameData.equipeQuiCommence;
            Debug.Log("Le tour commence avec : " + tourActuel + " (Selon le duel de dé)");
        }

        NouvellePhaseDeSelection();
    }

    void Update()
    {
        // GESTION DE LA SÉLECTION (Au début du tour)
        if (enAttenteDeSelection)
        {
            if (Input.GetMouseButtonDown(0))
            {
                TenterDeSelectionnerPion();
            }
        }
    }

    void TenterDeSelectionnerPion()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            PionStats pionTouche = hit.collider.GetComponent<PionStats>();
            
            // Si on clique sur un pion, qu'il est vivant, et qu'il est de mon équipe
            if (pionTouche != null && !pionTouche.estMort && pionTouche.equipe == tourActuel)
            {
                ValiderSelection(pionTouche);
            }
        }
    }

    void ValiderSelection(PionStats pion)
    {
        Debug.Log("Pion choisi : " + pion.name);
        pionSelectionne = pion;
        enAttenteDeSelection = false;

        // On configure le dé pour ce pion
        pion.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        scriptLogique.pionQuiJoue = pion.transform;
        scriptLanceur.personnage = pion.transform;
        
        scriptLogique.ReinitialiserVariables(); // Réinitialiser les variables avant de reset le dé
        scriptLanceur.ResetDe();
    }

    // Appelé par DeLogique quand le dé s'arrête
    public void AnalyserFinDeTir(bool aToucheEnnemi)
    {
        coupsRestants--; // On a utilisé un coup

        // RÈGLE : Si on touche, le tour s'arrête immédiatement
        if (aToucheEnnemi)
        {
            Debug.Log(">>> COUP GAGNANT ! Fin du tour.");
            PasserAuTourSuivant();
        }
        // RÈGLE : Si on a raté mais qu'il reste des coups (ex: 1er tir raté sur 2)
        else if (coupsRestants > 0)
        {
            Debug.Log(">>> RATÉ ! Il reste " + coupsRestants + " coup(s). Rejoue !");
            scriptLogique.ReinitialiserVariables(); // Réinitialiser les variables avant de reset le dé
            scriptLanceur.ResetDe(); // Le dé revient pour le 2ème tir
        }
        // RÈGLE : Plus de coups
        else
        {
            Debug.Log(">>> PLUS DE COUPS ! Fin du tour.");
            PasserAuTourSuivant();
        }
    }

    void PasserAuTourSuivant()
    {
        VerifierVictoire();

        // Changement d'équipe
        tourActuel = (tourActuel == "TeamA") ? "TeamB" : "TeamA";
        Debug.Log("=== AU TOUR DE : " + tourActuel + " ===");

        NouvellePhaseDeSelection();
    }

    void NouvellePhaseDeSelection()
    {
        if (pionSelectionne != null)
        {
            pionSelectionne.gameObject.layer = LayerMask.NameToLayer("Default");
        }

        // --- MISE A JOUR : NETTOYAGE DU DÉ ---
        // On dit au lanceur qu'il n'appartient plus à personne pour l'instant
        // Cela empêche le dé de se téléporter sur l'ancien joueur
        if (scriptLanceur != null) scriptLanceur.personnage = null; 
        // -------------------------------------

        pionSelectionne = null;
        coupsRestants = 2; // On remet le compteur à 2
        enAttenteDeSelection = true;
        Debug.Log("Veuillez sélectionner un pion de la " + tourActuel);
        
        // On cache le dé sous la map en attendant la sélection
        if(scriptLanceur != null) scriptLanceur.transform.position = new Vector3(0, -100, 0); 
    }

    void VerifierVictoire()
    {
        if (TousMorts(teamA)) Debug.Log("VICTOIRE DE LA TEAM B !");
        if (TousMorts(teamB)) Debug.Log("VICTOIRE DE LA TEAM A !");
    }

    bool TousMorts(List<PionStats> equipe)
    {
        foreach (var pion in equipe)
        {
            if (!pion.estMort) return false; // Il en reste au moins un vivant
        }
        return true;
    }
}