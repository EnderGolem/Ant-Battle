using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HexCell
{
    private int _cost;

    public HexCell(int cost)
    {
        
    }

    public int Cost => _cost;
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
    
    public static bool operator ==(HexCellHash a, HexCellHash b)
        => a.q == b.q && a.r == b.r;

    public static bool operator !=(HexCellHash a, HexCellHash b) => !(a == b);

    public override string ToString()
    {
        return $"({q},{r},{-q-r})";
    }
    
}
