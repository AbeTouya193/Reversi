using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

public class Game : SingletonMonoBehaviour<Game>
{
    public static readonly int XNum = 8;
    public static readonly int ZNum = 8;

    public enum State
    {
        None,
        Initializing,
        BlackTurn,
        WhiteTurn,
        Result,
    }

    [SerializeField]
    private Stone _stonePrefab;

    [SerializeField]
    private Transform _stoneBase;

    [SerializeField]
    private Player _selfPlayer;

    [SerializeField]
    private Player _enemyPlayer;

    [SerializeField]
    private TextMeshPro _blackScoreText;

    [SerializeField]
    private TextMeshPro _whiteScoreText;

    [SerializeField]
    private TextMeshPro _resultText;


    [SerializeField]
    private GameObject _cursor;

    [SerializeField]
    private AudioClip _seAudioSource;

    [SerializeField]
    private AudioClip _cursorMoveSe;

    [SerializeField]
    private AudioClip _stoneAppearSe;

    [SerializeField]
    private AudioClip _stoneReverseSe;

    public GameObject Cursor { get { return _cursor; } }

    public Stone[][] Stones { get; private set; }

    public State CurrentState { get; private set; } = State.None;

    public int CurrentTurn
    {
        get
        {
            var turnCount = 0;
            for (var z = 0; z < ZNum; z++)
            {
                for (var x = 0; x < XNum; x++)
                {
                    if (Stones[z][x].CurrentState != Stone.State.None)
                    {
                        turnCount++;
                    }
                }
            }
            return turnCount;
        }
    }

    private void Start()
    {
        Stones = new Stone[ZNum][];
        for (var z = 0; z < ZNum; z++)
        {
            Stones[z] = new Stone[XNum];
            for (var x = 0; x < XNum; x++)
            {
                var stone = Instantiate(_stonePrefab, _stoneBase);
                var t = stone.transform;
                t.localPosition = new Vector3(x * 10, 0, z * 10);
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
                //stone.SetActive(false, Stone.Color.Black);
                Stones[z][x] = stone;
            }
        }
        _cursor.SetActive(false);
        CurrentState = State.Initializing;
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case State.Initializing:
                {
                    for (var z = 0; z < ZNum; z++)
                    {
                        for (var x = 0; x < XNum; x++)
                        {
                            Stones[z][x].SetActive(false, Stone.Color.Black);
                        }
                    }
                    Stones[3][3].SetActive(true, Stone.Color.Black);
                    Stones[3][4].SetActive(true, Stone.Color.White);
                    Stones[4][3].SetActive(true, Stone.Color.White);
                    Stones[4][4].SetActive(true, Stone.Color.Black);
                    UpdateScore();
                    _resultText.gameObject.SetActive(false);

                    CurrentState = State.BlackTurn;
                }
                break;

            case State.BlackTurn:
                {
                    if (IsAnimating())
                    {
                        break;
                    }

                    if(_selfPlayer.TryGetSelected(out var x, out var z))
                    {
                        Stones[z][x].SetActive(true, Stone.Color.Black);
                        Reverse(Stone.Color.Black, x, z);
                        UpdateScore();
                        if (_enemyPlayer.CanPut())
                        {
                            CurrentState = State.WhiteTurn;
                        }
                        else if (!_selfPlayer.CanPut())
                        {
                            CurrentState = State.Result;
                        }
                    }
                }
                break;
            case State.WhiteTurn:
                {
                    if (IsAnimating())
                    {
                        break;
                    }

                    if (_enemyPlayer.TryGetSelected(out var x, out var z))
                    {
                        Stones[z][x].SetActive(true, Stone.Color.White);
                        Reverse(Stone.Color.White, x, z);
                        UpdateScore();
                        if (_selfPlayer.CanPut())
                        {
                            CurrentState = State.BlackTurn;
                        }
                        else if (!_enemyPlayer.CanPut())
                        {
                            CurrentState = State.Result;
                        }
                    }
                }
                break;
            case State.Result:
                {
                    int blackScore;
                    int whiteScore;
                    CalcScore(out blackScore, out whiteScore);
                    if (blackScore > whiteScore)
                    {
                        SceneManager.LoadScene("BlackWin");
                    }
                    else if (blackScore < whiteScore)
                    {
                        SceneManager.LoadScene("WhiteWin");
                    }
                    else 
                    {
                        SceneManager.LoadScene("Draw");
                    }
                    var kb = Keyboard.current;
                    if (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
                    {
                        CurrentState = State.Initializing;
                    }
                }
                break;

            case State.None:
            default:
                break;
        }
    }

    private bool IsAnimating()
    {
        for(var z = 0; z < ZNum; z++)
        {
            for(var x = 0; x < XNum; x++)
            {
                switch (Stones[z][x].CurrentState)
                {
                case Stone.State.Appearing:
                case Stone.State.Reversing:
                    return true;
                }
            }
        }
        return false;
    }

    private void UpdateScore()
    {
        int blackScore;
        int whiteScore;
        CalcScore(out blackScore, out whiteScore);
        _blackScoreText.text = blackScore.ToString();
        _whiteScoreText.text = whiteScore.ToString();
    }

    private void CalcScore(out int blackScore, out int whiteScore)
    {
        blackScore = 0;
        whiteScore = 0;

        for (var z = 0; z < ZNum; z++)
        {
            for (var x = 0; x < XNum; x++)
            {
                if (Stones[z][x].CurrentState != Stone.State.None)
                {
                    switch (Stones[z][x].CurrentColor)
                    {
                        case Stone.Color.Black:
                            blackScore++;
                            break;

                        case Stone.Color.White:
                            whiteScore++;
                            break;
                    }
                }
            }
        }
    }

    private void Reverse(Stone.Color color, int putX, int putZ)
    {
        for(var dirZ = -1; dirZ <= 1; dirZ++)
        {
            for(var dirX = -1; dirX <= 1; dirX++)
            {
                var reverseCount = CalcReverseCount(color, putX, putZ, dirX, dirZ);
                for(var i = 1; i <= reverseCount; i++)
                {
                    Stones[putZ + dirZ * i][putX + dirX * i].Reverse();
                }
            }
        }
    }

    private int CalcReverseCount(Stone.Color color, int putX, int putZ, int dirX, int dirZ)
    {
        var x = putX;
        var z = putZ;
        var reverseCount = 0;
        for(var i = 0; i < 8; i++)
        {
            x += dirX;
            z += dirZ;

            if(x < 0 || x >= XNum || z < 0 || z >= ZNum)
            {
                reverseCount = 0;
                break;
            }

            var stone = Stones[z][x];
            if (stone.CurrentState == Stone.State.None)
            {
                reverseCount = 0;
                break;
            }
            else
            {
                if (stone.CurrentColor != color)
                {
                    reverseCount++;
                }
                else
                {
                    break;
                }
            }
        }
        return reverseCount;
    }
    public int CalcTotalReverseCount(Stone.Color color, int putX, int putZ)
    {
        if (Stones[putZ][putX].CurrentState != Stone.State.None)
            return 0;

        var totalReverseCount = 0;
        for (var dirZ = -1; dirZ <= 1; dirZ++)
        {
            for (var dirX = -1; dirX <= 1; dirX++)
            {
                totalReverseCount += CalcReverseCount(color, putX, putZ, dirX, dirZ);

            }
        }
        return totalReverseCount;
    }

    /*
    public void PlayCursorMoveSe()
    {
        _seAudioSource.PlayOneShot(_cursorMoveSe);
    }

    public void PlayStoneAppearSe()
    {
        _seAudioSource.PlayOneShot(_stoneAppearSe);
    }

    public void PlayStoneReverseSe()
    {
        _seAudioSource.PlayOneShot(_stoneReverseSe);
    }
    */
}
//9:10