using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ozeki.Media.MediaHandlers;
using System.IO;
using System.ComponentModel;
using System.Threading;

namespace BrightVision.Common.Business
{
    public class BrightSalesMP3StreamPlayback : MP3StreamPlayback
    {
        public BrightSalesMP3StreamPlayback(string audioFilePath)
            : base(audioFilePath)
        {
     
        }

        
        public BrightSalesMP3StreamPlayback(string audioFilePath, bool repeat, bool cacheStream)
            : base(audioFilePath, repeat, cacheStream)
        {
   
        }
        public BrightSalesMP3StreamPlayback(string audioFilePath, bool repeat, bool cacheStream, uint packetizationTime)
            : base(audioFilePath, repeat, cacheStream, packetizationTime)
        {
   
        }
        public BrightSalesMP3StreamPlayback(System.IO.Stream stream)
            : base(stream)
        {
      
        }
        public BrightSalesMP3StreamPlayback(System.IO.Stream stream, bool repeat, bool cacheStream)
            : base(stream, repeat, cacheStream)
        {
    
        }
        protected BrightSalesMP3StreamPlayback(System.IO.Stream stream, bool repeat, bool cacheStream, uint packetizationTime)
            : base(stream, repeat, cacheStream, packetizationTime)
        {
         
        }

        public long Position
        {
            get
            {
                return base.Stream.Position;
            }
            set
            {
               
                //base.IsStreaming = false;
                base.Stream.Position = value;
            }
        }

        public long Length
        {
            get
            {
                return base.Stream.Length;
            }
        }

        long _mp3Position = 0;
        public void StopStreaming()
        {

            //base.IsStreaming = false;
            base.PauseStreaming();
            base.Stream.Position = 0;
        }

        public void StartStreaming()
        {
            //base.IsStreaming = true;
            base.StartStreaming();
            EventPositionMonitor();
        }
        public void PauseStreaming()
        {
            //base.IsStreaming = false;
            base.PauseStreaming();
        }
        public void Resume()
        {
            //base.IsStreaming = true;
            base.StartStreaming();
        }
        public void Dispose() {
            StopStreaming();
            base.Dispose();
        }

        public void EventPositionMonitor()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerAsync();
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (; ; )
            {
                bool isCallCangedPosition = false;
                if (_mp3Position != base.Stream.Position)
                {
                    if (ChangedPosition != null)
                    {
                        isCallCangedPosition = true;
                        _mp3Position = base.Stream.Position;
                        ChangedPosition(base.Stream.Position);
                    }
                }
                if (!base.IsStreaming)
                {
                    if(!isCallCangedPosition)
                        ChangedPosition(base.Stream.Position);
                    break;
                }
                Thread.Sleep(300);
            }
        }
        public event ChangedPositionHandler ChangedPosition;
        public delegate void ChangedPositionHandler(long position);
    
    }
}
