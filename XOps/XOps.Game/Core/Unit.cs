using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Events;
using SiliconStudio.Xenko.Physics;
using XOps.Common;
using XOps.Core.Events;
using XOps.Player;

namespace XOps.Core
{
    public abstract class Unit : SyncScript
    {
        private static readonly Pathfinding Pathfinder = new AStarPathfinding();
        private CharacterComponent _character;
        private float _yawOrientation;
        private Entity _modelEntity;

        // The PlayerController will propagate its speed to the AnimationController
        public static readonly EventKey<float> RunSpeedEventKey = new EventKey<float>();

        // The PlayerController will propagate if it is attacking to the AnimationController
        public static readonly EventKey<bool> IsAttackingEventKey = new EventKey<bool>();

        private readonly EventReceiver<ClickResult> _moveDestinationEvent = new EventReceiver<ClickResult>(PlayerInput.HoverMouseEventKey);

        private Vector3 _moveDestination;

        // Allow some inertia to the movement
        private Vector3 _moveDirection = Vector3.Zero;

        private int _waypointIndex;

        private List<Cell> _pathToDestination;

        private bool _isRunning = false;

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
        //public float MovementSpeed;

        /// <summary>
        /// The maximum speed the character can run at
        /// </summary>
        [Display("Run Speed")]
        public float MaxRunSpeed { get; set; } = 15;


        /// <summary>
        /// The distance from the destination at which the character will stop moving
        /// </summary>
        public float DestinationThreshold { get; set; } = 0.6f;

        /// <summary>
        /// A number from 0 to 1 indicating how much a character should slow down when going around corners
        /// </summary>
        /// <remarks>0 is no slowdown and 1 is completely stopping (on >90 degree angles)</remarks>
        public float CornerSlowdown { get; set; } = 0.6f;

        /// <summary>
        /// Multiplied by the distance to the target and clamped to 1 and used to slow down when nearing the destination
        /// </summary>
        public float DestinationSlowdown { get; set; } = 0.4f;

        private bool ReachedDestination => _waypointIndex >= _pathToDestination.Count; 

        private Vector3 CurrentWaypoint => _waypointIndex < _pathToDestination.Count ? _pathToDestination[_waypointIndex].Entity.Transform.Position : Vector3.Zero;

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

        public override void Start()
        {
            base.Start();
            _modelEntity = Entity.GetChild(0);
            _character = Entity.Get<CharacterComponent>();
            _moveDestination = Entity.Transform.WorldMatrix.TranslationVector;
        }

        public override void Update()
        {
            Move(MaxRunSpeed);
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

        private void Move(float maxRunSpeed)
        {
            if (!_isRunning)
            {
                RunSpeedEventKey.Broadcast(0);
                return;
            }
            UpdateMoveTowardsDestination(maxRunSpeed);
        }

        private void UpdateDestination(Cell destination)
        {

            Vector3 delta = _moveDestination - destination.Entity.Transform.Position;
            // Skip the points that are too close to the player

            while (!ReachedDestination &&
                   (CurrentWaypoint - Entity.Transform.WorldMatrix.TranslationVector).Length() < 0.25f)
            {
                _waypointIndex++;
            }

            // If this path still contains more points, set the player to running
            if (!ReachedDestination)
            {
                _isRunning = true;
                _moveDestination = destination.Entity.Transform.Position;
            }
            else
            {
                HaltMovement();
            }
        }

        private void UpdateMoveTowardsDestination(float speed)
        {
            if (!ReachedDestination)
            {
                var direction = CurrentWaypoint - Entity.Transform.WorldMatrix.TranslationVector;

                // Get distance towards next point and normalize the direction at the same time
                var length = direction.Length();
                direction /= length;

                // Check when to advance to the next waypoint
                bool advance = false;

                // Check to see if an intermediate point was passed by projecting the position along the path
                if (_pathToDestination.Count > 0 && _waypointIndex > 0 && _waypointIndex != _pathToDestination.Count - 1)
                {
                    Vector3 pointNormal = CurrentWaypoint - _pathToDestination[_waypointIndex - 1].Entity.Transform.Position;
                    pointNormal.Normalize();
                    float current = Vector3.Dot(Entity.Transform.WorldMatrix.TranslationVector, pointNormal);
                    float target = Vector3.Dot(CurrentWaypoint, pointNormal);
                    if (current > target)
                    {
                        advance = true;
                    }
                }
                else
                {
                    if (length < DestinationThreshold) // Check distance to final point
                    {
                        advance = true;
                    }
                }

                // Advance waypoint?
                if (advance)
                {
                    _waypointIndex++;
                    if (ReachedDestination)
                    {
                        // Final waypoint reached
                        HaltMovement();
                        return;
                    }
                }

                // Calculate speed based on distance from final destination
                float moveSpeed = (_moveDestination - Entity.Transform.WorldMatrix.TranslationVector).Length() * DestinationSlowdown;
                if (moveSpeed > 1.0f)
                    moveSpeed = 1.0f;

                // Slow down around corners
                float cornerSpeedMultiply = Math.Max(0.0f, Vector3.Dot(direction, _moveDirection)) * CornerSlowdown + (1.0f - CornerSlowdown);

                // Allow a very simple inertia to the character to make animation transitions more fluid
                _moveDirection =_moveDirection * 0.85f + direction * moveSpeed * cornerSpeedMultiply * 0.15f;

                _character.SetVelocity(_moveDirection * speed);

                // Broadcast speed as per cent of the max speed
                RunSpeedEventKey.Broadcast(_moveDirection.Length());

                // Character orientation
                if (_moveDirection.Length() > 0.001)
                {
                    _yawOrientation = MathUtil.RadiansToDegrees((float)Math.Atan2(-_moveDirection.Z, _moveDirection.X) + MathUtil.PiOverTwo);
                }
                _modelEntity.Transform.Rotation = Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(_yawOrientation), 0, 0);
            }
            else
            {
                // No target
                HaltMovement();
            }
        }

        private void HaltMovement()
        {
            _isRunning = false;
            _moveDirection = Vector3.Zero;
            _character.SetVelocity(Vector3.Zero);
            _moveDestination = Entity.Transform.WorldMatrix.TranslationVector;
        }

        public virtual void Move(Cell destinationCell, List<Cell> path)
        {
            if (_isRunning)
                return;

            var totalMovementCost = path.Sum(h => h.MovementCost);
            if (MovementPoints < totalMovementCost)
                return;
            path.Reverse();
            MovementPoints -= totalMovementCost;

            Cell.IsTaken = false;
            Cell = destinationCell;
            destinationCell.IsTaken = true;

            _pathToDestination = path;
            _waypointIndex = 0;
            UpdateDestination(destinationCell);
            //if (MovementSpeed > 0)
            //    MovementAnimation(path);
            //else
            //    Entity.Transform.Position = Cell.GetPosition();

            //UnitMoved?.Invoke(this, new MovementEventArgs(Cell, destinationCell, path));
        }
        //protected void MovementAnimation(List<Cell> path)
        //{
        //    IsMoving = true;
        //    path.Reverse();
        //    foreach (var cell in path)
        //    {
        //        UpdateDestination(cell);
        //    }
        //    IsMoving = false;
        //}

        //private void UpdateDestination(Cell cell)
        //{
        //    var currentWaypoint = cell.Entity.Transform.Position;
        //    var direction = currentWaypoint - Entity.Transform.WorldMatrix.TranslationVector;

        //    // Get distance towards next point and normalize the direction at the same time
        //    var length = direction.Length();
        //    direction /= length;

        //    // Calculate speed based on distance from final destination
        //    float moveSpeed = (_moveDestination - Entity.Transform.WorldMatrix.TranslationVector).Length() * DestinationSlowdown;
        //    if (moveSpeed > 1.0f)
        //        moveSpeed = 1.0f;

        //    // Slow down around corners
        //    float cornerSpeedMultiply = Math.Max(0.0f, Vector3.Dot(direction, _moveDirection)) * CornerSlowdown + (1.0f - CornerSlowdown);

        //    // Allow a very simple inertia to the character to make animation transitions more fluid
        //    _moveDirection = _moveDirection * 0.85f + direction * moveSpeed * cornerSpeedMultiply * 0.15f;

        //    _character.SetVelocity(_moveDirection * MaxRunSpeed);
        //}

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