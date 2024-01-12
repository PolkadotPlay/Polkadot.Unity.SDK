using Schnorrkel.Keys;
using Substrate.Integration;
using Substrate.Integration.Client;
using Substrate.Integration.Helper;
using Substrate.NET.Wallet;
using Substrate.NetApi;
using Substrate.NetApi.Model.Rpc;
using Substrate.NetApi.Model.Types;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public enum AccountType
    {
        Alice,
        Bob,
        Charlie,
        Dave,
        Custom
    }

    public enum NodeType
    {
        Local,
        Solo,
        Tanssi
    }

    public delegate void ExtrinsicStateUpdate(string subscriptionId, ExtrinsicStatus extrinsicUpdate);

    public class NetworkManager : Singleton<NetworkManager>
    {
        public delegate void ConnectionStateChangedHandler(bool IsConnected);

        public delegate void ExtrinsicCheckHandler();

        public event ConnectionStateChangedHandler ConnectionStateChanged;

        public event ExtrinsicCheckHandler ExtrinsicCheck;

        public MiniSecret MiniSecretAlice => new MiniSecret(Utils.HexToByteArray("0xe5be9a5092b81bca64be81d212e7f2f9eba183bb7a90954f7b76361f6edb5c0a"), ExpandMode.Ed25519);
        public Account SudoAlice => Account.Build(KeyType.Sr25519, MiniSecretAlice.ExpandToSecret().ToBytes(), MiniSecretAlice.GetPair().Public.Key);

        public MiniSecret MiniSecretSudo => new MiniSecret(Utils.HexToByteArray(""), ExpandMode.Ed25519);
        public Account SudoHexalem => Account.Build(KeyType.Sr25519, MiniSecretSudo.ExpandToSecret().ToBytes(), MiniSecretSudo.GetPair().Public.Key);

        // Sudo account if needed
        public Account Sudo { get; private set; }

        private string _nodeUrl;
        public string NodeUrl => _nodeUrl;

        private readonly NetworkType _networkType = NetworkType.Live;

        public AccountType CurrentAccountType { get; private set; }
        public string CurrentAccountName { get; private set; }

        public NodeType CurrentNodeType { get; private set; }

        private SubstrateNetwork _client;
        public SubstrateNetwork Client => _client;

        private bool? _lastConnectionState = null;

        protected override void Awake()
        {
            base.Awake();
            //Your code goes here
            CurrentAccountType = AccountType.Alice;
            CurrentNodeType = NodeType.Local;
            Sudo = SudoAlice;
            _nodeUrl = "ws://127.0.0.1:9944";
            InitializeClient();
        }

        public void Start()
        {
            InvokeRepeating(nameof(UpdateNetworkState), 0.0f, 2.0f);
            InvokeRepeating(nameof(UpdatedExtrinsic), 0.0f, 3.0f);
        }

        private void OnDestroy()
        {
            CancelInvoke(nameof(UpdateNetworkState));
            CancelInvoke(nameof(UpdatedExtrinsic));
        }

        private void UpdateNetworkState()
        {
            if (_client == null)
            {
                return;
            }

            var connectionState = _client.IsConnected;
            if (_lastConnectionState == null || _lastConnectionState != connectionState)
            {
                ConnectionStateChanged?.Invoke(connectionState);
                _lastConnectionState = connectionState;
            }
        }

        private void UpdatedExtrinsic()
        {
            ExtrinsicCheck?.Invoke();
        }

        public (Account, string) GetAccount(AccountType accountType, string custom = null)
        {
            Account result;
            string name;
            switch (accountType)
            {
                case AccountType.Alice:
                case AccountType.Bob:
                case AccountType.Charlie:
                case AccountType.Dave:
                    name = accountType.ToString();
                    result = BaseClient.RandomAccount(GameConstant.AccountSeed, accountType.ToString(), KeyType.Sr25519);
                    break;

                case AccountType.Custom:
                    name = custom.ToUpper();
                    result = BaseClient.RandomAccount(GameConstant.AccountSeed, custom, KeyType.Sr25519);
                    break;

                default:
                    name = AccountType.Alice.ToString();
                    result = BaseClient.RandomAccount(GameConstant.AccountSeed, AccountType.Alice.ToString(), KeyType.Sr25519); 
                    break;
            }

            return (result, name);
        }

        public bool SetAccount(AccountType accountType, string custom = null)
        {
            if (accountType == AccountType.Custom && (string.IsNullOrEmpty(custom) || custom.Length < 3))
            {
                return false;
            }

            CurrentAccountType = accountType;

            var tuple = GetAccount(accountType, custom);

            Client.Account = tuple.Item1;
            CurrentAccountName = tuple.Item2;

            return true;
        }

        public bool ToggleNodeType()
        {
            switch (CurrentNodeType)
            {
                case NodeType.Local:
                    CurrentNodeType = NodeType.Solo;
                    _nodeUrl = "wss://hexalem-rpc.substrategaming.org";
                    Sudo = SudoAlice;
                    break;

                case NodeType.Solo:
                    CurrentNodeType = NodeType.Tanssi;
                    _nodeUrl = "wss://fraa-dancebox-3023-rpc.a.dancebox.tanssi.network";
                    Sudo = SudoHexalem;
                    break;

                case NodeType.Tanssi:
                    CurrentNodeType = NodeType.Local;
                    _nodeUrl = "ws://127.0.0.1:9944";
                    Sudo = SudoAlice;
                    break;
            }

            InitializeClient();
            return true;
        }

        public List<Wallet> StoredWallets()
        {
            var result = new List<Wallet>();
            foreach (var w in WalletFiles())
            {
                if (!Wallet.Load(w, out Wallet wallet))
                {
                    Debug.Log($"Failed to load wallet {w}");
                }

                result.Add(wallet);
            }
            return result;
        }

        private IEnumerable<string> WalletFiles()
        {
            var d = new DirectoryInfo(CachingManager.GetInstance().PersistentPath);
            return d.GetFiles(Wallet.ConcatWalletFileType("*")).Select(p => Path.GetFileNameWithoutExtension(p.Name));
        }

        // Start is called before the first frame update
        public void InitializeClient()
        {
            _client = new SubstrateNetwork(null, _networkType, _nodeUrl);
            SetAccount(CurrentAccountType, CurrentAccountName);
        }
    }
}