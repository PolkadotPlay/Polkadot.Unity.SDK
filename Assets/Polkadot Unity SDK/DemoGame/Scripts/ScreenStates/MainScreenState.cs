using Substrate.Integration;
using Substrate.Integration.Helper;
using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
    public class MainScreenState : GameBaseState
    {
        private Label _lblAccount;
        private Label _lblAddress;
        private Label _lblToken;

        private Button _btnFaucet;

        private Label _lblNodeVersion;
        private Label _lblConnection;
        private Label _lblBlockNumber;

        public Texture2D PortraitAlice { get; }
        public Texture2D PortraitBob { get; }
        public Texture2D PortraitCharlie { get; }
        public Texture2D PortraitDave { get; }
        public Texture2D PortraitCustom { get; }

        public MainScreenState(DemoGameController _flowController)
            : base(_flowController) {

            PortraitAlice = Resources.Load<Texture2D>($"DemoGame/Images/alice_portrait");
            PortraitBob = Resources.Load<Texture2D>($"DemoGame/Images/bob_portrait");
            PortraitCharlie = Resources.Load<Texture2D>($"DemoGame/Images/charlie_portrait");
            PortraitDave = Resources.Load<Texture2D>($"DemoGame/Images/dave_portrait");
            PortraitCustom = Resources.Load<Texture2D>($"DemoGame/Images/custom_portrait");
        }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}] EnterState");

            // filler is to avoid camera in the ui
            var topFiller = FlowController.VelContainer.Q<VisualElement>("VelTopFiller");
            topFiller.style.backgroundColor = GameConstant.ColorDark;

            var visualTreeAsset = Resources.Load<VisualTreeAsset>($"DemoGame/UI/Screens/MainScreenUI");
            var instance = visualTreeAsset.Instantiate();
            instance.style.width = new Length(100, LengthUnit.Percent);
            instance.style.height = new Length(98, LengthUnit.Percent);

            var topBound = instance.Q<VisualElement>("TopBound");

            _lblAccount = topBound.Q<Label>("LblAccount");
            _lblAddress = topBound.Q<Label>("LblAddress");
            _lblToken = topBound.Q<Label>("LblToken");

            _btnFaucet = topBound.Query<Button>("BtnFaucet");
            _btnFaucet.RegisterCallback<ClickEvent>(OnFaucetClicked);
            _btnFaucet.SetEnabled(false);

            var lblNodeUrl = topBound.Q<Label>("LblNodeUrl");
            lblNodeUrl.text = Network.CurrentNodeType.ToString();
            _lblNodeVersion = topBound.Q<Label>("LblNodeVersion");
            _lblConnection = topBound.Q<Label>("LblConnection");
            _lblBlockNumber = topBound.Q<Label>("LblBlockNumber");

            // add container
            FlowController.VelContainer.Add(instance);

            // load initial sub state
            FlowController.ChangeScreenSubState(DemoGameScreen.MainScreen, DemoGameSubScreen.MainChoose);

            // subscribe to connection changes
            Network.ConnectionStateChanged += OnConnectionStateChanged;
            Storage.OnNextBlocknumber += UpdateBlocknumber;

            // connect to substrate node
            Network.Client.ConnectAsync(true, true, CancellationToken.None);
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}] ExitState");

            // unsubscribe from event
            Network.ConnectionStateChanged -= OnConnectionStateChanged;
            Storage.OnNextBlocknumber -= UpdateBlocknumber;

            // remove container
            FlowController.VelContainer.RemoveAt(1);
        }

        private void OnConnectionStateChanged(bool IsConnected)
        {
            if (IsConnected)
            {
                _lblConnection.text = "Online";
            }
            else
            {
                _lblConnection.text = "Offline";
            }
        }

        private void UpdateBlocknumber(uint blocknumber)
        {
            _lblBlockNumber.text = blocknumber.ToString();

            if (Network.Client?.Account == null)
            {
                return;
            }

            _btnFaucet.SetEnabled(false);

            _lblAccount.text = Network.CurrentAccountName;
                       
            var address = Network.Client.Account.Value;
            _lblAddress.text = address.Substring(0, 6) + " ... " + address.Substring(20, 6);

            if (Storage.AccountInfo != null && Storage.AccountInfo.Data != null)
            {
                var amount = BigInteger.Divide(Storage.AccountInfo.Data.Free, new BigInteger(SubstrateNetwork.DECIMALS));

                _lblToken.text = GameConstant.BalanceFormatter(amount) + " HEXA";
                var specName = Network.Client.SubstrateClient.RuntimeVersion.SpecName;
                _lblNodeVersion.text = specName.Length > 20 ? $"{specName[..17]}..." : specName;

                // only enable if
                _btnFaucet.SetEnabled(!Network.Client.ExtrinsicManager.Running.Any() && GameConstant.FaucetThreshold < 100);
            }
            else
            {
                _btnFaucet.SetEnabled(!Network.Client.ExtrinsicManager.Running.Any());
            }
        }

        private async void OnFaucetClicked(ClickEvent evt)
        {
            _btnFaucet.SetEnabled(false);

            var sender = Network.Sudo;
            var target = Network.Client.Account;

            var amountToTransfer = new BigInteger(1000 * SubstrateNetwork.DECIMALS);

            Debug.Log($"[{nameof(NetworkManager)})] Send {amountToTransfer} from {sender.ToAccountId32().ToAddress()} to {target.ToAccountId32().ToAddress()}");
            
            var subscriptionId = await Network.Client.TransferKeepAliveAsync(sender, target.ToAccountId32(), amountToTransfer, 1, CancellationToken.None);
            if (subscriptionId == null)
            {
                Debug.LogError($"[{nameof(NetworkManager)}] Transfer failed");
                _btnFaucet.SetEnabled(true);
                return;
            }

            Debug.Log($"[{nameof(NetworkManager)}] Transfer executed!");
        }
    }
}