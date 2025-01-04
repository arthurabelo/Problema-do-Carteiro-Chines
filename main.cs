using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

public class ChinesePostmanProblem : Form
{
    private List<int>[][] graph; // O grafo é uma matriz de listas de inteiros (lista de adjacência com lista que possibilita arestas paralelas)
    private int size = 1; // Tamanho do grafo
    private double zoom = 1.0; // Zoom
    private int offsetX = 0, offsetY = 0; // Deslocamento
    private Point lastMousePos; // Posição do mouse
    private bool isDragging = false; // Indica se o mouse está sendo arrastado
    private TextBox inputU, inputV, inputWeight, inputStart; // Caixas de texto para entrada de dados 
    private Button actionButton; // Botão de ação
    private Point[] positions; // Posições dos vértices
    private int? draggingVertex = null; // int? é um tipo de dado que aceita valores nulos
    private Rectangle drawingArea; // Área de desenho
    private TextBox pathOutput; // Caixa de texto para exibir o caminho

    public ChinesePostmanProblem()
    {
        this.Text = "Problema do Carteiro Chinês";
        this.Size = new Size(800, 400);
        this.graph = CreateGraph(size); // Inicializa o grafo
        this.positions = new Point[size]; // Inicializa as posições dos vértices para desenho

        var addButton = new Button { Text = "Adicionar Aresta", Location = new Point(10, 10), Size = new Size(100, 30) };
        addButton.Click += (sender, e) => PromptForInput(true, false);
        this.Controls.Add(addButton);

        var deleteButton = new Button { Text = "Deletar Aresta", Location = new Point(120, 10), Size = new Size(100, 30) };
        deleteButton.Click += (sender, e) => PromptForInput(false, false);
        this.Controls.Add(deleteButton);

        var executeButton = new Button { Text = "Executar PCC", Location = new Point(230, 10), Size = new Size(100, 30) };
        executeButton.Click += (sender, e) => PromptForInput(false, true);
        this.Controls.Add(executeButton);

        pathOutput = new TextBox { Location = new Point(340, 10), Size = new Size(440, 30), ReadOnly = true }; // Inicializa a caixa de texto do caminho
        this.Controls.Add(pathOutput); // Adiciona a caixa de texto do caminho

        drawingArea = new Rectangle(0, 40, this.ClientSize.Width, this.ClientSize.Height - 40); // Define a área de desenho

        actionButton = new Button { Location = new Point(190, 50), Size = new Size(100, 20) }; // Inicializa o botão de ação
        this.Controls.Add(actionButton); // Adiciona o botão de ação
        actionButton.Visible = false; // Esconde o botão de ação inicialmente

        // Evento de rolagem do mouse
        this.MouseWheel += (sender, e) =>
        {
            zoom += e.Delta / 1200.0;
            if (zoom < 0.1) zoom = 0.1;
            this.Invalidate(drawingArea); // Redesenha a área de desenho
        };

        // Evento de clique do mouse
        this.MouseDown += (sender, e) =>
        {
            if (e.Button == MouseButtons.Left) // Se o botão esquerdo do mouse for clicado
            {
                lastMousePos = e.Location; // Salva a posição do mouse
                isDragging = true; // Indica que o mouse está sendo clicado e arrastado
                for (int i = 0; i < size; i++) // Percorre os vértices do grafo
                {
                    // Se a posição do mouse estiver próxima de um vértice, então arrasta o vértice
                    if (positions[i] != Point.Empty && Math.Abs(e.X - positions[i].X) < (20 * zoom) && Math.Abs(e.Y - positions[i].Y) < (20 * zoom)) 
                    {
                        draggingVertex = i; // Salva o vértice que está sendo arrastado
                        break; // Interrompe o laço
                    }
                }
            }
        };

        // Evento de soltura do mouse
        this.MouseUp += (sender, e) =>
        {
            if (e.Button == MouseButtons.Left) // Se o botão esquerdo do mouse for solto
            {
                isDragging = false; // Indica que o mouse não está sendo clicado e arrastado
                draggingVertex = null; // Indica que não há vértice sendo arrastado
            }
        };

        // Evento de movimento do mouse
        this.MouseMove += (sender, e) =>
        {
            if (isDragging) // Se o mouse estiver sendo clicado e arrastado
            {
                if (draggingVertex.HasValue) // Se houver um vértice sendo arrastado
                {
                    // Calcula a nova posição do vértice
                    int newX = e.Location.X;
                    int newY = e.Location.Y;

                    // Impede que o vértice saia da tela
                    if (newX < drawingArea.Left) newX = drawingArea.Left;
                    if (newX > drawingArea.Right) newX = drawingArea.Right;
                    if (newY < drawingArea.Top) newY = drawingArea.Top;
                    if (newY > drawingArea.Bottom) newY = drawingArea.Bottom;

                    positions[draggingVertex.Value] = new Point(newX, newY); // Atualiza a posição do vértice
                }
                else
                {
                    offsetX += e.X - lastMousePos.X; // Atualiza o deslocamento horizontal
                    offsetY += e.Y - lastMousePos.Y; // Atualiza o deslocamento vertical
                    lastMousePos = e.Location; // Atualiza a posição do mouse
                }
                this.Invalidate(drawingArea); // Redesenha a área de desenho
            }
        };

        // Evento de redimensionamento da janela
        this.Resize += (sender, e) =>
        {
            drawingArea = new Rectangle(0, 40, this.ClientSize.Width, this.ClientSize.Height - 40); // Atualiza a área de desenho ao redimensionar
            this.Invalidate(drawingArea); // Redesenha a área de desenho
        };
    }

    private List<int>[][] CreateGraph(int size) // Cria um grafo
    {
        var graph = new List<int>[size][]; // Cria um array de listas de inteiros para representar o grafo
        for (int i = 0; i < size; i++) // Percorre as linhas do grafo
        {
            graph[i] = new List<int>[size]; // Cria um array de listas de inteiros para representar as colunas do grafo
            for (int j = 0; j < size; j++)
            {
                graph[i][j] = new List<int>(); // Cria uma lista de inteiros para representar as arestas entre os vértices i e j
            }
        }
        return graph;
    }

    private void ResizeGraph(ref List<int>[][] graph, int oldSize, int newSize)
    {
        // O parâmetro ref indica a referência do objeto e equivale ao * (ponteiro) em C
        Array.Resize(ref graph, newSize); // Redimensiona o grafo
        for (int i = 0; i < newSize; i++) // Percorre as linhas do grafo
        {
            if (i >= oldSize) // Se a linha não existir, então cria um novo array de listas de inteiros
            {
                graph[i] = new List<int>[newSize]; // Cria um array de listas de inteiros para representar as colunas do grafo
                for (int j = 0; j < newSize; j++) // Percorre as colunas do grafo
                {
                    graph[i][j] = new List<int>(); // Cria uma lista de inteiros para representar as arestas entre os vértices i e j
                }
            }
            else
            {
                Array.Resize(ref graph[i], newSize); // Redimensiona o array de listas de inteiros
                for (int j = oldSize; j < newSize; j++) // Percorre as colunas do grafo
                {
                    graph[i][j] = new List<int>(); // Cria uma lista de inteiros para representar as arestas entre os vértices i e j
                }
            }
        }
    }

    private void InsertEdge(ref List<int>[][] graph, ref int size, int u, int v, int weight, bool isDuplicated = false)
    {
        if (u >= size || v >= size) // Se o vértice u ou v não existir, então redimensiona o grafo
        {
            int newSize = Math.Max(u, v) + 1; // Calcula o novo tamanho do grafo
            ResizeGraph(ref graph, size, newSize); // Redimensiona o grafo
            size = newSize; // Atualiza o tamanho do grafo
        }
        graph[u][v].Add(weight); // Adiciona a aresta entre os vértices u e v
        graph[v][u].Add(weight); // Adiciona a aresta entre os vértices v e u
    }

    private void DeleteEdge(ref List<int>[][] graph, ref int size, int u, int v)
    {
        if (u >= size || v >= size) return; // Se o vértice u ou v não existir, então retorna e não faz nada
        if (graph[u][v].Count > 0) graph[u][v].RemoveAt(graph[u][v].Count - 1); // Remove a aresta entre os vértices u e v
        if (graph[v][u].Count > 0) graph[v][u].RemoveAt(graph[v][u].Count - 1); // Remove a aresta entre os vértices v e u

        int newSize = size; // Inicializa o novo tamanho do grafo
        bool hasEdge = false; // Indica se há aresta no grafo
        for (int i = newSize - 1; i >= 0; i--) // Percorre as linhas do grafo
        {
            for (int j = newSize - 1; j >= 0; j--) // Percorre as colunas do grafo
            {
                if (graph[i][j].Count > 0) // Se houver aresta entre os vértices i e j
                {
                    hasEdge = true; // Indica que há aresta no grafo entre os vértices i e j
                    break; // Interrompe o laço
                }
            }
            if (hasEdge) break; // Interrompe o laço se houver aresta no grafo e impede o decremento do tamanho do grafo
            newSize--; // Decrementa o tamanho do grafo
        }
        if (newSize < size) // Se o novo tamanho do grafo for menor que o tamanho atual
        {
            ResizeGraph(ref graph, size, newSize); // Redimensiona o grafo
            size = newSize; // Atualiza o tamanho do grafo
        }
    }

    private bool EulerianGraph(List<int>[][] graph, int size)
    {
        int[] grau = new int[size]; // Cria um array de inteiros para armazenar o grau dos vértices
        for (int i = 0; i < size; i++) // Percorre as linhas do grafo
        {
            for (int j = 0; j < size; j++) // Percorre as colunas do grafo
            {
                grau[i] += graph[i][j].Count; // Calcula o grau do vértice i
            }
        }
        return grau.All(g => g % 2 == 0); // Retorna verdadeiro se todos os vértices tiverem grau par
    }

    // Executa o algoritmo de Hierholzer para encontrar o caminho de Euler e resolver o Problema do Carteiro Chinês
    private void Hierholzer(List<int>[][] matriz, int size, int start)
    {
        var tempMatriz = matriz.Select(row => row.Select(col => col.ToList()).ToArray()).ToArray(); // Cria uma cópia da matriz
        var stack = new Stack<int>(); // Cria uma pilha
        var hierholzerPath = new List<int>(); // Cria uma lista para armazenar o caminho de Hierholzer

        stack.Push(start); // Empilha o vértice inicial

        while (stack.Count > 0) // Repete enquanto houver vértices na pilha
        {
            int vertice = stack.Peek(); // Obtém o vértice do topo da pilha
            if (tempMatriz[vertice].Any(col => col.Count > 0)) // Se houver arestas incidentes no vértice
            {
                // Encontra o próximo vértice adjacente, que nesse caso é o primeiro adjacente, mas poderia ser qualquer um
                int destino = Array.FindIndex(tempMatriz[vertice], col => col.Count > 0);
                stack.Push(destino); // Empilha o vértice adjacente
                Console.WriteLine($"{vertice} > {destino}"); // Exibe a aresta no console

                tempMatriz[vertice][destino].RemoveAt(0); // Remove a aresta entre os vértices vertice e destino da matriz temporária
                tempMatriz[destino][vertice].RemoveAt(0); // Remove a aresta entre os vértices destino e vertice da matriz temporária
            }
            else
            {
                // Se não houver arestas incidentes no vértice, então desempilha o vértice e adiciona na lista do caminho de Hierholzer
                stack.Pop(); // Desempilha o vértice
                hierholzerPath.Add(vertice); // Adiciona o vértice na lista do caminho de Hierholzer
                Console.WriteLine($"POP {vertice}"); // Exibe a remoção do vértice no console
            }
        }

        Console.WriteLine("Hierholzer Path: " + string.Join(" ", hierholzerPath)); // Exibe o caminho de Hierholzer no console
        pathOutput.Text = string.Join(" -> ", hierholzerPath); // Exibe o caminho de Hierholzer na caixa de texto
    }

    private int FindMinIndex(int[] dist, bool[] visited)
    {
        int min = int.MaxValue; // Define o valor mínimo como o maior valor possível
        int minIndex = -1; // Inicializa o index do menor vértice com -1 para indicar que ainda não foi definido

        for (int i = 0; i < dist.Length; i++) // Percorre os vértices
        {
            if (!visited[i] && dist[i] <= min) // Se não foi visitado e a distância for menor do a menor atual
            {
                min = dist[i]; // Atualiza a distância mínima
                minIndex = i; // Atualiza o index do vértice com a menor distância
            }
        }

        return minIndex; // Retorna o índice do vértice com a menor distância
    }

    private (int, List<int>) Dijkstra(List<int>[][] graph, int size, int orig, int dest, List<int> path)
    {
        int[] dist = Enumerable.Repeat(int.MaxValue, size).ToArray(); // Cria um array de inteiros com o valor máximo do tamanho do grafo
        bool[] visited = new bool[size]; // Cria um array de booleanos com o tamanho do grafo
        int[] prev = new int[size]; // Cria um array de inteiros com o tamanho do grafo que armazenará o predecessor de cada vértice
        dist[orig] = 0; // Define a distância do vértice de origem como zero

        for (int count = 0; count < size - 1; count++) // Repete para cada vértice
        {
            int u = FindMinIndex(dist, visited); // Encontra o vértice com a menor distância
            if (u == -1) break; // Se não houver vértice ou todos já tiverem sido visitados, então interrompe o laço
            visited[u] = true; // Marca o vértice como visitado
            for (int v = 0; v < size; v++) // Percorre os vértices
            {
                /*
                Se o vértice não foi visitado, houver aresta entre os vértices u e v, a distância do vértice com a menor distância for diferente 
                de infinito e a soma da distância do vértice com a menor distância com a aresta entre u -> v for menor do que a distância do vértice atual v
                 */
                if (!visited[v] && graph[u][v].Count > 0 && dist[u] != int.MaxValue && dist[u] + graph[u][v].Min() < dist[v])
                {
                    dist[v] = dist[u] + graph[u][v].Min(); // Atualiza a distância do vértice atual v à origem
                    prev[v] = u; // Atualiza o predecessor do vértice atual v
                }
            }
        }

        // Reconstrói o caminho do vértice de destino até o vértice de origem
        path.Clear(); // Limpa o caminho
        for (int vert = dest; vert != orig; vert = prev[vert]) // Percorre o caminho do vértice de destino até o vértice de origem
        {
            path.Add(vert); // Adiciona o vértice no caminho
        }
        path.Add(orig); // Adiciona o vértice de origem no caminho
        path.Reverse(); // Inverte o caminho para obter o caminho correto

        return (dist[dest], path); // Retorna a distância mínima e o caminho mínimo
    }

    // Calcula o matching mínimo perfeito
    private void MinCostPerfectMatching(List<int>[][] graph, int size, int[] imparVertices, int imparCount)
    {
        var auxGraph = new int[imparCount, imparCount]; // Cria uma matriz de inteiros para armazenar o grafo auxiliar
        var paths = new List<int>[imparCount, imparCount]; // Cria uma matriz de listas de inteiros para armazenar os caminhos mínimos

        for (int i = 0; i < imparCount; i++)
        {
            for (int j = 0; j < imparCount; j++)
            {
                if (i != j) // Se os vértices forem diferentes
                {
                    var (cost, p) = Dijkstra(graph, size, imparVertices[i], imparVertices[j], new List<int>()); // Calcula o caminho mínimo entre os vértices ímpares
                    auxGraph[i, j] = cost;  // Define o custo do caminho mínimo
                    paths[i, j] = p;    // Define o caminho mínimo
                }
                else // Se os vértices forem iguais
                {
                    auxGraph[i, j] = int.MaxValue; // Define o custo como infinito
                    paths[i, j] = new List<int>(); // Cria uma lista vazia
                }
            }
        }

        var minCostPairs = new List<(int, int)>(); // Lista para armazenar os pares de vértices com menor custo
        var minTotalCost = int.MaxValue; // Inicializa o custo total mínimo como infinito

        // Função recursiva para encontrar todas as combinações de emparelhamento
        void FindPairs(List<int> remaining, List<(int, int)> currentPairs, int currentCost)
        {
            if (remaining.Count == 0) // Se não houver mais vértices restantes
            {
                if (currentCost < minTotalCost) // Se o custo atual for menor que o custo total mínimo
                {
                    minTotalCost = currentCost; // Atualiza o custo total mínimo
                    minCostPairs = new List<(int, int)>(currentPairs); // Atualiza os pares de vértices com menor custo
                }
                return;
            }

            for (int i = 1; i < remaining.Count; i++)
            {
                var newPairs = new List<(int, int)>(currentPairs) { (remaining[0], remaining[i]) }; // Adiciona um novo par de vértices
                var newRemaining = remaining.Where((_, index) => index != 0 && index != i).ToList(); // Remove os vértices emparelhados da lista de vértices restantes
                var newCost = currentCost + auxGraph[remaining[0], remaining[i]]; // Calcula o novo custo

                FindPairs(newRemaining, newPairs, newCost); // Chama a função recursiva com os novos pares e vértices restantes
            }
        }

        FindPairs(Enumerable.Range(0, imparCount).ToList(), new List<(int, int)>(), 0); // Inicia a busca recursiva

        foreach (var (x, y) in minCostPairs) // Para cada par de vértices com menor custo
        {
            DuplicateEdges(graph, size, paths[x, y]); // Duplica as arestas do caminho mínimo entre os vértices x e y
        }
    }

    // Encontra os vértices ímpares que tornam o gráfico não-euleriano
    private void FindImparVertices(List<int>[][] graph, int size, out int[] imparVertices, out int imparCount)
    {
        int[] graus = new int[size]; // Cria um array de inteiros para armazenar o grau dos vértices
        imparVertices = new int[size]; // Cria um array de inteiros para armazenar os vértices ímpares
        imparCount = 0; // Inicializa o contador de vértices ímpares

        for (int i = 0; i < size; i++) // Percorre os vértices(linhas da matriz)
        {
            for (int j = 0; j < size; j++) // Percorre os vértices(colunas da matriz)
            {
                if (graph[i][j].Count > 0) // Se houver aresta entre os vértices i e j
                {
                    graus[i]++; // Incrementa o grau do vértice i
                }
            }
            if (graus[i] % 2 != 0) // Se o grau do vértice i for ímpar
            {
                imparVertices[imparCount++] = i; // Adiciona o vértice i no array de vértices ímpares e incrementa o contador de vértices ímpares
            }
        }
    }

    // Duplica as arestas
    private void DuplicateEdges(List<int>[][] graph, int size, List<int> path)
    {
        for (int k = 0; k < path.Count - 1; k++) // Percorre o caminho mínimo
        {
            InsertEdge(ref graph, ref size, path[k], path[k + 1], graph[path[k]][path[k + 1]].Min(), true); // Adiciona a aresta duplicada entre os vértices k e k + 1
        }
    }

    // Resolve o Problema do Carteiro Chinês
    private void ResolveCPP(List<int>[][] graph, int size, int startVertex)
    {
        if (EulerianGraph(graph, size)) // Se o grafo for euleriano
        {
            // Solução trivial
            Hierholzer(graph, size, startVertex);
        }
        else
        {
            // Solução não trivial
            FindImparVertices(graph, size, out int[] imparVertices, out int imparCount);
            MinCostPerfectMatching(graph, size, imparVertices, imparCount);
            Hierholzer(graph, size, startVertex);
        }
    }

    private void HidePrompt()
    {
        // Esconde as caixas de texto e o botão de ação
        if (inputU != null) 
        {
            inputU.Visible = false; // Se a caixa de texto do vértice u existir, então esconde
            inputU.Text = ""; // Limpa o texto da caixa de texto do vértice u
        }
        if (inputV != null) 
        {
            inputV.Visible = false; // Se a caixa de texto do vértice v existir, então esconde
            inputV.Text = ""; // Limpa o texto da caixa de texto do vértice v
        }
        if (inputWeight != null) 
        {
            inputWeight.Visible = false; // Se a caixa de texto do peso da aresta existir, então esconde
            inputWeight.Text = ""; // Limpa o texto da caixa de texto do peso da aresta
        }
        if (inputStart != null) 
        {
            inputStart.Visible = false; // Se a caixa de texto do vértice de início existir, então esconde
            inputStart.Text = ""; // Limpa o texto da caixa de texto do vértice de início
        }
        if (actionButton.Visible == true) actionButton.Visible = false; // Esconde o botão de ação
    }
    private void ActionButton_Click(object sender, EventArgs e)
    {
        if (actionButton.Text == "EXECUTE")
        {
            if (!int.TryParse(inputStart.Text, out int startVertex) || startVertex >= size) // Verifica se a entrada é válida
            {
                MessageBox.Show("O vértice não existe ou a entrada é inválida", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); // Exibe uma mensagem de erro
                return;
            }
            ResolveCPP(graph, size, startVertex); // Executa o Problema do Carteiro Chinês
        }
        else
        {
            if (!int.TryParse(inputU.Text, out int u) || !int.TryParse(inputV.Text, out int v)) // Verifica se as entradas são válidas
            {
                MessageBox.Show("Os vértices são inválidos", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); // Exibe uma mensagem de erro
                return;
            }
            if (actionButton.Text == "ADD") // Se for para adicionar aresta
            {
                if (!int.TryParse(inputWeight.Text, out int weight)) // Verifica se a entrada do peso é válida
                {
                    MessageBox.Show("O peso é inválido", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error); // Exibe uma mensagem de erro
                    return;
                }
                InsertEdge(ref graph, ref size, u, v, weight); // Adiciona a aresta entre os vértices u e v
            }
            else
            {
                DeleteEdge(ref graph, ref size, u, v); // Remove a aresta entre os vértices u e v
            }
        }
        HidePrompt(); // Esconde as caixas de texto
        this.Invalidate(drawingArea); // Redesenha a área de desenho do grafo
    }

    private void PromptForInput(bool isAdd, bool isExecute)
    {
        HidePrompt(); // Esconde as caixas de texto

        // Exibe as caixas de texto e o botão de ação
        if (isExecute) // Se for para executar o PCC
        {
            if (inputStart == null) // Se a caixa de texto do vértice de início não existir
            {
                inputStart = new TextBox { Location = new Point(10, 50), Size = new Size(50, 20) }; // Cria a caixa de texto do vértice de início
                this.Controls.Add(inputStart); // Adiciona a caixa de texto do vértice de início
            }
            inputStart.Visible = true; // Exibe a caixa de texto do vértice de início

            actionButton.Text = "EXECUTE"; // Define o texto do botão de ação
            actionButton.Click -= ActionButton_Click; // Remove o evento de clique existente
            actionButton.Click += ActionButton_Click; // Adiciona o novo evento de clique
            actionButton.Visible = true; // Exibe o botão de ação
        }
        else
        {
            if (inputU == null) // Se a caixa de texto do vértice u não existir
            {
                inputU = new TextBox { Location = new Point(10, 50), Size = new Size(50, 20) }; // Cria a caixa de texto do vértice u
                this.Controls.Add(inputU); // Adiciona a caixa de texto do vértice u
            }
            if (inputV == null) // Se a caixa de texto do vértice v não existir
            {
                inputV = new TextBox { Location = new Point(70, 50), Size = new Size(50, 20) }; // Cria a caixa de texto do vértice v
                this.Controls.Add(inputV); // Adiciona a caixa de texto do vértice v
            }
            if (isAdd && inputWeight == null) // Se for para adicionar aresta e a caixa de texto do peso não existir
            {
                inputWeight = new TextBox { Location = new Point(130, 50), Size = new Size(50, 20) }; // Cria a caixa de texto do peso da aresta
                this.Controls.Add(inputWeight); // Adiciona a caixa de texto do peso da aresta
            }

            inputU.Visible = true; // Exibe a caixa de texto do vértice u
            inputV.Visible = true; // Exibe a caixa de texto do vértice v
            if (isAdd) inputWeight.Visible = true; // Se for para adicionar aresta, então exibe a caixa de texto do peso da aresta

            actionButton.Text = isAdd ? "ADD" : "DELETE"; // Define o texto do botão de ação
            actionButton.Click -= ActionButton_Click; // Remove o evento de clique existente
            actionButton.Click += ActionButton_Click; // Adiciona o novo evento de clique
            actionButton.Visible = true; // Exibe o botão de ação
        }
    }

    private void DrawGraph(Graphics g, List<int>[][] graph, int size, Rectangle rect, double zoom, int offsetX, int offsetY)
    {
        int radius = (int)(20 * zoom); // Aplicar zoom no raio dos vértices do grafo (int) converte o resultado  da multiplicação para inteiro
        int centerX = rect.Width / 2 + offsetX; // Calcula o centro do eixo x
        int centerY = rect.Height / 2 + offsetY; // Calcula o centro do eixo y
        if (positions == null || positions.Length != size) // Se as posições dos vértices não foram definidas ou o tamanho for diferente
        {
            positions = new Point[size]; // Inicializa as posições dos vértices
        }
        int graphRadius = (int)((Math.Min(centerX, centerY) - 100) * zoom); // Calcula o raio do grafo

        for (int i = 0; i < size; i++) // Percorre os vértices
        {
            if (positions[i] == Point.Empty) // Se a posição do vértice i não foi definida
            {
                // Define a posição do vértice i na circunferência
                int posX = centerX + (int)(graphRadius * Math.Cos(2 * Math.PI * i / size));
                int posY = centerY + (int)(graphRadius * Math.Sin(2 * Math.PI * i / size));

                // Impede que o vértice apareça fora da tela
                if (posX < rect.Left + radius) posX = rect.Left + radius;
                if (posX > rect.Right - radius) posX = rect.Right - radius;
                if (posY < rect.Top + radius) posY = rect.Top + radius;
                if (posY > rect.Bottom - radius) posY = rect.Bottom - radius;

                positions[i] = new Point(posX, posY);
            }
        }

        for (int i = 0; i < size; i++) // Percorre os vértices
        {
            for (int j = i + 1; j < size; j++) // Percorre os vértices a partir do vértice i + 1
            {
                if (graph[i][j].Count > 0) // Se houver aresta entre os vértices i e j
                {
                    foreach (var weight in graph[i][j]) // Percorre os pesos das arestas
                    {
                        if (graph[i][j].Count > 1) // Se houver arestas duplicadas
                        {
                            using (var pen = new Pen(Color.Green, 2)) // Cria uma caneta verde
                            {
                                var midPoint = new Point((positions[i].X + positions[j].X) / 2, (positions[i].Y + positions[j].Y) / 2); // Calcula o ponto médio
                                var controlPoint = new Point(midPoint.X + 25, midPoint.Y - 25); // Calcula o ponto de controle
                                var graphpath = new GraphicsPath(); // Cria um caminho gráfico usando a dependência System.Drawing.Drawing2D
                                graphpath.AddBezier(positions[i], controlPoint, controlPoint, positions[j]); // Adiciona uma curva de Bézier ao caminho
                                g.DrawPath(pen, graphpath); // Desenha o caminho com a caneta
                                g.DrawString(weight.ToString(), this.Font, Brushes.Green, midPoint); // Desenha o peso da aresta
                            }
                        }
                        g.DrawLine(Pens.Black, positions[i], positions[j]); // Desenha a aresta entre os vértices i e j com a cor preta
                        var midPointText = new Point((positions[i].X + positions[j].X) / 2, (positions[i].Y + positions[j].Y) / 2); // Calcula o ponto médio do texto
                        g.DrawString(weight.ToString(), this.Font, Brushes.Black, midPointText); // Desenha o peso da aresta
                    }
                }
            }
        }

        for (int i = 0; i < size; i++) // Percorre os vértices
        {
            if (graph[i].Any(col => col.Count > 0)) // Se houver arestas incidentes no vértice
            {
                g.FillEllipse(Brushes.White, positions[i].X - radius, positions[i].Y - radius, radius * 2, radius * 2); // Preenche o vértice(elipse) com a cor branca
                g.DrawEllipse(Pens.Black, positions[i].X - radius, positions[i].Y - radius, radius * 2, radius * 2); // Desenha o vértice(elipse) com a cor preta
                g.DrawString(i.ToString(), this.Font, Brushes.Black, positions[i].X - 5, positions[i].Y - 5); // Desenha o índice do vértice
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        // Método chamado toda vez que a janela é redesenhada
        base.OnPaint(e); // Chama o método OnPaint da classe base(Form)
        DrawGraph(e.Graphics, graph, size, drawingArea, zoom, offsetX, offsetY); // Redesenha apenas a área de desenho
    }

    [STAThread] // Indica que o modelo de threading do aplicativo é single-threaded apartment (STA) - um modelo de threading que permite apenas uma thread por processo
    public static void Main()
    {
        Application.EnableVisualStyles(); // Habilita o estilo visual do aplicativo
        Application.SetCompatibleTextRenderingDefault(false); // Define o estilo de texto padrão como não compatível com o estilo visual
        Application.Run(new ChinesePostmanProblem()); // Executa o aplicativo com o formulário do Problema do Carteiro Chinês
    }
}