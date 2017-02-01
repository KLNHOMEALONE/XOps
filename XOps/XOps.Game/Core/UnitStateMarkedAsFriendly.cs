namespace XOps.Core
{
    public class UnitStateMarkedAsFriendly : UnitState
    {
        public UnitStateMarkedAsFriendly(Unit unit) : base(unit)
        {
        }

        public override void Apply()
        {
            Unit.MarkAsFriendly();
        }

        public override void MakeTransition(UnitState state)
        {
            state.Apply();
            Unit.UnitState = state;
        }
    }
}