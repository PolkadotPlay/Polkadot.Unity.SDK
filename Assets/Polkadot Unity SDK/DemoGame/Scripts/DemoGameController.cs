using Assets.Scripts.ScreenStates;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    public enum DemoGameScreen
    {
        StartScreen,
        MainScreen,
        PlayScreen,
    }

    public enum DemoGameSubScreen
    {
        MainChoose,
        MainInvite,
        Play,
        PlaySelect,
        PlayTileSelect,
        PlayTileUpgrade,
        PlayNextTurn,
        PlayFinish,
        PlayWaiting,
        PlayRanking,
        PlayTarget,
    }

    public class DemoGameController : ScreenStateMachine<DemoGameScreen, DemoGameSubScreen>
    {
        internal NetworkManager Network => NetworkManager.GetInstance();
        internal StorageManager Storage => StorageManager.GetInstance();

        internal readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();

        public Vector2 ScrollOffset { get; set; }

        public CacheData CacheData { get; private set; }

        public VisualElement VelContainer { get; private set; }

        private new void Awake()
        {
            base.Awake();
            //Your code goes here

            CacheData = new CacheData();
        }

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            VelContainer = root.Q<VisualElement>("VelContainer");

            VelContainer.RemoveAt(1);

            if (VelContainer.childCount > 1)
            {
                Debug.Log("Plaese remove development work, before starting!");
                return;
            }

            // call insital flow state
            ChangeScreenState(DemoGameScreen.StartScreen);
        }

        protected override void InitializeStates()
        {
            _stateDictionary.Add(DemoGameScreen.StartScreen, new StartScreen(this));

            var mainScreen = new MainScreenState(this);
            _stateDictionary.Add(DemoGameScreen.MainScreen, mainScreen);

            var mainScreenSubStates = new Dictionary<DemoGameSubScreen, IScreenState>
            {
                { DemoGameSubScreen.MainChoose, new MainChooseSubState(this, mainScreen) },
                { DemoGameSubScreen.MainInvite, new MainInviteSubState(this, mainScreen) },
                { DemoGameSubScreen.Play, new MainPlaySubState(this, mainScreen) },
            };
            _subStateDictionary.Add(DemoGameScreen.MainScreen, mainScreenSubStates);

            var playScreen = new PlayScreenState(this);
            _stateDictionary.Add(DemoGameScreen.PlayScreen, playScreen);

            var playScreenSubStates = new Dictionary<DemoGameSubScreen, IScreenState>
            {
                { DemoGameSubScreen.PlaySelect, new PlaySelectSubState(this, playScreen) },
                { DemoGameSubScreen.PlayTileSelect, new PlayTileSelectSubState(this, playScreen) },
                { DemoGameSubScreen.PlayTileUpgrade, new PlayTileUpgradeSubState(this, playScreen) },
                { DemoGameSubScreen.PlayNextTurn, new PlayNextTurnSubState(this, playScreen) },
                { DemoGameSubScreen.PlayFinish, new PlayFinishSubState(this, playScreen) },
                { DemoGameSubScreen.PlayWaiting, new PlayWaitingSubState(this, playScreen) },
                { DemoGameSubScreen.PlayRanking, new PlayRankingSubState(this, playScreen) },
                { DemoGameSubScreen.PlayTarget, new PlayTargetSubState(this, playScreen) },
            };
            _subStateDictionary.Add(DemoGameScreen.PlayScreen, playScreenSubStates);
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        private void Update()
        {
            // Method intentionally left empty.
        }
    }
}