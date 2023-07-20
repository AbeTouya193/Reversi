using UnityEngine;
using UnityEngine.InputSystem;

public class SelfPlayer : Player
{
    public override Stone.Color MyColor { get { return Stone.Color.Black; } }

    private int _processingPlayerTurn = 0;
    private Vector3Int _cursorPos = Vector3Int.zero;
    private Vector3Int? _desidedPos = null;

    public override bool TryGetSelected(out int x, out int z)
    {
        if (_desidedPos.HasValue)
        {
            var pos = _desidedPos.Value;
            x = pos.x;
            z = pos.z;
            return true;
        }
        x = 0;
        z = 0;
        return false;
    }

    private void Update()
    {
        switch (Game.Instance.CurrentState)
        {
            case Game.State.BlackTurn:
                ExecTurn();
                break;

            default:
                break;
        }
    }
    private void ExecTurn()
    {
        var currentTurn = Game.Instance.CurrentTurn;
        if (_processingPlayerTurn !=  currentTurn)
        {
            ShowDots();
            Game.Instance.Cursor.SetActive(true);
            _desidedPos = null;
            _processingPlayerTurn = currentTurn;
        }

        var keyboard = Keyboard.current;
        if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
        {
            TryCursorMove(-1, 0);
        }
        else if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
        {
            TryCursorMove(0, 1);
        }
        else if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
        {
            TryCursorMove(1, 0);
        }
        else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
        {
            TryCursorMove(0, -1);
        }
        else if (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
        {
            if (Game.Instance.CalcTotalReverseCount(MyColor, _cursorPos.x, _cursorPos.z) > 0)
            {
                _desidedPos = _cursorPos;
                Game.Instance.Cursor.SetActive(false);
                HideDots();
            }
        }
    }

    private bool TryCursorMove(int deltaX, int deltaZ)
    {
        var x = _cursorPos.x;
        var z = _cursorPos.z;
        x += deltaX;
        z += deltaZ;
        if (x < 0 || Game.XNum <= x)
            return false;
        if (z < 0 || Game.XNum <= z)
            return false;

        _cursorPos.x = x;
        _cursorPos.z = z;
        Game.Instance.Cursor.transform.localPosition = _cursorPos * 10;
        //Game.Instance.PlayCursorMoveSe();
        return true;
    }

    private void ShowDots()
    {
        var availablePoints = CalcAvailablePoints();
        var stones = Game.Instance.Stones;
        foreach (var availablePoint in availablePoints.Keys)
        {
            var x = availablePoint.Item1;
            var z = availablePoint.Item2;
            stones[z][x].EnableDot();
        }
    }

    private void HideDots()
    {
        var stones = Game.Instance.Stones;
        for (var z = 0; z < Game.ZNum; z++)
        {
            for (var x = 0; x < Game.XNum; x++)
            {
                if (stones[z][x].CurrentState == Stone.State.None)
                {
                    stones[z][x].SetActive(false, Stone.Color.Black);
                }
            }
        }
    }
}
