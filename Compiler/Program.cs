﻿using Compiler.Parser;
using Compiler.Parser.Visitors;
using Compiler.ThreeAddrCode;
using Compiler.ThreeAddrCode.CFG;
using Compiler.ThreeAddrCode.Expressions;
using Compiler.ThreeAddrCode.Nodes;
using Compiler.Optimizations;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Compiler
{
    internal class Program
    {
        public static void Main()
        {
            TaCodeTest();
            ASTTest();

            //Test Moving declarations
            var Test = new DeclarationTest();
            Test.DeclarationOptimizationTest();
        }

        private static void ASTTest()
        {
            Console.OutputEncoding = Encoding.UTF8;
            string fileName = @"..\..\sample.txt";
            try
            {
                string text = File.ReadAllText(fileName);

                var scanner = new Scanner();
                scanner.SetSource(text, 0);

                var parser = new Parser.Parser(scanner);

                var b = parser.Parse();
                Console.WriteLine(!b ? "Ошибка" : "Синтаксическое дерево построено");

                var prettyPrinter = new PrettyPrintVisitor();
                parser.root.Visit(prettyPrinter);
                Console.WriteLine(prettyPrinter.Text);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Файл {fileName} не найден");
            }
            catch (LexException e)
            {
                Console.WriteLine($"Лексическая ошибка. {e.Message}");
            }
            catch (SyntaxException e)
            {
                Console.WriteLine($"Синтаксическая ошибка. {e.Message}");
            }

            Console.ReadLine();
        }

        private static void TaCodeTest()
        {
            var taCode = new TACode();

            var ass1 = new Assign
            {
                Left = new IntConst(3),
                //Left = new IntConst(5),
                Operation = OpCode.Minus,
                Right = new IntConst(5),
                Result = new Var()
            };

            var ass2 = new Assign
            {
                Left = new IntConst(10),
                Operation = OpCode.Plus,
                Right = new IntConst(2),
                Result = new Var()
            };

            var ass3 = new Assign
            {
                Operation = OpCode.Minus,
                Right = new IntConst(1),
                Result = new Var()
            };

            var ifgt1 = new IfGoto
            {
                Condition = new IntConst(1),
                TargetLabel = ass3.Label
            };
            ass3.IsLabeled = true; //На этот оперетор мы переходим по условию

            var ass4 = new Assign
            {
                Left = ass3.Result,
                Operation = OpCode.Plus,
                Right = new IntConst(1999),
                Result = new Var()
            };

            var ifgt2 = new IfGoto
            {
                Condition = new IntConst(2),
                TargetLabel = ass2.Label
            };
            ass2.IsLabeled = true; //На этот оперетор мы переходим по условию

            var ass5 = new Assign
            {
                Left = new IntConst(7),
                Operation = OpCode.Mul,
                Right = new IntConst(4),
                Result = new Var()
            };

            var ass6 = new Assign
            {
                Left = new IntConst(100),
                Operation = OpCode.Div,
                Right = new IntConst(25),
                Result = new Var()
            };

            taCode.AddNode(ass1);
            taCode.AddNode(ass2);
            taCode.AddNode(ass3);
            taCode.AddNode(ifgt1);
            taCode.AddNode(ass4);
            taCode.AddNode(ifgt2);
            taCode.AddNode(ass5);
            taCode.AddNode(ass6);

            var algOpt = new AlgebraicOptimization();
            algOpt.Optimize(taCode.CodeList.ToList(), out var applied);

            Console.WriteLine($"TA Code\n: {taCode}");

            var bBlocks = taCode.CreateBasicBlockList();
            foreach (var bbl in bBlocks)
            {
                Console.WriteLine($"Block[{bbl.BlockId}]:");
                var bblCodeStr = bbl.CodeList.Aggregate("", (s, node) => s + node.ToString() + Environment.NewLine);
                Console.WriteLine($"{bblCodeStr}\n");
            }

            Console.WriteLine($"Algebraic Optimization was{(applied ? "" : "n't")} applied");

            Console.WriteLine("========================CFG test========================");
            var cfg = new ControlFlowGraph(taCode);
            foreach (var cfgn in cfg.CFGNodes)
            {
                Console.WriteLine($"Block[{cfgn.BlockId}]");
                var bblCodeStr = cfgn.CodeList.Aggregate("", (s, node) => s + node.ToString() + Environment.NewLine);
                Console.WriteLine($"{bblCodeStr}\n");

                Console.WriteLine("Children:\n");
                foreach (var ch in cfgn.Children)
                {
                    Console.WriteLine($"Block[{ch.BlockId}]");
                    var bblCodeCh = ch.CodeList.Aggregate("", (s, node) => s + node.ToString() + Environment.NewLine);
                    Console.WriteLine($"{bblCodeCh}\n");
                }

                Console.WriteLine("Parents:\n");
                foreach (var ch in cfgn.Parents)
                {
                    Console.WriteLine($"Block[{ch.BlockId}]");
                    var bblCodeCh = ch.CodeList.Aggregate("", (s, node) => s + node.ToString() + Environment.NewLine);
                    Console.WriteLine($"{bblCodeCh}\n");
                }
                Console.WriteLine("-----------------------------------------");
            }
			//Copy Propagation Test
			var taCodeCopyProp = new TACode();
			var assgn1 = new Assign()
			{
				Left = null,
				Operation = OpCode.Copy,
				Right = new IntConst(5),
				Result = new Var()
			};
			var assgn2 = new Assign()
			{
				Left = assgn1.Right,
				Operation = OpCode.Minus,
				Right = assgn1.Result,
				Result = new Var()
			};
			var assgn3 = new Assign()
			{
				Left = assgn2.Result,
				Operation = OpCode.Plus,
				Right = new IntConst(1),
				Result = new Var()
			};
			var assgn4 = new Assign()
			{
				Left = assgn3.Result,
				Operation = OpCode.Mul,
				Right = assgn1.Result,
				Result = new Var()
			};
			var assgn5 = new Assign()
			{
				Left = new IntConst(30),
				Operation = OpCode.Minus,
				Right = new IntConst(20),
				Result = assgn1.Result
			};
			var assgn6 = new Assign()
			{
				Left = assgn2.Result,
				Operation = OpCode.Plus,
				Right = assgn5.Result,
				Result = new Var()
			};

			taCodeCopyProp.AddNode(assgn1);
			taCodeCopyProp.AddNode(assgn2);
			taCodeCopyProp.AddNode(assgn3);
			taCodeCopyProp.AddNode(assgn4);
			taCodeCopyProp.AddNode(assgn5);
			taCodeCopyProp.AddNode(assgn6);

			/*
			  a = b
			  c = b - a     -----> c = b - b
			  d = c + 1
			  e = d * a     -----> e = d * b
			  a = 30 - 20
			  k = c + a     -----> k = c + a
			*/

			Console.WriteLine($"Testing Copy Propagation Optimisation.\n Three Adress Code:\n {taCodeCopyProp}");
			var optimization = new CopyPropagation();
			optimization.Optimize(taCodeCopyProp.CodeList.ToList(), out var appl);
			Console.WriteLine($"Optimisation Copy Propagation applied:\n");
			Console.WriteLine($"TA Code\n: {taCodeCopyProp}");
			Console.WriteLine("-----------------------------------------");
		}
    }
}