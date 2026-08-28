using UnityEngine;

public class UICounts : MonoBehaviour
{
    public int BC = 0;
    public float PH = 0f;
    public float FC = 0f;
    private TextMeshProUGUI textMeshPro;
    void Start()
    {
        BC = GlobalPlayerVars.BloodCount;
        PH = GlobalPlayerVars.PlayerHealth;
        FC = GlobalPlayerVars.JetFuel;
    }

    // Update is called once per frame
    void Update()
    {
        BC = GlobalPlayerVars.BloodCount;
        PH = GlobalPlayerVars.PlayerHealth;
        FC = GlobalPlayerVars.JetFuel;
    }
}
