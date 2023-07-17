namespace FrostweepGames.Plugins.WebGL.MicrophonePro
{
    [UnityEditor.InitializeOnLoad]
    public class DefineProcessing : Plugins.DefineProcessing
    {
        internal static readonly string[] _Defines = new string[]
        {
            "FG_MPRO"
        };

        static DefineProcessing()
        {
            AddOrRemoveDefines(true, true, _Defines);
        }
    }
}