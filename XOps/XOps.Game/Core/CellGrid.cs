using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
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
        private readonly EventReceiver<ClickResult> _unitClickedEvent = new EventReceiver<ClickResult>(PlayerInput.UnitClickedEventKey);
        private readonly System.Random _rnd = new System.Random();
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

        public int NumberOfPlayers { get; private set; }

        [DataMember(20)]
        [Display("CurrentPlayer")]
        public Player CurrentPlayer
        {
            get { return Players?.Find(p => p.PlayerNumber.Equals(CurrentPlayerNumber)); }
        }

        public Prefab PlayerPrefab { get; set; }

        public List<Cell> Cells { get; private set; }
        public List<Unit> Units { get; private set; }

        public List<Player> Players { get; set; } = new List<Player>();

        public int CurrentPlayerNumber { get; set; }

        public override void Start()
        {
            Units = new List<Unit>();
            _generator = Entity.Get<CellGridGenerator>();
            Cells = _generator.Cells;
            PlacePlayer();
            var player = new HumanPlayer() {PlayerNumber = 0};
            Players.Add(player);
            NumberOfPlayers = Players.Count;
            CurrentPlayerNumber = Players.Min(p => p.PlayerNumber);
            StartGame();
            //_cellBoundsX = Cells[0].CellSize.X;
            //_celleBoundsZ = Cells[0].CellSize.Z;
        }

        private void PlacePlayer()
        {
            List<Cell> freeCells = Cells.FindAll(h => h.IsTaken == false);
            freeCells = freeCells.OrderBy(h => _rnd.Next()).ToList();
            var cell = freeCells.ElementAt(0);
            freeCells.RemoveAt(0);
            cell.IsTaken = true;

            var unit = PlayerPrefab.Instantiate().First();
            unit.Transform.Position = cell.Entity.Transform.Position + new Vector3(0, 0, 0);
            unit.Get<Unit>().PlayerNumber = 0;
            unit.Get<Unit>().Cell = cell;
            unit.Get<Unit>().Initialize();
            Units.Add(unit.Get<Unit>());
            SceneSystem.SceneInstance.Scene.Entities.Add(unit);
        }


        public void StartGame()
        {
            //if (GameStarted != null)
            //    GameStarted.Invoke(this, new EventArgs());

            Units.FindAll(u => u.PlayerNumber.Equals(CurrentPlayerNumber)).ForEach(u => { u.OnTurnStart(); });
            Players.Find(p => p.PlayerNumber.Equals(CurrentPlayerNumber)).Play(this);
        }
        public override void Update()
        {
            SelectUnit();
            MoveTo();        
        }

        private void SelectUnit()
        {
            ClickResult clickResult;
            if (_unitClickedEvent.TryReceive(out clickResult) && clickResult.Type != ClickType.Empty)
            {
                var unit = clickResult.ClickedEntity.Get<Unit>();
                if (unit != null)
                {
                    CellGridState.OnUnitClicked(unit);
                }
            }
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

                if (clickResult.Type == ClickType.LootCrate)
                {
                    //attackEntity = clickResult.ClickedEntity;
                    //Attack();
                }
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