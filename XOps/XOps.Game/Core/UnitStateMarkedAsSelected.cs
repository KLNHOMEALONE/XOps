namespace XOps.Core
{
    public class UnitStateMarkedAsSelected : UnitState
    {
        public UnitStateMarkedAsSelected(Unit unit) : base(unit)
        {
        }

        public override void Apply()
        {
            Unit.MarkAsSelected();
        }

        public override void MakeTransition(UnitState state)
        {
            state.Apply();
            Unit.UnitState = state;
        }
    }
}