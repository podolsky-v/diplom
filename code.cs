using System;
using System.IO;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace program
{   
    
    class Gauss
    {
        double m, sigma;
        static Random rnd = new Random();
        public Gauss(double m, double sigma)
        {
            this.m = m;
            this.sigma = sigma;
        }
        public double[] genArray(int n)
        {
            double[] array = new double[n];
            double x, y, s, z0, z1, r;
            for (int i = 0; i < array.Length; i += 2)
            {
                do
                {
                    x = 2 * rnd.NextDouble() - 1;
                    y = 2 * rnd.NextDouble() - 1;
                    s = x * x + y * y;
                }
                while (s > 1 || s == 0);
                r = Math.Sqrt(-2 * Math.Log(s) / s); ;
                z0 = x * r;
                z1 = y * r;
                array[i] = m + sigma * z0;
                if (i < n - 1)
                {
                    array[i + 1] = m + sigma * z1;
                }
            }
            return array;
        }        
    }
    class Oscil
    {
        double w_t, t, sigma, freq;
        public Oscil(double w_t, double t, double sigma, double freq)
        {
            this.w_t = w_t;
            this.t = t;
            this.sigma = sigma;
            this.freq = freq;
        }
        static int FindRead(double r, double[] a)
        {
            if (r == 0)
                return 1;
            int num = -1;
            int i = 1;
            while (i < a.Length)
            {
                if ((a[i - 1] < r) && (a[i] >= r))
                {
                    num = i;
                    break;
                }
                else
                    i++;
            }
            return num;
        }
        public BitArray oscilate(StreamWriter str)
        {
            Gauss gen = new Gauss(t, sigma);
            int n = (int)(w_t / t) * 2;
            double[] ar = gen.genArray(n);
            double[] takts = new double[n + 1];
            takts[0] = 0;
            for (int i = 1; i < takts.Length; ++i)
            {
                takts[i] = takts[i - 1] + ar[i - 1];
            }
            //str.WriteLine(0 + ";");
            for (int i = 1; i < takts.Length; ++i)
            {
                str.WriteLine(i + ") " + takts[i - 1] + "+" + (ar[i - 1]) + "=" + takts[i]);
                //str.WriteLine(takts[i]-(takts[i]-takts[i-1])/2 + ";");
                //str.WriteLine(takts[i]+";");
                if (takts[i] > w_t)
                    break;
            }
            double now = freq;
            int count = (int)(w_t / freq);
            bool[] bits = new bool[count];
            int pos;
            for (int j = 0; j < count; ++j)
            {
                pos = FindRead(now, takts);
                if (now <= takts[pos] - (takts[pos] - takts[pos - 1]) / 2)
                    bits[j] = false;
                else
                    bits[j] = true;
                now += freq;
            }
            BitArray osc = new BitArray(bits);
            return osc;
        }
    }
    class Program
    {
        //monobit
        static double FrequencyTest(double sum, long lenght)
        {            
            double testStat = Math.Abs(sum) / Math.Sqrt(lenght);
            double rootTwo = 1.414213562373095;
            double pValue = ErrorFunctionComplement(testStat / rootTwo);
            return pValue;
        }
        static double ErrorFunction(double x)
        {
            // assume x > 0.0
            // Abramowitz and Stegun eq. 7.1.26
            double p = 0.3275911;
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double t = 1.0 / (1.0 + p * x);
            double err = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);
            return err;
        }
        static double ErrorFunctionComplement(double x)
        {
            return 1 - ErrorFunction(x);
        }
        //monobit end

        //ChiSq
        private static string sixGr(bool[] mas)
        {
            string str = "";
            for (int i = 0; i < mas.Length; ++i)
                str += (mas[i] == true ? 1 : 0);
            return str;
        }
        private static double fInv(double al, int n)
        {
            double d = 0;
            if ((al <= 0.999) && (al >= 0.5))
                d = 2.0637 * Math.Pow((Math.Log(1.0 / (1.0 - al)) - 0.16), 0.4274) - 1.5774;
            if ((al >= 0.001) && (al < 0.5))
                d = -2.0637 * Math.Pow((Math.Log(1.0 / al) - 0.16), 0.4274) + 1.5774;
            double A = d * Math.Sqrt(2);
            double B = 2 * (d * d - 1) / 3;
            double C = d * (d * d - 7) / 9 / Math.Sqrt(2);
            double D = (6 * d * d * d * d + 14 * d * d - 32) / 405;
            double E = d * (9 * d * d * d * d + 256 * d * d - 433) / 4860 / Math.Sqrt(2);
            return (n + A * Math.Sqrt(n) + B + C / Math.Sqrt(n) + D / n + E / n / Math.Sqrt(n));
        }
        //END ChiSq

        static void WriteBitsFile(BitArray bits, StreamWriter s)
        {
            foreach (bool b in bits)
                s.Write(b ? 1 : 0);
            s.WriteLine("\n");
        }
        static void WriteBits(BitArray bits)
        {
            foreach (bool b in bits)
                Console.Write(b ? 1 : 0);
            Console.WriteLine("\n");
        }       
        static void Main()
        {

            double w_t = 5000;
            double freq = 0.5;
            StreamReader r = new StreamReader("input.txt");
            int cq = Convert.ToInt32(r.ReadLine());
            double[] ts = new double[cq];
            double[] sigs = new double[cq];
            for (int i = 0; i < cq; ++i)
            {
                r.ReadLine();
                ts[i] = Convert.ToDouble(r.ReadLine());
                sigs[i] = Convert.ToDouble(r.ReadLine());
            }
            r.Close();
            StreamWriter[] streamwriters = new StreamWriter[cq];
            Oscil[] oscils = new Oscil[cq];
            BitArray[] bas = new BitArray[cq];
            for (int i = 0; i < cq; ++i)
            {
                streamwriters[i] = new StreamWriter("out" + (i + 1) + ".txt");
                oscils[i] = new Oscil(w_t, ts[i], sigs[i], freq);
                bas[i] = oscils[i].oscilate(streamwriters[i]);
                streamwriters[i].Close();
            }
            for (int i = 0; i < cq; ++i)
                WriteBits(bas[i]);
            Console.WriteLine("=============Results after Xor=========");
            BitArray res = new BitArray(bas[0]);
            for (int i = 1; i < cq; ++i)
            {
                res = res.Xor(bas[i]);
            }
            StreamWriter asd = new StreamWriter("seq.txt");
            WriteBits(res);
            WriteBitsFile(res, asd);
            asd.Close();

            //MONOBIT
            Console.WriteLine("===========MONOBIT========");
            double ones = 0;
            for (int i = 0; i < res.Length; ++i)
            {
                if (res[i] == true)
                    ones++;
            }
            double zeros = res.Length - (int)ones;
            double pval = FrequencyTest(ones - zeros, res.Length);
            Console.WriteLine(ones / res.Length + "   " + pval);
            if (pval < 0.05) //alpha
                Console.WriteLine("H1 monobit");
            else
                Console.WriteLine("H0 monobit");

            //ChiSq-6
            Console.WriteLine("===========ChiSq-6========");
            int N = res.Length;
            Dictionary<string, int> dict = new Dictionary<string, int>();

            //Другие словари для построения таблицы
            Dictionary<string, int>[] dict1 = new Dictionary<string, int>[cq];
            for (int j = 0; j < cq; ++j)
                dict1[j] = new Dictionary<string, int>();

            int count = N / 6;
            bool[] mas = new bool[6];
            string[] grams = new string[64];
            StreamReader gram = new StreamReader("grams.txt");
            for (int i = 0; i < grams.Length; ++i)
            {
                grams[i] = gram.ReadLine();
                dict[grams[i]] = 0;

                //словари
                for (int j = 0; j < cq; ++j)
                    dict1[j][grams[i]] = 0;
            }
            gram.Close();
            string s;
            for (int k = 0; k < count; ++k)
            {
                for (int i = 0; i < 6; ++i)
                    mas[i] = res[6 * k + i];
                s = sixGr(mas);
                if (dict.ContainsKey(s))
                    dict[s]++;
                else
                    dict[s] = 1;
            }

            //другие словари
            for (int j = 0; j < cq; ++j)
            {
                for (int k = 0; k < count; ++k)
                {
                    for (int i = 0; i < 6; ++i)
                        mas[i] = bas[j][6 * k + i];
                    s = sixGr(mas);
                    if (dict1[j].ContainsKey(s))
                        dict1[j][s]++;
                    else
                        dict1[j][s] = 1;
                }
            }

            StreamWriter fr = new StreamWriter("tab.txt");
            fr.Write("\\begin{longtable}{|l|");
            for (int j = 0; j < cq; ++j)
                fr.Write("l|");
            fr.WriteLine("l|}");
            fr.WriteLine("\\hline");
            foreach (var pair in dict.OrderBy(pair => pair.Key))
            {
                //fr.WriteLine("{0} & {1} & {2} & {3} & {4}\\\\", pair.Key, dict1[pair.Key], dict2[pair.Key], dict3[pair.Key], dict[pair.Key]);
                fr.Write(pair.Key);
                for (int j = 0; j < cq; ++j)
                    fr.Write(" & {0}", dict1[j][pair.Key]);
                fr.WriteLine(" & {0}\\\\", dict[pair.Key]);
                fr.WriteLine("\\hline");
                //Console.WriteLine("{0} - {1} - {2} - {3} - {4}", pair.Key, dict1[pair.Key], dict2[pair.Key], dict3[pair.Key], dict[pair.Key]);
            }
            fr.WriteLine("\\end{longtable}");
            fr.Close();

            double x = (double)N / 6 / Math.Pow(2, 6);
            double alpha = 0.05;
            double delta2 = fInv(1 - alpha, 63);

            double sum = 0;
            foreach (KeyValuePair<string, int> kvp in dict)
            {
                //Console.WriteLine("{0}\t{1}", kvp.Key, kvp.Value);
                sum += Math.Pow((kvp.Value - x), 2) / x;
            }
            int chk = dict.Count - (int)Math.Pow(2, 6);
            if (chk > 0)
                sum += chk * x;
            Console.WriteLine("All oscillators");
            Console.WriteLine("ChiSq= " + sum);
            Console.WriteLine("Delta= " + delta2);
            Console.WriteLine("alpha=" + alpha);
            if (sum < delta2) { Console.WriteLine("H0 (ChiSq)"); }
            else { Console.WriteLine("H1 (ChiSq)"); }
            Console.WriteLine();

            for (int j = 0; j < cq; ++j) 
            {
                sum = 0;
                foreach (KeyValuePair<string, int> kvp in dict1[j])
                {
                    sum += Math.Pow((kvp.Value - x), 2) / x;
                }
                chk = dict1[j].Count - (int)Math.Pow(2, 6);
                if (chk > 0)
                    sum += chk * x;
                Console.WriteLine("Oscillator #"+(j+1));
                Console.WriteLine("Mju = "+ts[j]+" , sigma = "+sigs[j]);
                Console.WriteLine("ChiSq= " + sum);
                Console.WriteLine("Delta= " + delta2);
                Console.WriteLine("alpha=" + alpha);
                if (sum < delta2) { Console.WriteLine("H0 (ChiSq)"); }
                else { Console.WriteLine("H1 (ChiSq)"); }
                Console.WriteLine();
            }

            Console.ReadLine();
        }        
    }
}
