﻿
using System.ComponentModel.DataAnnotations.Schema;

namespace org.inek.PeppBrowser.Data.Entities {
    public class Pepp {
        [Column("pe_SK")]
        public string StructureCategory { get; set; }

        [Column("pe_Pepp")]
        public string Code { get; set; }

        [Column("pe_Text")]
        public string Text { get; set; }
    }
}
