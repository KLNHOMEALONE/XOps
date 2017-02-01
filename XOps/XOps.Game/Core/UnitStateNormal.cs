namespace XOps.Core
{
    public class UnitStateNormal : UnitState
    {
        public UnitStateNormal(Unit unit) : base(unit)
        {
        }

        public override void Apply()
        {
            Unit.UnMark();
        }

        public override void MakeTransition(UnitState state)
        {
            state.Apply();
            Unit.UnitState = state;
        }
    }
}