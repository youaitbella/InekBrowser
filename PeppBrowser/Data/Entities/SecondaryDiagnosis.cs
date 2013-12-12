﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.inek.PeppBrowser.Data.Entities {
    public class SecondaryDiagnosis {
        [Column("nd_Pepp")]
        public string PeppCode { get; set; }

        [Column("nd_Code")]
        public string DiagCode { get; set; }

        [Column("nd_FaelleAnzahl")]
        public int CaseCount { get; set; }

        [Column("nd_FaelleAnteil")]
        public decimal CaseFraction { get; set; }

        [Column("nd_NennungenAnzahl")]
        public int EntryCount { get; set; }

        [Column("nd_NennungenAnteil")]
        public decimal EntryFraction { get; set; }

        public string Nebendiagnose { get; set; }


    }
}
