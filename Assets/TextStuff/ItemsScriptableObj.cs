using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class ItemsScriptableObj : ScriptableObject
{
    [Header("Image")]
    public Texture itemImage;

    [Header("Item Stats")]
    public int sellValue = 10;

    public GameObject objectPrefab;
    public CharacterTypes charType;
    public DioScriptableObj dio;
    public bool keyItem;
    public string stringLock;
    
    [Header("Non-Key Item Stats")]
    public int bloodValue = 25;

    public void Use()
    {
        if (!keyItem)
        {
            GlobalPlayerVars.BloodCount += bloodValue;
        }
    }

    public void Inspect(TextBoxes txtb, bool downorNot)
    {
        txtb.StartDio(charType, dio, downorNot);
    }

    public void Discard(Transform player)
    {
        Vector3 spawnPosition = player.position + player.forward * 1f;
        GameObject obj = Instantiate(objectPrefab, spawnPosition, Quaternion.identity);

        Rigidbody rb = obj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(player.forward * 2f, ForceMode.Impulse);
        }
    }
}
