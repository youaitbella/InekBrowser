﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.inek.PeppBrowser.Data.Entities {
    public class Procedure {
        [Column("pr_Pepp")]
        public string PeppCode { get; set; }

        [Column("pr_CodeF")]
        public string ProcCode { get; set; }

        [Column("pr_Code")]
        public string ProcCodeShort { get; set; }

        [Column("pr_AnzahlFaelle")]
        public int CaseCount { get; set; }

        [Column("pr_AnteilFaelle")]
        public int CaseFraction { get; set; }

        [Column("pr_AnzahlNennungen")]
        public int EntryCount { get; set; }

        [Column("pr_AnteilNennungen")]
        public int EntryFraction { get; set; }

    }
}
