using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using FreeSql;
using FreeSql.DataAnnotations;

namespace DynamicSequenceGen
{
    [Index("uk_CouponSequenceCode_sequenceno", "sequenceno", true)]
    public class CouponSequenceCode
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public long id { get; set; }

        [MaxLength(50)]
        public string sequenceno { get; set; }

        public int status { get; set; }

        public DateTime createdt { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string createby { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(200)]
        public string remark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? usetime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [MaxLength(50)]
        public string userid { get; set; }
    }
}
