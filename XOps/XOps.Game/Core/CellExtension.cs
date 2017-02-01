using SiliconStudio.Core.Mathematics;

namespace XOps.Core
{
    public static class CellExtension
    {
        public static Vector3 GetPosition(this Cell value)
        {
            return value.Entity.Transform.Position;
        }
    }
}