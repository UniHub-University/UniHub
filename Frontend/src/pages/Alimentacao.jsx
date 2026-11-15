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
import CardAlimento from '../components/CardAlimento.jsx';
import './Alimentacao.css';

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
                        <CardAlimento key={alimento.id} alimento={alimento} />
                    ))}
                </ul>
            </>
        )}
        </section>
    );
}
