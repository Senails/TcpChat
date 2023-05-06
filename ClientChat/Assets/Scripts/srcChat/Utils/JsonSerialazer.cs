using UnityEngine;

namespace Json{
    public static class JsonSerializer
    {
        static public string Serialize<T>(T obj){
            return JsonUtility.ToJson(obj);
        }

        static public T Deserialize<T>(string text){
            return JsonUtility.FromJson<T>(text);
        }
    }
}
