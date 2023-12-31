using Assets.Scripts;
using System.Threading;
using UnityEngine;

public class StorageManager : Singleton<StorageManager>
{
    public delegate void NextBlocknumberHandler(ulong blocknumber);

    public event NextBlocknumberHandler OnNextBlocknumber;

    public NetworkManager Network => NetworkManager.GetInstance();

    public ulong BlockNumber { get; private set; }

    /// <summary>
    /// Awake is called when the script instance is being loaded
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        //Your code goes here
    }

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start()
    {
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    private void Update()
    {
    }

    internal void StartPolling()
    {
        InvokeRepeating(nameof(UpdatedBaseData), 0.0f, 2.0f);
    }

    public bool CanPollStorage()
    {
        if (Network.Client == null)
        {
            Debug.LogError($"[StorageManager] Client is null");
            return false;
        }

        if (!Network.Client.IsConnected)
        {
            Debug.Log($"[StorageManager] Client is not connected");
            return false;
        }

        return true;
    }

    private async void UpdatedBaseData()
    {
        if (!CanPollStorage())
        {
            return;
        }

        var block = await Network.Client.Chain.GetBlockAsync(CancellationToken.None);
        if (block == null)
        {
            return;
        }

        ulong? blockNumber = block.Block?.Header?.Number?.Value;

        if (blockNumber == null || BlockNumber >= blockNumber)
        {
            return;
        }

        BlockNumber = blockNumber.Value;
        OnNextBlocknumber?.Invoke(blockNumber.Value);
    }
}