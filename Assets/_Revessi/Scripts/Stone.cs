using System;
using UnityEngine;

public class Stone : MonoBehaviour
{
    private static readonly float AppearSeconds = 0.3f;
    private static readonly float ReverseSeconds = 0.5f;

    public enum Color
    {
        Black,
        White,
    }

    public enum State
    {
        None,
        Appearing,
        Reversing,
        Fix,
    }

    [SerializeField]
    private GameObject _black;

    [SerializeField]
    private GameObject _white;

    [SerializeField]
    private GameObject _dot;

    public Color CurrentColor { get; private set; } = Color.Black;
    public State CurrentState { get; private set; } = State.None;
    private Quaternion Rotation
    {
        get
        {
            switch (CurrentColor)
            {
                case Color.Black:
                    return Quaternion.Euler(0, 0, 0);

                case Color.White:
                default:
                    return Quaternion.Euler(0, 0, 180);
            }
        }
    }

    private DateTime _stateChangedAt = DateTime.MinValue;
    private float ElapsedSecondsSinceStateChanged { get { return (float)(DateTime.UtcNow - _stateChangedAt).TotalSeconds; } }

    public void SetActive(bool value, Color color)
    {
        if (value)
        {
            this.CurrentColor = color;
            this.CurrentState = State.Appearing;
            this._black.SetActive(true);
            this._white.SetActive(true);
            this._dot.SetActive(false);
            _stateChangedAt = DateTime.UtcNow;
            //Game.Instance.PlayStoneAppearSe();
        }
        else
        {
            this.CurrentState = State.None;
        }
        this.gameObject.SetActive(value);
    }

    public void EnableDot()
    {
        this._black.SetActive(false);
        this._white.SetActive(false);
        this._dot.SetActive(true);
        gameObject.SetActive(true);
    }

    public void Reverse()
    {
        if (CurrentState == State.None)
        {
            Debug.LogError("Invalid Stone State");
            return;
        }

        switch (CurrentColor)
        {
            case Color.Black:
                CurrentColor = Color.White;
                break;

            case Color.White:
                CurrentColor = Color.Black;
                break;
        }
        CurrentState = State.Reversing;
        _stateChangedAt = DateTime.UtcNow;
        //Game.Instance.PlayStoneReverseSe();

    }

    private void Update()
    {
        switch (CurrentState)
        {
            case State.Appearing:
                
                // position animation
                {
                    transform.localRotation = Rotation;
                    var startPos = transform.localPosition;
                    var endPos = startPos;
                    startPos.y = 3;
                    endPos.y = 0;
                    var t = Mathf.Clamp01(1 - ElapsedSecondsSinceStateChanged / AppearSeconds);
                    t = 1 - t * t * t * t;
                    transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                    if (AppearSeconds < ElapsedSecondsSinceStateChanged)
                    {
                        transform.localPosition = endPos;
                        CurrentState = State.Fix;
                    }
                }
                break;

            case State.Reversing:
                {
                    // rotation animation
                    var startRot = Quaternion.identity;
                    var endRot = Rotation;
                    switch (CurrentColor)
                    {
                        case Color.Black:
                            startRot = Quaternion.Euler(0, 0, 180);
                            break;
                        case Color.White:
                            startRot = Quaternion.Euler(0, 0, 0);
                            break;
                    }
                    var t = Mathf.Clamp01(1 - ElapsedSecondsSinceStateChanged / ReverseSeconds);
                    t = 1 - t * t * t * t;
                    transform.localRotation = Quaternion.Lerp(startRot, endRot, t);

                    // position animation
                    var maxY = 5f;
                    t = Mathf.Clamp01(ElapsedSecondsSinceStateChanged / ReverseSeconds);
                    var pos = transform.localPosition;
                    pos.y = maxY * Mathf.Sin(t * Mathf.PI);
                    transform.localPosition = pos;

                    if (ReverseSeconds < ElapsedSecondsSinceStateChanged)
                    {
                        pos.y = 0;
                        transform.localPosition = pos;
                        transform.localRotation = endRot;
                        CurrentState = State.Fix;

                    }

                }
                break;

            case State.None:
            case State.Fix:
            default:
                break;
        }
    }
}
