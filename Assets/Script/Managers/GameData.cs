using UnityEngine;
using System.Collections.Generic;

public static class GameData
{
    // On stocke les choix ici (par exemple des index de prefabs ou des noms)
    public static List<int> choixTeamA = new List<int>();
    public static List<int> choixTeamB = new List<int>();

    // Qui a gagné le dé d'initiative ? ("TeamA" ou "TeamB")
    public static string equipeQuiCommence = "";

    // Méthode pour nettoyer avant une nouvelle partie
    public static void ResetDonnees()
    {
        choixTeamA.Clear();
        choixTeamB.Clear();
        equipeQuiCommence = "";
    }
}