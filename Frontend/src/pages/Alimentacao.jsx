// Alimentação — anúncios de produtos alimentícios da comunidade acadêmica.
//
// Esta tela consome o endpoint GET /api/alimentos e exibe apenas alimentos
// disponíveis para compra. Trata os estados de carregamento, erro e lista vazia.
//
// A pesquisa é encaminhada para a tela unificada /produtos, com tipo=lanche
// (mesmo comportamento da aba Bazar, via <BuscaAnuncios>).
//
// A compra é realizada na tela de detalhes do alimento.
// Alimentação usa somente o tipo Compra; Reserva não faz parte deste módulo.

import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

import { listarAlimentos } from '../services/alimentos.js';
import { ApiError } from '../services/api.js';
import { usePerfilVendedor } from '../auth/PerfilVendedor.jsx';
import BuscaAnuncios from '../components/BuscaAnuncios.jsx';
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

    const totalExibido = alimentos.length;
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

            <BuscaAnuncios tipo="lanche" />
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

        {!carregando && !erro && alimentos.length > 0 && (
            <>
                <p className="alimentacao__contador" aria-live="polite">
                    {textoContador}
                </p>

                <ul className="alimentacao__grade">
                    {alimentos.map((alimento) => (
                        <li className="alimentacao__card" key={alimento.id}>
                             {alimento.imagemUrl && (
                                <img
                                    className="alimentacao__imagem"
                                    src={alimento.imagemUrl}
                                    alt={alimento.nome}
                                    loading="lazy"
                                />
                            )}
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
