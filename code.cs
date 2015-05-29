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
        static string VER = "v0.3";
        static bool testsIsTrue = false;
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
        static double freq = 0.5;
        static void ManyOs()
        {
            int length;
            try
            {
                Console.WriteLine("Input sequence length:");
                length = Convert.ToInt32(Console.ReadLine());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.Beep();
                Console.WriteLine("Завершение работы функции");
                return;
            }
            //START MANY OSCILATORS
            //freq = 0.5;
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
            //заккоментированы строчки вывода дополнительной информации в файлы
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
                    Console.Title = "Oscilator-generator " + VER + " -- doing oscil #" + (i + 1);
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
            Console.Title = "Oscilator-generator " + VER;
            Console.WriteLine("Result sequence of {0} bits is ready! Writing to file...", res.Length);
            StreamWriter asd = new StreamWriter("seq" + finish.Hour + "-" + finish.Minute + "-" + finish.Second + ".txt");
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

            //ChiSq-6
            Console.WriteLine("===========ChiSq-6===========");

            Dictionary<string, int>[] suppDicts = new Dictionary<string, int>[cq];
            if (testsIsTrue)
            {
                for (int j = 0; j < cq; ++j)
                {
                    Console.WriteLine("Oscillator #" + (j + 1));
                    Console.WriteLine();
                    suppDicts[j] = Tests.ChiSq(0.05, bas[j]);
                    Console.WriteLine();
                }
            }

            Dictionary<string, int> mainDict = new Dictionary<string, int>();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Result sequence:");
            Console.ResetColor();
            Console.WriteLine();
            Tests.MonoBit(res, 0.01);
            mainDict = Tests.ChiSq(0.05, res);
            Tests.ChiSqK(0.001, res);
            Tests.ChiSqStab(res);
            Console.WriteLine("Оценка минимальной энтропии: {0:f5} на бит", Tests.MinEntrBound(res));
            //таблица для ТеХ'а
            if (testsIsTrue)
            {
                Console.WriteLine("Создание файла с выходной таблицей...");
                StreamWriter fr = new StreamWriter("ChiSqTabTex.txt");
                fr.WriteLine("Теоретическое количество каждой из шестиграмм -- {0}", length / 6 / 64);
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
            }
            //END MANY OSCILLATORS
        }

        static void IntelOs()
        {
            try
            {
                int length;
                try
                {
                    Console.WriteLine("Input sequence length:");
                    length = Convert.ToInt32(Console.ReadLine());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.Beep();
                    Console.WriteLine("Завершение работы функции");
                    return;
                }
                //ПАРАМЕТРЫ
                Oscil a = new Oscil(1.0, 0.16, 0.01);
                DateTime start = DateTime.Now;
                BitArray res = a.IntelOscillate(length);
                DateTime finish = DateTime.Now;
                TimeSpan genTime = finish - start;
                Console.WriteLine("Result sequence of {0} bits is ready! Writing to file...", res.Length);
                StreamWriter asd = new StreamWriter("seqI" + finish.Hour + "-" + finish.Minute + "-" + finish.Second + ".txt");
                //WriteBits(res);            
                WriteBitsFile(res, asd);
                asd.Close();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("The approximate speed of generation was {0:f3} bps", length / genTime.TotalSeconds);
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Result sequence:\n");
                Console.ResetColor();
                Tests.MonoBit(res, 0.01);
                Dictionary<string, int> mainDict = new Dictionary<string, int>();
                mainDict = Tests.ChiSq(0.05, res);
                Tests.ChiSqK(0.001, res);
                Tests.ChiSqStab(res);
                Console.WriteLine("Оценка минимальной энтропии: {0:f5} на бит", Tests.MinEntrBound(res));
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
            Console.Title = "Oscilator-generator " + VER;
            Console.WriteLine("Добро пожаловать в программу моделирования генераторов случайных чисел");
            string mode;

            while (true)
            {
                Console.WriteLine("МЕНЮ\n1. Генератор по многоосцилляторной схеме (текущая частота считывания {0})\n2. Генератор Intel\n3. Изменить настройки многоосцилляторного генератора\n0. Выход", freq);
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
                    case "3":
                        Console.WriteLine("Введите новую частоту");
                        try
                        {
                            freq = Convert.ToDouble(Console.ReadLine());
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Console.WriteLine("Частота не изменена");
                        }
                        Console.WriteLine("Тестировать ли составляющие осцилляторы y/n?");
                        mode = Console.ReadLine();
                        switch (mode)
                        {
                            case "y":
                                testsIsTrue = true;
                                break;
                            case "n":
                                testsIsTrue = false;
                                break;
                            default:
                                Console.WriteLine("Недопустимый ответ");
                                break;
                        }
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
