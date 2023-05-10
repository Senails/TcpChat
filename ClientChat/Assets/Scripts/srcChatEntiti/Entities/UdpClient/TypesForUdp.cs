using System;
using System.Net;

namespace TypesForUdp{

    public record Dgram {
        public DgramInfoType type { get; set; }
        public DgramMeanType? meanType { get; set; }
        public long ID { get; set; }
        public byte[] data { get; init; }
    }

    public record DgramForList{
        public EndPoint endPoint { get; init; }
        public Dgram dgram { get; init; }
        public byte[] serializedDgram { get; init; }
    }

    public enum DgramInfoType{
        TransportInfo,
        ConfirmationTransport
    }

    public enum DgramMeanType{
        simpleMessage,
        pequestMessage,
        responseMessage,
        iLiveMessage,
    }

    public record DgramWaiter {
        public long dgramID { get; init; }
        public Action<byte[]> handler { get; init; }
    }
}