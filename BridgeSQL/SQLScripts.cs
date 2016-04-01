using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BridgeSQL
{
    public class SQLScripts
    {
        public static string PeepSSP =
@"SELECT COUNT(*)
FROM sys.objects
WHERE object_id = OBJECT_ID(N'{0}')
AND type IN ( N'P', N'PC' )";

        public static string PeepFN =
@"SELECT COUNT(*)
FROM sys.objects
WHERE object_id = OBJECT_ID(N'{0}')
AND type IN ( N'FN', N'IF', 'TF')";
    }

}

//AF = Aggregate function(CLR)
//C = CHECK constraint
//D = DEFAULT(constraint or stand-alone)
//F = FOREIGN KEY constraint
//FN = SQL scalar function
//FS = Assembly(CLR) scalar-function
//FT = Assembly(CLR) table-valued function
//IF = SQL inline table-valued function
//IT = Internal table
//P = SQL Stored Procedure
//PC = Assembly(CLR) stored-procedure
//PG = Plan guide
//PK = PRIMARY KEY constraint
//R = Rule(old-style, stand-alone)
//RF = Replication-filter-procedure
//S = System base table
//SN = Synonym
//SO = Sequence object