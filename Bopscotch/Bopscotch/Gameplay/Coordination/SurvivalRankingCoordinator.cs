using Microsoft.Xna.Framework;

using Leda.Core.Gamestate_Management;

using Bopscotch.Data;
using Bopscotch.Effects.Popups;

namespace Bopscotch.Gameplay.Coordination
{
    public class SurvivalRankingCoordinator
    {
        public delegate void RankSequenceCallback();

        private RankSequenceCallback _handleSequenceComplete;
        private RankingStar[] _rankingStars;

        public SurvivalRankingCoordinator(RankSequenceCallback completionCallback)
        {
            _handleSequenceComplete = completionCallback;

            _rankingStars = new RankingStar[Ranking_Star_Count];
            for (int i = 0; i < Ranking_Star_Count; i++)
            {
                _rankingStars[i] = new RankingStar();
            }
        }

        public void InitializeAwardDisplay()
        {
            for (int i = 0; i < Ranking_Star_Count; i++)
            {
                _rankingStars[i].Initialize();
                _rankingStars[i].FrameOffset = i;
            }
        }

        public void RegisterDisplayComponents(Scene.ObjectRegistrationHandler registerObject)
        {
            for (int i = 0; i < Ranking_Star_Count; i++)
            {
                _rankingStars[i].Reset();
                registerObject(_rankingStars[i]);
            }
        }

        public void DisplayRanking(SurvivalLevelData levelData)
        {
            int rank = CalculateRank(levelData.CandyCollectionFraction, levelData.AttemptsAtLevel);

            if (rank < 1)
            {
                _handleSequenceComplete();
            }
            else
            {

                switch (rank)
                {
                    case 1: DisplayForRankOne(); break;
                    case 2: DisplayForRankTwo(); break;
                    case 3: DisplayForRankThree(); break;
                }

                _rankingStars[0].Activate();
            }
        }

        private void DisplayForRankOne()
        {
            _rankingStars[0].DisplayPosition = Definitions.Back_Buffer_Center;
            _rankingStars[0].NextAction = _handleSequenceComplete;
        }

        private void DisplayForRankTwo()
        {
            _rankingStars[1].DisplayPosition = Definitions.Back_Buffer_Center + new Vector2(Definitions.Grid_Cell_Pixel_Size, Display_Line);
            _rankingStars[1].NextAction = _handleSequenceComplete;

            _rankingStars[0].DisplayPosition = Definitions.Back_Buffer_Center + new Vector2(-Definitions.Grid_Cell_Pixel_Size, Display_Line);
            _rankingStars[0].NextAction = _rankingStars[1].Activate;
        }

        private void DisplayForRankThree()
        {
            _rankingStars[2].DisplayPosition = Definitions.Back_Buffer_Center + new Vector2(0.0f, Display_Line - (Definitions.Grid_Cell_Pixel_Size * 0.5f));
            _rankingStars[2].NextAction = _handleSequenceComplete;

            _rankingStars[1].DisplayPosition = Definitions.Back_Buffer_Center + new Vector2(Definitions.Grid_Cell_Pixel_Size * 2.0f, Display_Line);
            _rankingStars[1].NextAction = _rankingStars[2].Activate;

            _rankingStars[0].DisplayPosition = Definitions.Back_Buffer_Center + new Vector2(-Definitions.Grid_Cell_Pixel_Size * 2.0f, Display_Line);
            _rankingStars[0].NextAction = _rankingStars[1].Activate;
        }

        private int CalculateRank(float candyCollectionFraction, int livesUsed)
        {
            if ((candyCollectionFraction >= Rank_Three_Candy_Fraction) && (livesUsed < Rank_Three_Lives_Used)) { return 3; }
            else if ((candyCollectionFraction >= Rank_Two_Candy_Fraction) || (livesUsed < Rank_Two_Lives_Used)) { return 2; }
            else if ((candyCollectionFraction >= Rank_One_Candy_Fraction) || (livesUsed < Rank_One_Lives_Used)) { return 1; }

            return 0;
        }

        private const int Ranking_Star_Count = 3;
        private const float Rank_One_Candy_Fraction = 0.7f;
        private const float Rank_Two_Candy_Fraction = 0.775f;
        private const float Rank_Three_Candy_Fraction = 0.85f;
        private const float Rank_One_Lives_Used = 10;
        private const float Rank_Two_Lives_Used = 4;
        private const float Rank_Three_Lives_Used = 1;
        private const float Display_Line = -100.0f;
    }
}
