using System.Collections.Generic;
using UnityEngine;

namespace DemonsLord.CombatSystem
{
    public class BattleBoard
    {
        public const int Width = 12;
        public const int Height = 8;

        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        private readonly UnitRuntime[,] _occupants = new UnitRuntime[Width, Height];

        public bool IsInside(Vector2Int position)
        {
            return position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height;
        }

        public UnitRuntime GetUnitAt(Vector2Int position)
        {
            return IsInside(position) ? _occupants[position.x, position.y] : null;
        }

        public bool IsCellFree(Vector2Int position)
        {
            return IsInside(position) && GetUnitAt(position) == null;
        }

        public bool TryPlace(UnitRuntime unit, Vector2Int position)
        {
            if (unit == null || !IsCellFree(position))
            {
                return false;
            }

            _occupants[position.x, position.y] = unit;
            unit.SetPosition(position);
            return true;
        }

        public bool TryMove(UnitRuntime unit, Vector2Int destination)
        {
            if (unit == null || !unit.IsAlive || !IsCellFree(destination))
            {
                return false;
            }

            var current = unit.Position;
            if (!IsInside(current))
            {
                return false;
            }

            _occupants[current.x, current.y] = null;
            _occupants[destination.x, destination.y] = unit;
            unit.SetPosition(destination);
            return true;
        }

        public void Remove(UnitRuntime unit)
        {
            if (unit == null)
            {
                return;
            }

            var position = unit.Position;
            if (IsInside(position) && _occupants[position.x, position.y] == unit)
            {
                _occupants[position.x, position.y] = null;
            }
        }

        public List<Vector2Int> GetAdjacentPositions(Vector2Int origin)
        {
            var result = new List<Vector2Int>(Directions.Length);
            for (int i = 0; i < Directions.Length; i++)
            {
                var next = origin + Directions[i];
                if (IsInside(next))
                {
                    result.Add(next);
                }
            }

            return result;
        }

        public List<Vector2Int> GetMoveHighlights(UnitRuntime unit)
        {
            var result = new List<Vector2Int>();
            if (unit == null || !unit.IsAlive)
            {
                return result;
            }

            var adjacent = GetAdjacentPositions(unit.Position);
            for (int i = 0; i < adjacent.Count; i++)
            {
                if (IsCellFree(adjacent[i]))
                {
                    result.Add(adjacent[i]);
                }
            }

            return result;
        }

        public List<UnitRuntime> GetAttackableUnits(UnitRuntime unit)
        {
            var result = new List<UnitRuntime>();
            if (unit == null || !unit.IsAlive)
            {
                return result;
            }

            var adjacent = GetAdjacentPositions(unit.Position);
            for (int i = 0; i < adjacent.Count; i++)
            {
                var candidate = GetUnitAt(adjacent[i]);
                if (candidate != null && candidate.IsAlive && candidate.Side != unit.Side)
                {
                    result.Add(candidate);
                }
            }

            return result;
        }

        public List<Vector2Int> GetAttackHighlights(UnitRuntime unit)
        {
            var targets = GetAttackableUnits(unit);
            var result = new List<Vector2Int>(targets.Count);
            for (int i = 0; i < targets.Count; i++)
            {
                result.Add(targets[i].Position);
            }

            return result;
        }
    }
}
