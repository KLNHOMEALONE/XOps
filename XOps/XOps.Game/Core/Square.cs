using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Mathematics;

namespace XOps.Core
{
    public  abstract class Square : Cell
    {
        
        protected static readonly Vector2[] _directions = { new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 1), new Vector2(0, -1) };
        //Distance is given using Manhattan Norm.
        public override int GetDistance(Cell other)
        {
            return (int)(Math.Abs(OffsetCoord.X - other.OffsetCoord.X) + Math.Abs(OffsetCoord.Y - other.OffsetCoord.Y));
        }

        //Each square cell has four neighbors, which positions on grid relative to the cell are stored in _directions constant.
        //It is totally possible to implement squares that have eight neighbours, it would require modification of GetDistance function though.
        public override List<Cell> GetNeighbours(List<Cell> cells)
        {
            return _directions.Select(direction => cells.Find(c => c.OffsetCoord == OffsetCoord + direction)).Where(neighbour => neighbour != null).ToList();
        }
    }
}