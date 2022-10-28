using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Program
{
    static void Main(string[] args)
    {
        //Матрица 
        int[][] graph;
        //Список смежности
        List<int>[] adjList;
        ReadFile(out graph, out adjList);
        List<int> oddNodes = CheckGraph(ref adjList);
        int[,] oddGraph;
        int newEdgesDistSum = 0;
        if (oddNodes.Count > 0)
        {
            oddGraph = new int[oddNodes.Count, oddNodes.Count];
            int[] dists;
            //Составление матрицы полного графа из вершин нечётной степени
            for (int i = 0; i < oddNodes.Count; i++)
            {
                dists = Dijkstra(ref graph, oddNodes[i]);
                for (int y = 0; y < oddNodes.Count; y++)
                    oddGraph[i, y] = dists[oddNodes[y]];
            }
            List<int[]> newEdges;
            //Получаем ребра для доведения графа до Эйлерова
            CreateEdges(ref oddGraph, out newEdges, out newEdgesDistSum);
            //Добавление новых рёбер в список смежности
            foreach (var edge in newEdges)
            {
                adjList[edge[0]].Add(edge[1]);
                adjList[edge[1]].Add(edge[0]);
                Console.WriteLine($"{edge[0]} - {edge[1]}");
            }
        }
        //Вывход Эйлерова цикла
        foreach (var el in FindWay(ref adjList))
        {
            Console.WriteLine(el);
        }
        int waySum = 0;
        //Подсчет суммы рёбер изначального графа
        for (int i = 0; i < graph.GetLength(0); i++)
        {
            for (int y = i; y < graph.GetLength(0); y++)
            {
                if (graph[i][y] != int.MaxValue)
                    waySum += graph[i][y];
            }
        }
        //Вывод суммы рёбер Эйлерова графа
        Console.WriteLine(waySum + newEdgesDistSum);
    }

    /*
     * Чтение матрицы графа из файла; '-' - отсутствие ребра, '3' - вес ребра('1' для невзвешенного)
     * Пример:
     * - 1 - 3
     * 1 - 2 5
     * - 2 - -
     * 3 5 - -
     * Параметры: переменная для матрицы графа, переменная для невзвешенного списка смежности
     * Получим(inf == int.MaxValue):
     * Матрица:
     * inf 1 inf 3
     * 1 inf 2 5
     * inf 2 inf inf
     * 3 5 inf inf
     * 
     * Список:
     * [0] = {1,3}
     * [1] = {0,2,3}
     * [2] = {1}
     * [3] = {0,1}
     */
    static void ReadFile(out int[][] graph, out List<int>[] adjList)
    {
        string[] textFromFile; // Строки матрицы из файла
        using (StreamReader fstream = new StreamReader(@"D:\note2.txt"))
        {
            // Читаем весь файл, удаляем знак переноса строки '\r' и делим на строки по '\n'
            textFromFile = fstream.ReadToEnd().Replace("\r", "").Split('\n');
        }
        graph = new int[textFromFile.GetLength(0)][];
        adjList = new List<int>[textFromFile.GetLength(0)];
        string[] a; //Переменная для значений ячеек строки
        for (int i = 0; i < textFromFile.GetLength(0); i++) // Перебор по строкам файла
        {
            a = textFromFile[i].Split(' ');
            graph[i] = new int[textFromFile.GetLength(0)];
            adjList[i] = new List<int>();
            for (int y = 0; y < graph.GetLength(0); y++) // Перебор по ячейкам строки
            {
                //Если ребра нет, то задаём максимально доступное значение.
                if (a[y] == "-") 
                    graph[i][y] = int.MaxValue;
                else
                {
                    //Иначе преобразуем в число и добавляем в матрицу
                    //и добавляем вершину в список смежности
                    graph[i][y] = int.Parse(a[y]);
                    adjList[i].Add(y);
                }
            }
        }
    }

    /*
     * Проверка графа на эйлеровый граф
     * Если граф эйлеровый, возвращается пустой список
     * Иначе возвращается список вершин с нечётной степенью
     */
    static List<int> CheckGraph(ref List<int>[] adjList)
    {
        List<int> oddNodes = new List<int>();
        for (int i = 0; i < adjList.Length; i++)
        {
            if (adjList[i].Count % 2 == 1) // Если вершина нечетной степени
                oddNodes.Add(i);
        }
        return oddNodes;
    }

    /*
     * Алгоритм Дейкстры
     * На вход подаётся ссылка на матрицу графа и вершина с которой начинается отсчёт
     * Возвращает список наименьших расстояний от стартовой вершиный до всех остальных в графе
     * (Значение для стартовой вершины равно int.MaxValue)
     */
    static int[] Dijkstra(ref int[][] graph, int start)
    {
        //Массив расстояний до вершин(вес вершин)
        //Во время выполнения алгоритма вес стартовой вершины равен 0
        int[] dists = new int[graph.GetLength(0)];
        //Массив для отметки посещённых вершин
        bool[] marked = new bool[graph.GetLength(0)];
        //Инициализация массива растояний
        for (int i = 0; i < graph.GetLength(0); i++)
        {
            if (i == start) continue;
            dists[i] = int.MaxValue;
        }
        //Список вершин подлежащих осмотру
        List<int> nodes = new List<int>();
        nodes.Add(start);
        int minDist, minNode = -1;
        while (nodes.Count > 0)
        {
            minDist = int.MaxValue;
            //Выбор вершины с минимальным весом среди обрабатываемых в данный момент
            foreach (var n in nodes)
            {
                if (minDist > dists[n])
                {
                    minDist = dists[n];
                    minNode = n;
                }
            }
            nodes.Remove(minNode);
            marked[minNode] = true;
            //Обновление расстояний смежных вершин и добавления их в список обрабатываемых
            for (int i = 0; i < graph.GetLength(0); i++)
            {
                if (graph[minNode][i] == int.MaxValue || marked[i]) continue;
                if (dists[i] > graph[minNode][i] + dists[minNode])
                    dists[i] = graph[minNode][i] + dists[minNode];
                if (!nodes.Contains(i))
                    nodes.Add(i);
            }
        }
        dists[start] = int.MaxValue;
        return dists;
    }

    /*
     * Поиск минимального паросочетания для вершин нечётной степени по матрице перебором
     * На вход подаётся матрица полного графа из нечетных вершин
     * На выход передаётся переменная для спика минимальных паросочетаний и переменная суммы их длин
     */
    static void CreateEdges(ref int[,] oddGraph, out List<int[]> minChains, out int minSum)
    {
        //Массив для перечеркиваня строк и столбцов использованных вершин
        bool[] m = new bool[oddGraph.GetLength(0)];
        //Список текущих выбранных цепей(паросочетаний)
        List<int[]> chains = new List<int[]>();
        minChains = new List<int[]>();
        minSum = int.MaxValue;
        //Сумма длин текущих выбранных цепей
        int sum;
        //Цикл по первой строке задающий начальную цепь(паросочетание)
        for (int i = 1; i < oddGraph.GetLength(0); i++)
        {
            chains.Add(new int[2] { 0, i });
            //Зачёркиваем строки и столбцы выбранных вершин
            m[0] = true; m[i] = true;
            //Перебираем оставшиеся цепи
            for (int a = 1, b = 0; ; b++)
            {
                if (b == oddGraph.GetLength(0))
                {
                    a++;
                    if (a == oddGraph.GetLength(0)) break;
                    b = 0;
                }
                //Если столбец\строка зачеркнуты или цепь имеет начало и конец в одной вершине - пропускаем
                if (m[a] || m[b] || a == b) continue;
                chains.Add(new int[2] { a, b });
                m[a] = true; m[b] = true;
            }
            sum = 0;
            foreach (var ch in chains)
            {
                if (oddGraph[ch[0], ch[1]] == int.MaxValue)
                {
                    sum = int.MaxValue;
                    break;
                }
                sum += oddGraph[ch[0], ch[1]];
            }
            if (sum < minSum)
            {
                minSum = sum;
                minChains.Clear();
                minChains.AddRange(chains);
            }
            chains.Clear();
            for (int a = 0; a < m.GetLength(0); a++)
            {
                m[a] = false;
            }
        }
    }
    /*
     * Составление эйлерова цикла по списку смежности
     */
    static List<int> FindWay(ref List<int>[] graph)
    {
        List<int> way = new List<int>();
        List<int> s = new List<int>();
        s.Add(0); // Установка стартовой вершины
        while (s.Count != 0)
        {
            if (graph[s.Last()].Count == 0)
            {
                way.Add(s.Last());
                s.RemoveAt(s.Count - 1);
                continue;
            }
            int a = s.Last();
            int b = graph[s.Last()][0];
            graph[b].Remove(a);
            graph[a].Remove(b);
            s.Add(b);
        }
        return way;
    }
}