using UnityEngine;
using System.Collections.Generic;
public class GlobalPlayerVars : MonoBehaviour
{
    public static float BloodCount = 0;
    public static float JetFuel = 100f;
    public static float PlayerHealth = 100f;
    public static char ArmState = 'R';
    public static int PlayerDamage = 27;
    public static int playerMode = 0;
    public static string inventKeyCode;
    public static KeyInsertSpot inventInteraction;


    // InventorySlots
    public static ItemsScriptableObj inv1;
    public static ItemsScriptableObj inv2;
    public static ItemsScriptableObj inv3;
    public static ItemsScriptableObj inv4;
    public static ItemsScriptableObj inv5;
    public static ItemsScriptableObj inv6;

    [SerializeField] private CharacterTypes baseChar;
    [SerializeField] private DioScriptableObj baseDio;

    public static CharacterTypes BaseChar;
    public static DioScriptableObj BaseDio;

    private void Awake()
    {
        BaseChar = baseChar;
        BaseDio = baseDio;
    }
}