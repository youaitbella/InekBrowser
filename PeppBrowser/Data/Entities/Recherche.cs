﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.inek.PeppBrowser.Data.Entities {
    public class Recherche {
        [Column("re_Code")]
        public string Code { get; set; }

        [Column("re_Text")]
        public string Text { get; set; }

        [Column("re_Hauptdiagnose")]
        public int PrimaryDaignosis { get; set; }

        [Column("re_Nebendiagnose")]
        public int SecondaryDiagnosis { get; set; }

        [Column("re_Prozedur")]
        public int Procedure { get; set; } 
    }
}
