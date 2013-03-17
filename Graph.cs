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
		private bool Is_Cycle = false;
		private static int Vershuna, Rebro;
		private static int[,] Matrix, IncidentMatrix, AdjacencyMatrix, DistanceMatrix, ReachMatrix;
		private static int[] DegreeVertices, DegreeInput, DegreeOutput;
		public int Versh_Tree, get_Vershuna = Vershuna;
		public Graph() { }

		//Створення матриць у класі.
		public void SetMatrix() 
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
			Check_Cycle();
		}

		//Зчитування графу
		public void ReadGraph(TextBox textbox1)
		{
			FileStream fls = new FileStream("a.txt", FileMode.OpenOrCreate, FileAccess.Write);
			fls.SetLength(0);
			StreamWriter wrt = new StreamWriter(fls);
			wrt.Write(textbox1.Text);
			wrt.Flush();
			wrt.Close();
			fls.Close();
			using (StreamReader f = File.OpenText("a.txt"))
			{
				string temp = f.ReadLine();
				string[] arr = temp.Split(' ');
				Vershuna = Convert.ToInt32(arr[0]);
				Rebro = Convert.ToInt32(arr[1]);
				Matrix = new int[Rebro, 2];
				if (Rebro != 0)
				{
					int i = 0;
					while (true)
					{
						temp = f.ReadLine();
						if (temp == null)
							break;
						arr = temp.Split(' ');
						Matrix[i, 0] = Convert.ToInt32(arr[0]);
						Matrix[i, 1] = Convert.ToInt32(arr[1]);
						i++;
					}
					f.Close();
				}
			}
			File.Delete("a.txt");
			SetMatrix();
		}
		public void ReadGraph(OpenFileDialog dialog)
		{
			Stream f = dialog.OpenFile();
			StreamReader str = new StreamReader(f);
			string temp = str.ReadLine();
			string[] arr = temp.Split(' ');
			Vershuna = Convert.ToInt32(arr[0]);
			Rebro = Convert.ToInt32(arr[1]);
			Matrix = new int[Rebro, 2];
			int i = 0;
			while (true)
			{
				temp = str.ReadLine();
				if (temp == null)
					break;
				arr = temp.Split(' ');
				for (int j = 0; j < 2; j++)
					Matrix[i, j] = Convert.ToInt32(arr[j]);
				i++;
			}
			str.Close();
			str.Dispose();
			f.Close();
			f.Dispose();
			SetMatrix();
		}

		//Отримання матриць інцидентності та суміжності
		public void Get_Incident_and_Adjacency_Matrix(FolderBrowserDialog folder)
		{
			WriteMatrix(IncidentMatrix, Vershuna, Rebro, folder.SelectedPath + @"\IncidentMatrix.txt", "Матриця Інцидентності");
			WriteMatrix(AdjacencyMatrix, Vershuna, Vershuna, folder.SelectedPath + @"\AdjacencyMatrix.txt", "Матриця Суміжності");
			Process.Start(folder.SelectedPath + @"\IncidentMatrix.txt");
			Process.Start(folder.SelectedPath + @"\AdjacencyMatrix.txt");
		}
		public void Get_Incident_and_Adjacency_Matrix(TextBox textbox)
		{
			WriteMatrix(IncidentMatrix, Vershuna, Rebro, textbox, "Incident Matrix");
			WriteMatrix(AdjacencyMatrix, Vershuna, Vershuna, textbox, "Adjacency Matrix");
		}

		//Отримання степеней вершин графу
		public void Get_DegreeVertices_Matrix(FolderBrowserDialog folder)
		{
			string Type;
			int[] Vertices = new int[Vershuna];
			for (int i = 0; i < Vershuna; i++)
				Vertices[i] = DegreeVertices[i];
			if (Vertices.Max() == Vertices.Min())
				Type = "Граф однорідний! Його степінь " + Vertices.Max();
			else
				Type = "Граф неоднорідний!";
			WriteMatrix(DegreeVertices, Vershuna, folder.SelectedPath + @"\DegreeVertices.txt", Type);
			WriteMatrix(DegreeInput, Vershuna, folder.SelectedPath + @"\DegreeInput.txt", "Напівстепені входу");
			WriteMatrix(DegreeOutput, Vershuna, folder.SelectedPath + @"\DegreeOutput.txt", "Напівстепені виходу");
			Process.Start(folder.SelectedPath + @"\DegreeInput.txt");
			Process.Start(folder.SelectedPath + @"\DegreeOutput.txt");
			Process.Start(folder.SelectedPath + @"\DegreeVertices.txt");
		}
		public void Get_DegreeVertices_Matrix(TextBox textbox)
		{
			string Type;
				if (DegreeVertices.Max()==DegreeVertices.Min())
					Type = "Граф однорідний! Його степінь " + DegreeVertices.Max();
				else
					Type = "Граф неоднорідний!";
			WriteMatrix(DegreeVertices, Vershuna, textbox, Type );
			WriteMatrix(DegreeInput, Vershuna, textbox, "Напівстепені входу");
			WriteMatrix(DegreeOutput, Vershuna, textbox, "Напівстепені виходу");
		}

		//Отримання списку Ізольованих та Висячих вершин
		public void Get_IsolatedVertex_and_EndVertex(TextBox textbox)
		{
			int flag = 0;
			string IzolatedVertex = "Ізольовані вершини: ", EndVertex = "\r\n\r\nВисячі вершини: ";
			for (int i = 0; i < Vershuna; i++)
			{
				if (DegreeVertices[i] == 0)
					IzolatedVertex += "[" + (i+1) + "], ";
				else if (DegreeVertices[i] == 1)
					EndVertex += "[" + (i+1) + "], ";
				else 
					flag++;
			}
			if (flag == Vershuna)
			{
				IzolatedVertex += "Не існує";
				EndVertex += "Не існує";
			}
			textbox.Text = IzolatedVertex + EndVertex;
		}

		//Отримання матриць Відстаней та Досяжності
		public void Get_Distance_and_Reach_Matrix(TextBox textbox)
		{
			WriteMatrix(DistanceMatrix, Vershuna, Vershuna, textbox, "Матриця Відстаней");
			WriteMatrix(ReachMatrix, Vershuna, Vershuna, textbox, "Матриця Досяжності");
		}
		public void Get_Distance_and_Reach_Matrix(FolderBrowserDialog folder)
		{
			WriteMatrix(DistanceMatrix, Vershuna, Vershuna, folder.SelectedPath + @"\DistanceMatrix.txt", "Матриця Відстаней");
			WriteMatrix(ReachMatrix, Vershuna, Vershuna, folder.SelectedPath + @"\ReachMatrix.txt", "Матриця Досяжності");
			Process.Start(folder.SelectedPath + @"\DistanceMatrix.txt");
			Process.Start(folder.SelectedPath + @"\ReachMatrix.txt");
		}
		
		//Запис матриць
		private void WriteMatrix(int[,] args, int FirstArg, int SecondArg, string path, string AddInfo )
		{
			FileStream fls = File.Open(path, FileMode.Create, FileAccess.Write);
			StreamWriter f = new StreamWriter(fls);
			f.WriteLine(AddInfo);
			f.Write("   |");													//Початок дизайну виводу
			for (int temp = 1; temp <= SecondArg; temp++ )
				f.Write("{0,4:D}", temp);
			f.WriteLine();
			f.Write("---|");
			for (int temp = 1; temp <= SecondArg; temp++)
				f.Write("----");												//Кінець
			f.WriteLine();
			for (int i = 0 , temp = 1; i < FirstArg; i++ , temp++)
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

		//Отримання циклів
		public void Get_Cycle(TextBox textbox)
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
					Text +=  Cycle(queue[i], IncidentMatrix, Versh[z], ref marker, out flag);
					if (flag)
					{
						Text += String.Format("{0} " , Versh[z] + 1);
						string[] buff = Text.Split(' ');
						Text = "[";
						for (int x = buff.Length - 1; x >= 0; x--)
							Text += string.Format("{0} ",buff[x]);
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
		private void Check_Cycle()
		{
			for (int i = 0; i < Vershuna; i++)
				if (DistanceMatrix[i, i] > 0)
				{
					Is_Cycle = true;
					break;
				}
		}

		//Отримання типу зв'язності
		public void Get_Type_of_Connectivity(TextBox textbox)
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
		}

		//Обхід дерева графу методом BFS(вшир)
		public void Get_BFS_Method(TextBox textbox)
		{
			if ((Versh_Tree < 0) || (Versh_Tree > Vershuna - 1))
				throw new System.ArgumentException("Data must be stands for 1 to number of degree");
			int Increment = 0;
			int bfs = 1;
			object[,] Table = new object[Vershuna*2,3];
			int[] Marker = new int[Vershuna];
			Queue<int> queue = new Queue<int>();
			BFS(Versh_Tree,queue, IncidentMatrix, ref Marker,ref Increment, ref Table, ref bfs);
			textbox.Text += String.Format("{0,2}| Вершина |  BFS-номер |           Вміст черги        |\r\n",Versh_Tree+1);
			textbox.Text += "--|---------|------------|------------------------------|\r\n";
			int i = 0;
			while (Table[i, 2] != null)
			{
				textbox.Text += String.Format("  |  {0,4:D}   |    {1,5:D}   | {2} \r\n", Table[i, 0], Table[i, 1], Table[i, 2]);
				i++;
			}
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
		public void Get_DFS_Method(TextBox textbox)
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

		//Топологічне сортування
		public void Get_Topological_Sort(TextBox textbox)
		{
			if (Is_Cycle)
				throw new InvalidOperationException("This graph has a some cycle, if you want to sort topologically graph, please reboot the application and choose another graph.");
			int[] Marker = new int[Vershuna], Topolog_Array = new int[Vershuna];
			int Current_Label = Vershuna-1;
			for (int i = 0; i < Vershuna; i++)
			{
				if (Marker[i] == 1)
					continue;
				DFSR(i, IncidentMatrix, ref Marker, ref Topolog_Array, ref Current_Label);
			}
			textbox.Text = "Топологiчно посортований граф\r\n\r\n";
			foreach (var item in Topolog_Array)
				textbox.Text += String.Format("{0} ", item);
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

		//Отримання сильних компонент зв'язності графу
		public void Get_Graph_Connectivity(TextBox textbox)
		{
			int[] Time_Array = new int[Vershuna];
			int[,] IncidentMatrixCopy = new int[Vershuna,Rebro];
			Array.Copy(IncidentMatrix, IncidentMatrixCopy, Vershuna * Rebro);
			for (int i = Vershuna - 1, temp = 0; i >= 0; i--, temp++)
				Time_Array[i] = temp;
			string asd;
			DFS_Loop(IncidentMatrixCopy, ref Time_Array,out asd);
			Transpose_Graph(Matrix, out IncidentMatrixCopy);
			DFS_Loop(IncidentMatrixCopy, ref Time_Array, out asd);
			textbox.Text = "Сильнi компоненти зв'язностi графу \r\n\r\n" + asd;
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
		private void Transpose_Graph(int[,] Graph, out int[,] IncidentMatrix)
		{
			IncidentMatrix = new int[Vershuna, Rebro];
			for (int i = 0; i < Rebro; i++)
			{
				if (Graph[i, 0] == Graph[i, 1])
					IncidentMatrix[Graph[i, 0] - 1, i] = 2;
				else
				{
					IncidentMatrix[Graph[i, 0] - 1, i] = 1;
					IncidentMatrix[Graph[i, 1] - 1, i] = -1;
				}
			}
		}

		private void Swap(ref int first, ref int second)
		{
			int temp = first;
			first = second;
			second = temp;
		}
		~Graph() { }
	}
}