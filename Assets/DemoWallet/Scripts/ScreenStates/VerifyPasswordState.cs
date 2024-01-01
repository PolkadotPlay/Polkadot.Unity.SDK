using Substrate.NET.Wallet;
using Substrate.NetApi.Model.Types;
using UnityEngine;
using UnityEngine.UIElements;
using static Substrate.NetApi.Mnemonic;

namespace Assets.Scripts.ScreenStates
{
    public class VerifyPasswordState : WalletBaseScreen
    {
        private Button _btnCreateWallet;

        public VerifyPasswordState(DemoWalletController _flowController)
            : base(_flowController) { }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}] EnterState");

            var bottomBound = FlowController.VelContainer.Q<VisualElement>("BottomBound");
            bottomBound.Clear();

            var visualTreeAsset = Resources.Load<VisualTreeAsset>($"DemoWallet/UI/Elements/VerifyPasswordElement");
            var instance = visualTreeAsset.Instantiate();
            instance.style.width = new Length(100, LengthUnit.Percent);
            instance.style.height = new Length(100, LengthUnit.Percent);

            // add manipulators
            _btnCreateWallet = instance.Q<Button>("BtnCreateWallet");
            _btnCreateWallet.SetEnabled(false);
            _btnCreateWallet.RegisterCallback<ClickEvent>(OnClickBtnCreateWallet);

            var txfVerifyPassword = instance.Q<CustomTextField>("TxfVerifyPassword");
            txfVerifyPassword.TextField.RegisterValueChangedCallback(OnChangeEventVerifyPassword);

            // add element
            bottomBound.Add(instance);

            // set stuff on the container
            SetStepInfos(FlowController.VelContainer, StepState.Done, StepState.Done, StepState.Current);

            var velLogo = FlowController.VelContainer.Q<VisualElement>("VelLogo");
            var imgLogo = Resources.Load<Texture2D>($"DemoWallet/Icons/IconOnboardVerify");
            velLogo.style.backgroundImage = imgLogo;
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}] ExitState [currentState={FlowController.CurrentState}]");

            FlowController.VelContainer.RemoveAt(1);
        }

        private void OnClickBtnCreateWallet(ClickEvent evt)
        {
            if (!Wallet.CreateFromMnemonic(FlowController.TempAccountPassword, FlowController.TempMnemonic, KeyType.Sr25519, BIP39Wordlist.English, FlowController.TempAccountName, out Wallet wallet))
            {
                Debug.Log($"Failed to create {FlowController.TempAccountName} wallet!");
                return;
            }
            else if (!NetworkWalletManager.GetInstance().ChangeWallet(wallet))
            {
                Debug.Log($"Couldn't change to {FlowController.TempAccountName} wallet!");
                return;
            }

            Debug.Log($"Create {FlowController.TempAccountName} wallet successful!");
            FlowController.ChangeScreenState(DemoWalletScreen.LoadScreen);
        }

        private void OnChangeEventVerifyPassword(ChangeEvent<string> evt)
        {
            var accountPassword = evt.newValue;

            if (accountPassword != FlowController.TempAccountPassword)
            {
                _btnCreateWallet.SetEnabled(false);
                return;
            }

            _btnCreateWallet.SetEnabled(true);
        }
    }
}