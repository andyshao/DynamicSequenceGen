using System;
using System.Collections.Generic;
using System.Threading;

namespace DynamicSequenceGen
{
    class PoolRunCount
    {
        /// <summary>
        /// 当前数量
        /// </summary>
        public long CurrentCount { get; set; }

        /// <summary>
        /// 记录毫秒
        /// </summary>
        public int RecordTime { get; set; }

        /// <summary>
        /// 错误连续次数
        /// </summary>
        public int ErrorCount { get; set; }

        public long CurrentGenTotalCount { get; set; }
    }
    class Program
    {
        static void Main(string[] arg) => Run();
        static bool iscontinue = true;
        static long totalcount = 0;
        static long currentGenCount = 0;
        const long maxcount = 100000000;
        const long intervalcount = 100;
        static long starttick = 0;
        const int errorcountlimit = 3;
        const int everysavecount = 10;
        static Dictionary<string, PoolRunCount> poolcounts = new Dictionary<string, PoolRunCount>();

        private static void Run()
        {
            starttick = System.Environment.TickCount64;
            totalcount = db.fsql.Select<CouponSequenceCode>().Count();
            if (totalcount > maxcount)
            {
                Console.WriteLine("序列码表已经达到最大值，无需生成");
                return;
            }
            var poolcount = Int32.Parse(db.config["setting:ThreadPoolCount"]);

            poolcounts.Clear();
            for (int i = 0; i < poolcount; i++)
            {
                poolcounts.Add(i.ToString(), new PoolRunCount { CurrentCount = 0, RecordTime = System.Environment.TickCount });
                WaitCallback waitCallBack = t =>
                {
                    Console.WriteLine($"当前线程：{t.ToString()} 开始");
                    while (iscontinue)
                    {
                        try
                        {
                            List<CouponSequenceCode> list = new List<CouponSequenceCode> { };
                            //可设置每次提交多少条
                            for (int c = 0; c < everysavecount; c++)
                                list.Add(new CouponSequenceCode { sequenceno = SequenceGen.CouponCodeGen(10), createby = "auto", status = 0, });
                            db.fsql.Insert<CouponSequenceCode>(list).ExecuteAffrows();

                            currentGenCount += everysavecount;
                            poolcounts[t.ToString()].CurrentGenTotalCount += everysavecount;
                            poolcounts[t.ToString()].ErrorCount = 0;
                        }
                        catch (Exception ex)
                        {
                            poolcounts[t.ToString()].ErrorCount++;
                            Console.WriteLine($"当前线程：{t.ToString()} 出现连续异常，原因为：{ex.Message} 失败次数:{poolcounts[t.ToString()].ErrorCount}");

                            //如果出现连续错误，则中断循环
                            //if (poolcounts[t.ToString()].ErrorCount >= errorcountlimit)
                            //{
                            //    break;
                            //}
                        }
                        poolcounts[t.ToString()].CurrentCount += everysavecount;
                        totalcount += everysavecount;
                        if (intervalcount <= poolcounts[t.ToString()].CurrentCount)
                        {
                            poolcounts[t.ToString()].CurrentCount = 0;
                            Console.WriteLine($"当前线程：{t.ToString()} 总记录条数：{totalcount} 当前生成总数：{currentGenCount} 当前线程生成总数：{poolcounts[t.ToString()].CurrentGenTotalCount} 当次耗时：{System.Environment.TickCount - poolcounts[t.ToString()].RecordTime}毫米 总耗时：{(System.Environment.TickCount64 - starttick) / 1000}秒");
                        }
                        poolcounts[t.ToString()].RecordTime = System.Environment.TickCount;
                        //如果总数达到某个值，则自动停止 iscontinue=false
                        if (totalcount > maxcount)
                        {
                            iscontinue = false;
                            Console.WriteLine("生成结束：序列码表已经达到最大值，无需生成");
                        }
                    }
                };
                ThreadPool.QueueUserWorkItem(waitCallBack, i.ToString());
            }

            string ctrl = "";
            do
            {
                Console.Write("输入（close）关闭当前生成：");
                ctrl = Console.ReadLine();
                iscontinue = ctrl != "close";
                Console.ReadKey();
            } while (iscontinue);
        }
    }
}
