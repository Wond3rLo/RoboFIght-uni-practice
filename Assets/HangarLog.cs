using UnityEngine;
using TMPro;

public class HangarLog : MonoBehaviour
{
    private TextMeshProUGUI LogTextMesh;

    private void OnEnable()
    {
        LogTextMesh = GetComponent<TextMeshProUGUI>();

        if (BattleHistory.LastAttempt == null)
            return;
        
        Attempt data = BattleHistory.LastAttempt;
        string color = data.IsSuccess ? "#00FF00" : "#FF4444";
        string status = data.IsSuccess ? "VICTORY" : "LOSE:";

        LogTextMesh.text = $"<color={color}><b>{status}</b></color>\n";
        LogTextMesh.text += "<b>ALGORYTHM:</b>\n";
        if (data.Commands != null)
        {
            foreach (string cmd in data.Commands)
                LogTextMesh.text += $"{cmd}\n";
        }
        LogTextMesh.text += "\n<b>CHRONOLOGY:</b>\n";

        if (data.EventLog != null)
        {
            foreach (string evnt in data.EventLog)
                LogTextMesh.text += $"> {evnt}\n";
        }
    }
}
