using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace XOps.Core
{
    public class RandomUnitGenerator : SyncScript, IUnitGenerator
    {
        private readonly System.Random _rnd = new System.Random();

        public Prefab UnitPrefab { get; set; }
        public int NumberOfPlayers { get; set; }
        public int UnitsPerPlayer { get; set; }
        public override void Update()
        {

        }

        public List<Unit> SpawnUnits(List<Cell> cells)
        {
            List<Unit> ret = new List<Unit>();

            List<Cell> freeCells = cells.FindAll(h => h.IsTaken == false);
            freeCells = freeCells.OrderBy(h => _rnd.Next()).ToList();

            for (int i = 0; i < NumberOfPlayers; i++)
            {
                for (int j = 0; j < UnitsPerPlayer; j++)
                {
                    var cell = freeCells.ElementAt(0);
                    freeCells.RemoveAt(0);
                    cell.IsTaken = true;

                    var unitEntity = UnitPrefab.Instantiate().First();
                    var unit = unitEntity.Get<Unit>();
                    unitEntity.Transform.Position = cell.GetPosition() + new Vector3(0, 0, 0);
                    unit.PlayerNumber = i;
                    unit.Cell = cell;
                    unit.Initialize();
                    //unit.Transform.parent = UnitsParent;
                    ret.Add(unitEntity.Get<Unit>());
                }
            }
            return ret;
        }
    }
}