using System.Data.SQLite;

using DBTypes;
using static MyDateLib;


public static class MyDataBase
{
    static SQLiteConnection? Connection;

    public static void connect(string fileName){
        try{
            string connectionString = $"Data Source={fileName};Version=3;";
            Connection = new SQLiteConnection(connectionString);
            Connection.Open();
        }catch{
            Console.WriteLine("ошибка при подключении к базе данных");
        }
    }

    public static async Task addMessage(int authtorID ,string message){
        using var command = new SQLiteCommand(Connection);
        long date = getDateMilisec();

        string sqlRequest = 
        @"
        INSERT INTO messages (TimeDate,Authtor,Message)
        VALUES (@value1, (SELECT name FROM users WHERE users.id = @value2), @value3);
        ";

        command.CommandText=sqlRequest;
        command.Parameters.AddWithValue("@value1", date);
        command.Parameters.AddWithValue("@value2", authtorID);
        command.Parameters.AddWithValue("@value3", message);

        await command.ExecuteNonQueryAsync();
    }
    public static async Task<DBMessage[]> getMessages(){
        using var command = new SQLiteCommand(Connection);

        string sqlRequest = 
        @"
        SELECT * 
        FROM messages
        ORDER BY TimeDate DESC
        LIMIT 50;
        ";

        command.CommandText=sqlRequest;

        List<DBMessage> list= new List<DBMessage>();
        using var reader = await command.ExecuteReaderAsync();

        while(await reader.ReadAsync()){
            list.Add(new DBMessage(){
                id = reader.GetInt32(0),
                date = reader.GetInt64(1),
                authtor = reader.GetString(2),
                text = reader.GetString(3),
            });
        }

        DBMessage[] arr = new DBMessage[list.Count];
        int i=0;
        foreach(DBMessage mess in list){
            arr[i]=mess;
            i++;
        }

        return arr;
    }
    public static async Task addUser(string login, string name ,string password){
        using var command = new SQLiteCommand(Connection);

        string sqlRequest = 
        @"
        INSERT INTO users (Login,Name,Password)
        VALUES (@Column1, @Column2, @Column3);
        ";

        command.CommandText=sqlRequest;
        command.Parameters.AddWithValue("@Column1", login);
        command.Parameters.AddWithValue("@Column2", name);
        command.Parameters.AddWithValue("@Column3", password);

        await command.ExecuteNonQueryAsync();  
    }
    public static async Task<DBUser> getUser(string login){
        using var command = new SQLiteCommand(Connection);

        string sqlRequest = 
        @"
        SELECT * 
        FROM users 
        WHERE Login = @Column1;
        ";

        command.CommandText=sqlRequest;
        command.Parameters.AddWithValue("@Column1", login);

        using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();

        DBUser user = new DBUser(){
            id = reader.GetInt32(0),
            login = reader.GetString(1),
            name = reader.GetString(2),
            password = reader.GetString(3),
        };

        return user;
    }

}