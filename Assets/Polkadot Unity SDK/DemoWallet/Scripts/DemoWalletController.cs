using Assets.Scripts.ScreenStates;
using Substrate.NET.Wallet;
using Substrate.NetApi.Model.Types;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    public enum DemoWalletScreen
    {
        UnlockWallet,
        ResetWallet,
        LoadScreen,
        OnBoarding,
        CreateWallet,
        SetPassword,
        VerifyPassword,
        ImportSeed,
        ImportJson,
        AccountSelection,
        MainScreen
    }

    public enum DemoWalletSubScreen
    {
        Dashboard,
    }

    public class DemoWalletController : ScreenStateMachine<DemoWalletScreen, DemoWalletSubScreen>
    {
        internal NetworkWalletManager Network => NetworkWalletManager.GetInstance();

        internal readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();

        public VisualElement VelContainer { get; private set; }

        internal Account TempAccount { get; set; }
        internal FileStore TempFileStore { get; set; }

        internal string TempAccountName { get; set; } // TODO: remove this ....
        internal string TempAccountPassword { get; set; } // TODO: remove this ....
        internal string TempMnemonic { get; set; }  // TODO: remove this ....

        private new void Awake()
        {
            base.Awake();
            //Your code goes here
        }

        protected override void InitializeStates()
        {
            // Initialize states
            _stateDictionary.Add(DemoWalletScreen.UnlockWallet, new UnlockWalletState(this));
            _stateDictionary.Add(DemoWalletScreen.AccountSelection, new AccountSelectionState(this));
            _stateDictionary.Add(DemoWalletScreen.ResetWallet, new ResetWalletState(this));
            _stateDictionary.Add(DemoWalletScreen.OnBoarding, new OnBoardingState(this));
            _stateDictionary.Add(DemoWalletScreen.CreateWallet, new CreateWalletState(this));
            _stateDictionary.Add(DemoWalletScreen.SetPassword, new SetPasswordState(this));
            _stateDictionary.Add(DemoWalletScreen.VerifyPassword, new VerifyPasswordState(this));
            _stateDictionary.Add(DemoWalletScreen.ImportJson, new ImportJsonState(this));
            _stateDictionary.Add(DemoWalletScreen.ImportSeed, new ImportSeedState(this));
            _stateDictionary.Add(DemoWalletScreen.LoadScreen, new LoadScreenState(this));
            var mainScreen = new WalletScreenState(this);
            _stateDictionary.Add(DemoWalletScreen.MainScreen, mainScreen);

            var mainScreenSubStates = new Dictionary<DemoWalletSubScreen, IScreenState>
            {
                { DemoWalletSubScreen.Dashboard, new MainDashboardSubState(this, mainScreen) },
            };

            _subStateDictionary.Add(DemoWalletScreen.MainScreen, mainScreenSubStates);
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

            // initialize the client in the network manager
            Network.InitializeClient();

            // load the initial wallet
            if (!Network.LoadWallet())
            {
                Debug.Log("Failed to load initial wallet");
            }

            // call insital flow state
            ChangeScreenState(DemoWalletScreen.UnlockWallet);
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