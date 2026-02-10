using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Editor_de_Grafos
{
    public class Grafo : GrafoBase, iGrafo
    {
        public bool[] visitado;
        public Color corVertice = Color.Orange;
        public Color corAresta = Color.Blue;

        /*
        Estrutura para armazenar uma aresta com seu vértice de destino e peso
        A ideia aqui é criar uma estrutura que armazene a aresta, o vértice de destino e o peso da aresta
        de forma temporária, para que possa mexer com ela sem modificar a estrutura original do grafo
        evitar trabalho também, já que a aresta não é um objeto
         */
        struct ArestaInfo
        {
            public Aresta aresta;
            public int vertice;
            public int peso;

            // Construtor da estrutura
            public ArestaInfo(Aresta a, int v, int p)
            {
                aresta = a;
                vertice = v;
                peso = p;
            }
        }


        public int AGM(int v)
        {
            int total = 0; // Armazenará o custo total no final
            List<int> vertices = new List<int>(); // Listra para armazenar os vértices visitados
            bool[] visitado = new bool[getN()]; // Matriz para marcar os vértices visitados

            visitado[v] = true; // Marca o vértice que começou clicado como visitado
            vertices.Add(v); // Adiciona o vértice que começou clicado na lista de visitados

            getVertice(v).setCor(corVertice); // Seta a cor do vértice que começou clicado

            int contador = 0; // criando um contador para salvar a quantidade de vértices visitados

            while (contador != getN())
            {
                List<ArestaInfo> candidatos = new List<ArestaInfo>(); // Lista de arestas candidatas para AGM

                // Para cada vértice já visitado, analisa suas arestas
                foreach (int x in vertices)
                {
                    for (int i = 0; i < getN(); i++)
                    {
                        // Se existe aresta entre x e i, e o vértice i ainda não foi visitado
                        if (matAdj[x, i] != null && !visitado[i])
                        {
                            // Adiciona essa aresta como candidata
                            candidatos.Add(new ArestaInfo(matAdj[x, i], i, matAdj[x, i].getPeso()));
                        }
                    }
                }

                // Encontra a aresta com o menor peso entre os candidatos
                ArestaInfo menor = candidatos[0];
                foreach (var item in candidatos)
                {
                    if (item.peso < menor.peso)
                    {
                        menor = item;
                    }
                }

                // Marca o vértice como visitado e armazena os dados
                int novoVertice = menor.vertice;
                vertices.Add(novoVertice);
                visitado[novoVertice] = true;

                // Pinta a aresta e o vértice
                menor.aresta.setCor(corAresta);
                Thread.Sleep(500); // Delay visual (pode remover se quiser)
                getVertice(novoVertice).setCor(corVertice);

                // Soma o custo da aresta ao custo total
                total += menor.peso;

                candidatos.Clear(); // Limpa os candidatos para a próxima iteração

                // Atualiza o contador de vértices visitados
                contador = 0;
                for (int i = 0; i < visitado.Length; i++) // Percorre todos os vértices
                {
                    if (visitado[i])
                    {
                        contador++;
                    }
                }
            }

            return total; // Retorna o custo total da AGM gerada
        }

        // Estrutura que representa as informações do caminho mínimo
        public struct CustoCaminhoMinimo
        {
            public int Vertice;         // Índice do vértice atual
            public int Antecessor;      // Vértice anterior no caminho
            public int Estimativa;      // Custo estimado do caminho até esse vértice
            public bool Fechado;        // Se já foi processado (fechado) ou não

            // Construtor que inicializa todos os campos
            public CustoCaminhoMinimo(int vertice, int antecessor, int estimativa, bool fechado)
            {
                this.Vertice = vertice;
                this.Antecessor = antecessor;
                this.Estimativa = estimativa;
                this.Fechado = fechado;
            }

            // Método de apoio para visualizar o conteúdo
            public string DebugInfo()
            {
                return $"|V: {Vertice} Ant: {Antecessor} Est: {Estimativa} Fechado: {Fechado}|";
            }
        }

        public void CaminhoMinimo(int i, int j)
        {
            List<CustoCaminhoMinimo> CM = new List<CustoCaminhoMinimo>();
            bool[] visitado = new bool[getN()];
            int fechados = 0;

            // Inicializa a lista CM com os vértices presentes no grafo
            for (int x = 0; x < getN(); x++)
            {
                for (int z = 0; z < getN(); z++)
                {
                    if (matAdj[x, z] != null && !visitado[x])
                    {
                        visitado[x] = true;
                        CM.Add(new CustoCaminhoMinimo(x, -1, int.MaxValue, false));
                    }
                }
            }

            // Define o vértice inicial (i) com estimativa 0 e antecessor dele mesmo
            for (int k = 0; k < CM.Count; k++)
            {
                if (CM[k].Vertice == i)
                {
                    var temp = CM[k];
                    temp.Antecessor = i;
                    temp.Estimativa = 0;
                    CM[k] = temp;
                    break;
                }
            }

            // Enquanto nem todos os vértices estiverem fechados...
            while (fechados != CM.Count)
            {
                CustoCaminhoMinimo? menor = null;

                // Seleciona o vértice com menor estimativa que ainda não foi fechado
                for (int k = 0; k < CM.Count; k++)
                {
                    if (!CM[k].Fechado && (menor == null || CM[k].Estimativa < menor.Value.Estimativa))
                    {
                        menor = CM[k];
                    }
                }

                // Marca o vértice escolhido como fechado
                for (int k = 0; k < CM.Count; k++)
                {
                    if (CM[k].Vertice == menor.Value.Vertice)
                    {
                        var temp = CM[k];
                        temp.Fechado = true;
                        CM[k] = temp;
                        break;
                    }
                }

                // Para cada vizinho do vértice com menor estimativa...
                List<Vertice> adjacentes = getAdjacentes(menor.Value.Vertice);
                foreach (var vizinho in adjacentes)
                {
                    for (int k = 0; k < CM.Count; k++)
                    {
                        if (vizinho == getVertice(CM[k].Vertice) && !CM[k].Fechado)
                        {
                            int estimativa = getAresta(menor.Value.Vertice, CM[k].Vertice).getPeso() + menor.Value.Estimativa;

                            if (estimativa < CM[k].Estimativa)
                            {
                                var temp = CM[k];
                                temp.Estimativa = estimativa;
                                temp.Antecessor = menor.Value.Vertice;
                                CM[k] = temp;
                            }
                            break;
                        }
                    }
                }

                // Reconta quantos vértices estão fechados
                fechados = 0;
                foreach (var item in CM)
                {
                    if (item.Fechado) fechados++;
                }
            }

            // Começa a marcar o caminho do destino (j) até a origem (i)
            int ant = 0;
            for (int k = 0; k < CM.Count; k++)
            {
                if (CM[k].Vertice == j)
                {
                    getVertice(j).setCor(corAresta);
                    Thread.Sleep(500);
                    getAresta(j, CM[k].Antecessor).setCor(corVertice);
                    ant = CM[k].Antecessor;
                    break;
                }
            }

            // Caminha para trás, do destino até o início, colorindo o caminho
            while (ant != i)
            {
                for (int k = 0; k < CM.Count; k++)
                {
                    if (CM[k].Vertice == ant)
                    {
                        getAresta(ant, CM[k].Antecessor).setCor(corVertice);
                        getVertice(ant).setCor(corVertice);
                        Thread.Sleep(500);
                        ant = CM[k].Antecessor;
                        break;
                    }
                }
            }

            // Marca o vértice de origem
            getVertice(i).setCor(corAresta);
        }


        public void CompletarGrafo()
        {
            Random r = new Random(); // Gera um número aleatório
            int numPeso = 1; // Inicializa o peso com 1
            if (getPesosAleatorios()) // Se a opção de pesos aleatórios estiver marcada
            {
                for (int i = 0; i < getN(); i++) // Percorre todos os vértices
                {
                    for (int j = 0; j < getN(); j++) // Percorre todas as arestas
                    {
                        numPeso = r.Next(1, 100); // Gera um número aleatório entre 1 e 99
                        if (getAresta(i, j) == null) // Se a aresta não existir
                        {
                            setAresta(i, j, numPeso); // Cria a aresta com o peso aleatório
                        }
                    }
                }
            }
            else // Se a opção de pesos aleatórios não estiver marcada
            {
                for (int i = 0; i < getN(); i++) // Percorre todos os vértices
                {
                    for (int j = 0; j < getN(); j++) // Percorre todas as arestas
                    {
                        if (getAresta(i, j) == null) // Se a aresta não existir
                        {
                            setAresta(i, j, numPeso); // Cria a aresta com o peso 1
                        }
                    }
                }
            }
        }

        public bool IsEuleriano()
        {
            for (int i = 0; i < getN(); i++) // Percorre todos os vértices
            {
                if (grau(i) % 2 != 0) // Se o grau do vértice for ímpar
                {
                    return false; // Retorna falso
                }
            }
            return true; // Se todos os vértices tiverem grau par, retorna verdadeiro
        }

        public bool IsUnicursal()
        {
            int cont = 0; // Inicializa o contador
            for (int i = 0; i < getN(); i++) // Percorre todos os vértices
            {
                if (grau(i) % 2 != 0) // Se o grau do vértice for ímpar
                {
                    cont++; // Adiciona +1 ao contador
                }
            }
            return (cont == 2); // Se o contador for igual a 2, retorna verdadeiro
        }

        public void Largura(int v) 
        {
            LimparFormatacaoGrafo();
            Fila f = new Fila(matAdj.GetLength(0)); // Cria uma nova fila
            visitado = new bool[getN()]; // Cria o vetor de visitados

            f.enfileirar(v); // Enfileira o vértice
            visitado[v] = true; // Marca o vértice como visitado
            while (!f.vazia()) // Enquanto a fila não estiver vazia
            {
                v = f.desenfileirar(); // Desenfileira o vértice
                getVertice(v).setCor(corVertice); // Seta a cor do vértice

                for (int i = 0; i < matAdj.GetLength(0); i++) // Percorre todas as arestas
                {
                    if (matAdj[v, i] != null && !visitado[i]) // Se a aresta existir e o vértice não foi visitado
                    {
                        visitado[i] = true; // Marca o vértice como visitado
                        Thread.Sleep(300); // Pausa para a visualização no editor
                        matAdj[v, i].setCor(corAresta); // Seta a cor da aresta
                        f.enfileirar(i); // Enfileira o vértice
                    }
                }
            }
        }

        // Estrutura que representa uma cor usada no cálculo do número cromático
        public struct NumCromatico
        {
            // Nome da cor
            public string Nome { get; set; }

            // Indica se a cor já foi usada por um vértice adjacente
            public bool Usado { get; set; }

            // Construtor do struct
            public NumCromatico(string nome, bool usado)
            {
                Nome = nome;
                Usado = usado;
            }
        }


        public int NumeroCromatico()
        {
            Fila f = new Fila(matAdj.GetLength(0));
            bool[] visitado = new bool[getN()];
            int Vertice = 0, mNumVertices = 0;

            // Encontra o vértice com o maior número de vizinhos
            for (int i = 0; i < getN(); i++)
            {
                if (getAdjacentes(i).Count > mNumVertices)
                {
                    mNumVertices = getAdjacentes(i).Count;
                    Vertice = i;
                }
            }

            // Lista de cores
            List<Color> cores = new List<Color>
            {
                Color.Aquamarine,
                Color.LightGreen,
                Color.Green,
                Color.Pink,
                Color.Yellow,
                Color.Magenta,
                Color.Cyan,
                Color.DarkViolet,
                Color.Brown
            };

            // Lista com as cores usadas e seu status de uso por vizinhos
            List<NumCromatico> lCores = new List<NumCromatico>
            {
                new NumCromatico(Color.Blue.Name, false) // primeira cor usada
            };

            getVertice(Vertice).setCor(Color.Blue); // aplica cor ao vértice inicial
            f.enfileirar(Vertice); // enfileira o vértice inicial
            visitado[Vertice] = true; // marca como visitado

            while (!f.vazia())
            {
                Vertice = f.desenfileirar(); // pega próximo vértice da fila

                for (int i = 0; i < matAdj.GetLength(0); i++)
                {
                    if (matAdj[Vertice, i] != null && !visitado[i])
                    {
                        visitado[i] = true;
                        f.enfileirar(i);

                        // Reinicia o estado de "usado" das cores na lista
                        for (int k = 0; k < lCores.Count; k++)
                        {
                            var cor = lCores[k];
                            cor.Usado = false;
                            lCores[k] = cor; // reatribui, pois struct é tipo-valor
                        }

                        // Marca as cores já utilizadas pelos vizinhos
                        foreach (var item in getAdjacentes(i))
                        {
                            for (int k = 0; k < lCores.Count; k++)
                            {
                                if (item.getCor().Name == lCores[k].Nome)
                                {
                                    var cor = lCores[k];
                                    cor.Usado = true;
                                    lCores[k] = cor; // reatribui para aplicar a modificação
                                }
                            }
                        }

                        bool existeCor = false;

                        // Aplica a primeira cor disponível que não esteja sendo usada por vizinhos
                        for (int k = 0; k < lCores.Count; k++)
                        {
                            if (!lCores[k].Usado)
                            {
                                getVertice(i).setCor(Color.FromName(lCores[k].Nome));
                                existeCor = true;
                                break;
                            }
                        }

                        // Se todas as cores foram usadas, adiciona uma nova cor aleatória
                        if (!existeCor)
                        {
                            Random r = new Random();
                            Color color = cores[r.Next(cores.Count)]; // pega cor aleatória
                            lCores.Add(new NumCromatico(color.Name, false));
                            getVertice(i).setCor(color);
                            cores.Remove(color); // remove cor usada para não repetir
                        }
                    }
                }
            }

            return lCores.Count; // retorna o número de cores diferentes usadas
        }


        public String ParesOrdenados()
        {
            string retorno = ""; // Inicializa a string de retorno
            for (int i = 0; i < getN(); i++) // Percorre todos os vértices
            {
                for (int j = 0; j < getN(); j++) // Percorre todas as arestas
                {
                    if (getAresta(i, j) != null) // Se a aresta existir
                    {
                        retorno += $" ({getVertice(i).getRotulo()}, {getVertice(j).getRotulo()}) "; // Adiciona o par ordenado à string de retorno
                    }
                }
            }
            return "E={" + retorno + "}"; // Retorna a string de retorno
        }
        public void Profundidade(int v)
        {
            visitado[v] = true; // Marca o vértice como visitado
            for (int i = 0; i < matAdj.GetLength(0); i++) // Percorre todas as arestas
            {
                if (matAdj[v, i] != null && !visitado[i]) // Se a aresta existir e o vértice não foi visitado
                {
                    Profundidade(i); // Continua a busca por arestas a partir do novo vértice
                    Thread.Sleep(500); // Pausa para a visualização no editor
                    getAresta(v, i).setCor(corAresta); // Seta a cor da aresta
                }
            }
        }

        int contador = 0;
        public bool isArvore()
        {
            contador = 0;
            LimparFormatacaoGrafo();
            visitado = new bool[getN()]; // Rastreará quais vértices já foram visitados durante a contagem de arestas

            contador = ArestaContagem(0, visitado); // Inicia a contagem de arestas a partir do vértice 0

            if (getTotalArestas() / 2 == contador) // Se o número de arestas for igual ao esperado para uma árvore
            {
                return true; // Verdadeiro para árvore
            }
            else
            {
                return false; // Falso para grafo não árvore
            }
        }

        public int ArestaContagem(int v, bool[] visitado) // Método para contar as arestas de um grafo
        {
            visitado[v] = true; // Marca o vértice como visitado

            for (int i = 0; i < matAdj.GetLength(0); i++) // Faz iterações sobre todas as possíveis conexões do vértice na matriz
            {
                if (matAdj[v, i] != null && !visitado[i]) // Verifica se existe uma aresta entre o vértice e outro vértice e se o vértice i ainda não foi visitado
                {
                    contador++; //Adiciona +1 ao contador
                    Thread.Sleep(500); // Pausa para a visualização no editor
                    matAdj[v, i].setCor(corAresta); // Seta a cor da aresta
                    ArestaContagem(i, visitado); // Continua a busca por arestas a partir desse novo vértice
                }
            }

            return contador; // Retorna o contador de arestas
        }

        public void LimparFormatacaoGrafo()
        {
            for (int i = 0; i < getN(); i++) // Percorre todos os vértices
            {
                getVertice(i).setCor(Color.Black); // Seta a cor padrão dos vértices
                for (int j = 0; j < getN(); j++) // Percorre todas as arestas
                {
                    if (getAresta(i, j) != null) // Verifica se a aresta existe
                    {
                        getAresta(i, j).setCor(Color.Black); // Seta a cor padrão das arestas
                    }
                }
            }

        }
        public int IndiceCromatico()
        {
            List<Color> coresDisponiveis = new List<Color>
            {
                Color.Red, 
                Color.Green, 
                Color.Blue, 
                Color.Yellow, 
                Color.Orange,
                Color.Pink, 
                Color.Cyan, 
                Color.Brown, 
                Color.Magenta, 
                Color.Gray
            };

            List<string> coresUsadas = new List<string>();

            for (int i = 0; i < getN(); i++)
            {
                for (int j = i + 1; j < getN(); j++)
                {
                    if (matAdj[i, j] != null)
                    {
                        // Lista de cores que não podem ser usadas nessa aresta
                        List<string> coresAdjacentes = new List<string>();

                        for (int k = 0; k < getN(); k++)
                        {
                            if (matAdj[i, k] != null && matAdj[i, k].getCor().Name != "Black")
                            {
                                string cor = matAdj[i, k].getCor().Name;
                                if (!coresAdjacentes.Contains(cor))
                                    coresAdjacentes.Add(cor);
                            }

                            if (matAdj[j, k] != null && matAdj[j, k].getCor().Name != "Black")
                            {
                                string cor = matAdj[j, k].getCor().Name;
                                if (!coresAdjacentes.Contains(cor))
                                    coresAdjacentes.Add(cor);
                            }
                        }

                        // Escolhe a primeira cor disponível que não está em coresAdjacentes
                        Color corEscolhida = Color.Black;
                        foreach (var cor in coresDisponiveis)
                        {
                            if (!coresAdjacentes.Contains(cor.Name))
                            {
                                corEscolhida = cor;
                                break;
                            }
                        }

                        // Se não achar cor disponível, gera uma nova cor aleatória
                        if (corEscolhida == Color.Black)
                        {
                            Random rnd = new Random();
                            corEscolhida = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                            coresDisponiveis.Add(corEscolhida);
                        }

                        // Colore a aresta
                        matAdj[i, j].setCor(corEscolhida);
                        matAdj[j, i].setCor(corEscolhida);

                        if (!coresUsadas.Contains(corEscolhida.Name))
                            coresUsadas.Add(corEscolhida.Name);
                    }
                }
            }

            return coresUsadas.Count;
        }

    }
}
