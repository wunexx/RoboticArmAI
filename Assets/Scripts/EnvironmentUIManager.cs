using TMPro;
using UnityEngine;

public class EnvironmentUIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI infoText;

    public void DisplayInfo(string info) => infoText.text = info;
}
