using Serilog;
using UnityEngine;

namespace Assets.Scripts.Logging
{
    public class LogManager : MonoBehaviour
    {
        void Awake()
        {
            // configure serilog for Unity
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .WriteTo.UnityConsole()
                .CreateLogger();
        }
    }
}