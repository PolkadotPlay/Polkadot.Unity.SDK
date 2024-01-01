using Substrate.NET.Wallet;
using System.Text.Json;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
    public class ImportJsonState : WalletBaseScreen
    {
        private Button _btnCreateWalletJson;

        private CustomTextField _txfJsonContent;

        public ImportJsonState(DemoWalletController _flowController)
            : base(_flowController) { }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}] EnterState");

            var bottomBound = FlowController.VelContainer.Q<VisualElement>("BottomBound");
            bottomBound.Clear();

            var visualTreeAsset = Resources.Load<VisualTreeAsset>($"DemoWallet/UI/Elements/ImportJsonElement");
            var instance = visualTreeAsset.Instantiate();
            instance.style.width = new Length(100, LengthUnit.Percent);
            instance.style.height = new Length(100, LengthUnit.Percent);

            // add manipulators
            _btnCreateWalletJson = instance.Q<Button>("BtnCreateWalletJson");
            _btnCreateWalletJson.SetEnabled(false);
            _btnCreateWalletJson.RegisterCallback<ClickEvent>(OnClickBtnCreateWalletJson);
            _txfJsonContent = instance.Q<CustomTextField>("TxfJsonContent");
            _txfJsonContent.TextField.RegisterValueChangedCallback(OnChangeEventJsonContent);

            // add element
            bottomBound.Add(instance);

            // set stuff on the container
            SetStepInfos(FlowController.VelContainer, StepState.Current, StepState.None, StepState.None);

            var velLogo = FlowController.VelContainer.Q<VisualElement>("VelLogo");
            var imgLogo = Resources.Load<Texture2D>($"DemoWallet/Icons/IconImportJson");
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

        private void OnClickBtnCreateWalletJson(ClickEvent evt)
        {
            if (!Wallet.Load(FlowController.TempAccountName, FlowController.TempFileStore, out Wallet wallet))
            {
                return;
            }

            Network.ChangeWallet(wallet);
            Debug.Log($"Changing to json wallet {FlowController.TempAccountName}");
            FlowController.ChangeScreenState(DemoWalletScreen.UnlockWallet);
        }

        private void OnChangeEventJsonContent(ChangeEvent<string> evt)
        {
            var jsonContent = evt.newValue;

            _btnCreateWalletJson.SetEnabled(false);
            if (jsonContent.Length < 200 || !jsonContent.Contains('#'))
            {
                return;
            }

            var array = evt.newValue.Split('#');
            var accountName = array[0].ToUpper();
            var jsonWallet = array[1];
            if (!Wallet.IsValidWalletName(accountName) || jsonWallet.Length < 180)
            {
                return;
            }

            FileStore fileStore;
            try
            {
                fileStore = JsonSerializer.Deserialize<FileStore>(jsonWallet);
            }
            catch
            {
                return;
            }

            FlowController.TempAccountName = accountName;
            FlowController.TempFileStore = fileStore;

            _btnCreateWalletJson.SetEnabled(true);
        }
    }
}