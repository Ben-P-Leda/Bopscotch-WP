using System.Xml;
using System.Xml.Linq;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Asset_Management;

using Bopscotch.Scenes.BaseClasses;
using Bopscotch.Interface;
using Bopscotch.Interface.Content;
using Bopscotch.Interface.Dialogs;

namespace Bopscotch.Scenes.NonGame
{
    public class CreditsScene : ContentSceneWithControlDialog
    {
        public CreditsScene()
            : base()
        {
            BackgroundTextureName = Background_Texture_Name;
        }

        public override void Initialize()
        {
            BackDialog backDialog = new BackDialog();
            backDialog.SelectionCallback = HandleBackDialogDismiss;

            Dialog = backDialog;

            base.Initialize();

            CreateContent(Credits_Content_Elements_File);
        }

        protected void CreateContent(string fileName)
        {
            XDocument content = FileManager.LoadXMLContentFile(string.Format(fileName, Translator.CultureCode));

            foreach (XElement element in content.Element("elements").Elements("element"))
            {
                switch (element.Attribute("type").Value)
                {
                    case "text": CreateTextElementFromXml(element); break;
                    case "image": CreateImageElementFromXml(element); break;
                }
            }
        }

        private TextContent CreateTextElementFromXml(XElement source)
        {
            TextContent element = new TextContent(
                Translator.Translation(source.Value.Trim()),
                new Vector2((float)source.Attribute("x-position"), (float)source.Attribute("y-position")));

            element.RenderDepth = Element_Render_Depth;

            if (source.Attribute("scale") != null) { element.Scale = (float)source.Attribute("scale"); }
            if (source.Attribute("alignment") != null)
            {
                switch (source.Attribute("alignment").Value)
                {
                    case "left": element.Alignment = TextWriter.Alignment.Left; break;
                    case "right": element.Alignment = TextWriter.Alignment.Right; break;
                }
            }

            RegisterGameObject(element);

            return element;
        }

        private ImageContent CreateImageElementFromXml(XElement source)
        {
            ImageContent element = new ImageContent(source.Value.Trim(), new Vector2((float)source.Attribute("x-position"), (float)source.Attribute("y-position")));

            if (source.Attribute("scale") != null) { element.Scale = (float)source.Attribute("scale"); }

            if (source.Element("frame") != null)
            {
                element.Frame = new Rectangle((int)source.Element("frame").Attribute("x"), (int)source.Element("frame").Attribute("y"),
                    (int)source.Element("frame").Attribute("width"), (int)source.Element("frame").Attribute("height"));
            }

            if (source.Element("origin") != null)
            {
                element.Origin = new Vector2((float)source.Element("origin").Attribute("x"), (float)source.Element("origin").Attribute("y"));
            }

            element.RenderDepth = Element_Render_Depth;

            RegisterGameObject(element);

            return element;
        }

        private void HandleBackDialogDismiss(string buttonCaption)
        {
            NextSceneType = typeof(TitleScene);

            if (buttonCaption == "Rate") { NextSceneParameters.Set(TitleScene.First_Dialog_Parameter_Name, TitleScene.Rate_Game_Dialog); }
            
            NextSceneParameters.Set("music-already-running", true);

            Deactivate();
        }

        private const string Background_Texture_Name = "background-4";
        private const string Credits_Content_Elements_File = "Content/Files/Content/{0}/credits.xml";
    }
}
