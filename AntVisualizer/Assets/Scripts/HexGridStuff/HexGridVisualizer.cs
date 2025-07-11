using System;
using System.Collections.Generic;
using ElectricServiceCompany;
using UnityEngine;

namespace Game.Scripts.CombatVisualization
{
    public class HexGridVisualizer : MonoBehaviour
    {
        [SerializeField] private HexCellVis _hexCellVisPrefab;
        [SerializeField] private Transform _hexViewOrigin;
        [SerializeField] private LineRenderer _pathRenderer;

        private Dictionary<HexCellHash, HexCellVis> _cellDict = new Dictionary<HexCellHash, HexCellVis>();
        
        private HexGridHelper _helper;

        private HexCellVis _selectedCell;
        public Transform HexViewOrigin => _hexViewOrigin;

        public HexGridHelper Helper => _helper;

        private CombatField _combatField = new CombatField();
        private void Awake()
        {
            _helper = new HexGridHelper();
        }

        private void Start()
        {
            _pathRenderer.enabled = false;

            _combatField.GenerateCircularField(_helper, 5);
            InitVisualizeCombatField(_combatField);
        }

        public void InitVisualizeCombatField(CombatField combatField)
        {
            var field = combatField.Field;

            foreach (var kv in field)
            {
                var cell = Instantiate(_hexCellVisPrefab, _hexViewOrigin);
                cell.transform.localPosition = CellHashToNormalizedOffset(kv.Key);
                _cellDict.Add(kv.Key, cell);
            }
            
            /*foreach (var kv in _combat.CurrentState.CombatField)
            {
                if (kv.Value.DeploymentPlayerId == 1)
                {
                    _cellDict[kv.Key].SetColor(Color.blue);
                }
                else if(kv.Value.DeploymentPlayerId == 2)
                {
                    _cellDict[kv.Key].SetColor(Color.red);
                }
            }*/
        }

        public void SetCellFilled(HexCellHash hash)
        {
            _cellDict[hash].SetFilled();
        }
        
        public void SetCellUnfilled(HexCellHash hash)
        {
            _cellDict[hash].SetUnfilled();
        }

        public void ResetCellFilledAll()
        {
            foreach (var kv in _cellDict)
            {
                kv.Value.SetUnfilled();
            }
        }

        public void SetCellColor(HexCellHash hash, Color color, object source, int priority = 10)
        {
            _cellDict[hash].SetColor(source, priority, color);
        }

        public void ResetCellColor(HexCellHash hash, object source)
        {
            if (_cellDict.TryGetValue(hash, out var cell))
            {
                cell.ResetColor(source);
            }
        }
        
        public void ResetCellColorAll(object source)
        {
            foreach (var kv in _cellDict)
            {
                kv.Value.ResetColor(source);
            }
        }
        

        public void SetPathRendering(List<HexCellHash> path)
        {
            _pathRenderer.positionCount = path.Count;
            var positions = new Vector3[path.Count];

            for (int i = 0; i < path.Count; i++)
            {
                positions[i] = CellHashToNormalizedOffset(path[i]);
            }
            
            _pathRenderer.SetPositions(positions);
            _pathRenderer.enabled = true;
        }
        
        public void SetPathRendering(List<Vector2> path)
        {
            _pathRenderer.positionCount = path.Count;
            var positions = new Vector3[path.Count];

            for (int i = 0; i < path.Count; i++)
            {
                positions[i] = CellHashInVectorToNormalizedOffset(path[i]);
            }
            
            _pathRenderer.SetPositions(positions);
            _pathRenderer.enabled = true;
        }
        

        public void ResetPathRendering()
        {
            _pathRenderer.enabled = false;
        }

        private void Update()
        {
            if (_selectedCell != null)
            {
                _selectedCell.ResetColor(this);
            }

            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
           /* var offset = pos.SetZ(0) - _hexViewOrigin.position.SetZ(0);
            offset.Set(offset.x/_hexViewOrigin.localScale.x, offset.y/_hexViewOrigin.localScale.y,0);
            var hex = NormalizedOffsetToCellHash(offset);

            if (_cellDict.TryGetValue(hex, out var cellVis))
            {
                cellVis.Select();
                _selectedCell = cellVis;
            }*/
            
        }

        public void OnHoverNewCell(HexCellHash oldSelectedCell, HexCellHash newSelectedCell)
        {
            /*if (_cellDict.TryGetValue(newSelectedCell, out var cellVis))
            {
                cellVis.SetColor(this, 100, Color.green);
            }

            if (_cellDict.TryGetValue(oldSelectedCell, out var cell))
            {
                cell.ResetColor(this);
            }*/
        }

        public HexCellHash GetHexCellHashByScreenPoint(Vector3 screenPoint)
        {
            var offset = screenPoint.SetZ(0) - _hexViewOrigin.position.SetZ(0);
            offset.Set(offset.x/_hexViewOrigin.localScale.x, offset.y/_hexViewOrigin.localScale.y,0);
            var hex = NormalizedOffsetToCellHash(offset);

            if (_cellDict.TryGetValue(hex, out var cellVis))
            {
                return hex;
            }

            return HexCellHash.IncorrectValue();
        }


        public Vector2 CellHashToWorldPosition(HexCellHash cell)
        {
            var offset = CellHashToNormalizedOffset(cell);
            return _hexViewOrigin.TransformPoint(offset);
        }

        public Vector2 CellHashInVectorToNormalizedOffset(Vector2 cell)
        {
            var x = Mathf.Sqrt(3) * cell.x + Mathf.Sqrt(3) / 2 * cell.y;
            var y = 3.0f / 2 * cell.y;

            return new Vector2(x, y)/2f;
        }

        public Vector2 CellHashToNormalizedOffset(HexCellHash cell)
        {
            return CellHashInVectorToNormalizedOffset(cell.ToVector2());
            var x = Mathf.Sqrt(3) * cell.q + Mathf.Sqrt(3) / 2 * cell.r;
            var y = 3.0f / 2 * cell.r;

            return new Vector2(x, y)/2f;
        }

        public HexCellHash NormalizedOffsetToCellHash(Vector2 offset)
        {
            offset = offset * 2;
            var q = Mathf.Sqrt(3) / 3 * offset.x - (1.0f / 3) * offset.y;
            var r = (2.0f / 3) * offset.y;
            var s = -q - r;
            
            return _helper.RoundToHex(new Vector3(q,r,s));
        }
        
        
    }
}