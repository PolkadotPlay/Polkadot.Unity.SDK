using Substrate.Integration.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.ScreenStates
{
    public enum PartyAction
    {
        Non,
        Add,
        Rem,
    }

    internal class MainInviteSubState : GameBaseState
    {
        public MainScreenState PlayScreenState => ParentState as MainScreenState;

        private Button _btnCreate;
        private Button _btnInvite;

        private Label _lblPlayerCount;
        private Label _lblExtriniscUpdate;

        private List<VisualElement> _velPlayers;

        private string _subscriptionId;

        private List<string> _players;

        private AccountType _invitePlayer;
        private PartyAction _inviteAction;

        public MainInviteSubState(DemoGameController flowController, GameBaseState parent)
            : base(flowController, parent) { }

        public override void EnterState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] EnterState");

            _players = new List<string>();
            _invitePlayer = AccountType.Alice;
            _inviteAction = PartyAction.Non;

            var floatBody = FlowController.VelContainer.Q<VisualElement>("FloatBody");
            floatBody.Clear();

            TemplateContainer scrollViewElement = ElementInstance("DemoGame/UI/Elements/ScrollViewElement");
            floatBody.Add(scrollViewElement);

            var scrollView = scrollViewElement.Q<ScrollView>("ScvElement");

            TemplateContainer elementInstance = ElementInstance("DemoGame/UI/Frames/InviteFrame");

            _btnCreate = elementInstance.Q<Button>("BtnCreate");
            _btnCreate.RegisterCallback<ClickEvent>(OnBtnCreateClicked);

            var btnLeft = elementInstance.Q<Button>("BtnLeft");
            btnLeft.RegisterCallback<ClickEvent>(e => OnBtnChangeClicked(-1));

            var btnRight = elementInstance.Q<Button>("BtnRight");
            btnRight.RegisterCallback<ClickEvent>(e => OnBtnChangeClicked(+1));

            var velPlayerBox = elementInstance.Q<VisualElement>("VelPlayerBox");
            _velPlayers = velPlayerBox.Children().ToList();
            _velPlayers.ForEach((vel) => vel.style.display = DisplayStyle.None);

            _btnInvite = elementInstance.Q<Button>("BtnInvite");
            _btnInvite.SetEnabled(false);
            _btnInvite.RegisterCallback<ClickEvent>(OnBtnTogglePlayerClicked);

            var btnBack = elementInstance.Q<Button>("BtnBack");
            btnBack.RegisterCallback<ClickEvent>(OnBtnBackClicked);

            _lblPlayerCount = elementInstance.Q<Label>("LblPlayerCount");

            _lblExtriniscUpdate = elementInstance.Q<Label>("LblExtriniscUpdate");

            PlayerParty(PartyAction.Add, Network.CurrentAccountName);

            PlayerSetInvite(_invitePlayer);

            // add element
            scrollView.Add(elementInstance);

            // subscribe to connection changes
            Storage.OnStorageUpdated += OnStorageUpdated;
            Network.Client.ExtrinsicManager.ExtrinsicUpdated += OnExtrinsicUpdated;
        }

        public override void ExitState()
        {
            Debug.Log($"[{this.GetType().Name}][SUB] ExitState");

            // unsubscribe from event
            Storage.OnStorageUpdated -= OnStorageUpdated;
            Network.Client.ExtrinsicManager.ExtrinsicUpdated -= OnExtrinsicUpdated;
        }

        private void PlayerParty(PartyAction partyAction, string currentAccountName)
        {
            switch (partyAction)
            {
                case PartyAction.Add:
                    {
                        if (_players.Count >= 4)
                        {
                            return;
                        }

                        if (_players.Contains(currentAccountName))
                        {
                            return;
                        }

                        _players.Add(currentAccountName);
                    }
                    break;

                case PartyAction.Rem:
                    {
                        if (_players.Count <= 1)
                        {
                            return;
                        }

                        if (!_players.Contains(currentAccountName))
                        {
                            return;
                        }

                        _players.Remove(currentAccountName);
                    }
                    break;
            }

            _lblPlayerCount.text = $"{_players.Count} Player Game";

            for (int i = 0; i < _velPlayers.Count; i++)
            {
                if (_players.Count <= i)
                {
                    _velPlayers[i].style.display = DisplayStyle.None;
                    _velPlayers[i].style.backgroundImage = null;
                    continue;
                }

                var playerName = _players[i];

                // get corresponding account type for portrait
                if (!Enum.TryParse(playerName, out AccountType accountType))
                {
                    accountType = AccountType.Custom;
                }

                switch (accountType)
                {
                    case AccountType.Alice:
                        _velPlayers[i].style.backgroundImage = new StyleBackground(PlayScreenState.PortraitAlice);
                        break;

                    case AccountType.Bob:
                        _velPlayers[i].style.backgroundImage = new StyleBackground(PlayScreenState.PortraitBob);
                        break;

                    case AccountType.Charlie:
                        _velPlayers[i].style.backgroundImage = new StyleBackground(PlayScreenState.PortraitCharlie);
                        break;

                    case AccountType.Dave:
                        _velPlayers[i].style.backgroundImage = new StyleBackground(PlayScreenState.PortraitDave);
                        break;

                    default:
                        _velPlayers[i].style.backgroundImage = new StyleBackground(PlayScreenState.PortraitCustom);
                        break;
                }
                _velPlayers[i].style.display = DisplayStyle.Flex;

                // updateed button
                PlayerSetInvite(accountType);
            }
        }

        private void PlayerSetInvite(AccountType player)
        {
            _btnInvite.text = player.ToString().ToUpper();

            if (Network.Client.ExtrinsicManager.Running.Any())
            {
                _btnInvite.style.backgroundColor = GameConstant.PastelBlue;
                _btnInvite.SetEnabled(false);
                _inviteAction = PartyAction.Non;
                return;
            }

            var index = _players.IndexOf(player.ToString());

            if (index == -1 && _players.Count < 4)
            {
                _btnInvite.style.backgroundColor = GameConstant.AddPlayer;
                _btnInvite.SetEnabled(true);
                _inviteAction = PartyAction.Add;
            }
            else if (index > -1 && player.ToString() != _players[0])
            {
                _btnInvite.style.backgroundColor = GameConstant.RemPlayer;
                _btnInvite.SetEnabled(true);
                _inviteAction = PartyAction.Rem;
            }
            else
            {
                _btnInvite.style.backgroundColor = GameConstant.PastelBlue;
                _btnInvite.SetEnabled(false);
                _inviteAction = PartyAction.Non;
            }
        }

        private void OnStorageUpdated(uint blocknumber)
        {
            if (Network.Client.ExtrinsicManager.Running.Any())
            {
                _btnCreate.SetEnabled(false);
                return;
            }

            if (Storage.HexaGame == null)
            {
                _btnCreate.text = "CREATE";
                _lblExtriniscUpdate.text = "\"No pvp game to join, bro!\"";
            }
            else
            {
                _btnCreate.text = "JOIN";
                _lblExtriniscUpdate.text = $"\"Hey bro, {Storage.HexaGame.PlayersCount} buddies, waiting!\"";
                FlowController.ChangeScreenSubState(DemoGameScreen.MainScreen, DemoGameSubScreen.MainChoose);
            }

            _btnCreate.SetEnabled(true);

            // updated invite button here
            PlayerSetInvite(_invitePlayer);
        }

        private void OnExtrinsicUpdated(string subscriptionId, ExtrinsicInfo extrinsicInfo)
        {
            if (_subscriptionId == null || _subscriptionId != subscriptionId)
            {
                return;
            }
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                switch (extrinsicInfo.TransactionEvent)
                {
                    case Substrate.NetApi.Model.Rpc.TransactionEvent.Validated:
                        _lblExtriniscUpdate.text = $"\"Oh bro, need to check what you sent me.\"";
                        break;

                    case Substrate.NetApi.Model.Rpc.TransactionEvent.Broadcasted:
                        _lblExtriniscUpdate.text = $"\"Pump the jam, let's shuffle the dices, gang.\"";
                        break;

                    case Substrate.NetApi.Model.Rpc.TransactionEvent.BestChainBlockIncluded:
                        _lblExtriniscUpdate.text = $"\"Besti, bro!\"";
                        break;

                    case Substrate.NetApi.Model.Rpc.TransactionEvent.Finalized:
                        _lblExtriniscUpdate.text = $"\"We got a stamp!\"";
                        break;

                    case Substrate.NetApi.Model.Rpc.TransactionEvent.Error:
                        _lblExtriniscUpdate.text = $"\"That doesn't work, bro!\"";
                        break;

                    case Substrate.NetApi.Model.Rpc.TransactionEvent.Invalid:
                        _lblExtriniscUpdate.text = $"\"Invalid, bro, your invalid!\"";
                        break;

                    case Substrate.NetApi.Model.Rpc.TransactionEvent.Dropped:
                        _lblExtriniscUpdate.text = $"\"Gonna, drop this, bro.\"";
                        break;

                    default:
                        _lblExtriniscUpdate.text = $"\"No blue, funk soul bro!\"";
                        break;
                }
            });
        }

        private void OnBtnChangeClicked(int value)
        {
            var values = Enum.GetValues(typeof(AccountType)).Length;
            _invitePlayer = (AccountType)((((int)_invitePlayer) + values + value) % values);

            PlayerSetInvite(_invitePlayer);
        }

        private async void OnBtnCreateClicked(ClickEvent evt)
        {
            Storage.UpdateHexalem = true;

            if (Storage.HexaGame != null)
            {
                FlowController.ChangeScreenState(DemoGameScreen.PlayScreen);
            }
            else if (!Network.Client.ExtrinsicManager.Running.Any())
            {
                _btnCreate.SetEnabled(false);
                _btnInvite.SetEnabled(false);

                // get accounts to create game ...
                var playerAccounts = _players.Select(p =>
                {
                    if (!Enum.TryParse(p, out AccountType accountType))
                    {
                        accountType = AccountType.Custom;
                    }
                    return Network.GetAccount(accountType, p).Item1;
                }).ToList();

                _btnCreate.text = "WAIT";
                var subscriptionId = await Network.Client.CreateGameAsync(Network.Client.Account, playerAccounts, 25, 1, CancellationToken.None);
                if (subscriptionId == null)
                {
                    _btnCreate.SetEnabled(true);
                    _btnInvite.SetEnabled(true);
                    return;
                }

                Debug.Log($"Extrinsic[CreateGameAsync] submited: {subscriptionId}");

                _subscriptionId = subscriptionId;
            }
        }

        private void OnBtnTogglePlayerClicked(ClickEvent evt)
        {
            if (_inviteAction == PartyAction.Non)
            {
                return;
            }

            PlayerParty(_inviteAction, _invitePlayer.ToString());
        }

        private void OnBtnBackClicked(ClickEvent evt)
        {
            FlowController.ChangeScreenSubState(DemoGameScreen.MainScreen, DemoGameSubScreen.MainChoose);
        }
    }
}