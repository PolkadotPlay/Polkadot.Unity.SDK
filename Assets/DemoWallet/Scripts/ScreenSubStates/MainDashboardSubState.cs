﻿using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
    internal class MainDashboardSubState : ScreenBaseState
    {
        public VisualElement _velAvatarSmallElement;

        public MainScreenState MainScreenState => ParentState as MainScreenState;

        public MainDashboardSubState(FlowController flowController, ScreenBaseState parent)
            : base(flowController, parent) { }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] EnterState");

            var floatBody = FlowController.VelContainer.Q<VisualElement>("FloatBody");
            floatBody.style.backgroundColor = GameConstant.ColorLightGrey;
            floatBody.Clear();

            TemplateContainer elementInstance = ElementInstance("DemoWallet/UI/Frames/DashboardFrame");

            // add element
            floatBody.Add(elementInstance);
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] ExitState");
        }
    }
}