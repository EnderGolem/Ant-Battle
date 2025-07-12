using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Server.Combat;
using Server.Net.Models;

public class HexCell
{
    private byte _cost;

    private HexType _type;

    private bool _passable;

    public HexCell(HexType type, byte cost, bool passable)
    {
        _type = type;
        _cost = cost;
        _passable = passable;
    }

    public void SetType(HexType type)
    {
        _type = type;
    }

    public byte Cost => _cost;

    public HexType Type => _type;

    public bool Passable => _passable;
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
    
    public static HexCellHash operator *(HexCellHash cellHash, int multiplier) => 
        new HexCellHash(cellHash.q*multiplier, cellHash.r * multiplier);
    
    public static bool operator ==(HexCellHash a, HexCellHash b)
        => a.q == b.q && a.r == b.r;

    public static bool operator !=(HexCellHash a, HexCellHash b) => !(a == b);

    public override string ToString()
    {
        return $"({q},{r},{-q-r})";
    }

    public Coordinate ToCoordinate()
    {
        return new Coordinate { Q = q, R = r };
    }

    public static HexCellHash FromCoordinate(Coordinate coordinate)
    {
        return new HexCellHash(coordinate.Q, coordinate.R);
    }

}
