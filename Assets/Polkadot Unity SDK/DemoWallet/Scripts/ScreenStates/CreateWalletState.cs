using Substrate.NET.Wallet;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
    public class CreateWalletState : WalletBaseScreen
    {
        private Button _btnCreateWallet;

        private Label _lblAccountName;

        public CreateWalletState(DemoWalletController _flowController)
            : base(_flowController) { }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}] EnterState");

            var bottomBound = FlowController.VelContainer.Q<VisualElement>("BottomBound");
            bottomBound.Clear();

            var visualTreeAsset = Resources.Load<VisualTreeAsset>($"DemoWallet/UI/Elements/CreateWalletElement");
            var instance = visualTreeAsset.Instantiate();
            instance.style.width = new Length(100, LengthUnit.Percent);
            instance.style.height = new Length(100, LengthUnit.Percent);

            // add manipulators
            _btnCreateWallet = instance.Q<Button>("BtnCreateWallet");
            _btnCreateWallet.SetEnabled(false);
            _btnCreateWallet.RegisterCallback<ClickEvent>(OnClickBtnCreateWallet);

            _lblAccountName = instance.Q<Label>("LblAccountName");
            var lblAccountAddress = instance.Q<Label>("LblAccountAddress");
            lblAccountAddress.text = FlowController.TempAccount != null ? FlowController.TempAccount.Value : "unkown";

            var txfAccountName = instance.Q<CustomTextField>("TxfAccountName");
            txfAccountName.TextField.RegisterValueChangedCallback(OnChangeEventAccountName);

            // add element
            bottomBound.Add(instance);

            // set stuff on the container
            SetStepInfos(FlowController.VelContainer, StepState.Done, StepState.Current, StepState.None);

            var velLogo = FlowController.VelContainer.Q<VisualElement>("VelLogo");
            var imgLogo = Resources.Load<Texture2D>($"DemoWallet/Icons/IconOnboardAccount");
            velLogo.style.backgroundImage = imgLogo;
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}] ExitState [currentState={FlowController.CurrentState}]");

            if (FlowController.CurrentState == DemoWalletScreen.UnlockWallet)
            {
                FlowController.VelContainer.RemoveAt(1);
            }
        }

        private void OnClickBtnCreateWallet(ClickEvent evt)
        {
            FlowController.ChangeScreenState(DemoWalletScreen.SetPassword);
        }

        private void OnChangeEventAccountName(ChangeEvent<string> evt)
        {
            var accountName = evt.newValue.ToUpper();

            if (!Wallet.IsValidWalletName(accountName))
            {
                FlowController.TempAccountName = null;
                _lblAccountName.text = "...";
                _btnCreateWallet.SetEnabled(false);
                return;
            }
            FlowController.TempAccountName = accountName;
            _lblAccountName.text = accountName;
            _btnCreateWallet.SetEnabled(true);
        }
    }
}