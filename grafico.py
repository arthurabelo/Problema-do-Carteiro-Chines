import matplotlib.pyplot as plt
import matplotlib.ticker as ticker

# Ler o número de vértices ímpares
num_vertices_impares = int(input("Informe o número de vértices ímpares do grafo: "))

# Inicializar listas para armazenar os dados
vertices = []
tempos = []

# Ler os dados de entrada
while True:
    qtd_vertices = int(input("Informe a quantidade de vértices do grafo (ou -1 para terminar): "))
    if qtd_vertices == -1:
        break
    tempo_execucao = float(input(f"Informe o tempo de execução para {qtd_vertices} vértices (em ms): "))
    vertices.append(qtd_vertices)
    tempos.append(tempo_execucao)

# Gerar o gráfico de linha
plt.plot(vertices, tempos, marker='o')

# Adicionar título e rótulos aos eixos
plt.title(f"Tempo de Execução do Problema do Carteiro Chinês (Vértices Ímpares: {num_vertices_impares})")
plt.xlabel("Quantidade de Vértices")
plt.ylabel("Tempo de Execução (ms)")

# Definir os ticks dos eixos
plt.xticks(vertices)
from matplotlib.ticker import MaxNLocator

plt.gca().yaxis.set_major_locator(MaxNLocator(nbins=10))

# Adicionar anotações aos pontos do gráfico
for i, txt in enumerate(tempos):
    plt.annotate(f'{int(txt)}', (vertices[i], tempos[i]), fontsize=8, ha='right')

# Ajustar a visibilidade dos ticks para evitar sobreposição
plt.gca().xaxis.set_major_locator(ticker.MaxNLocator(integer=True))
plt.gca().xaxis.set_major_formatter(ticker.FuncFormatter(lambda x, _: f'{int(x)}'))
plt.gca().yaxis.set_major_locator(ticker.MaxNLocator(integer=True))
plt.gca().yaxis.set_major_formatter(ticker.FuncFormatter(lambda y, _: f'{int(y)}'))

# Exibir o gráfico
plt.grid(True)
plt.show()