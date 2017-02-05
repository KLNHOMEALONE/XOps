using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using XOps.Core;

namespace XOps.Example
{
    public class MySquare : Square
    {
        public override Vector3 GetCellDimensions(Entity entity)
        {
            var model = entity.Get<ModelComponent>().Model;
            return new Vector3(model.BoundingBox.Maximum.X - model.BoundingBox.Minimum.X, model.BoundingBox.Maximum.Y - model.BoundingBox.Minimum.Y, model.BoundingBox.Maximum.Z - model.BoundingBox.Minimum.Z);
            //var en = entity.Get<Material>()
        }

        public override void MarkAsReachable()
        {
            //ChangeMaterial(ReachableMaterial);
            SetColor(new Color(1, 0.92f, 0.16f, 0.5f));
        }

        public override void MarkAsPath()
        {
            //ChangeMaterial(PathMaterial);
           SetColor(new Color(0, 1, 0, 0.5f));
        }
        public override void MarkAsHighlighted()
        {
            //ChangeMaterial(HighlightedMaterial);
            SetColor(new Color(0.8f, 0.8f, 0.8f, 0.5f));
        }
        public override void UnMark()
        {
            //ChangeMaterial(DefaultMaterial);
            SetColor(new Color(1, 1, 1, 0));
        }

        private void ChangeMaterial(Material material)
        {
            var modelComponent = Entity?.Get<ModelComponent>();
            if (modelComponent != null)
            {
                var materialCount = modelComponent.Materials.Count;
                modelComponent.Materials.Clear();
                for (int i = 0; i < materialCount; i++)
                {
                    modelComponent.Materials.Add(i, material);
                }
            }
        }
        private void SetColor(Color color)
        {
            var sprite = Entity.GetChild(0).Get<SpriteComponent>();
            if (sprite != null)
            {
                sprite.Color = color;
            }

            //var highlighter = transform.FindChild("Highlighter");
            //var spriteRenderer = highlighter.GetComponent<SpriteRenderer>();
            //if (spriteRenderer != null)
            //{
            //    spriteRenderer.color = color;
            //}
        }

        public override void Update()
        {
            //throw new NotImplementedException();
        }
    }
}
