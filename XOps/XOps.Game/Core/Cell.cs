using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine.Events;
using SiliconStudio.Xenko.Rendering;
using XOps.Common;

namespace XOps.Core
{
    public abstract class Cell : SyncScript, IGraphNode
    {
        //[DataMember(20)]
        //[Display("Source")]
        //public Model Source { get; set; }
        public Vector2 OffsetCoord { get; set; }

        /// <summary>
        /// Indicates if something is occupying the cell.
        /// </summary>
        public bool IsTaken;
        /// <summary>
        /// Cost of moving through the cell.
        /// </summary>
        public int MovementCost;

        //public Vector3 CellSize { get; set; }


        //public event EventHandler CellClicked;

        private readonly EventReceiver<Cell> _unitClickedEventReceiver = new EventReceiver<Cell>(Unit.CellClickedEventKey);
        /// <summary>
        /// CellHighlighed event is invoked when user moves cursor over the cell. It requires a collider on the cell game object to work.
        /// </summary>
        public event EventHandler CellHighlighted;
        public event EventHandler CellDehighlighted;

        protected virtual void OnMouseEnter()
        {
            CellHighlighted?.Invoke(this, new EventArgs());
        }

        protected virtual void OnMouseExit()
        {
            CellDehighlighted?.Invoke(this, new EventArgs());
        }

        void OnMouseDown()
        {

            //CellClicked?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Method returns distance to other cell, that is given as parameter. 
        /// </summary>
        public abstract int GetDistance(Cell other);

        /// <summary>
        /// Method returns cells adjacent to current cell, from list of cells given as parameter.
        /// </summary>
        public abstract List<Cell> GetNeighbours(List<Cell> cells);

        public abstract Vector3 GetCellDimensions(Entity entity); //Cell dimensions are necessary for grid generators.

        /// <summary>
        ///  Method marks the cell to give user an indication, that selected unit can reach it.
        /// </summary>
        public abstract void MarkAsReachable();
        /// <summary>
        /// Method marks the cell as a part of a path.
        /// </summary>
        public abstract void MarkAsPath();
        /// <summary>
        /// Method marks the cell as highlighted. It gets called when the mouse is over the cell.
        /// </summary>
        public abstract void MarkAsHighlighted();
        /// <summary>
        /// Method returns the cell to its base appearance.
        /// </summary>
        public abstract void UnMark();

        public int GetDistance(IGraphNode other)
        {
            return GetDistance(other as Cell);
        }
    }
}