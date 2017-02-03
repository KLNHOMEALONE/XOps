using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Core;
using System.Linq;

namespace XOps.Core
{
    public class RectangularSquareGridGenerator : CellGridGenerator
    {
        [DataMember(20)]
        [Display("SquarePrefab")]
        public Prefab SquarePrefab { get; set; }

        public override void Update()
        {
        }

        public override void Start()
        {
            base.Start();
            Cells = GenerateGrid();
        }

        public override List<Cell> GenerateGrid()
        {
            if (SquarePrefab == null) return null;
            var position = Entity.Transform.Position;
            var ret = new List<Cell>();
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    var prefabEntity = SquarePrefab.Instantiate().First();
                    var cell = prefabEntity.Get<Cell>();
                    var squareSize = cell.GetCellDimensions(prefabEntity);
                    cell.CellSize = squareSize;
                    cell.OffsetCoord = new Vector2(i, j);
                    cell.MovementCost = 1;
                    prefabEntity.Transform.Position = new Vector3(i * squareSize.X + position.X, 0.5f + position.Y, j * squareSize.Z + position.Z);
                    SceneSystem.SceneInstance.Scene.Entities.Add(prefabEntity);
                    ret.Add(cell);

                    //square.transform.parent = CellsParent;
                }
            }
            return ret;
        }
    }
}