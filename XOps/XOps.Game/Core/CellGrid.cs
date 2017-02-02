using System.Collections.Generic;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Events;
using XOps.Common;
using XOps.Player;

namespace XOps.Core
{
    public class CellGrid : SyncScript
    {
        private readonly EventReceiver<ClickResult> _moveDestinationEvent = new EventReceiver<ClickResult>(PlayerInput.MoveDestinationEventKey);

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
              MoveTo();        
        }

        private void MoveTo()
        {
            // Character speed
            ClickResult clickResult;
            if (_moveDestinationEvent.TryReceive(out clickResult) && clickResult.Type != ClickType.Empty)
            {
                //if (clickResult.Type == ClickType.Ground)
                //{
                //    attackEntity = null;
                //    UpdateDestination(clickResult.WorldPosition);
                //}

                //if (clickResult.Type == ClickType.LootCrate)
                //{
                //    attackEntity = clickResult.ClickedEntity;
                //    Attack();
                //}
            }
        }
    }
}