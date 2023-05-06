namespace DBTypes{

    public record DBMessage(){
        public int id { get; init; } = 0;
        public long date { get; init; }
        public string authtor { get; init; } = "";
        public string text { get; init; } = "";
    };

    public record DBUser(){
        public int id { get; init; }
        public string login { get; init; } = ""; 
        public string name { get; init; } = "";
        public string password { get; init; } = "";
    }


}