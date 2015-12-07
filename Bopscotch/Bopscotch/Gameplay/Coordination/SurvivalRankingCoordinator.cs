using Bopscotch.Data;

namespace Bopscotch.Gameplay.Coordination
{
    public class SurvivalRankingCoordinator
    {
        public delegate void CompleteAwardSequenceCallback();

        private CompleteAwardSequenceCallback _handleSequenceComplete;

        public SurvivalRankingCoordinator(CompleteAwardSequenceCallback completionCallback)
        {
            _handleSequenceComplete = completionCallback;
        }

        public void DisplayRanking(SurvivalLevelData levelData)
        {
            _handleSequenceComplete();
        }
    }
}
