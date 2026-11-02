// Alimentação — anúncios de produtos alimentícios da comunidade acadêmica.
//
// Esta tela consome o endpoint GET /api/alimentos e exibe apenas alimentos
// disponíveis para compra. Trata os estados de carregamento, erro e lista vazia.
//
// A compra é realizada na tela de detalhes do alimento.
// Alimentação usa somente o tipo Compra; Reserva não faz parte deste módulo.

import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';

import { listarAlimentos } from '../services/alimentos.js';
import { ApiError } from '../services/api.js';
import { usePerfilVendedor } from '../auth/PerfilVendedor.jsx';
import './Alimentacao.css';

function formatarPreco(preco) {
  return Number(preco).toLocaleString('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  });
}

export default function Alimentacao() {
    const { ehVendedor } = usePerfilVendedor();
    const [alimentos, setAlimentos] = useState([]);
    const [carregando, setCarregando] = useState(true);
    const [erro, setErro] = useState('');
    const [busca, setBusca] = useState('');

    // Carrega os alimentos disponíveis quando a página é aberta
    useEffect(() => {
        let ativo = true;

        async function carregar() {
        setCarregando(true);
        setErro('');

        try {
            const dados = await listarAlimentos();

            if (ativo) {
            setAlimentos(dados);
            }
        } catch (error) {
            if (!ativo) return;

            setErro(
            error instanceof ApiError
                ? error.message
                : 'Não foi possível carregar os alimentos. Tente novamente.',
            );
        } finally {
            if (ativo) {
            setCarregando(false);
            }
        }
        }

        carregar();
        
        return () => {
        ativo = false;
        };
    }, []);

    const alimentosFiltrados = useMemo(() => {
        const termo = busca.trim().toLowerCase();

        if (!termo) {
            return alimentos;
        }

        return alimentos.filter((alimento) => {
            const textoPesquisavel = [
            alimento.nome,
            alimento.descricao,
            alimento.categoria,
            alimento.restricoes,
            ]
            .filter(Boolean)
            .join(' ')
            .toLowerCase();

            return textoPesquisavel.includes(termo);
        });
    }, [alimentos, busca]);

    const totalExibido = alimentosFiltrados.length;
    const textoContador =
        totalExibido === 1 ? '1 anúncio' : `${totalExibido} anúncios`;
    
    return (
        <section className="alimentacao">
        <header className="alimentacao__cabecalho">
            <div>
            <h1 className="alimentacao__titulo">Alimentação</h1>
            <p className="alimentacao__subtitulo">
                Explore todos os anúncios da área de Alimentação.
            </p>
            </div>

            {ehVendedor && (
            <Link className="alimentacao__adicionar" to="/vendedor/alimentos/novo">
                Adicionar alimento
            </Link>
            )}

            <input
            type="search"
            className="alimentacao__busca"
            placeholder="Buscar alimentos..."
            value={busca}
            onChange={(evento) => setBusca(evento.target.value)}
            aria-label="Buscar alimentos"
            />
        </header>

        {carregando && (
            <p className="alimentacao__status">
            Carregando alimentos…
            </p>
        )}

        {!carregando && erro && (
            <p className="alimentacao__erro" role="alert">
            {erro}
            </p>
        )}

        {!carregando && !erro && alimentos.length === 0 && (
            <p className="alimentacao__status">
            Ainda não há alimentos disponíveis.
            </p>
        )}

        {!carregando && !erro && alimentos.length > 0 &&
            alimentosFiltrados.length === 0 && (
                <p className="alimentacao__status">
                Nenhum alimento encontrado para “{busca}”.
                </p>
            )}

        {!carregando && !erro && alimentos.length > 0 && (
            <>
                <p className="alimentacao__contador" aria-live="polite">
                    {textoContador}
                </p>

                <ul className="alimentacao__grade">
                    {alimentosFiltrados.map((alimento) => (
                        <li className="alimentacao__card" key={alimento.id}>
                            <div className="alimentacao__card-conteudo">
                            <h2 className="alimentacao__nome">
                                <Link
                                    className="alimentacao__card-link"
                                    to={`/alimentacao/${alimento.id}`}
                                >
                                    {alimento.nome}
                                </Link>
                            </h2>

                            <div className="alimentacao__tags">
                                {alimento.categoria && (
                                <span className="alimentacao__categoria">
                                    {alimento.categoria}
                                </span>
                                )}
                            </div>

                            {alimento.descricao && (
                                <p className="alimentacao__descricao">
                                    {alimento.descricao}
                                </p>
                            )}

                            {alimento.restricoes && alimento.restricoes !== 'null' && (
                                <p className="alimentacao__restricoes">
                                    Restrições: {alimento.restricoes}
                                </p>
                            )}
                            </div>

                            <div className="alimentacao__rodape">
                                <span className="alimentacao__preco">
                                    {formatarPreco(alimento.preco)}
                                </span>

                                <span className="alimentacao__status">
                                    {alimento.quantidadeDisponivel}{' '}
                                    {alimento.quantidadeDisponivel === 1
                                    ? 'unidade'
                                    : 'unidades'}
                                </span>
                            </div>
                        </li>
                    ))}
                </ul>
            </>
        )}
        </section>
    );
}
