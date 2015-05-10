using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TestingLib;

namespace DiplomaOscil
{
    class Program
    {
        static void WriteBitsFile(BitArray bits, StreamWriter s)
        {
            foreach (bool b in bits)
                s.Write(b ? 1 : 0);
        }
        static void WriteBits(BitArray bits)
        {
            foreach (bool b in bits)
                Console.Write(b ? 1 : 0);
            Console.WriteLine("\n");
        }
        static void ManyOs()
        {
            Console.WriteLine("Input sequence length:");
            int length = Convert.ToInt32(Console.ReadLine());

            //START MANY OSCILATORS
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
            //StreamWriter[] streamwriters = new StreamWriter[cq];
            //StreamWriter[] streamwritersB = new StreamWriter[cq];
            Oscil[] oscils = new Oscil[cq];
            BitArray[] bas = new BitArray[cq];
            //Directory.CreateDirectory("takts");
            //Directory.CreateDirectory("bit");
            DateTime start = DateTime.Now;
            for (int i = 0; i < cq; ++i)
            {
                //streamwriters[i] = new StreamWriter("takts\\out" + (i + 1) + ".txt");
                //streamwritersB[i] = new StreamWriter("bit\\bits" + (i + 1) + ".txt");
                oscils[i] = new Oscil(ts[i], sigs[i], freq);
                try
                {
                    Console.Title = "Super-mega Oscilator-generator v0.666 -- doing oscil #" + (i + 1);
                    //bas[i] = oscils[i].Oscilate(streamwriters[i], length);
                    bas[i] = oscils[i].Oscilate(length);
                    //Console.WriteLine("Oscillator #" + (i + 1) + " done!");
                    //WriteBitsFile(bas[i], streamwritersB[i]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.Beep();
                    Console.WriteLine("Завершение работы функции");                   
                    return;
                }
                finally
                {
                    //streamwriters[i].Close();
                    //streamwritersB[i].Close();
                }
            }
            //Console.WriteLine("==========Results after Xor=========");
            BitArray res = new BitArray(bas[0]);
            for (int i = 1; i < cq; ++i)
            {
                res = res.Xor(bas[i]);
            }
            DateTime finish = DateTime.Now;
            Console.Title = "Super-mega Oscilator-generator v0.666";
            Console.WriteLine("Result sequence of {0} bits is ready! Writing to file...", res.Length);
            StreamWriter asd = new StreamWriter("seq.txt");
            //WriteBits(res);            
            WriteBitsFile(res, asd);
            asd.Close();
            TimeSpan genTime = finish - start;
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("The approximate speed of generation was {0:f3} bps", length / genTime.TotalSeconds);
            Console.WriteLine();
            Console.ResetColor();

            Console.WriteLine("***HEALTH TESTS***");
            //MONOBIT
            Console.WriteLine("===========MONOBIT===========");
            Tests.MonoBit(res, 0.01);

            //ChiSq-6
            Console.WriteLine("===========ChiSq-6===========");
            int N = res.Length;

            Dictionary<string, int>[] suppDicts = new Dictionary<string, int>[cq];
            for (int j = 0; j < cq; ++j)
            {
                Console.WriteLine("Oscillator #" + (j + 1));
                Console.WriteLine();
                suppDicts[j] = Tests.ChiSq(N, 0.05, bas[j]);
                Console.WriteLine();
            }

            Dictionary<string, int> mainDict = new Dictionary<string, int>();
            Console.WriteLine("Result sequence:");
            Console.WriteLine();
            mainDict = Tests.ChiSq(N, 0.05, res);
            Console.WriteLine();

            //таблица для ТеХ'а
            StreamWriter fr = new StreamWriter("tab.txt");
            fr.Write("\\begin{longtable}{|l|");
            for (int j = 0; j < cq; ++j)
                fr.Write("l|");
            fr.WriteLine("l|}");
            fr.WriteLine("\\hline");
            fr.Write("sixgr");
            for (int j = 0; j < cq; ++j)
                fr.Write(" & m={0},si={1}", oscils[j].time, oscils[j].sigma);
            fr.WriteLine(" & all\\\\");
            foreach (var pair in mainDict.OrderBy(pair => pair.Key))
            {
                fr.Write(pair.Key);
                for (int j = 0; j < cq; ++j)
                    fr.Write(" & {0}", suppDicts[j][pair.Key]);
                fr.WriteLine(" & {0}\\\\", mainDict[pair.Key]);
                fr.WriteLine("\\hline");
            }
            fr.WriteLine("\\end{longtable}");
            fr.Close();
            //END MANY OSCILLATORS
        }

        static void IntelOs()
        {
            try
            {
                Console.WriteLine("Input sequence length:");
                int length = Convert.ToInt32(Console.ReadLine());
                Oscil a = new Oscil(1.0, 0.16, 0.01);
                BitArray res = a.IntelOscillate(length);
                Console.WriteLine("Result sequence of {0} bits is ready! Writing to file...", res.Length);
                StreamWriter asd = new StreamWriter("seqI.txt");
                //WriteBits(res);            
                WriteBitsFile(res, asd);
                asd.Close();
                Tests.MonoBit(res, 0.01);
                int N = res.Length;
                Dictionary<string, int> mainDict = new Dictionary<string, int>();
                Console.WriteLine("Результирующая последовательность:");
                mainDict = Tests.ChiSq(N, 0.05, res);
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.Beep();
                Console.WriteLine("Завершение работы функции.");
                return;
            }

        }

        static void Main()
        {
            Console.Title = "Super-mega Oscilator-generator v0.666";
            Console.WriteLine("Добро пожаловать в программу моделирования генераторов случайных чисел");
            string mode;

            while (true)
            {
                Console.WriteLine("МЕНЮ\n1. Генератор many-XOR\n2. Генератор Intel\n0. Выход");
                mode = Console.ReadLine();
                switch (mode)
                {
                    case "1":
                        ManyOs();
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadLine();
                        Console.Clear();
                        break;
                    case "2":
                        IntelOs();
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadLine();
                        Console.Clear();
                        break;
                    case "0":
                        Console.WriteLine("Инициирован выход из программы\nPress any key to exit...");
                        Console.ReadKey();
                        return;
                    default:
                        Console.WriteLine("Неправильно набран номер");
                        break;
                }
            }
        }
    }
}
