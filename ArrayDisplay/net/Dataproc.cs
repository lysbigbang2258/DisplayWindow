﻿using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using ArrayDisplay.UI;

namespace ArrayDisplay.net {
    /// <summary>
    ///     处理与分发网络数据
    /// </summary>
    public class Dataproc : IDisposable {
        public Dataproc() {
            TransFormFft = new FFT_TransForm();
            int arryNum = ConstUdpArg.ARRAY_NUM; //探头个数
            WorkWaveBytes = new byte[arryNum][];
            WorkWaveFloats = new float[arryNum][];
            WorkWaveBytes = new byte[arryNum][];
            PlayWaveBytes = new byte[arryNum][];
            int frameNum = ConstUdpArg.WORK_FRAME_NUMS; //工作数据帧数
            for(int i = 0; i < arryNum; i++) {
                WorkWaveBytes[i] = new byte[frameNum * 4]; //避免数据为null
                WorkWaveFloats[i] = new float[frameNum]; //避免数据为null 
                PlayWaveBytes[i] = new byte[frameNum * 2]; //避免数据为null 
            }
            WorkWavefdatas = new float[frameNum]; //工作波形
            EnergyFloats = new float[arryNum];
            ListenCoefficent = (float) Math.Pow(10, 50 / 20.0F) * 100; //31622.78; //听音强度


            int oNums = ConstUdpArg.ORIG_DETECT_LENGTH; //时分长度
            OrigWaveBytes = new byte[oNums][];
            OrigWaveFloats = new float[oNums][];
            OrigChannnel = 0;
            OrigTimeDiv = 0;
           
            int oFrime = DisPlayWindow.SelectdInfo.OrigFramNums; //原始数据帧数
            Origdata = new byte[oFrime * arryNum];
            int olength = ConstUdpArg.ORIG_FRAME_LENGTH-2; //每帧长度
            for (int i = 0; i < oNums; i++)
            {
                OrigWaveBytes[i] = new byte[olength * oFrime];
                OrigWaveFloats[i] = new float[olength / 2 * oFrime];
                
            }
            int dchannels = ConstUdpArg.DELAY_FRAME_CHANNELS;
            int dframeLen = ConstUdpArg.DELAY_FRAME_LENGTH-2;
            int dframeNum = ConstUdpArg.DELAY_FRAME_NUMS;
            DelayWaveBytes = new byte[dchannels][];
            DelayWaveFloats = new float[dchannels][];
            for(int i = 0; i < dchannels; i++) {
                DelayWaveBytes[i] = new byte[dframeLen * dframeNum];
                DelayWaveFloats[i] = new float[dframeLen * dframeNum / 2];
            }

            FreqWaveEvent = new AutoResetEvent(false);
            WorkEnergyEvent = new AutoResetEvent(false);
            DelayBytesEvent = new AutoResetEvent(false);
            OrigBytesEvent = new AutoResetEvent(false);
            BvalueBytesEvent = new AutoResetEvent(false);
            WorkBytesEvent = new AutoResetEvent(false);

            IsRcvChanneldata = new int[64]; //8时分8空分
            new Thread(ThreadOrigWaveStart) {IsBackground = true}.Start();
            new Thread(ThreadWorkWaveStart) {IsBackground = true}.Start();
            new Thread(ThreadEnergyStart) {IsBackground = true}.Start();
            new Thread(ThreadDelayWaveStart) {IsBackground = true}.Start();
            new Thread(ThreadFreqWaveStart) {IsBackground = true}.Start();

            IsBavlueReaded = false;
           
        }
        public struct BvalueSturct
        {
          public  bool isRead;
          public  float value;
            public BvalueSturct(bool isRead, float value) : this() {
                this.isRead = isRead;
                this.value = value;
            }
        }
      

        public static AutoResetEvent OrigBvalueEvent {
            get;
            set;
        }

        /// <summary>
        ///     线程处理函数：延时数据处理
        /// </summary>
        void ThreadDelayWaveStart() {
            while(true) {
                DelayBytesEvent.WaitOne();
                var r = new byte[2];

                for(int i = 0; i < DelayWaveBytes.Length; i++) {
                    for(int j = 0; j < (DelayWaveBytes[0].Length / 2); j++) {
                        r[0] = DelayWaveBytes[i][j * 2 + 1];
                        r[1] = DelayWaveBytes[i][j * 2 + 0];
                        short a = BitConverter.ToInt16(r, 0);
                        DelayWaveFloats[i][j] = a / 8192.0f;

                        //                        OrigWave_Floats[i][j] = a / 104800.0f;
                    }
                }
                DelayGraphEventHandler(null, DelayWaveFloats);
            }
        }
        /// <summary>
        /// 线程处理函数：原始数据处理
        /// </summary>
        void ThreadOrigWaveStart() {
            while(true) {
                OrigBytesEvent.WaitOne();
                var r = new byte[2];
                for (int i = 0; i < ConstUdpArg.ORIG_DETECT_LENGTH; i++)
                {
                    for (int j = 0; j < OrigWaveBytes[0].Length / 2 - 1; j++)
                    {
                        r[0] = OrigWaveBytes[i][j * 2 + 1];
                        r[1] = OrigWaveBytes[i][j * 2];
                        short a = BitConverter.ToInt16(r, 0);

                        OrigWaveFloats[i][j] = a / 8192.0f;
                    }
                }
                if (OrigGraphEventHandler != null)
                {
                    OrigGraphEventHandler.Invoke(null, OrigWaveFloats);
                }
            }
        }

        

        /// <summary>
        ///线程处理函数：能量数据处理
        /// </summary>
        void ThreadEnergyStart() {
            while(true) {
                WorkEnergyEvent.WaitOne();
                double ftemp = 0;
                for(int i = 0; i < WorkWaveFloats.Length; i++) {
                    for(int j = 0; j < WorkWaveFloats[0].Length; j++) {
                        float f = WorkWaveFloats[i][j];
                        ftemp = Math.Abs(f);
                    }
                    EnergyFloats[i] = (float) ftemp;
                }
                float max = EnergyFloats.Max();
                for(int i = 0; i < EnergyFloats.Length; i++) {
                    if (Math.Abs(max) > float.Epsilon) {
                        EnergyFloats[i] = EnergyFloats[i] / max;
                    }
                    else {
                        EnergyFloats[i] = 0;
                    }
                    
                }
                if (EnergyArrayEventHandler != null) {
                    EnergyArrayEventHandler.Invoke(null, EnergyFloats);
                }
            }
        }

        /// <summary>
        /// 线程函数处理：正常工作数据处理
        /// </summary>
        void ThreadWorkWaveStart() {
            while(true) {
                WorkBytesEvent.WaitOne();
                var r = new byte[4];
                for(int i = 0; i < ConstUdpArg.ARRAY_NUM; i++) {
                    for(int j = 0; j < ConstUdpArg.WORK_FRAME_NUMS; j++) {
                        r[0] = WorkWaveBytes[i][j * 4 + 3];
                        r[1] = WorkWaveBytes[i][j * 4 + 2];
                        r[2] = WorkWaveBytes[i][j * 4 + 1];
                        r[3] = WorkWaveBytes[i][j * 4];
                        int a = BitConverter.ToInt32(r, 0);
                        WorkWaveFloats[i][j] = a / 1048576.0f;

                        //听音数据处理
                        float f = WorkWaveFloats[i][j] * ListenCoefficent;
                        short sh;
                        if (f > 32767) {
                            sh = 32767;
                        }
                        else if (f <= -32767) {
                            sh = -32767;
                        }
                        else {
                            sh = (short) f;
                        }
                        var x = BitConverter.GetBytes(sh);
                        Array.Copy(x, 0, PlayWaveBytes[i], j * 2, 2);
                    }
                }
                var offset = ConstUdpArg.offsetArray[DisPlayWindow.SelectdInfo.WorkWaveChannel];
                WorkWavefdatas = WorkWaveFloats[offset];
                //            WorkWaveTwo = WorkWaveFloats[DisPlayWindow.onSelectdInfo.WorkWaveChannel+1];

                if (PreGraphEventHandler != null)
                {
                    PreGraphEventHandler.Invoke(null, WorkWavefdatas);
                }
                //            if (BckGraphEventHandler != null) BckGraphEventHandler.Invoke(null, WorkWaveTwo);

                if (SoundEventHandler != null)
                {
                    int channel = DisPlayWindow.SelectdInfo.WorkWaveChannel;
                    if (channel > 0)
                    {
                        SoundEventHandler.Invoke(null, PlayWaveBytes[channel]);
                    }
                }
                WorkEnergyEvent.Set();
                FreqWaveEvent.Set();

                //求初始相位
//                double[] phaseDoubles = Task<double[]>.Factory.StartNew(GetInitPhase).Result;
//                foreach (double phaseDouble in phaseDoubles)
//                {
//                    Console.WriteLine(phaseDouble);
//                }
            }
        }

        /// <summary>
        /// 线程处理函数：正常工作数据处理
        /// </summary>
        void ThreadFreqWaveStart() {
            while(true) {
                FreqWaveEvent.WaitOne();
                FreqWaveOne = TransFormFft.FFT(WorkWavefdatas);
                var dataPoints = NewFFT.Start(WorkWavefdatas);
//                if (FrapGraphEventHandler != null) {
//                    FrapGraphEventHandler.Invoke(null, FreqWaveOne);
//                }
                if (FrapPointGraphEventHandler != null) {
                    FrapPointGraphEventHandler(null, dataPoints);   
                }
            }
        }

//        /// <summary>
//        /// 计算初始相位（基于原始数据）
//        /// </summary>
//        /// <returns></returns>
//        double[] GetInitPhase() {
//            float[][] phaseFloats = new float[OrigWaveFloats.Length][];
//            double[][] cosDoubles = new double[phaseFloats.Length][];
//            double[][] sinDoubles = new double[phaseFloats.Length][];
//            for(int i = 0; i < phaseFloats.Length; i++) {
//                phaseFloats[i] = new float[OrigWaveFloats[i].Length];
//                cosDoubles[i] = new double[OrigWaveFloats[i].Length];
//                sinDoubles[i] = new double[OrigWaveFloats[i].Length];
//            }
//            int n = OrigWaveFloats[0].Length; //一帧数据
//            try {
//                for(int i = 0; i < phaseFloats.Length; i++) {
//                    Array.Copy(OrigWaveFloats[i], 0, phaseFloats[i], 0, n);
//                }
//            }
//            catch(Exception e) {
//                Console.WriteLine(e);
//                throw;
//            }
//                        
//            for(int i = 0; i < sinDoubles.Length; i++) {
//                for(int j = 0; j < sinDoubles[i].Length; j++) {
//                    cosDoubles[i][j] = phaseFloats[i][j] * Math.Cos(2 * Math.PI *31.25 * 1000 *j / n);   //31.25Khz
//                    sinDoubles[i][j] = phaseFloats[i][j] * Math.Sin(2 * Math.PI *31.25 * 1000 *j / n);
//                }
//            }
//            double[] phase = new double[phaseFloats.Length];
//            for(int i = 0; i < phase.Length; i++) {
//                phase[i] =(Math.PI - Math.Atan(sinDoubles[i].Average() / cosDoubles[i].Average()))*Math.PI/180;
//            }
//            return phase;
//        }

        /// <summary>
        ///     计算初始相位（工作波形）
        /// </summary>
        /// <returns></returns>
        double[] GetInitPhase() {
            var phaseFloats = new float[WorkWaveFloats.Length][];
            var cosDoubles = new double[phaseFloats.Length][];
            var sinDoubles = new double[phaseFloats.Length][];
            for(int i = 0; i < phaseFloats.Length; i++) {
                phaseFloats[i] = new float[WorkWaveFloats[i].Length];
                cosDoubles[i] = new double[WorkWaveFloats[i].Length];
                sinDoubles[i] = new double[WorkWaveFloats[i].Length];
            }
            int n = WorkWaveFloats[0].Length; //一帧数据
            try {
                for(int i = 0; i < phaseFloats.Length; i++) {
                    Array.Copy(WorkWaveFloats[i], 0, phaseFloats[i], 0, n);
                }
            }
            catch(Exception e) {
                Console.WriteLine(e);
                throw;
            }

            for(int i = 0; i < sinDoubles.Length; i++) {
                for(int j = 0; j < sinDoubles[i].Length; j++) {
                    cosDoubles[i][j] = phaseFloats[i][j] * Math.Cos(2 * Math.PI * 31.25 * 1000 * j / n); //31.25Khz
                    sinDoubles[i][j] = phaseFloats[i][j] * Math.Sin(2 * Math.PI * 31.25 * 1000 * j / n);
                }
            }
            var phase = new double[phaseFloats.Length];
            for(int i = 0; i < phase.Length; i++) {
                phase[i] = (Math.PI - Math.Atan(sinDoubles[i].Average() / cosDoubles[i].Average())) * Math.PI / 180;
            }
            return phase;
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                if (DelayBytesEvent != null) {
                    DelayBytesEvent.Dispose();
                }
                if (FreqWaveEvent != null) {
                    FreqWaveEvent.Dispose();
                }
                if (OrigBytesEvent != null) {
                    OrigBytesEvent.Dispose();
                }
                if (WorkBytesEvent != null) {
                    WorkBytesEvent.Dispose();
                }
                if (WorkEnergyEvent != null) {
                    WorkEnergyEvent.Dispose();
                }
            }
        }

        /// <inheritdoc />
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region 变量

        /// <summary>
        ///     事件通知
        /// </summary>
        public AutoResetEvent WorkBytesEvent {
            get;
            set;
        }

        public AutoResetEvent OrigBytesEvent {
            get;
            set;
        }
        public AutoResetEvent BvalueBytesEvent
        {
            get;
            set;
        }

        public AutoResetEvent DelayBytesEvent {
            get;
            set;
        }

        public AutoResetEvent WorkEnergyEvent {
            get;
            set;
        }

        public AutoResetEvent FreqWaveEvent {
            get;
            set;
        }

        /// <summary>
        ///     事件句柄
        /// </summary>
        public static EventHandler<byte[]> SoundEventHandler {
            get;
            set;
        }

        public static EventHandler<float[]> PreGraphEventHandler {
            get;
            set;
        }

        public static EventHandler<float[]> BckGraphEventHandler {
            get;
            set;
        }

        public static EventHandler<float[]> EnergyArrayEventHandler {
            get;
            set;
        }

        public static EventHandler<float[][]> OrigGraphEventHandler {
            get;
            set;
        }

        public static EventHandler<float[]> FrapGraphEventHandler {
            get;
            set;
        }

        public static EventHandler<Point[]> FrapPointGraphEventHandler {
            get;
            set;
        }

        public static EventHandler<float[][]> DelayGraphEventHandler {
            get;
            set;
        }

        /// <summary>
        ///     外界数据
        /// </summary>
        public int OrigChannnel {
            get;
            set;
        }
         public int OrigTimeDiv
        {
            get;
            set;
        }
        public byte[][] DelayWaveBytes {
            get;
            set;
        }

        public byte[][] OrigWaveBytes {
            get;
            set;
        }

        public byte[][] WorkWaveBytes {
            get;
            set;
        }

        /// <summary>
        ///     内部数据
        /// </summary>
        float[] WorkWavefdatas {
            get;
            set;
        }

        float[] WorkWaveTwo {
            get;
            set;
        }

        float[] FreqWaveOne {
            get;
            set;
        }

        float[][] DelayWaveFloats {
            get;
            set;
        }

        

        FFT_TransForm TransFormFft {
            get;
            set;
        }

        byte[] Origdata {
            get;
            set;
        }

        float[][] WorkWaveFloats {
            get;
            set;
        }

        /// <summary>
        ///     能量图数据
        /// </summary>
        float[] EnergyFloats {
            get;
            set;
        }

        float[][] OrigWaveFloats {
            get;
            set;
        }

        byte[][] PlayWaveBytes {
            get;
            set;
        }

        //听音系数
        float ListenCoefficent {
            get;
            set;
        }

        public static bool IsBavlueReaded {
            get;
            set;
        }

        public int[] IsRcvChanneldata {
            get;
            set;
        }

        #endregion
    }
}
