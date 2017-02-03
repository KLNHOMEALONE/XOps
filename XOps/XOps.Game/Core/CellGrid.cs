using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
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

        //private float _cellBoundsX, _celleBoundsZ;

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
            //_cellBoundsX = Cells[0].CellSize.X;
            //_celleBoundsZ = Cells[0].CellSize.Z;
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
                if (clickResult.Type == ClickType.Ground)
                {
                    //int index = VectorToIndex2D(clickResult.WorldPosition);
                    //attackEntity = null;
                    //UpdateDestination(clickResult.WorldPosition);
                }

                //if (clickResult.Type == ClickType.LootCrate)
                //{
                //    attackEntity = clickResult.ClickedEntity;
                //    Attack();
                //}
            }
        }

        //public int VectorToIndex2D(Vector3 vector)
        //{
        //    float pivotCenterPointX = (vector.X + _cellBoundsX / 2);
        //    float pivotCenterPointY = (vector.Z + _celleBoundsZ / 2);

        //    int tileBreakerX = (int)Math.Floor(pivotCenterPointX % _cellBoundsX);
        //    int tileBreakerY = (int)Math.Floor(pivotCenterPointY % _celleBoundsZ);

        //    bool resx = Convert.ToBoolean(tileBreakerX);
        //    bool resy = Convert.ToBoolean(tileBreakerY);

        //    int selectX = resx ? 1 : 0;
        //    int selectY = resy ? 1 : 0;

        //    Vector3 location = Entity.Transform.Position;

        //    int posX = (int)Math.Floor((pivotCenterPointX + selectX - location.X) / _cellBoundsX);
        //    int posY = (int)Math.Floor((pivotCenterPointY + selectY - location.Z) / _celleBoundsZ);

        //    //// Each tile in the Y axis increases the index by the grid width
        //    //// Adding X and Y together find the corresponding index in the arrays
        //    int resultIndex = posX + (_generator.Width * posY);

        //    ////not finished yet
        //    return resultIndex >= 0 ? resultIndex : 0;
        //}

        //public Vector3 Vector3FromIndex(int index)
        //{
        //    return IndexToVectorSquareGrid(index);
        //}

        //private Vector3 IndexToVectorSquareGrid(int index)
        //{
        //    var pos = Entity.Transform.Position;
        //    float x = _cellBoundsX * (index % _generator.Width);
        //    float y = index / (_generator.Width * _generator.Height) * _cellBoundsX;
        //    float z = _celleBoundsZ * ((index / _generator.Width) - (_generator.Width * (index / (_generator.Width * _generator.Height))));
        //    return new Vector3(x + pos.X, y + pos.Y, z + pos.Z);
        //}
    }
}