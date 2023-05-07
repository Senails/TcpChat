using System;
using System.Text;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;

public static class MyRSAcoder
{   
    public static void createKeys(out KeyRSA openKey, out KeyRSA secretKey){
        long p = getSimpleNumber();
        long q = getSimpleNumber();

        while (p==q) q = getSimpleNumber();

        long n = p*q;
        long m = (p-1)*(q-1);
        long d = findNumberD(m);
        long e = findNumberE(m ,d);

        openKey = new KeyRSA() { num = e, mod = n};
        secretKey = new KeyRSA() { num = d, mod = n};
    }


    public static string encodeText(string text , long key , long mod){
        byte[] bufferMessage = Encoding.UTF8.GetBytes(text);
        string res = "";

        foreach(byte b in bufferMessage){
            BigInteger bi = new BigInteger(b);
            bi = BigInteger.Pow(bi,(int)key);
            bi = bi%mod;
            res+=$"{bi}\n";
        }

        return res;
    }
    public static string decodeText(string text , long key , long mod){
        var list = new List<string>();

        string buf = "";

        foreach(char ch in text){
            if (ch!='\n'){
                buf+=ch;
            }else{
                list.Add(buf);
                buf="";
            }
        }

        byte[] blob = new byte[list.Count];

        int i=0;
        foreach(string word in list){
            int num = Convert.ToInt32(word);

            BigInteger bi = new BigInteger(num);
            bi = BigInteger.Pow(bi,(int)key);
            bi = bi%mod;

            blob[i]=(byte)bi;
            i++;
        }

        return Encoding.UTF8.GetString(blob, 0, list.Count);
    }


    static long findNumberE(long m,long d){
        long e = 1;
 
        while (true){
            if ((e * d) % m == 1 && e!=d) break;
            e++;
        }
 
        return e;
    }
    static long findNumberD(long m){
        long d = m - 1;

        long i = 2;
        double numX = (double)m;
        while(true){
            if ((m % i == 0) && (d % i == 0)){
                d--;
                i = 1;
            }
            if (i>=numX) return d;
            numX = ((double)m)/i;
            i++;
        }
    }
    
    
    static long getSimpleNumber(){
        Random rand = new Random();
        long randomNumber = (long)rand.Next(2,100);

        int i = 1; 
        while (true){
            if (isSimple(randomNumber)) break;
            if (i%2==0){
                randomNumber+=i;
            }else{
                randomNumber-=i;
            }
            i++;
        }

        return randomNumber;
    }
    static bool isSimple(long num)
    {
        if (num<2) return false;
        if (num==2) return true;
        if (num==3) return true;

        long i = 2;
        long numX = num;
        while (true){
            if (num%i==0) return false;
            if (i>=numX) return true;
            i++;
            numX = num/i;
        }
    }


    public record KeyRSA{
        public long num { get; init; }
        public long mod { get; init; }
    }
}
