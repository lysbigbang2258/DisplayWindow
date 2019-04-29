using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArrayDisplay.Net;

namespace ArrayDisplay.DiscFile {
    public enum WaveData {
        WorkData,
        Origdata
    }

    public class DataFile : IDisposable {
        #region Field

        readonly string filepath;
        readonly int origLength;
        readonly ConcurrentQueue<byte[]> origRcvQueue;
        readonly AutoResetEvent origResetEvent;
        readonly ConcurrentQueue<byte[]> workRcvQueue;
        readonly AutoResetEvent workResetEvent;

        string filename;

        Thread oriThread;
        readonly int workLength;
        Thread workThread;

        #endregion

        #region Property

        public bool IsStartFlag {
            get;
            set;
        }

        #endregion

        #region Method

        public DataFile() {
            origRcvQueue = new ConcurrentQueue<byte[]>();
            workRcvQueue = new ConcurrentQueue<byte[]>();
            origResetEvent = new AutoResetEvent(false);
            workResetEvent = new AutoResetEvent(false);
            RelativeDirectory rd = new RelativeDirectory();
            origLength = ConstUdpArg.SAVE_ORIGPACK;
            workLength = ConstUdpArg.SAVE_WORKPACK;
            filepath = rd.Path + "\\" + "wavedata" + "\\";
            IsStartFlag = false;
        }

        /// <summary>
        ///     获取当前时间信息
        /// </summary>
        /// <returns></returns>
        static string SetNowTimeStr(WaveData wave) {
            DateTime dt = DateTime.Now;
            StringBuilder sb = new StringBuilder();
            sb.Append(wave == WaveData.WorkData ? "WorkFile_" : "OrigFile_");
            sb.Append(dt.Year.ToString("d4"));
            sb.Append("-");
            sb.Append(dt.Month.ToString("d2"));
            sb.Append("-");
            sb.Append(dt.Day.ToString("d2"));
            sb.Append("-");
            sb.Append(dt.Hour.ToString("d2"));
            sb.Append("-");
            sb.Append(dt.Minute.ToString("d2"));
            sb.Append("-");
            sb.Append(dt.Second.ToString("d2"));
            sb.Append("-");
            sb.Append(dt.Millisecond.ToString("d2"));
            sb.Append(".bin");
            string str = sb.ToString();
            return str;
        }

        /// <summary>
        ///     关闭数据保存
        /// </summary>
        public void DisableSaveFile() {
            if (oriThread != null && oriThread.IsAlive) {
                origResetEvent.Reset();
                oriThread.Abort();
            }
            if (workThread != null && workThread.IsAlive) {
                workThread.Abort();
            }
        }

        #region orig相关

        /// <summary>
        ///     线程处理：写入原始数据
        /// </summary>
        void Thread_OrigDataSave() {
            try {
                while(true) {
                    origResetEvent.WaitOne();
                    if (origRcvQueue.Count < origLength) {
                        continue;
                    }
                    try {
                        using(FileStream fsOrig = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write)) {
                            using(BinaryWriter brWork = new BinaryWriter(fsOrig)) {
                                byte[] temp;
                                for(int i = 0; i <= origLength; i++) {
                                    origRcvQueue.TryDequeue(out temp);
                                    if (temp != null) {
                                        brWork.Write(temp);
                                    }
                                }
                            }
                        }
                    }
                    catch(Exception e) {
                        Console.WriteLine(e);
                        throw;
                    }
                    origResetEvent.Set();
                }
            }
            catch(ThreadAbortException) {
                Thread.ResetAbort();
                if (!File.Exists(filename)) { return; }
                try {
                    using(FileStream fsOrig = new FileStream(filename, FileMode.Append, FileAccess.Write)) {
                        using(BinaryWriter brWork = new BinaryWriter(fsOrig)) {
                            byte[] temp;
                            for(int i = 0; i <= origRcvQueue.Count; i++) {
                                origRcvQueue.TryDequeue(out temp);
                                if (temp != null) {
                                    brWork.Write(temp);
                                }
                            }
                        }
                    }
                }
                catch(Exception e) {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        /// <summary>
        ///     开启原始数据保存
        /// </summary>
        public void EnableOrigSaveFile() {
            oriThread = new Thread(Thread_OrigDataSave) {IsBackground = true};
            oriThread.Start();
            UdpWaveData.OrigSaveDataEventHandler += WriteOrigData;
        }

        /// <summary>
        ///     实时获取原始数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        void WriteOrigData(object sender, byte[] data) {
            origRcvQueue.Enqueue(data);
            if (!IsStartFlag) {
                return;
            }
            string str = SetNowTimeStr(WaveData.Origdata);
            filename = filepath + str;
            origResetEvent.Set();
        }

        /// <summary>
        ///     回放数据
        /// </summary>
        /// <param name="fileList">文件列表</param>
        /// <param name="cancellationToken">取消信号控制</param>
        /// <returns>回放成功为true</returns>
        public bool ReplayData(List<string> fileList, CancellationTokenSource cancellationToken) {
            if (fileList == null || cancellationToken == null) {
                return false;
            }
            byte[] buffBytes;
            var bytesesList = new List<byte>(); //数据缓存
            CancellationToken tc = cancellationToken.Token;
            Task.Run(() => {
                         //获取数据流保存于列表
                         foreach(string file in fileList) {
                             using(FileStream fsOrig = new FileStream(file, FileMode.Open, FileAccess.Read)) {
                                 int readlen = (int) fsOrig.Length;
                                 buffBytes = new byte[readlen];
                                 fsOrig.ReadAsync(buffBytes, 0, readlen);
                                 bytesesList.AddRange(buffBytes);
                             }
                         }
                         var floatlist = Floatlist(bytesesList, WaveData.Origdata);
                         var splitList = SplitList(floatlist, 400); //切分数据为多个包，每包长度为400

                         for(int index = 0; index < splitList.Count && !tc.IsCancellationRequested; index++) {
                             var list = splitList[index];
                             var sendfloats = list.ToArray();
                             Dataproc.OrigGraphEventHandler.Invoke(null, sendfloats);
                             Thread.Sleep(200);
                         }
                     }, tc);
            return true;
        }

        /// <summary>
        /// 读取字节数据转换为float数据
        /// </summary>
        /// <param name="bytesesList">数据源</param>
        /// <param name="waveData"></param>
        /// <returns>解码数据</returns>
        List<float> Floatlist(List<byte> bytesesList,WaveData waveData) {
            var sourceBytes = bytesesList.ToArray();
            var floatlist = new List<float>();
            if (waveData == WaveData.Origdata) {
                var tmp = new byte[2];
                
                for (int i = 0; i < (sourceBytes.Length / 2 - 1); i++)
                {
                    tmp[0] = sourceBytes[i * 2 + 1];
                    tmp[1] = sourceBytes[i * 2];
                    short a = BitConverter.ToInt16(tmp, 0);
                    floatlist.Add(a / 8192.0f);
                }
            }
            else {
                var r = new byte[4];
                for (int j = 0; j < sourceBytes.Length/4; j++)
                {
                    r[0] = sourceBytes[j * 4 + 3];
                    r[1] = sourceBytes[j * 4 + 2];
                    r[2] = sourceBytes[j * 4 + 1];
                    r[3] = sourceBytes[j * 4];
                    int a = BitConverter.ToInt32(r, 0);
                    float tmp = a / 1048576.0f;
                    //                        float tmp = a / 2.0f;
                    floatlist.Add(tmp);
                }
            }
            return floatlist;
        }

        /// <summary>
        ///     切分列表
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="list">列表</param>
        /// <param name="size">列表单位大小</param>
        /// <returns>切分后的二维列表</returns>
        public List<List<T>> SplitList<T>(List<T> list, int size) {
            var result = new List<List<T>>();
            for(int i = 0; i < (list.Count() / size); i++) {
                var clist = new T[size];
                list.CopyTo(i * size, clist, 0, size);
                result.Add(clist.ToList());
            }

            int r = list.Count() % size;
            if (r != 0) {
                var cclist = new T[r];
                list.CopyTo(list.Count() - r, cclist, 0, r);
                result.Add(cclist.ToList());
            }

            return result;
        }

        #endregion

        #region work相关

        void Thread_WorkDataSave() {
            try {
                while(true) {
                    workResetEvent.WaitOne();
                    if (workRcvQueue.Count < workLength) {
                        continue;
                    }
                    try {
                        using(FileStream fsOrig = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write)) {
                            using(BinaryWriter brWork = new BinaryWriter(fsOrig)) {
                                byte[] temp;
                                for(int i = 0; i <= workLength; i++) {
                                    workRcvQueue.TryDequeue(out temp);
                                    if (temp != null) {
                                        brWork.Write(temp);
                                    }
                                }
                            }
                        }
                    }
                    catch(Exception e) {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
            catch(ThreadAbortException) {
                Thread.ResetAbort();
                if (!File.Exists(filename)) { return; }
                try {
                    using(FileStream fsOrig = new FileStream(filename, FileMode.Append, FileAccess.Write)) {
                        using(BinaryWriter brWork = new BinaryWriter(fsOrig)) {
                            byte[] temp;
                            for(int i = 0; i <= workRcvQueue.Count; i++) {
                                workRcvQueue.TryDequeue(out temp);
                                if (temp != null) {
                                    brWork.Write(temp);
                                }
                            }
                        }
                    }
                }
                catch(Exception e) {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        /// <summary>
        ///     开启工作数据保存
        /// </summary>
        public void EnableWorkSaveFile() {
            workThread = new Thread(Thread_WorkDataSave) {IsBackground = true};
            workThread.Start();
            UdpWaveData.WorkSaveDataEventHandler += WriteWorkData;
        }

        /// <summary>
        ///     写入工作数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        void WriteWorkData(object sender, byte[] data) {
            workRcvQueue.Enqueue(data);
            if (!IsStartFlag) {
                return;
            }
            string str = SetNowTimeStr(WaveData.WorkData);
            filename = filepath + str;
            workResetEvent.Set();
        }

        #endregion

   

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose() {
            origResetEvent?.Dispose();
            workResetEvent?.Dispose();
        }

        #endregion
    }
}
