using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using ArrayDisplay.UI;
using SlimDX.DirectSound;
using SlimDX.Multimedia;

namespace ArrayDisplay.sound {
    /// <summary>
    ///     声音播放控件
    /// </summary>
    public class DxPlaySound
    {
        readonly int cNotifyNum; // 通知的个数

        readonly DirectSound device;

        readonly AutoResetEvent mNotificationEvent;
        readonly int mNotifySize; // 每次通知大小

        readonly List<byte> playBuf = new List<byte>();
        readonly SecondarySoundBuffer scdBuffer;
        bool isStarted;
        bool isRunning;
        int preSaveTime;

        public DxPlaySound(int frequency)
        {
            device = new DirectSound(); //音频设备对象

            IntPtr hwnd = new WindowInteropHelper(DisPlayWindow.HMainWindow).Handle; //设置窗口句柄
            device.SetCooperativeLevel(hwnd, CooperativeLevel.Priority);

            //设置Wav音频文件对象属性
            WaveFormat waveformat = SetWaveFormat(frequency);

            //设置通知对象
            mNotifySize = waveformat.AverageBytesPerSecond / 5; //0.2S数据
            int mainBufferSize = waveformat.AverageBytesPerSecond * 2; //  2s数据长度
            cNotifyNum = mainBufferSize / mNotifySize; //通知个数 10 == 2s/0.2s

            //设置主缓存区
            SoundBufferDescription sndBufferDesc = new SoundBufferDescription
            {
                SizeInBytes = mainBufferSize,
                Format = waveformat,
                Flags = BufferFlags.ControlPositionNotify | BufferFlags.ControlFrequency | BufferFlags.GlobalFocus | BufferFlags.GetCurrentPosition2
            };

            scdBuffer = new SecondarySoundBuffer(device, sndBufferDesc); //为声音建立二级缓存

            if (scdBuffer != null)
            {
                mNotificationEvent = new AutoResetEvent(false);
                isRunning = true;
                Task.Factory.StartNew(NotifyThread);
                NotificationPosition[] positionNotify = new NotificationPosition[cNotifyNum]; //通知数组
                for (int i = 0; i < cNotifyNum; i++)
                {
                    positionNotify[i].Offset = mNotifySize * (i + 1) - 1;
                    positionNotify[i].Event = mNotificationEvent;
                }
                scdBuffer.SetNotificationPositions(positionNotify);
            }
        }

        WaveFormat SetWaveFormat(int frequency)
        {
            WaveFormat waveformat = new WaveFormat();
            waveformat.Channels = 1; //声道
            waveformat.FormatTag = WaveFormatTag.Pcm; //音频类型
            waveformat.SamplesPerSecond = frequency; // 采样率（单位：赫兹） 典型值（11025,22050,44100）Hz
            waveformat.BitsPerSample = 16; //采样位数
            waveformat.BlockAlignment = 2; //单位采样点额字节数 =  BitsPerSample / 2            
            waveformat.AverageBytesPerSecond = waveformat.BlockAlignment * waveformat.SamplesPerSecond; //采样1秒的字节数
            return waveformat;
        }

        void NotifyThread()
        {
            var temp = new byte[mNotifySize];
            int offset = 0;
            try
            {
                while (isRunning)
                {
                    mNotificationEvent.WaitOne();
                    int playpos = scdBuffer.CurrentPlayPosition; //读指针（读取数据开端）
                    int wrpos = scdBuffer.CurrentWritePosition; // 写指针（读取数据结尾）

                    //while (PlayBuf.Count < mNotifySize) ;

                    if (playBuf.Count >= mNotifySize)
                    {
                        playBuf.CopyTo(0, temp, 0, mNotifySize);
                        playBuf.RemoveRange(0, mNotifySize);
                        scdBuffer.Write(temp, 0, mNotifySize, offset * mNotifySize, LockFlags.None);

                        //                        App.log.InfoFormat("声音....{0},{1},{2}", playpos, wrpos, playBuf.Count);
                    }
                    else
                    {
                        Array.Clear(temp, 0, temp.Length);
                        scdBuffer.Write(temp, 0, mNotifySize, offset * mNotifySize, LockFlags.None);
                        
                    }
                    scdBuffer.Play(0, PlayFlags.Looping);
                    Console.WriteLine("Second:  " + DateTime.Now.Second);
                    offset = (offset + 1) % cNotifyNum;
                }
            }
            catch (ThreadAbortException)
            {
                device.Dispose();
                mNotificationEvent.Dispose();
            }
        }
        /// <summary>
        /// 写入每秒数据
        /// </summary>
        /// <param name="buf">每秒音频数据</param>
        public void WriteOneTimData(byte[] buf)
        {
            if (!isStarted)
            {
                //缓存2s
                if (preSaveTime < 2)
                    scdBuffer.Write(buf, 0, buf.Length, buf.Length * preSaveTime, LockFlags.None);
                else
                {
                    if (preSaveTime == 2)
                    {
                        isStarted = true;
                        scdBuffer.CurrentPlayPosition = 0;
                        scdBuffer.Play(0, PlayFlags.Looping);
                        Console.WriteLine("First:  " + DateTime.Now.Second);
                    }

                }
                preSaveTime++;
            }
            else
            {
                var byData = new byte[buf.Length];
                Array.Copy(buf, 0, byData, 0, buf.Length);
                playBuf.AddRange(byData);
                //Console.WriteLine("Write sound Data");
            }
        }

        public void Close()
        {
            isRunning = false;
            playBuf.Clear();
        }
    }
}
