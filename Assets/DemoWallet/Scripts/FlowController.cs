using Assets.Scripts.ScreenStates;
using Substrate.NET.Wallet;
using Substrate.NetApi.Model.Types;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    public enum ScreenState
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

    public enum ScreenSubState
    {
        Dashboard,
    }

    public class FlowController : MonoBehaviour
    {
        internal NetworkManager Network => NetworkManager.GetInstance();
        internal StorageManager Storage => StorageManager.GetInstance();

        internal readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();

        public VisualElement VelContainer { get; private set; }

        public ScreenState CurrentState { get; private set; }

        private ScreenBaseState _currentState;
        private ScreenBaseState _currentSubState;
        private readonly Dictionary<ScreenState, ScreenBaseState> _stateDictionary = new();
        private readonly Dictionary<ScreenState, Dictionary<ScreenSubState, ScreenBaseState>> _subStateDictionary = new();

        internal Account TempAccount { get; set; }
        internal FileStore TempFileStore { get; set; }

        internal string TempAccountName { get; set; } // TODO: remove this ....
        internal string TempAccountPassword { get; set; } // TODO: remove this ....
        internal string TempMnemonic { get; set; }  // TODO: remove this ....

        private void Awake()
        {
            // Initialize states
            _stateDictionary.Add(ScreenState.UnlockWallet, new UnlockWalletState(this));
            _stateDictionary.Add(ScreenState.AccountSelection, new AccountSelectionState(this));
            _stateDictionary.Add(ScreenState.ResetWallet, new ResetWalletState(this));
            _stateDictionary.Add(ScreenState.OnBoarding, new OnBoardingState(this));
            _stateDictionary.Add(ScreenState.CreateWallet, new CreateWalletState(this));
            _stateDictionary.Add(ScreenState.SetPassword, new SetPasswordState(this));
            _stateDictionary.Add(ScreenState.VerifyPassword, new VerifyPasswordState(this));
            _stateDictionary.Add(ScreenState.ImportJson, new ImportJsonState(this));
            _stateDictionary.Add(ScreenState.ImportSeed, new ImportSeedState(this));
            _stateDictionary.Add(ScreenState.LoadScreen, new LoadScreenState(this));
            var mainScreen = new MainScreenState(this);
            _stateDictionary.Add(ScreenState.MainScreen, mainScreen);

            var mainScreenSubStates = new Dictionary<ScreenSubState, ScreenBaseState>
            {
                { ScreenSubState.Dashboard, new MainDashboardSubState(this, mainScreen) },
            };

            _subStateDictionary.Add(ScreenState.MainScreen, mainScreenSubStates);
        }

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            VelContainer = root.Q<VisualElement>("VelContainer");

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
            ChangeScreenState(ScreenState.UnlockWallet);
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        private void Update()
        {
            // Method intentionally left empty.
        }

        /// <summary>
        /// Change the screen state
        /// </summary>
        /// <param name="newScreenState"></param>
        internal void ChangeScreenState(ScreenState newScreenState)
        {
            CurrentState = newScreenState;

            // exit current state if any
            _currentState?.ExitState();

            // change the state
            _currentState = _stateDictionary[newScreenState];

            // exit any active sub-state when changing the main state
            _currentSubState?.ExitState();
            _currentSubState = null;

            // enter current state
            _currentState.EnterState();
        }

        /// <summary>
        /// Change the sub state of the current screen state
        /// </summary>
        /// <param name="parentState"></param>
        /// <param name="newSubState"></param>
        internal void ChangeScreenSubState(ScreenState parentState, ScreenSubState newSubState)
        {
            if (_subStateDictionary.ContainsKey(parentState) && _subStateDictionary[parentState].ContainsKey(newSubState))
            {
                // exit current sub state if any
                _currentSubState?.ExitState();

                // change the sub state
                _currentSubState = _subStateDictionary[parentState][newSubState];

                // enter current sub state
                _currentSubState.EnterState();
            }
        }
    }
}