// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   处理与分发网络数据
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ArrayDisplay.Net {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Windows;

    using ArrayDisplay.UI;

    /// <inheritdoc />
    /// <summary>
    ///     处理与分发网络数据
    /// </summary>
    public sealed class Dataproc : IDisposable {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Dataproc" /> class.
        /// </summary>
        public Dataproc() {
            this.TransFormFft = new FFT_TransForm();
            this.ArrayNums = ConstUdpArg.ARRAY_NUM; // 探头个数

            this.FreqWaveEvent = new AutoResetEvent(false);
            this.WorkEnergyEvent = new AutoResetEvent(false);
            this.DelayBytesEvent = new AutoResetEvent(false);
            this.OrigBytesEvent = new AutoResetEvent(false);
            this.WorkBytesEvent = new AutoResetEvent(false);
        }

        /// <summary>
        ///     Gets or sets the sound event handler.
        /// </summary>
        public static EventHandler<byte[]> SoundEventHandler {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the pre graph event handler.
        /// </summary>
        public static EventHandler<float[]> PreGraphEventHandler {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the bck graph event handler.
        /// </summary>
        public static EventHandler<float[]> BckGraphEventHandler {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the energy array event handler.
        /// </summary>
        public static EventHandler<float[]> EnergyArrayEventHandler {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the orig graph event handler.
        /// </summary>
        public static EventHandler<float[]> OrigGraphEventHandler {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the frap graph event handler.
        /// </summary>
        public static EventHandler<float[]> FrapGraphEventHandler {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the frap point graph event handler.
        /// </summary>
        public static EventHandler<Point[]> FrapPointGraphEventHandler {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the delay graph event handler.
        /// </summary>
        public static EventHandler<float[]> DelayGraphEventHandler {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the energy thread.
        /// </summary>
        public Thread EnergyThread {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the work thread.
        /// </summary>
        public Thread WorkThread {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the delay thread.
        /// </summary>
        public Thread DelayThread {
            get;

            set;
        }

        /// <summary>
        ///     Gets or sets the orig thread.
        /// </summary>
        public Thread OrigThread {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the freq thread.
        /// </summary>
        public Thread FreqThread {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the array nums.
        /// </summary>
        public int ArrayNums {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the work bytes event.
        /// </summary>
        public AutoResetEvent WorkBytesEvent {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the orig bytes event.
        /// </summary>
        public AutoResetEvent OrigBytesEvent {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the delay bytes event.
        /// </summary>
        public AutoResetEvent DelayBytesEvent {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the work energy event.
        /// </summary>
        public AutoResetEvent WorkEnergyEvent {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the freq wave event.
        /// </summary>
        public AutoResetEvent FreqWaveEvent {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the delay wave bytes.
        /// </summary>
        public byte[][] DelayWaveBytes {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the orig wave bytes.
        /// </summary>
        public byte[][] OrigWaveBytes {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the work wave bytes.
        /// </summary>
        public byte[][] WorkWaveBytes {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the work wavefdatas.
        /// </summary>
        float[] WorkWavefdatas {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the freq wave one.
        /// </summary>
        float[] FreqWavefData {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the delay wave floats.
        /// </summary>
        float[][] DelayWaveFloats {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the trans form fft.
        /// </summary>
        FFT_TransForm TransFormFft {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the origdata.
        /// </summary>
        byte[] Origdata {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the work wave floats.
        /// </summary>
        float[][] WorkWaveFloats {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the energy floats.
        /// </summary>
        float[] EnergyFloats {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the orig wave floats.
        ///     原始数据解调数据，横坐标表示探头序列，纵坐标表示数据组成
        /// </summary>
        float[][] OrigWaveFloats {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the play wave bytes.
        /// </summary>
        byte[][] PlayWaveBytes {
            get;
            set;
        }

        /// <summary>
        ///     Gets or sets the listen coefficent.
        /// </summary>
        float ListenCoefficent {
            get;
            set;
        }

        #region IDisposable

        /// <summary>
        ///     The release unmanaged resources.
        /// </summary>
        void ReleaseUnmanagedResources() {
            // TODO release unmanaged resources here
        }

        /// <summary>
        ///     The dispose.
        /// </summary>
        /// <param name="disposing">
        ///     The disposing.
        /// </param>
        void Dispose(bool disposing) {
            this.ReleaseUnmanagedResources();
            if (disposing) {
                if (this.OrigThread != null) {
                    this.OrigThread.Abort();
                    if (this.OrigThread.ThreadState != ThreadState.Aborted) {
                        Thread.Sleep(100);
                    }
                }

                this.DelayThread?.Abort();

                if (this.WorkThread != null) {
                    this.WorkThread.Abort();
                    if (this.WorkThread.ThreadState != ThreadState.Aborted) {
                        Thread.Sleep(100);
                    }
                }

                this.EnergyThread?.Abort();

                this.FreqThread?.Abort();

                this.WorkBytesEvent?.Dispose();

                this.OrigBytesEvent?.Dispose();

                this.DelayBytesEvent?.Dispose();

                this.WorkEnergyEvent?.Dispose();

                this.FreqWaveEvent?.Dispose();
            }
        }

        /// <inheritdoc />
        ~Dataproc() {
            this.Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        ///     The init depend on Network type .
        /// </summary>
        /// <param name="type">
        ///     The type is WaveType.
        /// </param>
        public void Init(ConstUdpArg.WaveType type) {
            switch(type) {
                case ConstUdpArg.WaveType.Delay:
                    this.DelayInit();
                    break;
                case ConstUdpArg.WaveType.Normal:
                    this.NormalInit();
                    break;
                case ConstUdpArg.WaveType.Orig:
                    this.OrigInit();
                    break;
                default:
                    Console.WriteLine("初始化Dataproc出错");
                    break;
            }
        }

        /// <summary>
        ///     The origdata init.
        /// </summary>
        void OrigInit() {
            // 时分长度
            this.OrigWaveBytes = new byte[ConstUdpArg.ORIG_DETECT_LENGTH][];
            this.OrigWaveFloats = new float[ConstUdpArg.ORIG_DETECT_LENGTH][];

            // 原始数据帧数
            this.Origdata = new byte[DisPlayWindow.Info.OrigFramNums * this.ArrayNums];

            // 每帧长度
            for(int i = 0; i < ConstUdpArg.ORIG_DETECT_LENGTH; i++) {
                this.OrigWaveBytes[i] = new byte[(ConstUdpArg.ORIG_FRAME_LENGTH - 2) * DisPlayWindow.Info.OrigFramNums]; // 2位数据位
                this.OrigWaveFloats[i] = new float[(ConstUdpArg.ORIG_FRAME_LENGTH - 2) / 2 * DisPlayWindow.Info.OrigFramNums];
            }

            this.OrigThread = new Thread(this.ThreadOrigWaveStart) { IsBackground = true };
            this.OrigThread.Start();
        }

        /// <summary>
        ///     The normalData init.
        /// </summary>
        void NormalInit() {
            this.TransFormFft = new FFT_TransForm();
            this.WorkWaveBytes = new byte[this.ArrayNums][];
            this.WorkWaveFloats = new float[this.ArrayNums][];
            this.WorkWaveBytes = new byte[this.ArrayNums][];
            this.PlayWaveBytes = new byte[this.ArrayNums][];

            int frameNum = ConstUdpArg.WORK_FRAME_NUMS * 4; // 工作数据帧数

            // int frameNum = 2000 * ConstUdpArg.WORK_FRAME_LENGTH; //工作数据帧数
            for(int i = 0; i < this.ArrayNums; i++) {
                this.WorkWaveBytes[i] = new byte[frameNum * 4]; // 避免数据为null
                this.WorkWaveFloats[i] = new float[frameNum]; // 避免数据为null 
                this.PlayWaveBytes[i] = new byte[frameNum * 2]; // 避免数据为null 
            }

            this.WorkWavefdatas = new float[frameNum]; // 工作波形
            this.EnergyFloats = new float[this.ArrayNums];
            this.ListenCoefficent = (float)Math.Pow(10, 50 / 20.0F) * 100; // 31622.78; //听音强度

            this.WorkThread = new Thread(this.ThreadWorkWaveStart) { IsBackground = true };
            this.WorkThread.Start();
            this.EnergyThread = new Thread(this.ThreadEnergyStart) { IsBackground = true };
            this.EnergyThread.Start();
            this.FreqThread = new Thread(this.ThreadFreqWaveStart) { IsBackground = true };
            this.FreqThread.Start();
        }

        /// <summary>
        ///     The delaydata init.
        /// </summary>
        void DelayInit() {
            this.DelayWaveBytes = new byte[ConstUdpArg.DELAY_FRAME_CHANNELS][];
            this.DelayWaveFloats = new float[ConstUdpArg.DELAY_FRAME_CHANNELS][];
            for(int i = 0; i < ConstUdpArg.DELAY_FRAME_CHANNELS; i++) {
                this.DelayWaveBytes[i] = new byte[(ConstUdpArg.DELAY_FRAME_LENGTH - 2) * ConstUdpArg.DELAY_FRAME_NUMS];
                this.DelayWaveFloats[i] = new float[(ConstUdpArg.DELAY_FRAME_LENGTH - 2) * ConstUdpArg.DELAY_FRAME_NUMS / 2];
            }

            this.DelayThread = new Thread(this.ThreadDelayWaveStart) { IsBackground = true };
            this.DelayThread.Start();
        }

        /// <summary>
        ///     The thread delay wave start.
        /// </summary>
        void ThreadDelayWaveStart() {
            while(true) {
                this.DelayBytesEvent.WaitOne();
                var r = new byte[2];

                for(int i = 0; i < this.DelayWaveBytes.Length; i++) {
                    for(int j = 0; j < this.DelayWaveBytes[0].Length / 2; j++) {
                        r[0] = this.DelayWaveBytes[i][(j * 2) + 1];
                        r[1] = this.DelayWaveBytes[i][(j * 2) + 0];
                        short a = BitConverter.ToInt16(r, 0);
                        this.DelayWaveFloats[i][j] = a / 8192.0f;
                    }
                }

                int channel = DisPlayWindow.Info.DelayChannel - 1;
                DelayGraphEventHandler(null, this.DelayWaveFloats[channel]);
            }
        }

        /// <summary>
        ///     The thread orig wave start.
        /// </summary>
        void ThreadOrigWaveStart() {
            while(true) {
                this.OrigBytesEvent.WaitOne();
                var r = new byte[2];
                for(int i = 0; i < ConstUdpArg.ORIG_DETECT_LENGTH; i++) {
                    for(int j = 0; j < (this.OrigWaveBytes[0].Length / 2) - 1; j++) {
                        r[0] = this.OrigWaveBytes[i][(j * 2) + 1];
                        r[1] = this.OrigWaveBytes[i][j * 2];
                        short a = BitConverter.ToInt16(r, 0);

                        this.OrigWaveFloats[i][j] = a / 8192.0f;
                    }
                }

                if (OrigGraphEventHandler != null) {
                    string sender = "OrigNet";
                    int index = DisPlayWindow.Info.OrigChannel - 1 + ((DisPlayWindow.Info.OrigTdiv - 1) * 8);
                    OrigGraphEventHandler.Invoke(sender, this.OrigWaveFloats[index]);
                }
            }
        }

        /// <summary>
        ///     线程处理函数：能量数据处理
        /// </summary>
        void ThreadEnergyStart() {
            while(true) {
                this.WorkEnergyEvent.WaitOne();
                double ftemp = 0;
                for(int i = 0; i < this.WorkWaveFloats.Length; i++) {
                    float f = this.WorkWaveFloats[i].Max();
                    ftemp = Math.Abs(f);
                    this.EnergyFloats[i] = (float)ftemp;
                }

                float max = this.EnergyFloats.Max();
                for(int i = 0; i < this.EnergyFloats.Length; i++) {
                    if (Math.Abs(max) > float.Epsilon) {
                        this.EnergyFloats[i] = this.EnergyFloats[i] / max;
                    }
                    else {
                        this.EnergyFloats[i] = 0;
                    }
                }

                var rlist = new List<float>();
                for(int i = 0; i < this.EnergyFloats.Length; i++) {
                    rlist.Add(this.EnergyFloats[i]);
                }

                if (EnergyArrayEventHandler != null) {
                    EnergyArrayEventHandler.Invoke(null, rlist.ToArray());
                }
            }
        }

        /// <summary>
        ///     The thread work wave start.
        /// </summary>
        void ThreadWorkWaveStart() {
            while(true) {
                this.WorkBytesEvent.WaitOne();
                var r = new byte[4];
                for(int i = 0; i < ConstUdpArg.ARRAY_NUM; i++) {
                    for(int j = 0; j < ConstUdpArg.WORK_FRAME_NUMS; j++) {
                        r[0] = this.WorkWaveBytes[i][(j * 4) + 3];
                        r[1] = this.WorkWaveBytes[i][(j * 4) + 2];
                        r[2] = this.WorkWaveBytes[i][(j * 4) + 1];
                        r[3] = this.WorkWaveBytes[i][j * 4];
                        int a = BitConverter.ToInt32(r, 0);
                        float tmp = a / 1048576.0f;

                        // float tmp = a / 2.0f;
                        this.WorkWaveFloats[i][j] = tmp;

                        // 听音数据处理
                        float f = this.WorkWaveFloats[i][j] * this.ListenCoefficent;
                        short sh;
                        if (f > 32767) {
                            sh = 32767;
                        }
                        else if (f <= -32767) {
                            sh = -32767;
                        }
                        else {
                            sh = (short)f;
                        }

                        var x = BitConverter.GetBytes(sh);
                        Array.Copy(x, 0, this.PlayWaveBytes[i], j * 2, 2);
                    }
                }

                int offset = ConstUdpArg.offsetArrayTwo[DisPlayWindow.Info.WorkChannel - 1];
                this.WorkWavefdatas = this.WorkWaveFloats[offset - 1];
                this.WorkEnergyEvent.Set();
                this.FreqWaveEvent.Set();

                PreGraphEventHandler?.Invoke(null, this.WorkWavefdatas);

                // if (BckGraphEventHandler != null) BckGraphEventHandler.Invoke(null, WorkWaveTwo);
                if (SoundEventHandler != null) {
                    int channel = DisPlayWindow.Info.WorkChannel;
                    if (channel > 0) {
                        SoundEventHandler.Invoke(null, this.PlayWaveBytes[channel]);
                    }
                }
            }
        }

        /// <summary>
        ///     The thread freq wave start.
        /// </summary>
        void ThreadFreqWaveStart() {
            while(true) {
                this.FreqWaveEvent.WaitOne();
                this.FreqWavefData = this.TransFormFft.FFT(this.WorkWavefdatas);
                var dataPoints = NewFFT.Start(this.WorkWavefdatas, 1024 * 16); // 用前1024 * 16个点

                FrapPointGraphEventHandler?.Invoke(null, dataPoints);
            }
        }
    }
}
