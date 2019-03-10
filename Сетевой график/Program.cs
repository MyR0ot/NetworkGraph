using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Сетевой_график
{
	class Program
	{
		public static int N = 7;
		public static bool test = true; // при флаге true - загрузка теста из методички, при false - свой вариант

		static void Main(string[] args)
		{
			Work[][] matrix = GenerateMatrix(N);
			List<List<int>> ways = GetWays(matrix, 0, N);				

			double[] t = new double[ways.Count];     // суммарные времена для всех возможных путей
			double Tmax;                             // длительность критического пути
			double Tmin;                             // длительность минимального пути
			double[] Tp = new double[N + 1];         // ранний срок наступления события i
			double[] Tn = new double[N + 1];         // поздний срок наступления события i
			double[] P = new double[N + 1];          // резерв времени события i


			for (int i = 0; i < ways.Count; i++)
				t[i] = GetTime(matrix, ways[i]);
			Tmax = t.Max();
			Tmin = t.Min();
			UpdateWorks(matrix, Tp, Tn, P, Tmax); // обновление данных о работах

			PrintTable1(Tp, Tn, P);
			PrintTable2(matrix);
			PrintTable3(ways, t, Tmax, Tmin);

			
			#region Optmization
			int count = 0;
			double TmaxCur = Tmax;
			while (true)
			{
				int iFrom = 0;
				int jFrom = 0;
				int iTo = 0;
				int jTo = 0;
				Optimize(matrix, ways, Tp, Tn, P, t, out Tmax, out Tmin, ref iFrom, ref jFrom, ref iTo, ref jTo);
				count++;
				if (Tmax > TmaxCur)
				{
					matrix[iTo][jTo].RemovePeople(1);
					matrix[iFrom][jFrom].AddPeople(1);
					for (int i = 0; i < ways.Count; i++)
						t[i] = GetTime(matrix, ways[i]);

					Tmax = t.Max();
					Tmin = t.Min();

					UpdateWorks(matrix, Tp, Tn, P, Tmax);
					count--;
					break;
				}
				TmaxCur = Tmax;
			}
			#endregion 

			PrintTable1(Tp, Tn, P, "#4");
			PrintTable2(matrix, "#5");
			PrintTable3(ways, t, Tmax, Tmin, "#6");

			Console.WriteLine("\nКол-во перемещений: {0}", count);


			Console.ReadLine();
		}

		public static Work[][] GenerateMatrix(int n)
		{
			#region Sample Test
			if (test)
			{
				N = 5;
				Work[][] matrix = new Work[n + 1][];
				for (int i = 0; i <= n; i++)
					matrix[i] = new Work[n + 1];
				for (int i = 0; i < n + 1; i++)
					for (int j = 0; j < n + 1; j++)
						matrix[i][j] = new Work(false);

				matrix[0][1] = new Work(0, 1, 18, 9);
				matrix[0][2] = new Work(0, 2, 16, 5);

				matrix[1][2] = new Work(1, 2, 30, 12);
				matrix[1][3] = new Work(1, 3, 4, 2);
				matrix[1][4] = new Work(1, 4, 18, 9);

				matrix[2][4] = new Work(2, 4, 75, 13);

				matrix[3][4] = new Work(3, 4, 4, 2);
				matrix[3][5] = new Work(3, 5, 90, 18);

				matrix[4][5] = new Work(4, 5, 4, 1);

				return matrix;
			}
			#endregion
			else
			{
				Work[][] matrix = new Work[n + 1][];
				for (int i = 0; i <= n; i++)
					matrix[i] = new Work[n + 1];
				for (int i = 0; i < n + 1; i++)
					for (int j = 0; j < n + 1; j++)
						matrix[i][j] = new Work(false);

				matrix[0][1] = new Work(0, 1, 200, 10);
				matrix[0][2] = new Work(0, 2, 145, 11);

				matrix[1][2] = new Work(1, 2, 18, 100);
				matrix[1][3] = new Work(1, 3, 58, 13);

				matrix[2][3] = new Work(2, 3, 39, 7);

				matrix[3][4] = new Work(3, 4, 9, 45);
				matrix[3][7] = new Work(3, 7, 69, 18);


				matrix[4][5] = new Work(4, 5, 2, 15);
				matrix[4][6] = new Work(4, 6, 75, 20);

				matrix[5][6] = new Work(5, 6, 12, 1);

				matrix[6][7] = new Work(6, 7, 2, 17);

				return matrix;
			}
			
		}


		/// <summary>
		/// Получение всех путей
		/// </summary>
		/// <param name="matrix"></param>
		/// <param name="start"></param>
		/// <param name="finish"></param>
		/// <returns></returns>
		public static List<List<int>> GetWays(Work[][] matrix, int start, int finish) // FIX
		{
			List<List<int>> res = new List<List<int>>();
			if (start == finish)
			{
				res.Add(new List<int>());
				res[0].Add(finish);
				return res;
			}

			for (int i = 0; i <= N; i++)
				if (matrix[start][i].IsExist)
				{
					List<List<int>> tmpLs = GetWays(matrix, i, finish);
					foreach (var ls in tmpLs)
					{
						ls.Insert(0, start);
						res.Add(ls);
					}
				}
			return res;
		}


		/// <summary>
		/// Получение суммарного времени пути
		/// </summary>
		/// <param name="gr"></param>
		/// <param name="ways"></param>
		/// <returns></returns>
		public static double GetTime(Work[][] matrix, List<int> way)
		{
			double res = 0;
			for (int j = 0; j < way.Count - 1; j++)
				res += matrix[way[j]][way[j + 1]].T;

			return res;
		}

		public static void UpdateWorks(Work[][] matrix, double[] Tp, double[] Tn, double[] P, double Tmax)
		{
			for (int i = 0; i <= N; i++)
			{
				List<List<int>> waysTmp = GetWays(matrix, 0, i);
				double[] tmpT1 = new double[waysTmp.Count]; // суммарные времена для всех возможных путей
				for (int j = 0; j < waysTmp.Count; j++)
					tmpT1[j] = GetTime(matrix, waysTmp[j]);
				Tp[i] = tmpT1.Max();

				waysTmp = GetWays(matrix, i, N);
				double[] tmpT2 = new double[waysTmp.Count]; // суммарные времена для всех возможных путей
				for (int j = 0; j < waysTmp.Count; j++)
					tmpT2[j] = GetTime(matrix, waysTmp[j]);
				Tn[i] = Tmax - tmpT2.Max();
				P[i] = Tn[i] - Tp[i];
			}

			UpdateGraph(matrix, Tp, Tn, P);
		}

		/// <summary>
		/// Обновление значений для работ
		/// </summary>
		/// <param name="gr"></param>
		/// <param name="Tp"></param>
		/// <param name="Tn"></param>
		/// <param name="P"></param>
		/// <param name="Tmax"></param>
		/// <param name="Tmin"></param>
		public static void UpdateGraph(Work[][] matrix, double[] Tp, double[] Tn, double[] P)
		{
			for (int i = 0; i <= N; i++)
				for (int j = 0; j <= N; j++)
				{
					if (matrix[i][j].IsExist)
					{
						matrix[i][j].Tp = Tp[i];
						matrix[i][j].Tn = Tn[j] - matrix[i][j].T;
						matrix[i][j].Tpo = Tp[i] + matrix[i][j].T;
						matrix[i][j].Tno = Tn[j];
						matrix[i][j].Pn = Tn[j] - Tp[i] - matrix[i][j].T;
						matrix[i][j].Pc = Tp[j] - Tn[i] - matrix[i][j].T;
					}
				}
		}


		/// <summary>
		/// Перестановку будем совершать с работы с наибольшим свободным резервом времени на работу критического пути с наибольшей длительностью.
		/// </summary>
		/// <param name="matrix">Исходный граф</param>
		/// <param name="ways">Список всех путей</param>
		/// <param name="Tp"></param>
		/// <param name="Tn"></param>
		/// <param name="P"></param>
		/// <param name="t"></param>
		/// <param name="Tmax"></param>
		/// <param name="Tmin"></param>
		/// <param name="indexTmax">Индекс </param>
		/// <param name="ifrom"></param>
		/// <param name="jfrom"></param>
		/// <param name="iTo"></param>
		/// <param name="jTo"></param>
		public static void Optimize(Work[][] matrix, List<List<int>> ways, double[] Tp, double[] Tn, double[] P, double[] t, out double Tmax, out double Tmin, ref int iFrom, ref int jFrom, ref int iTo, ref int jTo)
		{
			Tmax = t.Max();
			int indexTmax = Array.IndexOf(t, Tmax);


			GetIndexFrom(matrix, ref iFrom, ref jFrom);
			GetIndexTo(matrix, ways[indexTmax], ref iTo, ref jTo);

			matrix[iFrom][jFrom].RemovePeople(1);
			matrix[iTo][jTo].AddPeople(1);
			for (int i = 0; i < ways.Count; i++)
				t[i] = GetTime(matrix, ways[i]);

			Tmax = t.Max();
			Tmin = t.Min();

			UpdateWorks(matrix, Tp, Tn, P, Tmax);
		}


		public static void PrintTable1(double[] Tp, double[] Tn, double[] P, string msg = "#1")
		{
			Console.WriteLine("Table {0}", msg);
			Console.WriteLine();
			Console.WriteLine("i	Tp(i)	Tn(i)	P(i)");
			for (int i = 0; i <= N; i++)
				Console.WriteLine("{0}	{1}	{2}	{3}", i, Math.Round(Tp[i], 2), Math.Round(Tn[i], 2), Math.Round(P[i], 2));
			Console.WriteLine();

		}

		public static void PrintTable2(Work[][] matrix, string msg = "#2")
		{
			Console.WriteLine("Table {0}", msg);
			Console.WriteLine();

			Console.WriteLine("Работа	d	r	t	Pn	Pc");

			for (int i = 0; i <= N; i++)
				for (int j = 0; j <= N; j++)
					if (matrix[i][j].IsExist)
						Console.WriteLine("({0}, {1})	{2}	{3}	{4}	{5}	{6}", i, j, matrix[i][j].d, matrix[i][j].r, Math.Round(matrix[i][j].T, 2), Math.Round(matrix[i][j].Pn, 2), Math.Round(matrix[i][j].Pc, 2));
			Console.WriteLine();
		}

		public static void PrintTable3(List<List<int>> ways, double[] t, double Tmax, double Tmin, string msg = "#3")
		{
			Console.WriteLine("Table {0}", msg);
			Console.WriteLine();
			Console.WriteLine("L(0, n)		T, ед");
			for (int i = 0; i < ways.Count; i++)
			{
				foreach (var mean in ways[i])
					Console.Write("{0} ", mean);
				Console.WriteLine("	{0}", Math.Round(t[i], 2));
			}
			Console.WriteLine("Tкр:	{0}", Math.Round(Tmax, 2));
			Console.WriteLine("Tmin:	{0}", Math.Round(Tmin, 2));
			Console.WriteLine("K:	{0}\n", Math.Round(Tmin / Tmax, 2));
		}


		/// <summary>
		/// Получение работы, с которой будем убирать персонал
		/// </summary>
		/// <param name="matrix"></param>
		/// <param name="I"></param>
		/// <param name="J"></param>
		/// <returns></returns>
		public static bool GetIndexFrom(Work[][] matrix, ref int I, ref int J)
		{
			double curPc = 0;
			for (int i = 0; i <= N; i++)
				for (int j = 0; j <= N; j++)
					if (matrix[i][j].IsExist)
					{
						if (matrix[i][j].Pc > curPc && matrix[i][j].r > 0)
						{
							I = i;
							J = j;
							curPc = matrix[i][j].Pc;
						}
					}
			if (curPc <= 0)
				return false;
			return true;
		}


		/// <summary>
		/// Получение работы, куда будем перемещать персонал
		/// </summary>
		/// <param name="matrix"></param>
		/// <param name="way"></param>
		/// <param name="I"></param>
		/// <param name="J"></param>
		public static void GetIndexTo(Work[][] matrix, List<int> way, ref int I, ref int J)
		{
			double res = matrix[way[0]][way[1]].T;
			I = way[0];
			J = way[1];
			for (int i = 1; i < way.Count - 1; i++)
			{
				if (matrix[way[i]][way[i + 1]].T > res)
				{
					res = matrix[way[i]][way[i + 1]].T;
					I = way[i];
					J = way[i + 1];
				}
			}
		}
	}
}
