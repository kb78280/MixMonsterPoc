using UnityEngine;

public class CameraOrbitale : MonoBehaviour
{
    public Transform cible; // Le dé
    
    [Header("Réglages Rotation")]
    public float vitesseRotation = 5f;
    public float distance = 10f;

    [Header("Limites Verticales")]
    public float minAngleY = 10f;  // Empêche d'aller sous le sol (10°)
    public float maxAngleY = 80f;  // Empêche de passer par-dessus la tête (80°)

    // On stocke les deux angles (X = Horizontal, Y = Vertical)
    private float angleHorizontal = 0f;
    private float angleVertical = 45f; // On commence un peu en hauteur par défaut

    // Static pour garder la position entre les tirs si on désactive/réactive
    private static bool aEteInitialise = false;
    private static float memoireAngleH;
    private static float memoireAngleV;

    void Start()
    {
        // Si on a déjà des valeurs en mémoire (après un tir), on les reprend
        if (aEteInitialise)
        {
            angleHorizontal = memoireAngleH;
            angleVertical = memoireAngleV;
        }
        else
        {
            // Sinon on initialise proprement la première fois
            Vector3 angles = transform.eulerAngles;
            angleHorizontal = angles.y;
            angleVertical = angles.x; 
            aEteInitialise = true;
        }
    }

    void LateUpdate()
    {
        if (!cible) return;

        // GESTION DU SWIPE (Rotation)
        if (Input.GetMouseButton(0))
        {
            // Horizontal (Axe Y du monde)
            float sourisX = Input.GetAxis("Mouse X");
            angleHorizontal += sourisX * vitesseRotation;

            // Vertical (Axe X local)
            // On soustrait pour que "Souris vers le bas" = "Caméra monte" (comme un avion ou une orbite standard)
            // Si tu préfères l'inverse, mets un "+"
            float sourisY = Input.GetAxis("Mouse Y");
            angleVertical -= sourisY * vitesseRotation;

            // IMPORTANT : On limite l'angle vertical (Clamp)
            angleVertical = Mathf.Clamp(angleVertical, minAngleY, maxAngleY);

            // On sauvegarde pour le prochain réveil du script
            memoireAngleH = angleHorizontal;
            memoireAngleV = angleVertical;
        }

        // CALCUL DE LA POSITION (Logique Sphérique)
        
        // 1. On crée une rotation qui combine Horizontal ET Vertical
        Quaternion rotation = Quaternion.Euler(angleVertical, angleHorizontal, 0);

        // 2. On calcule la position en reculant de "distance" par rapport à cette rotation
        Vector3 positionNégative = new Vector3(0, 0, -distance);
        
        // 3. On applique au monde
        Vector3 positionFinale = rotation * positionNégative + cible.position;

        transform.position = positionFinale;
        transform.LookAt(cible);
    }
}