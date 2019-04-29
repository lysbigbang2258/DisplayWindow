// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UdpWaveData.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the UdpWaveData type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ArrayDisplay.Net {
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Windows;
    using ArrayDisplay.UI;

    /// <summary>
    /// The udp wave data.
    /// </summary>
    public class UdpWaveData : IDisposable {
        /// <summary>
        /// Initializes a new instance of the <see cref="UdpWaveData"/> class.
        /// </summary>
        public UdpWaveData() {
            try {
                this.waveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                this.StartRcvEvent = new AutoResetEvent(false);
                this.IsStopRcved = false;
            }
            catch(Exception e) {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// The start receive data.
        /// </summary>
        /// <param name="ip">
        /// The ip.
        /// </param>
        public void StartReceiveData(IPEndPoint ip) {
            this.Ip = ip;
            try {
                this.waveSocket.Bind(ip);
                this.IsBuilded = true;
            }
            catch(Exception e) {
                Console.WriteLine(@"创建UDP失败...错误为{0}", e);
                MessageBox.Show(@"创建UDP失败...");
            }

            if (string.Equals(ip, ConstUdpArg.Src_NormWaveIp)) {
                this.WorktInit();
                this.WaveType = ConstUdpArg.WaveType.Normal;
            }
            else if (Equals(ip, ConstUdpArg.Src_OrigWaveIp)) {
                this.OrigInit();
                this.WaveType = ConstUdpArg.WaveType.Orig;
            }
            else if (Equals(ip, ConstUdpArg.Src_DelayWaveIp)) {
                this.DelayInit();
                this.WaveType = ConstUdpArg.WaveType.Delay;
            }

            this.WaveDataproc = new Dataproc();
            this.WaveDataproc.Init(this.WaveType);
            this.RcvThread.Start();
        }

        /// <summary>
        /// The orig init.
        /// </summary>
        void OrigInit() {
            this.waveSocket.ReceiveBufferSize = ConstUdpArg.ORIG_FRAME_LENGTH * ConstUdpArg.ORIG_FRAME_NUMS * 2;
            FrameNums = DisPlayWindow.Info.OrigFramNums;
            this.rcvBuf = new byte[ConstUdpArg.ORIG_FRAME_LENGTH];
            this.RcvThread = new Thread(this.OrigThreadStart) {IsBackground = true, Priority = ThreadPriority.Highest, Name = "Orig"};
        }

        void DelayInit() {
            this.waveSocket.ReceiveBufferSize = ConstUdpArg.DELAY_FRAME_NUMS * ConstUdpArg.DELAY_FRAME_LENGTH * 2;
            FrameNums = ConstUdpArg.DELAY_FRAME_NUMS;
            this.rcvBuf = new byte[ConstUdpArg.DELAY_FRAME_LENGTH];
            this.RcvThread = new Thread(this.DelayThreadStart) {IsBackground = true, Priority = ThreadPriority.Highest, Name = "Delay"};
        }

        void WorktInit() {
            this.waveSocket.ReceiveBufferSize = ConstUdpArg.WORK_FRAME_LENGTH * ConstUdpArg.WORK_FRAME_NUMS * 2;
            FrameNums = ConstUdpArg.WORK_FRAME_NUMS;
            this.rcvBuf = new byte[ConstUdpArg.WORK_FRAME_LENGTH * 2];
            this.RcvThread = new Thread(this.NormalThreadStart) {IsBackground = true, Priority = ThreadPriority.Highest, Name = "WorkWave"};
        }

        void DelayThreadStart() {
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = remote;
            Console.WriteLine(@"启动UDP线程...");

            while(true) {
                while(true) {
                    this.StartRcvEvent.WaitOne();

                    int index = 0;
                    this.IsRcving = true;
                    while(index < FrameNums) {
                        if (this.waveSocket == null) {
                            this.IsRcving = false;
                            break;
                        }

                        int offset = 0;
                        try {
                            // 接收数据
                            int ret = this.waveSocket.ReceiveFrom(this.rcvBuf, offset, this.rcvBuf.Length - offset, SocketFlags.None, ref senderRemote);
                        }
                        catch(Exception e) {
                            Console.WriteLine(e);
                            this.IsRcving = false;
                            break;
                        }

                        switch(this.WaveType) {
                            case ConstUdpArg.WaveType.Normal:
                                if (WorkSaveDataEventHandler != null) {
                                    WorkSaveDataEventHandler(null, this.rcvBuf);
                                }

                                this.PutWorkData(this.rcvBuf, index++);
                                if (index >= FrameNums) {
                                    index = 0;
                                }

                                this.WaveDataproc.WorkBytesEvent.Set();
                                break;
                            case ConstUdpArg.WaveType.Orig:
                                this.PutOrigData(this.rcvBuf);
                                this.WaveDataproc.OrigBytesEvent.Set();
                                index++;
                                index++;
                                if (index >= FrameNums) {
                                    index = 0;
                                }

                                break;
                            case ConstUdpArg.WaveType.Delay:
                                this.PutDelayData(this.rcvBuf);
                                index++;
                                if (index >= FrameNums) {
                                    index = 0;
                                }

                                this.WaveDataproc.DelayBytesEvent.Set();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
        }

        void OrigThreadStart() {
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = remote;
            Console.WriteLine(@"启动UDP线程...");
            while(true) {
                while(true) {
                    this.StartRcvEvent.WaitOne();
                    int index = 0;
                    this.IsRcving = true;
                    
                    while(index < FrameNums) {

                        if (this.waveSocket == null) {
                            this.IsRcving = false;
                            break;
                        }

                        int offset = 0;
                        try {
                            // 接收数据                         
                                int ret = this.waveSocket.ReceiveFrom(this.rcvBuf, offset, this.rcvBuf.Length - offset, SocketFlags.None, ref senderRemote);
                        }
                        catch(Exception e) {
                            Console.WriteLine(e);
                            this.IsRcving = false;
                            break;
                        }

                        switch(this.WaveType) {
                            case ConstUdpArg.WaveType.Normal:
                                if (WorkSaveDataEventHandler != null) {
                                    WorkSaveDataEventHandler(null, this.rcvBuf);
                                }

                                this.PutWorkData(this.rcvBuf, index++);
                                if (index >= FrameNums) {
                                    index = 0;
                                }

                                this.WaveDataproc.WorkBytesEvent.Set();
                                break;
                            case ConstUdpArg.WaveType.Orig:
                                this.PutOrigData(this.rcvBuf);
                                this.WaveDataproc.OrigBytesEvent.Set();
                                index++;
                                index++;
                                if (index >= FrameNums) {
                                    index = 0;
                                }

                                break;
                            case ConstUdpArg.WaveType.Delay:
                                this.PutDelayData(this.rcvBuf);
                                index++;
                                if (index >= FrameNums) {
                                    index = 0;
                                }

                                this.WaveDataproc.DelayBytesEvent.Set();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
        }

        void NormalThreadStart() {
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = remote;
            Console.WriteLine(@"启动UDP线程...");
            while(true) {
                this.StartRcvEvent.WaitOne();
                if (this.IsStopRcved) {
                    return;
                }

                if (this.ExitFlag)
                {
                    this.ExitFlag = false;
                    return;
                }

                int index = 0;
                this.IsRcving = true;
                while(index < FrameNums) {
                    if (this.waveSocket == null) {
                        this.IsRcving = false;
                        break;
                    }

                    int offset = 0;
                    try {
                        int ret = this.waveSocket.ReceiveFrom(this.rcvBuf, offset, this.rcvBuf.Length - offset, SocketFlags.None, ref senderRemote);
                        if (!Equals(senderRemote, ConstUdpArg.Dst_NormWaveIp)) {
                            Console.WriteLine("接收错误");
                            Console.WriteLine(senderRemote.ToString());
                            continue;
                        }
                    }
                    catch(Exception e) {
                        Console.WriteLine(e);
                        break;
                    }

                    if (WorkSaveDataEventHandler != null) {
                        WorkSaveDataEventHandler(null, this.rcvBuf);
                    }

                    this.PutWorkData(this.rcvBuf, index++);
                    if (index >= FrameNums) {
                        index = 0;
                    }

                    this.WaveDataproc.WorkBytesEvent.Set();
                }
            }
        }

        void PutDelayData(byte[] buf) {
            var temp = new byte[400];
            var head = new byte[2];

            int offset = 0;
            if (!Equals(this.Ip, ConstUdpArg.Src_DelayWaveIp)) {
                return;
            }

            Array.Copy(buf, 0, head, 0, head.Length);
            int channel = head[1];
            if (channel > 7 || channel < 0) {
                return;
            }

            offset += head.Length;
            Array.Copy(buf, offset, temp, 0, temp.Length);
            if (this.delayChannelOffsets[channel] >= this.WaveDataproc.DelayWaveBytes[0].Length) {
                this.delayChannelOffsets[channel] = 0;
            }

            Array.Copy(temp, 0, this.WaveDataproc.DelayWaveBytes[channel], this.delayChannelOffsets[channel], temp.Length);
            this.delayChannelOffsets[channel] += temp.Length;
        }

        /// <summary>
        ///     将原始数据保存到对应通道
        ///     发送数据到保存线程
        /// </summary>
        /// <param name="buf"></param>
        void PutOrigData(byte[] buf) {
            var head = new byte[2];
            int offset = 0;
            if (!Equals(this.Ip, ConstUdpArg.Src_OrigWaveIp)) {
                return;
            }

            Array.Copy(buf, 0, head, 0, head.Length);
            int channel = head[0];
            int timdiv = head[1];
            if (channel < 0 && channel > ConstUdpArg.ORIG_CHANNEL_NUMS) {
                channel = 1;
            }

            if (timdiv < 0 && timdiv > ConstUdpArg.ORIG_TIME_NUMS) {
                timdiv = 1;
            }

            offset += head.Length;

            int len = this.origChannelOffsets[channel  + timdiv * 8];// 写入数据偏移长度
            if (len >= this.WaveDataproc.OrigWaveBytes[0].Length) { // 大于容量数据覆盖，从头再写入
                this.origChannelOffsets[channel + timdiv * 8] = 0;
                len = 0;
            }

            Array.Copy(buf, offset, this.WaveDataproc.OrigWaveBytes[channel + timdiv* 8 ], len, buf.Length - 2);

            var data = new byte[buf.Length - 2];
            Array.Copy(buf, offset, data, 0, buf.Length - 2);
            this.origChannelOffsets[channel * 8 + timdiv] += data.Length;
            if (OrigSaveDataEventHandler != null) {
                // 发送给保存线程
                OrigSaveDataEventHandler(null, data);
            }
        }

        /// <summary>
        ///     将WorkWave数据导入Detect_Bytes
        /// </summary>
        /// <param name="buf">一帧数据，长度为1024，每4位表示1个探头数据</param>
        /// <param name="index">帧数</param>
        void PutWorkData(byte[] buf, int index) {
            var temp = new byte[4];
            if (!Equals(this.Ip, ConstUdpArg.Src_NormWaveIp)) {
                return;
            }

            var dataBytes = new byte[256];
            Array.Copy(buf, dataBytes, dataBytes.Length);
            for(int i = 0; i < (dataBytes.Length / 4); i++) {
                Array.Copy(dataBytes, i * 4, temp, 0, temp.Length);
                Array.Copy(temp, 0, this.WaveDataproc.WorkWaveBytes[i], index * 4, temp.Length);
            }
        }

        #region Field

        static readonly LinkedList<Array> linkbuffer = new LinkedList<Array>(); // 缓存数据buff
        public static int FrameNums; // 一帧数据长度
        public static ConstUdpArg.WaveType waveType; // 波形数据类型
        readonly int[] delayChannelOffsets = new int[8];
        readonly int[] origChannelOffsets = new int[64];
        byte[] rcvBuf; // 接收数据缓存
        readonly Socket waveSocket;
        volatile bool  exitFlag = false;

        

        #endregion

        #region 属性
        

        public bool IsBuilded {
            get;
            set;
        }

        public bool IsRcving {
            get;
            set;
        }

        public AutoResetEvent StartRcvEvent {
            get;
            set;
        }

        public Thread RcvThread {
            get;
            private set;
        }

        public IPEndPoint Ip {
            get;
            private set;
        }

        public bool IsStopRcved {
            get;
            set;
        }

        /// <summary>
        ///     正常工作数据保存方法
        /// </summary>
        public static EventHandler<byte[]> WorkSaveDataEventHandler {
            get;
            set;
        }

        /// <summary>
        ///     原始数据保存方法
        /// </summary>
        public static EventHandler<byte[]> OrigSaveDataEventHandler {
            get;
            set;
        }

        /// <summary>
        ///     正在传输波形
        /// </summary>
        public ConstUdpArg.WaveType WaveType {
            get {
                return waveType;
            }

            set {
                waveType = value;
            }
        }

        /// <summary>
        /// 标志位
        /// </summary>
        public bool ExitFlag { get => this.exitFlag; set => this.exitFlag = value; }

        /// <summary>
        /// 数据处理线程
        /// </summary>
        public Dataproc WaveDataproc
        {
            get;
            private set;
        }

        #endregion

        #region IDisposable

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (this.waveSocket != null) {
                    this.waveSocket.Shutdown(SocketShutdown.Both);
                    this.waveSocket.Close();
                    this.waveSocket.Dispose();
                }

                if (this.WaveDataproc != null) {
                    this.WaveDataproc.Dispose();
                }

                if (this.StartRcvEvent != null) {
                    this.StartRcvEvent.Dispose();
                }

                if (this.StartRcvEvent != null) {
                    this.StartRcvEvent.Dispose();
                }

                if (this.RcvThread != null) {
                    this.RcvThread.Abort();
                    this.RcvThread = null;
                }

                this.IsBuilded = false;
                this.IsStopRcved = true;
                linkbuffer.Clear();
                Console.WriteLine(@"关闭UDP线程...");
            }
        }

        /// <inheritdoc />
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
