# Problema do Carteiro Chinês

Este projeto resolve o problema do carteiro chinês, que consiste em encontrar o caminho mínimo que cobre todas as arestas de um grafo.

## Estrutura do Projeto

O projeto é organizado da seguinte forma:

- **main.cs**: Ponto de entrada da aplicação. Inicializa o aplicativo e implementa a interface gráfica para resolver o problema do carteiro chinês.
- **grafico.py**: Script Python para gerar gráficos de desempenho com base nos dados de execução do problema do carteiro chinês.

## Funcionalidades

- Adicionar e remover arestas no grafo.
- Importar e exportar grafos a partir de arquivos.
- Executar o algoritmo do carteiro chinês para encontrar o caminho mínimo.
- Testar a performance do algoritmo com diferentes tamanhos de grafos.

## Compilação e Execução

Para compilar e executar a aplicação, siga os passos abaixo:

1. Certifique-se de ter o .NET SDK instalado em sua máquina.
2. Navegue até o diretório do projeto.
3. Execute o comando `dotnet build` para compilar o projeto.
4. Após a compilação bem-sucedida, execute o comando `dotnet run` para iniciar a aplicação.

## Testes de Performance

A aplicação inclui uma interface para testar a performance do algoritmo do carteiro chinês com diferentes tamanhos de grafos. Para utilizar essa funcionalidade:

1. Abra a aplicação.
2. Clique no botão "Testar Performance".
3. Insira a quantidade de vértices e a quantidade de vértices ímpares desejada.
4. Clique em "Executar Teste" para iniciar o teste de performance.
5. Os resultados serão exibidos na interface e um gráfico de desempenho será gerado.

## Importar e Exportar Grafos

Para importar e exportar grafos, siga os passos abaixo:

### Importar Grafos

1. Abra a aplicação.
2. Clique no botão "Importar Grafo".
3. Selecione o arquivo de grafo que deseja importar.
4. O grafo será carregado na aplicação.

#### Estrutura do Arquivo para Importação

O arquivo deve conter uma matriz de adjacência onde cada célula representa as arestas entre os vértices. A matriz pode estar no formato CSV ou TXT. Cada célula deve conter uma lista de pesos das arestas entre os vértices, ou `[0]` se não houver aresta. Exemplo:

Para formato CSV:
```
[0],[1,2],[0]
[1,2],[0],[3]
[0],[3],[0]
```

Para formato TXT:
```
[0] [1,2] [0]
[1,2] [0] [3]
[0] [3] [0]
```

### Exportar Grafos

1. Abra a aplicação.
2. Clique no botão "Exportar Grafo".
3. Escolha o local e o nome do arquivo para salvar o grafo.
4. O grafo será exportado para o arquivo selecionado.

## Gerar Gráficos de Desempenho

Para gerar gráficos de desempenho utilizando recursos do `matplotlib`, siga os passos abaixo:

1. Certifique-se de ter o `Python` e a biblioteca `matplotlib` instalados em sua máquina.
2. Navegue até o diretório do projeto.
3. Execute o script `grafico.py` com o comando `python grafico.py`.
4. Siga as instruções para inserir os dados de entrada e visualizar o gráfico gerado.