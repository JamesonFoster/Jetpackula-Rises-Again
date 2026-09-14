using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Dio", menuName = "Scriptable Objects/Dio")]
public class DioScriptableObj : ScriptableObject
{
    [Header("The Text")]
    public string theText;

    [Header("Next Text")]
    public DioScriptableObj nextText;
}
