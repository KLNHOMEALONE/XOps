namespace XOps.Core
{
    public class UnitStateMarkedAsFinished : UnitState
    {
        public UnitStateMarkedAsFinished(Unit unit) : base(unit)
        {
        }

        public override void Apply()
        {
            Unit.MarkAsFinished();
        }

        public override void MakeTransition(UnitState state)
        {
            if (state is UnitStateNormal)
            {
                state.Apply();
                Unit.UnitState = state;
            }
        }
    }
}