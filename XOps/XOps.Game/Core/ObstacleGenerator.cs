using System;
using System.Collections;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using System.Linq;

namespace XOps.Core
{
    public class ObstacleGenerator : SyncScript
    {
        private readonly System.Random _rnd = new System.Random();
        public Prefab ObstaclePrefab { get; set; }

        public int NumberofObstacles { get; set; }

        public List<Cell> Cells { get; set; } 

        public override void Start()
        {
            Cells = Entity.Get<CellGridGenerator>().Cells;
            SpawnObstacles();
        }

        public override void Update()
        {
            
        }

        public void SpawnObstacles()
        {
            List<Cell> freeCells = Cells.FindAll(h => h.IsTaken == false);
            freeCells = freeCells.OrderBy(h => _rnd.Next()).ToList();

            for (int i = 0; i < NumberofObstacles; i++)
            {
                var cell = freeCells.ElementAt(0);
                freeCells.RemoveAt(0);
                cell.IsTaken = true;
                var obstacle = ObstaclePrefab.Instantiate().First();

                //var cell = Cells.OrderBy(c => Math.Abs((c.Entity.Transform.Position - obstacle.Transform.Position).Length())). First(e => e.IsTaken == false);
                //cell.IsTaken = true;
                Vector3 offset = new Vector3(0, 0.5f, 0);
                obstacle.Transform.Position = cell.Entity.Transform.Position + offset;
                SceneSystem.SceneInstance.Scene.Entities.Add(obstacle);
            }
        }
    }
}