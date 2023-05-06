public static class NeedMetods{

    public static T[] Reverse<T>(T[] arr){
        T[] reverseArr= new T[arr.Length];

        int i = arr.Length-1;

        foreach(T elem in arr){
            reverseArr[i]=elem;
            i--;
        }

        return reverseArr;
    }


}