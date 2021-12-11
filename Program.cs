using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;

namespace Lab3
{

    class Program
    {
        [DllImport("..\\..\\..\\x64\\Debug\\Dll3.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern double Global_func(double[] v, int n, double[] res_mkl, double[] res, string mode,
                ref double v_time, ref int ret);

        static void Main(string[] args)
        {
            // на 50 и 100 точках tn без mkl быстрее (при точности HA и LA отношение >> 1) 
            // но уже на 1000 отношения времени < 1, след mkl быстрее
            //-------------------------50 points----------------------------
            VMBenchmark bm = new();
            bm.List_Add(0, 10, 50);//[0, 10] 50 points
            Console.WriteLine(bm.ToString());
            bm.SaveAsText("data.txt");
            //------------------------100 points----------------------------
            VMBenchmark bm1 = new();
            bm1.List_Add(-15, 15, 100);//[-15, 15] 100 points
            Console.WriteLine(bm1.ToString());
            bm1.SaveAsText("data.txt");
            //------------------------1000 points----------------------------
            VMBenchmark bm2 = new();
            bm2.List_Add(-100, 100, 1000);//[-100, 100] 1000 points
            Console.WriteLine(bm2.ToString());
            bm2.SaveAsText("data.txt");

        }


        public struct VMTime
        {
            double[] v;//points for tg
            double[] res_mkl;//tg result using mkl
            double[] res;//tg result without mkl
            public VMTime(double[] a)
            {
                this.v = a;
                this.res = new double[a.Length];
                this.res_mkl = new double[a.Length];
            }
            public int VMT_Length
            {
                get
                {
                    return v.Length;
                }
            }

            public double[] Time_res
            {
                get
                {
                    double[] v_time = new double[3];
                    //For mode = VML_HA
                    int ret = -1;
                    string mode = "VML_HA";
                    Global_func(this.v, this.VMT_Length, this.res_mkl, this.res, mode,
                            ref v_time[0], ref ret);
                    //For mode = VML_HA
                    ret = -1;
                    mode = "VML_LA";
                    Global_func(this.v, this.VMT_Length, this.res_mkl, this.res, mode,
                            ref v_time[1], ref ret);
                    //For mode = VML_HA
                    ret = -1;
                    mode = "VML_EP";
                    Global_func(this.v, this.VMT_Length, this.res_mkl, this.res, mode,
                            ref v_time[2], ref ret);
                    return v_time;
                }
            }
        }

        public struct VMAccuracy
        {
            int start;
            int end;
            double[] v;//points for tg from segment [start, end]
            double[] res_mkl_HA;//tg result using mkl HA
            double[] res_mkl_EP;//tg result using mkl EP
            double[] res;//tg result without using mkl
            public VMAccuracy(int st, int e, int n,  double[] mas)//n - number of points
            {
                this.v = mas;
                this.res = new double[n];
                this.res_mkl_HA = new double[n];
                this.res_mkl_EP = new double[n];
                this.start = st;
                this.end = e;
            }
            public int VMAcc_Length
            {
                get
                {
                    return v.Length;
                }
            }
            public double VMAcc_start
            {
                get
                {
                    return this.start;
                }
            }
            public double VMAcc_end
            {
                get
                {
                    return this.end;
                }
            }
            public double VMAcc_max_rel
            {
                get
                {
                    //For mode = VML_HA
                    int ret = -1;
                    string mode = "VML_HA";
                    double time_work = 0;
                    Global_func(this.v, this.VMAcc_Length, this.res_mkl_HA, this.res, mode,
                            ref time_work, ref ret);
                    //For mode = VML_EP
                    ret = -1;
                    mode = "VML_EP";
                    time_work = 0;
                    Global_func(this.v, this.VMAcc_Length, this.res_mkl_EP, this.res, mode,
                            ref time_work, ref ret);
                    //-----------------Searching for max---------------------
                    double max_res = Math.Abs(this.res_mkl_HA[0] - this.res_mkl_EP[0]) /
                            Math.Abs(this.res_mkl_HA[0]);
                    for (int i = 1; i < VMAcc_Length; i++)
                    {
                        double cur_diff = Math.Abs(this.res_mkl_HA[0] - this.res_mkl_EP[0]) /
                            Math.Abs(this.res_mkl_HA[0]);
                        if (cur_diff > max_res)
                        {
                            max_res = cur_diff;
                        }
                    }
                    return max_res;
                }
            }
            public double[] VMAcc_max_diff
            {
                get
                {
                    //For mode = VML_HA
                    int ret = -1;
                    string mode = "VML_HA";
                    double time_work = 0;
                    Global_func(this.v, this.VMAcc_Length, this.res_mkl_HA, this.res, mode,
                            ref time_work, ref ret);
                    //For mode = VML_EP
                    ret = -1;
                    mode = "VML_EP";
                    time_work = 0;
                    Global_func(this.v, this.VMAcc_Length, this.res_mkl_EP, this.res, mode,
                            ref time_work, ref ret);
                    
                    //max diff
                    double max_diff = Math.Abs(this.res_mkl_HA[0] - this.res_mkl_EP[0]);
                    int max_diff_i = 0;
                    for (int i = 1; i < this.VMAcc_Length; i++)
                    {
                        double cur_diff = Math.Abs(this.res_mkl_HA[0] - this.res_mkl_EP[0]);
                        if (cur_diff > max_diff)
                        {
                            max_diff_i = i;
                        }
                    }

                    double[] pair = new double[2];
                    pair[0] = res[max_diff_i];//func val
                    pair[1] = this.v[max_diff_i];//arg val
                    return pair;
                }
            }
        }

        public class VMBenchmark
        {
            List<VMTime> t_list;
            List<VMAccuracy> acc_list;
            public VMBenchmark()
            {
                t_list = new List<VMTime>();
                acc_list = new List<VMAccuracy>();
            }
            public void List_Add(int start, int end, int n)
            {
                double[] v = new double[n];//рандом на отрезке 
                Random x = new Random();
                for (int i = 0; i < n; i++)
                {
                    v[i] = Convert.ToDouble(x.Next(start, end) / 123.0);
                }

                VMTime t_struct = new VMTime(v);
                VMAccuracy acc_struct = new VMAccuracy(start, end, n, v);
                this.t_list.Add(t_struct);
                this.acc_list.Add(acc_struct);
            }
            public override string ToString()
            {
                string res = "VMTime List: \n";
                for (int i = 0; i < this.t_list.Count; i++)
                {
                    double[] t_res = t_list[i].Time_res;
                    string res1 = String.Concat("Length of vector: ", t_list[i].VMT_Length.ToString(),
                            "\n   Time relation for VMT_HA: ", t_res[0].ToString(),
                            "\n   Time relation for VMT_LA: ", t_res[1].ToString(), 
                            "\n   Time relation for VMT_EP: ", t_res[2].ToString());
                    res = String.Concat(res, res1);
                }
                res = String.Concat(res, "\nVMAccyracy List: \n");
                for (int i = 0; i < this.acc_list.Count; i++)
                {
                    double[] diff = acc_list[i].VMAcc_max_diff;
                    string res1 = String.Concat("Length of vector: ", acc_list[i].VMAcc_Length.ToString(),
                            "\n   Max relation: ", acc_list[i].VMAcc_max_rel.ToString(),
                            "\n   Function val with max deviation: ", diff[0].ToString(),
                            "\n   Functon arg with max deviation: ", diff[1].ToString());
                    res = String.Concat(res, res1);
                }
                return res;
            }
            public bool SaveAsText(string filename)
            {
                //save VMBenchmark in filename file
                FileStream fs = null;
                try
                {
                    fs = new FileStream(filename, FileMode.Append);//OpenOrCreate
                    StreamWriter writer = new(fs);
                    writer.WriteLine(this.ToString());
                    writer.Close();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
                finally
                {
                    if (fs != null) fs.Close();
                }
                return true;
            }

        }
    }
}
