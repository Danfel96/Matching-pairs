using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Timer, which is used for counting the duration of a game session 
/// </summary>
public class TimerManager : MonoBehaviour
{
    [SerializeField] Text m_TimerText;

    private float m_TotalSeconds = 0;
    private bool m_Count = false;

    private void Update()
    {
        if(m_Count)
        {
            m_TotalSeconds += Time.deltaTime;
            UpdateTimer(m_TotalSeconds);
        }
    }

    public void StartTimer()
    {
        m_Count = true;
    }

    public void StopTimer()
    {
        m_Count = false;
    }

    public void ResetTimer()
    {
        m_TotalSeconds = 0;
    }

    private void UpdateTimer(float totalSeconds)
    {
        string minutes = Mathf.Floor(totalSeconds / 60).ToString();
        string seconds = Mathf.Floor(totalSeconds % 60).ToString("00");

        m_TimerText.text = string.Format("{0}:{1}", minutes, seconds);
    }
}
