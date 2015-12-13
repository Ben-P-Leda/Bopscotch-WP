using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Asset_Management;

using Bopscotch.Data;
using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Interface;
using Bopscotch.Interface.Dialogs.RankingScene;
using Bopscotch.Interface.Content;

namespace Bopscotch.Scenes.NonGame
{
    public class RankingScene : ContentSceneWithControlDialog
    {
        private string _selectedArea;
        private NavigationDialog _navigationDialog;

        public RankingScene()
            : base()
        {
            _selectedArea = "Hilltops";

            BackgroundTextureName = Background_Texture_Name;

            Overlay.Tint = Color.Black;
            Overlay.TintFraction = 0.0f;
            Overlay.RenderLayer = 4;
            Overlay.RenderDepth = 0.99999f;
        }

        public override void Initialize()
        {
            _navigationDialog = new NavigationDialog();
            _navigationDialog.SelectionCallback = HandleNavigationDialogDismiss;
            _navigationDialog.PageChangeCallback = HandlePageChange;

            Dialog = _navigationDialog;

            base.Initialize();
        }

        private void HandleNavigationDialogDismiss(string buttonCaption)
        {
            NextSceneType = typeof(TitleScene);
            NextSceneParameters.Set("music-already-running", true);

            Deactivate();
        }

        private void HandlePageChange(string areaName)
        {
            _selectedArea = areaName;
            CreateContentForSelectedArea();
            _navigationDialog.PagingComplete = true;
        }

        public override void Activate()
        {
            base.Activate();

            CreateContentForSelectedArea();
        }

        private void CreateContentForSelectedArea()
        {
            FlushContent();

            CreateTextElement(_selectedArea, new Vector2(Definitions.Back_Buffer_Center.X, Title_Y_Position), TextWriter.Alignment.Center, 1.0f);

            AreaDataContainer areaData = Profile.GetDataForNamedArea(_selectedArea);
            if (areaData != null)
            {
                _background.TextureReference = areaData.SelectionTexture;

                if (areaData.Locked)
                {
                    CreateContentForLockedArea();
                }
                else
                {
                    CreateRankingContent(areaData.LevelScores, areaData.LevelRanks);
                }
            }
        }

        private void CreateContentForLockedArea()
        {
            Rectangle iconFrame = TextureManager.Textures["icon-locked"].Bounds;
            Vector2 iconOrigin = new Vector2(iconFrame.Width, iconFrame.Height) / 2.0f;
            ImageContent icon = CreateImageElement("icon-locked", Definitions.Back_Buffer_Center, iconFrame, iconOrigin, 5.0f);
            icon.RenderDepth = 0.6f;

            CreateTextElement(Translator.Translation("Locked"), Definitions.Back_Buffer_Center, TextWriter.Alignment.Center, 1.0f);
        }

        private void CreateRankingContent(List<int> scores, List<Definitions.SurvivalRank> ranks)
        {
            float leftSide = Definitions.Back_Buffer_Center.X - (Definitions.Back_Buffer_Center.X * 0.8f);
            float rightSide = Definitions.Back_Buffer_Center.X + (Definitions.Back_Buffer_Center.X * 0.2f);
            int cumulativeScore = 0;

            for (int i=0; i < ranks.Count; i++)
            {
                float x = i % 2 == 1 ? rightSide : leftSide;
                float y = ((i / 2) * Table_Line_Height) + Table_Top_Y;

                CreateTextElement((i + 1).ToString() + ":" , new Vector2(x, y), TextWriter.Alignment.Right, 0.6f);

                if (scores.Count > i)
                {
                    cumulativeScore += scores[i];
                    CreateTextElement(cumulativeScore.ToString(), new Vector2(x, y), TextWriter.Alignment.Left, 0.6f);

                    if (ranks[i] != Definitions.SurvivalRank.NotSet)
                    {
                        Rectangle rankFrame = new Rectangle(160 * (int)ranks[i], 0, 160, 200);
                        CreateImageElement("ranking-letters", new Vector2(x + Rank_Offset, y + Vertical_Offset), rankFrame, Vector2.Zero, 0.2f);

                        Rectangle starFrame = new Rectangle(0, 0, 120 * (3 - (int)ranks[i]), 120);
                        CreateImageElement("ranking-stars", new Vector2(x + Stars_Offset, y + Vertical_Offset), starFrame, Vector2.Zero, 0.3f);
                    }
                    else
                    {
                        CreateTextElement(Translator.Translation("(unranked)"), new Vector2(x + Rank_Offset, y), TextWriter.Alignment.Left, 0.6f);
                    }
                }
            }
        }

        private const string Background_Texture_Name = "background-4";
        private const float Title_Y_Position = 10.0f;
        private const float Table_Top_Y = 90.0f;
        private const float Table_Line_Height = 40.0f;
        private const float Rank_Offset = 300.0f;
        private const float Stars_Offset = 400.0f;
        private const float Vertical_Offset = 12.5f;
    }
}
