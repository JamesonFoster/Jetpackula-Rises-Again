using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "CharacterType", menuName = "Scriptable Objects/CharType")]
public class CharacterTypes : ScriptableObject
{
    [Header("Images")]
    public Texture noTalkingImage;
    public Texture talkingImage;

    [Header("Image Movement")]
    public float rotationMax = 35f;
    public float imageRotaSpeed = 3f;
    public float talkingImageSpeed = 0.24f;

    [Header("Text Settings")]
    public int textSpeed = 20;
    public float postTextWait = 5f;
}
