using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicSequenceGen
{
    public class SequenceGen
    {
        public static string CouponCodeGen(int length)
        {
            var result = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                var r = new Random(Guid.NewGuid().GetHashCode());
                result.Append(r.Next(0, 10));
            }
            return result.ToString();
        }
    }
}
