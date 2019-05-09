using System.Net;

namespace ArrayDisplay.Net {
    public struct StreamDataPack {
        public IPEndPoint Ip { get; set; }
        public byte[] DataBytes { get;set; }
    }
}
