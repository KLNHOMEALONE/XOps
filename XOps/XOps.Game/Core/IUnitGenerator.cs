using System.Collections.Generic;

namespace XOps.Core
{
    public interface IUnitGenerator
    {
        List<Unit> SpawnUnits(List<Cell> cells);
    }
}