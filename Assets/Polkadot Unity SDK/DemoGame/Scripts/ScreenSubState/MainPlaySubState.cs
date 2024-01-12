using Assets.Scripts.ScreenStates;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    internal class MainPlaySubState : GameBaseState
    {
        public MainScreenState PlayScreenState => ParentState as MainScreenState;

        public MainPlaySubState(DemoGameController flowController, GameBaseState parent)
            : base(flowController, parent) { }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] EnterState");

            var floatBody = FlowController.VelContainer.Q<VisualElement>("FloatBody");
            floatBody.Clear();

            TemplateContainer scrollViewElement = ElementInstance("DemoGame/UI/Elements/ScrollViewElement");
            floatBody.Add(scrollViewElement);

            var scrollView = scrollViewElement.Q<ScrollView>("ScvElement");

            TemplateContainer elementInstance = ElementInstance("DemoGame/UI/Frames/PlayFrame");

            // add element
            scrollView.Add(elementInstance);
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] ExitState");
        }
    }
}