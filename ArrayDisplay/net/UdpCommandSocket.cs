// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UdpCommandSocket.cs" company="">
// </copyright>
// <summary>
//   Defines the UdpCommandSocket type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ArrayDisplay.Net {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using ArrayDisplay.UI;

    /// <summary>
    /// The udp command socket.
    /// </summary>
    public sealed class UdpCommandSocket : IDisposable {
        /// <summary>
        /// Initializes a new instance of the <see cref="UdpCommandSocket"/> class.
        /// </summary>
        public UdpCommandSocket() {
            this.rcvsocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.rcvsocket.ReceiveTimeout = 2000;
            this.sedsocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.sedsocket.SendTimeout = 1000;
            this.sedsocket.ReceiveTimeout = 1000;
            this.Init();
        }

        public void RcvThreadStart() {
            Thread thread = new Thread(this.Thread_Rcv);
            thread.Start();
            thread.IsBackground = true;
            
        }

        void Thread_Rcv() {
            EndPoint senderRemote = new IPEndPoint(IPAddress.Any, 0);
            var rcvUdpBuffer = new byte[100 * 4];
            int reflag = 0;
            while(true) {
                int offset = 0;
                try
                {
                    reflag = this.rcvsocket.ReceiveFrom(rcvUdpBuffer, offset, rcvUdpBuffer.Length - offset, SocketFlags.None, ref senderRemote);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
            }
        }

        /// <summary>
        ///     初始化Socket，绑定IP
        /// </summary>
        public void Init() {
            try {
                this.SedIp = ConstUdpArg.Src_ComMsgIp;
                this.RcvIp = ConstUdpArg.Src_ComDatIp;
                this.rcvsocket.Bind(this.RcvIp);
                this.sedsocket.Bind(this.SedIp);
                this.TestConnect();
            }
            catch(Exception e) {
                Console.WriteLine(@"创建UDP失败...错误为{0}", e);
                MessageBox.Show(@"创建UDP失败...");
            }
           
        }

        public void TestConnect() {
            this.sedsocket.SendTo(ConstUdpArg.SwicthToOriginalWindow, ConstUdpArg.Dst_ComMsgIp);
            var testBuffer = new byte[18];
            EndPoint testRemote = new IPEndPoint(IPAddress.Any, 0);
            int offset = 0;
            try
            {
                int ret = this.sedsocket.ReceiveFrom(testBuffer, offset, testBuffer.Length - offset, SocketFlags.None, ref testRemote);
                this.IsSocketConnect = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(@"网络连接失败");
                this.IsSocketConnect = false;
            }
        }

        /// <summary>///切换功能窗口（波形或命令）/// </summary>
        public void SwitchWindow(byte[] cmdBytes) {
            var tempBytes = new byte[8];
            cmdBytes.CopyTo(tempBytes, 0);
            this.Send(tempBytes, ConstUdpArg.Dst_ComMsgIp);
        }

        #region 数据发送

        /// <summary>
        ///     发送数据
        /// </summary>
        /// <param name="bytes">待发送byte[]数据</param>
        /// <param name="endPoint">下位机IP与端口号</param>
        /// <returns></returns>
        public void Send(byte[] bytes, IPEndPoint endPoint) {
            if (this.IsProcess || !this.IsSocketConnect) {
                return;
            }

            try {
                this.IsProcess = true;
                var sendbytes = new byte[bytes.Length];
                bytes.CopyTo(sendbytes, 0);
                IPEndPoint sendip = endPoint;
                EndPoint senderRemote = new IPEndPoint(IPAddress.Any, 0);
                try {
                    this.sedsocket.SendTo(sendbytes, sendip);
                }
                catch(Exception e) {
                    Console.WriteLine(e);
                }

                Console.WriteLine("发送数据:{0}", BitConverter.ToString(sendbytes, 0, sendbytes.Length));

                var rcvUdpBuffer = new byte[18];
                try {
                    int offset = 0;
                    try {
                        int ret = this.sedsocket.ReceiveFrom(rcvUdpBuffer, offset, rcvUdpBuffer.Length - offset, SocketFlags.None, ref senderRemote);
                        offset += ret;
                        this.IsProcess = false;
                    }
                    catch(Exception e) {
                        Console.WriteLine(e);
                    }

                    var flagBytes = new byte[8];
                    Array.Copy(rcvUdpBuffer, 0, flagBytes, 0, 8);
                    string temp = BitConverter.ToString(flagBytes);

                    // Console.WriteLine("接收回传数据:{0}", temp);
                }
                catch(Exception e) {
                    Console.WriteLine(e);
                }
            }
            catch(Exception e) {
                Console.WriteLine(e);
                throw;
            }
            finally {
                this.IsProcess = false;
            }
        }

        #endregion

        #region Property

        public IPEndPoint SedIp {
            get;
            set;
        }

        public IPEndPoint RcvIp {
            get;
            set;
        }

        bool IsProcess {
            get;
            set;
        }

        bool IsGetRedata {
            get;
            set;
        }

        #endregion

        #region Field

        readonly Socket rcvsocket;
        readonly Socket sedsocket;
        public bool IsSocketConnect;

        #endregion

        #region IDisposable

        void Dispose(bool disposing) {
            if (disposing) {
                if (this.rcvsocket != null) {
                    this.rcvsocket.Dispose();
                }

                if (this.sedsocket != null) {
                    this.sedsocket.Dispose();
                }
            }
        }

        /// <inheritdoc />
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region 获取系统状态

        /// <summary>
        ///     获取系统状态
        ///     一次发送9条指令,包含:设备类型,设备ID,设备MAC,ADC_Err,脉冲周期,脉冲延时,脉冲宽度,ADC偏移,ADC禁用
        ///     接收反馈信息长度分别为:26bit,26bit,24bit,21bit,20bit,20bit,20bit,19bit,19bit
        /// </summary>
        public void GetDeviceState() {
            if (!this.IsSocketConnect) {
                MessageBox.Show("网络未连接");
                return;
            }

            // 读取:设备类型
            this.ReadDeviceType();
            Thread.Sleep(200);

            // 读取:设备ID
            this.ReadDeviceId();
            Thread.Sleep(200);

            // 读取:设备MAC
            this.ReadDeviceMac();
            Thread.Sleep(200);

            // 读取:脉冲周期
            this.ReadPulsePeriod();
            Thread.Sleep(200);

            // 读取:脉冲延时
            this.ReadPulseDelay();
            Thread.Sleep(200);

            // 读取:脉冲宽度
            this.ReadPulseWidth();
            Thread.Sleep(200);

            // 读取:ADC偏移
            this.ReadAdcOffset();
        }

        #region 读(发送指令及数据)

        /// <summary>读取:设备类型</summary>
        public void ReadDeviceType() {
            this.Send(ConstUdpArg.Device_Type, ConstUdpArg.Dst_ComMsgIp);
            this.ReceiveDeviceType(26);
        }

        /// <summary>读取:设备ID</summary>
        public void ReadDeviceId() {
            this.Send(ConstUdpArg.Device_Id, ConstUdpArg.Dst_ComMsgIp);
            this.ReceiveDeviceId(26);
        }

        /// <summary>读取:设备MAC</summary>
        public void ReadDeviceMac() {
            this.Send(ConstUdpArg.Device_Mac, ConstUdpArg.Dst_ComMsgIp);
            this.ReceiveDeviceMac(24);
        }

        /// <summary>读取:脉冲周期</summary>
        public void ReadPulsePeriod() {
            this.Send(ConstUdpArg.Pulse_Period_Read, ConstUdpArg.Dst_ComMsgIp);
            this.ReceivePulsePeriod(20);
        }

        /// <summary>读取:脉冲延时</summary>
        public void ReadPulseDelay() {
            this.Send(ConstUdpArg.Pulse_Delay_Read, ConstUdpArg.Dst_ComMsgIp);
            this.ReceivePulseDelay(20);
        }

        /// <summary>读取:脉冲宽度</summary>
        public void ReadPulseWidth() {
            this.Send(ConstUdpArg.Pulse_Width_Read, ConstUdpArg.Dst_ComMsgIp);
            this.ReceivePulseWidth(20);
        }

        /// <summary>读取:ADC偏移</summary>
        public void ReadAdcOffset() {
            this.Send(ConstUdpArg.GetAdcOffsetRead(DisPlayWindow.Info.AdcNum), ConstUdpArg.Dst_ComMsgIp);
            this.ReceiveAdcOffset(19);
        }

        /// <summary>///读取延时信息/// </summary>
        public void ReadDelyTime() {
            var sedip = ConstUdpArg.GetDelayTimeReadCommand(DisPlayWindow.Info.DelayChannel);
            this.Send(sedip, ConstUdpArg.Dst_ComMsgIp);
            this.ReceiveChannelDeleyTime(20);
        }

        /// <summary>///读取ADC通道/// </summary>
        public void ReadCanChannelLen() {
            var sedip = ConstUdpArg.GetDacChannelReadCommand(DisPlayWindow.Info.DacChannel - 1);
            this.Send(sedip, ConstUdpArg.Dst_ComMsgIp);
            this.ReceiveCanChannelLen(20);
        }

        #endregion

        #region 写(发送指令及数据)

        /// <summary>写:脉冲周期</summary>
        /// <param name="data">数据,16bit,配置值/5</param>
        public void WritePulsePeriod(byte[] data) {
            this.WriteData(ConstUdpArg.Pulse_Period_Write, data);
        }

        /// <summary>写:脉冲延时</summary>
        /// <param name="data">数据,16bit,配置值/5</param>
        public void WritePulseDelay(byte[] data) {
            this.WriteData(ConstUdpArg.Pulse_Delay_Write, data);
        }

        /// <summary>写:脉冲宽度</summary>
        /// <param name="data">数据,16bit,配置值/5</param>
        public void WritePulseWidth(byte[] data) {
            this.WriteData(ConstUdpArg.Pulse_Width_Write, data);
        }

        /// <summary>写:ADC偏移</summary>
        /// <param name="data">数据8bit,[(配置值+1.2)/1.2]*128</param>
        public void WriteAdcOffset(byte data) {
            var addr = ConstUdpArg.GetAdcOffsetWrite(DisPlayWindow.Info.AdcNum);
            var sendData = new byte[addr.Length + 1];
            Array.Copy(addr, sendData, addr.Length);

            // 最后一位是需要发送的数据
            sendData.SetValue(data, sendData.Length - 1);

            this.Send(sendData, ConstUdpArg.Dst_ComMsgIp);
        }

        /// <summary>写:脉冲宽度</summary>
        /// <param name="data">数据,16bit,配置值/5</param>
        public void WriteDelayTime(byte[] data) {
            var tmp = ConstUdpArg.GetDelayTimeWriteCommand(int.Parse(DisPlayWindow.HMainWindow.tb_deleyChannel.Text) - 1);
            this.WriteData(tmp, data);
        }

        /// <summary>
        ///     写入改变原始信号通道号
        /// </summary>
        /// <param name="num"></param>
        public void WriteOrigChannel(int num) {
            var data = ConstUdpArg.OrigChannel_Write;
            var temp = new byte[1];
            temp[0] = (byte) num;
            this.WriteData(data, temp);
        }

        /// <summary>
        ///     写入改变原始信号时分号
        /// </summary>
        /// <param name="num"></param>
        public void WriteOrigTDiv(int num) {
            var data = ConstUdpArg.OrigTimDiv_Write;
            var temp = new byte[1];
            temp[0] = (byte) num;
            this.WriteData(data, temp);
        }

        /// <summary>
        ///     写入Dac通道号
        /// </summary>
        /// <param name="num"></param>
        public void WriteDacChannel(int num) {
            var partAddr = ConstUdpArg.DacChannel_Write;
            var addr = new byte[partAddr.Length + 1];
            Array.Copy(partAddr, 0, addr, 0, partAddr.Length);
            int offset = partAddr.Length;

            int len;
            int.TryParse(DisPlayWindow.HMainWindow.tb_dacChannel.Text, out len);
            int chennelsite = 91 + len;
            var ctmp = new byte[1];
            ctmp[0] = (byte) chennelsite;
            addr.SetValue(ctmp[0], offset);
            var data = new byte[2];
            int t = num / 256;
            data[0] = (byte) t;
            data[1] = (byte) (num - t * 256);
            this.WriteData(addr, data);
        }

        /// <summary>
        ///     写入B值
        /// </summary>
        /// <param name="num"></param>
        public void WriteBvalue(byte[] dataBytes)
        {
            var addr = ConstUdpArg.Bvalue_Write;
            this.WriteData(addr, dataBytes);
        }

        public void WritePhase(byte[][] datalistBytes)
        {
            var bytelist = new List<byte>();
            
            Task.Run(() => {
                         for(int i = 0; i < datalistBytes.Length; i++) {
                             bytelist.AddRange(ConstUdpArg.Phase_Write);
                             byte t1 = (byte) (8 + i / 4);
                             byte t2 = (byte) (64 * (i % 4));
                             bytelist.Add(t1);
                             bytelist.Add(t2);
                             this.WriteData(bytelist.ToArray(), datalistBytes[i]);
                             
                             bytelist.Clear();

                         }
                     });


        }

        #endregion

        #region 存(发送指令及数据)

        /// <summary>存:脉冲周期</summary>
        /// <param name="data">数据,每次只能存入8bit,配置值/5</param>
        public void SavePulsePeriod(byte[] data) {
            this.SaveData(ConstUdpArg.Pulse_Period_Save, data);
        }

        /// <summary>存:脉冲延时</summary>
        /// <param name="data">数据,每次只能存入8bit,配置值/5</param>
        public void SavePulseDelay(byte[] data) {
            this.SaveData(ConstUdpArg.Pulse_Delay_Save, data);
        }

        /// <summary>存:脉冲宽度</summary>
        /// <param name="data">数据,每次只能存入8bit,配置值/5</param>
        public void SavePulseWidth(byte[] data) {
            this.SaveData(ConstUdpArg.Pulse_Width_Save, data);
        }

        /// <summary>存:ADC偏移</summary>
        /// <param name="data">数据8bit,[(配置值+1.2)/1.2]*128</param>
        public void SaveAdcOffset(byte data) {
            var addr = ConstUdpArg.GetAdcOffsetSave(DisPlayWindow.Info.AdcNum);
            var sendData = new byte[addr.Length + 1];
            Array.Copy(addr, sendData, addr.Length);

            // 最后一位是需要发送的数据
            sendData.SetValue(data, sendData.Length - 1);

            this.Send(sendData, ConstUdpArg.Dst_ComMsgIp);
        }

        /// <summary>存:延时通道</summary>
        /// <param name="data">数据8bit,[(配置值+1.2)/1.2]*128</param>
        public void SaveDelayTime(byte[] data) {
            var cmd = ConstUdpArg.GetDelayTimeSaveCommand(int.Parse(DisPlayWindow.HMainWindow.tb_deleyChannel.Text));
            this.SaveData(cmd, data);
        }

        /// <summary>存:DAC幅值</summary>
        /// <param name="data">数据8bit,[(配置值+1.2)/1.2]*128</param>
        public void SaveDacLen(byte[] data)
        {
            var cmd = ConstUdpArg.GetDacLenSaveCommand(int.Parse(DisPlayWindow.HMainWindow.tb_dacChannel.Text));
            this.SaveData(cmd, data);
        }

        /// <summary>存:ADC禁用</summary>
        /// <summary>
        ///     执行写指令
        /// </summary>
        /// <param name="addr">指令及地址</param>
        /// <param name="data">待发送的数据</param>
        void WriteData(byte[] addr, byte[] data) {
            var sendData = new byte[addr.Length + data.Length];
            Array.Copy(addr, 0, sendData, 0, addr.Length);

            // 最后两位是需要发送的数据
            Array.Copy(data, 0, sendData, sendData.Length - data.Length, data.Length);

            this.Send(sendData, ConstUdpArg.Dst_ComMsgIp);
        }

        /// <summary>
        ///     执行存指令
        /// </summary>
        /// <param name="addr">指令及地址</param>
        /// <param name="data">待发送的数据</param>
        void SaveData(byte[] addr, byte[] data) {
            if (!this.IsSocketConnect)
            {
                return;
            }

            var sendData = new byte[addr.Length + 1];
            Array.Copy(addr, sendData, addr.Length);

            for(int i = 0; i < data.Length; i++) {
                byte d = data[i];

                // 每循环一次地址(倒数第二位)+1
                sendData.SetValue((byte)(sendData[sendData.Length - 2] + i), sendData.Length - 2);

                // 最后一个元素是数据
                sendData.SetValue(d, sendData.Length - 1);

                // 发送
                this.Send(sendData, ConstUdpArg.Dst_ComMsgIp);
                Thread.Sleep(1000);
            }
        }

        #endregion

        #endregion

        #region 系统参数(读),数据接收      

        /// <summary>
        ///     接收设备类型数据
        /// </summary>
        /// <param name="bufferLength">buffer大小</param>
        public void ReceiveDeviceType(int bufferLength) {
            var rcvUdpBuffer = this.ReadRemote(bufferLength);
            if (rcvUdpBuffer == null) {
                Console.WriteLine("接收设备类型数据错误");
                return;
            }

                string mcType = Encoding.ASCII.GetString(rcvUdpBuffer, 6, 8);
                Console.WriteLine("mc_type={0}", mcType);
                DisPlayWindow.Info.McType = mcType;
            
        }

        /// <summary>
        ///     接收设备ID数据
        /// </summary>
        /// <param name="bufferLength">buffer大小</param>
        public void ReceiveDeviceId(int bufferLength) {
            var rcvUdpBuffer = this.ReadRemote(bufferLength);
            if (rcvUdpBuffer == null) {
                Console.WriteLine("接收设备ID数据错误");
                return;
            }

            string mcId = string.Empty;
                for(int i = 0; i < 8; i++) {
                    mcId += BitConverter.ToString(rcvUdpBuffer, 6 + i, 1);
                }

                Console.WriteLine("mc_id={0}", mcId);
                DisPlayWindow.Info.McId = mcId;
            
        }

        /// <summary>
        ///     接收设备Mac数据
        /// </summary>
        /// <param name="bufferLength">buffer大小</param>
        public void ReceiveDeviceMac(int bufferLength) {
            var rcvUdpBuffer = this.ReadRemote(bufferLength);
            if (rcvUdpBuffer == null) {
                Console.WriteLine("接收设备Mac数据错误");
                return;
            }

                string mcMac = string.Empty;
                for(int i = 0; i < 6; i++) {
                    if (i > 0) {
                        mcMac += ":" + BitConverter.ToString(rcvUdpBuffer, 6 + i, 1);
                    }
                    else {
                        mcMac += BitConverter.ToString(rcvUdpBuffer, 6 + i, 1);
                    }
                }

                Console.WriteLine("mc_mac={0}", mcMac);
                DisPlayWindow.Info.McMac = mcMac;
            
            
           
        }

        /// <summary>
        ///    接收脉冲周期数据
        /// </summary>
        /// <param name="bufferLength"></param>
        public void ReceivePulsePeriod(int bufferLength) {
            var rcvUdpBuffer = this.ReadRemote(bufferLength);
            if (rcvUdpBuffer == null) {
                Console.WriteLine("接收脉冲周期数据错误");
                return;
            }

            // 返回数据以指令为开头
            var cmd = new byte[6];
            Array.Copy(rcvUdpBuffer, 0, cmd, 0, 6);
            var data = new byte[2];
            Array.Copy(rcvUdpBuffer, 6, data, 0, 2);
            this.SetData(Encoding.ASCII.GetString(cmd, 0, 6), data);
        }

        /// <summary>
        ///    接收脉冲延迟数据
        /// </summary>
        /// <param name="bufferLength"></param>
        public void ReceivePulseDelay(int bufferLength) {
            var rcvUdpBuffer = this.ReadRemote(bufferLength);

            // 返回数据以指令为开头
            if (rcvUdpBuffer == null) {
                Console.WriteLine("接收脉冲延迟错误");
                return;
            }

            var cmd = new byte[6];
            Array.Copy(rcvUdpBuffer, 0, cmd, 0, 6);
            var temp = new byte[2];
            Array.Copy(rcvUdpBuffer, 6, temp, 0, 2);
            this.SetData(Encoding.ASCII.GetString(cmd, 0, 6), temp);
        }

        /// <summary>
        ///  接收脉冲宽度数据
        /// </summary>
        /// <param name="bufferLength"></param>
        public void ReceivePulseWidth(int bufferLength) {
            var rcvUdpBuffer = this.ReadRemote(bufferLength);

            // 返回数据以指令为开头
            if (rcvUdpBuffer == null) {
                Console.WriteLine("接收脉冲宽度错误");
                return;
            }

            var cmd = new byte[6];
            Array.Copy(rcvUdpBuffer, 0, cmd, 0, 6);
            var temp = new byte[2];
            Array.Copy(rcvUdpBuffer, 6, temp, 0, 2);
            this.SetData(Encoding.ASCII.GetString(cmd, 0, 6), temp);
        }

        /// <summary>
        /// 接收ADC数据
        /// </summary>
        /// <param name="bufferLength"></param>
        public void ReceiveAdcOffset(int bufferLength) {
            var rcvUdpBuffer = this.ReadRemote(bufferLength);

            // 返回数据以指令为开头
            if (rcvUdpBuffer == null) {
                Console.WriteLine("接收Adc偏移错误");
                return;
            }

            var cmd = new byte[6];
            Array.Copy(rcvUdpBuffer, 0, cmd, 0, 6);
            var temp = new byte[1];
            Array.Copy(rcvUdpBuffer, 6, temp, 0, 1);
            this.SetData(Encoding.ASCII.GetString(cmd, 0, 6), temp);
        }

        /// <summary>
        ///  接收通道延时
        /// </summary>
        /// <param name="bufferLength"></param>
        public void ReceiveChannelDeleyTime(int bufferLength) {
            var rcvUdpBuffer = this.ReadRemote(bufferLength);

            // 返回数据以指令为开头
            if (rcvUdpBuffer == null) {
                Console.WriteLine(" 接收通道延时错误");
                return;
            }

            var cmd = new byte[6];
            Array.Copy(rcvUdpBuffer, 0, cmd, 0, 6);
            var temp = new byte[2];
            Array.Copy(rcvUdpBuffer, 6, temp, 0, 2);
            this.SetData(Encoding.ASCII.GetString(cmd, 0, 6), temp);
        }

        /// <summary>
        ///  接收ADC通道
        /// </summary>
        /// <param name="bufferLength"></param>
        public void ReceiveCanChannelLen(int bufferLength) {
            var rcvUdpBuffer = this.ReadRemote(bufferLength);
            if (rcvUdpBuffer == null) {
                Console.WriteLine("接收ADC通道错误");
                return;
            }

            // 返回数据以指令为开头
            var cmd = new byte[6];
            Array.Copy(rcvUdpBuffer, 0, cmd, 0, 6);
            var temp = new byte[2];
            Array.Copy(rcvUdpBuffer, 6, temp, 0, temp.Length);
            this.SetData(Encoding.ASCII.GetString(cmd, 0, 6), temp);
        }

        /// 将接收到的(脉冲周期/脉冲延时/脉冲宽度/ADC偏移)数据转换为界面显示的值
        /// <param name="data"></param>
        /// <returns></returns>
        int ToInt(byte[] data) {
            if (data == null || data.Length < 1) {
                return -1;
            }

            if (data.Length != 2) {
                throw new Exception("异常(脉冲周期/脉冲延时/脉冲宽度)数据.");
            }

            int hig = data[0] * 256;
            int low = data[1];
            return hig + low;
        }

        /// 读取远程发送的UDP数据
        /// <param name="bufferLength">缓存区大小</param>
        /// <returns></returns>
        byte[] ReadRemote(int bufferLength) {
            if (!this.IsSocketConnect)
            {
                return null;
            }

            EndPoint senderRemote = new IPEndPoint(IPAddress.Any, 0);
            var rcvUdpBuffer = new byte[bufferLength * 4];
            int offset = 0;
            try
            {
                this.rcvsocket.ReceiveFrom(rcvUdpBuffer, offset, rcvUdpBuffer.Length - offset, SocketFlags.None, ref senderRemote);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }

            return rcvUdpBuffer;
        }

        /// 将接收到的数据显示到对应的界面上
        /// 由于UDP返回数据包可能不是按发送命令顺序,此处增加识别处理
        /// <param name="cmd"></param>
        /// <param name="data"></param>
        void SetData(string cmd, byte[] data) {
            // 得到发送指令后与ConstUdpArg中指令集对比,再将对应的数据处理后显示到界面上
            if (Encoding.ASCII.GetString(ConstUdpArg.Pulse_Period_Read, 0, 6).Equals(cmd)) {
                // 脉冲周期
                int pulsePeriod = this.ToInt(data) * 5;
                Console.WriteLine("pulse_period={0}", pulsePeriod);
                DisPlayWindow.Info.PulsePeriod = pulsePeriod;
            }
            else if (Encoding.ASCII.GetString(ConstUdpArg.Pulse_Delay_Read, 0, 6).Equals(cmd)) {
                // 脉冲延时
                int pulseDelay = this.ToInt(data) * 5;
                Console.WriteLine("pulse_delay={0}", pulseDelay);
                DisPlayWindow.Info.PulseDelay = pulseDelay;
            }
            else if (Encoding.ASCII.GetString(ConstUdpArg.Pulse_Width_Read, 0, 6).Equals(cmd)) {
                // 脉冲宽度
                int pulseWidth = this.ToInt(data) * 5;
                Console.WriteLine("pulse_width={0}", pulseWidth);
                DisPlayWindow.Info.PulseWidth = pulseWidth;
            }
            else if (Encoding.ASCII.GetString(ConstUdpArg.GetAdcOffsetRead(DisPlayWindow.Info.AdcNum), 0, 6).Equals(cmd)) {
                // ADC偏移
                double adcOffset = data[0];
                adcOffset = adcOffset * 1.2 / 128 - 1.2;

                // AdcOffset.ToString("G3");
                Console.WriteLine("adc_offset={0}", adcOffset.ToString("G2"));
                DisPlayWindow.Info.AdcOffset = adcOffset.ToString("G2");
            }
            else if (Encoding.ASCII.GetString(ConstUdpArg.GetDelayTimeReadCommand(DisPlayWindow.Info.DelayChannel), 0, 6).Equals(cmd)) {
                // 延迟通道
                byte t = data[0];
                data[0] = data[1];
                data[1] = t;
                int delaytime = BitConverter.ToUInt16(data, 0);
                DisPlayWindow.Info.DelayTime = delaytime;
            }
            else if (Encoding.ASCII.GetString(ConstUdpArg.GetDacChannelReadCommand(DisPlayWindow.Info.DacChannel - 1), 0, 6).Equals(cmd)) {
                // Dac数据
                int result = data[0] * 256;
                result += data[1];
                DisPlayWindow.Info.DacLenth = result;
            }
            else {
                // 其他,未定义
                Console.WriteLine("cmd={0}", cmd);
            }
        }

        #endregion
    }
}
