using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Core;
using System.Linq;
using SiliconStudio.Xenko.Physics;
using XOps.Example;

namespace XOps.Core
{
    public class RectangularSquareGridGenerator : CellGridGenerator
    {
        [DataMember(20)]
        [Display("SquarePrefab")]
        public Prefab SquarePrefab { get; set; }

        [DataMember(20)]
        [Display("Square Model")]
        public Model SquareModel { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

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
            var material = Content.Load<Material>("Materials/Ground Material");
            //var squareSize = GetDimensions();

            //var shape = Content.Load<ColliderShapeAssetDesc>("TileCollider");
            var ret = new List<Cell>();
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Entity prefabEntity = SquarePrefab.Instantiate().First();
                    var cell = prefabEntity.Get<Cell>();
                    var squareSize = cell.GetCellDimensions(prefabEntity);
                    cell.OffsetCoord = new Vector2(i, j);
                    cell.MovementCost = 1;
                    cell.UnMark();
                    var model = prefabEntity.Get<ModelComponent>();
                    model.Materials.Add(0, material);
                    prefabEntity.Transform.Position = new Vector3(i * squareSize.X + position.X, 0.5f + position.Y, j * squareSize.Z + position.Z);
                    SceneSystem.SceneInstance.Scene.Entities.Add(prefabEntity);
                    ret.Add(cell);

                    //var tileEntity = new Entity(new Vector3(i * squareSize.X + position.X, 0.5f + position.Y, j * squareSize.Z + position.Z), $"Tile_{i}");
                    //var modelComponent = new ModelComponent(SquareModel);
                    //var material = Content.Load<Material>("Materials/Ground Material");
                    //modelComponent.Materials.Add(0, material);
                    //tileEntity.Add(modelComponent);
                    //var cell = new MySquare
                    //{
                    //    OffsetCoord = new Vector2(i, j),
                    //    MovementCost = 1,
                    //    DefaultMaterial = Content.Load<Material>("Materials/Ground Material"),
                    //    ReachableMaterial = Content.Load<Material>("Materials/Sphere Material"),
                    //    PathMaterial = Content.Load<Material>("Materials/GreenMaterial"),
                    //    HighlightedMaterial = Content.Load<Material>("Materials/HighlightedMaterial")
                    //};
                    //tileEntity.Add(cell);
                    //var collider = new StaticColliderComponent
                    //{
                    //    CollisionGroup = CollisionFilterGroups.CustomFilter1,
                    //    CanSleep = true,
                    //    Friction = 0.5f
                    //};
                    //collider.ColliderShapes.Add(new StaticPlaneColliderShapeDesc());
                    //tileEntity.Add(collider);
                    //SceneSystem.SceneInstance.Scene.Entities.Add(tileEntity);
                    //ret.Add(cell);
                }
            }
            return ret;
        }

        private Vector3 GetDimensions()
        {
            var model = SquareModel;
            return new Vector3(model.BoundingBox.Maximum.X - model.BoundingBox.Minimum.X, model.BoundingBox.Maximum.Y - model.BoundingBox.Minimum.Y, model.BoundingBox.Maximum.Z - model.BoundingBox.Minimum.Z);
        }
    }
}