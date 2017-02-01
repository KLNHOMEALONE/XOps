using System.Collections.Generic;
using SiliconStudio.Xenko.Engine;

namespace XOps.Core
{
    public class CellGrid : SyncScript
    {
        private CellGridGenerator _generator;

        private CellGridState _cellGridState;//The grid delegates some of its behaviours to cellGridState object.
        public CellGridState CellGridState
        {
            private get
            {
                return _cellGridState;
            }
            set
            {
                _cellGridState?.OnStateExit();
                _cellGridState = value;
                _cellGridState.OnStateEnter();
            }
        }

        public List<Cell> Cells { get; private set; }
        public List<Unit> Units { get; private set; }

        public override void Start()
        {
            _generator = Entity.Get<CellGridGenerator>();
            Cells = _generator.Cells;
        }

        public override void Update()
        {
            
        }
    }
}