using System;
using System.Xml.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leda.Core;
using Leda.Core.Timing;
using Leda.Core.Input;
using Leda.Core.Serialization;
using Leda.Core.Gamestate_Management;
using Leda.Core.Asset_Management;
using Leda.Core.Game_Objects.Behaviours;

#if WINDOWS_PHONE
using Microsoft.Phone.Shell;
using Microsoft.Phone.Controls;
#endif

namespace Leda.Core
{
    public class GameBase : Game
    {
		private static GameBase _instance = null;
        public static GameBase Instance { get { return _instance; } }

        public static Vector2 ScreenPosition(Vector2 worldSpacePosition) { return _instance.ConvertToScreenPosition(worldSpacePosition); }
        public static Vector2 ScreenPosition(float worldSpaceX, float worldSpaceY) { return _instance.ConvertToScreenPosition(worldSpaceX, worldSpaceY); }
        public static Vector2 WorldSpaceClipping { get { return _instance._resolutionOffset * _instance._resolutionScaling; } }
        public static float ScreenScale(float scale) { return _instance.ConvertToScreenScale(scale); }
        public static float ScreenScale() { return _instance.ConvertToScreenScale(1.0f); }
        public static Rectangle SafeDisplayArea { get { return _instance._safeDisplayArea; } }

		public static ScalingAxis DisplayControlAxis
		{
			set
			{
				_instance.SetResolutionMetrics(
					(int)_instance._unscaledBackBufferDimensions.X,
					(int)_instance._unscaledBackBufferDimensions.Y,
					value);
			}
		}

        private Dictionary<Type, Scene> _scenes;
        private Scene _currentScene;
        private string _tombstoneFileName;
        private string _sceneTransitionCrossFadeTextureName;

        public string TombstoneFileName { set { _tombstoneFileName = value; } }
        public int MillisecondsSinceLastUpdate { get { if (_currentScene != null) { return _currentScene.MillisecondsSinceLastUpdate; } else { return 0; } } }

        public string SceneTransitionCrossFadeTextureName
        {
            set
            {
                _sceneTransitionCrossFadeTextureName = value;
                foreach (KeyValuePair<Type, Scene> kvp in _scenes) { kvp.Value.CrossFadeTextureName = _sceneTransitionCrossFadeTextureName; }
            }
        }

        public bool EnsureAllContentIsVisible { get; set; }

#if ANDROID
		public IResumeManager ResumeDisplay { set { ((AndroidGameWindow)Window).SetResumer (value); } }
#endif

		protected Vector2 _unscaledBackBufferDimensions;
        protected Vector2 _resolutionOffset;
        protected float _resolutionScaling;
        protected Rectangle _safeDisplayArea;

		protected Scene CurrentScene { get { return _currentScene; } }

#if IOS
		public External_APIS.iOS.InAppPurchaseManager PurchaseManager { get; private set; }
#endif

		public GameBase(Orientation orientation)
            : base()
        {
            _instance = this;

            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

			if (orientation == Orientation.Portrait) 
			{
				graphics.SupportedOrientations = DisplayOrientation.Portrait;
#if IOS
				graphics.SupportedOrientations |= DisplayOrientation.PortraitDown;
#endif
			} 
            else 
            {
				graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
			}

			graphics.IsFullScreen = true;

            Content.RootDirectory = "Content";

			GlobalTimerController.ClearInstance();
            TouchProcessor.ClearInstance();

            _scenes = new Dictionary<Type, Scene>();
            _currentScene = null;
            _tombstoneFileName = DefaultTombstoneFileName;
            _sceneTransitionCrossFadeTextureName = "";

#if IOS
			PurchaseManager = new Leda.Core.External_APIS.iOS.InAppPurchaseManager();
#endif

#if WINDOWS_PHONE
            PhoneApplicationService.Current.Deactivated += new EventHandler<DeactivatedEventArgs>(HandleGameDeactivated);
            PhoneApplicationService.Current.Closing += new EventHandler<ClosingEventArgs>(HandleGameClosed);
            PhoneApplicationService.Current.Activated += new System.EventHandler<ActivatedEventArgs>(HandleGameActivated);
#endif
        }

        private Vector2 ConvertToScreenPosition(Vector2 worldSpacePosition) { return (worldSpacePosition * _resolutionScaling) + _resolutionOffset; }
        private Vector2 ConvertToScreenPosition(float worldSpaceX, float worldSpaceY) { return (new Vector2(worldSpaceX, worldSpaceY) * _resolutionScaling) + _resolutionOffset; }
        private float ConvertToScreenScale(float scale) { return scale * _resolutionScaling; }

        protected override void Initialize()
        {
			MusicManager.Initialize();
            base.Initialize();
        }

        protected void AddScene(Scene toAdd)
        {
            if (toAdd.DeactivationHandler == null) { toAdd.DeactivationHandler = SceneTransitionHandler; }
            if (toAdd is AssetLoaderScene) { ((AssetLoaderScene)toAdd).LoadCompletionHandler = HandleAssetLoadCompletion; }

            if (!string.IsNullOrEmpty(_sceneTransitionCrossFadeTextureName)) { toAdd.CrossFadeTextureName = _sceneTransitionCrossFadeTextureName; }

            _scenes.Add(toAdd.GetType(), toAdd);
        }

        protected void SetResolutionMetrics(int optimumBackBufferWidth, int optimumBackBufferHeight, ScalingAxis scalingAxis)
        {
            _unscaledBackBufferDimensions = new Vector2(optimumBackBufferWidth, optimumBackBufferHeight);

            if (scalingAxis == ScalingAxis.X)
            {
                _resolutionScaling = (float)GraphicsDevice.Viewport.Width / (float)optimumBackBufferWidth;
                _resolutionOffset = new Vector2(0.0f, ((float)GraphicsDevice.Viewport.Height - (optimumBackBufferHeight * _resolutionScaling)) / 2.0f);
            }
            else
            {
                _resolutionScaling = (float)GraphicsDevice.Viewport.Height / (float)optimumBackBufferHeight;
                _resolutionOffset = new Vector2(((float)GraphicsDevice.Viewport.Width - (optimumBackBufferWidth * _resolutionScaling)) / 2.0f, 0.0f);
            }

            TouchProcessor.ResolutionScaling = _resolutionScaling;
            TouchProcessor.ResolutionOffset = _resolutionOffset;

            _safeDisplayArea = new Rectangle(
                (int)Math.Max(-(_resolutionOffset.X / _resolutionScaling), 0),
                (int)Math.Max(-(_resolutionOffset.Y / _resolutionScaling), 0),
                (int)Math.Min(optimumBackBufferWidth + ((_resolutionOffset.X / _resolutionScaling) * 2.0f), optimumBackBufferWidth),
                (int)Math.Min(optimumBackBufferHeight + ((_resolutionOffset.Y / _resolutionScaling) * 2.0f), optimumBackBufferHeight));
        }

        protected void StartInitialScene(Type startingSceneType)
        {
            if ((_currentScene == null) && (_scenes.ContainsKey(startingSceneType)))
            {
                if (FileManager.FileExists(_tombstoneFileName)) { startingSceneType = HandleTombstoneRecovery(startingSceneType); }
                SceneTransitionHandler(startingSceneType); 
            }
        }

        private void SceneTransitionHandler(Type nextSceneType)
        {
            if (_scenes.ContainsKey(nextSceneType)) 
            {
                _currentScene = _scenes[nextSceneType];
                _scenes[nextSceneType].Activate(); 
            }
        }

        private void HandleAssetLoadCompletion(Type loaderSceneType)
        {
            foreach (KeyValuePair<Type, Scene> kvp in _scenes) { kvp.Value.HandleAssetLoadCompletion(loaderSceneType); }
        }

#if WINDOWS_PHONE
        protected void HandleGameDeactivated(object sender, DeactivatedEventArgs e)
        {
            HandleTombstoneEvent();
        }

        protected void HandleGameClosed(object sender, ClosingEventArgs e)
        {
            HandleTombstoneEvent();
        }

        protected void HandleGameActivated(object sender, ActivatedEventArgs e)
        {
            if ((e.IsApplicationInstancePreserved) && (_currentScene != null)) 
            {
                FileManager.DeleteFile(_tombstoneFileName);
                _currentScene.HandleFastResume(); 
            }
        }

        protected void HandleGameObscured(object sender, ObscuredEventArgs e)
        {
            _currentScene.HandleGameObscured();
            MusicManager.HandleGameObscured();
            SoundEffectManager.HandleGameObscured();
        }

        protected void HandleGameUnobscured(object sender, EventArgs e)
        {
            _currentScene.HandleGameUnobscured();
            MusicManager.HandleGameUnobscured();
            SoundEffectManager.HandleGameUnobscured();
        }

#endif

#if ANDROID

		public void HandleActivityPauseEvent()
		{
			MusicManager.StopMusic();
			HandleTombstoneEvent();
		}

		public void HandleActivityResumeEvent()
		{
			if ((FileManager.FileExists(_tombstoneFileName)) && (_currentScene != null))
			{
				FileManager.DeleteFile(_tombstoneFileName);
				_currentScene.HandleFastResume();
			}
		}

#endif

#if IOS
		public void HandleGameActivatedEvent()
		{
			if (_currentScene != null)
			{
				_currentScene.HandleGameActivated();
			}
		}

		public void HandleGameResignedEvent()
		{
			_currentScene.HandleGameResigned();
		}

		public void HandleGameResumedEvent()
		{
			_currentScene.HandleGameReturnedToForeground ();
		}

		public void HandleGameBackgroundEvent()
		{
			_currentScene.HandleGameSentToBackground ();
		}

#endif

        private void HandleTombstoneEvent()
        {
            if (_currentScene != null)
            {
                _currentScene.HandleTombstone();

                if (_currentScene is ISerializable)
                {
                    XDocument stateData = new XDocument(new XDeclaration("1.0", "utf", "yes"));
                    stateData.Add(new XElement("gamestatedata"));

                    stateData.Element("gamestatedata").Add(((ISerializable)_currentScene).Serialize());

                    FileManager.SaveXMLFile(_tombstoneFileName, stateData);
                }
            }
        }

        public Type HandleTombstoneRecovery(Type startingSceneType)
        {
            XDocument stateData = FileManager.LoadXMLFile(_tombstoneFileName);
            Serializer serializer = new Serializer(stateData.Element("gamestatedata").Element("object"));
            string recoveringSceneID = stateData.Element("gamestatedata").Element("object").Attribute("id").Value;
            string recoveringSceneTypeName = stateData.Element("gamestatedata").Element("object").Attribute("type").Value;
            string recoveringSceneNextSceneTypeName = "";

            if (serializer.GetDataItem<Scene.Status>("state") == Scene.Status.Deactivating)
            {
                recoveringSceneNextSceneTypeName = serializer.GetDataItem<string>("nextscene");
            }

            Scene startingScene = null;
            Scene sceneToRecover = null;
            Type recoveringSceneType = null;

            foreach (KeyValuePair<Type, Scene> kvp in _scenes)
            {
                if ((kvp.Key.ToString() == recoveringSceneTypeName) && (kvp.Value is StorableScene) && (((StorableScene)kvp.Value).ID == recoveringSceneID)) 
                { 
                    sceneToRecover = kvp.Value; 
                    recoveringSceneType = kvp.Key;
                }
                else if (kvp.Key == startingSceneType) 
                { 
                    startingScene = kvp.Value; 
                }
            }

            if (!string.IsNullOrEmpty(recoveringSceneNextSceneTypeName))
            {
                recoveringSceneType = Type.GetType(recoveringSceneNextSceneTypeName);
            }
            else if (sceneToRecover != null) 
            { 
                ((StorableScene)sceneToRecover).TombstoneRecoveryData = stateData.Element("gamestatedata").Element("object"); 
            }

            if ((startingScene != null) && (startingScene is AssetLoaderScene) && (recoveringSceneType != null))
            {
                ((AssetLoaderScene)startingScene).PostLoadTargetSceneType = recoveringSceneType;
            }
            else
            {
                startingSceneType = recoveringSceneType;
            }

            FileManager.DeleteFile(_tombstoneFileName);

            return startingSceneType;
        }

		public enum ScalingAxis
		{
			X,
			Y
		}

        private const string DefaultTombstoneFileName = "tombstonedata.xml";

    }
}
