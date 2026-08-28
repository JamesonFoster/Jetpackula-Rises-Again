using UnityEngine;
using TMPro;
using Unity.Collections;

public class UICounts : MonoBehaviour
{
    public int BC = 0;
    public float PH = 0f;
    public float FC = 0f;
    public char TellMode = 'B';
    [SerializeField] private TMP_Text textMeshPro;

    private void Awake()
    {
        textMeshPro = GetComponent<TMP_Text>();
    }
    void Start()
    {
        if (TellMode == 'B')
        {
        BC = GlobalPlayerVars.BloodCount;
        textMeshPro.text = "Blood Count: " + BC.ToString();
        }
        else if (TellMode == 'H')
        {
        PH = GlobalPlayerVars.PlayerHealth;
        textMeshPro.text = "Health: " + PH.ToString();
        }
        else
        {
        FC = GlobalPlayerVars.JetFuel;
        textMeshPro.text = "Fuel: " + FC.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (TellMode == 'B')
        {
        BC = GlobalPlayerVars.BloodCount;
        textMeshPro.text = "Blood Count: " + BC.ToString();
        }
        else if (TellMode == 'H')
        {
        PH = GlobalPlayerVars.PlayerHealth;
        textMeshPro.text = "Health: " + PH.ToString();
        }
        else
        {
        FC = GlobalPlayerVars.JetFuel;
        textMeshPro.text = "Fuel: " + FC.ToString();
        }
    }
}
