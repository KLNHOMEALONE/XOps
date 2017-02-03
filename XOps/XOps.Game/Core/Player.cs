using SiliconStudio.Xenko.Engine;

namespace XOps.Core
{
    public abstract class Player : SyncScript
    {
        public int PlayerNumber;
        /// <summary>
        /// Method is called every turn. Allows player to interact with his units.
        /// </summary>         
        public abstract void Play(CellGrid cellGrid);
    }
}