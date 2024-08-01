using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Counter for the number times a user does a try
/// </summary>
public class TriesManager : MonoBehaviour
{
    [SerializeField] Text m_TriesCountText;

    private int m_TriesCount = 0;

    public void UserTried()
    {
        m_TriesCount++;

        UpdateTriesUICounter();
    }

    public void Reset()
    {
        m_TriesCount = 0;

        UpdateTriesUICounter();
    }

    private void UpdateTriesUICounter()
    {
        m_TriesCountText.text = m_TriesCount.ToString();
    }
}
