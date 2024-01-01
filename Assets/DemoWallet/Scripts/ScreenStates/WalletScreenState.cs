using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
    public class WalletScreenState : WalletBaseScreen
    {
        public WalletScreenState(DemoWalletController _flowController)
            : base(_flowController) { }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}] EnterState");

            // filler is to avoid camera in the ui
            var topFiller = FlowController.VelContainer.Q<VisualElement>("VelTopFiller");
            topFiller.style.backgroundColor = DemoWalletConstants.ColorDark;

            var visualTreeAsset = Resources.Load<VisualTreeAsset>($"DemoWallet/UI/Screens/MainScreenUI");
            var instance = visualTreeAsset.Instantiate();
            instance.style.width = new Length(100, LengthUnit.Percent);
            instance.style.height = new Length(98, LengthUnit.Percent);

            // add container
            FlowController.VelContainer.Add(instance);

            FlowController.ChangeScreenSubState(DemoWalletScreen.MainScreen, DemoWalletSubScreen.Dashboard);
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}] ExitState");

            FlowController.VelContainer.RemoveAt(1);
        }
    }
}