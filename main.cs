using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

public class ChinesePostmanProblem : Form
{
    private const int ID_ADD_EDGE = 1;
    private const int ID_DELETE_EDGE = 2;
    private const int ID_EXECUTE_PCC = 3;

    private List<int>[][] graph;
    private List<Edge> duplicatedEdges = new List<Edge>();
    private int size = 1;
    private double zoom = 1.0;
    private int offsetX = 0, offsetY = 0;
    private Point lastMousePos;
    private bool isDragging = false;
    private TextBox inputU, inputV, inputWeight, inputStart;
    private Button actionButton;
    private Point[] positions;
    private int? draggingVertex = null;

    public ChinesePostmanProblem()
    {
        this.Text = "Problema do Carteiro Chinês";
        this.Size = new Size(800, 400);
        this.graph = CreateGraph(size);
        this.positions = new Point[size];

        var addButton = new Button { Text = "Add Edge", Location = new Point(10, 10), Size = new Size(100, 30) };
        addButton.Click += (sender, e) => PromptForInput(true, false);
        this.Controls.Add(addButton);

        var deleteButton = new Button { Text = "Delete Edge", Location = new Point(120, 10), Size = new Size(100, 30) };
        deleteButton.Click += (sender, e) => PromptForInput(false, false);
        this.Controls.Add(deleteButton);

        var executeButton = new Button { Text = "Execute PCC", Location = new Point(230, 10), Size = new Size(100, 30) };
        executeButton.Click += (sender, e) => PromptForInput(false, true);
        this.Controls.Add(executeButton);

        this.MouseWheel += (sender, e) =>
        {
            zoom += e.Delta / 1200.0;
            if (zoom < 0.1) zoom = 0.1;
            this.Invalidate();
        };

        this.MouseDown += (sender, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                lastMousePos = e.Location;
                isDragging = true;
                for (int i = 0; i < size; i++)
                {
                    if (positions[i] != Point.Empty && Math.Abs(e.X - positions[i].X) < 10 && Math.Abs(e.Y - positions[i].Y) < 10)
                    {
                        draggingVertex = i;
                        break;
                    }
                }
            }
        };

        this.MouseUp += (sender, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
                draggingVertex = null;
            }
        };

        this.MouseMove += (sender, e) =>
        {
            if (isDragging)
            {
                if (draggingVertex.HasValue)
                {
                    positions[draggingVertex.Value] = e.Location;
                }
                else
                {
                    offsetX += e.X - lastMousePos.X;
                    offsetY += e.Y - lastMousePos.Y;
                    lastMousePos = e.Location;
                }
                    this.Invalidate();
            }
        };
    }

    private List<int>[][] CreateGraph(int size)
    {
        var graph = new List<int>[size][];
        for (int i = 0; i < size; i++)
        {
            graph[i] = new List<int>[size];
            for (int j = 0; j < size; j++)
            {
                graph[i][j] = new List<int>();
            }
        }
        return graph;
    }

    private void ResizeGraph(ref List<int>[][] graph, int oldSize, int newSize)
    {
        Array.Resize(ref graph, newSize);
        for (int i = 0; i < newSize; i++)
        {
            if (i >= oldSize)
            {
                graph[i] = new List<int>[newSize];
                for (int j = 0; j < newSize; j++)
                {
                    graph[i][j] = new List<int>();
                }
            }
            else
            {
                Array.Resize(ref graph[i], newSize);
                for (int j = oldSize; j < newSize; j++)
                {
                    graph[i][j] = new List<int>();
                }
            }
        }
    }

    private void InsertEdge(ref List<int>[][] graph, ref int size, int u, int v, int weight, bool isDuplicated = false)
    {
        if (u >= size || v >= size)
        {
            int newSize = Math.Max(u, v) + 1;
            ResizeGraph(ref graph, size, newSize);
            size = newSize;
        }
        graph[u][v].Add(weight);
        graph[v][u].Add(weight);
        if (isDuplicated)
        {
            duplicatedEdges.Add(new Edge { U = u, V = v });
        }
    }

    private void DeleteEdge(ref List<int>[][] graph, ref int size, int u, int v)
    {
        if (u >= size || v >= size) return;
        if (graph[u][v].Count > 0) graph[u][v].RemoveAt(graph[u][v].Count - 1);
        if (graph[v][u].Count > 0) graph[v][u].RemoveAt(graph[v][u].Count - 1);

        int newSize = size;
        bool hasEdge = false;
        for (int i = newSize - 1; i >= 0; i--)
        {
            for (int j = newSize - 1; j >= 0; j--)
            {
                if (graph[i][j].Count > 0)
                {
                    hasEdge = true;
                    break;
                }
            }
            if (hasEdge) break;
            newSize--;
        }
        if (newSize < size)
        {
            ResizeGraph(ref graph, size, newSize);
            size = newSize;
        }
    }

    private bool EulerianGraph(List<int>[][] graph, int size)
    {
        int[] grau = new int[size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                grau[i] += graph[i][j].Count;
            }
        }
        return grau.All(g => g % 2 == 0);
    }

    private void Hierholzer(List<int>[][] matriz, int size, int start)
    {
        var tempMatriz = matriz.Select(row => row.Select(col => col.ToList()).ToArray()).ToArray();
        var stack = new Stack<int>();
        var hierholzerPath = new List<int>();

        stack.Push(start);

        while (stack.Count > 0)
        {
            int vertice = stack.Peek();
            if (tempMatriz[vertice].Any(col => col.Count > 0))
            {
                int destino = Array.FindIndex(tempMatriz[vertice], col => col.Count > 0);
                stack.Push(destino);
                Console.WriteLine($"{vertice} > {destino}");

                tempMatriz[vertice][destino].RemoveAt(0);
                tempMatriz[destino][vertice].RemoveAt(0);
            }
            else
            {
                stack.Pop();
                hierholzerPath.Add(vertice);
                Console.WriteLine($"POP {vertice}");
            }
        }

        Console.WriteLine("Hierholzer Path: " + string.Join(" ", hierholzerPath));
    }

    private int FindMinIndex(int[] dist, bool[] visited)
    {
        int min = int.MaxValue;
        int minIndex = -1;

        for (int i = 0; i < dist.Length; i++)
        {
            if (!visited[i] && dist[i] <= min)
            {
                min = dist[i];
                minIndex = i;
            }
        }

        return minIndex;
    }

    private (int, List<int>) Dijkstra(List<int>[][] graph, int size, int orig, int dest, List<int> path)
    {
        int[] dist = Enumerable.Repeat(int.MaxValue, size).ToArray();
        bool[] visited = new bool[size];
        int[] prev = new int[size];
        dist[orig] = 0;

        for (int count = 0; count < size - 1; count++)
        {
            int u = FindMinIndex(dist, visited);
            if (u == -1) break;
            visited[u] = true;
            for (int v = 0; v < size; v++)
            {
                if (!visited[v] && graph[u][v].Count > 0 && dist[u] != int.MaxValue && dist[u] + graph[u][v].Min() < dist[v])
                {
                    dist[v] = dist[u] + graph[u][v].Min();
                    prev[v] = u;
                }
            }
        }

        for (int vert = dest; vert != orig; vert = prev[vert])
        {
            path.Add(vert);
        }
        path.Add(orig);
        path.Reverse();

        return (dist[dest], path);
    }

    private List<int> MinCostPerfectMatching(List<int>[][] graph, int size, int[] imparVertices, int imparCount, List<int>[][] matching, List<int> path)
    {
        for (int i = 0; i < imparCount; i++)
        {
            for (int j = i + 1; j < imparCount; j++)
            {
                int u = imparVertices[i];
                int v = imparVertices[j];
                var (cost, _) = Dijkstra(graph, size, u, v, path);
                matching[u][v].Add(cost);
                matching[v][u].Add(cost);
            }
        }
        return path;
    }

    private void FindImparVertices(List<int>[][] graph, int size, out int[] imparVertices, out int imparCount)
    {
        int[] graus = new int[size];
        imparVertices = new int[size];
        imparCount = 0;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (graph[i][j].Count > 0)
                {
                    graus[i]++;
                }
            }
            if (graus[i] % 2 != 0)
            {
                imparVertices[imparCount++] = i;
            }
        }
    }


    private void DuplicateEdges(List<int>[][] graph, int size, List<int> path)
    {
        duplicatedEdges.Clear();

        for (int k = 0; k < path.Count - 1; k++)
        {
            InsertEdge(ref graph, ref size, path[k], path[k + 1], graph[path[k]][path[k + 1]].Min(), true);
        }
    }

    private void Pcc(List<int>[][] graph, int size, int startVertex)
    {
        if (EulerianGraph(graph, size))
        {
            Hierholzer(graph, size, startVertex);
        }
        else
        {
            FindImparVertices(graph, size, out int[] imparVertices, out int imparCount);

            var matching = new List<int>[size][];
            for (int i = 0; i < size; i++)
            {
                matching[i] = new List<int>[size];
                for (int j = 0; j < size; j++)
                {
                    matching[i][j] = new List<int>();
                }
            }

            List<int> path = new List<int>();

            path = MinCostPerfectMatching(graph, size, imparVertices, imparCount, matching, path);
            DuplicateEdges(graph, size, path);
            Hierholzer(graph, size, startVertex);
        }
    }

    private void DrawGraph(Graphics g, List<int>[][] graph, int size, Rectangle rect, double zoom, int offsetX, int offsetY)
    {
        int radius = 20;
        int centerX = rect.Width / 2 + offsetX;
        int centerY = rect.Height / 2 + offsetY;
        if (positions == null || positions.Length != size)
        {
            positions = new Point[size];
        }
        int graphRadius = (int)((Math.Min(centerX, centerY) - 50) * zoom);

        for (int i = 0; i < size; i++)
        {
            if (positions[i] == Point.Empty)
            {
                positions[i] = new Point(
                    centerX + (int)(graphRadius * Math.Cos(2 * Math.PI * i / size)),
                    centerY + (int)(graphRadius * Math.Sin(2 * Math.PI * i / size))
                );
            }
        }

        for (int i = 0; i < size; i++)
        {
            for (int j = i + 1; j < size; j++)
            {
                if (graph[i][j].Count > 0)
                {
                    foreach (var weight in graph[i][j])
                    {
                        g.DrawLine(Pens.Black, positions[i], positions[j]);
                        var midPoint = new Point((positions[i].X + positions[j].X) / 2, (positions[i].Y + positions[j].Y) / 2);
                        g.DrawString(weight.ToString(), this.Font, Brushes.Black, midPoint);
                    }
                }
            }
        }

        using (var pen = new Pen(Color.Green, 2))
        {
            foreach (var edge in duplicatedEdges)
            {
                var midPoint = new Point((positions[edge.U].X + positions[edge.V].X) / 2, (positions[edge.U].Y + positions[edge.V].Y) / 2);
                var controlPoint = new Point(midPoint.X + 20, midPoint.Y - 20);
                var graphpath = new GraphicsPath();
                graphpath.AddBezier(positions[edge.U], controlPoint, controlPoint, positions[edge.V]);
                g.DrawPath(pen, graphpath);
            }
        }

        for (int i = 0; i < size; i++)
        {
            if (graph[i].Any(col => col.Count > 0))
            {
                g.FillEllipse(Brushes.White, positions[i].X - radius, positions[i].Y - radius, radius * 2, radius * 2);
                g.DrawEllipse(Pens.Black, positions[i].X - radius, positions[i].Y - radius, radius * 2, radius * 2);
                g.DrawString(i.ToString(), this.Font, Brushes.Black, positions[i].X - 5, positions[i].Y - 5);
            }
        }
    }

    private void HidePrompt()
    {
        if (inputU != null) inputU.Visible = false;
        if (inputV != null) inputV.Visible = false;
        if (inputWeight != null) inputWeight.Visible = false;
        if (inputStart != null) inputStart.Visible = false;
        if (actionButton != null) actionButton.Visible = false;
    }

    private void PromptForInput(bool isAdd, bool isExecute)
    {
        HidePrompt();

        if (isExecute)
        {
            if (inputStart == null)
            {
                inputStart = new TextBox { Location = new Point(10, 50), Size = new Size(50, 20) };
                actionButton = new Button { Text = "Execute", Location = new Point(70, 50), Size = new Size(70, 20) };
                actionButton.Click += (sender, e) =>
                {
                    int startVertex = int.Parse(inputStart.Text);
                    if (inputStart.Text == "" || startVertex >= size)
                    {
                        MessageBox.Show("O vértice não existe", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    Pcc(graph, size, startVertex);
                    HidePrompt();
                    this.Invalidate();
                };
                this.Controls.Add(inputStart);
                this.Controls.Add(actionButton);
            }
            else
            {
                inputStart.Visible = true;
                actionButton.Visible = true;
            }
        }
        else
        {
            if (inputU == null)
            {
                inputU = new TextBox { Location = new Point(10, 50), Size = new Size(50, 20) };
                inputV = new TextBox { Location = new Point(70, 50), Size = new Size(50, 20) };
                if (isAdd)
                {
                    inputWeight = new TextBox { Location = new Point(130, 50), Size = new Size(50, 20) };
                }
                actionButton = new Button { Text = isAdd ? "Add" : "Delete", Location = new Point(190, 50), Size = new Size(50, 20) };
                actionButton.Click += (sender, e) =>
                {
                    int u = int.Parse(inputU.Text);
                    int v = int.Parse(inputV.Text);
                    if (isAdd)
                    {
                        int weight = int.Parse(inputWeight.Text);
                        InsertEdge(ref graph, ref size, u, v, weight);
                    }
                    else
                    {
                        DeleteEdge(ref graph, ref size, u, v);
                    }
                    HidePrompt();
                    this.Invalidate(); // Redesenha o formulário
                };
                this.Controls.Add(inputU);
                this.Controls.Add(inputV);
                if (isAdd) this.Controls.Add(inputWeight);
                this.Controls.Add(actionButton);
            }
            else
            {
                inputU.Visible = true;
                inputV.Visible = true;
                if (isAdd) inputWeight.Visible = true;
                actionButton.Text = isAdd ? "Add" : "Delete";
                actionButton.Visible = true;
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        DrawGraph(e.Graphics, graph, size, this.ClientRectangle, zoom, offsetX, offsetY);
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new ChinesePostmanProblem());
    }

    private struct Edge
    {
        public int U { get; set; }
        public int V { get; set; }
    }
}