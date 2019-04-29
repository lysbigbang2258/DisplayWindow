// 201812284:30 PM
namespace ArrayDisplay.Net {
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    using ArrayDisplay.Annotations;

    /// <summary>
    /// The system info.
    /// </summary>
    public sealed class SystemInfo : INotifyPropertyChanged {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemInfo"/> class.
        /// </summary>
        public SystemInfo() {
            this.mcId = string.Empty;
            this.mcMac = string.Empty;
            this.mcType = string.Empty;
            this.pulseDelay = -1;
            this.pulsePeriod = -1;
            this.pulseWidth = -1;
            this.adcNum = 1;

            this.adcOffset = string.Empty;
            this.delayChannel = 1;
            this.delayTime = 1;

            this.daclen = 3000;
            this.dacChannel = 2;
            this.origFrams = 200;
            this.origChannel = 1;
            this.origTdiv = 1;
        }

        /// <summary>
        /// The property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The on property changed.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        [NotifyPropertyChangedInvocator]
        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Field

        /// <summary>
        /// The mc id.
        /// </summary>
        string mcId;

        /// <summary>
        /// The mc mac.hun
        /// </summary>
        string mcMac;

        /// <summary>
        /// The mc type.
        /// </summary>
        string mcType;

        /// <summary>
        /// The pulse delay.
        /// </summary>
        int pulseDelay;

        /// <summary>
        /// The pulse period.
        /// </summary>
        int pulsePeriod;

        /// <summary>
        /// The pulse width.
        /// </summary>
        int pulseWidth;

        /// <summary>
        /// The adc num.
        /// </summary>
        int adcNum;

        /// <summary>
        /// The adc offset.
        /// </summary>
        string adcOffset;

        /// <summary>
        /// The delay channel.
        /// </summary>
        int delayChannel;

        /// <summary>
        /// The delay time.
        /// </summary>
        int delayTime;

        /// <summary>
        /// The daclen.
        /// </summary>
        int daclen;

        /// <summary>
        /// The dac channel.
        /// </summary>
        int dacChannel;

        /// <summary>
        /// The orig frams.
        /// </summary>
        int origFrams;

        /// <summary>
        /// The orig channel.
        /// </summary>
        int origChannel;

        /// <summary>
        /// The orig tdiv.
        /// </summary>
        int origTdiv;

        /// <summary>
        /// The work channel.
        /// </summary>
        int workChannel;

        #endregion

        #region Property

        /// <summary>
        /// Gets or sets the mc type.
        /// 设备类型
        /// </summary>
        public string McType {
            get => this.mcType;

            set {
                if (value == this.mcType) {
                    return;
                }
                this.mcType = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the mc id.
        /// 设备ID
        /// </summary>
        public string McId {
            get => this.mcId;

            set {
                if (value == this.mcId) {
                    return;
                }
                this.mcId = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the mc mac.
        /// 设备MAC
        /// </summary>
        public string McMac {
            get => this.mcMac;

            set {
                if (value != this.mcMac) {
                    this.mcMac = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the adc offset.
        /// adc偏移
        /// </summary>
        public string AdcOffset {
            get => this.adcOffset;

            set {
                if (value != this.adcOffset) {
                    this.adcOffset = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the adc num.
        /// adc偏移.adcID
        /// </summary>
        public int AdcNum {
            get => this.adcNum;

            set {
                if (value != this.adcNum) {
                    this.adcNum = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the pulse period.
        /// 脉冲周期
        /// </summary>
        public int PulsePeriod {
            get => this.pulsePeriod;

            set {
                if (value != this.pulsePeriod) {
                    this.pulsePeriod = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the pulse delay.
        /// 脉冲延时
        /// </summary>
        public int PulseDelay {
            get => this.pulseDelay;

            set {
                if (value != this.pulseDelay) {
                    this.pulseDelay = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the pulse width.
        /// 脉冲宽度
        /// </summary>
        public int PulseWidth {
            get => this.pulseWidth;

            set {
                if (value != this.pulseWidth) {
                    this.pulseWidth = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the delay channel.
        /// 延时通道
        /// </summary>
        public int DelayChannel {
            get => this.delayChannel;

            set {
                this.delayChannel = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the delay time.
        /// 延时时间量
        /// </summary>
        public int DelayTime {
            get => this.delayTime;

            set {
                this.delayTime = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the work channel.
        /// 工作通道
        /// </summary>
        public int WorkChannel {
            get => this.workChannel;

            set {
                this.workChannel = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the dac lenth.
        /// DAC长度
        /// </summary>
        public int DacLenth {
            get => this.daclen;

            set {
                this.daclen = value;
                this.OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Gets or sets the dac channel.
        /// Dac通道
        /// </summary>
        public int DacChannel {
            get => this.dacChannel;

            set {
                this.dacChannel = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the orig fram nums.
        /// 原始数据帧数
        /// </summary>
        public int OrigFramNums {
            get => this.origFrams;

            set {
                this.origFrams = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the orig channel.
        /// // 原始通道
        /// </summary>
        public int OrigChannel {
            get => this.origChannel;

            set => this.origChannel = value;
        }

        /// <summary>
        /// Gets or sets the orig tdiv.
        /// 原始时分
        /// </summary>
        public int OrigTdiv {
            get => this.origTdiv;

            set => this.origTdiv = value;
        }
        #endregion
    }
}
