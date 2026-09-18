using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Dio", menuName = "Scriptable Objects/Dio")]
public class DioScriptableObj : ScriptableObject
{
    [TextArea]
    public string theText;

    [Header("Next Text")]
    public DioScriptableObj nextText;
}
