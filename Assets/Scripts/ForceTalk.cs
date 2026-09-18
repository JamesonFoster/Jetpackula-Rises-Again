using UnityEngine;

public class ForceTalk : MonoBehaviour
{
    public GameObject player;
    public TextBoxes tB;
    public DioScriptableObj dio;
    public CharacterTypes charType;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            gameObject.SetActive(false);

            tB.StartDio(charType, dio);
        }
    }
}
