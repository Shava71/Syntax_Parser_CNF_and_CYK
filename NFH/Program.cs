using System;
using System.Collections.Generic;
using System.Text;



class Production
{
    public string Left, Right;
    public Production(string Left, string Right) 
    {
        this.Left = Left;
        this.Right = Right;
    }

    public Production(Production production)
    {
        Left = production.Left;
        Right = production.Right;
    }

    public override string ToString()
    {
        return Left + " ::= " + Right;
    }
    
    public override bool Equals(object obj)
    {
        if (obj is Production other)
        {
            return Left == other.Left && Right == other.Right;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Left.GetHashCode() + Right.GetHashCode();
    }
    
}

class Grammatic
{
    public HashSet<string> VN, VT;
    public string S;
    public HashSet<Production> P;
    // public Grammatic(HashSet<string> vN, HashSet<string> vT, string s, HashSet<Production> p)
    // {
    //     VN=vN;
    //     VT=vT;
    //     S=s;
    //     P=p;
    // }
    public Grammatic(HashSet<string> vN, HashSet<string> vT, string s, HashSet<Production> p)
    {
        VN = new HashSet<string>(vN); 
        VT = new HashSet<string>(vT); 
        S = s; 
        P = new HashSet<Production>(p);
    }
    public override string ToString()
    {
        string VNstr = string.Join(", ", VN);
        string VTstr = string.Join(", ", VT);
        string Pstr = "P:\n";
        foreach (Production p in P)
        {
            Pstr += p.ToString() + "\n";
        }
        string Str = "G:\nVN: {" + VNstr + "},\n" +
            "VT: {" + VTstr + "},\n" +
            "Z = " + S + ",\n" +
            Pstr + "\n";
        return Str;
    }
    public void CNF()
    {
        TerminalToNotTerminal();
        RemovingLongRules();
        RemovingEpsilonRules();
        RemovingChainRules();
    }
    public void TerminalToNotTerminal()
    {
        int i = 1;
        foreach (string Term in VT)
        {
            bool flagT = true;
            foreach (Production p in P)
            {
                if (p.Right ==  Term)
                    flagT = false;
            }
            if (flagT)
            {
                string T = $"T{i++}";
                P.Add(new Production(T, Term));
                VN.Add(T);
            }

        }
        foreach (string Term in VT)
        {
            string Ti = "";
            foreach (Production p in P)
            {
                bool flag = false;
                if (p.Right == Term)
                {
                    Ti = p.Left;
                    flag = true;
                }

                if (flag)
                {
                    foreach (Production p1 in P)
                    {
                        if (p1.Right.Length > 1)
                        {
                            string[] Rights = p1.Right.Split(new char[] { ' ' });
                            for (int j = 0; j < Rights.Length; j++)
                            {
                                if (Rights[j] == Term)
                                    Rights[j] = Ti;
                            }
                            p1.Right = string.Join(" ", Rights);
                        }
                    }
                }
            }
        }
        //return this;
    }
    
    int i_LongRules = 1;
    public void RemovingLongRules()
    {
        
        string NewRightGram = "";
        HashSet<Production> PNewRules = new HashSet<Production>();
        foreach (Production p in P)
        {
            string[] Rights = p.Right.Split(new char[] { ' ' });
            if (Rights.Length > 2)
            {
                NewRightGram = $"{Rights[0]} N{i_LongRules}";
                p.Right = NewRightGram;
                NewRightGram = string.Join(" ", Rights);
                NewRightGram = NewRightGram.Substring(2).TrimStart();
                PNewRules.Add(new Production($"N{i_LongRules}", NewRightGram));
                VN.Add($"N{i_LongRules++}");
            }
        }
        foreach (Production p in PNewRules)
            P.Add(p);
        
        // foreach (Production p in P)
        // {
        //     string[] Rights = p.Right.Split(new char[] { ' ' });
        //     if (Rights.Length > 2)
        //     {
        //         RemovingLongRules();
        //     }
        // }
        if (P.Any(p => p.Right.Split(new char[] { ' ' }).Length > 2))
        {
            RemovingLongRules();
        }
    }
    public void RemovingChainRules()
    {
        HashSet<Production> ChainPair = new HashSet<Production>();
        foreach (string NonTerminal in VN)
        {
            ChainPair.Add(new Production(NonTerminal, NonTerminal));
        }
        
        foreach (Production p in P)
        {
            if (p.Right.Split(new char[] { ' ' }).Length == 1 && VN.Contains(p.Right))
            {
                HashSet<Production> newPair = new HashSet<Production>();
                foreach (Production pair in ChainPair)
                {
                    if(pair.Right == p.Left)
                    {
                        newPair.Add(new Production (pair.Left, p.Right));                        
                    }
                }
                foreach (Production p1 in newPair)
                {
                    ChainPair.Add(p1);
                }

            }
        }
        foreach (Production pair in ChainPair)
        {
            if(pair.Left==pair.Right)
                ChainPair.Remove(pair);
        }
        foreach (Production p in ChainPair)
            Console.WriteLine(p);
        Console.WriteLine();
        
        
        HashSet<Production> newP = new HashSet<Production>();
        foreach (Production pair in ChainPair)
        {
            foreach (Production p in P)
            {
                if (VN.Contains(p.Right))
                {
                    foreach (Production p1 in P)
                        if (pair.Right == p1.Left && pair.Left == p.Left)
                        {
                            newP.Add(new Production(p.Left, p1.Right));
                        }
                }
            }
        }
        foreach (Production p in newP)
            P.Add(p);
        //foreach (Production p2 in Chain)
        //    P.Remove(p2);
        Console.WriteLine();
        //foreach (Production p in P)
        //    Console.WriteLine(p);
        HashSet<Production> ToRemove = new HashSet<Production>();
        foreach (Production p in P)
        {
            if (VN.Contains(p.Right))
                ToRemove.Add(p);
        }
        foreach (Production p2 in ToRemove)
            P.Remove(p2);
        //foreach (Production p in P)
        //    Console.WriteLine(p);
    }
        
        // public void RemovingChainRules()
        // {
        //     // Шаг 1: Найти все цепные пары (A -> B, где оба — нетерминалы)
        //     var chainPairs = new HashSet<Production>();
        //
        //     // Инициализируем пары A -> A для всех нетерминалов
        //     foreach (string nonTerm in VN)
        //     {
        //         chainPairs.Add(new Production(nonTerm, nonTerm));
        //     }
        //
        //     // Добавляем все цепные правила (A -> B)
        //     foreach (Production rule in P)
        //     {
        //         string[] rightSide = rule.Right.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        //         if (rightSide.Length == 1 && VN.Contains(rightSide[0]))
        //         {
        //             chainPairs.Add(new Production(rule.Left, rightSide[0]));
        //         }
        //     }
        //     
        //     // Console.WriteLine("================");
        //     // Console.WriteLine("ChainPair:");
        //     // foreach (Production p in chainPairs)
        //     //     Console.WriteLine(p);
        //     // Console.WriteLine("================");
        //
        // // Замыкание цепных пар
        // bool changed = true;
        // while (changed)
        // {
        //     changed = false;
        //     var newPairs = new HashSet<Production>();
        //
        //     foreach (var pair in chainPairs)
        //     {
        //         foreach (var otherPair in chainPairs)
        //         {
        //             //var newPair = new Production(pair.Left, otherPair.Right);
        //             if (pair.Right == otherPair.Left && !chainPairs.Contains(new Production(pair.Left, otherPair.Right)))
        //             {
        //                 newPairs.Add(new Production(pair.Left, otherPair.Right));
        //                 changed = true;
        //             }
        //         }
        //     }
        //
        //     foreach (var newPair in newPairs)
        //     {
        //         chainPairs.Add(newPair);
        //     }
        // }
        //     
        //     Console.WriteLine("================");
        //     Console.WriteLine("ChainPair:");
        //     foreach (Production p in chainPairs)
        //         Console.WriteLine(p);
        //     Console.WriteLine("================");
        //
        //     // Шаг 2: Генерация новых правил на основе цепных пар
        //     var newProductions = new HashSet<Production>();
        //     foreach (var pair in chainPairs)
        //     {
        //         foreach (Production p in P)
        //         {
        //             string[] Rigths = p.Right.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        //
        //             // Учитываем только те правила, которые не являются цепными
        //             if (p.Left == pair.Right && (Rigths.Length != 1 || !VN.Contains(Rigths[0])))
        //             {
        //                 newProductions.Add(new Production(pair.Left, p.Right));
        //             }
        //         }
        //     }
        //     Console.WriteLine("================");
        //     Console.WriteLine("newProductions:");
        //     foreach (Production p in newProductions)
        //         Console.WriteLine(p);
        //     Console.WriteLine("================");
        //     
        //     // Шаг 3: Удаление цепных правил
        //     var toRemove = new HashSet<Production>();
        //     foreach (Production rule in P)
        //     {
        //         string[] rightSide = rule.Right.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        //         if (rightSide.Length == 1 && VN.Contains(rightSide[0]))
        //         {
        //             toRemove.Add(rule);
        //         }
        //     }
        //
        //     foreach (Production rule in toRemove)
        //     {
        //         P.Remove(rule);
        //     }
        //
        //     // Добавляем новые правила
        //     foreach (Production newRule in newProductions)
        //     {
        //         P.Add(newRule);
        //     }
        //     
        // }
    public void RemovingEpsilonRules()
{
    // Шаг 1: Найти ε-порождающие нетерминалы
    HashSet<string> epsilonGenerating = new HashSet<string>();
    bool changed = true;

    while (changed)
    {
        changed = false;
        foreach (var rule in P)
        {
            // Если правая часть — "ε", добавляем левую часть в ε-порождающие
            if (rule.Right.Trim() == "ε" && !epsilonGenerating.Contains(rule.Left))
            {
                epsilonGenerating.Add(rule.Left);
                changed = true;
            }
            // Если все символы правой части порождают ε
            else if (!epsilonGenerating.Contains(rule.Left))
            {
                var symbols = rule.Right.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (symbols.All(epsilonGenerating.Contains))
                {
                    epsilonGenerating.Add(rule.Left);
                    changed = true;
                }
            }
        }
    }

    // Шаг 2: Удалить ε-правила (кроме стартового символа)
    P.RemoveWhere(p => p.Right.Trim() == "ε" && p.Left != S);

    // Шаг 3: Генерация новых правил с учетом ε-порождающих символов
    HashSet<Production> newProductions = new HashSet<Production>();
    foreach (var rule in P)
    {
        var symbols = rule.Right.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        GenerateRules(rule.Left, symbols, 0, new List<string>(), epsilonGenerating, newProductions);
    }

    // Добавляем новые правила в множество P
    foreach (var newProduction in newProductions)
    {
        P.Add(newProduction);
    }

    // Шаг 4: Если стартовый символ порождает ε, добавляем новое стартовое правило
    if (epsilonGenerating.Contains(S))
    {
        //P.RemoveWhere()
        string newStart = S + "'";
        VN.Add(newStart);
        P.Add(new Production(newStart, S));
        P.Add(new Production(newStart, "ε"));
        S = newStart;
    }
}

// Вспомогательный метод для генерации комбинаций
    private void GenerateRules(
        string left,
        string[] symbols,
        int index,
        List<string> current,
        HashSet<string> epsilonGenerating,
        HashSet<Production> newProductions)
    {
        // Если дошли до конца, формируем правило
        if (index == symbols.Length)
        {
            if (current.Count > 0)
            {
                newProductions.Add(new Production(left, string.Join(" ", current)));
            }
            return;
        }

        // Вариант 1: Добавляем текущий символ
        current.Add(symbols[index]);
        GenerateRules(left, symbols, index + 1, current, epsilonGenerating, newProductions);
        current.RemoveAt(current.Count - 1);

        // Вариант 2: Пропускаем текущий символ, если он ε-порождающий
        if (epsilonGenerating.Contains(symbols[index]))
        {
            GenerateRules(left, symbols, index + 1, current, epsilonGenerating, newProductions);
        }
    }

    private void Print(HashSet<string>[,] Table)
    {
        const int cellWidth = 15; // Фиксированная ширина для каждой ячейки

        for (int i = 0; i < Table.GetLength(0); i++)
        {
            Console.Write($"[{i+1}]: ");
            for (int j = 0; j < Table.GetLength(1); j++)
            {
                string output = "";
            
                if (Table[i, j] != null && Table[i, j].Count > 0)
                {
                    output = string.Join(",", Table[i, j]);
                }
                else
                {
                    output = " "; // Если ячейка пуста, оставляем её пустой
                }

                // Применяем выравнивание
                Console.Write(output.PadRight(cellWidth));
            }
            Console.WriteLine();
        }
    }
    public void CYK(string input)
    {
        HashSet<string>[,] cykTable = new HashSet<string>[input.Length, input.Length];
        char[] symbols = input.ToCharArray();

        // Инициализация таблицы
        for (int i = 0; i < input.Length; i++)
        {
            for (int j = 0; j < input.Length; j++)
            {
                cykTable[i, j] = new HashSet<string>();
            }
        }
        
        // Заполняем первый уровень таблицы
        for (int j = 0; j < input.Length; j++)
        {
            
            foreach (Production p in P)
            {
                if (p.Right == symbols[j].ToString())
                {
                    cykTable[0, j].Add(p.Left);
                }
            }
        }

        // Заполняем остальные уровни таблицы
        for (int i = 1; i < input.Length; i++) // Длина строки
        {
            for (int j = 0; j < input.Length - i; j++) // Стартовая позиция
            {
                for (int k = 0; k < i; k++) // Точка разбиения
                {
                    foreach (string B in cykTable[k, j])
                    {
                        foreach (string C in cykTable[i - k - 1, j + k + 1])
                        {
                            foreach (Production p in P)
                            {
                                string[] Rights = p.Right.Split(' ');
                                if (Rights.Length == 2 && Rights[0] == B && Rights[1] == C)
                                {
                                    cykTable[i, j].Add(p.Left);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Печать таблицы
        Print(cykTable);

        // Проверяем, содержится ли стартовый символ в последней ячейке
        if (cykTable[input.Length - 1, 0].Contains(S))
        {
            Console.WriteLine($"Строка '{input}' принадлежит языку.");
            // TreeNode tree = ReconstructTree(cykTable, input, input.Length - 1, 0, S);
            //List<string> linearReconstruction = ReconstructLinear(cykTable, input.Length - 1, 0, "","",S);
            var linearReconstruction = ReconstructLinear(cykTable, input.Length - 1, 0,S);
            // Console.WriteLine("Дерево:");
            // Console.WriteLine(PrintTreeLinear(tree));
            Console.WriteLine("Реконструкция линейно:");
            
            foreach (string line in linearReconstruction)
            {
                Console.WriteLine(line);
            }

        }
        else
        {
            Console.WriteLine($"Строка '{input}' не принадлежит языку.");
        }
    }
    
    StringBuilder reconstruction = new StringBuilder();
    
    private HashSet<string> ReconstructLinear(HashSet<string>[,] cykTable, int i, int j, string current = "", string left = "", string right = "")
    {
        HashSet<string> reconstructionLines = new HashSet<string>();

        // Базовый случай: достигли атомарного элемента
        if (i == 0)
        {
            reconstructionLines.Add(current);
            return reconstructionLines;
        }

        // Перебираем возможные разбиения
        for (int k = 0; k < i; k++)
        {
            foreach (string cyk1 in cykTable[k, j]) // Левый элемент
            {
                foreach (string cyk2 in cykTable[i - k - 1, j + k + 1]) // Правый элемент
                {
                    // Формируем строку для текущего шага
                    string step = $"{current}=>{left}{cyk1}{cyk2}{right}";

                    // Рекурсивная обработка правого элемента
                    var rightResults = ReconstructLinear(cykTable, i - k - 1, j + k + 1, step, left + cyk1, right);
                    reconstructionLines.UnionWith(rightResults);

                    // Рекурсивная обработка левого элемента
                    var leftResults = ReconstructLinear(cykTable, k, j, step, left, right + cyk2);
                    reconstructionLines.UnionWith(leftResults);
                }
            }
        }

        return reconstructionLines;
    }
    private string ReconstructLinear1(HashSet<string>[,] table, string input, int i, int j, string current)
    {
       

        // Рекурсивная функция для реконструкции
        void Reconstruct(int i, int j, string current)
        {
            reconstruction.Append(current);

            foreach (var production in P)
            {
                string[] rights = production.Right.Split(' ');

                if (production.Left == current && rights.Length == 2) // Двухчленные правила
                {
                    for (int k = 0; k < i; k++) // Перебираем точку разбиения
                    {
                        if (table[k, j].Contains(rights[0]) && table[i - k - 1, j + k + 1].Contains(rights[1]))
                        {
                            reconstruction.Append(" => ");
                            reconstruction.Append(rights[0]).Append(rights[1]); // Линейное представление
                            Reconstruct(k, j, rights[0]);
                            Reconstruct(i - k - 1, j + k + 1, rights[1]);
                            return;
                        }
                    }
                }
                // else if (production.Left == current && rights.Length == 1 && j < input.Length && rights[0] == input[j].ToString()) // Терминальные символы
                // {
                //     reconstruction.Append(" => ").Append(rights[0]);
                //     return;
                // }
            }
        }

        Reconstruct(i, j, current);
        return reconstruction.ToString();
    }
    
    
    class TreeNode
    {
        public string Value;
        public List<TreeNode> Children;

        public TreeNode(string value)
        {
            Value = value;
            Children = new List<TreeNode>();
        }
    }

// Построение дерева преобразований
    private TreeNode ReconstructTree(HashSet<string>[,] table, string input, int i, int j, string current)
    {
        TreeNode node = new TreeNode(current);

        foreach (Production p in P)
        {
            string[] Rights = p.Right.Split(' ');

            if (p.Left == current && Rights.Length == 2)
            {
                for (int k = 0; k < i; k++)
                {
                    if (table[k, j].Contains(Rights[0]) && table[i - k - 1, j + k + 1].Contains(Rights[1]))
                    {
                        TreeNode left = ReconstructTree(table, input, k, j, Rights[0]);
                        TreeNode right = ReconstructTree(table, input, i - k - 1, j + k + 1, Rights[1]);

                        node.Children.Add(left);
                        node.Children.Add(right);
                        return node;
                    }
                }
            }
            else if (p.Left == current && Rights.Length == 1 && Rights[0] == input[j].ToString())
            {
                node.Children.Add(new TreeNode(Rights[0]));
                return node;
            }
            // else if (p.Left == current && Rights.Length == 1 && i==0)
            // {
            //     node.Children.Add(new TreeNode(Rights[0]));
            //     return node;
            // }
        }

        return node;
    }

// Печать дерева
    private void PrintTree(TreeNode node, string indent)
    {
        Console.WriteLine(indent + node.Value);
        foreach (TreeNode child in node.Children)
        {
            PrintTree(child, indent + "  ");
        }
    }

    private int i = 0;
    private string PrintTreeLinear(TreeNode node)
    {
        if (node == null)
            return "@";

        string left = node.Children.Count > 0 ? PrintTreeLinear(node.Children[0]) : "@";
        string right = node.Children.Count > 1 ? PrintTreeLinear(node.Children[1]) : "@";
        return $" ({i++})[{left},{node.Value},{right}] ";
    }
    
}

class Program
{
    static void Main(string[] args)
    {
        // HashSet<string> VN0 = new HashSet<string> { "Z", "E", "T", "F" };
        // HashSet<string> VT0 = new HashSet<string> { "#", "+", "*", "(", ")", "i" };
        // HashSet<Production> P0 = new HashSet<Production>
        // {
        //     new Production("Z", "E #"),
        //     new Production("E", "E + T"),
        //     new Production("E", "T"),
        //     new Production("T", "F"),
        //     new Production("T", "T * F"),
        //     new Production("F", "i"),
        //     new Production("F", "( E )")
        // };
        // Grammatic grammatic0 = new Grammatic(VN0, VT0, "Z", P0);
        // Console.WriteLine("Исходная грамматика: \n"+grammatic0);
        // grammatic0.TerminalToNotTerminal();
        // Console.WriteLine("Перевод терминалов в нетерминалы: \n"+grammatic0);
        // grammatic0.RemovingLongRules();
        // Console.WriteLine("Удаление длинных правил: \n"+grammatic0);
        // grammatic0.RemovingChainRules();
        // Console.WriteLine("Удаление цепных правил: \n"+grammatic0);
        
        // HashSet<string> VN0 = new HashSet<string> { "Z", "E", "T", "F" };
        // HashSet<string> VT0 = new HashSet<string> { "#", "+", "*", "(", ")", "a", "b","c" };
        // HashSet<Production> P0 = new HashSet<Production>
        // {
        //     new Production("Z", "E #"),
        //     new Production("E", "E + T"),
        //     new Production("E", "T"),
        //     new Production("T", "F"),
        //     new Production("T", "T * F"),
        //     new Production("F", "a"),
        //     new Production("F", "b"),
        //     new Production("F", "c"),
        //     new Production("F", "( E )")
        // };
        // Grammatic grammatic0 = new Grammatic(VN0, VT0, "Z", P0);
        // Console.WriteLine("Исходная грамматика: \n"+grammatic0);
        // grammatic0.TerminalToNotTerminal();
        // Console.WriteLine("Перевод терминалов в нетерминалы: \n"+grammatic0);
        // grammatic0.RemovingLongRules();
        // Console.WriteLine("Удаление длинных правил: \n"+grammatic0);
        // grammatic0.RemovingChainRules();
        // Console.WriteLine("Удаление цепных правил: \n"+grammatic0);
        //
        // string s0 = "(a+b)#";
        // grammatic0.CYK(s0);
        
        //
        //
        //
        // HashSet<string> VN0 = new HashSet<string> { "Z", "E", "T", "F" };
        // HashSet<string> VT0 = new HashSet<string> { "#", "|", "(", ")", "a", "b", "!" };
        // HashSet<Production> P0 = new HashSet<Production>
        // {
        //     new Production("Z", "E #"),
        //     new Production("E", "E | T"),
        //     new Production("E", "T"),
        //     new Production("T", "! F"),
        //     new Production("F", "a"),
        //     new Production("F", "b"),
        //     new Production("F", "( E )")
        // };
        // Grammatic grammatic0 = new Grammatic(VN0, VT0, "Z", P0);
        // Console.WriteLine("Исходная грамматика: \n"+grammatic0);
        // grammatic0.TerminalToNotTerminal();
        // Console.WriteLine("Перевод терминалов в нетерминалы: \n"+grammatic0);
        // grammatic0.RemovingLongRules();
        // Console.WriteLine("Удаление длинных правил: \n"+grammatic0);
        // grammatic0.RemovingChainRules();
        // Console.WriteLine("Удаление цепных правил: \n"+grammatic0);
        //
        // string s = "!a|!b#";
        // grammatic0.CYK(s);
        
        
        // Console.WriteLine("------------------------------------------------");
        // HashSet<string> VN2 = new HashSet<string> { "S","Y","Z","X" };
        // HashSet<string> VT2 = new HashSet<string> { "a","b", "c",};
        // HashSet<Production> P2 = new HashSet<Production>
        // {
        //     new Production("S", "a X b X"),
        //     new Production("S", "a Z"),
        //     new Production("X", "a Y"),
        //     new Production("X", "b Y"),
        //     new Production("X", "ε "),
        //     new Production("Y", "X"),
        //     new Production("Y", "c c"),
        //     new Production("Z", "Z X"),
        // };
        // Grammatic grammatic2 = new Grammatic(VN2, VT2, "S", P2);
        // Console.WriteLine("Исходная грамматика: \n"+grammatic2);
        // grammatic2.RemovingLongRules();
        // Console.WriteLine("Удаление длинных правил: \n"+grammatic2);
        // grammatic2.RemovingEpsilonRules();
        // Console.WriteLine("Удаление epsilon-правил: \n"+grammatic2);
        
        HashSet<string> VN = new HashSet<string>() {"S","A","B"};
        HashSet<string> VT = new HashSet<string>() {"a","b","c"};
        HashSet<Production> P = new HashSet<Production>()
        {
            new Production("S", "A S"),
            new Production("S", "b"),
            new Production("A", "A B"),
            new Production("B", "a"),
            new Production("B", "c"),
            new Production("A", "c"),
        };
        Grammatic grammatic = new Grammatic(VN, VT, "S", P);
        string s = "caab";
        grammatic.CYK(s);
        
        HashSet<string> VN1 = new HashSet<string> { "S", };
        HashSet<string> VT1 = new HashSet<string> { "(", ")" };
        HashSet<Production> P1 = new HashSet<Production>
        {
            new Production("S", "( S ) S"),
            // new Production("S", ""),
            new Production("S", "ε"),
        };
        Grammatic grammatic1 = new Grammatic(VN1, VT1, "S", P1);
        Console.WriteLine("Исходная грамматика: \n"+grammatic1);
        grammatic1.TerminalToNotTerminal();
        Console.WriteLine("Перевод терминалов в нетерминалы: \n"+grammatic1);
        grammatic1.RemovingLongRules();
        Console.WriteLine("Удаление длинных правил: \n"+grammatic1);
        grammatic1.RemovingEpsilonRules();
        Console.WriteLine("Удаление эпсилон-правил: \n"+grammatic1);
        grammatic1.RemovingChainRules();
        Console.WriteLine("Удаление цепных правил: \n"+grammatic1);

        string s1 = "(())";
        grammatic1.CYK(s1);

    }
}