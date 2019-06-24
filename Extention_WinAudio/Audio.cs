using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using NetCoreAudio;

namespace Extention_WinAudio
{
    public class Audio
    {
        private static Dictionary<string, FileInfo> SoundList = new Dictionary<string, FileInfo>();
        private static NetCoreAudio.Player player = null;
        private static bool playerPlayFinished = true;


        public static void PlaySound(string filePath)
        {
            Console.WriteLine("PlaySound(string '"+filePath+"')");
            if (player == null)
            {
                player = new NetCoreAudio.Player();
                player.PlaybackFinished += Player_PlaybackFinished;
            }
            if (playerPlayFinished == false) { player.Stop(); }
            playerPlayFinished = false;
            player.Play(filePath);
        }

        private static void Player_PlaybackFinished(object sender, EventArgs e)
        {
            playerPlayFinished = true;
        }
    }
}
