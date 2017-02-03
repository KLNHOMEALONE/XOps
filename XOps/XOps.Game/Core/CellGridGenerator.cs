using System.Collections.Generic;
using SiliconStudio.Xenko.Engine;

namespace XOps.Core
{
    public abstract class CellGridGenerator : SyncScript
    {
        public Entity CellsParent;
        public List<Cell> Cells { get; set; } 
        public abstract List<Cell> GenerateGrid();
    }
}