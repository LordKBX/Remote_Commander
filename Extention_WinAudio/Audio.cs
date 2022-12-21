using System;
using System.IO;
using System.Collections.Generic;
using Accord.Audio;
using Accord.DirectSound;
using Accord.Video;
using Accord.Collections;

namespace Extention_WinAudio
{
    public class Audio
    {
        private static Dictionary<string, FileInfo> SoundList = new Dictionary<string, FileInfo>();
        private static NetCoreAudio.Player player = null;
        private static bool playerPlayFinished = true;

        private static AudioCaptureDevice audioSource = null;
        private static Accord.Video.MJPEGStream videoSource = null;


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

        private static void SoundRecordStart() {
            // Create default capture device
            audioSource = new AudioCaptureDevice();
            Accord.Video.MJPEGStream videoSource = new Accord.Video.MJPEGStream();

            // Specify capturing options
            audioSource.DesiredFrameSize = 4096;
            audioSource.SampleRate = 22050;
            audioSource.Format = SampleFormat.Format128BitComplex;

            // Specify the callback function which will be
            // called once a sample is completely available
            audioSource.NewFrame += audioSource_NewFrame;

            // Start capturing
            audioSource.Start();

        }

        // The callback function should determine what
        // should be done with the samples being caught
        private static void audioSource_NewFrame(object sender, Accord.Audio.NewFrameEventArgs eventArgs)
        {
            // Read current frame...
            Signal s = eventArgs.Signal;
            byte[] tab = s.RawData;


            // Process/play/record it
            // ...
        }
    }
}
