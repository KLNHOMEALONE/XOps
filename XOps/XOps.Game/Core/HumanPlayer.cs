namespace XOps.Core
{
    public class HumanPlayer : Player
    {
        public override void Update()
        {
            
        }

        public override void Play(CellGrid cellGrid)
        {
            cellGrid.CellGridState = new CellGridStateWaitingForInput(cellGrid);
        }
    }
}