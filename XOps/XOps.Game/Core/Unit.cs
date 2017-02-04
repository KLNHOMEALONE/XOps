using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Events;
using XOps.Common;
using XOps.Core.Events;
using XOps.Player;

namespace XOps.Core
{
    public abstract class Unit : SyncScript
    {
        private static readonly Pathfinding Pathfinder = new AStarPathfinding();

        private readonly EventReceiver<ClickResult> _moveDestinationEvent = new EventReceiver<ClickResult>(PlayerInput.HoverMouseEventKey);
        /// <summary>
        /// CellClicked event is invoked when user clicks the unit. It requires a collider on the cell game object to work.
        /// </summary>
        public static readonly EventKey<Cell> CellClickedEventKey = new EventKey<Cell>();

        /// <summary>
        /// UnitClicked event is invoked when user clicks the unit. It requires a collider on the unit game object to work.
        /// </summary>
        public event EventHandler UnitClicked;
        /// <summary>
        /// UnitSelected event is invoked when user clicks on unit that belongs to him. It requires a collider on the unit game object to work.
        /// </summary>
        public event EventHandler UnitSelected;
        public event EventHandler UnitDeselected;
        /// <summary>
        /// UnitHighlighted event is invoked when user moves cursor over the unit. It requires a collider on the unit game object to work.
        /// </summary>
        public event EventHandler UnitHighlighted;
        public event EventHandler UnitDehighlighted;
        public event EventHandler<AttackEventArgs> UnitAttacked;
        public event EventHandler<AttackEventArgs> UnitDestroyed;
        public event EventHandler<MovementEventArgs> UnitMoved;

        public UnitState UnitState { get; set; }
        public void SetState(UnitState state)
        {
            UnitState.MakeTransition(state);
        }

        public List<IBuff> Buffs { get; private set; }

        public int TotalHitPoints { get; private set; }
        protected int TotalMovementPoints;
        protected int TotalActionPoints;

        /// <summary>
        /// Cell that the unit is currently occupying.
        /// </summary>
        public Cell Cell { get; set; }

        public int HitPoints;
        public int AttackRange;
        public int AttackFactor;
        public int DefenceFactor;
        /// <summary>
        /// Determines how far on the grid the unit can move.
        /// </summary>
        public int MovementPoints;
        /// <summary>
        /// Determines speed of movement animation.
        /// </summary>
        public float MovementSpeed;
        /// <summary>
        /// Determines how many attacks unit can perform in one turn.
        /// </summary>
        public int ActionPoints;

        /// <summary>
        /// Indicates the player that the unit belongs to. Should correspoond with PlayerNumber variable on Player script.
        /// </summary>
        public int PlayerNumber;

        /// <summary>
        /// Indicates if movement animation is playing.
        /// </summary>
        public bool IsMoving { get; set; }

        public override void Update()
        {
        }

        /// <summary>
        /// Method called after object instantiation to initialize fields etc. 
        /// </summary>
        public virtual void Initialize()
        {
            Buffs = new List<IBuff>();

            UnitState = new UnitStateNormal(this);

            TotalHitPoints = HitPoints;
            TotalMovementPoints = MovementPoints;
            TotalActionPoints = ActionPoints;
        }

        protected virtual void OnMouseDown()
        {
            UnitClicked?.Invoke(this, new EventArgs());
        }

        protected virtual void OnMouseEnter()
        {
            UnitHighlighted?.Invoke(this, new EventArgs());
        }

        protected virtual void OnMouseExit()
        {
            UnitDehighlighted?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Method is called at the start of each turn.
        /// </summary>
        public virtual void OnTurnStart()
        {
            MovementPoints = TotalMovementPoints;
            ActionPoints = TotalActionPoints;
            SetState(new UnitStateMarkedAsFriendly(this));
        }
        /// <summary>
        /// Method is called at the end of each turn.
        /// </summary>
        public virtual void OnTurnEnd()
        {
            Buffs.FindAll(b => b.Duration == 0).ForEach(b => { b.Undo(this); });
            Buffs.RemoveAll(b => b.Duration == 0);
            Buffs.ForEach(b => { b.Duration--; });

            SetState(new UnitStateNormal(this));
        }
        /// <summary>
        /// Method is called when units HP drops below 1.
        /// </summary>
        protected virtual void OnDestroyed()
        {
            Cell.IsTaken = false;
            MarkAsDestroyed();
            //Destroy(gameObject);
        }

        /// <summary>
        /// Method is called when unit is selected.
        /// </summary>
        public virtual void OnUnitSelected()
        {
            SetState(new UnitStateMarkedAsSelected(this));
            UnitSelected?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// Method is called when unit is deselected.
        /// </summary>
        public virtual void OnUnitDeselected()
        {
            SetState(new UnitStateMarkedAsFriendly(this));
            UnitDeselected?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Method indicates if it is possible to attack unit given as parameter, from cell given as second parameter.
        /// </summary>
        public virtual bool IsUnitAttackable(Unit other, Cell sourceCell)
        {
            if (sourceCell.GetDistance(other.Cell) <= AttackRange)
                return true;

            return false;
        }
        /// <summary>
        /// Method deals damage to unit given as parameter.
        /// </summary>
        public virtual void DealDamage(Unit other)
        {
            if (IsMoving)
                return;
            if (ActionPoints == 0)
                return;
            if (!IsUnitAttackable(other, Cell))
                return;

            MarkAsAttacking(other);
            ActionPoints--;
            other.Defend(this, AttackFactor);

            if (ActionPoints == 0)
            {
                SetState(new UnitStateMarkedAsFinished(this));
                MovementPoints = 0;
            }
        }
        /// <summary>
        /// Attacking unit calls Defend method on defending unit. 
        /// </summary>
        protected virtual void Defend(Unit other, int damage)
        {
            MarkAsDefending(other);
            // Damage is calculated by subtracting attack factor of attacker and defence factor of defender. If result is below 1, it is set to 1.
            // This behaviour can be overridden in derived classes.
            HitPoints -= MathUtil.Clamp(damage - DefenceFactor, 1, damage);  
                                                                          
            UnitAttacked?.Invoke(this, new AttackEventArgs(other, this, damage));

            if (HitPoints <= 0)
            {
                UnitDestroyed?.Invoke(this, new AttackEventArgs(other, this, damage));
                OnDestroyed();
            }
        }

        public virtual void Move(Cell destinationCell, List<Cell> path)
        {
            if (IsMoving)
                return;

            var totalMovementCost = path.Sum(h => h.MovementCost);
            if (MovementPoints < totalMovementCost)
                return;

            MovementPoints -= totalMovementCost;

            Cell.IsTaken = false;
            Cell = destinationCell;
            destinationCell.IsTaken = true;

            if (MovementSpeed > 0)
                MovementAnimation(path);
            else
                Entity.Transform.Position = Cell.GetPosition();

            UnitMoved?.Invoke(this, new MovementEventArgs(Cell, destinationCell, path));
        }
        protected virtual IEnumerator MovementAnimation(List<Cell> path)
        {
            IsMoving = true;

            path.Reverse();
            foreach (var cell in path)
            {
                while (new Vector2(Entity.Transform.Position.X, Entity.Transform.Position.Z) != new Vector2(cell.GetPosition().X, cell.GetPosition().Z))
                {
                    //TODO:
                    //Entity.Transform.Position = Vector3.MoveTowards(Entity.Transform.Position, new Vector3(cell.GetPosition().X, cell.GetPosition().Z, Entity.Transform.Position.Z), Time.deltaTime * MovementSpeed);
                    yield return 0;
                }
            }

            IsMoving = false;
        }

        ///<summary>
        /// Method indicates if unit is capable of moving to cell given as parameter.
        /// </summary>
        public virtual bool IsCellMovableTo(Cell cell)
        {
            return !cell.IsTaken;
        }
        /// <summary>
        /// Method indicates if unit is capable of moving through cell given as parameter.
        /// </summary>
        public virtual bool IsCellTraversable(Cell cell)
        {
            return !cell.IsTaken;
        }
        /// <summary>
        /// Method returns all cells that the unit is capable of moving to.
        /// </summary>
        public List<Cell> GetAvailableDestinations(List<Cell> cells)
        {
            var ret = new List<Cell>();
            var cellsInMovementRange = cells.FindAll(c => IsCellMovableTo(c) && c.GetDistance(Cell) <= MovementPoints);

            var traversableCells = cells.FindAll(c => IsCellTraversable(c) && c.GetDistance(Cell) <= MovementPoints);
            traversableCells.Add(Cell);

            foreach (var cellInRange in cellsInMovementRange)
            {
                if (cellInRange.Equals(Cell)) continue;

                var path = FindPath(traversableCells, cellInRange);
                var pathCost = path.Sum(c => c.MovementCost);
                if (pathCost > 0 && pathCost <= MovementPoints)
                    ret.AddRange(path);
            }
            return ret.FindAll(IsCellMovableTo).Distinct().ToList();
        }

        public List<Cell> FindPath(List<Cell> cells, Cell destination)
        {
            return Pathfinder.FindPath(GetGraphEdges(cells), Cell, destination);
        }
        /// <summary>
        /// Method returns graph representation of cell grid for pathfinding.
        /// </summary>
        protected virtual Dictionary<Cell, Dictionary<Cell, int>> GetGraphEdges(List<Cell> cells)
        {
            Dictionary<Cell, Dictionary<Cell, int>> ret = new Dictionary<Cell, Dictionary<Cell, int>>();
            foreach (var cell in cells)
            {
                if (IsCellTraversable(cell) || cell.Equals(Cell))
                {
                    ret[cell] = new Dictionary<Cell, int>();
                    foreach (var neighbour in cell.GetNeighbours(cells).FindAll(IsCellTraversable))
                    {
                        ret[cell][neighbour] = neighbour.MovementCost;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Gives visual indication that the unit is under attack.
        /// </summary>
        /// <param name="other"></param>
        public abstract void MarkAsDefending(Unit other);
        /// <summary>
        /// Gives visual indication that the unit is attacking.
        /// </summary>
        /// <param name="other"></param>
        public abstract void MarkAsAttacking(Unit other);
        /// <summary>
        /// Gives visual indication that the unit is destroyed. It gets called right before the unit game object is
        /// destroyed, so either instantiate some new object to indicate destruction or redesign Defend method. 
        /// </summary>
        public abstract void MarkAsDestroyed();

        /// <summary>
        /// Method marks unit as current players unit.
        /// </summary>
        public abstract void MarkAsFriendly();
        /// <summary>
        /// Method mark units to indicate user that the unit is in range and can be attacked.
        /// </summary>
        public abstract void MarkAsReachableEnemy();
        /// <summary>
        /// Method marks unit as currently selected, to distinguish it from other units.
        /// </summary>
        public abstract void MarkAsSelected();
        /// <summary>
        /// Method marks unit to indicate user that he can't do anything more with it this turn.
        /// </summary>
        public abstract void MarkAsFinished();
        /// <summary>
        /// Method returns the unit to its base appearance
        /// </summary>
        public abstract void UnMark();
    }
}