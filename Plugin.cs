using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using HarmonyLib;
using IPA;
using IPA.Loader;
using IpaLogger = IPA.Logging.Logger;

namespace BeatShocker;

[Plugin(RuntimeOptions.DynamicInit)]
internal class BeatShocker : MissMissionObjectiveChecker
{
    private Harmony harmony;
    private Assembly executingAssembly = Assembly.GetExecutingAssembly();
    internal static IpaLogger Log { get; private set; } = null!;

    // Methods with [Init] are called when the plugin is first loaded by IPA.
    // All the parameters are provided by IPA and are optional.
    // The constructor is called before any method with [Init]. Only use [Init] with one constructor.
    [Init]
    public BeatShocker(IpaLogger ipaLogger, PluginMetadata pluginMetadata)
    {
        harmony = new Harmony("BeatShocker");
        
        Log = ipaLogger;
        Log.Info($"{pluginMetadata.Name} {pluginMetadata.HVersion} initialized.");
    }

    [HarmonyPatch(typeof(ComboController), nameof(ComboController.HandleNoteWasMissed))]
    public class ShockHandler
    {
        public static void Prefix(ComboController __instance, NoteController noteController)
        {
            if (noteController.noteData.gameplayType == NoteData.GameplayType.Bomb) return;
            
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPAddress serverAddr = IPAddress.Parse("192.168.1.154");

            IPEndPoint endPoint = new IPEndPoint(serverAddr, 49555);

            string text = "ZAP";
            byte[] send_buffer = Encoding.ASCII.GetBytes(text);

            sock.SendTo(send_buffer, endPoint);
        }
    }

    [OnStart]
    public void OnApplicationStart()
    {
        harmony.PatchAll(executingAssembly);
        Log.Debug("OnApplicationStart");
    }

    [OnExit]
    public void OnApplicationQuit()
    {
        Log.Debug("OnApplicationQuit");
    }
}