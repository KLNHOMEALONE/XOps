using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using XOps.Core;

namespace XOps.Example
{
    public class MySquare : Square
    {
        public override Vector3 GetCellDimensions(Entity entity)
        {
            var model = entity.Get<ModelComponent>().Model;
            return new Vector3(model.BoundingBox.Maximum.X - model.BoundingBox.Minimum.X, model.BoundingBox.Maximum.Y - model.BoundingBox.Minimum.Y, model.BoundingBox.Maximum.Z - model.BoundingBox.Minimum.Z);
        }


        public override void MarkAsHighlighted()
        {
            //throw new NotImplementedException();
        }

        public override void MarkAsPath()
        {
            //throw new NotImplementedException();
        }

        public override void MarkAsReachable()
        {
            //throw new NotImplementedException();
        }

        public override void UnMark()
        {
            //throw new NotImplementedException();
        }

        public override void Update()
        {
            //throw new NotImplementedException();
        }
    }
}
