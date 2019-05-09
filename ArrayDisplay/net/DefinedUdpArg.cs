using System;
using System.Net;

namespace ArrayDisplay.Net {
    public class ConstUdpArg {
        #region IP与端口field

        static ConstUdpArg() {
            Src_OrigWaveIp = new IPEndPoint(IPAddress.Parse("192.168.172.1"), 8974);
            Src_NormWaveIp = new IPEndPoint(IPAddress.Parse("192.168.172.1"), 8975);
            Dst_NormWaveIp = new IPEndPoint(IPAddress.Parse("192.168.172.100"), 8976);
            Src_DelayWaveIp = new IPEndPoint(IPAddress.Parse("192.168.172.1"), 8973);
            Dst_ComMsgIp = new IPEndPoint(IPAddress.Parse("192.168.172.100"), 8972);
            Dst_ComDatIp = new IPEndPoint(IPAddress.Parse("192.168.172.100"), 8973);
            Src_ComDatIp = new IPEndPoint(IPAddress.Parse("192.168.172.1"), 8972);
            Src_ComMsgIp = new IPEndPoint(IPAddress.Parse("192.168.172.1"), 8971);
            SwicthToStateWindow = swicthToStateWindow;
            SwicthToDeleyWindow = swicthToDeleyWindow;
            SwicthToNormalWindow = swicthToNormalWindow;
            SwicthToOriginalWindow = swicthToOriginalWindow;
        }

        #endregion

        #region IP Property

        /// <summary>
        ///     src端发送与回传
        /// </summary>
        public static IPEndPoint Src_ComMsgIp {
            get;
            private set;
        }

        /// <summary>
        ///     src端数据通信
        /// </summary>
        public static IPEndPoint Src_ComDatIp {
            get;
            private set;
        }

        /// <summary>
        ///     dst端命令发送与回传
        /// </summary>
        public static IPEndPoint Dst_ComMsgIp {
            get;
            private set;
        }

        /// <summary>
        ///     dst端数据通信
        /// </summary>
        public static IPEndPoint Dst_ComDatIp {
            get;
            private set;
        }

        /// <summary>
        ///     src端延时波形数据
        /// </summary>
        public static IPEndPoint Src_DelayWaveIp {
            get;
            private set;
        }

        /// <summary>
        ///     dst端波形数据
        /// </summary>
        public static IPEndPoint Dst_NormWaveIp {
            get;
            private set;
        }

        /// <summary>
        ///     src端延时波形数据
        /// </summary>
        public static IPEndPoint Src_NormWaveIp {
            get;
            private set;
        }

        /// <summary>
        ///     src端原始波形数据
        /// </summary>
        public static IPEndPoint Src_OrigWaveIp {
            get;
            private set;
        }

        /// <summary>
        /// </summary>
        public static byte[] SwicthToStateWindow {
            get {
                return swicthToStateWindow;
            }
            private set { }
        }

        public static byte[] SwicthToOriginalWindow {
            get {
                return swicthToOriginalWindow;
            }
            private set { }
        }

        public static byte[] SwicthToDeleyWindow {
            get {
                return swicthToDeleyWindow;
            }
            private set { }
        }

        public static byte[] SwicthToNormalWindow {
            get {
                return swicthToNormalWindow;
            }
            private set { }
        }

        #endregion

        #region 指令常量Property

        public static byte[] Device_Type {
            get {
                return device_Type;
            }
        }

        public static byte[] Adc_Err {
            get {
                return adc_Err;
            }
        }

        public static byte[] Device_Id {
            get {
                return device_Id;
            }
        }

        public static byte[] Device_Mac {
            get {
                return device_Mac;
            }
        }

        /// <summary>读指令:脉冲周期</summary>
        public static byte[] Pulse_Period_Read {
            get {
                return pulse_Period_Read;
            }
        }

        /// <summary>读指令:脉冲延时</summary>
        public static byte[] Pulse_Delay_Read {
            get {
                return pulse_Delay_Read;
            }
        }

        /// <summary>读指令:脉冲宽度</summary>
        public static byte[] Pulse_Width_Read {
            get {
                return pulse_Width_Read;
            }
        }

        /// <summary>写指令:脉冲周期</summary>
        public static byte[] Pulse_Period_Write {
            get {
                return pulse_Period_Write;
            }
        }

        /// <summary>写指令:脉冲延时</summary>
        public static byte[] Pulse_Delay_Write {
            get {
                return pulse_Delay_Write;
            }
        }

        /// <summary>写指令:脉冲宽度</summary>
        public static byte[] Pulse_Width_Write {
            get {
                return pulse_Width_Write;
            }
        }

        /// <summary>存指令:脉冲周期</summary>
        public static byte[] Pulse_Period_Save {
            get {
                return pulse_Period_Save;
            }
        }

        /// <summary>存指令:脉冲延时</summary>
        public static byte[] Pulse_Delay_Save {
            get {
                return pulse_Delay_Save;
            }
        }

        /// <summary>存指令:脉冲宽度</summary>
        public static byte[] Pulse_Width_Save {
            get {
                return pulse_Width_Save;
            }
        }

        /// <summary>读指令:延时通道</summary>
        public static byte[] DelayChannel_Read {
            get {
                return delayChannel_Read;
            }
        }

        /// <summary>写指令:延时通道</summary>
        public static byte[] DelayChannel_Write {
            get {
                return delayChannel_Write;
            }
        }

        /// <summary>写指令:延时通道</summary>
        public static byte[] DelayChannel_Save {
            get {
                return delayChannel_Save;
            }
        }

        public static byte[] OrigChannel_Write {
            get {
                return origChannel_Write;
            }
            set {
                origChannel_Write = value;
            }
        }

        public static byte[] OrigTimDiv_Write {
            get {
                return origTimDiv_Write;
            }
            set {
                origTimDiv_Write = value;
            }
        }

        /// <summary>
        /// Dac操作
        /// </summary>
        public static byte[] DacChannel_Read {
            get {
                return dacChannel_Read;
            }
            set {
                dacChannel_Read = value;
            }
        }

        public static byte[] DacChannel_Write {
            get {
                return dacChannel_Write;
            }
            set {
                dacChannel_Write = value;
            }
        }

        public static byte[] DacChannel_Save {
            get {
                return dacChannel_Save;
            }
            set {
                dacChannel_Save = value;
            }
        }

        public static byte[] Bvalue_Write {
            get {
                return bvalue_Write;
            }
            set {
                bvalue_Write = value;
            }
        }

        public static byte[] Phase_Write {
            get {
                return phase_Write;
            }
            set {
                phase_Write = value;
            }
        }

        #endregion

        #region 指令变量Method

        /// <summary>读指令:ADC偏移</summary>
        public static byte[] GetAdcOffsetRead(int idcNum) {
            byte[] adcOffset = {0, 0, 1, 0, 0, 82};
            adcOffset.SetValue((byte) (adcOffset[5] + idcNum), 5);
            return adcOffset;
        }

        /// <summary>写指令:ADC偏移</summary>
        public static byte[] GetAdcOffsetWrite(int idcNum) {
            byte[] adcOffset = {1, 0, 1, 0, 0, 82};
            adcOffset.SetValue((byte) (adcOffset[5] + idcNum), 5);
            return adcOffset;
        }

        /// <summary>存指令:ADC偏移</summary>
        public static byte[] GetAdcOffsetSave(int idcNum) {
            byte[] adcOffset = {1, 0, 1, 2, 0, 146};
            adcOffset.SetValue((byte) (adcOffset[5] + idcNum), 5);
            return adcOffset;
        }

        /// <summary>读指令:延时通道</summary>
        public static byte[] GetDelayTimeReadCommand(int idcNum) {
            var channel = new byte[DelayChannel_Read.Length];
            Array.Copy(DelayChannel_Read, 0, channel, 0, DelayChannel_Read.Length);
            channel.SetValue((byte) (channel[5] + (idcNum - 1) * 2), 5);
            return channel;
        }
        /// <summary>读指令:DacChannel</summary>
        public static byte[] GetDacChannelReadCommand(int idcNum)
        {
            var channel = new byte[DacChannel_Read.Length];
            Array.Copy(DacChannel_Read, 0, channel, 0, DacChannel_Read.Length);
            channel.SetValue((byte)(channel[5] + (idcNum)*2), 5);
            return channel;
        }


        /// <summary>写指令:延时通道</summary>
        public static byte[] GetDelayTimeWriteCommand(int idcNum) {
            var channel = new byte[DelayChannel_Write.Length];
            Array.Copy(DelayChannel_Write, 0, channel, 0, DelayChannel_Write.Length);
            channel.SetValue((byte) (channel[5] + idcNum * 2), 5);
            return channel;
        }

        /// <summary>存指令:ADC偏移</summary>
        public static byte[] GetDelayTimeSaveCommand(int idcNum) {
            var channel = new byte[DelayChannel_Write.Length];
            Array.Copy(DelayChannel_Save, 0, channel, 0, DelayChannel_Write.Length);
            channel.SetValue((byte) (channel[5] + (idcNum - 1) * 2), 5);
            return channel;
        }
        /// <summary>存指令:DacLen</summary>
        public static byte[] GetDacLenSaveCommand(int idcNum)
        {
            var channel = new byte[DacChannel_Save.Length];
            Array.Copy(DacChannel_Save, 0, channel, 0, DacChannel_Save.Length);
            channel.SetValue((byte)(channel[5] + (idcNum - 1) * 2), 5);
            return channel;
        }

        #endregion

        #region 结构体

        public struct UdpData {
            public IPEndPoint Ip {
                get;
                set;
            }

            public byte[] DataBytes {
                get;
                set;
            }
        }

        public enum WaveType {
            Normal = 0,
            Orig = 1,
            Delay = 2
        }

        #endregion

        #region 指令field

        /// <summary>
        ///     获取状态指令
        /// </summary>
        static readonly byte[] device_Type = {0, 0, 8, 5, 0, 0};
        static readonly byte[] adc_Err = {0, 0, 3, 0, 0, 61};
        static readonly byte[] device_Id = {0, 0, 8, 5, 0, 8};
        static readonly byte[] device_Mac = {0, 0, 6, 4, 0, 26};

        /// <summary>
        ///     切换窗口指令
        /// </summary>
        static readonly byte[] swicthToStateWindow = {1, 0, 1, 0, 0, 0, 0};

        static readonly byte[] swicthToOriginalWindow = {1, 0, 1, 0, 0, 0, 2};
        static readonly byte[] swicthToDeleyWindow = {1, 0, 1, 0, 0, 0, 1};
        static readonly byte[] swicthToNormalWindow = {1, 0, 1, 0, 0, 0, 4};

        /// <summary>
        ///     读指令
        /// </summary>
        static readonly byte[] pulse_Period_Read = {0, 0, 2, 0, 0, 1};

        static readonly byte[] pulse_Delay_Read = {0, 0, 2, 0, 0, 5};
        static readonly byte[] pulse_Width_Read = {0, 0, 2, 0, 0, 3};

        /// <summary>
        ///     写指令
        /// </summary>
        static readonly byte[] pulse_Period_Write = {1, 0, 2, 0, 0, 1};

        static readonly byte[] pulse_Delay_Write = {1, 0, 2, 0, 0, 5};
        static readonly byte[] pulse_Width_Write = {1, 0, 2, 0, 0, 3};

        /// <summary>
        ///     存指令
        /// </summary>
        static readonly byte[] pulse_Period_Save = {1, 0, 1, 2, 0, 65};

        static readonly byte[] pulse_Delay_Save = {1, 0, 1, 2, 0, 69};
        static readonly byte[] pulse_Width_Save = {1, 0, 1, 2, 0, 67};

        /// <summary>
        ///     删除指令
        /// </summary>
        static readonly byte[] delayChannel_Read = {0, 0, 2, 0, 0, 10};
        static readonly byte[] delayChannel_Write = {1, 0, 2, 0, 0, 10};
        static readonly byte[] delayChannel_Save = {1, 0, 1, 2, 0, 74};

        /// <summary>
        /// 原始数据操作
        /// </summary>
        static  byte[] origChannel_Write = {1, 0, 1, 0,0, 7 };
        static  byte[] origTimDiv_Write = {1, 0, 1, 0,0, 8 };
        static byte[] bvalue_Write = {1, 2, 0, 1, 48, 0};
        static byte[] phase_Write = {1, 0, 64, 0};

        /// <summary>
        /// Dac操作
        /// </summary>
        static byte[] dacChannel_Read = { 0, 0, 2, 0, 0, 91 };
        static byte[] dacChannel_Write = { 1, 0, 2, 0, 0 }; 
        static byte[] dacChannel_Save = { 1, 0, 1, 2, 0, 155 }; 

        #endregion

        #region 偏移数组

        public static int[] offsetArray = {
            1, 33, 65, 97, 129, 161, 193, 225, 2, 34, 66, 98, 130, 162, 194, 226, 3, 35, 67, 99, 131, 163, 195, 227, 4, 36, 68, 100,
            132, 164, 196, 228, 5, 37, 69, 101, 133, 165, 197, 229, 6, 38, 70, 102, 134, 166, 198, 230, 7, 39, 71, 103, 135, 167, 199, 231, 8, 40, 72,
            104, 136, 168, 200, 232, 9, 41, 73, 105, 137, 169, 201, 233, 10, 42, 74, 106, 138, 170, 202, 234, 11, 43, 75, 107, 139, 171, 203, 235, 12,
            44, 76, 108, 140, 172, 204, 236, 13, 45, 77, 109, 141, 173, 205, 237, 14, 46, 78, 110, 142, 174, 206, 238, 15, 47, 79, 111, 143, 175, 207,
            239, 16, 48, 80, 112, 144, 176, 208, 240, 17, 49, 81, 113, 145, 177, 209, 241, 18, 50, 82, 114, 146, 178, 210, 242, 19, 51, 83, 115, 147,
            179, 211, 243, 20, 52, 84, 116, 148, 180, 212, 244, 21, 53, 85, 117, 149, 181, 213, 245, 22, 54, 86, 118, 150, 182, 214, 246, 23, 55, 87,
            119, 151, 183, 215, 247, 24, 56, 88, 120, 152, 184, 216, 248, 25, 57, 89, 121, 153, 185, 217, 249, 26, 58, 90, 122, 154, 186, 218, 250, 27,
            59, 91, 123, 155, 187, 219, 251, 28, 60, 92, 124, 156, 188, 220, 252, 29, 61, 93, 125, 157, 189, 221, 253, 30, 62, 94, 126, 158, 190, 222,
            254, 31, 63, 95, 127, 159, 191, 223, 255, 32, 64, 96, 128, 160, 192, 224, 256
        };
        public static int[] offsetArrayTwo = {
            1,  9,  17, 25, 33, 41, 49, 57,
            2,  10, 18, 26, 34, 42, 50, 58,
            3,  11, 19, 27, 35, 43, 51, 59,
            4,  12, 20, 28, 36, 44, 52, 60,
            5,  13, 21, 29, 37, 45, 53, 61,
            6,  14, 22, 30, 38, 46, 54, 62,
            7,  15, 23, 31, 39, 47, 55, 63,
            8,  16, 24, 32, 40, 48, 56, 64
        };
        #endregion


        #region 常量定义

        //阵元数
        public const int ARRAY_NUM = 64; //阵元数
        public const int ARRAY_USED = 64; //阵元数
        //Buffer设置
//        public const int WORK_FRAME_NUMS = 31250; //正常工作波形同时显示帧数
        public const int WORK_FRAME_NUMS = 1024*16; //正常工作波形同时显示帧数
        public const int WORK_FRAME_LENGTH = 1024; // 正常工作波形帧长
        //能量图像素长度
        public const int MAX_ENERGY_PIXELS_LENGTH = 70;
        public const int MIN_ENERGY_PIXELS_LENGTH = 0;
        public const int DEAFULT_ENERGY_PIXELS_LENGTH = 30;

        public const int ORIG_FRAME_NUMS = 200; //原始工作波形同时显示帧数
        public const int ORIG_FRAME_LENGTH = 1282; //原始工作波形帧长
        public const int ORIG_DETECT_LENGTH = 64;  
        public const int ORIG_TIME_NUMS = 8;
        public const int ORIG_TIME_OLDNUMS= 32;
        public const int ORIG_CHANNEL_NUMS = 8;


        public const int DELAY_FRAME_CHANNELS = 8;
        public const int DELAY_FRAME_NUMS = 10;
        public const int DELAY_FRAME_LENGTH = 402;

        public const int SAVE_ORIGPACK = 4 * 1024;
        public const int SAVE_WORKPACK = 1024 * 4 * 100;


        #endregion
    }
}
