using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Card script mainly deals with receiving user input and being told by gamemanager
/// when to show/hide/animate cards
/// </summary>
public class Card : MonoBehaviour
{
    [SerializeField] Button m_Button;

    [SerializeField] Image m_Image;

    [SerializeField] float m_Speed = 25f;

    private Quaternion m_Target;

    private bool m_Hidden;
    private bool m_Animate = false;

    public int ChildId { get; private set; }
    public int PairId { get; private set; }


    public void Setup(int childid,int pairid, Sprite sprite)
    {
        ChildId = childid;
        PairId = pairid;

        m_Image.sprite = sprite;

        m_Button.onClick.RemoveAllListeners();
        m_Button.onClick.AddListener(() =>
        {
            GameManager.Instance.SelectedCard(childid);
        });
    }


    private void Start()
    {
        m_Hidden = true;
    }

    private void Update()
    {
        if(m_Animate)
        {
            AnimateCard();
        }
    }

    public void ShowCard()
    {
        m_Target = Quaternion.Euler(180, 0, 0);

        m_Animate = true;
    }

    public void HideCard()
    {
        m_Target = Quaternion.Euler(0, 0, 0);

        m_Animate = true;
    }

    private void AnimateCard()
    {
        m_Button.transform.rotation = Quaternion.RotateTowards(m_Button.transform.rotation, m_Target, m_Speed);

        if (Quaternion.Angle(m_Button.transform.rotation, m_Target) <= 0.01f)
        {
            m_Button.transform.rotation = m_Target;

            m_Animate = false;
        }
    }
}
