using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    public abstract class ScreenBaseState
    {
        public enum StepState
        {
            None,
            Current,
            Done
        }

        protected FlowController FlowController { get; private set; }

        protected ScreenBaseState ParentState { get; private set; }

        protected NetworkManager Network => NetworkManager.GetInstance();

        protected StorageManager Storage => StorageManager.GetInstance();

        protected ScreenBaseState(FlowController flowController, ScreenBaseState parentState = null)
        {
            FlowController = flowController;
            ParentState = parentState;
        }

        public abstract void EnterState();

        public abstract void ExitState();

        internal TemplateContainer ElementInstance(string elementPath, int widthPerc = 100, int heightPerc = 100)
        {
            var element = Resources.Load<VisualTreeAsset>(elementPath);
            var elementInstance = element.Instantiate();
            elementInstance.style.width = new Length(widthPerc, LengthUnit.Percent);
            elementInstance.style.height = new Length(heightPerc, LengthUnit.Percent);
            return elementInstance;
        }

        internal void SetStepInfos(VisualElement visualElement, StepState step1, StepState step2, StepState step3)
        {
            var imgCurrent1 = visualElement.Q<VisualElement>("ImgCurrent1");
            var imgMark1 = visualElement.Q<VisualElement>("ImgMark1");
            imgCurrent1.style.display = step1 == StepState.None || step1 == StepState.Done ? DisplayStyle.None : DisplayStyle.Flex;
            imgMark1.style.display = step1 == StepState.None || step1 == StepState.Current ? DisplayStyle.None : DisplayStyle.Flex;
            var imgCurrent2 = visualElement.Q<VisualElement>("ImgCurrent2");
            var imgMark2 = visualElement.Q<VisualElement>("ImgMark2");
            imgCurrent2.style.display = step2 == StepState.None || step2 == StepState.Done ? DisplayStyle.None : DisplayStyle.Flex;
            imgMark2.style.display = step2 == StepState.None || step2 == StepState.Current ? DisplayStyle.None : DisplayStyle.Flex;
            var imgCurrent3 = visualElement.Q<VisualElement>("ImgCurrent3");
            var imgMark3 = visualElement.Q<VisualElement>("ImgMark3");
            imgCurrent3.style.display = step3 == StepState.None || step3 == StepState.Done ? DisplayStyle.None : DisplayStyle.Flex;
            imgMark3.style.display = step3 == StepState.None || step3 == StepState.Current ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
}