using Assets.Scripts;
using Schnorrkel.Keys;
using Substrate.NetApi;
using Substrate.NetApi.Model.Extrinsics;
using Substrate.NetApi.Model.Rpc;
using Substrate.NetApi.Model.Types;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using KusamaExt = Substrate.Kusama.NET.NetApiExt.Generated;
using LocalExt = Substrate.Hexalem.NET.NetApiExt.Generated;
using PolkadotExt = Substrate.Polkadot.NET.NetApiExt.Generated;

namespace Substrate
{
    public enum SubstrateChains
    {
        Polkadot,
        Kusama,
        Local,
    }

    public enum SubstrateCmds
    {
        Runtime,
        Properties,
        Block,
        Custom
    }

    public class DemoExplorer : MonoBehaviour
    {
        public static MiniSecret MiniSecretAlice => new MiniSecret(Utils.HexToByteArray("0xe5be9a5092b81bca64be81d212e7f2f9eba183bb7a90954f7b76361f6edb5c0a"), ExpandMode.Ed25519);
        public static Account Alice => Account.Build(KeyType.Sr25519, MiniSecretAlice.ExpandToSecret().ToBytes(), MiniSecretAlice.GetPair().Public.Key);

        private Label _lblNodeUrl;
        private Label _lblNodeInfo;

        private Button _btnConnection;

        private List<VisualElement> _velSelectionArray;

        private List<Label> _lblCommandArray;
        private Label _lblCmdTransfer;

        private VisualElement _velChainLogo;

        private SubstrateClient _client;
        private bool _running = false;

        private Texture2D _polkadotLogo, _kusamaLogo, _hostLogo;
        private Texture2D _checkYes, _checkNo;

        private Func<CancellationToken, Task<RuntimeVersion>> StateRuntimeVersion { get; set; }
        private Func<CancellationToken, Task<Properties>> SystemProperties { get; set; }
        private Func<CancellationToken, Task<U32>> SystemStorageNumber { get; set; }
        private Func<CancellationToken, Task<U32>> SystemStorageCustom { get; set; }

        private JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        ///
        /// </summary>
        private void Awake()
        {
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                //Converters = { new BigIntegerConverter() }
            };

            _polkadotLogo = Resources.Load<Texture2D>("DemoExplorer/Icons/polkadot_logo");
            _kusamaLogo = Resources.Load<Texture2D>("DemoExplorer/Icons/kusama_logo");
            _hostLogo = Resources.Load<Texture2D>("DemoExplorer/Icons/host_logo");

            _checkYes = Resources.Load<Texture2D>("DemoExplorer/Icons/check_yes");
            _checkNo = Resources.Load<Texture2D>("DemoExplorer/Icons/check_no");
        }

        /// <summary>
        ///
        /// </summary>
        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var velMainView = root.Q<VisualElement>("VelMainView");

            _lblNodeUrl = velMainView.Q<Label>("LblNodeUrl");

            _lblNodeInfo = velMainView.Q<Label>("LblNodeInfo");
            _lblNodeInfo.text = $"C:\\>";

            _btnConnection = velMainView.Q<Button>("BtnConnection");
            _btnConnection.RegisterCallback<ClickEvent>(ev => OnToggleConnectAsync());

            _velChainLogo = velMainView.Q<VisualElement>("VelChainLogo");

            _velSelectionArray = new List<VisualElement>();

            var velSelPolkadotCheck = velMainView.Q<VisualElement>("VelSelPolkadotCheck");
            velSelPolkadotCheck.RegisterCallback<ClickEvent>(ev => OnSelectClicked(SubstrateChains.Polkadot));
            _velSelectionArray.Add(velSelPolkadotCheck);
            var velSelKusamaCheck = velMainView.Q<VisualElement>("VelSelKusamaCheck");
            velSelKusamaCheck.RegisterCallback<ClickEvent>(ev => OnSelectClicked(SubstrateChains.Kusama));
            _velSelectionArray.Add(velSelKusamaCheck);
            var velSelLocalCheck = velMainView.Q<VisualElement>("VelSelLocalCheck");
            velSelLocalCheck.RegisterCallback<ClickEvent>(ev => OnSelectClicked(SubstrateChains.Local));
            _velSelectionArray.Add(velSelLocalCheck);

            _lblCommandArray = new List<Label>();

            var lblCmdRuntime = velMainView.Q<Label>("LblCmdRuntime");
            lblCmdRuntime.RegisterCallback<ClickEvent>(ev => OnCommandClicked(SubstrateCmds.Runtime));
            _lblCommandArray.Add(lblCmdRuntime);
            var lblCmdProperties = velMainView.Q<Label>("LblCmdProperties");
            lblCmdProperties.RegisterCallback<ClickEvent>(ev => OnCommandClicked(SubstrateCmds.Properties));
            _lblCommandArray.Add(lblCmdProperties);
            var lblCmdBlock = velMainView.Q<Label>("LblCmdBlock");
            lblCmdBlock.RegisterCallback<ClickEvent>(ev => OnCommandClicked(SubstrateCmds.Block));
            _lblCommandArray.Add(lblCmdBlock);
            var lblCmdCustom = velMainView.Q<Label>("LblCmdCustom");
            lblCmdCustom.RegisterCallback<ClickEvent>(ev => OnCommandClicked(SubstrateCmds.Custom));
            _lblCommandArray.Add(lblCmdCustom);

            _lblCmdTransfer = velMainView.Q<Label>("LblCmdTransfer");
            _lblCmdTransfer.RegisterCallback<ClickEvent>(ev => OnTransferClicked());

            // initialize
            OnSelectClicked(SubstrateChains.Polkadot);
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        private void Update()
        {
            // Method intentionally left empty.
        }

        /// <summary>
        /// Drop down menu initialising a new client specific to each relay- or parachain.
        /// </summary>
        /// <param name="dropdown"></param>
        public async void OnSelectClicked(SubstrateChains substrateChain)
        {
            Debug.Log($"OnSelectNode {substrateChain}");

            // disconnect when changing substrate chain
            if (_client != null && _client.IsConnected)
            {
                await _client.CloseAsync();
                _btnConnection.text = "CONNECT";
            }

            _lblCmdTransfer.SetEnabled(false);
            _lblCommandArray.ForEach(p => p.SetEnabled(false));

            // reset selection to unselected
            _velSelectionArray.ForEach(p => p.style.backgroundImage = _checkNo);
            _velSelectionArray[(int)substrateChain].style.backgroundImage = _checkYes;

            // the system storage calls for most of the substrate based chains are similar, one could use one client to access
            // the similar storage calls, which is good for most of the frame pallets, but it might not work due to different
            // frame versions or different orders in the generation proccess.
            string url = string.Empty;
            switch (substrateChain)
            {
                case SubstrateChains.Polkadot:
                    {
                        url = "wss://1rpc.io/dot";
                        _lblNodeUrl.text = url;
                        _velChainLogo.style.backgroundImage = _polkadotLogo;
                        _client = new PolkadotExt.SubstrateClientExt(new Uri(url), ChargeTransactionPayment.Default());

                        StateRuntimeVersion = ((PolkadotExt.SubstrateClientExt)_client).State.GetRuntimeVersionAsync;
                        SystemProperties = ((PolkadotExt.SubstrateClientExt)_client).System.PropertiesAsync;
                        SystemStorageNumber = ((PolkadotExt.SubstrateClientExt)_client).SystemStorage.Number;

                        SystemStorageCustom = ((PolkadotExt.SubstrateClientExt)_client).SystemStorage.EventCount;
                    }
                    break;

                case SubstrateChains.Kusama:
                    {
                        url = "wss://1rpc.io/ksm";
                        _lblNodeUrl.text = url;
                        _velChainLogo.style.backgroundImage = _kusamaLogo;
                        _client = new KusamaExt.SubstrateClientExt(new Uri(url), ChargeTransactionPayment.Default());

                        StateRuntimeVersion = ((KusamaExt.SubstrateClientExt)_client).State.GetRuntimeVersionAsync;
                        SystemProperties = ((KusamaExt.SubstrateClientExt)_client).System.PropertiesAsync;
                        SystemStorageNumber = ((KusamaExt.SubstrateClientExt)_client).SystemStorage.Number;

                        SystemStorageCustom = ((KusamaExt.SubstrateClientExt)_client).SystemStorage.EventCount;
                    }
                    break;

                case SubstrateChains.Local:
                    {
                        url = "ws://127.0.0.1:9944";
                        _lblNodeUrl.text = url;
                        _velChainLogo.style.backgroundImage = _hostLogo;
                        _client = new LocalExt.SubstrateClientExt(new Uri(url), ChargeTransactionPayment.Default());

                        StateRuntimeVersion = ((LocalExt.SubstrateClientExt)_client).State.GetRuntimeVersionAsync;
                        SystemProperties = ((LocalExt.SubstrateClientExt)_client).System.PropertiesAsync;
                        SystemStorageNumber = ((LocalExt.SubstrateClientExt)_client).SystemStorage.Number;

                        SystemStorageCustom = ((LocalExt.SubstrateClientExt)_client).SystemStorage.EventCount;
                    }
                    break;

                default:
                    Debug.LogError($"Unhandled enumeration value {substrateChain}!");
                    break;
            }

            _lblNodeInfo.text = $"_";
        }

        private async Task OnCommandClicked(SubstrateCmds command)
        {
            Debug.Log($"OnCommand {command}");

            if (_running)
            {
                Debug.Log("Command skipped because a command is already running.");
                return;
            }

            if (_client == null)
            {
                Debug.Log("Command skipped because client is null.");
                return;
            }

            if (!_client.IsConnected)
            {
                Debug.Log("Command skipped because client is not connected.");
                return;
            }

            _running = true;

            try
            {
                await ExecuteCommand(command);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            finally
            {
                _running = false;
            }
        }

        private async Task ExecuteCommand(SubstrateCmds command)
        {
            switch (command)
            {
                case SubstrateCmds.Runtime:
                    await HandleRuntimeCommand();
                    break;

                case SubstrateCmds.Properties:
                    await HandlePropertiesCommand();
                    break;

                case SubstrateCmds.Block:
                    await HandleBlockCommand();
                    break;

                case SubstrateCmds.Custom:
                    await HandleCustomCommand();
                    break;
            }
        }

        private async Task HandleRuntimeCommand()
        {
            string commandText = SubstrateCmds.Runtime.ToString().ToLower();
            _lblNodeInfo.text = $"{commandText}\n -> {commandText} = ...";
            var runtimeVersion = await StateRuntimeVersion(CancellationToken.None);

            _lblNodeInfo.text = runtimeVersion == null
                ? $"{commandText}\n -> {commandText} = null"
                : $"{commandText}\n -> {commandText} = {JsonSerializer.Serialize(runtimeVersion, _jsonSerializerOptions)}";
        }

        private async Task HandlePropertiesCommand()
        {
            string commandText = SubstrateCmds.Properties.ToString().ToLower();
            _lblNodeInfo.text = $"{commandText}\n -> {commandText} = ...";
            var properties = await SystemProperties(CancellationToken.None);

            _lblNodeInfo.text = properties == null
                ? $"{commandText}\n -> {commandText} = null"
                : $"{commandText}\n -> {commandText} = {JsonSerializer.Serialize(properties, _jsonSerializerOptions)}";
        }

        private async Task HandleBlockCommand()
        {
            string commandText = SubstrateCmds.Block.ToString().ToLower();
            _lblNodeInfo.text = $"{commandText}\n -> {commandText} = ...";
            var blockNumber = await SystemStorageNumber(CancellationToken.None);

            _lblNodeInfo.text = blockNumber == null
                ? $"{commandText}\n -> {commandText} = null"
                : $"{commandText}\n -> {commandText} = {blockNumber.Value}";
        }

        private async Task HandleCustomCommand()
        {
            string commandText = SubstrateCmds.Custom.ToString().ToLower();
            string customName = "event count";
            _lblNodeInfo.text = $"{commandText}\n -> {customName} = ...";
            var blockNumber = await SystemStorageCustom(CancellationToken.None);

            _lblNodeInfo.text = blockNumber == null
                ? $"{commandText}\n -> {customName} = null"
                : $"{commandText}\n -> {customName} = {blockNumber.Value}";
        }

        /// <summary>
        /// Toogeling connection to the substrate chain on and off.
        /// </summary>
        public async void OnToggleConnectAsync()
        {
            if (_running || _client == null)
            {
                return;
            }

            _btnConnection.SetEnabled(false);

            _lblNodeInfo.text = $"{_btnConnection.text.ToLower()}\n -> client is_connected = ...";

            _running = true;

            if (_client.IsConnected)
            {
                await _client.CloseAsync();
            }
            else
            {
                await _client.ConnectAsync(false, true, CancellationToken.None);
            }

            _lblCommandArray.ForEach(p => p.SetEnabled(_client.IsConnected));
            _lblCmdTransfer.SetEnabled(_client.IsConnected && _client is LocalExt.SubstrateClientExt);

            _lblNodeInfo.text = $"{_btnConnection.text.ToLower()}\n -> client is_connected = {_client.IsConnected}";

            _btnConnection.text = _client.IsConnected ? "DISCONNECT" : "CONNECT";

            _btnConnection.SetEnabled(true);

            _running = false;
        }

        /// <summary>
        /// On transfer button clicked.
        /// </summary>
        private async void OnTransferClicked()
        {
            if (_running || _client == null || !_client.IsConnected)
            {
                return;
            }

            _running = true;

            var logStr = "";

            var accountAlice = new LocalExt.Model.sp_core.crypto.AccountId32();
            accountAlice.Create(Utils.GetPublicKeyFrom(Alice.Value));

            var properties = await SystemProperties(CancellationToken.None);
            var tokenDecimals = BigInteger.Pow(10, properties.TokenDecimals);

            var accountInfo = await ((LocalExt.SubstrateClientExt)_client).SystemStorage.Account(accountAlice, CancellationToken.None);
            if (accountInfo == null)
            {
                Debug.Log("No account found!");
            }

            logStr += $"Alice account has: {BigInteger.Divide(accountInfo.Data.Free.Value, tokenDecimals)} {properties.TokenSymbol}\n";
            _lblNodeInfo.text = logStr;

            var account32 = new LocalExt.Model.sp_core.crypto.AccountId32();
            account32.Create(Utils.GetPublicKeyFrom("5FHneW46xGXgs5mUiveU4sbTyGBzmstUspZC92UhjJM694ty"));

            var multiAddress = new LocalExt.Model.sp_runtime.multiaddress.EnumMultiAddress();
            multiAddress.Create(LocalExt.Model.sp_runtime.multiaddress.MultiAddress.Id, account32);

            var amount = new BaseCom<U128>();
            amount.Create(BigInteger.Multiply(42, tokenDecimals));
            logStr += $"Sending Bob: {amount.Value.Value} {properties.TokenSymbol}\n";
            _lblNodeInfo.text = logStr;

            var transferKeepAlive = LocalExt.Storage.BalancesCalls.TransferKeepAlive(multiAddress, amount);

            try
            {
                var subscription = await GenericExtrinsicAsync(_client, Alice, transferKeepAlive, CancellationToken.None);
                Debug.Log($"subscription id => {subscription}");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            _running = false;
        }

        /// <summary>
        /// Generic extrinsic method.
        /// </summary>
        /// <param name="extrinsicType"></param>
        /// <param name="extrinsicMethod"></param>
        /// <param name="concurrentTasks"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        internal async Task<string> GenericExtrinsicAsync(SubstrateClient client, Account account, Method extrinsicMethod, CancellationToken token)
        {
            string subscription = await client.Author.SubmitAndWatchExtrinsicAsync(ActionExtrinsicUpdate, extrinsicMethod, account, ChargeTransactionPayment.Default(), 64, token);

            if (subscription == null)
            {
                return null;
            }

            Debug.Log($"Generic extrinsic sent {extrinsicMethod.ModuleName}_{extrinsicMethod.CallName} with {subscription}");

            return subscription;
        }

        /// <summary>
        /// Callback for extrinsic updates.
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="extrinsicUpdate"></param>
        public void ActionExtrinsicUpdate(string subscriptionId, ExtrinsicStatus extrinsicUpdate)
        {
            var broadcast = extrinsicUpdate.Broadcast != null ? string.Join(",", extrinsicUpdate.Broadcast) : "";
            var hash = extrinsicUpdate.Hash != null ? extrinsicUpdate.Hash.Value : "";

            Debug.Log($"{subscriptionId} => {extrinsicUpdate.ExtrinsicState} [HASH: {hash}] [BROADCAST: {broadcast}]");

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                _lblNodeInfo.text = _lblNodeInfo.text + $"" +
                $"\n{subscriptionId}" +
                $"\n => {extrinsicUpdate.ExtrinsicState} {(hash.Length > 0 ? $"[{hash}]" : "")}{(broadcast.Length > 0 ? $"[{broadcast}]" : "")}";
            });
        }
    }
}