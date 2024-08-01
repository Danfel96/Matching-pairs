using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


[System.Serializable]
public class Config
{
    public Vector2 GridCellSize = new Vector2(200, 200);
    public Vector2 Spacing = new Vector2(10, 10);
}

/// <summary>
/// Main brains of the pair game, dealing with spawning, shuffling, receiving card selections, matching and gameover 
/// you can easily add additional code to CardsMatched(), GameStarted(), GameOver() which are called at key points of the game
/// Vector(x,y) x= rows y=columns
/// </summary>
public class GameManager : Singleton<GameManager>
{
    [SerializeField] GridLayoutGroup m_GridLayoutGroup;
    [SerializeField] GameObject m_Prefab;
    [SerializeField] Transform m_Parent;

    [SerializeField] TimerManager m_Timer;
    [SerializeField] TriesManager m_TriesManager;

    [SerializeField] List<Sprite> m_Items;

    private List<Card> m_Cards;
    private Dictionary<int, Card> m_SelectedCards;
    private Dictionary<int, Card> m_MatchedCards;

    [SerializeField] GridConfigDictionary m_GridLayoutOverridesDictionary;

    // If no override exists, gridlayout will default to these layouts
    [Header("Default grid layout - can be overriden")]
    [SerializeField] Vector2 m_DefaultCellSize = new Vector2(100, 100);
    [SerializeField] Vector2 m_DefaultSpacing = new Vector2(10, 10);

    [Header("Startup")]
    [SerializeField] Vector2 m_StartupDefaultGridSize = new Vector2(3, 2);

    private void Start()
    {
        Setup((int)m_StartupDefaultGridSize.x, (int)m_StartupDefaultGridSize.y);
    }

    public void Setup(int _rowCount, int _columnCount)
    {
        Vector2 cellSize = m_DefaultCellSize;
        Vector2 spacing = m_DefaultSpacing;

        // check if users is attempting to override spacing and cellsize through inspector
        if (m_GridLayoutOverridesDictionary.ContainsKey(new Vector2(_rowCount, _columnCount)))
        {
            cellSize = m_GridLayoutOverridesDictionary[new Vector2(_rowCount, _columnCount)].GridCellSize;
            spacing = m_GridLayoutOverridesDictionary[new Vector2(_rowCount, _columnCount)].Spacing;
        }

        m_GridLayoutGroup.cellSize = cellSize;
        m_GridLayoutGroup.spacing = spacing;

        m_GridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        m_GridLayoutGroup.constraintCount = _columnCount;

        int total = _rowCount * _columnCount;

        int pairCount = total / 2;
        
        if(total % 2 != 0)
        {
            Debug.LogError("There is an odd card, totalCards=" + total + " - reconsider changing rowCount or column count so total == even number");
            return;
        }

        if(m_Items.Count < pairCount)
        {
            Debug.LogError("Sprite limitation: GameManager Items --> only has: " + m_Items.Count + "different items/sprites in list, requires items count= " + pairCount + "  sprites");
            return;
        }



        // list of ids that will be used
        List<int> ids = new List<int>();

        for (int i = 0; i < pairCount; i++)
        {
            while(true)
            {
                int id = Random.Range(0, m_Items.Count);

                if(!ids.Contains(id))
                {
                    ids.Add(id);    // draw two pairs and store in ids
                    ids.Add(id);
                    break;
                }
            }
        }

        // Shuffle list
        ids = Shuffle(ids);


        // spawn objects
        m_Cards = new List<Card>();

        m_SelectedCards = new Dictionary<int, Card>();
        m_MatchedCards = new Dictionary<int, Card>();

        for (int i = 0; i < total; i++)
        {
            GameObject obj = Instantiate(m_Prefab, m_Parent, false);
   
            Card card = obj.GetComponent<Card>();
            card.Setup(i, ids[i], m_Items[ids[i]]);

            m_Cards.Add(card);
        }

        GameStarted();
    }

    public void SelectedCard(int childid)
    {
        Card currentCard = m_Cards[childid];


        if (m_SelectedCards.Count == 2)
        {

            foreach(KeyValuePair<int, Card> card in m_SelectedCards)
            {
                if(!m_MatchedCards.ContainsKey(card.Key))
                {
                    card.Value.HideCard();
                }
            }

            m_SelectedCards = new Dictionary<int, Card>();
        }

        if(m_MatchedCards.Count > 0)
        {
            foreach(KeyValuePair<int, Card> card in m_MatchedCards)
            {
                if(childid == card.Key)
                {
                    Debug.Log("Card already exists in 'matched' stack - user has selected card that is already showing and 'matched'");
                    return;
                }
            }
        }

        currentCard.ShowCard();
    

        // first card, show no need to do any matching algorithm
        if (m_SelectedCards.Count == 0)
        {
            m_SelectedCards.Add(childid, currentCard);
            return;
        }

     
        if(m_SelectedCards.ContainsKey(childid))
        {
            Debug.Log("Card already exists in selected stack - user has selected card that is already showing");
            return;
        }


        // add to selected cards
        if(m_SelectedCards.Count < 2)
        {
            m_SelectedCards.Add(childid, currentCard);
        }


        // check for a match
        if(m_SelectedCards.Count == 2)
        {

            bool matched = false;
            int? pairId = null;

            foreach (KeyValuePair<int, Card> card in m_SelectedCards)
            {
                if (pairId == null)
                    pairId = card.Value.PairId;
                else if (pairId == card.Value.PairId)
                    matched = true;
                else
                {
                    Debug.Log("More than on item in list did not match");
                    matched = false;
                }
            }

            if(matched)
            {
                foreach(KeyValuePair<int, Card> card in m_SelectedCards)
                {
                    m_MatchedCards.Add(card.Key, card.Value);
                }

                CardsMatched();

                CheckGameOver();
            }
            else
            {
                CardsDidNotMatch();
            }

            m_TriesManager.UserTried();

            return;
        }
    }

    /// <summary>
    /// Restarts level to defaultgrid
    /// </summary>
    public void Restart()
    {
        for (int i = 0; i < m_Cards.Count; i++)
        {
            Destroy(m_Cards[i].gameObject);
        }

        m_Cards = new List<Card>();
        m_SelectedCards = new Dictionary<int, Card>();
        m_MatchedCards = new Dictionary<int, Card>();

        Setup((int)m_StartupDefaultGridSize.x, (int)m_StartupDefaultGridSize.y);

    }

    
    private void CheckGameOver()
    {
        if(m_MatchedCards.Count == m_Cards.Count)
        {
            GameOver();
        }
    }

    private void GameStarted()
    {
        Debug.Log("Game STARTED");

        m_Timer.ResetTimer();
        m_Timer.StartTimer();

        m_TriesManager.Reset();
    }

    private void GameOver()
    {
        Debug.Log("GAME OVER");

        m_Timer.StopTimer();
    }

    private void CardsMatched()
    {
        Debug.Log("Two cards matched!");
        // do something - such as add sound
    }

    private void CardsDidNotMatch()
    {
        Debug.Log("Cards did not match");
    }

    private List<int> Shuffle(List<int> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }

        return ts;
    }
}
