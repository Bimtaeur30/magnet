using System.Collections.Generic;
using JTH.Scripts.Domain.Placement;
using UnityEngine;

namespace JTH.Scripts.Domain.Clear
{
    /// <summary>
    /// 이젝터를 half+1 링부터, 링 안은 12시→시계방향으로 정렬한다.
    /// </summary>
    public static class CellRelocationOrder
    {
        public static void Sort(List<OccupiedCell> ejectors)
        {
            ejectors.Sort(Compare);
        }

        public static int Compare(OccupiedCell a, OccupiedCell b)
        {
            int ringA = Chebyshev(a.Position);
            int ringB = Chebyshev(b.Position);
            if (ringA != ringB)
            {
                return ringA.CompareTo(ringB);
            }

            return ClockAngle01(a.Position).CompareTo(ClockAngle01(b.Position));
        }

        public static int Chebyshev(Vector2Int cell)
        {
            return Mathf.Max(Mathf.Abs(cell.x), Mathf.Abs(cell.y));
        }

        /// <summary>12시=0, 시계방향 증가, [0,1).</summary>
        public static float ClockAngle01(Vector2Int cell)
        {
            float radians = Mathf.Atan2(cell.x, cell.y);
            if (radians < 0f)
            {
                radians += Mathf.PI * 2f;
            }

            return radians / (Mathf.PI * 2f);
        }
    }
}
