using UnityEngine;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // Nécessaire pour changer de scène

public class GameManager : MonoBehaviour
{
    [Header("Les Équipes")]
    public List<PionStats> teamA; 
    public List<PionStats> teamB; 
    
    [Header("Caméras & Intro")] 
    public CinemachineFreeLook camIntro; 
    public CinemachineFreeLook camJeu;   
    public GameObject canvasChoixPion;   
    public TextMeshProUGUI texteTitre; // Titre "Au tour de..."

    [Header("Game Over UI")] // --- NOUVEAU ---
    public GameObject panelGameOver;
    public TextMeshProUGUI texteVictoire;
    public TextMeshProUGUI texteHistorique;

    [Header("Références")]
    public LanceurDeDe scriptLanceur;
    public DeLogique scriptLogique;

    // --- ÉTAT DU JEU ---
    private string tourActuel = "TeamA"; 
    private int coupsRestants = 2;       
    private bool enAttenteDeSelection = true; 
    private PionStats pionSelectionne;
    private bool introTerminee = false;
    private bool partieTerminee = false; // Pour bloquer le jeu

    [Header("Historique")]
    public List<ActionTour> historiqueMatch = new List<ActionTour>(); 
    private int numeroTourGlobal = 0;

    void Start()
    {
        // Init Caméras
        camIntro.Priority = 10;
        camJeu.Priority = 0;

        if (canvasChoixPion != null) canvasChoixPion.SetActive(true);
        if (panelGameOver != null) panelGameOver.SetActive(false); // On cache le Game Over au début
        
        if (GameData.equipeQuiCommence != "")
        {
            tourActuel = GameData.equipeQuiCommence;
        }

        // On ne lance pas NouvellePhaseDeSelection ici car c'est le joueur qui clique au tout début
    }

    void Update()
    {
        if (!introTerminee || partieTerminee) return; // Si fini, on ne joue plus

        if (enAttenteDeSelection)
        {
            if (Input.GetMouseButtonDown(0))
            {
                TenterDeSelectionnerPion();
            }
        }
    }

    // --- LOGIQUE AUTOMATIQUE 1v1 ---
    // Cette fonction vérifie s'il reste 1 survivant dans chaque équipe
    bool VerifierConditionDuel()
    {
        int vivantsA = CompterVivants(teamA);
        int vivantsB = CompterVivants(teamB);
        return (vivantsA == 1 && vivantsB == 1);
    }

    int CompterVivants(List<PionStats> equipe)
    {
        int count = 0;
        foreach (var p in equipe) if (!p.estMort) count++;
        return count;
    }

    PionStats GetDernierVivant(List<PionStats> equipe)
    {
        foreach (var p in equipe) if (!p.estMort) return p;
        return null;
    }

    // ----------------------------------------------------------------

    public void ChoisirPionDepart(int indexPion)
    {
        PionStats pionChoisi = null;

        if (tourActuel == "TeamA" && indexPion < teamA.Count) 
            pionChoisi = teamA[indexPion];
        else if (tourActuel == "TeamB" && indexPion < teamB.Count) 
            pionChoisi = teamB[indexPion];

        if (pionChoisi != null && !pionChoisi.estMort)
        {
            SelectionnerPionEtDemarrer(pionChoisi);
        }
    }

    // Fonction commune pour démarrer le tour (utilisée par le Clic UI et l'Auto-Select)
    void SelectionnerPionEtDemarrer(PionStats pion)
    {
        // UI et Etats
        if(canvasChoixPion != null) canvasChoixPion.SetActive(false);
        introTerminee = true;
        
        // Caméra : Transition directe
        camJeu.Follow = pion.transform;
        camJeu.LookAt = pion.transform;
        
        camIntro.Priority = 0; 
        camJeu.Priority = 10; 
        
        // Logique Jeu
        ValiderSelection(pion);
    }

    void TenterDeSelectionnerPion()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            PionStats pionTouche = hit.collider.GetComponent<PionStats>();
            
            if (pionTouche != null && !pionTouche.estMort && pionTouche.equipe == tourActuel)
            {
                camJeu.Follow = pionTouche.transform;
                camJeu.LookAt = pionTouche.transform;
                ValiderSelection(pionTouche);
            }
        }
    }

    void ValiderSelection(PionStats pion)
    {
        pionSelectionne = pion;
        enAttenteDeSelection = false;

        pion.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        scriptLogique.pionQuiJoue = pion.transform;
        scriptLanceur.personnage = pion.transform;
        
        scriptLogique.ReinitialiserVariables(); 
        scriptLanceur.ResetDe();
    }

    public void AnalyserFinDeTir(List<PionStats> victimes, List<string> effets)
    {
        coupsRestants--; 
        bool aToucheAuMoinsUn = victimes.Count > 0;

        // HISTORIQUE
        List<string> nomsVictimes = new List<string>();
        foreach(var v in victimes) nomsVictimes.Add(v.name);
        
        ActionTour nouveauLog = new ActionTour(
            numeroTourGlobal,
            scriptLanceur.personnage.name,
            aToucheAuMoinsUn ? "Attaque" : "Tir Raté",
            nomsVictimes,
            effets
        );
        historiqueMatch.Add(nouveauLog);
        numeroTourGlobal++;

        // LOGIQUE SUITE
        if (aToucheAuMoinsUn)
        {
            PasserAuTourSuivant();
        }
        else if (coupsRestants > 0)
        {
            scriptLogique.ReinitialiserVariables();
            scriptLanceur.ResetDe(); 
        }
        else
        {
            PasserAuTourSuivant();
        }
    }

    void PasserAuTourSuivant()
    {
        // Vérification Victoire AVANT de changer de tour
        if (VerifierVictoire()) return; // Si quelqu'un a gagné, on arrête tout

        tourActuel = (tourActuel == "TeamA") ? "TeamB" : "TeamA";
        NouvellePhaseDeSelection();
    }

    void NouvellePhaseDeSelection()
    {
        // 1. Nettoyage (Dé & Ancien joueur)
        if (pionSelectionne != null) pionSelectionne.gameObject.layer = LayerMask.NameToLayer("Default");

        if (scriptLanceur != null)
        {
            if (scriptLanceur.personnage != null)
            {
                Collider colAncien = scriptLanceur.personnage.GetComponent<Collider>();
                Collider colDe = scriptLanceur.GetComponent<Collider>();
                if (colAncien && colDe) Physics.IgnoreCollision(colAncien, colDe, false);
            }
            scriptLanceur.personnage = null; 
            scriptLanceur.ResetDe(); 
            scriptLanceur.transform.position = new Vector3(0, -100, 0); 
        }

        pionSelectionne = null;
        coupsRestants = 2; 
        enAttenteDeSelection = true;

        // 2. CHECK 1v1 : Est-ce qu'on doit zapper l'intro ?
        if (VerifierConditionDuel())
        {
            // OUI : On trouve le seul joueur dispo et on le lance direct
            List<PionStats> equipeActive = (tourActuel == "TeamA") ? teamA : teamB;
            PionStats survivant = GetDernierVivant(equipeActive);
            
            if (survivant != null)
            {
                // Mise à jour du texte titre au cas où
                if (texteTitre != null) texteTitre.text = "DUEL FINAL : " + tourActuel;
                Debug.Log("Mode 1v1 détecté : Passage direct à " + survivant.name);
                
                // On appelle la validation directe (sans passer par la caméra ciel)
                SelectionnerPionEtDemarrer(survivant);
                return; // On sort de la fonction, le tour est lancé
            }
        }

        // 3. SINON : Comportement Classique (Caméra Ciel + UI)
        camJeu.Follow = null; 
        camJeu.LookAt = null;
        camIntro.Priority = 10; 
        camJeu.Priority = 0;

        if (canvasChoixPion != null) 
        {
            canvasChoixPion.SetActive(true);
            if (texteTitre != null) texteTitre.text = "Au tour de : " + tourActuel;
        }
    }

    bool VerifierVictoire()
    {
        // --- MOUCHARDS ---
        int vivantsA = 0;
        foreach(var p in teamA) if(!p.estMort) vivantsA++;
        
        int vivantsB = 0;
        foreach(var p in teamB) if(!p.estMort) vivantsB++;

        Debug.Log("Check Victoire -> Vivants A: " + vivantsA + " | Vivants B: " + vivantsB);
        // -----------------

        if (TousMorts(teamA))
        {
            Debug.Log("GameManager : TEAM B GAGNE ! J'appelle l'écran de fin.");
            AfficherFinDePartie("TEAM B");
            return true;
        }
        if (TousMorts(teamB))
        {
            Debug.Log("GameManager : TEAM A GAGNE ! J'appelle l'écran de fin.");
            AfficherFinDePartie("TEAM A");
            return true;
        }
        return false;
    }

    bool TousMorts(List<PionStats> equipe)
    {
        foreach (var pion in equipe) if (!pion.estMort) return false;
        return true;
    }

    // --- GESTION FIN DE PARTIE ---
    void AfficherFinDePartie(string equipeGagnante)
    {
        partieTerminee = true;
        Debug.Log("VICTOIRE : " + equipeGagnante);

        // UI
        if (canvasChoixPion != null) canvasChoixPion.SetActive(false); // Cacher menu sélection
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
            if (texteVictoire != null) texteVictoire.text = "VICTOIRE DE LA " + equipeGagnante + " !";
            
            // Génération de l'historique texte
            if (texteHistorique != null)
            {
                string rapport = "";
                foreach(var action in historiqueMatch)
                {
                    rapport += "Tour " + action.tourIndex + " (" + action.nomJoueurActif + ") : " + action.typeAction + "\n";
                    for(int i=0; i < action.nomsVictimes.Count; i++)
                    {
                        rapport += "   -> Touche " + action.nomsVictimes[i] + " [" + action.effetsAppliques[i] + "]\n";
                    }
                    rapport += "----------------\n";
                }
                texteHistorique.text = rapport;
            }
        }
    }

    // --- FONCTIONS BOUTONS (A relier dans Unity) ---

    public void BoutonRestart()
    {
        // Relance la scène de choix "Qui commence ?"
        SceneManager.LoadScene("SceneInitiative"); 
    }

    public void BoutonChangeCharacter()
    {
        // Relance la sélection des personnages
        SceneManager.LoadScene("SceneSelection"); 
    }

    public void BoutonQuit()
    {
        // Retourne à l'accueil
        SceneManager.LoadScene("SceneAccueil"); 
    }
}

// Classe de données pour le score
[System.Serializable]
public class ActionTour
{
    public int tourIndex;
    public string nomJoueurActif;
    public string typeAction; 
    public List<string> nomsVictimes;
    public List<string> effetsAppliques; 

    public ActionTour(int tour, string joueur, string action, List<string> victimes, List<string> effets)
    {
        tourIndex = tour;
        nomJoueurActif = joueur;
        typeAction = action;
        nomsVictimes = victimes;
        effetsAppliques = effets;
    }
}