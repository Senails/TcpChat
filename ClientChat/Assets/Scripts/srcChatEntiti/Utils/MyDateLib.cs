using System;

static class MyDateLib {
    static public long getDateMilisec(){
        DateTime date1 = DateTime.Now;
        DateTime date2 = new DateTime(1970,1,1);

        return (date1.Ticks-date2.Ticks)/10000;;
    }
    static public string timestampToString(long milisec){
        DateTime datex = new DateTime(1970,1,1);

        long tiks = milisec*10000 + datex.Ticks;

        DateTime date =new DateTime(tiks);

        return date.ToString();
    }
}