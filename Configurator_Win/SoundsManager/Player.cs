using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;

namespace Configurator_Win.SoundsManager
{
    class Player
    {
        private List<Thread> ThreadList = null;
        private SoundsManager parent = null;

        public Player(SoundsManager parent=null) {
            this.parent = parent;
            ThreadList = new List<Thread>();
        }
        public void StopAll() {
            if (ThreadList.Count > 0)
            {
                foreach (Thread th in ThreadList)
                {
                    th.Abort();
                }
                ThreadList.Clear();
            }
        }

        public void PlaySound(string file)
        {
            try
            {
                parent.SoundsManager_FormClosed(null, null);
                Thread objThread = new Thread(new ParameterizedThreadStart(PlaySoundAsync));
                objThread.IsBackground = true;
                objThread.Priority = ThreadPriority.AboveNormal;
                objThread.Start(file);
                ThreadList.Add(objThread);
            }
            catch (ThreadStartException objException) { }
            catch (ThreadAbortException objException) { }
            catch (Exception objException) { }
        }

        private void PlaySoundAsync(object file)
        {
            try
            {
                using (var audioFile = new AudioFileReader((string)file))
                {
                    using (var outputDevice = new WaveOutEvent())
                    {
                        outputDevice.Init(audioFile);
                        outputDevice.Play();
                        outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;
                        while (outputDevice.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(1000);
                        }
                        try { outputDevice.Stop(); } catch (Exception err) { }
                        outputDevice.Dispose();
                    }
                    audioFile.Close();
                    audioFile.Dispose();
                }
            }
            catch (Exception err) { }
        }

        private void OutputDevice_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            WaveOutEvent se = (WaveOutEvent)sender;
            se.Dispose();
        }
    }
}
