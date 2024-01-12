using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
    public class LoadScreenState : WalletBaseScreen
    {
        private VisualElement _velAnimSpinner;
        private VisualElement _velBackDrop;
        private VisualElement _velOverlay;

        public LoadScreenState(DemoWalletController _flowController)
            : base(_flowController) { }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}] EnterState");

            // clear sensitive data
            FlowController.TempAccount = null;
            FlowController.TempAccountName = "";
            FlowController.TempAccountPassword = "";
            FlowController.TempMnemonic = "";

            var visualTreeAsset = Resources.Load<VisualTreeAsset>($"DemoWallet/UI/Screens/LoadingScreenUI");
            var instance = visualTreeAsset.Instantiate();
            instance.style.width = new Length(100, LengthUnit.Percent);
            instance.style.height = new Length(98, LengthUnit.Percent);

            var lbProgressInfo = instance.Q<Label>("LblProgressInfo");
            lbProgressInfo.text = $"...tick tack tick...";

            _velAnimSpinner = instance.Q<VisualElement>("VelAnimSpinner");
            _velAnimSpinner.style.transitionProperty = new List<StylePropertyName>() { "rotate" };
            _velAnimSpinner.style.transitionTimingFunction = new List<EasingFunction> { EasingMode.Linear };
            _velAnimSpinner.style.transitionDuration = new List<TimeValue> { new(1, TimeUnit.Second) };
            _velAnimSpinner.style.transitionDelay = new List<TimeValue> { 0f };
            _velAnimSpinner.RegisterCallback<TransitionEndEvent>((evt) =>
            {
                _velAnimSpinner.style.transitionDuration = new List<TimeValue> { new(0, TimeUnit.Second) };
                _velAnimSpinner.style.rotate = new StyleRotate(new Rotate(0)); // Angle.Turns(0)
                _velAnimSpinner.style.transitionDuration = new List<TimeValue> { new(1, TimeUnit.Second) };
                _velAnimSpinner.style.rotate = new StyleRotate(new Rotate(360)); // Angle.Turns(1)

                var qu = Random.Range(0, DemoWalletConstants.GameQuotes.Length);
                lbProgressInfo.text = $"...{DemoWalletConstants.GameQuotes[qu]}...";
            });

            _velOverlay = instance.Q<VisualElement>("Overlay");

            _velBackDrop = instance.Q<VisualElement>("VelBackDrop");
            _velBackDrop.style.transitionProperty = new List<StylePropertyName>() { "background-color" };
            _velBackDrop.style.transitionTimingFunction = new List<EasingFunction> { EasingMode.Linear };
            _velBackDrop.style.transitionDuration = new List<TimeValue> { new(3, TimeUnit.Second) };
            _velBackDrop.style.transitionDelay = new List<TimeValue> { 0f };

            // add container
            FlowController.VelContainer.Add(instance);

            _velAnimSpinner.schedule.Execute(() =>
            {
                _velAnimSpinner.style.rotate = new StyleRotate(new Rotate(360)); // Angle.Turns(1)
            }).StartingIn(100); // Delay in milliseconds

            // subscribe to connection changes
            Network.ConnectionStateChanged += OnConnectionStateChanged;

            // connect to substrate node
            Network.Client.ConnectAsync(true, true, CancellationToken.None);
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}] ExitState");

            // unsubscribe from event
            Network.ConnectionStateChanged -= OnConnectionStateChanged;

            FlowController.VelContainer.RemoveAt(1);
        }

        private void OnConnectionStateChanged(bool IsConnected)
        {
            if (IsConnected)
            {
                _velOverlay.style.display = DisplayStyle.Flex;
                _velBackDrop.RegisterCallback<TransitionEndEvent>((evt) =>
                {
                    FlowController.ChangeScreenState(DemoWalletScreen.MainScreen);
                });

                _velBackDrop.schedule.Execute(() =>
                {
                    _velBackDrop.style.backgroundColor = new StyleColor(new Color32(255, 255, 255, 255));
                }).StartingIn(100); // Delay in milliseconds
            }
        }
    }
}