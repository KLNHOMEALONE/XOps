namespace XOps.Core
{
    public abstract class UnitState
    {
        protected Unit Unit;

        protected UnitState(Unit unit)
        {
            Unit = unit;
        }

        public abstract void Apply();
        public abstract void MakeTransition(UnitState state);
    }
}