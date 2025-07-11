using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexCell
{
    /// <summary>
    /// Указывает id игрока, чье это поле при расстановке войск
    /// </summary>
    private int _deploymentPlayerId;

    /// <summary>
    /// На основе этого значения рассчитывается в какую сторону смотрят юниты в начале боя
    /// </summary>
    private HexCellHash _deploymentDirection;

    /// <summary>
    /// Занята ли эта клетка уже чем-то
    /// </summary>
    private bool _isOccupied;
    

    public int DeploymentPlayerId
    {
        get => _deploymentPlayerId;
        set => _deploymentPlayerId = value;
    }

    public HexCellHash DeploymentDirection
    {
        get => _deploymentDirection;
        set => _deploymentDirection = value;
    }

    public bool IsOccupied
    {
        get => _isOccupied;
        set => _isOccupied = value;
    }
    
    
    
}

[Serializable]
public struct HexCellHash
{
    public int q;
    public int r;
    
    public HexCellHash(int q, int r)
    {
        this.q = q;
        this.r = r;
    }

    public Vector2 ToVector2()
    {
        return new Vector2(q,r);
    }

    public static HexCellHash Zero()
    {
        return new HexCellHash(0, 0);
    }

    public static HexCellHash LeftDown()
    {
        return new HexCellHash(0, -1);
    }
    
    public static HexCellHash Left()
    {
        return new HexCellHash(-1, 0);
    }
    
    public static HexCellHash LeftUp()
    {
        return new HexCellHash(-1, 1);
    }
    
    public static HexCellHash RightDown()
    {
        return new HexCellHash(1, -1);
    }
    
    public static HexCellHash Right()
    {
        return new HexCellHash(1, 0);
    }
    
    public static HexCellHash RightUp()
    {
        return new HexCellHash(0, 1);
    }

    public static HexCellHash IncorrectValue()
    {
        return new HexCellHash(int.MinValue, int.MinValue);
    }

    public static HexCellHash operator +(HexCellHash a) => a;
    public static HexCellHash operator -(HexCellHash a) => new HexCellHash(-a.q, -a.r);

    public static HexCellHash operator +(HexCellHash a, HexCellHash b)
        => new HexCellHash(a.q + b.q, a.r + b.r);

    public static HexCellHash operator -(HexCellHash a, HexCellHash b)
        => a + (-b);
    
    public static HexCellHash operator /(HexCellHash a, int b)
        => new HexCellHash(a.q/b, a.r/b);
    
    public static bool operator ==(HexCellHash a, HexCellHash b)
        => a.q == b.q && a.r == b.r;

    public static bool operator !=(HexCellHash a, HexCellHash b) => !(a == b);

    public override string ToString()
    {
        return $"({q},{r},{-q-r})";
    }
    
}
