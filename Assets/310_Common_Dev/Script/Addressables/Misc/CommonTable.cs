using FlexFramework.Excel;
using System;

namespace DoDoEng
{
    [Serializable, Table(1, SafeMode = true)]
    public class TableVersion
    {
        [Column("A")] public string Revision;
        [Column("B")] public string LastUpdated;
        [Column("C")] public string Comment;

        public override string ToString()
        {
            return $"{Revision} | {LastUpdated} | {Comment}";
        }
    }

}