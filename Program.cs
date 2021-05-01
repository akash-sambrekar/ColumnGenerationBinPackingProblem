using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILOG.Concert;
using ILOG.CPLEX;

namespace BPP
{
    class Program
    {
        static void Main(string[] args)
        {
            //Added from local
            double BinCap = 0;
            var dict = InputReader.ReadDataFile(ref BinCap);

            Cplex cplex = new Cplex();
            Dictionary<string, INumVar> dictvariables = new Dictionary<string, INumVar>();
            Dictionary<string, IRange> dictconstraints = new Dictionary<string, IRange>();
            IObjective objective = cplex.AddMinimize();

            foreach ( var  vari in dict.Keys)
            {
                var cname = "C" + vari;
                dictconstraints.Add(cname, cplex.AddRange(1, Int32.MaxValue, cname));
            }

            Dictionary<int, Set> InitialSets = new Dictionary<int, Set>(); ;
            InitialSets.Add(1,new Set(new List<int> { 1, 5 }));
            InitialSets.Add(2,new Set(new List<int> { 2, 5 }));
            InitialSets.Add(3,new Set(new List<int> { 3, 5 }));
            InitialSets.Add(4,new Set(new List<int> { 4, 5 }));

            //Add intial sets to the model
            foreach( var vari in InitialSets)
            {
                var setID = vari.Key.ToString();
                Column VarSet = cplex.Column(objective, 1);
                foreach( var members in vari.Value.member)
                {
                    var cname = "C" + members;
                    VarSet = VarSet.And(cplex.Column(dictconstraints[cname], 1));
                }

                dictvariables.Add(setID, cplex.NumVar(VarSet, 0, 1, NumVarType.Float));
            }
            cplex.Solve();
            var duals = getDuals(cplex, dictconstraints);
            var solution = getSolution(cplex, dictvariables);
            Console.WriteLine("The objective value is {0}",cplex.GetObjValue());

            int piter = 0;
            while (true)
            {
                //Formulate Pricing Problem 
                Cplex pcplex = new Cplex();
                IObjective pobjective = pcplex.AddMaximize();
                piter++;
                //Add Bin Capacity Constraint
                IRange Knapsack = pcplex.AddRange(0, BinCap, "Bin");
                Dictionary<string, INumVar> pdictvar = new Dictionary<string, INumVar>();
                foreach (var vari in dict.Keys)
                {
                    var varname = vari.ToString();
                    var objcoeff = duals["C" + varname];
                    Column item = pcplex.Column(pobjective, objcoeff);
                    item = item.And(pcplex.Column(Knapsack, dict[vari]));
                    pdictvar.Add(varname, pcplex.NumVar(item, 0, 1, NumVarType.Int));
                }

                pcplex.Solve();
                if (pcplex.GetObjValue() > 1)
                {
                    Console.WriteLine("Pricing Iteration: {0} and obj value is {1} ", piter, pcplex.GetObjValue());
                    var psolution = getSolution(pcplex, pdictvar);
                    List<int> sol = new List<int>();
                    foreach (var vari in psolution.Keys)
                    {
                        sol.Add(Convert.ToInt32(vari));
                    }
                    InitialSets.Add(InitialSets.Count + 1, new Set(sol));

                    var setID = (InitialSets.Count).ToString();
                    Column VarSet1 = cplex.Column(objective, 1);
                    foreach (var members in sol)
                    {
                        var cname = "C" + members;
                        VarSet1 = VarSet1.And(cplex.Column(dictconstraints[cname], 1));
                    }

                    dictvariables.Add(setID, cplex.NumVar(VarSet1, 0, 1, NumVarType.Float));
                    
                    cplex.Solve();
                    Console.WriteLine("The objective value of cplex after adding column  is {0}", cplex.GetObjValue());
                    duals = getDuals(cplex, dictconstraints);
                    solution = getSolution(cplex, dictvariables);
                }
                else
                {
                    break;
                }

            }

            solution = getSolution(cplex, dictvariables);
            Console.WriteLine("The objective value is {0}", cplex.GetObjValue());

            //Begin Fixing a few variables

        }

        public static Dictionary<string,double> getDuals(Cplex cplex, Dictionary<string,IRange> dictconst )
        {
            Dictionary<string, double> duals = new Dictionary<string, double>();
            foreach( var constt in dictconst)
            {
                var val = cplex.GetDual(constt.Value);
                duals.Add(constt.Key, val);
            }
            return duals;
        }

        public static Dictionary<string,double> getSolution(Cplex cplex, Dictionary<string,INumVar> dictvar)
        {
            Dictionary<string, double> sol = new Dictionary<string, double>();
            foreach( var vari in dictvar)
            {
                var val = cplex.GetValue(vari.Value);
                if (val >0)
                {
                    sol.Add(vari.Key, val);
                }
                

            }
            return sol;
        }
        
    }
}
