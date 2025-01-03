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

    private int[][] graph;
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

    private int[][] CreateGraph(int size)
    {
        var graph = new int[size][]; // Cria um vetor de inteiros para representar o grafo
        for (int i = 0; i < size; i++) // Percorre as linhas do grafo
        {
            graph[i] = new int[size]; // Cria um vetor de inteiros para representar a linha do grafo com o tamanho igual ao número de vértices por linha
        }
        return graph;
    }

    private void ResizeGraph(ref int[][] graph, int oldSize, int newSize)
    {
        Array.Resize(ref graph, newSize); // Redimensiona o grafo (o parâmetro ref indica a referência do objeto e equivale ao * no C)
        for (int i = 0; i < newSize; i++) // Percorre as linhas do grafo
        {
            if (i >= oldSize) // Se a linha não existe, então cria uma nova linha
            {
                graph[i] = new int[newSize]; // Cria uma nova linha com o novo tamanho
            }
            else
            {
                Array.Resize(ref graph[i], newSize); // Redimensiona as linhas existentes para o novo tamanho
            }
        }
    }

    private void InsertEdge(ref int[][] graph, ref int size, int u, int v, int weight, bool isDuplicated = false)
    {
        if (u >= size || v >= size) // Se os vértices não existem, então redimensiona o grafo
        {
            int newSize = Math.Max(u, v) + 1; // Calcula o novo tamanho do grafo
            ResizeGraph(ref graph, size, newSize); // Redimensiona o grafo
            size = newSize; // Atualiza o tamanho do grafo
        }
        graph[u][v] = weight; // Adiciona a aresta u -> v
        graph[v][u] = weight; // Adiciona a aresta v -> u
        if (isDuplicated)
        {
            duplicatedEdges.Add(new Edge { U = u, V = v });
        }
    }

    private void DeleteEdge(ref int[][] graph, ref int size, int u, int v)
    {
        if (u >= size || v >= size) return; // Se os vértices não existem, então não faz nada
        graph[u][v] = 0; // Remove a aresta u -> v
        graph[v][u] = 0; // Remove a aresta v -> u

        int newSize = size; // Inicializa o novo tamanho do grafo
        bool hasEdge = false;
        for (int i = newSize - 1; i >= 0; i--) // Percorre as linhas de baixo para cima
        {
            for (int j = newSize - 1; j >= 0; j--) // Percorre as colunas da direita para esquerda
            {
                if (graph[i][j] > 0) // Se encontrar uma aresta
                {
                    hasEdge = true; // Indica que foi encontrada uma aresta
                    break; // Sai do loop de colunas
                }
            }
            if (hasEdge) break; // Sai do loop de linhas
            newSize--; // Caso não tenha encontrado nenhuma aresta na linha, diminui o tamanho do grafo
        }
        if (newSize < size) // Se o tamanho do grafo diminuiu, então redimensiona o grafo
        {
            ResizeGraph(ref graph, size, newSize); // Redimensiona o grafo
            size = newSize; // Atualiza o tamanho do grafo
        }
    }

    private bool EulerianGraph(int[][] graph, int size)
    {
        int[] grau = new int[size]; // Cria um vetor de inteiros para armazenar o grau de cada vértice
        for (int i = 0; i < size; i++) // Percorre as linhas do grafo
        {
            for (int j = 0; j < size; j++) // Percorre as colunas do grafo
            {
                if (graph[i][j] > 0) // Se existe uma aresta entre os vértices i e j
                {
                    grau[i]++; // Incrementa o grau do vértice i
                }
            }
        }
        return grau.All(g => g % 2 == 0); // Retorna verdadeiro se todos os vértices tiverem grau par
    }

    private void Hierholzer(int[][] matriz, int size, int start)
    {
        var tempMatriz = matriz.Select(row => row.ToArray()).ToArray(); // Cria uma cópia da matriz
        var stack = new Stack<int>(); // Cria uma pilha para armazenar os vértices
        var hierholzerPath = new List<int>(); // Cria uma lista para armazenar o caminho de Hierholzer

        stack.Push(start); // Adiciona o vértice inicial na pilha

        while (stack.Count > 0) // Repete enquanto a pilha não estiver vazia
        {
            int vertice = stack.Peek(); // Obtém o vértice do topo da pilha
            int sum = tempMatriz[vertice].Sum(); // Calcula a soma dos pesos das arestas do vértice
            if (sum > 0) // Se a soma dos pesos for maior que zero, então tem aresta
            {
                int destino = Array.FindIndex(tempMatriz[vertice], w => w > 0); // Obtém o primeiro vértice adjacente com peso positivo
                stack.Push(destino); // Adiciona o vértice adjacente na pilha
                Console.WriteLine($"{vertice} > {destino}");

                tempMatriz[vertice][destino] = 0; // Remove a aresta entre os vértices para marcar como visitado
                tempMatriz[destino][vertice] = 0; // Remove a aresta entre os vértices para marcar como visitado
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
        int min = int.MaxValue; // Inicializa a distância mínima com o valor máximo (infinito)
        int minIndex = -1; // Inicializa o índice do vértice com menor distância com -1

        for (int i = 0; i < dist.Length; i++)
        {
            if (!visited[i] && dist[i] <= min) // Se o vértice não foi visitado e a distância é menor ou igual a mínima
            {
                min = dist[i]; // Atualiza a distância mínima
                minIndex = i; // Atualiza o índice do vértice com a distância mínima
            }
        }

        return minIndex; // Retorna o índice do vértice com a menor distância
    }

    private (int, List<int>) Dijkstra(int[][] graph, int size, int orig, int dest, List<int> path)
    {
        // dist armazena um array com as distâncias de cada vértice até o vértice de origem
        int[] dist = Enumerable.Repeat(int.MaxValue, size).ToArray(); // Cria um vetor de inteiros com o valor máximo(infinito) para cada vértice
        bool[] visited = new bool[size]; // Cria um vetor de booleanos para marcar os vértices visitados
        int[] prev = new int[size]; // Cria um vetor de inteiros para armazenar o vértice anterior de cada vértice
        dist[orig] = 0; // A distância do vértice de origem é zero

        for (int count = 0; count < size - 1; count++)
        {
            int u = FindMinIndex(dist, visited); // Encontra o vértice com a menor distância que ainda não foi visitado
            if (u == -1) break; // Se todos os vértices foram visitados, então sai do loop
            visited[u] = true; // Marca o vértice como visitado
            for (int v = 0; v < size; v++)
            {
                // Se o vértice não foi visitado, existe uma aresta entre u e v, a distância do vértice mais próximo(u) for diferente de infinito e a soma da distância do vértice mais próximo(u) com o peso da aresta u -> v for menor que a distância do vértice v(infinito, inicialmente)
                if (!visited[v] && graph[u][v] > 0 && dist[u] != int.MaxValue && dist[u] + graph[u][v] < dist[v])
                {
                    dist[v] = dist[u] + graph[u][v]; // Atualiza a distância do vértice v à origem
                    prev[v] = u; // Atualiza o predecessor do vértice v
                }
            }
        }

        for (int vert = dest; vert != orig; vert = prev[vert]) // Percorre o caminho do destino até a origem
        {
            path.Add(vert); // Adiciona o vértice no caminho
        }
        path.Add(orig); // Adiciona o vértice de origem no caminho
        path.Reverse(); // Inverte a ordem dos vértices para obter o caminho correto

        return (dist[dest], path); // Retorna a distância e o caminho
    }

    private List<int> MinCostPerfectMatching(int[][] graph, int size, int[] imparVertices, int imparCount, int[][] matching, List<int> path)
    {
        for (int i = 0; i < imparCount; i++)
        {
            for (int j = i + 1; j < imparCount; j++)
            {
                int u = imparVertices[i];
                int v = imparVertices[j];
                var (cost, _) = Dijkstra(graph, size, u, v, path);
                matching[u][v] = cost;
                matching[v][u] = cost;
            }
        }
        return path;
    }

    private void FindImparVertices(int[][] graph, int size, out int[] imparVertices, out int imparCount)
    {
        // O parâmetro out transforma em variável global
        int[] graus = new int[size]; // Cria um vetor de inteiros para armazenar o grau de cada vértice
        imparVertices = new int[size]; // Cria um vetor de inteiros para armazenar os vértices com grau ímpar
        imparCount = 0;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (graph[i][j] > 0) // Se existe uma aresta entre os vértices i e j
                {
                    graus[i]++; // Incrementa o grau do vértice i
                }
            }
            if (graus[i] % 2 != 0) // Se o grau do vértice i for ímpar
            {
                imparVertices[imparCount++] = i; // Adiciona o vértice i no vetor de vértices ímpares e incrementa o contador de vértices ímpares
            }
        }
    }


    private void DuplicateEdges(int[][] graph, int size, List<int> path)
    {
        duplicatedEdges.Clear(); // Limpa a lista de arestas duplicadas

        for (int k = 0; k < path.Count - 1; k++)
        {
            // Adiciona a aresta duplicada entre os vértices do caminho mínimo
            InsertEdge(ref graph, ref size, path[k], path[k + 1], graph[path[k]][path[k + 1]], true);
        }
    }

    private void Pcc(int[][] graph, int size, int startVertex)
    {
        if (EulerianGraph(graph, size)) // Se o grafo for euleriano a solução é trivial
        {
            Hierholzer(graph, size, startVertex); // Chama a função Hierholzer (caminho euleriano)
        }
        else // Se o grafo não for euleriano, então é necessário duplicar as arestas do caminho mínimo entre os vértices ímpares
        {
            FindImparVertices(graph, size, out int[] imparVertices, out int imparCount);

            var matching = new int[size][];
            for (int i = 0; i < size; i++)
            {
                matching[i] = new int[size];
            }

            List<int> path = new List<int>();

            path = MinCostPerfectMatching(graph, size, imparVertices, imparCount, matching, path);
            DuplicateEdges(graph, size, path);
            Hierholzer(graph, size, startVertex);
        }
    }

    private void DrawGraph(Graphics g, int[][] graph, int size, Rectangle rect, double zoom, int offsetX, int offsetY)
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
                if (graph[i][j] > 0)
                {
                    g.DrawLine(Pens.Black, positions[i], positions[j]); // Desenha a aresta
                    var midPoint = new Point((positions[i].X + positions[j].X) / 2, (positions[i].Y + positions[j].Y) / 2); // Calcula o meio da aresta
                    g.DrawString(graph[i][j].ToString(), this.Font, Brushes.Black, midPoint); // Insere o peso da aresta
                }
            }
        }

        using (var pen = new Pen(Color.Green, 2))
        {
            foreach (var edge in duplicatedEdges)
            {
                var midPoint = new Point((positions[edge.U].X + positions[edge.V].X) / 2, (positions[edge.U].Y + positions[edge.V].Y) / 2);
                var controlPoint = new Point(midPoint.X + 20, midPoint.Y - 20); // Ponto de controle para a parábola
                var graphpath = new GraphicsPath();
                graphpath.AddBezier(positions[edge.U], controlPoint, controlPoint, positions[edge.V]);
                g.DrawPath(pen, graphpath);
            }
        }

        for (int i = 0; i < size; i++)
        {
            if (graph[i].Any(w => w > 0))
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