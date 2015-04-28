using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TestingLib
{
    /// <summary>
    /// Класс для моделирования нормально распределенной случайной величины
    /// </summary>
    public class Gauss
    {
        static Random rnd = new Random();
        double m, sigma;                       //Матоджидание и среднеквадратическое отколонение
        public Gauss(double m, double sigma)
        {
            this.m = m;
            this.sigma = sigma;
        }
        /// <summary>
        /// Метод генерации массива нормально распределенных величин с параметрами объекта
        /// </summary>
        /// <param name="n">Длина массива</param>
        /// <returns>Массив величин</returns>
        public double[] GenArray(int n)
        {
            //http://habrahabr.ru/post/208684/
            double[] array = new double[n];
            double x, y, s, r;
            for (int i = 0; i < array.Length; i += 2)
            {
                do
                {
                    x = 2 * rnd.NextDouble() - 1;
                    y = 2 * rnd.NextDouble() - 1;
                    s = x * x + y * y;
                }
                while (s > 1 || s == 0);
                r = Math.Sqrt(-2 * Math.Log(s) / s);
                array[i] = m + sigma * x * r;
                if (i < n - 1)
                {
                    array[i + 1] = m + sigma * y * r;
                }
            }
            return array;
        }
    }

    /// <summary>
    /// Класс, моделирующий осциллятор
    /// </summary>
    public class Oscil
    {
        double workTime, freq;
        public double time, sigma;
        public Oscil(double w_t, double t, double sigma, double freq)
        {
            this.workTime = w_t;
            this.time = t;
            this.sigma = sigma;
            this.freq = freq;
        }
        /// <summary>
        /// Поиск интервала, в который попадает величина
        /// </summary>
        /// <param name="r">Величина</param>
        /// <param name="a">Массив с границами интервалов (должен быть упорядоченным)</param>
        /// <returns>Номер интервала (нумерация с 1)</returns>
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
        /// <summary>
        /// Метод моделирования процесса осциллирования.
        /// </summary>
        /// <param name="str">Поток для записи длин тактов</param>
        /// <returns>Массив битов, выданный осциллятором</returns>
        public BitArray Oscilate(StreamWriter str)
        {
            Gauss gen = new Gauss(time, sigma);
            int n = (int)(workTime / time) * 2;   //берётся с запасом, в два раза больше?
            double[] ar = gen.GenArray(n);
            double[] takts = new double[n + 1];


            //опасная штука, что если в ar[] затесались отрицательные величины?


            takts[0] = 0;
            for (int i = 1; i < takts.Length; ++i)
            {
                takts[i] = takts[i - 1] + ar[i - 1];
            }
            for (int i = 1; i < takts.Length; ++i)
            {
                str.WriteLine(i + ") " + takts[i - 1] + "+" + (ar[i - 1]) + "=" + takts[i]);
                if (takts[i] > workTime)
                    break;
            }

            double now = freq;                      //частота считываний
            int count = (int)(workTime / freq);
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


        //working on IT
        public BitArray IntelOscillate()
        {
            Gauss gen = new Gauss(time, sigma);
            int n = (int)(workTime / time) * 2;   //берётся с запасом, в два раза больше?
            double[] ar = gen.GenArray(n);
            double[] takts = new double[n];


            //опасная штука, что если в ar[] затесались отрицательные величины?


            takts[0] = ar[0];
            for (int i = 1; i < takts.Length; ++i)
            {
                takts[i] = takts[i - 1] + ar[i];
            }
            for (int i = 1; i < takts.Length; ++i)
            {
                if (takts[i] > workTime)
                {
                    Array.Resize(ref takts, i);
                    break;
                }
            }

            int fast_n = (int)Math.Ceiling(workTime / freq);
            double[] fast_takts = new double[fast_n];
            fast_takts[0] = 0;
            for (int k = 1; k < fast_n; ++k)
            {
                fast_takts[k] = fast_takts[k - 1] + freq;
            }
            double now;                      
            int count = takts.Length;
            bool[] bits = new bool[count];
            int pos;
            for (int j = 0; j < count; ++j)
            {
                now = takts[j];
                pos = FindRead(now, fast_takts);
                if (now <= fast_takts[pos] - freq / 2)
                    bits[j] = false;
                else
                    bits[j] = true;
            }
            BitArray osc = new BitArray(bits);
            return osc;
        }
    }

    public static class Tests
    {
        // for monobit
        static double FrequencyTest(double sum, long length)
        {
            double testStat = Math.Abs(sum) / Math.Sqrt(length);
            double rootTwo = 1.414213562373095;
            double pValue = ErrorFunctionComplement(testStat / rootTwo);
            return pValue;
        }
        // for monobit
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
        /// <summary>
        /// Тест частот (монобит)
        /// </summary>
        /// <param name="res">Битовая последовательность</param>
        /// <param name="alpha">Уровень значимости</param>
        public static void MonoBit(BitArray res, double alpha)
        {
            double ones = 0;
            for (int i = 0; i < res.Length; ++i)
            {
                if (res[i] == true)
                    ones++;
            }
            double zeros = res.Length - (int)ones;
            double pval = FrequencyTest(ones - zeros, res.Length);
            Console.WriteLine(ones / res.Length + "   " + pval);
            if (pval < alpha) //для этого теста обычно берут alpha=0.01 
                Console.WriteLine("H1 monobit");
            else
                Console.WriteLine("H0 monobit");
        }

        /// <summary>
        /// Этот метод вычисляет обратную функцию распредения для Гауссовского распределения
        /// </summary>        
        static double gcdfi(double arg)
        {
            double p, eta, xnum = 0, denom = 1, etap = 1, gcdfi1;
            int i;
            double[] a = { 2.515517, 0.802853, 0.010328 };
            double[] b = { 1.432788, 0.189269, 0.001308 };

            p = arg;
            if (p > 0.5) p = 1.0 - p;
            eta = Math.Sqrt(-2 * Math.Log(p));
            for (i = 0; i < 3; ++i)
            {
                xnum += a[i] * etap;
                etap *= eta;
                denom += b[i] * etap;
            }
            gcdfi1 = xnum / denom - eta;
            return (p != arg) ? -gcdfi1 : gcdfi1;
        }

        /// <summary>
        /// Этот метод вычисляет обратную функцию распределения хи-квадрат 
        /// </summary>
        /// <param name="p">Значение хи-квадрат</param>
        /// <param name="n">Степени свободы</param>
        /// <returns>Значение обратной функции</returns>
        private static double FInv(double p, int n)
        {
            double[] c = {1.565326e-3,1.060438e-3,-6.950356e-3,
		                 -1.323293e-2,2.277679e-2,-8.986007e-3,-1.513904e-2,2.530010e-3,
		                 -1.450117e-3,5.169654e-3,-1.153761e-2,1.128186e-2,2.607083e-2,
		                 -0.2237368,9.780499e-5,-8.426812e-4,3.125580e-3,-8.553069e-3,
		                  1.348028e-4,0.4713941,1.0000886};
            double[] a = {1.264616e-2,-1.425296e-2,1.400483e-2,
		                 -5.886090e-3,-1.091214e-2,-2.304527e-2,3.135411e-3,-2.728484e-4,
		                 -9.699681e-3,1.316872e-2,2.618914e-2,-0.2222222,5.406674e-5,
		                  3.483789e-5,-7.274761e-4,3.292181e-3,-8.729713e-3,0.4714045,1.0};
            double ch1, f, f1, t, f2;

            if (n < 2)
            {
                ch1 = gcdfi(0.5 * (1.0 - p));
                return ch1 * ch1;
            }
            else if (n == 2)
            {
                return -2.0 * Math.Log(1.0 - p);
            }
            else
            {
                f = n;
                f1 = 1.0 / f;
                t = gcdfi(p);
                f2 = Math.Sqrt(f1) * t;

                if (n <= (2 + (int)(4.0 * Math.Abs(t))))
                {
                    ch1 = (((((((c[0] * f2 + c[1]) * f2 + c[2]) * f2 + c[3]) * f2 + c[4]) * f2 + c[5]) * f2
                    + c[6])) * f1 + ((((((c[7] + c[8] * f2 + c[9]) * f2 + c[10]) * f2 + c[11]) * f2
                    + c[12]) * f2 + c[13])) * f1 + (((((c[14] * f2 + c[15]) * f2 + c[16]) * f2 +
                    c[17]) * f2 + c[18]) * f2 + c[19]) * f2 + c[20];
                }
                else
                {
                    ch1 = (((a[0] + a[1] * f2) * f1 + (((a[2] + a[3] * f2) * f2 + a[4]) * f2 + a[5])) *
                    f1 + (((((a[6] + a[7] * f2) * f2 + a[8]) * f2 + a[9]) * f2 + a[10]) * f2 + a[11]))
                    * f1 + (((((a[12] * f2 + a[13]) * f2 + a[14]) * f2 + a[15]) * f2 + a[16]) * f2
                    * f2 + a[17]) * f2 + a[18];
                }
                return ch1 * ch1 * ch1 * f;
            }
        }

        /// <summary>
        /// Метод инициализирует словарь-параметр шестиграммами в качестве ключей.
        /// </summary>
        /// <param name="dict">Словарь-параметр</param>        
        /// <returns></returns>
        private static void LoadDictionary(Dictionary<string, int> dict)
        {
            for (int i = 0; i < 64; ++i)
            {
                dict[grams[i]] = 0;
            }
        }
        static string[] grams = new string[64];
        static Tests()
        {
            for (byte a = 0; a < 64; a++)
            {
                grams[a] = Convert.ToString(a, 2).PadLeft(6, '0');
            }
            //StreamReader gram = new StreamReader("grams.b");
            //for (int i = 0; i < 64; ++i)
            //{
            //    grams[i] = gram.ReadLine();
            //}
            //gram.Close();
        }

        private static string SixGr(bool[] mas)
        {
            string str = "";
            for (int i = 0; i < mas.Length; ++i)
                str += (mas[i] == true ? 1 : 0);
            return str;
        }
        public static Dictionary<string, int> ChiSq(int len, double alpha, BitArray arr)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            LoadDictionary(dict);
            int count = len / 6;
            string s;
            bool[] mas = new bool[6];
            for (int k = 0; k < count; ++k)
            {
                for (int i = 0; i < 6; ++i)
                    mas[i] = arr[6 * k + i];
                s = SixGr(mas);
                if (dict.ContainsKey(s))
                    dict[s]++;
                else
                    dict[s] = 1;
            }
            double x = (double)len / 6 / Math.Pow(2, 6);
            double delta2 = FInv(1 - alpha, 63);
            double sum = 0;
            foreach (KeyValuePair<string, int> kvp in dict)
            {
                sum += Math.Pow((kvp.Value - x), 2) / x;
            }
            int chk = dict.Count - (int)Math.Pow(2, 6);
            if (chk > 0)
                sum += chk * x;
            Console.WriteLine("ChiSq= " + sum);
            Console.WriteLine("Delta= " + delta2);
            Console.WriteLine("alpha=" + alpha);
            if (sum < delta2)
                Console.WriteLine("H0 (ChiSq < delta, гипотеза о случайности принята)");
            else Console.WriteLine("H1 (ChiSq > delta, гипотеза о случайности  не принята)");
            return dict;
        }
    }
}