using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class HexCellVis : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _outline;
    [SerializeField] private SpriteRenderer _fill;

    /// <summary>
    /// ключ - источник установки цвета
    /// 1 - й параметр значения - приоритет
    /// 2 - й параметр значения - цвет
    /// </summary>
    private Dictionary<object, (int, Color)> _setColorRecords = new Dictionary<object, (int, Color)>();
    
    
    public void ClearColor()
    {
        _setColorRecords.Clear();
        _outline.color = Color.black;
    }

    public void Clear()
    {
        ClearColor();
    }

    public void TurnOff()
    {
        
    }

    public void TurnOn()
    {
        
    }

    public void SetFilled()
    {
        _fill.color = new Color(_fill.color.r, _fill.color.g, _fill.color.b, 0.6f);
    }

    public void SetUnfilled()
    {
        _fill.color = new Color(_fill.color.r, _fill.color.g, _fill.color.b, 0);
    }

    public void SetColor(object source, int priority, Color color)
    {
        _setColorRecords[source] = (priority, color);
        EvaluateCellColor();
    }

    public void ResetColor(object source)
    {
        _setColorRecords.Remove(source);
        EvaluateCellColor();
    }

    private void EvaluateCellColor()
    {
        if (_setColorRecords.Count == 0)
        {
            _outline.color = Color.black;
            return;
        }

        int maxPriority = int.MinValue;
        Color maxColor = Color.black;
        foreach (var record in _setColorRecords)
        {
            if (record.Value.Item1 >= maxPriority)
            {
                maxPriority = record.Value.Item1;
                maxColor = record.Value.Item2;
            }
        }

        _outline.color = maxColor;
    }
    
}
