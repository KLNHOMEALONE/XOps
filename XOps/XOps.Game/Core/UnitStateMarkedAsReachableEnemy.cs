namespace XOps.Core
{
    public class UnitStateMarkedAsReachableEnemy : UnitState
    {
        public UnitStateMarkedAsReachableEnemy(Unit unit) : base(unit)
        {
        }

        public override void Apply()
        {
            Unit.MarkAsReachableEnemy();
        }

        public override void MakeTransition(UnitState state)
        {
            state.Apply();
            Unit.UnitState = state;
        }
    }
}