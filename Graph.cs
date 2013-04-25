using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;


namespace TheoryOfGraph
{
	public class Graph
	{
		#region Поля класу

		private bool Is_Cycle = false, ShortestConnetctionGraph = false, NegativeWeight = false;
		private int vershuna, rebro, beginDegree, endDegree;
		public int Vershuna { get { return vershuna; } private set { vershuna = value; } }
		public int Rebro { get { return rebro; } private set { rebro = value; } }
		public int BeginDegree
		{
			get { return beginDegree; }
			set {
				if ((value + 1 < 0) || (value + 1 > vershuna))
					throw new ArgumentException("Невірно задана вершина!");
				else
					beginDegree = value;
				}
		}
		public int EndDegree
		{
			get { return endDegree; }
			set
			{
				if ((value + 1 < 0) || (value + 1 > vershuna))
					throw new ArgumentException("Невірно задана вершина!");
				else
					endDegree = value;
			}
		}
		public int Versh_Tree { get; set; }
		private static int[,] Matrix, IncidentMatrix, AdjacencyMatrix, DistanceMatrix, ReachMatrix, ShortestDistanceMatrix;
		private static int[] DegreeVertices, DegreeInput, DegreeOutput;
		private delegate void TextBox_Manipulate();

		#endregion

		#region Системні функції
		//Створення матриць у класі.
		private void SetMatrix() 
		{	
			IncidentMatrix = new int[Vershuna, Rebro];
			AdjacencyMatrix = new int[Vershuna, Vershuna];
			DegreeVertices = new int[Vershuna];
			DegreeInput = new int[Vershuna];
			DegreeOutput = new int[Vershuna];
			DistanceMatrix = new int[Vershuna, Vershuna];
			ReachMatrix = new int[Vershuna, Vershuna];
			InitialiseMatrix();
		}

		//Ініціалізацію даних у всіх матрицях класу
		private void InitialiseMatrix()
		{
			//Incident Matrix
			if (Rebro != 0)
				for (int i = 0; i < Rebro; i++)
				{
					if (Matrix[i, 0] == Matrix[i, 1])
						IncidentMatrix[Matrix[i, 0] - 1, i] = 2;
					else
					{
						IncidentMatrix[Matrix[i, 0] - 1, i] = -1;
						IncidentMatrix[Matrix[i, 1] - 1, i] = 1;
					}
				}

			//Adjacency Matrix
			if (Rebro != 0)
				for (int i = 0; i < Rebro; i++)
					AdjacencyMatrix[Matrix[i, 0] - 1, Matrix[i, 1] - 1] += 1;


			//Matrix of DegreeInput and DegreeOutput
			for (int i = 0; i < Vershuna; i++)
				for (int j = 0; j < Rebro; j++)
				{
					if (IncidentMatrix[i, j] == -1)
						DegreeOutput[i] += 1;
					if (IncidentMatrix[i, j] == 1)
						DegreeInput[i] += 1;
					if (IncidentMatrix[i, j] == 2)
					{
						DegreeOutput[i] += 1;
						DegreeInput[i] += 1;
					}
				}

			//Matrix of DegreeVertices
			for (int i = 0; i < Vershuna; i++)
				DegreeVertices[i] += DegreeInput[i] + DegreeOutput[i];


			//Distance and Reach Matrix
			if (Rebro != 0)
			{
				Array.Copy(AdjacencyMatrix, DistanceMatrix, Vershuna * Vershuna);//Створення додаткових об'єктів
				int[,] temp = new int[Vershuna, Vershuna];
				Array.Copy(DistanceMatrix, temp, Vershuna * Vershuna);
				for (int i = 0; i < Vershuna; i++)//ініціалізація одиничної матриці
					ReachMatrix[i, i] = 1;
				for (int distance = 2; distance < Vershuna; distance++)
				{
					for (int k = 0; k < Vershuna; k++)//Піднесення матриці суміжності до степеня +1
						for (int i = 0; i < Vershuna; i++)
							for (int j = 0; j < Vershuna; j++)
								temp[k, i] += AdjacencyMatrix[k, j] * DistanceMatrix[j, i];
					for (int i = 0; i < Vershuna; i++)//Визначення матриць відстаней та досяжності
						for (int j = 0; j < Vershuna; j++)
						{
							ReachMatrix[i, j] += temp[i, j];//матриця досяжності
							if ((DistanceMatrix[i, j] == 0) && (temp[i, j] != 0))//матриця відстаней
								DistanceMatrix[i, j] = distance;
						}
				}
				for (int i = 0; i < Vershuna; i++)//Булеве перетворення матриці досяжності
					for (int j = 0; j < Vershuna; j++)
						if (ReachMatrix[i, j] > 0)
							ReachMatrix[i, j] = 1;
			}

			//Матриця суміжності з відстанями між вершинами.
            if (Rebro != 0)
                if (ShortestConnetctionGraph)
                {
                    ShortestDistanceMatrix = new int[Vershuna, Vershuna];
                    for (int i = 0; i < Rebro; i++)
                        ShortestDistanceMatrix[Matrix[i, 0] - 1, Matrix[i, 1] - 1] = Matrix[i, 2];
                }
			Check_Cycle();
            Check_NegativeWeight();
		}

		//Зчитування графу
		public void ReadGraph(OpenFileDialog dialog)
		{
			Stream f = dialog.OpenFile();
			StreamReader str = new StreamReader(f);
			string[] arr = str.ReadLine().Split(' ');
			Vershuna = Convert.ToInt32(arr[0]);
			Rebro = Convert.ToInt32(arr[1]);
			arr = str.ReadLine().Split(' ');
			this.ShortestConnetctionGraph = (arr.Length % 3 == 0);
			Matrix = new int[Rebro, arr.Length % 2 == 0 ? 2 : 3];
			for (int j = 0; j < (arr.Length % 2 == 0 ? 2 : 3); j++)
				Matrix[0, j] = Convert.ToInt32(arr[j]);
			for (int i = 1; i < Rebro; i++ )
			{
				arr = str.ReadLine().Split(' ');
				for (int j = 0; j < (arr.Length % 2 == 0 ? 2 : 3); j++)
					Matrix[i, j] = Convert.ToInt32(arr[j]);
			}
			str.Close();
			str.Dispose();
			f.Close();
			f.Dispose();
			SetMatrix();
		}

		//Запис матриць
		private void WriteMatrix(int[,] args, int FirstArg, int SecondArg, string path, string AddInfo)
		{
			FileStream fls = File.Open(path, FileMode.Create, FileAccess.Write);
			StreamWriter f = new StreamWriter(fls);
			f.WriteLine(AddInfo);
			f.Write("   |");													//Початок дизайну виводу
			for (int temp = 1; temp <= SecondArg; temp++)
				f.Write("{0,4:D}", temp);
			f.WriteLine();
			f.Write("---|");
			for (int temp = 1; temp <= SecondArg; temp++)
				f.Write("----");												//Кінець
			f.WriteLine();
			for (int i = 0, temp = 1; i < FirstArg; i++, temp++)
			{
				f.Write("{0,3:D}|", temp);
				for (int j = 0; j < SecondArg; j++)
					f.Write("{0,4:D}", args[i, j]);
				f.WriteLine();
			}
			f.Flush();
			f.Close();
			fls.Close();
		}
		private void WriteMatrix(int[,] args, int FirstArg, int SecondArg, TextBox textbox, string AddInfo)
		{
			if (SecondArg == 0)
				textbox.Text += AddInfo + "\r\nНе визначена\r\n\r\n";
			else
			{
				textbox.Text += String.Format("{0}\r\n   |", AddInfo);
				for (int temp = 1; temp <= SecondArg; temp++)
					textbox.Text += String.Format("{0,4:D}", temp);
				textbox.Text += "\r\n---|";
				for (int temp = 1; temp <= SecondArg; temp++)
					textbox.Text += "----";
				textbox.Text += "\r\n";
				for (int i = 0, temp = 1; i < FirstArg; i++, temp++)
				{
					textbox.Text += String.Format("{0,3:D}|", temp);
					for (int j = 0; j < SecondArg; j++)
						textbox.Text += String.Format("{0,4:D}", args[i, j]);
					textbox.Text += "\r\n";
				}
				textbox.Text += "\r\n\r\n";
			}
		}
		private void WriteMatrix(int[] args, int FirstArg, string path, string AddInfo)
		{
			FileStream fls = File.Open(path, FileMode.Create, FileAccess.Write);
			StreamWriter f = new StreamWriter(fls);
			f.WriteLine(AddInfo);
			f.WriteLine("---|----");
			for (int i = 0, temp = 1; i < FirstArg; i++, temp++)
				f.WriteLine("{0,3:D}|{1,2:D}", temp, args[i]);
			f.Flush();
			f.Close();
			fls.Close();
		}
		private void WriteMatrix(int[] args, int FirstArg, TextBox textbox, string AddInfo)
		{
			textbox.Text += String.Format("{0}\r\n", AddInfo);
			textbox.Text += "---|---\r\n";
			for (int i = 0; i < FirstArg; i++)
				textbox.Text += String.Format("{0,3:D}|{1,3:D}\r\n", i + 1, args[i]);
			textbox.Text += "\r\n\r\n";
		}

        private void Check_Cycle()
        {
            for (int i = 0; i < Vershuna; i++)
                if (DistanceMatrix[i, i] > 0)
                {
                    Is_Cycle = true;
                    break;
                }
        }
        private void Check_NegativeWeight()
        {
            if (ShortestConnetctionGraph)
                for (int i = 0; i < Rebro; i++)
                    if (Matrix[i, 2] < 0)
                        NegativeWeight = true;
        }

		private void Swap<T>(ref T first, ref T second)
		{
			T temp = first;
			first = second;
			second = temp;
		}
		private string FindShortestPath(int[] ArrayOfDegree, int BeginDegree, int EndDegree)
		{
			string path = (EndDegree + 1) + "]"; int asd = ArrayOfDegree[EndDegree];
			while (asd != BeginDegree)
			{
				path = path.Insert(0, String.Format("{0} => ", asd + 1));
				asd = ArrayOfDegree[asd];
			}
			path = path.Insert(0, String.Format("[{0} => ", BeginDegree + 1));
			return path;
		}

		private void Initialize(TextBox textBox1,TextBox_Manipulate Function)
		{
			var form = new Form() { Size = new System.Drawing.Size(1000, 700) };
			// 
			// textBox1
			// 
			textBox1.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
			textBox1.MaxLength = 0;
			textBox1.Multiline = true;
			textBox1.ReadOnly = true;
			textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			textBox1.Size = form.Size;
			textBox1.WordWrap = false;
			form.Controls.Add(textBox1);
			Function();
			form.ShowDialog();
		}
		#endregion

		#region Головні функції

		#region Матриці: інцидентності, суміжності, відстаней, досяжності

		//Отримання матриць інцидентності та суміжності
		public void Get_Incident_and_Adjacency_Matrix(string path)
		{
			WriteMatrix(IncidentMatrix, Vershuna, Rebro, path + @"\IncidentMatrix.txt", "Матриця Інцидентності");
			WriteMatrix(AdjacencyMatrix, Vershuna, Vershuna, path + @"\AdjacencyMatrix.txt", "Матриця Суміжності");
			Process.Start(path + @"\IncidentMatrix.txt");
			Process.Start(path + @"\AdjacencyMatrix.txt");
		}
		public void Get_Incident_and_Adjacency_Matrix()
		{
			TextBox textBox1 = new System.Windows.Forms.TextBox();
			Initialize(textBox1, () =>
			{
				WriteMatrix(IncidentMatrix, Vershuna, Rebro, textBox1, "Incident Matrix");
				WriteMatrix(AdjacencyMatrix, Vershuna, Vershuna, textBox1 , "Adjacency Matrix");
			});
		}

		//Отримання матриць Відстаней та Досяжності
		public void Get_Distance_and_Reach_Matrix()
		{
			TextBox textbox = new System.Windows.Forms.TextBox();
			Initialize(textbox, () =>
			{
				WriteMatrix(DistanceMatrix, Vershuna, Vershuna, textbox, "Матриця Відстаней");
				WriteMatrix(ReachMatrix, Vershuna, Vershuna, textbox, "Матриця Досяжності");
			});
		}
		public void Get_Distance_and_Reach_Matrix(string path)
		{
			WriteMatrix(DistanceMatrix, Vershuna, Vershuna, path + @"\DistanceMatrix.txt", "Матриця Відстаней");
			WriteMatrix(ReachMatrix, Vershuna, Vershuna, path + @"\ReachMatrix.txt", "Матриця Досяжності");
			Process.Start(path + @"\DistanceMatrix.txt");
			Process.Start(path + @"\ReachMatrix.txt");
		}

		#endregion

		#region Степені вершин та список ізольованих та висячих вершин

		//Отримання степеней вершин графу
		public void Get_DegreeVertices_Matrix(string path)
		{
			string Type;
			int[] Vertices = new int[Vershuna];
			for (int i = 0; i < Vershuna; i++)
				Vertices[i] = DegreeVertices[i];
			if (Vertices.Max() == Vertices.Min())
				Type = "Граф однорідний! Його степінь " + Vertices.Max();
			else
				Type = "Граф неоднорідний!";
			WriteMatrix(DegreeVertices, Vershuna, path + @"\DegreeVertices.txt", Type);
			WriteMatrix(DegreeInput, Vershuna, path + @"\DegreeInput.txt", "Напівстепені входу");
			WriteMatrix(DegreeOutput, Vershuna, path + @"\DegreeOutput.txt", "Напівстепені виходу");
			Process.Start(path + @"\DegreeInput.txt");
			Process.Start(path + @"\DegreeOutput.txt");
			Process.Start(path + @"\DegreeVertices.txt");
		}
		public void Get_DegreeVertices_Matrix()
		{
			TextBox textbox = new System.Windows.Forms.TextBox();
			Initialize(textbox, () =>
			{
				string Type;
				if (DegreeVertices.Max() == DegreeVertices.Min())
					Type = "Граф однорідний! Його степінь " + DegreeVertices.Max();
				else
					Type = "Граф неоднорідний!";
				WriteMatrix(DegreeVertices, Vershuna, textbox, Type);
				WriteMatrix(DegreeInput, Vershuna, textbox, "Напівстепені входу");
				WriteMatrix(DegreeOutput, Vershuna, textbox, "Напівстепені виходу");
			});
		}

		//Отримання списку Ізольованих та Висячих вершин
		public void Get_IsolatedVertex_and_EndVertex()
		{
			TextBox textbox = new System.Windows.Forms.TextBox();
			Initialize(textbox, () =>
			{
				int flag = 0;
				string IzolatedVertex = "Ізольовані вершини: ", EndVertex = "\r\n\r\nВисячі вершини: ";
				for (int i = 0; i < Vershuna; i++)
				{
					if (DegreeVertices[i] == 0)
						IzolatedVertex += "[" + (i + 1) + "], ";
					else if (DegreeVertices[i] == 1)
						EndVertex += "[" + (i + 1) + "], ";
					else
						flag++;
				}
				if (flag == Vershuna)
				{
					IzolatedVertex += "Не існує";
					EndVertex += "Не існує";
				}
				textbox.Text = IzolatedVertex + EndVertex;
			});
		}

		#endregion

		#region Прості цикли ти типи зв'язності графу

		//Отримання циклів
		public void Get_Cycle()
		{
			TextBox textbox = new System.Windows.Forms.TextBox();
			Initialize(textbox, () =>
			{
				bool flag = false;
				int[] marker = new int[Vershuna];
				List<int> Versh = new List<int>();
				List<int> queue = new List<int>();
				for (int i = 0; i < Vershuna; i++)
					if (DistanceMatrix[i, i] > 1)
						Versh.Add(i);
				for (int z = 0; z < Versh.Count; z++)
				{
					for (int i = 0; i < Rebro; i++)
						if (IncidentMatrix[Versh[z], i] == -1)
							for (int j = 0; j < Vershuna; j++)
								if (IncidentMatrix[j, i] == 1)
								{ queue.Add(j); break; }
					string Text;
					flag = false;
					for (int i = 0; i < queue.Count; i++)
					{
						Text = null;
						Text += Cycle(queue[i], IncidentMatrix, Versh[z], ref marker, out flag);
						if (flag)
						{
							Text += String.Format("{0} ", Versh[z] + 1);
							string[] buff = Text.Split(' ');
							Text = "[";
							for (int x = buff.Length - 1; x >= 0; x--)
								Text += string.Format("{0} ", buff[x]);
							Text += "]\r\n";
							textbox.Text += Text;
							break;
						}
						marker = new int[Vershuna];
					}
					queue.Clear();
				}
				if (!Is_Cycle)
					textbox.Text += "Циклів не існує";
			});
		}
		private string Cycle(int v,int[,] IncidentMatrix,int ololo, ref int[] Marker, out bool flag )
		{
			flag = false;
			List<int> queue = new List<int>();
			string Text = null;
			if (v == ololo)
			{
				Text = String.Format("{0} ", v + 1); 
				flag = true;
				return Text;
			}
			if (Marker[v] == 1)
				return null;
			Marker[v] = 1;
			for (int i = 0; i < Rebro; i++)
				if (IncidentMatrix[v, i] == -1)
					for (int j = 0; j < Vershuna; j++)
						if (IncidentMatrix[j, i] == 1)
						{ queue.Add(j); break; }
			for (int i = 0; i < queue.Count; i++)
			{
				if (Marker[queue[i]] == 1)
					continue;
				Text += Cycle(queue[i], IncidentMatrix,ololo, ref Marker, out flag);
				if (flag)
				{ Text += String.Format("{0} ",v+1); break; }
			}
			return Text;
		}

		//Отримання типу зв'язності
		public void Get_Type_of_Connectivity()
		{
			TextBox textbox = new System.Windows.Forms.TextBox();
			Initialize(textbox, () =>
			{
				byte type = 0;
				do
				{
					{//Визначення сильної зв'язності
						UInt32 temp = 0;
						for (int i = 0; i < Vershuna; i++)
							for (int j = 0; j < Vershuna; j++)
								if (ReachMatrix[i, j] == 1)
									temp++;
						if (temp == (Vershuna * Vershuna))
						{ type = 1; break; }
					}
					{//Визначення однобічної зв'язності
						int[,] Transpose = new int[Vershuna, Vershuna];
						UInt32 temp = 0;
						for (int i = 0; i < Vershuna; i++)//Транспонування матриці досяжності
							for (int j = 0; j < Vershuna; j++)
								Transpose[j, i] = ReachMatrix[i, j];
						for (int i = 0; i < Vershuna; i++)//Додаваня матриць траспонованої та досяжності
							for (int j = 0; j < Vershuna; j++)
							{
								Transpose[i, j] += ReachMatrix[i, j];
								if (Transpose[i, j] > 0)
								{ Transpose[i, j] = 1; temp++; }
							}
						if (temp == (Vershuna * Vershuna))
						{ type = 2; break; }
					}
					{//Визначення слабої зв'язності
						int[,] Transpose_Adjcency = new int[Vershuna, Vershuna];
						UInt32 temp = 0;
						int[,] One = new int[Vershuna, Vershuna];
						for (int i = 0; i < Vershuna; i++)//Транспонування матриці суміжності та ініціалізація одиничної матриці
						{
							One[i, i] = 1;
							for (int j = 0; j < Vershuna; j++)
								Transpose_Adjcency[j, i] = AdjacencyMatrix[i, j];
						}
						for (int i = 0; i < Vershuna; i++)//Додаваня матриць одиничної, траспонованої та суміжності
							for (int j = 0; j < Vershuna; j++)
								Transpose_Adjcency[i, j] += AdjacencyMatrix[i, j] + One[i, j];
						int[,] TempArray = new int[Vershuna, Vershuna];
						for (int power = 2; power < Vershuna; power++)
						{
							for (int k = 0; k < Vershuna; k++)//Піднесення транспонованої матриці до степеня
								for (int i = 0; i < Vershuna; i++)
									for (int j = 0; j < Vershuna; j++)
										TempArray[k, i] += Transpose_Adjcency[k, j] * Transpose_Adjcency[j, i];
							Array.Copy(TempArray, Transpose_Adjcency, Vershuna * Vershuna);
							for (int i = 0; i < Vershuna; i++)//Булеве перетворення транспонованої матриці 
								for (int j = 0; j < Vershuna; j++)
									if (Transpose_Adjcency[i, j] > 0)
										Transpose_Adjcency[i, j] = 1;
						}
						for (int i = 0; i < Vershuna; i++)//Булеве перетворення транспонованої матриці 
							for (int j = 0; j < Vershuna; j++)
								if (Transpose_Adjcency[i, j] == 1)
									temp++;
						if (temp == (Vershuna * Vershuna))
						{ type = 3; break; }
					}
				} while (false);
				switch (type)
				{
					case 0:
						textbox.Text = "Граф є незв'язним"; break;
					case 1:
						textbox.Text = "Граф є сильнозв'язним"; break;
					case 2:
						textbox.Text = "Граф є однобічнозв'язним"; break;
					case 3:
						textbox.Text = "Граф є слабозв'язним"; break;
					default:
						break;
				}
			});
		}

		#endregion

		#region Пошуки вшир та вглиб

		//Обхід дерева графу методом BFS(вшир)
		public void Get_BFS_Method()
		{
			TextBox textbox = new System.Windows.Forms.TextBox();
			Initialize(textbox, () =>
			{
				if ((Versh_Tree < 0) || (Versh_Tree > Vershuna - 1))
					throw new System.ArgumentException("Data must be stands for 1 to number of degree");
				int Increment = 0;
				int bfs = 1;
				object[,] Table = new object[Vershuna * 2, 3];
				int[] Marker = new int[Vershuna];
				Queue<int> queue = new Queue<int>();
				BFS(Versh_Tree, queue, IncidentMatrix, ref Marker, ref Increment, ref Table, ref bfs);
				textbox.Text += String.Format("{0,2}| Вершина |  BFS-номер |           Вміст черги        |\r\n", Versh_Tree + 1);
				textbox.Text += "--|---------|------------|------------------------------|\r\n";
				int i = 0;
				while (Table[i, 2] != null)
				{
					textbox.Text += String.Format("  |  {0,4:D}   |    {1,5:D}   | {2} \r\n", Table[i, 0], Table[i, 1], Table[i, 2]);
					i++;
				}
			});
		}
		private void BFS(int v, Queue<int> queue, int[,] IncidentMatrix,ref int[] Marker, ref int Increment, ref object[,] Table, ref int bfs)
		{
			string buffer = null;
			if (Marker[v] == 0)
			{
				Marker[v] = 1;
				queue.Enqueue(v);
				Table[Increment, 0] = v + 1;
				foreach (var item in queue)
					buffer += String.Format("{0} ", item+1);
				Table[Increment, 2] = buffer;
				Table[Increment, 1] = bfs++;
				Increment++;
			}
			for (int i = 0; i < Rebro; i++)
				if (IncidentMatrix[v, i] == -1)
					for (int j = 0; j < Vershuna; j++)
						if (IncidentMatrix[j, i] == 1)
						{
							if (Marker[j] == 1)
								break;
							buffer = null;
							Marker[j] = 1;
							queue.Enqueue(j);
							Table[Increment, 0] = j+1;
							foreach (var item in queue)
								buffer += String.Format("{0} ", item+1);
							Table[Increment, 2] = buffer;
							Table[Increment, 1] = bfs++;
							Increment++;
						}
			buffer = null;
			queue.Dequeue();
			Table[Increment, 0] = "----";
			foreach (var item in queue)
				buffer += String.Format("{0} ", item+1);
			Table[Increment, 2] = buffer;
			Table[Increment, 1] = "----";
			Increment++;
			for (int i = 0; i < queue.Count; i++)
				BFS(queue.Peek(), queue, IncidentMatrix, ref Marker, ref Increment, ref Table, ref bfs);
		}

		//Обхід дерева графу методом DFS(вглиб)
		public void Get_DFS_Method()
		{
			TextBox textbox = new System.Windows.Forms.TextBox();
			Initialize(textbox, () =>
			{
				if ((Versh_Tree < 0) || (Versh_Tree > Vershuna - 1))
					throw new System.ArgumentException("Data must be stands for 1 to number of degree");
				int Increment = 0;
				int bfs = 1;
				object[,] Table = new object[Vershuna * 2, 3];
				int[] Marker = new int[Vershuna];
				Stack<int> stack = new Stack<int>();
				DFS(Versh_Tree, stack, IncidentMatrix, ref Marker, ref Increment, ref Table, ref bfs);
				textbox.Text += String.Format("{0,2}| Вершина |  DFS-номер |           Вміст стеку        |\r\n", Versh_Tree + 1);
				textbox.Text += "--|---------|------------|------------------------------|\r\n";
				int i = 0;
				while (Table[i, 2] != null)
				{
					textbox.Text += String.Format("  |  {0,4:D}   |    {1,5:D}   | {2} \r\n", Table[i, 0], Table[i, 1], Table[i, 2]);
					i++;
				}
			});
		}
		private void DFS(int v, Stack<int> stack, int[,] IncidentMatrix, ref int[] Marker, ref int Increment, ref object[,] Table, ref int bfs)
		{
			string buffer = null;
			if (Marker[v] == 0)
			{
				Marker[v] = 1;
				stack.Push(v);
				Table[Increment, 0] = v + 1;
				foreach (var item in stack)
					buffer += String.Format("{0} ", item + 1);
				Table[Increment, 2] = Inverse(buffer);
				Table[Increment, 1] = bfs++;
				Increment++;
			}
			for (int i = 0; i < Rebro; i++)
				if (IncidentMatrix[v, i] == -1)
					for (int j = 0; j < Vershuna; j++)
						if (IncidentMatrix[j, i] == 1)
						{
							if (Marker[j] == 1)
								break;
							buffer = null;
							Marker[j] = 1;
							stack.Push(j);
							Table[Increment, 0] = j + 1;
							foreach (var item in stack)
								buffer += String.Format("{0} ", item + 1);
							Table[Increment, 2] = Inverse(buffer);
							Table[Increment, 1] = bfs++;
							Increment++;
							DFS(stack.Peek(), stack, IncidentMatrix, ref Marker, ref Increment, ref Table, ref bfs);
						}
			buffer = null;
			stack.Pop();
			Table[Increment, 0] = "----";
			foreach (var item in stack)
				buffer += String.Format("{0} ", item + 1);
			Table[Increment, 2] = Inverse(buffer);
			Table[Increment, 1] = "----";
			Increment++;
		}
		private string Inverse(string Text)
		{
			if (Text == null)
				return null;
			string temp = null;
			string[] buffer = Text.Split(' ');
			for (int x = buffer.Length - 1; x >= 0; x--)
				temp += string.Format("{0} ", buffer[x]);
			return temp;
		}

		#endregion

		#region Топологічне сортування та сильні компоненти зв'язності графу

		//Топологічне сортування
		public void Get_Topological_Sort()
		{
			TextBox textbox = new System.Windows.Forms.TextBox();
			Initialize(textbox, () =>
			{
				if (Is_Cycle)
					throw new InvalidOperationException("This graph has a some cycle, if you want to sort topologically graph, please reboot the application and choose another graph.");
				int[] Marker = new int[Vershuna], Topolog_Array = new int[Vershuna];
				int Current_Label = Vershuna - 1;
				for (int i = 0; i < Vershuna; i++)
				{
					if (Marker[i] == 1)
						continue;
					DFSR(i, IncidentMatrix, ref Marker, ref Topolog_Array, ref Current_Label);
				}
				textbox.Text = "Топологiчно посортований граф\r\n\r\n";
				foreach (var item in Topolog_Array)
					textbox.Text += String.Format("{0} ", item);
			});
		}
		private void DFSR(int v, int[,] IncidentMatrix, ref int[] Marker, ref int[] Topolog_Array, ref int Current_Label)
		{
			Marker[v] = 1;
			for (int i = 0; i < Rebro; i++)
				if (IncidentMatrix[v, i] == -1)
					for (int j = 0; j < Vershuna; j++)
						if (IncidentMatrix[j, i] == 1)
						{
							if (Marker[j] == 1)
								break;
							Marker[j] = 1;
							DFSR(j, IncidentMatrix, ref Marker, ref Topolog_Array, ref Current_Label);
						}
			Topolog_Array[Current_Label--] = v+1;
		}

		//Сильні компоненти зв'язності графу
		public void Get_graph_Connectivity()
		{
			TextBox textbox = new System.Windows.Forms.TextBox();
			Initialize(textbox, () =>
			{
				int[] Time_Array = new int[Vershuna];
				int[,] IncidentMatrixCopy = new int[Vershuna, Rebro];
				Array.Copy(IncidentMatrix, IncidentMatrixCopy, Vershuna * Rebro);
				for (int i = Vershuna - 1, temp = 0; i >= 0; i--, temp++)
					Time_Array[i] = temp;
				string asd;
				DFS_Loop(IncidentMatrixCopy, ref Time_Array, out asd);
				Transpose_graph(Matrix, out IncidentMatrixCopy);
				DFS_Loop(IncidentMatrixCopy, ref Time_Array, out asd);
				textbox.Text = "Сильнi компоненти зв'язностi графу \r\n\r\n" + asd;
			});
		}
		private void DFS_Loop(int[,] IncidentMatrix, ref int[] Time_Array,out string Component)
		{
			int Number_component = 1;
			Component = null;
			int[] Marker = new int[Vershuna];
			int[] Temp = new int[Vershuna];
			Array.Copy(Time_Array, Temp,Vershuna);
			int time = 0;
			for (int i = Vershuna-1; i>=0; i--)
			{
				int tempteime = time;
				if (Marker[Temp[i]] == 1)
					continue;
				DFSR_Connectivity(Temp[i], IncidentMatrix, ref Marker, ref Time_Array, ref time);
				Component += String.Format("Компонента {0,2}: [",Number_component++);
				while (tempteime < time)
					Component += String.Format(" {0}", Time_Array[tempteime++]+1);
				Component += " ]\r\n";
			}
		}
		private void DFSR_Connectivity(int v, int[,] IncidentMatrix, ref int[] Marker, ref int[] Time_Array, ref int time)
		{
			Marker[v] = 1;
			for (int i = 0; i < Rebro; i++)
				if (IncidentMatrix[v, i] == -1)
					for (int j = 0; j < Vershuna; j++)
						if (IncidentMatrix[j, i] == 1)
						{
							if (Marker[j] == 1)
								break;
							Marker[j] = 1;
							DFSR_Connectivity(j, IncidentMatrix, ref Marker, ref Time_Array, ref time);
						}
			Time_Array[time++] = v;
		}
		private void Transpose_graph(int[,] graph, out int[,] IncidentMatrix)
		{
			IncidentMatrix = new int[Vershuna, Rebro];
			for (int i = 0; i < Rebro; i++)
			{
				if (graph[i, 0] == graph[i, 1])
					IncidentMatrix[graph[i, 0] - 1, i] = 2;
				else
				{
					IncidentMatrix[graph[i, 0] - 1, i] = 1;
					IncidentMatrix[graph[i, 1] - 1, i] = -1;
				}
			}
		}

		#endregion

		#region Пошук найкоротших відстаней
		//Алгоритм Дейкстри
		public void Get_ShortestConnection_Deyxtra()
		{
			TextBox textbox = new System.Windows.Forms.TextBox();
			Initialize(textbox, () =>
			{
				if (!ShortestConnetctionGraph)
					throw new Exception("Знаходження найкоротших шляхів графу неможливе. Граф незважений!");
				if (NegativeWeight)
					throw new Exception("Граф має ребра з від'ємною вагою, виконання алгоритму Дейкстри неможливе!");
				int[] DegreeArray, ShortDistance = ShortestConnection_Deyxtra(Matrix, BeginDegree, out DegreeArray);
				WriteMatrix(ShortDistance, Vershuna, textbox, String.Format("Найкоротші відстані від {0}-ї вершини до усіх інших вершин (аллгоритм Дейкстри)", BeginDegree + 1));
				string path = FindShortestPath(DegreeArray, BeginDegree, EndDegree);
				if (ShortDistance[EndDegree] == 0)
					path = String.Format("Маршруту не існує, {0} вершина недосяжна з {1}-ї вершини", EndDegree + 1, BeginDegree + 1);
				textbox.Text += String.Format("\r\n\r\nНайкоротший маршрут від вершини №{0} до вершини №{1} :{2}", BeginDegree + 1, EndDegree + 1, path);
			});
		}
		private int[] ShortestConnection_Deyxtra(int[,] Graph, int BeginDegree, out int[] ArrayOfDegree)
		{
			ArrayOfDegree = new int[Vershuna];
			int[] Long_m = new int[Vershuna], Temp_m = new int[Vershuna];
            bool[] marker = new bool[Vershuna];
            System.Collections.Generic.Queue<int> queue = new Queue<int>();
            for (int i = 0; i < Vershuna; i++)
                Temp_m[i] = Int16.MaxValue;
            Temp_m[BeginDegree] = 0;
			ShortestConnection_Deyxtra_Recursy(BeginDegree, ref ArrayOfDegree, ref marker, ref queue, ref Long_m, ref Temp_m, Graph);
			return Long_m;
		}
		private void ShortestConnection_Deyxtra_Recursy(int input_degree,ref int[] degree, ref bool[] marker, ref System.Collections.Generic.Queue<int> queue, ref int[] Long_marks, ref int[] Temp_marks, int[,] Graph)
		{
			Dictionary<int, int> dict = new Dictionary<int, int>();
            if (!marker[input_degree])
                marker[input_degree] = true;
            else return;
			Long_marks[input_degree] = Temp_marks[input_degree];
            for (int i = 0; i < Rebro; i++)
				if ((Graph[i,0]-1)==input_degree)
                {
					int temp = Temp_marks[Graph[i, 1] - 1];
					Temp_marks[Graph[i, 1] - 1] = Math.Min(Long_marks[input_degree] + Graph[i, 2], Temp_marks[Graph[i, 1] - 1]);
					if (temp > Temp_marks[Graph[i, 1] - 1])
						degree[Graph[i, 1] - 1] = input_degree;
					dict.Add(Graph[i, 1] - 1, Graph[i, 2]);
                }
			var query = (from asd in dict orderby asd.Value ascending select asd.Key).ToList();
			foreach (int item in query)
				queue.Enqueue(item);
			while(queue.Count!=0)
				ShortestConnection_Deyxtra_Recursy(queue.Dequeue(), ref degree, ref marker, ref queue, ref Long_marks, ref Temp_marks, Graph);
		}
		
		//Алгоритм Белмана-Форда
		public void Get_ShortestConnection_BelmanFord()
		{
			TextBox textbox = new System.Windows.Forms.TextBox();
			Initialize(textbox, () =>
			{
				if (!ShortestConnetctionGraph)
					throw new Exception("Знаходження найкоротших шляхів графу неможливе. Граф незважений!");
				string path;
				int[] A = ShortestConnection_BelmanFord(Matrix, Vershuna, Rebro, BeginDegree, out path);
				WriteMatrix(A, Vershuna, textbox, String.Format("Найкоротші відстані від {0}-ї вершини до усіх інших вершин (аллгоритм Белмана-Форда)", BeginDegree + 1));
				if (A[EndDegree] == 0)
					path = String.Format("Маршруту не існує, {0} вершина недосяжна з {1}-ї вершини", EndDegree + 1, BeginDegree + 1);
				textbox.Text += String.Format("\r\n\r\nНайкоротший маршрут від вершини №{0} до вершини №{1} :{2}", BeginDegree + 1, EndDegree + 1, path);
			});
		}
		private int[] ShortestConnection_BelmanFord(int[,] Graph, int Vershuna, int Rebro, int BeginDegree, out string Path)
		{
			int[] A = new int[Vershuna], DegreeArray = new int[Vershuna];
			for (int i = 0; i < Vershuna; i++)
				A[i] = Int16.MaxValue;
			bool Change = false;
			A[BeginDegree] = 0;
			for (int i = 0; i < Vershuna - 1; i++)
			{
				if (Change)
					break;
				Change = true;
				for (int j = 0; j < Rebro; j++)
					if (A[Graph[j, 1] - 1] > (A[Graph[j, 0] - 1] + Graph[j, 2]))
					{
						Change = false;
						A[Graph[j, 1] - 1] = A[Graph[j, 0] - 1] + Graph[j, 2];
						DegreeArray[Graph[j, 1] - 1] = Graph[j, 0] - 1;
					}
			}
			for (int j = 0; j < Rebro; j++)
				if (A[Graph[j, 0] - 1] < Int32.MaxValue)
				{
					int temp = A[Graph[j, 1] - 1];
					A[Graph[j, 1] - 1] = Math.Min(A[Graph[j, 1] - 1], A[Graph[j, 0] - 1] + Graph[j, 2]);
					if (A[Graph[j, 1] - 1] != temp)
						throw new Exception("Граф має негативні цикли, виконання алгоритму Беллмана-Форда неможливе!");
				}
			Path = FindShortestPath(DegreeArray, BeginDegree, EndDegree);
			return A;
		}
		
		//Алгоритм Флойда-Уоршелла
		public void Get_ShortestConnection_FloydUorshall()
		{
			TextBox textbox = new System.Windows.Forms.TextBox();
			Initialize(textbox, () =>
			{
				if (!ShortestConnetctionGraph)
					throw new Exception("Знаходження найкоротших шляхів графу неможливе. Граф незважений!");
				int[] DegreeArray;
				int[,] Distance = ShortestConnection_FloydUorshall(ShortestDistanceMatrix, BeginDegree, out DegreeArray);
				WriteMatrix(Distance, Vershuna, Vershuna, textbox, String.Format("Найкоротші відстані для будь-якої пари вершин (аллгоритм Флойда-Уоршелла)\r\n"));
				string path = FindShortestPath(DegreeArray, BeginDegree, EndDegree);
				if ((Distance[BeginDegree, EndDegree] == 0) || (Distance[BeginDegree, EndDegree] == Int16.MaxValue))
					path = String.Format("Маршруту не існує, {0} вершина недосяжна з {1}-ї вершини", EndDegree + 1, BeginDegree + 1);
				textbox.Text += String.Format("\r\n\r\nНайкоротший маршрут від вершини №{0} до вершини №{1} :{2}", BeginDegree + 1, EndDegree + 1, path);
			});
		}
		private int[,] ShortestConnection_FloydUorshall(int [,] MatrixOfShortDistance, int BeginDegree,  out int[] ArrayOfDegree)
		{
			ArrayOfDegree = new int[Vershuna];
			int[,] Distance = new int[Vershuna, Vershuna], TempDistance = new int[Vershuna, Vershuna], Connection = new int[Vershuna, Vershuna], TempConnection = new int[Vershuna, Vershuna];
			Array.Copy(MatrixOfShortDistance, Distance, Vershuna * Vershuna);
			for (int i = 0; i < Vershuna; i++)
			{
				for (int j = 0; j < Vershuna; j++)
				{
					Connection[i, j] = i;
					if (Distance[i, j] == 0)
						Distance[i, j] = Int16.MaxValue;
				}
				Distance[i, i] = Connection[i, i] = 0;
			}
			for (int k = 0; k < Vershuna; k++)
			{
				Array.Copy(Distance, TempDistance, Vershuna * Vershuna);
				Array.Copy(Connection, TempConnection, Vershuna * Vershuna);
				for (int i = 0; i < Vershuna; i++)
					for (int j = 0; j < Vershuna; j++)
					{
						Distance[i, j] = Math.Min(Distance[i, j], Distance[i, k] + Distance[k, j]);
						Connection[i, j] = TempDistance[i, j] <= TempDistance[i, k] + TempDistance[k, j] ? TempConnection[i, j] : TempConnection[k, j];
						if (Distance[i, i] < 0)
							throw new Exception("Граф має негативні цикли, виконання алгоритму Флойда-Уоршелла неможливе!");
					}
			}
			for (int i = 0; i < Vershuna; i++)
				ArrayOfDegree[i] = Connection[BeginDegree, i];
			for (int i = 0; i < Vershuna; i++)
				for (int j = 0; j < Vershuna; j++)
					if (Distance[i, j]/10000>1)
						Distance[i, j] = 0;
			return Distance;
		}
		
		//Алгоритм Джонсона
		public void Get_ShortestConnection_Jonson()
		{
			TextBox textbox = new System.Windows.Forms.TextBox();
			Initialize(textbox, () =>
			{
				if (!ShortestConnetctionGraph)
					throw new Exception("Знаходження найкоротших шляхів графу неможливе. Граф незважений!");
				int[,] NewGraph = new int[Rebro + Vershuna, 3], MatrixOfShortDist = new int[Vershuna, Vershuna], OutMatrix = new int[Vershuna, Vershuna];
				Array.Copy(Matrix, NewGraph, Rebro * 3);
				int[] ShortestMatrix = new int[Vershuna + 1], LineOfMatrix = new int[Vershuna], degreeArray = new int[Vershuna];
				//додавання нової вершини та ребер в новий граф
				for (int i = Rebro, k = 1; i < Rebro + Vershuna; i++)
				{
					NewGraph[i, 0] = Vershuna + 1;
					NewGraph[i, 1] = k++;
				}
				string path;
				//Пошук найкоротших відстаней від нової вершини до усіх інших
				ShortestMatrix = ShortestConnection_BelmanFord(NewGraph, Vershuna + 1, Rebro + Vershuna, Vershuna, out path);
				//Визначення нових ваг ребер графу
				for (int i = 0; i < Rebro; i++)
					NewGraph[i, 2] = NewGraph[i, 2] + ShortestMatrix[NewGraph[i, 0] - 1] - ShortestMatrix[NewGraph[i, 1] - 1];
				//виконання алг Дейкстри для пошуку найкоротших шляхів до усіх вершин.
				for (int i = 0; i < Vershuna; i++)
				{
					LineOfMatrix = ShortestConnection_Deyxtra(NewGraph, i, out degreeArray);
					for (int j = 0; j < Vershuna; j++)
						OutMatrix[i, j] = LineOfMatrix[j];
					if (BeginDegree == i)
						path = FindShortestPath(degreeArray, BeginDegree, EndDegree);
				}
				//Визначення реальних відстаней у матриці
				for (int i = 0; i < Rebro; i++)
					OutMatrix[NewGraph[i, 0] - 1, NewGraph[i, 1] - 1] = OutMatrix[NewGraph[i, 0] - 1, NewGraph[i, 1] - 1] - ShortestMatrix[NewGraph[i, 0] - 1] + ShortestMatrix[NewGraph[i, 1] - 1];
				OutMatrix = ShortestConnection_FloydUorshall(ShortestDistanceMatrix, BeginDegree, out degreeArray);
				WriteMatrix(OutMatrix, Vershuna, Vershuna, textbox, String.Format("Найкоротші відстані для будь-якої пари вершин (аллгоритм Джонсона)\r\n"));
				if (OutMatrix[BeginDegree, EndDegree] == 0)
					path = String.Format("Маршруту не існує, {0} вершина недосяжна з {1}-ї вершини", EndDegree + 1, BeginDegree + 1);
				textbox.Text += String.Format("\r\n\r\nНайкоротший маршрут від вершини №{0} до вершини №{1} :{2}", BeginDegree + 1, EndDegree + 1, path);
			});
		}

		#endregion

		#region Ейлерові та Гамільтонові цикли, маршрути

		//Ейлерові
		public void Get_Eyler_Cycle()
		{
			TextBox textbox = new System.Windows.Forms.TextBox();
			Initialize(textbox, () =>
			{
				textbox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
				textbox.WordWrap = true;
				if (!(from i in DegreeInput select i).SequenceEqual(from x in DegreeOutput select x))
				{ 
					textbox.Text += "Граф не містить Ейлерових циклів\r\n\r\n";
					textbox.Text += Get_Eyler_Path();
					return;
				}
				int[,] tempAdjacencyMatrix = new int[Vershuna,Vershuna];
				Array.Copy(AdjacencyMatrix,tempAdjacencyMatrix,Vershuna*Vershuna);
				List<string> dict = new List<string>(), list = new List<string>();
				for (int i = 0; i < Rebro; i++)
					list.Add(String.Format("{0} {1}", Matrix[i, 0], Matrix[i, 1]));
				bool allBridge = true;
				int vershuna = 0, increment = 0;
				while (dict.Count != Rebro)
				{
					string temp=null;
					for (int i = 0; i < Vershuna; i++)
						if (tempAdjacencyMatrix[vershuna, i] > 0)
						{
							if (tempAdjacencyMatrix[i, vershuna] > 0)
							{
								dict.Add(String.Format("{0} {1}", vershuna + 1, i + 1));
								list.Remove(String.Format("{0} {1}", vershuna + 1, i + 1));
								allBridge = false;
								tempAdjacencyMatrix[vershuna, i] -= 1;
								vershuna = i;
								break;
							}
							else
								temp = String.Format("{0} {1}", vershuna + 1, i + 1);
						}
					if (allBridge)
					{
						tempAdjacencyMatrix[Convert.ToInt32(temp.Split(' ')[0]) - 1, Convert.ToInt32(temp.Split(' ')[1]) - 1] -= 1;
						dict.Add(temp);
						vershuna = Convert.ToInt32(temp.Split(' ')[1]) - 1;
					}
					allBridge = true;
					temp = null;
				}
				textbox.Text += "Знайдеий Ейлеровий цикл\r\n";
				foreach (var item in dict)
					textbox.Text += String.Format("{0} -->", item.Split(' ')[0]);
				textbox.Text += "1\r\n";
			});
		}
		private string Get_Eyler_Path()
		{
			int[,] tempAdjacencyMatrix = new int[Vershuna, Vershuna];
			Array.Copy(AdjacencyMatrix, tempAdjacencyMatrix, Vershuna * Vershuna);
			string Path = null;
			int tempcount = 0, begDegree = 0, endDegree = 0; 
			for (int i = 0; i < Vershuna; i++ )
			{
				if ((DegreeInput[i] != DegreeOutput[i])&&DegreeInput[i]>DegreeOutput[i])
				{tempcount++; endDegree = i;}
				if ((DegreeInput[i] != DegreeOutput[i])&&DegreeInput[i]<DegreeOutput[i])
					{tempcount++; begDegree = i;}
			}
			if (tempcount!=2)
				return "Ейлерового маршруту не існує!";
			List<string> dict = new List<string>(), list = new List<string>();
			bool allBridge = true;
			int vershuna = begDegree, increment = 0;
			while (dict.Count != Rebro)
			{
				string temp = null;
				for (int i = 0; i < Vershuna; i++)
					if (tempAdjacencyMatrix[vershuna, i] > 0)
					{
						if (tempAdjacencyMatrix[i, vershuna] > 0)
						{
							dict.Add(String.Format("{0} {1}", vershuna + 1, i + 1));
							list.Remove(String.Format("{0} {1}", vershuna + 1, i + 1));
							allBridge = false;
							tempAdjacencyMatrix[vershuna, i] -= 1;
							vershuna = i;
							break;
						}
						else
							temp = String.Format("{0} {1}", vershuna + 1, i + 1);
					}
				if (allBridge)
				{
					tempAdjacencyMatrix[Convert.ToInt32(temp.Split(' ')[0]) - 1, Convert.ToInt32(temp.Split(' ')[1]) - 1] -= 1;
					dict.Add(temp);
					vershuna = Convert.ToInt32(temp.Split(' ')[1]) - 1;
				}
				allBridge = true;
				temp = null;
			}
			Path += "Знайдений Ейлеровий маршрут\r\n";
			foreach (var item in dict)
				Path += String.Format("{0} -->", item.Split(' ')[0]);
			Path += endDegree + 1;
			return Path;
		}

		//Гамільтонові
		public void Get_Gamilton_Cycle()
		{
			TextBox textbox = new System.Windows.Forms.TextBox();
			Initialize(textbox, () =>
			{
				//if (!Check_Dirak() || !Check_Ore())
				//{
				//	textbox.Text += "Граф не мiстить гамiльтонових циклiв!";
				//	return;
				//}
				bool[] Marker = new bool[Vershuna]; bool End = false;
				List<int> gamilton = new List<int>();
				Search_cycle(0, 0, ref Marker, ref gamilton, ref End);
				if (gamilton.Count == 0)
				{
					Marker = new bool[Vershuna]; End = false; gamilton.Clear();
					textbox.Text = "Гамільтонового циклу не існує\r\n\r\n";
					Search_path(0,ref Marker,ref gamilton, ref End);
					if (gamilton.Count != Vershuna)
						textbox.Text += "Гамiльтонового маршруту не iснує";
					else
					{
						textbox.Text += "Знайдений Гамiльтоновий маршрут\r\n";
						for (int i = 0; i < Vershuna-1; i++ )
							textbox.Text += String.Format("{0} -->", gamilton[i] + 1);
						textbox.Text += gamilton[Vershuna - 1] + 1;
					}
				}
				else
				{
					textbox.Text = "Знайдений Гамільтоновий цикл\r\n";
					foreach (var item in gamilton)
						textbox.Text += String.Format("{0} -->", item + 1);
					textbox.Text += " 1";
				}
			});
		}
		private void Search_cycle(int Degree, int EndDegree, ref bool[] Marker, ref List<int> GamiltonCycle, ref bool End)
		{
			if ((from i in Marker where i == true select i).Count() == Marker.Count() && Degree == EndDegree)
			{
				End = true;
				return;
			}
			if (!Marker[Degree])
			{
				Marker[Degree] = true;
				GamiltonCycle.Add(Degree);
			}
			else return;
			for (int i = 0; i < Vershuna; i++)
				if (AdjacencyMatrix[Degree, i] > 0)
				{
					Search_cycle(i, EndDegree, ref Marker, ref GamiltonCycle, ref End);
					if (End)
						return;
				}
			Marker[Degree] = false;
			GamiltonCycle.Remove(Degree);
		}
		private void Search_path(int Degree, ref bool[] Marker, ref List<int> GamiltonPath, ref bool End)
		{
			if (!Marker[Degree])
			{
				Marker[Degree] = true;
				GamiltonPath.Add(Degree);
			}
			else return;
			for (int i = 0; i < Vershuna; i++)
				if (AdjacencyMatrix[Degree, i] > 0)
				{
					Search_path(i, ref Marker, ref GamiltonPath, ref End);
					if ((from a in Marker where a == true select a).Count() == Marker.Count())
					{
						End = true;
						return;
					}
					if (End)
						return;
				}
			Marker[Degree] = false;
			GamiltonPath.Remove(Degree);
		}
		private bool Check_Dirak()
		{
			foreach (var item in DegreeVertices)
				if (item < Vershuna / 2)
					return false;
			return true;
		}
		private bool Check_Ore()
		{
			for (int i = 0; i < Vershuna; i++)
				for (int j = 0; j < Vershuna; j++ )
					if (DegreeVertices[i]+DegreeVertices[j] < Vershuna)
						return false;
			return true;
		}

		#endregion

		#region Задача Комівояжера

		public void Get_Comivoyajer()
		{

		}

		#endregion

		#endregion
	}
}