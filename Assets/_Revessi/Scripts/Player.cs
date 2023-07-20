using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player : MonoBehaviour
{
    public abstract Stone.Color MyColor { get; }
    public virtual bool TryGetSelected(out int x, out int z)
    {
        x = 0;
        z = 0;
        return false;
    }

    public Dictionary<Tuple<int, int>, int> CalcAvailablePoints()
    {
        var game = Game.Instance;
        var stones = game.Stones;
        var availablePoints = new Dictionary<Tuple<int, int>, int>();
        for (var z = 0; z < Game.ZNum; z++)
        {
            for (var x = 0; x < Game.XNum; x++)
            {
                if (stones[z][x].CurrentState == Stone.State.None)
                {
                    var reverseCount = game.CalcTotalReverseCount(MyColor, x, z);
                    if (reverseCount > 0)
                    {
                        availablePoints[Tuple.Create(x, z)] = reverseCount;
                    }
                }
            }
        }
        return availablePoints;
    }
    public bool CanPut()
    {
        return CalcAvailablePoints().Count > 0;
    }
}
