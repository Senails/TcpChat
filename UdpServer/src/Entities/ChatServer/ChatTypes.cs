using DBTypes;

namespace ChatTypes{
    
    public record Message{
        public messageType typeMessage { get; init; }
        public byte[]? data { get; init; }
    }

    public enum messageType{
        //without response      sendMessage/onMessage
        newMessage, //only server -> client
        iSendMessage, //only client -> server
        openChat, //only server -> client
        leaveFromChat, //only server -> client


        //with response     sendRequest/onRequest
        getKeyRequest, //only client -> server
        authMeRequest, //only client -> server
        regMeRequest, //only client -> server
        getPrevInfo, //only client -> server
        resultMessage, //only server -> client
    }

    public record PrevInfo{
        public string[]? userNames { get; init; }
        public DBMessage[]? messages { get; init; }
    }

    public record LoginData{
        public string? login { get; init; }
        public string? criptPass { get; init; }
    }

    public record RegisterData{
        public string? name { get; init; }
        public string? login { get; init; }
        public string? criptPass { get; init; }
    }
}
